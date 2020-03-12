using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

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
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [Guid(ConvertProjectPackage.PackageGuidString)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideOptionPage(typeof(OptionPageGrid), "XConverter", "Project XConverter", 0, 0, true)]

    public sealed class ConvertProjectPackage : AsyncPackage {
        /// <summary>
        /// DxConverterCommandPackage GUID string.
        /// </summary>
        public const string PackageGuidString = "cc878b65-8a2b-4fd9-915c-43362aec54c4";

        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token to monitor for initialization cancellation, which can occur when VS is shutting down.</param>
        /// <param name="progress">A provider for progress updates.</param>
        /// <returns>A task representing the async work of package initialization, or an already completed task if there is none. Do not return null from this method.</returns>
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress) {
            // When initialized asynchronously, the current thread may be a background thread at this point.
            // Do any initialization that requires the UI thread after switching to the UI thread.
            await this.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            var dte = await GetServiceAsync(typeof(EnvDTE.DTE)) as EnvDTE.DTE;
            var sol = dte.Solution;
            await ConvertProject.InitializeAsync(this, sol);
        }

        public string XConverterFolderPath {
            get {
                OptionPageGrid page = (OptionPageGrid)GetDialogPage(typeof(OptionPageGrid));
                var st = page.XConverterPath;
                return st;
            }
        }
        public bool IsWaitConverter {
            get {
                OptionPageGrid page = (OptionPageGrid)GetDialogPage(typeof(OptionPageGrid));
                var isClose = page.WaitConverterOnFinish;
                return isClose;
            }
        }

        public bool CanUpdate {
            get {
                OptionPageGrid page = (OptionPageGrid)GetDialogPage(typeof(OptionPageGrid));
                var isClose = page.CanUpdate;
                return isClose;
            }
        }
        public bool UseLocalCache {
            get {
                OptionPageGrid page = (OptionPageGrid)GetDialogPage(typeof(OptionPageGrid));
                var useLocalCache = page.UseLocalCache;
                return useLocalCache;
            }
        }
        #endregion
    }
    public class OptionPageGrid : DialogPage {
        private string xConverterPath = @"\\corp\internal\common\4Kozhevnikov\Deploy\DXConverterDeploy\";
        [Category("XConverterPath")]
        [DisplayName("XConverterPath")]
        [Description("Path to DXConverter folder")]
        public string XConverterPath {
            get { return xConverterPath; }
            set { xConverterPath = value; }
        }

        bool waitToExit = false;
        [Category("XConverterPath")]
        [DisplayName("Wait at the end")]
        [Description("Whether to close a converter console at the end")]
        public bool WaitConverterOnFinish {
            get { return waitToExit; }
            set { waitToExit = value; }
        }
        
        bool _canUpdate = false;
        [Category("XConverterPath")]
        [DisplayName("CanUpdate")]
        [Description("CanUpdate")]
        public bool CanUpdate {
            get { return _canUpdate; }
            set { _canUpdate = value; }
        }
        bool _useLocalCache = false;
        [Category("XConverterPath")]
        [DisplayName("UseLocalCache")]
        [Description("UseLocalCache")]
        public bool UseLocalCache {
            get { return _useLocalCache; }
            set { _useLocalCache = value; }
        }
    }
}
