using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

namespace DxConverterCommand {
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class ConvertProject {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 4131;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("f2bdbcad-9f8d-4ab2-8bd4-684a6f267c6d");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage package;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConvertProject"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private ConvertProject(AsyncPackage package, OleMenuCommandService commandService) {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new MenuCommand(this.Execute, menuCommandID);
            commandService.AddCommand(menuItem);
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
        private Microsoft.VisualStudio.Shell.IAsyncServiceProvider ServiceProvider {
            get {
                return this.package;
            }
        }

        public static string VersionsPath { get; internal set; }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static async Task InitializeAsync(AsyncPackage package, EnvDTE.Solution _sol) {
            // Switch to the main thread - the call to AddCommand in ConvertProject's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new ConvertProject(package, commandService);
            Instance.sol = _sol;
        }
        EnvDTE.Solution sol;

        string workPath;
        bool isWaitConverter;
        string _solutionDir;
        private void Execute(object sender, EventArgs e) {
            ConvertProjectPackage options = package as ConvertProjectPackage;
            workPath = options.XConverterFolderPath;
            isWaitConverter = options.IsWaitConverter;
            VersionsPath = workPath + @"\versions.xml";
            if(sol.Count == 0) {
                System.Windows.Forms.MessageBox.Show("Solutions were not found", "Converter Runner");
                return;
            }

            ProjectType projectType = GetProjectType(sol.Projects.Item(1));
            if(projectType == ProjectType.VSProject) {
                string version = GetVSProjectVersion(sol.Projects.Item(1));
                if(!string.IsNullOrEmpty(sol.FullName)) {
                    _solutionDir = Path.GetDirectoryName(sol.FullName);
                } else {
                    _solutionDir = Path.GetDirectoryName(sol.Projects.Item(1).FullName);
                }

                VersionChooser form = new VersionChooser(_solutionDir, version, options.CanUpdate);
                var wnd = new System.Windows.Window();
                wnd.Width = 460;
                wnd.SizeToContent = SizeToContent.Height;
                wnd.Content = form;
                wnd.Title = "ConvertProject";

                wnd.ShowDialog();
                if(wnd.DialogResult == true) {
                    string st = string.Format("\"{0}\" \"{1}\" {2} \"{3}\"", _solutionDir, form.Version, isWaitConverter, form.InstalledVersionPath);
                    ProcessStartInfo startInfo = new ProcessStartInfo();
                    startInfo.FileName = Path.Combine(workPath, "DXConverter.exe");
                    startInfo.Arguments = st;
                    Process.Start(startInfo);
                }
            } else {
                VsShellUtilities.ShowMessageBox(
                this.package,
                sol.FullName + "ttt",
                "wrong project",
                OLEMSGICON.OLEMSGICON_INFO,
                OLEMSGBUTTON.OLEMSGBUTTON_OK,
                OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
            }
        }
        string GetVSProjectVersion(EnvDTE.Project project) {
            var vsproject = project.Object as VSLangProj.VSProject;
            foreach(VSLangProj.Reference reference in vsproject.References) {
                if(reference.SourceProject == null && reference.Name.Contains("DevExpress")) {
                    return string.Format("{0}.{1}.{2}", reference.MajorVersion, reference.MinorVersion, reference.BuildNumber);
                }
            }
            return string.Empty;
        }
        ProjectType GetProjectType(EnvDTE.Project project) {
            if((project.Object as VSLangProj.VSProject) != null) {
                return ProjectType.VSProject;
            }
            //if((project.Object as VSLangProj.VSWebSite) != null) {
            //    return ProjectType.VSWebSite;
            //}
            return ProjectType.Unknown;
        }
    }

    enum ProjectType {
        VSProject,
        VSWebSite,
        Unknown
    }
}
