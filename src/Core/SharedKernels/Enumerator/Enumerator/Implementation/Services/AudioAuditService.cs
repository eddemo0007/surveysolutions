﻿using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Plugin.Permissions.Abstractions;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Services
{
    public class AudioAuditService : IAudioAuditService
    {
        private readonly IAudioService audioService;
        private string fileNamePrefix = "audio-audit";
        private readonly IAudioAuditFileStorage audioAuditFileStorage;
        private readonly IPermissionsService permissions;
        public AudioAuditService(
            IAudioService audioService, 
            IAudioAuditFileStorage audioAuditFileStorage, 
            IPermissionsService permissions)
        {
            this.audioService = audioService;
            this.audioAuditFileStorage = audioAuditFileStorage;
            this.permissions = permissions;
        }

        private string currentAuditFileName = null;
        private string currentAuditFilePath = null;

        public async Task StartRecordingAsync(Guid interviewId)
        {
            Debug.WriteLine("!!!!!!!!!!! StartRecording");

            await this.permissions.AssureHasPermission(Permission.Microphone);
            await this.permissions.AssureHasPermission(Permission.Storage);
            
            currentAuditFileName = $"{fileNamePrefix}-{interviewId.FormatGuid()}-{DateTime.Now:ddMMyyyy_HHmmss}";
            try
            {
                currentAuditFilePath = audioService.StartRecording(currentAuditFileName);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                throw;
            }
        }

        public Task StopRecordingAsync(Guid interviewId)
        {
            Debug.WriteLine("!!!!!!!!!!! StopRecording");

            audioService.StopRecording(currentAuditFileName);
            var audioStream = audioService.GetRecord(currentAuditFilePath);
            var mimeType = this.audioService.GetMimeType();
            using (var audioMemoryStream = new MemoryStream())
            {
                audioStream.CopyTo(audioMemoryStream);
                this.audioAuditFileStorage.StoreInterviewBinaryData(
                    interviewId, 
                    currentAuditFileName,
                    audioMemoryStream.ToArray(), 
                    mimeType);
            }

            currentAuditFileName = null;
            currentAuditFilePath = null;

            return Task.CompletedTask;
        }
    }
}
