using System;
using Machine.Specifications;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Synchronization;
using WB.Core.SharedKernels.SurveyManagement.Views;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.InterviewPackagesServiceTests
{
    internal class when_getting_invalid_packages_count : InterviewPackagesServiceTestsContext
    {
        Establish context = () =>
        {
            packagesStorage = new TestPlainStorage<InterviewPackage>();
            brokenPackagesStorage = new TestPlainStorage<BrokenInterviewPackage>();

            for (int i = 0; i < 100; i++)
                brokenPackagesStorage.Store(new BrokenInterviewPackage { InterviewId = Guid.NewGuid() }, null);

            interviewPackagesService = CreateInterviewPackagesService(interviewPackageStorage: packagesStorage, brokenInterviewPackageStorage: brokenPackagesStorage);
        };

        Because of = () => packagesLength = interviewPackagesService.InvalidPackagesCount;

        It should_be_specified_packages_length = () => packagesLength.ShouldEqual(100);

        private static int packagesLength;
        private static InterviewPackagesService interviewPackagesService;
        private static IPlainStorageAccessor<BrokenInterviewPackage> brokenPackagesStorage;
        private static IPlainStorageAccessor<InterviewPackage> packagesStorage;
    }
}