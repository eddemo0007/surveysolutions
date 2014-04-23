﻿using System;
using System.Web.Mvc;
using WB.Core.GenericSubdomains.Utils;
using WB.UI.Shared.Web.Membership;

namespace WB.UI.Designer.Controllers
{
    [CustomAuthorize]
    public class AppController : BaseController
    {
        private readonly IMembershipUserService userHelper;

        public AppController(IMembershipUserService userHelper) : base(userHelper)
        {
            this.userHelper = userHelper;
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Open(Guid id)
        {
            return Redirect("~/app#/" + id.FormatGuid());
        }
    }
}