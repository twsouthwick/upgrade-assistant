﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Microsoft.DotNet.UpgradeAssistant.DeprecatedApisAnalyzer.CodeFixes {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class CodeFixResources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal CodeFixResources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Microsoft.DotNet.UpgradeAssistant.DeprecatedApisAnalyzer.CodeFixes.CodeFixResourc" +
                            "es", typeof(CodeFixResources).Assembly);
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
        ///   Looks up a localized string similar to Add member to abstraction.
        /// </summary>
        internal static string AdapterAddMemberTitle {
            get {
                return ResourceManager.GetString("AdapterAddMemberTitle", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Call factory method for abstraction.
        /// </summary>
        internal static string AdapterCallFactoryTitle {
            get {
                return ResourceManager.GetString("AdapterCallFactoryTitle", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Create abstraction for definition.
        /// </summary>
        internal static string AdapterDefinitionTitle {
            get {
                return ResourceManager.GetString("AdapterDefinitionTitle", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Refactor usage to alternate API.
        /// </summary>
        internal static string AdapterRefactorTitle {
            get {
                return ResourceManager.GetString("AdapterRefactorTitle", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Redirect static member access to alternate member.
        /// </summary>
        internal static string AdapterStaticMemberTitle {
            get {
                return ResourceManager.GetString("AdapterStaticMemberTitle", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Add an adapter descriptor.
        /// </summary>
        internal static string AddAdapterDescriptorTitle {
            get {
                return ResourceManager.GetString("AddAdapterDescriptorTitle", resourceCulture);
            }
        }
    }
}
