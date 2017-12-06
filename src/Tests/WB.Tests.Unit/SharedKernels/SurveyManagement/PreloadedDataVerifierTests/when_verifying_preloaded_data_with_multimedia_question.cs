﻿using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities.Question;
using Moq;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Parser;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Verifier;
using WB.Core.BoundedContexts.Headquarters.Services.Preloading;
using WB.Core.BoundedContexts.Headquarters.ValueObjects;
using WB.Core.BoundedContexts.Headquarters.ValueObjects.PreloadedData;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.PreloadedDataVerifierTests
{
    [Ignore("KP-10197. Disabled verification")]
    internal class when_verifying_preloaded_data_with_multimedia_question : PreloadedDataVerifierTestContext
    {
        private Establish context = () =>
        {
            questionnaireId = Guid.Parse("11111111111111111111111111111111");
            questionId = Guid.Parse("21111111111111111111111111111111");
            questionnaire =
                CreateQuestionnaireDocumentWithOneChapter(new MultimediaQuestion()
                {
                    StataExportCaption = "q1",
                    PublicKey = questionId
                });
            questionnaire.Title = "questionnaire";
            preloadedDataByFile = CreatePreloadedDataByFile(new[] { "Id", "q1" },
                new string[][] { new string[] { "1", "text" } },
                "questionnaire.csv");

            preloadedDataServiceMock = new Mock<IPreloadedDataService>();
            preloadedDataServiceMock.Setup(x => x.FindLevelInPreloadedData(Moq.It.IsAny<string>()))
                .Returns(new HeaderStructureForLevel()
                {
                    HeaderItems =
                        new Dictionary<Guid, IExportedHeaderItem>
                        {
                            { Guid.NewGuid(), new ExportedQuestionHeaderItem() { VariableName = "q1", ColumnNames = new[] { "q1" } } }
                        }
                });

            object outValue;

            preloadedDataServiceMock.Setup(
                x => x.ParseQuestionInLevel(Moq.It.IsAny<string>(), "q1", Moq.It.IsAny<HeaderStructureForLevel>(), out outValue)).Returns(ValueParsingResult.UnsupportedMultimediaQuestion);
            preloadedDataServiceMock.Setup(x => x.GetColumnIndexByHeaderName(preloadedDataByFile, Moq.It.IsAny<string>())).Returns(-1);

            importDataVerifier = CreatePreloadedDataVerifier(questionnaire, preloadedDataServiceMock.Object);
        };

        private Because of =
            () => importDataVerifier.VerifyPanelFiles(questionnaireId, 1, new[] { preloadedDataByFile }, status);

        private It should_result_has_1_error = () =>
            status.VerificationState.Errors.Count().ShouldEqual(1);

        private It should_return_single_PL0023_error = () =>
            status.VerificationState.Errors.First().Code.ShouldEqual("PL0023");

        private It should_return_reference_with_Cell_type = () =>
            status.VerificationState.Errors.First().References.First().Type.ShouldEqual(PreloadedDataVerificationReferenceType.Cell);

        private static ImportDataVerifier importDataVerifier;
        private static QuestionnaireDocument questionnaire;
        private static Guid questionnaireId;
        private static Guid questionId;
        private static PreloadedDataByFile preloadedDataByFile;

        private static Mock<IPreloadedDataService> preloadedDataServiceMock;
    }
}
