﻿using System;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration;

namespace WB.Tests.Unit.BoundedContexts.Designer.CodeGeneratorTests
{
    internal class when_generate_single_class_for_version_6_with_questionnaire_with_roster : CodeGeneratorTestsContext
    {
        Establish context = () =>
        {
            questionnaire =
                Create.QuestionnaireDocument(
                    children: new[]
                    {
                        Create.Chapter(
                            title: "Chapter",
                            chapterId: chapterId,
                            children: new[]
                            {
                                Create.Roster(
                                    rosterId: rosterId,
                                    variable: "fixed_roster",
                                    rosterSizeSourceType: RosterSizeSourceType.FixedTitles,
                                    fixedTitles: new string[] {"1", "2"})
                            })
                    });

            generator = Create.CodeGenerator();
        };

        Because of = () =>
            generatedClassContent =
                generator.Generate(questionnaire, version);

        It should_generate_class_with_V3_namespaces_included = () =>
            generatedClassContent.ShouldNotContain("WB.Core.SharedKernels.DataCollection.V3");

        It should_generate_class_without_AbstractConditionalLevelInstanceV3 = () =>
            generatedClassContent.ShouldNotContain("AbstractConditionalLevelInstanceV3");

        

        private static Version version = new Version(6, 0, 0);
        private static CodeGenerator generator;
        private static string generatedClassContent;
        private static readonly Guid chapterId = Guid.Parse("11111111111111111111111111111111");
        private static readonly Guid rosterId = Guid.Parse("22222222222222222222222222222222");
        private static QuestionnaireDocument questionnaire;
    }
}
