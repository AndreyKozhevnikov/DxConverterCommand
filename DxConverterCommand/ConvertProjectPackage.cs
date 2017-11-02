//------------------------------------------------------------------------------
// <copyright file="ConvertProjectPackage.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.Win32;
using System.ComponentModel;

namespace DxConverterCommand {
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the
    /// IVsPackage interface and uses the registration attributes defined in the framework to
    /// register itself and its components with the shell. These attributes tell the pkgdef creation
    /// utility what data to put into .pkgdef file.
    /// </para>
    /// <para>
    /// To get loaded into VS, the package must be referred by &lt;Asset Type="Microsoft.VisualStudio.VsPackage" ...&gt; in .vsixmanifest file.
    /// </para>
    /// </remarks>
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)] // Info on this package for Help/About
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(ConvertProjectPackage.PackageGuidString)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    public sealed class ConvertProjectPackage : Package {
        /// <summary>
        /// ConvertProjectPackage GUID string.
        /// </summary>
        
        public const string PackageGuidString = "c616fdff-2a99-4e89-8c78-530b7e1ef305";

        /// <summary>
        /// Initializes a new instance of the <see cref="ConvertProject"/> class.
        /// </summary>
        public ConvertProjectPackage() {
            // Inside this method you can place any initialization code that does not require
            // any Visual Studio service because at this point the package object is created but
            // not sited yet inside Visual Studio environment. The place to do all the other
            // initialization is the Initialize method.
        }

        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize() {
            EnvDTE.DTE _dte = (EnvDTE.DTE)GetService(typeof(EnvDTE.DTE));
            //if (_dte.Solution.Count == 0) {
            //    System.Windows.Forms.MessageBox.Show("Solutions were not found", "Converter Runner");
            //    return;
            //}
            _dte.Solution.SolutionBuild.SolutionConfigurations.Item(2).Activate();
            ConvertProject.Initialize(this,_dte);
            base.Initialize();
        }
        //public string ReferencePath {
        //    get {
        //        OptionPageGrid page = (OptionPageGrid)GetDialogPage(typeof(OptionPageGrid));
        //        return page.ReferencePath;
        //    }
        //}
        #endregion
    }

    //public class OptionPageGrid : DialogPage {
    //    private string refPath = @"C:\{version}\XAF\";
    //    [Category("Project Converter")]
    //    [DisplayName("Reference Path")]
    //    [Description("Xaf Reference Path")]
    //    public string ReferencePath {
    //        get { return refPath; }
    //        set { refPath = value; }
    //    }
    //}
}
