﻿using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Graph.Drives.Item.Items.Item.CreateUploadSession;
using Microsoft.Graph.Models;
using Microsoft.Kiota.Abstractions.Authentication;
using Polly;
using Polly.Retry;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.Services.Processing;
using WB.Services.Infrastructure.Tenant;
using File = System.IO.File;

namespace WB.Services.Export.ExportProcessHandlers.Externals
{
    internal class OneDriveDataClient : IExternalDataClient
    {
        private readonly ILogger<OneDriveDataClient> logger;
        private readonly ITenantContext tenantContext;
        private GraphServiceClient? graphServiceClient;
        private string refreshToken = string.Empty;
        private readonly AsyncRetryPolicy retry;

        private static long MaxAllowedFileSizeByMicrosoftGraphApi = 4 * 1024 * 1024;

        private GraphServiceClient GraphServiceClient
        {
            get => graphServiceClient ?? throw new InvalidOperationException("Client is not initialized;");
            set => graphServiceClient = value;
        }

        public OneDriveDataClient(
            ILogger<OneDriveDataClient> logger,
            ITenantContext tenantContext)
        {
            this.logger = logger;
            this.tenantContext = tenantContext;

            this.retry = Policy.Handle<ServiceException>(e => e.ResponseStatusCode == StatusCodes.Status401Unauthorized)
                .RetryAsync(2, async (exception, span) =>
                {
                    this.logger.LogError(exception, $"Unauthorized exception during request to OneDrive");

                    var newAccessToken = await this.tenantContext.Api
                        .GetExternalStorageAccessTokenByRefreshTokenAsync(ExternalStorageType.OneDrive,
                            this.refreshToken).ConfigureAwait(false);

                    this.CreateClient(newAccessToken);

                });
        }

        public void InitializeDataClient(string accessToken, string refreshToken, TenantInfo tenant)
        {
            this.refreshToken = refreshToken;

            this.CreateClient(accessToken);
        }

        private void CreateClient(string accessToken)
        {
            logger.LogTrace("Creating Microsoft.Graph.Client for OneDrive file upload");

            this.GraphServiceClient = new GraphServiceClient(
                new ApiKeyAuthenticationProvider(accessToken, "bearer", ApiKeyAuthenticationProvider.KeyLocation.Header)            
            );
        }

        private string Join(params string[] path) 
            => string.Join("/", path.Where( p => p != null));

        public Task<string> CreateApplicationFolderAsync(string subFolder)
            => Task.FromResult(Join("Survey Solutions", this.tenantContext.Tenant.Name, subFolder));

        public Task<string> CreateFolderAsync(string folder, string parentFolder)
            => Task.FromResult(Join(parentFolder, folder));

        public async Task UploadFileAsync(string folder, string fileName, Stream fileStream, long contentLength, CancellationToken cancellationToken = default)
        {
            if (contentLength > MaxAllowedFileSizeByMicrosoftGraphApi)
            {
                logger.LogTrace("Uploading {fileName} to {folder}. Large file of size {Length} in chunks",
                    fileName, folder, contentLength);
                const int maxSizeChunk = 320 * 4 * 1024;

                var temp = Path.GetTempFileName();
                var fs = File.Open(temp, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                try
                {
                    await fileStream.CopyToAsync(fs);

                    await this.retry.ExecuteAsync(async () =>
                    {
                        var driveItem = await GraphServiceClient.Me.Drive.GetAsync(cancellationToken: cancellationToken);
                        if (driveItem == null)
                            throw new Exception("Can't receive drive information");

                        var session = await GraphServiceClient.Drives[driveItem.Id].Root.ItemWithPath(Join(folder, fileName)).CreateUploadSession
                            .PostAsync(new CreateUploadSessionPostRequestBody(), cancellationToken: cancellationToken);
                        
                        return new LargeFileUploadTask<DriveItem>(session, fs, maxSizeChunk).UploadAsync(cancellationToken: cancellationToken);
                    });
                }
                finally
                {
                    fs.Close();
                    File.Delete(temp);
                }
            }
            else
            {
                logger.LogTrace("Uploading {fileName} to {folder}. Small file of size {Length}", fileName, folder, contentLength);
                
                await this.retry.ExecuteAsync(async () =>
                {
                    var driveItem = await GraphServiceClient.Me.Drive.GetAsync(cancellationToken: cancellationToken);
                    if (driveItem == null)
                        throw new Exception("Can't receive drive information");
                    
                    return await GraphServiceClient.Drives[driveItem.Id].Root.ItemWithPath(Join(folder, fileName)).Content
                        .PutAsync(fileStream, cancellationToken: cancellationToken);
                });
            }
        }

        public async Task<long?> GetFreeSpaceAsync(CancellationToken cancellationToken)
        {
            var storageInfo = 
                await this.retry.ExecuteAsync(() => 
                    GraphServiceClient.Me.Drive.GetAsync(cancellationToken: cancellationToken));
            if (storageInfo?.Quota?.Total == null) return null;

            return storageInfo.Quota.Total - storageInfo.Quota.Used ?? 0;
        }

        public void Dispose() { }
    }
}
