using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Process = System.Diagnostics.Process;
using Task = System.Threading.Tasks.Task;

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

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static async Task InitializeAsync(AsyncPackage package) {
            // Switch to the main thread - the call to AddCommand in ConvertProject's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new ConvertProject(package, commandService);
        }

        string _solutionDir;
        string workPath;
        bool isWaitConverter;

        public static string VersionsPath { get; set; }
        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private async void Execute(object sender, EventArgs e) {
            ConvertProjectPackage options = package as ConvertProjectPackage;
            workPath = options.XConverterFolderPath;
            isWaitConverter = options.IsWaitConverter;
            VersionsPath = workPath + @"\versions.xml";

            DTE dte = await package.GetServiceAsync(typeof(DTE)).ConfigureAwait(false) as DTE;
            if(dte.Solution.Count == 0) {
                System.Windows.Forms.MessageBox.Show("Solutions were not found", "Converter Runner");
                return;
            }

            ProjectType projectType = GetProjectType(dte.Solution.Projects.Item(1));
            if(projectType == ProjectType.VSProject) {
                string version = GetVSProjectVersion(dte.Solution.Projects.Item(1));
                if(!string.IsNullOrEmpty(dte.Solution.FullName)) {
                    _solutionDir = Path.GetDirectoryName(dte.Solution.FullName);
                } else {
                    _solutionDir = Path.GetDirectoryName(dte.Solution.Projects.Item(1).FullName);
                }

                VersionChooser form = new VersionChooser(_solutionDir, version);
                var wnd = new System.Windows.Window();
                wnd.Width = 460;
                wnd.SizeToContent = SizeToContent.Height;
                wnd.Content = form;
                wnd.Title = "ConvertProject";
                Application.Current.Dispatcher.Invoke((Action)delegate {
                    wnd.ShowDialog();
                    if(wnd.DialogResult == true) {
                        string st = string.Format("\"{0}\" \"{1}\" {2} \"{3}\"", _solutionDir, form.Version, isWaitConverter, form.InstalledVersionPath);
                        ProcessStartInfo startInfo = new ProcessStartInfo();
                        startInfo.FileName = Path.Combine(workPath, "DXConverter.exe");
                        startInfo.Arguments = st;
                        Process.Start(startInfo);
                    }
                });
            } else {
                VsShellUtilities.ShowMessageBox(
                this.package,
                "wrong project",
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
