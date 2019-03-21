using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.BoundedContexts.Headquarters.WebInterview;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.UI.Headquarters.Filters;
using WB.UI.Shared.Web.Captcha;
using Microsoft.AspNet.Identity;
using StackExchange.Exceptional;
using WB.Core.BoundedContexts.Headquarters.Invitations;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Enumerator.Native.WebInterview;
using WB.Enumerator.Native.WebInterview.Services;
using WB.Infrastructure.Native.Storage;
using WB.UI.Headquarters.Models.WebInterview;
using WB.UI.Shared.Web.Services;

namespace WB.UI.Headquarters.Controllers
{
    [BrowsersRestriction]
    public partial class WebInterviewController : BaseController
    {
        private readonly ICommandService commandService;
        private readonly IWebInterviewConfigProvider configProvider;
        private readonly IImageFileStorage imageFileStorage;
        private readonly IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory;
        private readonly IStatefulInterviewRepository statefulInterviewRepository;
        private readonly IUserViewFactory usersRepository;
        private readonly IInterviewUniqueKeyGenerator keyGenerator;
        private readonly ICaptchaProvider captchaProvider;
        private readonly IPlainStorageAccessor<Assignment> assignments;
        private readonly IWebInterviewConfigProvider webInterviewConfigProvider;
        private readonly IImageProcessingService imageProcessingService;
        private readonly IConnectionLimiter connectionLimiter;
        private readonly IWebInterviewNotificationService webInterviewNotificationService;
        private readonly IAudioFileStorage audioFileStorage;
        private readonly IAudioProcessingService audioProcessingService;
        private readonly IPauseResumeQueue pauseResumeQueue;
        private readonly IInvitationService invitationService;
        private readonly INativeReadSideStorage<InterviewSummary> interviewSummary;
        private readonly IInvitationMailingService invitationMailingService;

        private const string CapchaCompletedKey = "CaptchaCompletedKey";
        private const string PasswordVerifiedKey = "PasswordVerifiedKey";
        public static readonly string LastCreatedInterviewIdKey = "lastCreatedInterviewId";
        public static readonly string AskForEmail = "askForEmail";

        private bool CapchaVerificationNeededForInterview(string interviewId)
        {
            var passedInterviews = this.Session[CapchaCompletedKey] as List<string>;
            return !(passedInterviews?.Contains(interviewId)).GetValueOrDefault();
        }

        private bool PasswordVerificationNeededForInterview(string interviewId)
        {
            var passedInterviews = this.Session[PasswordVerifiedKey] as List<string>;
            return !(passedInterviews?.Contains(interviewId)).GetValueOrDefault();
        }

        private void RememberCapchaFilled(string interviewId)
        {
            var interviews = this.Session[CapchaCompletedKey] as List<string> ?? new List<string>();
            if (!interviews.Contains(interviewId))
            {
                interviews.Add(interviewId);
            }
            this.Session[CapchaCompletedKey] = interviews;
        }

        private void RememberPasswordVerified(string interviewId)
        {
            var interviews = this.Session[PasswordVerifiedKey] as List<string> ?? new List<string>();
            if (!interviews.Contains(interviewId))
            {
                interviews.Add(interviewId);
            }
            this.Session[PasswordVerifiedKey] = interviews;
        }

