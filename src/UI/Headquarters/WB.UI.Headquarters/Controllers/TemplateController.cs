﻿using System;
using System.Net;
using System.Security.Authentication;
using System.ServiceModel.Security;
using System.Threading.Tasks;
using System.Web.Mvc;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernel.Structures.Synchronization.Designer;
using WB.Core.SharedKernel.Utils.Implementation.Services;
using WB.Core.SharedKernel.Utils.Services.Rest;
using WB.Core.SharedKernels.SurveyManagement.Views.Template;
using WB.Core.SharedKernels.SurveyManagement.Web.Code;
using WB.Core.SharedKernels.SurveyManagement.Web.Controllers;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Membership;

namespace WB.UI.Headquarters.Controllers
{
    [Authorize(Roles = "Headquarter")]
    public class TemplateController : BaseController
    {
        private readonly IRestService designerQuestionnaireApiRestService;

        public TemplateController(ICommandService commandService, IGlobalInfoProvider globalInfo, ILogger logger, IRestService designerQuestionnaireApiRestService)
            : base(commandService, globalInfo, logger)
        {
            this.designerQuestionnaireApiRestService = designerQuestionnaireApiRestService;
            this.ViewBag.ActivePage = MenuItem.Administration;

            if (AppSettings.Instance.AcceptUnsignedCertificate)
            {
                ServicePointManager.ServerCertificateValidationCallback =
                    (self, certificate, chain, sslPolicyErrors) => true;
            }
        }


        private RestCredentials designerUserCredentials
        {
            get { return (RestCredentials)this.Session[this.GlobalInfo.GetCurrentUser().Name]; }

            set { this.Session[this.GlobalInfo.GetCurrentUser().Name] = value; }
        }

        public ActionResult Import(QuestionnaireListInputModel model)
        {
            if (this.designerUserCredentials == null)
            {
                return this.RedirectToAction("LoginToDesigner");
            }

            return
                this.View();
        }

        public ActionResult LoginToDesigner()
        {
            this.Attention("Before log in, please make sure that the 'Designer' website is available and it's not in the maintenance mode");
            return this.View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> LoginToDesigner(LogOnModel model)
        {
            if (this.ModelState.IsValid)
            {
                var designerUserCredentials = new RestCredentials {Login = model.UserName, Password = model.Password};

                try
                {
                    await this.designerQuestionnaireApiRestService.PostAsync(url: "validatecredentials", credentials: designerUserCredentials);
                    
                    this.designerUserCredentials = designerUserCredentials;

                    return this.RedirectToAction("Import");
                }
                catch (Exception ex)
                {
                    this.Error(
                        string.Format(
                            "Could not connect to designer. Please check that designer is available and try <a href='{0}'>again</a>",
                            GlobalHelper.GenerateUrl("Import", "Template", new { area = string.Empty })));
                    this.Logger.Error("Could not connect to designer.", ex);
                }
            }

            return this.View(model);
        }
    }
}