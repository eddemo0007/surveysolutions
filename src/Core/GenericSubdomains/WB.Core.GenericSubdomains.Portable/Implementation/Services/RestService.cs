﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using Flurl.Http.Content;
using WB.Core.GenericSubdomains.Portable.Implementation.Compression;
using WB.Core.GenericSubdomains.Portable.Properties;
using WB.Core.GenericSubdomains.Portable.Services;

namespace WB.Core.GenericSubdomains.Portable.Implementation.Services
{
    public class RestService : IRestService
    {
        private readonly IRestServiceSettings restServiceSettings;
        private readonly INetworkService networkService;
        private readonly IJsonAllTypesSerializer synchronizationSerializer;
        private readonly IStringCompressor stringCompressor;
        private readonly IHttpStatistician httpStatistician;

        public RestService(
            IRestServiceSettings restServiceSettings,
            INetworkService networkService,
            IJsonAllTypesSerializer synchronizationSerializer,
            IStringCompressor stringCompressor,
            IRestServicePointManager restServicePointManager,
            IHttpStatistician httpStatistician)
        {
            this.restServiceSettings = restServiceSettings;
            this.networkService = networkService;
            this.synchronizationSerializer = synchronizationSerializer;
            this.stringCompressor = stringCompressor;
            this.httpStatistician = httpStatistician;

            if (this.restServiceSettings.AcceptUnsignedSslCertificate)
                restServicePointManager?.AcceptUnsignedSslCertificate();
        }

        private Task<HttpResponseMessage> ExecuteRequestAsync(
            string url,
            HttpMethod method,
            object queryString = null,
            object request = null,
            RestCredentials credentials = null,
            bool forceNoCache = false,
            Dictionary<string, string> customHeaders = null,
            CancellationToken? userCancellationToken = null)
        {
            var compressedJsonContent = this.CreateCompressedJsonContent(request);
            return this.ExecuteRequestAsync(url, method, queryString, compressedJsonContent, credentials, forceNoCache,
                customHeaders, userCancellationToken);
        }

