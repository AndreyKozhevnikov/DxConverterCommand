using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;


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
        public string ComboBoxSelectedVersion { get; set; }
        void LoadVersionsForComboBox() {
            string filePath = Path.Combine(ConvertProject.workPath, "versions.txt");
            StreamReader sr = new StreamReader(filePath);
            var stringLst = sr.ReadToEnd();
            sr.Close();
            var lst = stringLst.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            VersionList = lst.ToList();
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

        private void Button_Click(object sender, RoutedEventArgs e) {
            Version = ComboBoxSelectedVersion;
            (this.Parent as Window).DialogResult = true;
            (this.Parent as Window).Close();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e) {
            string filePath = Path.Combine(ConvertProject.workPath, "versions.txt");
            List<string> versions = GetVersions();
            string versionsToString = string.Join("\r\n", versions);
            StreamWriter sw = new StreamWriter(filePath, false);
            sw.Write(versionsToString);
            sw.Close();
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
