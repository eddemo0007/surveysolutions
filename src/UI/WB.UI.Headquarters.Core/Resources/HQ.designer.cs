﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace WB.UI.Headquarters.Resources {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "16.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class HQ {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal HQ() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("WB.UI.Headquarters.Resources.HQ", typeof(HQ).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Create one more?.
        /// </summary>
        public static string CreateOneMore {
            get {
                return ResourceManager.GetString("CreateOneMore", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Interview was successfully created..
        /// </summary>
        public static string InterviewWasCreated {
            get {
                return ResourceManager.GetString("InterviewWasCreated", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Could not update user information because you don&apos;t have permission to perform this operation..
        /// </summary>
        public static string NoPermission {
            get {
                return ResourceManager.GetString("NoPermission", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Observer user &lt;b&gt;{0}&lt;/b&gt; was successfully created..
        /// </summary>
        public static string ObserverCreatedFormat {
            get {
                return ResourceManager.GetString("ObserverCreatedFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Questionnaire &apos;{0}&apos; was successfully cloned with a new title &apos;{1}&apos;..
        /// </summary>
        public static string QuestionnaireClonedAndRenamedFormat {
            get {
                return ResourceManager.GetString("QuestionnaireClonedAndRenamedFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Questionnaire &apos;{0}&apos; was successfully cloned..
        /// </summary>
        public static string QuestionnaireClonedFormat {
            get {
                return ResourceManager.GetString("QuestionnaireClonedFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Questionnaire with id {0} and version {1} cannot be found..
        /// </summary>
        public static string QuestionnaireNotFoundFormat {
            get {
                return ResourceManager.GetString("QuestionnaireNotFoundFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Supervisor &lt;b&gt;{0}&lt;/b&gt; was successfully created..
        /// </summary>
        public static string SuccessfullyCreatedFormat {
            get {
                return ResourceManager.GetString("SuccessfullyCreatedFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Could create interviewer. Selected supervior do not exist.
        /// </summary>
        public static string SupervisorNotFound {
            get {
                return ResourceManager.GetString("SupervisorNotFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Could not update user information because user does not exist.
        /// </summary>
        public static string UserNotExists {
            get {
                return ResourceManager.GetString("UserNotExists", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Headquarters user {0} was successfully created..
        /// </summary>
        public static string UserWasCreatedFormat {
            get {
                return ResourceManager.GetString("UserWasCreatedFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Information about &lt;b&gt;{0}&lt;/b&gt; successfully updated.
        /// </summary>
        public static string UserWasUpdatedFormat {
            get {
                return ResourceManager.GetString("UserWasUpdatedFormat", resourceCulture);
            }
        }
    }
}