        private async Task<HttpResponseMessage> ExecuteRequestAsync(
            string url,
            HttpMethod method,
            object queryString = null,
            HttpContent httpContent = null,
            RestCredentials credentials = null,
            bool forceNoCache = false,
            Dictionary<string, string> customHeaders = null,
            CancellationToken? userCancellationToken = null)
        {
            if (!this.IsValidHostAddress(this.restServiceSettings.Endpoint))
                throw new RestException("Invalid URL", type: RestExceptionType.InvalidUrl);

            if (this.networkService != null)
            {
                if (!this.networkService.IsNetworkEnabled())
                    throw new RestException("No network", type: RestExceptionType.NoNetwork);

                if (!this.networkService.IsHostReachable(this.restServiceSettings.Endpoint))
                    throw new RestException("Host unreachable", type: RestExceptionType.HostUnreachable);
            }

            var requestTimeoutToken = new CancellationTokenSource(this.restServiceSettings.Timeout).Token;
            var linkedCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(requestTimeoutToken,
                userCancellationToken ?? default(CancellationToken));

            var fullUrl = this.restServiceSettings.Endpoint
                .AppendPathSegment(url)
                .SetQueryParams(queryString);

            IFlurlClient restClient = fullUrl
                .WithTimeout(this.restServiceSettings.Timeout)
                .AllowHttpStatus(HttpStatusCode.NotModified, HttpStatusCode.NoContent)
                .CollectHttpStats(this.httpStatistician)
                .WithHeader("User-Agent", this.restServiceSettings.UserAgent)
                .WithHeader("Accept-Encoding", "gzip,deflate");
            
            if (forceNoCache)
            {
                restClient.WithHeader("Cache-Control", "no-cache");
            }

            if (credentials?.Token != null)
            {
                string base64String = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{credentials.Login}:{credentials.Token}"));
                restClient.WithHeader("Authorization", new AuthenticationHeaderValue(ApiAuthenticationScheme.AuthToken.ToString(), base64String));
            }

            if (credentials?.Password != null && credentials?.Token == null)
            {
                restClient.WithBasicAuth(credentials.Login, credentials.Password);
            }

            if (customHeaders != null)
            {
                foreach (var customHeader in customHeaders)
                {
                    restClient.WithHeader(customHeader.Key, customHeader.Value);
                }
            }

            try
            {
                return await restClient.SendAsync(method, httpContent, linkedCancellationTokenSource.Token).ConfigureAwait(false);
            }
            catch (OperationCanceledException ex)
            {
                // throwed when receiving bytes in ReceiveBytesWithProgressAsync method and user canceling request
                throw new RestException("Request canceled by user", type: RestExceptionType.RequestCanceledByUser, innerException: ex);
            }
            catch (FlurlHttpException ex)
            {
                if (ex.GetSelfOrInnerAs<TaskCanceledException>() != null)
                {
                    if (requestTimeoutToken.IsCancellationRequested)
                    {
                        throw new RestException("Request timeout", type: RestExceptionType.RequestByTimeout,
                            statusCode: HttpStatusCode.RequestTimeout, innerException: ex);
                    }

                    if (userCancellationToken.HasValue && userCancellationToken.Value.IsCancellationRequested)
                    {
                        throw new RestException("Request canceled by user",
                               type: RestExceptionType.RequestCanceledByUser, innerException: ex);
                    }
                }
                else if (ex.Call.Response != null)
                {
                    throw new RestException(ex.Call.Response.ReasonPhrase, statusCode: ex.Call.Response.StatusCode,
                           innerException: ex);
                }
                else
                {
                    // https://github.com/tmenier/Flurl/issues/163
                    var exceptionInnerException = ex.Call?.Exception?.InnerException;
                    if (exceptionInnerException != null)
                    {
                        var exceptionTypeName = exceptionInnerException.GetType().Name;
                        var releaseException = exceptionTypeName.Contains("IOException") && exceptionInnerException.Message.Contains("Unacceptable certificate");
                        if (exceptionTypeName.Contains("SSLHandshakeException") || releaseException)
                        {
                            throw new RestException(exceptionInnerException.Message, type: RestExceptionType.UnacceptableCertificate, innerException: exceptionInnerException);
                        }
                    }
                }

                throw new RestException(message: "Unexpected web exception", innerException: ex);
            }
        }

        public Task GetAsync(string url, object queryString, RestCredentials credentials, bool forceNoCache, 
            Dictionary<string, string> customHeaders, CancellationToken? token)
        {
            return this.ExecuteRequestAsync(url: url,
                queryString: queryString,
                credentials: credentials,
                method: HttpMethod.Get,
                customHeaders: customHeaders,
                forceNoCache: forceNoCache,
                userCancellationToken: token,
                request: null);
        }

        public Task PostAsync(string url, object request = null, RestCredentials credentials = null, CancellationToken? token = null)
        {
            return this.ExecuteRequestAsync(url: url, 
                credentials: credentials,
                method: HttpMethod.Post,
                request: request,
                userCancellationToken: token);
        }

        public Task<T> GetAsync<T>(string url,
            Action<DownloadProgressChangedEventArgs> onDownloadProgressChanged, object queryString = null,
            RestCredentials credentials = null, CancellationToken? token = null)
        {
            var response = this.ExecuteRequestAsync(url: url, queryString: queryString, credentials: credentials, method: HttpMethod.Get,
                userCancellationToken: token, request: null);

            return this.ReceiveCompressedJsonWithProgressAsync<T>(response: response, token: token ?? default(CancellationToken),
                onDownloadProgressChanged: onDownloadProgressChanged);
        }

        public Task<T> PostAsync<T>(string url,
            Action<DownloadProgressChangedEventArgs> onDownloadProgressChanged, object request = null,
            RestCredentials credentials = null, CancellationToken? token = null)
        {
            var response = this.ExecuteRequestAsync(url: url, credentials: credentials, method: HttpMethod.Post, request: request,
                userCancellationToken: token);

            return this.ReceiveCompressedJsonWithProgressAsync<T>(response: response, token: token ?? default(CancellationToken),
                onDownloadProgressChanged: onDownloadProgressChanged);
        }

