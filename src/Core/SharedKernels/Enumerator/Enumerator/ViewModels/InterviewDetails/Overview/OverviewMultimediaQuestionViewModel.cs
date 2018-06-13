﻿using System;
using MvvmCross.Commands;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Overview
{
    public class OverviewMultimediaQuestionViewModel : OverviewQuestion
    {
        private readonly IViewModelNavigationService navigationService;
        private readonly Guid interviewId;
        private readonly string fileName;

        public OverviewMultimediaQuestionViewModel(InterviewTreeQuestion treeNode, 
            IImageFileStorage fileStorageas,
            IViewModelNavigationService navigationService) : base(treeNode)
        {
            this.navigationService = navigationService;
            interviewId = Guid.Parse(treeNode.Tree.InterviewId);
            if (treeNode.IsAnswered())
            {
                var multimediaQuestion = treeNode.GetAsInterviewTreeMultimediaQuestion();
                var fileName = multimediaQuestion.GetAnswer().FileName;
                this.fileName = fileName;

                this.Image = fileStorageas.GetInterviewBinaryData(interviewId,
                    fileName);
            }
        }

        public byte[] Image { get; set; }

        public IMvxAsyncCommand ShowPhotoView => new MvxAsyncCommand(async ()=> 
        {
            if (this.Image?.Length > 0)
            {
                await this.navigationService.NavigateToAsync<PhotoViewViewModel, PhotoViewViewModelArgs>(
                    new PhotoViewViewModelArgs
                    {
                        InterviewId = this.interviewId,
                        FileName = fileName
                    });
            }
        });

    }
}