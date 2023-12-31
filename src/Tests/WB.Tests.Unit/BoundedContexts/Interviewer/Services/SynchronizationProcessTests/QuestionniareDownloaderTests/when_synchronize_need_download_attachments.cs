﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Interviewer.Implementation.Services;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.Core.SharedKernels.Questionnaire.Api;
using WB.Core.SharedKernels.Questionnaire.Translations;
using WB.Core.SharedKernels.SurveySolutions.ReusableCategories;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.Services.SynchronizationProcessTests.QuestionniareDownloaderTests
{
    [TestOf(typeof(InterviewerOnlineSynchronizationProcess))]
    internal class when_synchronize_and_need_download_missing_attachments
    {
        [OneTimeSetUp]
        public async Task context()
        {
            var newCensusInterviewIdentities = new List<QuestionnaireIdentity>
            {
                new QuestionnaireIdentity(Guid.NewGuid(), 1),
                new QuestionnaireIdentity(Guid.NewGuid(), 3)
            };

            var attachmentContentIds1 = new List<string>() { "1", "2", "3" };
            var attachmentContentIds2 = new List<string>() { "2", "3", "5" };

            synchronizationService = Mock.Of<IInterviewerSynchronizationService>(
                x => 
                   x.GetQuestionnaireAsync(Moq.It.IsAny<QuestionnaireIdentity>(), Moq.It.IsAny<IProgress<TransferProgress>>(), Moq.It.IsAny<CancellationToken>()) == Task.FromResult(new QuestionnaireApiView())
                && x.GetAttachmentContentsAsync(newCensusInterviewIdentities[0], Moq.It.IsAny<IProgress<TransferProgress>>(), Moq.It.IsAny<CancellationToken>()) == Task.FromResult(attachmentContentIds1)
                && x.GetAttachmentContentsAsync(newCensusInterviewIdentities[1], Moq.It.IsAny<IProgress<TransferProgress>>(), Moq.It.IsAny<CancellationToken>()) == Task.FromResult(attachmentContentIds2)
                && x.GetAttachmentContentAsync("1", Moq.It.IsAny<IProgress<TransferProgress>>(), Moq.It.IsAny<CancellationToken>()) == Task.FromResult(Create.Entity.AttachmentContent_Enumerator("1"))
                && x.GetAttachmentContentAsync("5", Moq.It.IsAny<IProgress<TransferProgress>>(), Moq.It.IsAny<CancellationToken>()) == Task.FromResult(Create.Entity.AttachmentContent_Enumerator("5"))
                && x.GetQuestionnaireTranslationAsync(Moq.It.IsAny<QuestionnaireIdentity>(), Moq.It.IsAny<CancellationToken>()) == Task.FromResult(new List<TranslationDto>())
                && x.GetQuestionnaireReusableCategoriesAsync(Moq.It.IsAny<QuestionnaireIdentity>(), Moq.It.IsAny<CancellationToken>()) == Task.FromResult(new List<ReusableCategoriesDto>())
                );

            interviewerQuestionnaireAccessor = Mock.Of<IInterviewerQuestionnaireAccessor>(
                x => x.IsQuestionnaireAssemblyExists(Moq.It.IsAny<QuestionnaireIdentity>())
            );

            attachmentContentStorage = Mock.Of<IAttachmentContentStorage>(
                x => x.ExistsAsync("1") ==Task.FromResult(false)
                && x.ExistsAsync("2") == Task.FromResult(true)
                && x.ExistsAsync("3") == Task.FromResult(true)
                && x.ExistsAsync("5") == Task.FromResult(false)
                && x.StoreAsync(It.IsAny<AttachmentContent>()) == Task.CompletedTask
                );

            downloader = Create.Service.QuestionnaireDownloader(
                attachmentContentStorage: attachmentContentStorage,
                synchronizationService: synchronizationService,
                questionnairesAccessor: interviewerQuestionnaireAccessor
            );

            await downloader.DownloadQuestionnaireAsync(newCensusInterviewIdentities[0], new SynchronizationStatistics(), null, CancellationToken.None);
            await downloader.DownloadQuestionnaireAsync(newCensusInterviewIdentities[1], new SynchronizationStatistics(), null, CancellationToken.None);
        }

        [Test]
        public void should_download_attachment_content_for_id_1() =>
            Mock.Get(synchronizationService).Verify(s => s.GetAttachmentContentAsync("1", Moq.It.IsAny<IProgress<TransferProgress>>(), Moq.It.IsAny<CancellationToken>()), Times.Once());

        [Test]
        public void should_download_attachment_content_for_id_2() =>
            Mock.Get(synchronizationService).Verify(s => s.GetAttachmentContentAsync("2", Moq.It.IsAny<IProgress<TransferProgress>>(), Moq.It.IsAny<CancellationToken>()), Times.Never());

        [Test]
        public void should_download_attachment_content_for_id_3() =>
            Mock.Get(synchronizationService).Verify(s => s.GetAttachmentContentAsync("3", Moq.It.IsAny<IProgress<TransferProgress>>(), Moq.It.IsAny<CancellationToken>()), Times.Never());

        [Test]
        public void should_download_attachment_content_for_id_5() =>
            Mock.Get(synchronizationService).Verify(s => s.GetAttachmentContentAsync("5", Moq.It.IsAny<IProgress<TransferProgress>>(), Moq.It.IsAny<CancellationToken>()), Times.Once());

        [Test]
        public void should_store_attachment_content_for_id_1() =>
            Mock.Get(attachmentContentStorage).Verify(s => s.StoreAsync(Moq.It.Is<AttachmentContent>(ac => ac.Id == "1")), Times.Once());

        [Test]
        public void should_store_attachment_content_for_id_2() =>
            Mock.Get(attachmentContentStorage).Verify(s => s.StoreAsync(Moq.It.Is<AttachmentContent>(ac => ac.Id == "2")), Times.Never());

        [Test]
        public void should_store_attachment_content_for_id_3() =>
            Mock.Get(attachmentContentStorage).Verify(s => s.StoreAsync(Moq.It.Is<AttachmentContent>(ac => ac.Id == "3")), Times.Never());

        [Test]
        public void should_store_attachment_content_for_id_5() =>
            Mock.Get(attachmentContentStorage).Verify(s => s.StoreAsync(Moq.It.Is<AttachmentContent>(ac => ac.Id == "5")), Times.Once());


        static IQuestionnaireDownloader downloader;
        static IAttachmentContentStorage attachmentContentStorage;
        static ISynchronizationService synchronizationService;
        static IInterviewerQuestionnaireAccessor interviewerQuestionnaireAccessor;
    }
}
