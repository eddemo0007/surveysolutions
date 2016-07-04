﻿using Machine.Specifications;
using WB.UI.Designer.Controllers;
using WB.UI.Shared.Web.Extensions;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Designer.Applications.QuestionnaireControllerTests
{
    internal class when_editing_options_and_was_no_uploaded_file : QuestionnaireControllerTestContext
    {
        Establish context = () =>
        {
            controller = CreateQuestionnaireController();
            SetControllerContextWithSession(controller, "options", new QuestionnaireController.EditOptionsViewModel());
        };

        Because of = () => controller.EditOptions(null);

        It should_add_error_message_to_temp_data = () =>
            controller.TempData[Alerts.ERROR].ShouldEqual("Choose tab-separated values file to upload, please");

        private static QuestionnaireController controller;
    }
}