        public WebInterviewController(ICommandService commandService,
            IWebInterviewConfigProvider configProvider,
            IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory,
            IImageFileStorage imageFileStorage,
            IStatefulInterviewRepository statefulInterviewRepository,
            IWebInterviewConfigProvider webInterviewConfigProvider,
            IImageProcessingService imageProcessingService,
            IConnectionLimiter connectionLimiter,
            IWebInterviewNotificationService webInterviewNotificationService,
            ILogger logger, IUserViewFactory usersRepository,
            IInterviewUniqueKeyGenerator keyGenerator,
            ICaptchaProvider captchaProvider,
            IPlainStorageAccessor<Assignment> assignments, 
            IAudioFileStorage audioFileStorage,
            IAudioProcessingService audioProcessingService,
            IPauseResumeQueue pauseResumeQueue, 
            IInvitationService invitationService, 
            INativeReadSideStorage<InterviewSummary> interviewSummary, 
            IInvitationMailingService invitationMailingService)
            : base(commandService, logger)
        {
            this.commandService = commandService;
            this.configProvider = configProvider;
            this.questionnaireBrowseViewFactory = questionnaireBrowseViewFactory;
            this.imageFileStorage = imageFileStorage;
            this.statefulInterviewRepository = statefulInterviewRepository;
            this.webInterviewConfigProvider = webInterviewConfigProvider;
            this.imageProcessingService = imageProcessingService;
            this.connectionLimiter = connectionLimiter;
            this.webInterviewNotificationService = webInterviewNotificationService;
            this.usersRepository = usersRepository;
            this.keyGenerator = keyGenerator;
            this.captchaProvider = captchaProvider;
            this.assignments = assignments;
            this.audioFileStorage = audioFileStorage;
            this.audioProcessingService = audioProcessingService;
            this.pauseResumeQueue = pauseResumeQueue;
            this.invitationService = invitationService;
            this.interviewSummary = interviewSummary;
            this.invitationMailingService = invitationMailingService;
        }

        [WebInterviewAuthorize]
        public ActionResult Section(string id, string sectionId)
        {
            var interview = this.statefulInterviewRepository.Get(id);

            var targetSectionIsEnabled = interview.IsEnabled(Identity.Parse(sectionId));
            if (!targetSectionIsEnabled)
            {
                return this.RedirectToFirstSection(id, interview);
            }

            var webInterviewConfig = this.configProvider.Get(interview.QuestionnaireIdentity);
            if (webInterviewConfig.UseCaptcha && !this.IsAuthorizedUser(interview.CurrentResponsibleId) && this.CapchaVerificationNeededForInterview(id))
            {
                var returnUrl = GenerateUrl(@"Section", id, sectionId);
                return this.RedirectToAction("Resume", routeValues: new { id, returnUrl });
            }

            LogResume(interview);
            return this.View("Index");
        }

        public string GenerateUrl(string action, string interviewId, string sectionId = null)
        {
            return $@"~/WebInterview/{interviewId}/{action}" + (string.IsNullOrWhiteSpace(sectionId) ? "" : $@"/{sectionId}");
        }

        public ActionResult Start(string id)
        {
            var invitationInfo = GetInvitationInfoByToken(id);

            if (invitationInfo.Assignment == null)
                throw new InterviewAccessException(InterviewAccessExceptionReason.InterviewNotFound, Enumerator.Native.Resources.WebInterview.Error_NotFound);

            var assignment = invitationInfo.Assignment;
            var invitation = invitationInfo.Invitation;

            if (assignment.Archived)
            {
                throw new InterviewAccessException(InterviewAccessExceptionReason.InterviewExpired, Enumerator.Native.Resources.WebInterview.Error_InterviewExpired);
            }

            if (!(assignment.WebMode ?? false))
            {
                // Compatibility issue. Every time users will use link, they will create a new interview. All links will be bounced back
                throw new InterviewAccessException(InterviewAccessExceptionReason.InterviewNotFound, Enumerator.Native.Resources.WebInterview.Error_NotFound);
            }

            var webInterviewConfig = this.configProvider.Get(assignment.QuestionnaireId);
            if (!webInterviewConfig.Started)
                throw new InterviewAccessException(InterviewAccessExceptionReason.InterviewExpired, Enumerator.Native.Resources.WebInterview.Error_InterviewExpired);

            if (assignment.Quantity == 1)
            {
                // personal link
                if (!webInterviewConfig.UseCaptcha && string.IsNullOrWhiteSpace(assignment.Password))
                {
                    if (invitation?.InterviewId != null)
                    {
                        if (invitation.Interview.Status >= InterviewStatus.Completed)
                            throw new InterviewAccessException(InterviewAccessExceptionReason.NoActionsNeeded,
                                Enumerator.Native.Resources.WebInterview.Error_NoActionsNeeded);
                        return this.Redirect(GenerateUrl("Cover", invitation.InterviewId));
                    }

                    var interviewId = this.CreateInterview(assignment);
                    if (invitation != null)
                    {
                        invitationService.InterviewWasCreated(invitation.Id, interviewId);
                    }

                    return this.Redirect(GenerateUrl("Cover", interviewId));
                }
                else
                {
                    // page should be shown
                }
            }
            else
            {
                // public mode
                var interviewIdCookie = Request.Cookies[$"InterviewId-{assignment.Id}"];
                if (interviewIdCookie != null)
                {
                    if (Guid.TryParse(interviewIdCookie.Value, out Guid interviewIdFromCookie))
                    {
                        return this.Redirect(GenerateUrl("Cover", interviewIdFromCookie.FormatGuid()));
                    }
                }

                if (!webInterviewConfig.UseCaptcha && string.IsNullOrWhiteSpace(assignment.Password))
                {
                    var interviewId = this.CreateInterview(assignment);
                    
                    Response.Cookies.Add(new HttpCookie($"InterviewId-{assignment.Id}")
                    {
                        Value = interviewId, Expires = DateTime.Now.AddYears(1)
                    });

                    return this.Redirect(GenerateUrl("Cover", interviewId));
                }

                TempData[AskForEmail] = true;
            }

            var model = this.GetStartModel(assignment.QuestionnaireId, webInterviewConfig, assignment);
            model.ServerUnderLoad = !this.connectionLimiter.CanConnect();
            return this.View(model);
        }

