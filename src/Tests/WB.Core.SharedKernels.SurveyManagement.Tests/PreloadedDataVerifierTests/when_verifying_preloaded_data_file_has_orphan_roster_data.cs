﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Moq;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services.Preloading;
using WB.Core.SharedKernels.SurveyManagement.Services.Preloading;
using WB.Core.SharedKernels.SurveyManagement.ValueObjects.PreloadedData;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Views.PreloadedData;
using It = Machine.Specifications.It;

namespace WB.Core.SharedKernels.SurveyManagement.Tests.PreloadedDataVerifierTests
{
    internal class when_verifying_preloaded_data_file_has_orphan_roster_data : PreloadedDataVerifierTestContext
    {
        Establish context = () =>
        {
            questionnaireId = Guid.Parse("11111111111111111111111111111111");
            questionnaire =
                CreateQuestionnaireDocumentWithOneChapter(new Group(rosterTitle)
                {
                    IsRoster = true,
                    PublicKey = Guid.NewGuid(),
                    RosterSizeSource = RosterSizeSourceType.FixedTitles,
                    RosterFixedTitles = new[] { "a" }
                });
            questionnaire.Title = questionnaireTitle;
            preloadedDataByFileTopLevel = CreatePreloadedDataByFile(new[] { "Id", "ParentId" }, new string[0][],
                questionnaireTitle + ".csv");
            preloadedDataByFileRosterLevel = CreatePreloadedDataByFile(new[] { "Id", "ParentId" }, new string[][] { new string[] { "0", "1" } },
                rosterTitle + ".csv");
            files = new[] { preloadedDataByFileTopLevel, preloadedDataByFileRosterLevel };
            preloadedDataServiceMock = new Mock<IPreloadedDataService>();

            preloadedDataServiceMock.Setup(x => x.GetIdColumnIndex(preloadedDataByFileRosterLevel)).Returns(0);
            preloadedDataServiceMock.Setup(x => x.GetParentIdColumnIndex(preloadedDataByFileRosterLevel)).Returns(1);
            preloadedDataServiceMock.Setup(x => x.FindLevelInPreloadedData(Moq.It.IsAny<string>())).Returns(new HeaderStructureForLevel());
            preloadedDataServiceMock.Setup(x => x.GetParentDataFile(preloadedDataByFileRosterLevel.FileName, files))
                .Returns(preloadedDataByFileTopLevel);

            preloadedDataServiceMock.Setup(x => x.GetAvalibleIdListForParent(preloadedDataByFileTopLevel, Moq.It.IsAny<ValueVector<Guid>>(), "1"))
                .Returns((decimal[])null);

            preloadedDataVerifier = CreatePreloadedDataVerifier(questionnaire, null, preloadedDataServiceMock.Object);
        };

        Because of =
            () =>
                result =
                    preloadedDataVerifier.Verify(questionnaireId, 1, files);

        It should_result_has_1_error = () =>
            result.Count().ShouldEqual(1);

        It should_return_single_PL0008_error = () =>
            result.First().Code.ShouldEqual("PL0008");

        It should_return_reference_with_Cell_type = () =>
            result.First().References.First().Type.ShouldEqual(PreloadedDataVerificationReferenceType.Cell);

        It should_error_PositionX_be_equal_to_1 = () =>
          result.First().References.First().PositionX.ShouldEqual(1);

        It should_error_PositionY_be_equal_to_0 = () =>
          result.First().References.First().PositionY.ShouldEqual(0);

        It should_error_has_content_id_of_orphan_record = () =>
            result.First().References.First().Content.ShouldEqual("1");

        private static PreloadedDataVerifier preloadedDataVerifier;
        private static IEnumerable<PreloadedDataVerificationError> result;
        private static QuestionnaireDocument questionnaire;
        private static Guid questionnaireId;
        private static PreloadedDataByFile preloadedDataByFileTopLevel;
        private static PreloadedDataByFile preloadedDataByFileRosterLevel;
        private static string questionnaireTitle = "questionnaire";
        private static string rosterTitle = "roster";

        private static Mock<IPreloadedDataService> preloadedDataServiceMock;
        private static PreloadedDataByFile[] files;
    }
}