        public async Task<RestFile> DownloadFileAsync(string url,
            Action<DownloadProgressChangedEventArgs> onDownloadProgressChanged, 
            RestCredentials credentials = null,
            CancellationToken? token = null,
            Dictionary<string, string> customHeaders = null)
        {
            var response = this.ExecuteRequestAsync(url: url, credentials: credentials, method: HttpMethod.Get,
                userCancellationToken: token, request: null, customHeaders: customHeaders);

            var restResponse = await this.ReceiveBytesWithProgressAsync(response: response, token: token ?? default(CancellationToken),
                        onDownloadProgressChanged: onDownloadProgressChanged).ConfigureAwait(false);

            var fileContent = this.GetDecompressedContentFromHttpResponseMessage(restResponse);

            return new RestFile(content: fileContent, contentType: restResponse.RawContentType,
                contentHash: restResponse.ETag, contentLength: restResponse.Length, fileName: restResponse.FileName, 
                statusCode: restResponse.StatusCode);
        }

        public async Task SendStreamAsync(Stream streamData, string url, RestCredentials credentials,
            Dictionary<string, string> customHeaders = null, CancellationToken? token = null)
        {
            using (var multipartFormDataContent = new MultipartFormDataContent())
            {
                using (HttpContent streamContent = new StreamContent(streamData))
                {
                    streamContent.Headers.Add("Content-Type", "application/octet-stream");
                    streamContent.Headers.Add("Content-Length", streamData.Length.ToString());
                    streamContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                    {
                        FileName = "backup.zip"
                    };
                    multipartFormDataContent.Add(streamContent);

                    await this.ExecuteRequestAsync(url: url, queryString: null, credentials: credentials,
                        method: HttpMethod.Post, forceNoCache: true, userCancellationToken: token,
                        customHeaders: customHeaders, httpContent: multipartFormDataContent).ConfigureAwait(false);
                }
            }
        }

        private HttpContent CreateCompressedJsonContent(object data)
        {
            return data == null
                ? null
                : new CompressedContent(
                    new CapturedStringContent(this.synchronizationSerializer.Serialize(data), Encoding.UTF8, "application/json"), new GZipCompressor());
        }

        private async Task<T> ReceiveCompressedJsonWithProgressAsync<T>(Task<HttpResponseMessage> response,
            CancellationToken token, Action<DownloadProgressChangedEventArgs> onDownloadProgressChanged = null)
        {
            var restResponse = await this.ReceiveBytesWithProgressAsync(token, onDownloadProgressChanged, response).ConfigureAwait(false);

            return this.GetDecompressedJsonFromHttpResponseMessage<T>(restResponse);
        }

