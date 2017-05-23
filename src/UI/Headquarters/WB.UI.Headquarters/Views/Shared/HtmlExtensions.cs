﻿using System;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using Resources;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.UI.Headquarters.Resources;

namespace ASP
{
    public static partial class HtmlExtensions
    {
        public static IHtmlString  MainMenuItem(this HtmlHelper html, string actionName, string controllerName, string linkText, MenuItem renderedPage)
        {
            var page = html.ViewBag.ActivePage ?? MenuItem.Logon;
            string isActive = page == renderedPage ? "active" : String.Empty;

            string liStartTag = $"<li class='{isActive}'>\r\n";
            MvcHtmlString part2 = html.ActionLink(linkText, actionName, controllerName, new {area = ""}, new { title = linkText });

            return new MvcHtmlString(liStartTag + part2 + "</li>");
        }

        public static IHtmlString ActivePage(this HtmlHelper html)
        {
            MenuItem page = html.ViewBag.ActivePage ?? MenuItem.Logon;
            return new MvcHtmlString(GetMenuItemTitle(page));
        }

        public static string QuestionnaireName(this HtmlHelper html, string name, long version)
        {
            return string.Format(Pages.QuestionnaireNameFormat, name, version);
        }

        public static string QuestionnaireNameVerstionFirst(this HtmlHelper html, string name, long version)
        {
            return string.Format(Pages.QuestionnaireNameVersionFirst, name, version);
        }

        public static MvcHtmlString HasErrorClassFor<TModel, TProperty>(
            this HtmlHelper<TModel> htmlHelper,
            Expression<Func<TModel, TProperty>> expression)
        {
            string expressionText = ExpressionHelper.GetExpressionText(expression);

            string htmlFieldPrefix = htmlHelper.ViewData.TemplateInfo.HtmlFieldPrefix;

            string fullyQualifiedName;

            if (htmlFieldPrefix.Length > 0)
            {
                fullyQualifiedName = string.Join(".", htmlFieldPrefix, expressionText);
            }
            else
            {
                fullyQualifiedName = expressionText;
            }

            bool isValid = htmlHelper.ViewData.ModelState.IsValidField(fullyQualifiedName);

            if (!isValid)
            {
                return MvcHtmlString.Create("has-error");
            }

            return MvcHtmlString.Empty;
        }

        private static string GetMenuItemTitle(MenuItem page)
        {
            switch (page)
            {
                case MenuItem.Surveys: return @MainMenu.SurveysAndStatuses;
                case MenuItem.ApiUsers: return @MainMenu.ApiUsers;
                case MenuItem.DataExport: return @MainMenu.DataExport;
                case MenuItem.Docs: return @MainMenu.Interviews;
                case MenuItem.Headquarters: return @MainMenu.Headquarters;
                case MenuItem.Interviewers: return @MainMenu.Interviewers;
                case MenuItem.InterviewsChart: return @MainMenu.CumulativeChart;
                case MenuItem.ManageAccount: return Strings.SurverManagement_MainMenu_ManageAccount;
                case MenuItem.MapReport: return @MainMenu.MapReport;
                case MenuItem.NumberOfCompletedInterviews: return @MainMenu.Quantity;
                case MenuItem.SpeedOfCompletingInterviews: return @MainMenu.Speed;
                case MenuItem.Observers: return @MainMenu.Observers;
                case MenuItem.Questionnaires: return @MainMenu.SurveySetup;
                case MenuItem.Settings: return Resources.Common.Settings;
                case MenuItem.Summary: return @MainMenu.TeamsAndStatuses;
                case MenuItem.Teams: return @MainMenu.Supervisors;
                case MenuItem.UserBatchUpload: return @MainMenu.UserBatchUpload;
                case MenuItem.Statuses: return @MainMenu.SurveysAndStatuses;
                case MenuItem.Logon: return "Logon";
                case MenuItem.Devices: return "Devices";
                case MenuItem.SyncLog: return "SyncLog";
                default: return String.Empty;
            }
        }
    }
}