﻿using System;
using MvvmCross.ViewModels;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State
{
    public class AttachmentViewModel : MvxNotifyPropertyChanged
    {
        private readonly IQuestionnaireStorage questionnaireRepository;
        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly IAttachmentContentStorage attachmentContentStorage;
        
        private AttachmentContentMetadata attachmentContentMetadata;

        private const string ImageMimeType = "image";
        private const string VideMimeType = "video";

        public AttachmentViewModel(
            IQuestionnaireStorage questionnaireRepository,
            IStatefulInterviewRepository interviewRepository,
            IAttachmentContentStorage attachmentContentStorage)
        {
            this.questionnaireRepository = questionnaireRepository;
            this.interviewRepository = interviewRepository;
            this.attachmentContentStorage = attachmentContentStorage;
        }

        public void Init(string interviewId, Identity entityIdentity)
        {
            if (interviewId == null) throw new ArgumentNullException(nameof(interviewId));
            if (entityIdentity == null) throw new ArgumentNullException(nameof(entityIdentity));

            var interview = this.interviewRepository.Get(interviewId);
            IQuestionnaire questionnaire = this.questionnaireRepository.GetQuestionnaire(interview.QuestionnaireIdentity, interview.Language);

            var attachment = questionnaire.GetAttachmentForEntity(entityIdentity.Id);

            if (attachment != null)
            {
                this.attachmentContentMetadata = this.attachmentContentStorage.GetMetadata(attachment.ContentId);

                if (IsImage)
                {
                    this.Content = this.attachmentContentStorage.GetContent(attachment.ContentId);
                    this.RaisePropertyChanged(() => Content);
                }

                if (IsVideo)
                {
                    var backingFile = this.attachmentContentStorage.GetFileCacheLocation(attachment.ContentId);
                    this.ContentPath = backingFile;
                    this.RaisePropertyChanged(() => ContentPath);
                }
            }
        }

        public string ContentPath { get; set; }

        public bool IsImage => this.attachmentContentMetadata != null
            && this.attachmentContentMetadata.ContentType.StartsWith(ImageMimeType, StringComparison.OrdinalIgnoreCase);

        public bool IsVideo => this.attachmentContentMetadata != null
            && this.attachmentContentMetadata.ContentType.StartsWith(VideMimeType, StringComparison.OrdinalIgnoreCase);

        public byte[] Content { get; private set; }
    }
}
