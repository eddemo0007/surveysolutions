﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.34209
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace WB.Core.BoundedContexts.Headquarters.Resources {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class UserPreloadingServiceMessages {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal UserPreloadingServiceMessages() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("WB.Core.BoundedContexts.Headquarters.Resources.UserPreloadingServiceMessages", typeof(UserPreloadingServiceMessages).Assembly);
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
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to file &apos;{0}&apos; contains following columns which can&apos;t be mapped on any existing user&apos;s property: {1}.
        /// </summary>
        internal static string FileColumnsCantBeMappedFormat {
            get {
                return ResourceManager.GetString("FileColumnsCantBeMappedFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The dataset contains {0} records, but max allowed record number is {1}.
        /// </summary>
        internal static string TheDatasetMaxRecordNumberReachedFormat {
            get {
                return ResourceManager.GetString("TheDatasetMaxRecordNumberReachedFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to user preloading process with id &apos;{0}&apos; can&apos;t be finished because only {1} created out of {2}.
        /// </summary>
        internal static string userPreloadingProcessCantBeFinishedFormat {
            get {
                return ResourceManager.GetString("userPreloadingProcessCantBeFinishedFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to user preloading process with id &apos;{0}&apos;has {1} error(s)..
        /// </summary>
        internal static string UserPreloadingProcessWithIdHasErrorsFormat {
            get {
                return ResourceManager.GetString("UserPreloadingProcessWithIdHasErrorsFormat", resourceCulture);
            }
        }
    }
}
