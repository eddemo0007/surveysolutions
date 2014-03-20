using System;
using Machine.Specifications;
using Moq;
using Ncqrs.Spec;
using WB.Core.BoundedContexts.Headquarters.Exceptions;
using WB.Core.BoundedContexts.Headquarters.Implementation.Aggregates;
using WB.Core.BoundedContexts.Headquarters.PasswordPolicy;
using WB.Core.BoundedContexts.Headquarters.Services;
using It = Machine.Specifications.It;

namespace WB.Core.BoundedContexts.Headquarters.Tests.SurveyTests
{
    internal class when_registering_supervisor_with_password_that_doesnot_match_password_pattern : SurveyTestsContext
    {
        Establish context = () =>
        {
            loginChecker = Mock.Of<ISupervisorLoginService>(x => x.IsUnique(login) == true);

            SetupInstanceToMockedServiceLocator<ISupervisorLoginService>(loginChecker);

            var passwordPolicy = CreateApplicationPasswordPolicySettings(minPasswordLength: 3, passwordPattern: "^(?=.*[a-z])(?=.*[0-9])(?=.*[A-Z]).*$");

            SetupInstanceToMockedServiceLocator<ApplicationPasswordPolicySettings>(passwordPolicy);

            survey = CreateSurvey();

            eventContext = new EventContext();
        };

        private Because of = () =>
            exception = Catch.Exception(() =>
                survey.RegisterSupervisor(login, password));

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        It should_throw_SurveyException = () =>
            exception.ShouldBeOfExactType<SurveyException>();

        It should_throw_exception_with_message_containing__name____empty__ = () =>
            exception.Message.ToLower().ToSeparateWords().ShouldContain("password", "contain", "one", "number", "upper", "lower", "character");

        private static EventContext eventContext;
        private static Survey survey;
        private static string login = "Vasya";
        private static string password = "aaaaaaa";
        private static ISupervisorLoginService loginChecker;
        private static Exception exception;
    }
}