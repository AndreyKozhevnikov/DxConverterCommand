using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DxConverterCommand {
    public partial class VersionChooserForm : Form {
   
        public string Version { get; set; }
        public VersionChooserForm() {
            InitializeComponent();
            ProvideVersions();
        }

        private void ProvideVersions() {

        }

        public VersionChooserForm(string _solutionDir, string _version) {
            InitializeComponent();
            tbSolutionPath.Text = _solutionDir;
            Version = _version;
            this.Shown += VersionChooserForm_Shown;
            btnConvert.Click += BtnConvert_Click;
            btnUpdate.Click += BtnUpdate_Click;

            LoadVersionsForComboBox();

        }

        void LoadVersionsForComboBox() {
            string filePath = Path.Combine(ConvertProject.workPath, "versions.txt");
            StreamReader sr = new StreamReader(filePath);
            var stringLst = sr.ReadToEnd();
            sr.Close();
            var lst = stringLst.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            cbVersion.DataSource = lst;
        }

        private void BtnUpdate_Click(object sender, EventArgs e) {
            string filePath = Path.Combine(ConvertProject.workPath, "versions.txt");
            List<string> versions = GetVersions();
            string versionsToString = string.Join("\r\n", versions);
            StreamWriter sw = new StreamWriter(filePath, false);
            sw.Write(versionsToString);
            sw.Close();
            cbVersion.DataSource = versions;
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
        private void BtnConvert_Click(object sender, EventArgs e) {
            Version = cbVersion.Text;
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
        }

        private void VersionChooserForm_Shown(object sender, EventArgs e) {
            this.Text = string.Format("{0} - Project version: {1}", this.Text, Version);
            cbVersion.Text = Version;
            cbVersion.SelectAll();
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
