﻿using System.Web.Configuration;

namespace WB.UI.Headquarters.Code
{
    public static class LegacyOptions
    {
        public static bool SupervisorFunctionsEnabled
        {
            get { return bool.Parse(WebConfigurationManager.AppSettings["SupervisorFunctionsEnabled"]); }
        }

        public static bool InterviewDetailsDataSchedulerEnabled
        {
            get { return bool.Parse(WebConfigurationManager.AppSettings["InterviewDetailsDataScheduler.Enabled"]); }
        }

        public static int InterviewDetailsDataSchedulerSynchronizationInterval
        {
            get
            {
                return
                    int.Parse(
                        WebConfigurationManager.AppSettings["InterviewDetailsDataScheduler.SynchronizationInterval"]);
            }
        }

        public static int InterviewDetailsDataSchedulerNumberOfInterviewsProcessedAtTime
        {
            get
            {
                return
                    int.Parse(
                        WebConfigurationManager.AppSettings["InterviewDetailsDataScheduler.NumberOfInterviewsProcessedAtTime"]);
            }
        }

        public static string SynchronizationIncomingCapiPackagesDirectory
        {
            get
            {
                return WebConfigurationManager.AppSettings["Synchronization.IncomingCapiPackagesDirectory"];
            }
        }

        public static string SynchronizationIncomingCapiPackagesWithErrorsDirectory
        {
            get
            {
                return WebConfigurationManager.AppSettings["Synchronization.IncomingCapiPackagesWithErrorsDirectory"];
            }
        }

        public static string SynchronizationIncomingCapiPackageFileNameExtension
        {
            get
            {
                return WebConfigurationManager.AppSettings["Synchronization.IncomingCapiPackageFileNameExtension"];
            }
        }
    }
}