        private InvitationInfo GetInvitationInfoByToken(string id)
        {
            var invitation = invitationService.GetInvitationByToken(id);
            Assignment assignment = null;
            if (invitation == null)
            {
                if (int.TryParse(id, out int assignmentId))
                {
                    assignment = this.assignments.GetById(assignmentId);
                }
            }
            else
            {
                assignment = invitation.Assignment;
            }

            return new InvitationInfo
            {
                Assignment = assignment,
                Invitation = invitation,
                HasMoreThanOneInvitation = id.StartsWith("I", StringComparison.InvariantCultureIgnoreCase)
            };
        }

        private class InvitationInfo
        {
            public Invitation Invitation { get; set; }
            public bool HasMoreThanOneInvitation { get; set; }
            public Assignment Assignment { get; set; }
        }

        [HttpPost]
        [ActionName("Start")]
        [ValidateAntiForgeryToken]
        public ActionResult StartPost(string id, string password)
        {
            var invitationInfo = GetInvitationInfoByToken(id);

            var assignment = invitationInfo.Assignment;
            var invitation = invitationInfo.Invitation;
           
            var webInterviewConfig = this.configProvider.Get(assignment.QuestionnaireId);
            if (!webInterviewConfig.Started)
                throw new InterviewAccessException(InterviewAccessExceptionReason.InterviewExpired, Enumerator.Native.Resources.WebInterview.Error_InterviewExpired);

            if (!this.connectionLimiter.CanConnect())
            {
                var model = this.GetStartModel(assignment.QuestionnaireId, webInterviewConfig, null);
                model.ServerUnderLoad = true;
                return this.View(model);
            }

            if (!string.IsNullOrWhiteSpace(assignment.Password))
            {
                bool isValidPassword = invitationInfo.HasMoreThanOneInvitation
                    ? invitationService.IsValidTokenAndPassword(id, password)
                    : assignment.Password.Equals(password, StringComparison.InvariantCultureIgnoreCase);
                
                if (!isValidPassword)
                {
                    var model = this.GetStartModel(assignment.QuestionnaireId, webInterviewConfig, assignment);
                    this.ModelState.AddModelError("InvalidPassword", "Wrong password");
                    return this.View(model);
                }
            }

            if (webInterviewConfig.UseCaptcha)
            {
                if (!this.captchaProvider.IsCaptchaValid(this))
                {
                    var model = this.GetStartModel(assignment.QuestionnaireId, webInterviewConfig, assignment);
                    this.ModelState.AddModelError("InvalidCaptcha", Enumerator.Native.Resources.WebInterview.PleaseFillCaptcha);
                    return this.View(model);
                }
            }

            if (invitationInfo.HasMoreThanOneInvitation)
            {
                invitation = invitationService.GetInvitationByTokenAndPassword(id, password);
                assignment = invitation.Assignment;
            }

            if (invitation?.InterviewId != null)
            {
                return this.Redirect(GenerateUrl("Cover", invitation.InterviewId ));
            }

            var interviewId =  this.CreateInterview(assignment);

            if (invitation != null && assignment.Quantity == 1)
            {
                invitationService.InterviewWasCreated(invitation.Id, interviewId);
            }

            HttpCookie interviewIdCookie = new HttpCookie($"InterviewId-{assignment.Id}");
            interviewIdCookie.Value = interviewId;
            interviewIdCookie.Expires = DateTime.Now.AddYears(1);
            Response.Cookies.Add(interviewIdCookie);

            RememberCapchaFilled(interviewId);
            if (!string.IsNullOrWhiteSpace(assignment.Password))
            {
                RememberPasswordVerified(interviewId);
            }
            TempData[LastCreatedInterviewIdKey] = interviewId;

            return this.Redirect(GenerateUrl("Cover", interviewId));
        }

