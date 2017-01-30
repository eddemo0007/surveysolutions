﻿using System;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.Infrastructure.FileSystem;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.DataCollection.QuestionnaireAssemblyFileAccessorTests
{
    internal class when_getting_assembly_as_base_64_string : QuestionnaireAssemblyFileAccessorTestsContext
    {
        private Establish context = () =>
        {
            AssemblyServiceMock.Setup(x => x.GetAssemblyInfo(Moq.It.IsAny<string>())).Returns(new AssemblyInfo() {Content = data1 });
            questionnaireAssemblyFileAccessor = CreateQuestionnaireAssemblyFileAccessor(assemblyService: AssemblyServiceMock.Object);
        };

        Because of = () => result = questionnaireAssemblyFileAccessor.GetAssemblyAsBase64String(questionnaireId, version);

        It should_data_of_returned_file_be_equal_to_expected = () =>
            result.ShouldEqual(expected);

        private static QuestionnaireAssemblyFileAccessor questionnaireAssemblyFileAccessor;
        private static readonly Mock<IAssemblyService> AssemblyServiceMock = CreateIAssemblyService();
        private static Guid questionnaireId = Guid.Parse("33332222111100000000111122223333");
        private static long version = 3;

        private static byte[] data1 = new byte[] { 1 };
        private static string expected = Convert.ToBase64String(data1);

        private static string result;
    }
}
