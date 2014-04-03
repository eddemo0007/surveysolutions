﻿using System;
using System.Web.Mvc;
using System.Web.Security;
using Main.Core.Entities.SubEntities;
using Main.Core.Utility;
using Questionnaire.Core.Web.Helpers;
using Questionnaire.Core.Web.Security;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.UI.Headquarters.Models;

namespace WB.UI.Headquarters.Controllers
{
    public class AccountController : Controller
    {
        private readonly IFormsAuthentication authentication;
        private readonly IGlobalInfoProvider globalProvider;
        private readonly IPasswordHasher passwordHasher;
        private readonly IHeadquartersSynchronizer headquartersSynchronizer;
        private readonly Func<string, string, bool> validateUserCredentials;

        public AccountController(IFormsAuthentication authentication, IGlobalInfoProvider globalProvider, IPasswordHasher passwordHasher,
            IHeadquartersSynchronizer headquartersSynchronizer)
            : this(authentication, globalProvider, passwordHasher, headquartersSynchronizer, Membership.ValidateUser) { }

        internal AccountController(IFormsAuthentication auth, IGlobalInfoProvider globalProvider, IPasswordHasher passwordHasher,
            IHeadquartersSynchronizer headquartersSynchronizer, Func<string, string, bool> validateUserCredentials)
        {
            this.authentication = auth;
            this.globalProvider = globalProvider;
            this.passwordHasher = passwordHasher;
            this.headquartersSynchronizer = headquartersSynchronizer;
            this.validateUserCredentials = validateUserCredentials;
        }

        [HttpGet]
        public ActionResult LogOn()
        {
            this.ViewBag.ActivePage = MenuItem.Logon;
            return this.View();
        }

        [HttpPost]
        public ActionResult LogOn(LogOnModel model, string returnUrl)
        {
            this.ViewBag.ActivePage = MenuItem.Logon;
            if (this.ModelState.IsValid)
            {
                if (this.LoginIncludingHeadquartersData(model.UserName, model.Password))
                {
                    bool isHeadquarter = Roles.IsUserInRole(model.UserName, UserRoles.Headquarter.ToString());
                    if (isHeadquarter)
                    {
                        this.authentication.SignIn(model.UserName, false);
                        
                        return this.RedirectToAction("Index", "HQ");
                    }

                    this.ModelState.AddModelError(string.Empty, "You have no access to this site. Contact your administrator.");
                }
                else
                    this.ModelState.AddModelError(string.Empty, "The user name or password provided is incorrect.");
            }

            return this.View(model);
        }

        private bool LoginIncludingHeadquartersData(string login, string password)
        {
            if (this.LoginUsingLocalDatabase(login, password))
                return true;

            this.UpdateLocalDataFromHeadquarters(login, password);

            return this.LoginUsingLocalDatabase(login, password);
        }

        private void UpdateLocalDataFromHeadquarters(string login, string password)
        {
            //this.headquartersSynchronizer.Pull(login, password);
        }

        private bool LoginUsingLocalDatabase(string login, string password)
        {
            return this.validateUserCredentials(login, this.passwordHasher.Hash(password))
                || this.validateUserCredentials(login, SimpleHash.ComputeHash(password));
        }

        public bool IsLoggedIn()
        {
            return this.globalProvider.GetCurrentUser() != null;
        }

        public ActionResult LogOff()
        {
            this.authentication.SignOut();
            return this.Redirect("~/");
        }

        public Guid GetCurrentUser()
        {
            UserLight currentUser = this.globalProvider.GetCurrentUser();
            return currentUser != null ? currentUser.Id : Guid.Empty;
        }
    }
}