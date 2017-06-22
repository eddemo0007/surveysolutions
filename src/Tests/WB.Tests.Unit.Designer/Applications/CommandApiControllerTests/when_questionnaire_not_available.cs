using System.Net;
using System.Net.Http;
using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.CommandBus;
using WB.UI.Designer.Api;
using WB.UI.Designer.Code;
using WB.UI.Designer.Code.Implementation;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Designer.Applications.CommandApiControllerTests
{
    internal class when_questionnaire_not_available : CommandApiControllerTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            
            model = new CommandController.CommandExecutionModel();
            commandInflaterMock = new Mock<ICommandInflater>();
            commandInflaterMock.Setup(x => x.PrepareDeserializedCommandForExecution(Moq.It.IsAny<ICommand>()))
                .Throws(new CommandInflaitingException(CommandInflatingExceptionType.EntityNotFound, "test"));

            controller = CreateCommandController(commandInflater:commandInflaterMock.Object);
        }

        private void BecauseOf() =>
            message = controller.Post(model);

        [NUnit.Framework.Test] public void should_not_be_null () =>
            message.ShouldNotBeNull();

        [NUnit.Framework.Test] public void should_not_be_correct_status () =>
            message.StatusCode.ShouldEqual(HttpStatusCode.NotFound);

        private static CommandController controller;
        private static CommandController.CommandExecutionModel model;
        private static Mock<ICommandInflater> commandInflaterMock;
        private static HttpResponseMessage message;
    }
}
