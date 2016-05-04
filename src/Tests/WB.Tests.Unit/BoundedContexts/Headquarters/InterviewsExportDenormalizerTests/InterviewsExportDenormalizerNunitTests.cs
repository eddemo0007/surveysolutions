﻿using System;
using System.Linq;
using Main.Core.Documents;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.DataExport.Denormalizers;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.Implementation;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Factories;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Tests.Unit.SharedKernels.SurveyManagement;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.InterviewsExportDenormalizerTests
{
    [TestFixture]
    internal class InterviewsExportDenormalizerNunitTests
    {
        [Test]
        public void
            When_InterviewStatusChanged_On_Completed_event_received_after_Reject_and_the_list_of_levels_has_changed_Then_all_previous_levels_must_be_replaced
            ()
        {
            Guid interviewId=Guid.NewGuid();
            var interviewDataExportView = Create.InterviewDataExportView(interviewId,
                levels:
                    new[]
                    {
                        Create.InterviewDataExportLevelView(interviewId,
                            records:
                                new []
                                {
                                    Create.InterviewDataExportRecord(interviewId),
                                    Create.InterviewDataExportRecord(interviewId),
                                    Create.InterviewDataExportRecord(interviewId)
                                })
                    });
            var exportViewFactoryMock=new Mock<IExportViewFactory>();

            exportViewFactoryMock.Setup(
                x =>
                    x.CreateInterviewDataExportView(Moq.It.IsAny<QuestionnaireExportStructure>(),
                        Moq.It.IsAny<InterviewData>())).Returns(interviewDataExportView);
            var exportRecords = new TestInMemoryWriter<InterviewDataExportRecord>();

            var interviewsExportDenormalizer = CreateInterviewsExportDenormalizer(interviewId, exportViewFactoryMock.Object, exportRecords);

            interviewsExportDenormalizer.Handle(Create.InterviewStatusChangedEvent(InterviewStatus.Completed,
                interviewId: interviewId));

            var countInterviewRecords = exportRecords.Query(_ => _.Count(i => i.InterviewId == interviewId));

            Assert.That(countInterviewRecords, Is.EqualTo(3));

            var newInterviewDataExportView = Create.InterviewDataExportView(interviewId,
            levels:
                new[]
                {
                        Create.InterviewDataExportLevelView(interviewId,
                            records:
                                new []
                                {
                                    Create.InterviewDataExportRecord(interviewId),
                                    Create.InterviewDataExportRecord(interviewId)
                                })
                });
            exportViewFactoryMock.Setup(
                x =>
                    x.CreateInterviewDataExportView(Moq.It.IsAny<QuestionnaireExportStructure>(),
                        Moq.It.IsAny<InterviewData>())).Returns(newInterviewDataExportView);

            interviewsExportDenormalizer.Handle(Create.InterviewStatusChangedEvent(InterviewStatus.Completed,
               interviewId: interviewId));

            countInterviewRecords = exportRecords.Query(_ => _.Count(i => i.InterviewId == interviewId));

            Assert.That(countInterviewRecords, Is.EqualTo(2));
        }
        private InterviewsExportDenormalizer CreateInterviewsExportDenormalizer(
            Guid interviewId, IExportViewFactory exportViewFactory, IReadSideRepositoryWriter<InterviewDataExportRecord> exportRecords)
        {
            var interviewData = Create.InterviewData();
            var interviewReferenceStorage = new TestInMemoryWriter<InterviewReferences>();
            interviewReferenceStorage.Store(new InterviewReferences(interviewId, Guid.NewGuid(), 1),
                interviewId);
            var interviewDataStorage = new TestInMemoryWriter<InterviewData>();
            interviewDataStorage.Store(interviewData, interviewId);

            return new InterviewsExportDenormalizer(interviewDataStorage, interviewReferenceStorage,
                exportViewFactory, exportRecords,new InMemoryKeyValueStorage<QuestionnaireExportStructure>());
        }
    }
}