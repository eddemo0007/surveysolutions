﻿using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Views.BinaryData;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;


namespace WB.Core.BoundedContexts.Interviewer.Implementation.Services
{
    public abstract class InterviewerFileStorage<TMetadataView, TFileView> : IInterviewFileStorage
        where TMetadataView : class, IFileMetadataView, IPlainStorageEntity, new()
        where TFileView : class, IFileView, IPlainStorageEntity, new()
    {
        private readonly IPlainStorage<TMetadataView> fileMetadataViewStorage;
        private readonly IPlainStorage<TFileView> fileViewStorage;

        protected InterviewerFileStorage(
            IPlainStorage<TMetadataView> fileMetadataViewStorage,
            IPlainStorage<TFileView> fileViewStorage)
        {
            this.fileMetadataViewStorage = fileMetadataViewStorage;
            this.fileViewStorage = fileViewStorage;
        }

        public byte[] GetInterviewBinaryData(Guid interviewId, string fileName)
        {
            var metadataView =
                this.fileMetadataViewStorage.Where(image => image.InterviewId == interviewId && image.FileName == fileName)
                    .SingleOrDefault();

            if (metadataView == null) return null;

            var fileView = this.fileViewStorage.GetById(metadataView.FileId);

            return fileView?.File;
        }

        public List<InterviewBinaryDataDescriptor> GetBinaryFilesForInterview(Guid interviewId)
        {
            var metadataViews = this.fileMetadataViewStorage.Where(image => image.InterviewId == interviewId).ToList();
            return metadataViews.Select(f =>
                new InterviewBinaryDataDescriptor(
                    f.InterviewId,
                    f.FileName,
                    () => this.fileViewStorage.GetById(f.FileId).File
                )
            ).ToList();
        }

        public void StoreInterviewBinaryData(Guid interviewId, string fileName, byte[] data)
        {
            var metadataView =
                this.fileMetadataViewStorage.Where(image => image.InterviewId == interviewId && image.FileName == fileName)
                    .SingleOrDefault();

            if (metadataView == null)
            {
                string fileId = Guid.NewGuid().FormatGuid();
                this.fileViewStorage.Store(new TFileView
                {
                    Id = fileId,
                    File = data
                });

                this.fileMetadataViewStorage.Store(new TMetadataView
                {
                    Id = Guid.NewGuid().FormatGuid(),
                    InterviewId = interviewId,
                    FileId = fileId,
                    FileName = fileName
                });
            }
            else
            {
                this.fileViewStorage.Store(new TFileView
                {
                    Id = metadataView.FileId,
                    File = data
                });
            }
        }

        public void RemoveInterviewBinaryData(Guid interviewId, string fileName)
        {
            var metadataView = this.fileMetadataViewStorage.Where(image => image.InterviewId == interviewId && image.FileName == fileName).SingleOrDefault();

            if (metadataView == null) return;

            this.fileViewStorage.Remove(metadataView.FileId);
            this.fileMetadataViewStorage.Remove(metadataView.Id);
        }
    }
}