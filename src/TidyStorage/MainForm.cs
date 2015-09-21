using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SQLite;
using System.IO;

namespace TidyStorage
{
    public partial class MainForm : Form
    {
        Storage currentStorage;

        string part_filter = "1";
        string part_filter_fulltext = "";

        string[] TabFilename = new string[3];


        const string GitHubLink = @"https://github.com/micovo/tidy_storage";
        const string HomepageLink = @"http://tidystorage.micovo.cz/";


        List<string> RecentStorages = new List<string>();
        List<string> RecentDevices = new List<string>();



        ExcelImporter excelImporter;

        /// <summary>
        /// 
        /// </summary>
        public MainForm()
        {
            InitializeComponent();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainForm_Load(object sender, EventArgs e)
        {
            showConsoleToolStripMenuItem.Checked = false; //Commits ChangeConsoleVisibility(false);
            RefreshStorageTable();

            Rectangle rec = Screen.FromControl(this).Bounds;

            if ((rec.Width <= 1280) && (rec.Height <= 800))
            {
                this.WindowState = FormWindowState.Maximized;
            }


            LoadRecentFilesList();
        }
        
        private string GetMenuItemText(string filename, int maxlength)
        {
            if (filename.Length > maxlength)
            {
                string [] folders = filename.Split('\\').Reverse().ToArray();

                string output = "";
                string root = Path.GetPathRoot(filename) + "...\\";

                foreach (string s in folders)
                {
                    if ((root.Length + output.Length + 1) < maxlength)
                    {
                        output = s + "\\" + output;
                    }
                    else
                    {
                        break;
                    }
                }

                output = root + output.Trim('\\');
                return output;
            }
            else
            {
                return filename;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        void SaveRecentFilesList()
        {
            using (StreamWriter sw = new StreamWriter("recent.ini"))
            {
                var x = RecentStorages;
                foreach (string s in x)
                {
                    sw.WriteLine("[STRG]" + s);
                }

                var y = RecentDevices;
                foreach (string s in y)
                {
                    sw.WriteLine("[DEVC]" + s);
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        void LoadRecentFilesList()
        {
            
            RecentStorages.Clear();
            RecentDevices.Clear();

            if (File.Exists("recent.ini"))
            {
                using (StreamReader sr = new StreamReader("recent.ini"))
                {
                    while (sr.EndOfStream == false)
                    {
                        string s = sr.ReadLine().Trim();
                        if (s.StartsWith("[STRG]"))
                        {
                            RecentStorages.Add(s.Substring(6));
                        }
                        else if (s.StartsWith("[DEVC]"))
                        {
                            RecentDevices.Add(s.Substring(6));
                        }
                    }
                }
            }
            

            for (int i = 0; i < 10; i++)
            {
                if (RecentStorages.ElementAtOrDefault(i) != null)
                {
                    string s = RecentStorages[i];
                    recentStoragesToolStripMenuItem.DropDownItems[i].Text = (i + 1).ToString() + " " + GetMenuItemText(s, 50);
                    recentStoragesToolStripMenuItem.DropDownItems[i].Visible = true;
                }
                else
                {
                    recentStoragesToolStripMenuItem.DropDownItems[i].Visible = false;
                }

                if (RecentDevices.ElementAtOrDefault(i) != null)
                {
                    string s = RecentDevices[i];
                    recentFilesToolStripMenuItem.DropDownItems[i].Text = (i + 1).ToString() + " " + GetMenuItemText(s, 50);
                    recentFilesToolStripMenuItem.DropDownItems[i].Visible = true;
                }
                else
                {
                    recentFilesToolStripMenuItem.DropDownItems[i].Visible = false;
                }
            }

            recentStoragesToolStripMenuItem.Enabled = (RecentStorages.Count > 0);
            recentFilesToolStripMenuItem.Enabled = (RecentDevices.Count > 0);
        }


        /// <summary>
        /// Main form closing processing, files are checked for unsaved changes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (StorageChangesProcedure() == DialogResult.Cancel)
            {
                e.Cancel = true;
            }
        }

        


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void newToolStripButton_Click(object sender, EventArgs e)
        {
            if (tabControl1.SelectedIndex == 0)
            {
                CreateNewStoragePart();
            }
            else if (tabControl1.SelectedIndex == 1)
            {
              
            }
        }
        


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridViewStorage_UserDeletingRow(object sender, DataGridViewRowCancelEventArgs e)
        {
            e.Cancel = false;

            if (e.Row != null)
            {
                currentStorage.DeleteRow(StorageConst.Str_Part, StorageConst.Str_Part_id, (int)(Int64)e.Row.Cells[0].Value);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void partToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CreateNewStoragePart();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (currentStorage != null)
            {
                currentStorage.Save();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void saveToolStripButton_Click(object sender, EventArgs e)
        {
            if (tabControl1.SelectedIndex == 0)
            {
                if (currentStorage != null)
                {
                    currentStorage.Save();
                }
            }
        }
        

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridViewStorage_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                int id = (int)(Int64)dataGridViewStorage.Rows[e.RowIndex].Cells[StorageConst.Str_Part_id].Value;
                string filter = StorageConst.Str_Part_id + "=" + id.ToString();
                DataTable tb = currentStorage.GetTable(StorageConst.Str_Part, "*", filter);
                StoragePart sp = new StoragePart(tb);
                EditStoragePart(sp);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonListFilter_Click(object sender, EventArgs e)
        {
            var s = listBoxFilterType.SelectedItems;

            part_filter = "";

            if (s.Count > 0)
            {
                part_filter += StorageConst.Str_Part + "." + StorageConst.Str_PartType_id + " IN (";

                foreach (var ss in s)
                {
                    IndexedName i = (IndexedName)ss;
                    part_filter += i.id.ToString() + ",";
                }

                part_filter = part_filter.Trim(',') + ") ";
            }

            s = listBoxFilterPackage.SelectedItems;
            

            if (s.Count > 0)
            {
                if (part_filter != "")
                {
                    part_filter += " AND ";
                }

                part_filter += StorageConst.Str_Part + "." + StorageConst.Str_Package_id + " IN (";

                foreach (var ss in s)
                {
                    IndexedName i = (IndexedName)ss;
                    part_filter += i.id.ToString() + ",";
                }

                part_filter = part_filter.Trim(',') + ") ";
            }

            if (part_filter == "") part_filter = "1";

            RefreshStorageTable();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonFulltextClear_Click(object sender, EventArgs e)
        {
            part_filter_fulltext = "";
            RefreshStorageTable();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonFulltextFilter_Click(object sender, EventArgs e)
        {
            string text = textBoxStorageFulltext.Text;
            part_filter_fulltext = "";

            string[] columns = { "primary_value", "secondary_value", "tertiary_value", "comment", "productnumber" ,"suppliernumber"};
            string[] keywords = text.Split(' ');

            if (keywords.Count() > 0)
            {
                part_filter_fulltext = StringHelpers.LikeCondition(columns, keywords);
                RefreshStorageTable();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonListFilterClear_Click(object sender, EventArgs e)
        {
            listBoxFilterPackage.ClearSelected();
            listBoxFilterType.ClearSelected();

            part_filter = "1";
            RefreshStorageTable();
        }
        

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutBox ab = new AboutBox();
            ab.ShowDialog();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void editManufacturersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (currentStorage != null)
            {
                StorageTypeEditor spte = new StorageTypeEditor(currentStorage, StorageTypeTables.Manufacturer);
                spte.ShowDialog();
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void editPartTypesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (currentStorage != null)
            {
                StorageTypeEditor spte = new StorageTypeEditor(currentStorage, StorageTypeTables.PartType);
                spte.ShowDialog();
                RefreshListBox();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void editPackagesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (currentStorage != null)
            {
                StorageTypeEditor spte = new StorageTypeEditor(currentStorage, StorageTypeTables.Package);
                spte.ShowDialog();
                RefreshListBox();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void editPlaceTypesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (currentStorage != null)
            {
                StorageTypeEditor spte = new StorageTypeEditor(currentStorage, StorageTypeTables.PlaceType);
                spte.ShowDialog();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void storagePartsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (currentStorage != null)
            {
                excelImporter = new ExcelImporter(currentStorage, this);
                excelImporter.Start();
            }
        }
        


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void storageToolStripMenuItem3_Click(object sender, EventArgs e)
        {
            OpenStorageFileRequest();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void openToolStripButton_Click(object sender, EventArgs e)
        {
            if (tabControl1.SelectedIndex == 0)
            {
                OpenStorageFileRequest();
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void storageToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            CreateNewStorage();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StorageSaveAs();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void showConsoleToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            ChangeConsoleVisibility(showConsoleToolStripMenuItem.Checked);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CloseStorage();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateMainFormText();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tidyStorageGitHubToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(GitHubLink);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tidyStorageWebsiteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(HomepageLink); 
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RecentStoragesMenu_Click(object sender, EventArgs e)
        {
            if (StorageChangesProcedure() == DialogResult.Cancel)
            {
                return;
            }

            int i = recentStoragesToolStripMenuItem.DropDownItems.IndexOf((ToolStripItem)sender);
            string s = RecentStorages[i];

            OpenStorageFile(s);
        }

        private void RecentDevicesMenu_Click(object sender, EventArgs e)
        {
            int i = recentFilesToolStripMenuItem.DropDownItems.IndexOf((ToolStripItem)sender);
            MessageBox.Show(RecentDevices[i]);
        }
    }
}
