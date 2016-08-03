//------------------------------------------------------------------------------
// <copyright file="ConvertProject.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.ComponentModel.Design;
using System.Globalization;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System.IO;
using System.Diagnostics;

namespace DxConverterCommand {
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class ConvertProject {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0100;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("456a20fb-202a-4b8e-961f-9676c2786eca");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly Package package;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConvertProject"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        EnvDTE.DTE dte;
        private ConvertProject(Package package, EnvDTE.DTE _dte) {
            if (package == null) {
                throw new ArgumentNullException("package");
            }

            this.package = package;
            this.dte = _dte;
            OleMenuCommandService commandService = this.ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (commandService != null) {
                var menuCommandID = new CommandID(CommandSet, CommandId);
                var menuItem = new MenuCommand(this.MenuItemCallback, menuCommandID);
                commandService.AddCommand(menuItem);
            }
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static ConvertProject Instance {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private IServiceProvider ServiceProvider {
            get {
                return this.package;
            }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>

        public static void Initialize(Package package, EnvDTE.DTE _dte) {


            Instance = new ConvertProject(package, _dte);
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>

        string _solutionDir;
        public const string workPath= @"c:\Dropbox\Deploy\DXConverterDeploy\";
        private void MenuItemCallback(object sender, EventArgs e) {
            string message = string.Format(CultureInfo.CurrentCulture, "Inside {0}.MenuItemCallback()", this.GetType().FullName);
            string title = "ConvertProject";



            if (dte.Solution.Count == 0) {
                System.Windows.Forms.MessageBox.Show("Solutions were not found", "Converter Runner");
                return;
            }
            ProjectType projectType = GetProjectType(dte.Solution.Projects.Item(1));
            if (projectType == ProjectType.VSProject) {
                string version = GetVSProjectVersion(dte.Solution.Projects.Item(1));
                if (!string.IsNullOrEmpty(dte.Solution.FullName)) {
                    _solutionDir = Path.GetDirectoryName(dte.Solution.FullName);
                }
                else {
                    _solutionDir = Path.GetDirectoryName(dte.Solution.Projects.Item(1).FullName);
                }
                using (VersionChooserForm form = new VersionChooserForm(_solutionDir, version)) {
                    form.ShowDialog();
                    if (form.DialogResult == System.Windows.Forms.DialogResult.OK) {
                        string st = string.Format("\"{0}\" \"{1}\"", _solutionDir, form.Version);
                        ProcessStartInfo startInfo = new ProcessStartInfo();
                        // startInfo.FileName = @"c:\Dropbox\Deploy\DXConverterDeploy\DXConverter.exe";
                        startInfo.FileName =Path.Combine(workPath, "DXConverter.exe");
                        startInfo.Arguments = st;
                        Process.Start(startInfo);
                    }
                }
            }
            else {
                VsShellUtilities.ShowMessageBox(this.ServiceProvider, "wrong project", null, OLEMSGICON.OLEMSGICON_INFO,
                    OLEMSGBUTTON.OLEMSGBUTTON_OK,
                OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
            }
            // Show a message box to prove we were here
            //VsShellUtilities.ShowMessageBox(
            //    this.ServiceProvider,
            //    message,
            //    title,
            //    OLEMSGICON.OLEMSGICON_INFO,
            //    OLEMSGBUTTON.OLEMSGBUTTON_OK,
            //    OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
        }
        string GetVSProjectVersion(EnvDTE.Project project) {
            var vsproject = project.Object as VSLangProj.VSProject;
            foreach (VSLangProj.Reference reference in vsproject.References) {
                if (reference.SourceProject == null && reference.Name.Contains("DevExpress")) {
                    return string.Format("{0}.{1}.{2}", reference.MajorVersion, reference.MinorVersion, reference.BuildNumber);
                }
            }
            return string.Empty;
        }
        ProjectType GetProjectType(EnvDTE.Project project) {
            if ((project.Object as VSLangProj.VSProject) != null) {
                return ProjectType.VSProject;
            }
            if ((project.Object as VsWebSite.VSWebSite) != null) {
                return ProjectType.VSWebSite;
            }
            return ProjectType.Unknown;
        }
    }
    enum ProjectType {
        VSProject,
        VSWebSite,
        Unknown
    }
}
