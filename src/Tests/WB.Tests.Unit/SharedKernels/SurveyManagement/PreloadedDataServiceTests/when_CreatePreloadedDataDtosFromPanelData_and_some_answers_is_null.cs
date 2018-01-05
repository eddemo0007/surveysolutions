﻿using Main.Core.Entities.Composite;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Parser;
using WB.Core.GenericSubdomains.Portable.Implementation.ServiceVariables;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.PreloadedDataServiceTests
{
    [TestOf(typeof(PreloadedDataServiceTestContext))]
    internal class when_CreatePreloadedDataDtosFromPanelData_and_some_answers_is_null : PreloadedDataServiceTestContext
    {
        [Test]
        public void should_not_throw_null_reference_exception()
        {
            var questionnaireDocument =
                CreateQuestionnaireDocumentWithOneChapter(
                    Create.Entity.NumericIntegerQuestion(variable: "nq1", id: Id.gA),
                    Create.Entity.TextQuestion(questionId: Id.gB, variable: "tq1"),
                       Create.Entity.FixedRoster(rosterId: Id.g1,
                        variable: "rostergroup",
                        obsoleteFixedTitles: new[] { "t1", "t2" },
                        children: new IComposite[]
                        {
                            Create.Entity.NumericIntegerQuestion(id: Id.gC, variable: "nq2")
                        }));

            var importDataParsingService = CreatePreloadedDataService(questionnaireDocument);

            AssignmentPreloadedDataRecord[] data = importDataParsingService.CreatePreloadedDataDtosFromPanelData(new[] {
                CreatePreloadedDataByFile(new[] { ServiceColumns.InterviewId, "nq1" }, new[] {new[] {"1", null}}, questionnaireDocument.Title),
                CreatePreloadedDataByFile(new[] { "rostergroup__id", "nq2", ServiceColumns.InterviewId}, new[] {new[] {"1", null, "1"}}, "rostergroup")
            });

            Assert.That(data, Has.Length.EqualTo(1));
            Assert.That(data[0].PreloadedDataDto.Answers, Has.Count.EqualTo(0));
        }
    }
}
