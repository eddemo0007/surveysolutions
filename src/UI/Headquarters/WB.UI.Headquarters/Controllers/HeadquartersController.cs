﻿using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Main.Core.Entities.SubEntities;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.SurveyManagement.Views.User;
using WB.Core.SharedKernels.SurveyManagement.Web.Code.Security;
using WB.Core.SharedKernels.SurveyManagement.Web.Controllers;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Membership;
using WB.UI.Shared.Web.Filters;

namespace WB.UI.Headquarters.Controllers
{
    [Authorize(Roles = "Administrator, Observer")]
    public class HeadquartersController : TeamController
    {
        public HeadquartersController(ICommandService commandService, 
                              IGlobalInfoProvider globalInfo, 
                              ILogger logger,
                              IUserViewFactory userViewFactory,
                              IPasswordHasher passwordHasher)
            : base(commandService, globalInfo, logger, userViewFactory, passwordHasher)
        {
            
        }

        public ActionResult Create()
        {
            this.ViewBag.ActivePage = MenuItem.Headquarters;

            return this.View(new UserModel());
        }

        [HttpPost]
        [PreventDoubleSubmit]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public ActionResult Create(UserModel model)
        {
            this.ViewBag.ActivePage = MenuItem.Headquarters;

            if (this.ModelState.IsValid)
            {
                UserView user = GetUserByName(model.UserName);
                if (user == null)
                {
                    this.CreateHeadquarters(model);
                    this.Success("Headquarters user was successfully created");
                    return this.RedirectToAction("Index");
                }
                else
                {
                    this.Error("User name already exists. Please enter a different user name.");
                }
            }

            return this.View(model);
        }

        [Authorize(Roles = "Administrator, Observer")]
        public ActionResult Index()
        {
            this.ViewBag.ActivePage = MenuItem.Headquarters;

            return this.View();
        }

        [Authorize(Roles = "Administrator")]
        public ActionResult Edit(Guid id)
        {
            this.ViewBag.ActivePage = MenuItem.Headquarters;

            var user = this.GetUserById(id);

            if(user == null) throw new HttpException(404, string.Empty);

            return this.View(new UserEditModel()
                {
                    Id = user.PublicKey,
                    Email = user.Email,
                    IsLocked = user.IsLockedByHQ,
                    UserName = user.UserName
                });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public ActionResult Edit(UserEditModel model)
        {
            this.ViewBag.ActivePage = MenuItem.Headquarters;

            if (this.ModelState.IsValid)
            {
                if (User.Identity.IsObserver())
                {
                    this.Error("You cannot perform any operation in observer mode.");
                }
                else
                {
                    var user = this.GetUserById(model.Id);
                    if (user != null)
                    {
                        bool isAdmin = Roles.IsUserInRole(user.UserName, UserRoles.Administrator.ToString());

                        if (!isAdmin)
                        {
                            this.UpdateAccount(user: user, editModel: model);
                            this.Success(string.Format("Information about <b>{0}</b> successfully updated",
                                user.UserName));
                            return this.RedirectToAction("Index");
                        }

                        this.Error(
                            "Could not update user information because you don't have permission to perform this operation");
                    }
                    else
                    {
                        this.Error("Could not update user information because current user does not exist");
                    }
                }
            }

            return this.View(model);
        }
    }
}