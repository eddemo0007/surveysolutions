﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Moq;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using It = Machine.Specifications.It;

namespace WB.Core.SharedKernels.SurveyManagement.Tests.EventHandlers.Interview.InterviewExportedDataEventHandlerTests
{
    internal class when_RejectedByHeadquarter_recived : InterviewExportedDataEventHandlerTestContext
    {
        Establish context = () =>
        {
            dataExportService = new Mock<IDataExportRepositoryWriter>();
            interviewExportedDataDenormalizer =
                CreateInterviewExportedDataEventHandlerForQuestionnarieCreatedByMethod(dataExportRepositoryWriter: dataExportService.Object);
        };

        Because of = () =>
            interviewExportedDataDenormalizer.Handle(CreatePublishableEvent(() => new InterviewRejectedByHQ(Guid.NewGuid(), "comment"),
                interviewId));

        It should_RejectedByHeadquarter_action_be_added_to_dataExport = () =>
         dataExportService.Verify(x => x.AddInterviewAction(
                        InterviewExportedAction.RejectedByHeadquarter,
                        Moq.It.IsAny<Guid>(), Moq.It.IsAny<Guid>(), Moq.It.IsAny<DateTime>()), Times.Once);

        private static InterviewExportedDataDenormalizer interviewExportedDataDenormalizer;

        private static Mock<IDataExportRepositoryWriter> dataExportService;
        private static Guid interviewId = Guid.NewGuid();
    }
}
