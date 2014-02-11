﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using WB.Core.BoundedContexts.Supervisor.Implementation.Services.DataExport;
using WB.Core.BoundedContexts.Supervisor.Views.DataExport;

namespace WB.Core.BoundedContexts.Supervisor.Tests.ServiceTests.DataExport.StataEnvironmentContentGeneratorTests
{
    internal class when_HeaderStructureForLevel_has_one_question_with_labels : StataEnvironmentContentGeneratorTestContext
    {
        Establish context = () =>
        {
            oneQuestionHeaderStructureForLevel =
                CreateHeaderStructureForLevel(CreateExportedHeaderItem(questionsVariableName, questionsTitle, CreateLabelItem("c1", "t1"), CreateLabelItem("c2", "t2")));

            stataEnvironmentContentGenerator = CreateStataEnvironmentContentGenerator(oneQuestionHeaderStructureForLevel, dataFileName);
        };

        Because of = () => stataGeneratedContent = stataEnvironmentContentGenerator.ContentOfAdditionalFile;

        It should_contain_stata_script_for_insheet_file = () =>
            stataGeneratedContent.ShouldContain(string.Format("insheet using \"{0}\", comma\r\n", dataFileName));

        It should_contain_stata_variable_on_title_mapping = () =>
           stataGeneratedContent.ShouldContain(string.Format("label variable {0} `\"{1}\"'", questionsVariableName, questionsTitle));

        It should_contain_stata_variable_on_label_mapping = () =>
           stataGeneratedContent.ShouldContain(string.Format("label values {0} l{0}", questionsVariableName));

        It should_contain_label_definition = () =>
            stataGeneratedContent.ShouldContain(string.Format("label define l{0} c1 `\"t1\"' c2 `\"t2\"'", questionsVariableName));

        private static StataEnvironmentContentGenerator stataEnvironmentContentGenerator;
        private static HeaderStructureForLevel oneQuestionHeaderStructureForLevel;
        private static string dataFileName = "data file name";

        private static string questionsVariableName = "var1";
        private static string questionsTitle = "title1";
        private static string stataGeneratedContent;
    }
}
