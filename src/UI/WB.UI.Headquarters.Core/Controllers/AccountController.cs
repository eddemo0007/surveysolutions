﻿using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WB.Core.BoundedContexts.Headquarters.Resources;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Users;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.UI.Headquarters.Models.CompanyLogo;
using WB.UI.Headquarters.Services.Impl;
using WB.UI.Shared.Web.Captcha;
using WB.UI.Shared.Web.Services;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace WB.UI.Headquarters.Controllers
{
    public class AccountController : Controller
    {
        private readonly IPlainKeyValueStorage<CompanyLogo> appSettingsStorage;
        private readonly ICaptchaService captchaService;
        private readonly ICaptchaProvider captchaProvider;
        private readonly SignInManager<HqUser> signInManager;
        protected readonly IAuthorizedUser authorizedUser;
        private readonly ILogger<AccountController> logger;
        private readonly IUserRepository userRepository;

        public AccountController(IPlainKeyValueStorage<CompanyLogo> appSettingsStorage, 
            ICaptchaService captchaService,
            ICaptchaProvider captchaProvider,
            SignInManager<HqUser> signInManager,
            IAuthorizedUser authorizedUser,
            ILogger<AccountController> logger,
            IUserRepository userRepository)
        {
            this.appSettingsStorage = appSettingsStorage;
            this.captchaService = captchaService;
            this.captchaProvider = captchaProvider;
            this.signInManager = signInManager;
            this.authorizedUser = authorizedUser;
            this.logger = logger;
            this.userRepository = userRepository;
        }

        [HttpGet]
        public async Task<IActionResult> LogOn(string returnUrl)
        {
            this.ViewBag.ActivePage = MenuItem.Logon;
            this.ViewBag.ReturnUrl = returnUrl;
            this.ViewBag.HasCompanyLogo = this.appSettingsStorage.GetById(CompanyLogo.CompanyLogoStorageKey) != null;

            var providers = (await signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            return this.View(new LogOnModel
            {
                RequireCaptcha = this.captchaService.ShouldShowCaptcha(null),
                ExternalLogins = providers
            });
        }

        [HttpGet]
        public async Task<IActionResult> LogOn2fa(string returnUrl)
        {
            this.ViewBag.ActivePage = MenuItem.Logon;
            this.ViewBag.ReturnUrl = returnUrl;
            this.ViewBag.HasCompanyLogo = this.appSettingsStorage.GetById(CompanyLogo.CompanyLogoStorageKey) != null;
            
            var user = await signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                return RedirectToAction("Index", "Home");
            }

            return this.View(new LogOn2faModel());
        }

        [HttpGet]
        public async Task<IActionResult> LoginWithRecoveryCode()
        {
            this.ViewBag.ActivePage = MenuItem.Logon;
            //this.ViewBag.ReturnUrl = returnUrl;
            this.ViewBag.HasCompanyLogo = this.appSettingsStorage.GetById(CompanyLogo.CompanyLogoStorageKey) != null;

            var user = await signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                return RedirectToAction("Index", "Home");
            }

            return this.View(new LoginWithRecoveryCodeModel());
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> LogOn(LogOnModel model, string returnUrl)
        {
            this.ViewBag.ActivePage = MenuItem.Logon;
            this.ViewBag.HasCompanyLogo = this.appSettingsStorage.GetById(CompanyLogo.CompanyLogoStorageKey) != null;
            model.RequireCaptcha = this.captchaService.ShouldShowCaptcha(model.UserName);

            if (model.RequireCaptcha && !await this.captchaProvider.IsCaptchaValid(Request))
            {
                this.ModelState.AddModelError("InvalidCaptcha", ErrorMessages.PleaseFillCaptcha);
                return this.View(model);
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var signInResult = await this.signInManager.PasswordSignInAsync(model.UserName, model.Password, true, false);
            if (signInResult.Succeeded)
            {
                this.captchaService.ResetFailedLogin(model.UserName);
                return Redirect(returnUrl ?? Url.Action("Index", "Home"));
            }
            if (signInResult.RequiresTwoFactor)
            {
                return RedirectToAction("LogOn2fa", new { ReturnUrl = returnUrl, RememberMe = true });
            }

            if (signInResult.IsLockedOut)
            {
                this.captchaService.ResetFailedLogin(model.UserName);
                this.ModelState.AddModelError("LockedOut", ErrorMessages.SiteAccessNotAllowed);
                return View(model);
            }

            this.captchaService.RegisterFailedLogin(model.UserName);
            model.RequireCaptcha = this.captchaService.ShouldShowCaptcha(model.UserName);
            this.ModelState.AddModelError("InvalidCredentials", ErrorMessages.IncorrectUserNameOrPassword);
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> LogOn2fa(LogOn2faModel model, string returnUrl)
        {
            this.ViewBag.ActivePage = MenuItem.Logon;
            this.ViewBag.HasCompanyLogo = this.appSettingsStorage.GetById(CompanyLogo.CompanyLogoStorageKey) != null;
            
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                return RedirectToAction("LogOn", new { ReturnUrl = returnUrl, RememberMe = true });
            }

            var authenticatorCode = model.TwoFactorCode.Replace(" ", string.Empty).Replace("-", string.Empty);
            var signInResult = await signInManager.TwoFactorAuthenticatorSignInAsync(authenticatorCode, true, false);
            
            if (signInResult.Succeeded)
            {
                return Redirect(returnUrl ?? Url.Action("Index", "Home"));
            }
            
            if (signInResult.IsLockedOut)
            {
                this.ModelState.AddModelError("LockedOut", ErrorMessages.SiteAccessNotAllowed);
                return View(model);
            }

            
            this.ModelState.AddModelError("InvalidCredentials", ErrorMessages.InvalidAuthenticatorCode);
            return View(model);
        }


        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> LoginWithRecoveryCode(LoginWithRecoveryCodeModel model, string returnUrl)
        {
            this.ViewBag.ActivePage = MenuItem.Logon;
            this.ViewBag.HasCompanyLogo = this.appSettingsStorage.GetById(CompanyLogo.CompanyLogoStorageKey) != null;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                return RedirectToAction("LogOn", new { ReturnUrl = returnUrl, RememberMe = true });
            }

            var recoveryCode = model.RecoveryCode.Replace(" ", string.Empty);
            var signInResult = await signInManager.TwoFactorRecoveryCodeSignInAsync(recoveryCode);

            if (signInResult.Succeeded)
            {
                return Redirect(returnUrl ?? Url.Action("Index", "Home"));
            }

            if (signInResult.IsLockedOut)
            {
                this.ModelState.AddModelError("LockedOut", ErrorMessages.SiteAccessNotAllowed);
                return View(model);
            }


            this.ModelState.AddModelError("InvalidCredentials", ErrorMessages.InvalidRecoveryCode);
            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ExternalLogin()
        {
            return this.RedirectToAction("LogOn");
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult ExternalLogin(string provider, string returnUrl = null)
        {
            // Request a redirect to the external login provider.
            var redirectUrl = Url.Action("ExternalLoginCallback", new { returnUrl });
            var properties = signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return new ChallengeResult(provider, properties);
        }

        [HttpGet]
        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null, string remoteError = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            if (remoteError != null)
            {
                //ErrorMessage = $"Error from external provider: {remoteError}";
                return RedirectToAction("LogOn", new { ReturnUrl = returnUrl });
            }
            var info = await signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                //ErrorMessage = "Error loading external login information.";
                return RedirectToAction("LogOn", new { ReturnUrl = returnUrl });
            }

            // Sign in the user with this external login provider if the user already has a login.
            var result = await signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
            if (result.Succeeded)
            {
                //logger.LogInformation("{Name} logged in with {LoginProvider} provider.", info.Principal.Identity.Name, info.LoginProvider);
                return LocalRedirect(returnUrl);
            }
            //user was not found by login
            else if (result == SignInResult.Failed)
            {
                var preferredUsername = info.Principal.FindFirstValue("preferred_username");

                if (!string.IsNullOrEmpty(preferredUsername))
                {
                    var user = this.userRepository.Users.FirstOrDefault(x => x.Profile.ExternalName.ToUpper() == preferredUsername.ToUpper());
                    if (user != null)
                    {
                        var addLoginResult = await signInManager.UserManager.AddLoginAsync(user, info);
                        if (addLoginResult.Succeeded)
                        {
                            logger.LogInformation("User created an account using {Name} provider.", info.LoginProvider);
                            
                            await signInManager.SignInAsync(user, isPersistent: false);
                            
                            return LocalRedirect(returnUrl);
                        }
                    }
                }
                else
                {

                }
            }


            return RedirectToAction("LogOn", new { ReturnUrl = returnUrl });
            //display that account doesn't exists
            //return RedirectToAction("LogOn", new { ReturnUrl = returnUrl });

            //add 2fa support

            //handle locked
            /*if (result.IsLockedOut)
            {
                return RedirectToPage("./Lockout");
            }*/

            /*else
            {
                // If the user does not have an account, then ask the user to create an account.
                ReturnUrl = returnUrl;
                LoginProvider = info.LoginProvider;
                if (info.Principal.HasClaim(c => c.Type == ClaimTypes.Email))
                {
                    Input = new InputModel
                    {
                        Email = info.Principal.FindFirstValue(ClaimTypes.Email)
                    };
                }
                return Page();
            }*/
        }


        public IActionResult LogOff()
        {
            this.signInManager.SignOutAsync();
            return this.Redirect("~/");
        }

        [Authorize(Roles = "Headquarter, Supervisor")]
        public async Task<ActionResult> ReturnToObserver()
        {
            if (!this.authorizedUser.IsObserver)
                return NotFound();

            var observerName = User.FindFirst(AuthorizedUser.ObserverClaimType)?.Value;
            var observer = await this.signInManager.UserManager.FindByNameAsync(observerName);

            await this.signInManager.SignOutAsync();
            await this.signInManager.SignInAsync(observer, true);
            
            return this.Redirect("~/");
        }
        
        private static readonly Guid[] ObservableRoles = {UserRoles.Headquarter.ToUserId(), UserRoles.Supervisor.ToUserId()};
        
        [Authorize(Roles = "Administrator, Observer")]
        public async Task<IActionResult> ObservePerson(string personName)
        {
            if (string.IsNullOrEmpty(personName))
                return  NotFound();

            var user = await this.signInManager.UserManager.FindByNameAsync(personName);
            if (user == null || !ObservableRoles.Contains(user.Roles.First().Id))
               return NotFound();

            //do not forget pass current user to display you are observing
            await this.SignInAsObserverAsync(personName);

            return user.IsInRole(UserRoles.Headquarter) ?
                this.RedirectToAction("SurveysAndStatuses", "Reports") :
                this.RedirectToAction("SurveysAndStatusesForSv", "Reports");
        }

        public async Task SignInAsObserverAsync(string userName)
        {
            var userToObserve = await this.signInManager.UserManager.FindByNameAsync(userName);
            userToObserve.Claims.Add(new HqUserClaim
            {
                UserId = userToObserve.Id,
                ClaimType = AuthorizedUser.ObserverClaimType,
                ClaimValue = authorizedUser.UserName
            });
            userToObserve.Claims.Add(new HqUserClaim
            {
                UserId = userToObserve.Id,
                ClaimType = ClaimTypes.Role,
                ClaimValue = Enum.GetName(typeof(UserRoles), UserRoles.Observer)
            });
            
            await this.signInManager.SignOutAsync();
            await this.signInManager.SignInAsync(userToObserve, true);
        }
    }
}
