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
using System.Diagnostics;

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
        StorageImporter storageImporter;

        public string Version;
        public string BuildDate;

        /// <summary>
        /// Initialize GUI and get build date and assembly version
        /// </summary>
        public MainForm()
        {
            InitializeComponent();

            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            string version = fvi.FileVersion;

            string builddate = Properties.Resources.BuildDate;
            int i = builddate.IndexOf(' ');
            i = i < 0 ? 0 : i;
            builddate = builddate.Substring(i).Trim(); //Remove name of the day

            this.Version = version;
            this.BuildDate = builddate;
        }


        /// <summary>
        /// Initialize form maximalization state on load
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainForm_Load(object sender, EventArgs e)
        { 
            RefreshStorageTable();

            Rectangle rec = Screen.FromControl(this).Bounds;

            if ((rec.Width <= 1280) && (rec.Height <= 800))
            {
                this.WindowState = FormWindowState.Maximized;
            }

            LoadRecentFilesList();
        }
        
        /// <summary>
        /// Function creates nice short path string for Recent files menu
        /// </summary>
        /// <param name="filename">Full path to be shorten</param>
        /// <param name="maxlength">Max length of the output string</param>
        /// <returns>Input string if its shorter than maxlength or nicely shortened path string</returns>
        private string GetMenuItemText(string filename, int maxlength)
        {
            if (filename.Length > maxlength)
            {
                //Get folders in path
                string [] folders = filename.Split('\\').Reverse().ToArray();

                string output = "";

                //Drive name have to be always visible
                string root = Path.GetPathRoot(filename) + "...\\";

                //Check how many folder names can be added to the string
                foreach (string s in folders)
                {
                    if ((root.Length + output.Length + 1) < maxlength)
                    {
                        output = s + "\\" + output;
                    }
                    else
                    {
                        //No more folders can be added to the string
                        break;
                    }
                }
                
                //Trim extra backslash characters 
                output = root + output.Trim('\\');
                return output;
            }
            else
            {
                //Path is shorter than maxlength, return whole path
                return filename;
            }
        }
        
        /// <summary>
        /// Function stores list of recently used storage and device files into recent.ini file
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
        /// Function loads list of recently used storage and device files from recent.ini file
        /// </summary>
        void LoadRecentFilesList()
        {
            //Clear all recent lists
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
            
            //Sync Recent lists with dropdown menu
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
            //Check changes and cancel closing if necessary
            if (StorageChangesProcedure() == DialogResult.Cancel)
            {
                e.Cancel = true;
            }
        }

        


        /// <summary>
        /// Toolbox menu New button click handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void newToolStripButton_Click(object sender, EventArgs e)
        {
            if (tabControl1.SelectedIndex == 0)
            {
                //Create new storage part
                CreateNewStoragePart();
            }
            else if (tabControl1.SelectedIndex == 1)
            {
                //TODO New device
            }
        }
        


        /// <summary>
        /// Storage data grid view delete action handler
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
        /// Drop down menu New->Part click handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void partToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CreateNewStoragePart();
        }

        /// <summary>
        /// Drop down menu save button handler
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
        /// Toolbox save button handler
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
        /// Storage data grid view cell double click handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridViewStorage_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                dataGridViewStorage.Rows[e.RowIndex].Selected = true;
                int id = (int)(Int64)dataGridViewStorage.Rows[e.RowIndex].Cells[StorageConst.Str_Part_id].Value;
                string filter = StorageConst.Str_Part_id + "=" + id.ToString();
                DataTable tb = currentStorage.GetTable(StorageConst.Str_Part, "*", filter);
                StoragePart sp = new StoragePart(tb);
                EditStoragePart(sp);
            }
        }

        /// <summary>
        /// Storage filter button click handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonListFilter_Click(object sender, EventArgs e)
        {
            //Function creates conditions used in main storage SQL command in WHERE section

            //Filter part type (Resistor, Capacitor....)
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
            
            //Filter part package (SOIC, TSSOP.....)
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

            //Filter storage type (Cut tape, full reel....)
            s = listBoxStorageTypes.SelectedItems;
            if (s.Count > 0)
            {
                if (part_filter != "")
                {
                    part_filter += " AND ";
                }

                part_filter += StorageConst.Str_Part + "." + StorageConst.Str_PlaceType_id + " IN (";

                foreach (var ss in s)
                {
                    IndexedName i = (IndexedName)ss;
                    part_filter += i.id.ToString() + ",";
                }

                part_filter = part_filter.Trim(',') + ") ";
            }

            //Output uf this function is direcly use in WHERE condition
            //so we have to set path_filter variable to 1 (true) to deactivate filter.
            if (part_filter == "") part_filter = "1";

            //Refresh storage data grid view
            RefreshStorageTable();
        }


        /// <summary>
        /// Storage fulltext search clear button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonFulltextClear_Click(object sender, EventArgs e)
        {
            part_filter_fulltext = "";
            textBoxStorageFulltext.Text = "";
            RefreshStorageTable();
        }

        /// <summary>
        /// Storage fulltext search button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonFulltextFilter_Click(object sender, EventArgs e)
        {
            //Create condition for the storage SQL command
            string text = textBoxStorageFulltext.Text;
            part_filter_fulltext = "";

            //Add columns for fulltext search
            string[] columns = { "primary_value", "secondary_value", "tertiary_value", "comment", "productnumber" ,"suppliernumber"};
            string[] keywords = text.Split(' ');

            if (keywords.Count() > 0)
            {
                part_filter_fulltext = StringHelpers.LikeCondition(columns, keywords);
                RefreshStorageTable();
            }
        }

        /// <summary>
        /// Storage list filter clear button handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonListFilterClear_Click(object sender, EventArgs e)
        {
            listBoxFilterPackage.ClearSelected();
            listBoxFilterType.ClearSelected();
            listBoxStorageTypes.ClearSelected();

            part_filter = "1";
            RefreshStorageTable();
        }
        

        /// <summary>
        /// Drop down about button click handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutBox ab = new AboutBox();
            ab.ShowDialog();
        }

        /// <summary>
        /// Parts manufacturers editor drop down menu button click handler
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
        /// Part types editor drop down menu button click handler
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
        /// Part packages editor drop down menu button click handler
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
        /// Part storage places editor drop down menu button click handler
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
        /// File->Exit drop down menu button click handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// File->Import->Storage Part drop down menu button click handler
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
        /// Open->File Storage drop down menu button click handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void storageToolStripMenuItem3_Click(object sender, EventArgs e)
        {
            OpenStorageFileRequest();
        }


        /// <summary>
        /// Toolbox Open button click handler
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
        /// File->New->Storage drop down menu click handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void storageToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            CreateNewStorage();
        }

        /// <summary>
        /// File->Save As drop down menu click handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StorageSaveAs();
        }

        /// <summary>
        /// File->Close drop down menu click handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CloseStorage();
        }

        /// <summary>
        /// Main tab control index changed event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateMainFormText();
        }

        /// <summary>
        /// About->Visit TidyStorage GitHub
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tidyStorageGitHubToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(GitHubLink);
        }

        /// <summary>
        /// About->Visit TidyStorage Website
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tidyStorageWebsiteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(HomepageLink); 
        }



        /// <summary>
        /// Recent storages in File drop down menu item click
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

        /// <summary>
        /// Recent devices in File drop down menu item click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RecentDevicesMenu_Click(object sender, EventArgs e)
        {
            int i = recentFilesToolStripMenuItem.DropDownItems.IndexOf((ToolStripItem)sender);
            MessageBox.Show(RecentDevices[i]);
        }
        

        /// <summary>
        /// Storage->Get Parts Out of Stock drop down menu item click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void getPartsOutOfStockToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            listBoxFilterPackage.ClearSelected();
            listBoxFilterType.ClearSelected();
            listBoxStorageTypes.ClearSelected();

            part_filter = "stock = 0";
            part_filter_fulltext = "";
            RefreshStorageTable();
        }


        /// <summary>
        /// Part->Pricecheck Selected Parts drop down menu item click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pricecheckSelectedPartsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            List<StoragePart> list = StorageSelectionToList();
            storageImporter = new StorageImporter(currentStorage, this, list, true);
            storageImporter.Start();
        }

        /// <summary>
        /// Part->Pricecheck All Parts drop down menu item click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pricecheckAllPartsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            List<StoragePart> list = StorageTableToList();
            storageImporter = new StorageImporter(currentStorage, this, list, true);
            storageImporter.Start();
        }

        /// <summary>
        /// Part->Web Import Selected Parts drop down menu item click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void webImportSelectedPartsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            List<StoragePart> list = StorageSelectionToList();
            storageImporter = new StorageImporter(currentStorage, this, list, false);
            storageImporter.Start();
        }

        /// <summary>
        /// Part->Web Import All Parts drop down menu item click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void webImportAllPartsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            List<StoragePart> list = StorageTableToList();
            storageImporter = new StorageImporter(currentStorage, this, list, false);
            storageImporter.Start();
        }

        /// <summary>
        /// Edit->Select All
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void selectAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tabControl1.SelectedIndex == 0)
            {
                dataGridViewStorage.SelectAll();
            }
            else if (tabControl1.SelectedIndex == 1)
            {

            }
        }

        /// <summary>
        /// Part Types filter list mouse double click event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listBoxFilterType_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            buttonListFilter_Click(null, null);
        }

        /// <summary>
        /// Part Packages filter list mouse double click event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listBoxFilterPackage_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            buttonListFilter_Click(null, null);
        }

        /// <summary>
        /// Part->Copy Selected Part
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void copySelectedPartToolStripMenuItem_Click(object sender, EventArgs e)
        {
            List<StoragePart> list = StorageSelectionToList();

            if (list.Count > 0)
            {
                CreateNewStoragePart(list[0]);
            }
        }

        private void editSuppliersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (currentStorage != null)
            {
                StorageTypeEditor spte = new StorageTypeEditor(currentStorage, StorageTypeTables.Supplier);
                spte.ShowDialog();
            }
        }
    }
}
