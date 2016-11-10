using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Xml.Linq;

namespace DxConverterCommand {
    /// <summary>
    /// Interaction logic for VersionChooser.xaml
    /// </summary>
    public partial class VersionChooser : UserControl {
        public VersionChooser() {
            InitializeComponent();
        }
        public string Version { get; set; }
        public VersionChooser(string _solutionDir, string _version) {

            // tbSolutionPath.Text = _solutionDir;
            //  Version = _version;
            //btnConvert.Click += BtnConvert_Click;
            //btnUpdate.Click += BtnUpdate_Click;

            LoadVersionsForComboBox();
            DataContext = this;
            InitializeComponent();
        }
        public List<string> VersionList { get; set; }
        public List<string> InstalledVersionList { get; set; }
        public string ComboBoxSelectedVersion { get; set; }
        void LoadVersionsForComboBox() {
            string filePath = Path.Combine(ConvertProject.workPath, "versions.xml");
            var xDoc = XDocument.Load(filePath);
            var allVersionsString = xDoc.Element("Versions").Element("AllVersions").Value;
            var allVersionsList = allVersionsString.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
            VersionList = allVersionsList.ToList();

            var installedString = xDoc.Element("Versions").Element("InstalledVersions").Value;
            var installedList = installedString.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
            InstalledVersionList = installedList.ToList();

        }

        public static List<string> GetVersions() {
            List<string> directories = new List<string>();
            try {
                foreach (string directory in Directory.GetDirectories(@"\\CORP\builds\release\DXDlls\"))
                    directories.Add(Path.GetFileName(directory));
            }
            catch (Exception e) {
                MessageBox.Show(e.Message);
            }
            directories.Sort(new VersionComparer());
            return directories;
        }

        private void ConvertButton_Click(object sender, RoutedEventArgs e) {
            Version = ComboBoxSelectedVersion;
            CloseWindow();
        }

        private void CloseWindow() {
            (this.Parent as Window).DialogResult = true;
            (this.Parent as Window).Close();
        }

        private void InstalledButton_Click(object sender, RoutedEventArgs e) {
            var bt = sender as Button;
            Version = bt.Content.ToString();
            CloseWindow();

        }
        private void UpdateButton_Click_1(object sender, RoutedEventArgs e) {
            string filePath = Path.Combine(ConvertProject.workPath, "versions.txt");
            List<string> versions = GetVersions();
            List<string> installedVersion = GetInstalledVersions();
            string versionsToString = string.Join("\n", versions);
            string installedVersionsToString = string.Join("\n", installedVersion);
            
            XElement xAllVersion = new XElement("AllVersions");
            xAllVersion.Value = versionsToString;

            XElement xInstalledVersion = new XElement("InstalledVersions");
            xInstalledVersion.Value = installedVersionsToString;

            XElement xVersions = new XElement("Versions");
            xVersions.Add(xAllVersion);
            xVersions.Add(xInstalledVersion);

            
            var xDoc = new XDocument();
            xDoc.Add(xVersions);
            string fileXDocPath = Path.Combine(ConvertProject.workPath, "versions.xml");
            xDoc.Save(fileXDocPath);


            //StreamWriter sw = new StreamWriter(filePath, false);
            //sw.Write(versionsToString);
            //sw.Close();
        }
        List<string> GetInstalledVersions() {
            List<string> installedVersions = new List<string>();
            List<string> versions = GetRegistryVersions("SOFTWARE\\DevExpress\\Components\\");
            const string projectUpgradeToolRelativePath = "Tools\\Components\\ProjectConverter-console.exe";
            foreach (string rootPath in versions) {
                var rootPath2 = Path.Combine(rootPath, projectUpgradeToolRelativePath);
                string libVersion = GetProjectUpgradeVersion(rootPath2);
                installedVersions.Add(libVersion);
            }
            installedVersions.Sort(new VersionComparer());
            return installedVersions;
        }
        string GetProjectUpgradeVersion(string projectUpgradeToolPath) {//5.1 td
            var assembly = Assembly.LoadFile(projectUpgradeToolPath);
            string assemblyFullName = assembly.FullName;
            string versionAssemblypattern = @"version=(?<Version>\d+\.\d.\d+)";
            Regex regexVersion = new Regex(versionAssemblypattern, RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
            Match versionMatch = regexVersion.Match(assemblyFullName);
            string versValue = versionMatch.Groups["Version"].Value;
            return versValue;
        }
        public List<string> GetRegistryVersions(string path) {
            var regKey = Registry.LocalMachine.OpenSubKey(path);
            var lst = regKey.GetSubKeyNames();
            List<string> resList = new List<string>();
            foreach (string st in lst) {
                RegistryKey dxVersionKey = regKey.OpenSubKey(st);
                string projectUpgradeToolPath = dxVersionKey.GetValue("RootDirectory") as string;
                resList.Add(projectUpgradeToolPath);
            }
            return resList;
        }

    }

    public class VersionComparer : IComparer<string> {

        public int Compare(string x, string y) {
            int counter = 0, res = 0;
            while (counter < 3 && res == 0) {
                int versionX = Convert.ToInt32(x.Split('.')[counter].Replace("_new", ""));
                int versionY = Convert.ToInt32(y.Split('.')[counter]);
                res = Comparer.Default.Compare(versionX, versionY);
                counter++;
            }
            return -res;
        }
    }
}