        [WebInterviewAuthorize]
        public ActionResult Cover(string id)
        {
            var interview = this.statefulInterviewRepository.Get(id);
            var webInterviewConfig = this.configProvider.Get(interview.QuestionnaireIdentity);
            if (webInterviewConfig.UseCaptcha && !this.IsAuthorizedUser(interview.CurrentResponsibleId) && this.CapchaVerificationNeededForInterview(id))
            {
                var returnUrl = GenerateUrl(nameof(Cover), id);
                return this.RedirectToAction("Resume", routeValues: new { id = id, returnUrl = returnUrl });
            }

            LogResume(interview);
            ViewBag.AskForEmail = TempData[AskForEmail] ?? false;

            return View("Index");
        }

        private void LogResume(IStatefulInterview statefulInterview)
        {
            var lastCreatedInterview = TempData[LastCreatedInterviewIdKey] as string;
            if (lastCreatedInterview != statefulInterview.Id.FormatGuid())
            {
                this.pauseResumeQueue.EnqueueResume(new ResumeInterviewCommand(statefulInterview.Id,
                    statefulInterview.CurrentResponsibleId));
            }
        }

        public ActionResult Finish(string id)
        {
            var interview = this.statefulInterviewRepository.Get(id);
            if (interview == null || !interview.IsCompleted) return this.HttpNotFound();
            
            if (this.IsAuthorizedUser(interview.CurrentResponsibleId))
            {
                return RedirectToAction("Completed", "InterviewerHq");
            }

            var webInterviewConfig = this.configProvider.Get(interview.QuestionnaireIdentity);

            if (webInterviewConfig.UseCaptcha && !this.IsAuthorizedUser(interview.CurrentResponsibleId) && this.CapchaVerificationNeededForInterview(id))
            {
                var returnUrl = GenerateUrl(@"Finish", id);
                return this.RedirectToAction("Resume", routeValues: new { id = id, returnUrl = returnUrl });
            }

            var finishWebInterview = this.GetFinishModel(interview, webInterviewConfig);
            finishWebInterview.CustomMessages = webInterviewConfig.CustomMessages;

            return View(finishWebInterview);
        }

        [WebInterviewAuthorize]
        public ActionResult Resume(string id, string returnUrl)
        {
            var interview = this.statefulInterviewRepository.Get(id);
            var webInterviewConfig = this.configProvider.Get(interview.QuestionnaireIdentity);

            if (webInterviewConfig.UseCaptcha && !this.IsAuthorizedUser(interview.CurrentResponsibleId))
            {
                var model = this.GetResumeModel(id);
                return this.View("Resume", model);
            }

            RememberCapchaFilled(id);

            if (returnUrl == null)
            {
                return Redirect(GenerateUrl(@"Cover", id));
            }

            return Redirect(returnUrl);
        }

        public ActionResult Complete(string id)
        {
            var interview = this.statefulInterviewRepository.Get(id);
            var webInterviewConfig = this.configProvider.Get(interview.QuestionnaireIdentity);

            var isAuthorizedUser = this.IsAuthorizedUser(interview.CurrentResponsibleId);


            if (isAuthorizedUser && interview.Status == InterviewStatus.Completed)
            {
                return RedirectToAction("Completed", "InterviewerHq");
            }

            if (!isAuthorizedUser)
            {
                if (!webInterviewConfig.Started)
                    throw new InterviewAccessException(InterviewAccessExceptionReason.InterviewExpired, Enumerator.Native.Resources.WebInterview.Error_InterviewExpired);

                if(interview.Status == InterviewStatus.Completed)
                    throw new InterviewAccessException(InterviewAccessExceptionReason.NoActionsNeeded, Enumerator.Native.Resources.WebInterview.Error_NoActionsNeeded);
            }

            if (webInterviewConfig.UseCaptcha && this.CapchaVerificationNeededForInterview(id))
            {
                var returnUrl = GenerateUrl(@"Complete", id);
                return this.RedirectToAction("Resume", routeValues: new { id, returnUrl });
            }

            return View("Index");
        }

