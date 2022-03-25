using DxConverterCommand2022;
using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
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
    public partial class VersionChooser : UserControl, INotifyPropertyChanged {
        public VersionChooser() {
            InitializeComponent();
        }
        public string Version { get; set; }
        public VersionChooser(string _solutionDir, string _version,bool _canUpdate) {
            LoadVersionsForComboBox();
            CanUpdate = _canUpdate;
            DataContext = this;
            InitializeComponent();
            ComboBoxSelectedVersion = _version;

        }
        public bool CanUpdate { get; set; }
        List<string> versionList;
        public List<string> VersionList {
            get {
                return versionList;
            }
            set {
                versionList = value;
                RaisePropertyChanged("VersionList");
            }
        }
        List<XElement> installedVersionList;
        public List<XElement> InstalledVersionList {
            get {
                return installedVersionList;
            }
            set {
                installedVersionList = value;
                RaisePropertyChanged("InstalledVersionList");
            }
        }


        string comboBoxSelectedVersion;
        public string ComboBoxSelectedVersion {
            get {
                return comboBoxSelectedVersion;
            }
            set {
                comboBoxSelectedVersion = value;
                RaisePropertyChanged("ComboBoxSelectedVersion");
            }
        }

        public string InstalledVersionPath { get; set; }
        void LoadVersionsForComboBox() {
            var xDoc = XDocument.Load(ConvertCommand.VersionsPath);
            var allVersionElement = xDoc.Element("Versions").Element("AllVersions");
            VersionList = allVersionElement.Elements().Select(x => x.Attribute("Version").Value).ToList();
            InstalledVersionList = GetInstalledVersions();
        }

        public static List<XElement> GetVersions() {
            List<XElement> directories = new List<XElement>();
            try {
                var lst = Directory.GetDirectories(@"\\CORP\builds\codecentral\DXDlls\");
                foreach(string directory in lst) {
                    var pName = Path.GetFileName(directory);
                    var xEl = new XElement("Version");
                    xEl.Add(new XAttribute("Version", pName));
                    directories.Add(xEl);
                }
            }
            catch(Exception e) {
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
            var XVersion = bt.DataContext as XElement;

            Version = XVersion.Attribute("Version").Value;
            InstalledVersionPath = XVersion.Attribute("Path").Value;
            CloseWindow();

        }
        private void UpdateButton_Click_1(object sender, RoutedEventArgs e) {
            List<XElement> allVersions = GetVersions();
            List<XElement> installedVersion = GetInstalledVersions();


            XElement xAllVersion = new XElement("AllVersions");
            foreach(var xVer in allVersions)
                xAllVersion.Add(xVer);
            VersionList = allVersions.Select(x => x.Attribute("Version").Value).ToList();
            InstalledVersionList = installedVersion.ToList();
            XElement xInstalledVersion = new XElement("InstalledVersions");
            foreach(var xVer in installedVersion)
                xInstalledVersion.Add(xVer);

            XElement xVersions = new XElement("Versions");
            xVersions.Add(xAllVersion);
            xVersions.Add(xInstalledVersion);


            var xDoc = new XDocument();
            xDoc.Add(xVersions);
            xDoc.Save(ConvertCommand.VersionsPath);


            //StreamWriter sw = new StreamWriter(filePath, false);
            //sw.Write(versionsToString);
            //sw.Close();
        }
        List<XElement> GetInstalledVersions() {
            List<XElement> installedVersions = new List<XElement>();
            List<string> versions = GetRegistryVersions("SOFTWARE\\WOW6432Node\\DevExpress\\Components\\");
            const string projectUpgradeToolRelativePath = "Tools\\Components\\ProjectConverter-console.exe";
            foreach(string rootPath in versions) {
                var rootPath2 = Path.Combine(rootPath, projectUpgradeToolRelativePath);
                string libVersion = GetProjectUpgradeVersion(rootPath2);
                var xEl = new XElement("Version");
                xEl.Add(new XAttribute("Version", libVersion));
                xEl.Add(new XAttribute("Path", rootPath2));
                installedVersions.Add(xEl);
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
            var lst23 = Registry.LocalMachine.GetValueNames();
            var soft = Registry.LocalMachine.OpenSubKey("SOFTWARE\\WOW6432Node\\DevExpress\\Components");
            var names3 = soft.GetSubKeyNames();
            var regKey = Registry.LocalMachine.OpenSubKey(path);
            if(regKey == null)
                return new List<string>();
            var lst = regKey.GetSubKeyNames();
            List<string> resList = new List<string>();
            foreach(string st in lst) {
                RegistryKey dxVersionKey = regKey.OpenSubKey(st);
                string projectUpgradeToolPath = dxVersionKey.GetValue("RootDirectory") as string;
                resList.Add(projectUpgradeToolPath);
            }
            return resList;
        }
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged(String propertyName) {
            if(PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

    }

    public class VersionComparer : IComparer<XElement> {


        public int Compare(XElement xLx, XElement xLy) {
            var x = xLx.Attribute("Version").Value;
            var y = xLy.Attribute("Version").Value;
            int counter = 0, res = 0;
            while(counter < 3 && res == 0) {
                int versionX = Convert.ToInt32(x.Split('.')[counter]);
                int versionY = Convert.ToInt32(y.Split('.')[counter]);
                res = Comparer.Default.Compare(versionX, versionY);
                counter++;
            }
            return -res;
        }
    }
}