        private async Task<RestResponse> ReceiveBytesWithProgressAsync(CancellationToken token, Action<DownloadProgressChangedEventArgs> onDownloadProgressChanged,
            Task<HttpResponseMessage> response)
        {
            var responseMessage = await response.ConfigureAwait(false);
            byte[] responseContent;
            var contentLength = responseMessage.Content.Headers.ContentLength;

            using (var responseStream = await responseMessage.Content.ReadAsStreamAsync().ConfigureAwait(false))
            {
                if (token.IsCancellationRequested)
                {
                    token.ThrowIfCancellationRequested();
                }

                var buffer = new byte[this.restServiceSettings.BufferSize];
                var downloadProgressChangedEventArgs = new DownloadProgressChangedEventArgs()
                {
                    TotalBytesToReceive = contentLength
                };
                using (var ms = new MemoryStream())
                {
                    int read;
                    while ((read = await responseStream.ReadAsync(buffer, 0, buffer.Length, token).ConfigureAwait(false)) > 0)
                    {
                        if (token.IsCancellationRequested)
                        {
                            token.ThrowIfCancellationRequested();
                        }

                        ms.Write(buffer, 0, read);

                        if (onDownloadProgressChanged == null) continue;

                        downloadProgressChangedEventArgs.BytesReceived = ms.Length;
                        downloadProgressChangedEventArgs.ProgressPercentage = Math.Round((decimal)(100 * ms.Length) / contentLength.Value);
                        onDownloadProgressChanged(downloadProgressChangedEventArgs);
                    }
                    responseContent = ms.ToArray();
                }
            }

            return new RestResponse
            {
                Response = responseContent,
                ContentType = this.GetContentType(responseMessage.Content.Headers.ContentType?.MediaType),
                ContentCompressionType = this.GetContentCompressionType(responseMessage.Content.Headers),
                RawContentType = responseMessage.Content?.Headers?.ContentType?.MediaType,
                Length = responseMessage.Content?.Headers?.ContentLength,
                ETag = responseMessage.Headers?.ETag?.Tag,
                FileName = responseMessage.Content?.Headers?.ContentDisposition?.FileName,
                StatusCode = responseMessage.StatusCode
            };
        }

        private RestContentType GetContentType(string mediaType)
        {
            if (mediaType.IsNullOrEmpty())
                return RestContentType.Unknown;

            if (mediaType.IndexOf("json", StringComparison.OrdinalIgnoreCase) > -1 || mediaType.IndexOf("javascript", StringComparison.OrdinalIgnoreCase) > -1)
                return RestContentType.Json;

            return RestContentType.Unknown;
        }

        private RestContentCompressionType GetContentCompressionType(HttpContentHeaders headers)
        {
            IEnumerable<string> acceptedEncodings;
            headers.TryGetValues("Content-Encoding", out acceptedEncodings);

            if (acceptedEncodings == null) return RestContentCompressionType.None;

            if (acceptedEncodings.Contains("gzip"))
                return RestContentCompressionType.GZip;

            if (acceptedEncodings.Contains("deflate"))
                return RestContentCompressionType.Deflate;

            return RestContentCompressionType.None;
        }

        private T GetDecompressedJsonFromHttpResponseMessage<T>(RestResponse restResponse)
        {
            if (restResponse.ContentType != RestContentType.Json)
                throw new RestException(message: Resources.CheckServerSettings, statusCode: HttpStatusCode.Redirect);

            try
            {
                var responseContent = GetDecompressedContentFromHttpResponseMessage(restResponse);
                return this.synchronizationSerializer.Deserialize<T>(responseContent);
            }
            catch (JsonDeserializationException ex)
            {
                throw new RestException(message: Resources.UpdateRequired, statusCode: HttpStatusCode.UpgradeRequired, innerException: ex);
            }
        }


        private byte[] GetDecompressedContentFromHttpResponseMessage(RestResponse restResponse)
        {
            var responseContent = restResponse.Response;

            switch (restResponse.ContentCompressionType)
            {
                case RestContentCompressionType.GZip:
                    responseContent = this.stringCompressor.DecompressGZip(responseContent);
                    break;
                case RestContentCompressionType.Deflate:
                    responseContent = this.stringCompressor.DecompressDeflate(responseContent);
                    break;
            }

            return responseContent;
        }

        private bool IsValidHostAddress(string url)
        {
            Uri uriResult;
            return Uri.TryCreate(url, UriKind.Absolute, out uriResult) && (uriResult.Scheme == "http" || uriResult.Scheme == "https");
        }

        internal class RestResponse
        {
            public byte[] Response { get; set; }
            public RestContentType ContentType { get; set; }
            public RestContentCompressionType ContentCompressionType { get; set; }
            public string RawContentType { get; set; }
            public long? Length { get; set; }
            public string ETag { get; set; }
            public string FileName { get; set; }
            public HttpStatusCode StatusCode { get; set; }
        }

        internal enum RestContentType { Unknown, Json }
        internal enum RestContentCompressionType { None, GZip, Deflate }
    }
}