        [HttpPost]
        [ActionName("Resume")]
        [WebInterviewAuthorize]
        public ActionResult ResumePost(string id, string returnUrl)
        {
            if (!this.captchaProvider.IsCaptchaValid(this))
            {
                var model = this.GetResumeModel(id);
                this.ModelState.AddModelError("InvalidCaptcha", Enumerator.Native.Resources.WebInterview.PleaseFillCaptcha);
                return this.View(model);
            }

            RememberCapchaFilled(id);

            if (returnUrl == null)
            {
                return Redirect(GenerateUrl(@"Cover", id));
            }

            return this.Redirect(returnUrl);
        }

        public ActionResult OutdatedBrowser()
        {
            return View();
        }

        private string CreateInterview(Assignment assignment)
        {
            var webInterviewConfig = this.configProvider.Get(assignment.QuestionnaireId);
            if (!webInterviewConfig.Started)
                throw new InvalidOperationException(@"Web interview is not started for this questionnaire");

            var interviewer = this.usersRepository.GetUser(new UserViewInputModel(assignment.ResponsibleId));
            //if (!interviewer.IsInterviewer())
            //    throw new InvalidOperationException($"Assignment {assignment.Id} has responsible that is not an interviewer. Interview cannot be created");

            var interviewId = Guid.NewGuid();

            var createInterviewCommand = new CreateInterview(
                interviewId,
                interviewer.PublicKey,
                assignment.QuestionnaireId,
                assignment.Answers.ToList(),
                assignment.ProtectedVariables,
                interviewer.Supervisor?.Id ?? interviewer.PublicKey,
                interviewer.IsInterviewer() ? interviewer.PublicKey : (Guid?)null,
                this.keyGenerator.Get(),
                assignment.Id,
                assignment.IsAudioRecordingEnabled);

            this.commandService.Execute(createInterviewCommand);
            return interviewId.FormatGuid();
        }

        private ResumeWebInterview GetResumeModel(string id)
        {
            var interview = this.statefulInterviewRepository.Get(id);
            var questionnaireBrowseItem = this.questionnaireBrowseViewFactory.GetById(interview.QuestionnaireIdentity);

            if (questionnaireBrowseItem.IsDeleted)
            {
                throw new InterviewAccessException(InterviewAccessExceptionReason.InterviewExpired, Enumerator.Native.Resources.WebInterview.Error_InterviewExpired);
            }

            var webInterviewConfig = this.configProvider.Get(interview.QuestionnaireIdentity);

            return new ResumeWebInterview
            {
                QuestionnaireTitle = questionnaireBrowseItem.Title,
                UseCaptcha = this.webInterviewConfigProvider.Get(interview.QuestionnaireIdentity).UseCaptcha,
                StartedDate = interview.StartedDate,
                CustomMessages = webInterviewConfig.CustomMessages
            };
        }

        private StartWebInterview GetStartModel(
            QuestionnaireIdentity questionnaireIdentity,
            WebInterviewConfig webInterviewConfig, 
            Assignment assignment)
        {
            var questionnaireBrowseItem = this.questionnaireBrowseViewFactory.GetById(questionnaireIdentity);

            if (questionnaireBrowseItem.IsDeleted)
            {
                throw new InterviewAccessException(InterviewAccessExceptionReason.InterviewExpired, Enumerator.Native.Resources.WebInterview.Error_InterviewExpired);
            }

            var view = new StartWebInterview
            {
                QuestionnaireTitle = questionnaireBrowseItem.Title,
                UseCaptcha = webInterviewConfig.UseCaptcha,
                CustomMessages = webInterviewConfig.CustomMessages,
                HasPassword = !string.IsNullOrWhiteSpace(assignment?.Password ?? String.Empty)
            };

            return view;
        }

