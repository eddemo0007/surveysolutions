//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Resources {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option or rebuild the Visual Studio project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Web.Application.StronglyTypedResourceProxyBuilder", "16.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Roles {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Roles() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Resources.Roles", global::System.Reflection.Assembly.Load("App_GlobalResources"));
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
        ///   Looks up a localized string similar to Admin.
        /// </summary>
        internal static string Administrator {
            get {
                return ResourceManager.GetString("Administrator", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to User for application programming interface.
        /// </summary>
        internal static string ApiUser {
            get {
                return ResourceManager.GetString("ApiUser", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Headquarters.
        /// </summary>
        internal static string Headquarter {
            get {
                return ResourceManager.GetString("Headquarter", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Interviewer.
        /// </summary>
        internal static string Interviewer {
            get {
                return ResourceManager.GetString("Interviewer", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Observer.
        /// </summary>
        internal static string Observer {
            get {
                return ResourceManager.GetString("Observer", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Supervisor.
        /// </summary>
        internal static string Supervisor {
            get {
                return ResourceManager.GetString("Supervisor", resourceCulture);
            }
        }
    }
}