        private FinishWebInterview GetFinishModel(IStatefulInterview interview, WebInterviewConfig webInterviewConfig)
        {
            var questionnaireBrowseItem = this.questionnaireBrowseViewFactory.GetById(interview.QuestionnaireIdentity);

            if (questionnaireBrowseItem.IsDeleted)
            {
                throw new InterviewAccessException(InterviewAccessExceptionReason.InterviewExpired, Enumerator.Native.Resources.WebInterview.Error_InterviewExpired);
            }

            return new FinishWebInterview
            {
                QuestionnaireTitle = questionnaireBrowseItem.Title,
                StartedDate = interview.StartedDate,
                CompletedDate = interview.CompletedDate,
                CustomMessages = webInterviewConfig.CustomMessages
            };
        }

        private RedirectResult RedirectToFirstSection(string id, IStatefulInterview interview)
        {
            var sectionId = interview.GetAllEnabledGroupsAndRosters().First().Identity.ToString();
            var uri = GenerateUrl(@"Section", id, sectionId);

            return Redirect(uri);
        }

        private bool IsAuthorizedUser(Guid responsibleId)
        {
            if (User.Identity.IsAuthenticated)
            {
                var isInterviewer = this.User.IsInRole(UserRoles.Interviewer.ToString());

                if (isInterviewer)
                {
                    if (Guid.TryParse(this.User.Identity.GetUserId(), out Guid userid))
                    {
                        return responsibleId == userid;
                    }
                }
            }

            return false;
        }

        protected override void OnException(ExceptionContext filterContext)
        {
            var interviewException = filterContext.Exception.GetSelfOrInnerAs<InterviewException>();
            if (interviewException != null 
                && interviewException.ExceptionType != InterviewDomainExceptionType.Undefined)
            {
                string errorMessage = WebInterview.GetUiMessageFromException(interviewException);
                
                this.HandleInterviewAccessError(filterContext, errorMessage);
                return;
            }
            if (filterContext.Exception is HttpAntiForgeryException)
            {
                this.HandleInterviewAccessError(filterContext, Enumerator.Native.Resources.WebInterview.Error_CookiesTurnedOff);
                return;
            }

            if (filterContext.Exception is InterviewAccessException interviewAccessException)
            {
                if (interviewAccessException.Reason == InterviewAccessExceptionReason.UserNotAuthorised)
                {
                    filterContext.ExceptionHandled = true;
                    filterContext.HttpContext.Response.Clear();
                    filterContext.HttpContext.Response.StatusCode = 401;
                    filterContext.Result = new ContentResult
                    {
                        Content = "User is Not Authorised"
                    };

                    return;
                }

                if (interviewAccessException.Reason == InterviewAccessExceptionReason.Forbidden)
                {
                    filterContext.ExceptionHandled = true;
                    filterContext.HttpContext.Response.Clear();
                    filterContext.HttpContext.Response.StatusCode = 403;
                    filterContext.Result = new ContentResult
                    {
                        Content = "Forbidden"
                    };
                    return;
                }

                this.HandleInterviewAccessError(filterContext, interviewAccessException.Message);
                return;
            }

            filterContext.Exception.Log(System.Web.HttpContext.Current);
            this.HandleInDebugMode(filterContext);
        }

        [Conditional("DEBUG")]
        private void HandleInDebugMode(ExceptionContext filterContext)
        {
            filterContext.ExceptionHandled = true;

            filterContext.Result = new ContentResult
            {
                Content = $@"
<h1>No <b>Index.cshtml<b> found in ~Views\WebInterview folder.</h1>
<p>Index.cshtml is generated by 'WB.UI.Headquarters.Interview' application build. </p>
<p>Please navigate to WB.UI.Headquarters.Interview folder and run following commands that will install all nodejs deps and run dev server</p>
<pre>
npm install 
npm run dev
</pre>
Exception details:<br />
<pre>{filterContext.Exception}</pre>"
            };
        }


        private void HandleInterviewAccessError(ExceptionContext filterContext, string message)
        {
            filterContext.ExceptionHandled = true;
            filterContext.Result = new ViewResult
            {
                ViewName = @"~/Views/WebInterview/Error.cshtml",
                ViewData = new ViewDataDictionary(new WebInterviewError { Message = message })
            };
        }
    }
}
