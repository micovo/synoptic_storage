using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Data;
using System.Drawing;
using System.Threading.Tasks;
using System.ComponentModel;

namespace TidyStorage
{
    public partial class MainForm
    {
        /// <summary>
        /// Create new storage part from the string values
        /// </summary>
        /// <param name="PartName">Part name, usually a manifacture model name</param>
        /// <param name="SupplierName">Part supplier name such as Farnell or Mouser</param>
        /// <param name="SupplierNumber">Part number used by the part supplier</param>
        /// <param name="StoragePlace">Name of the place where the part is stored</param>
        /// <param name="Stock">Number of parts in stock</param>
        public void CreateNewStoragePart(string PartName = "", string SupplierName = "", string SupplierNumber = "", string StoragePlace = "", string Stock = "")
        {
            if (currentStorage != null)
            {
                dataGridViewStorage.Enabled = false;

                //Create new part and show it in storage part form
                StorageForm spf = new StorageForm(currentStorage, null, PartName, SupplierName, SupplierNumber, StoragePlace, Stock);
                spf.Show();
                spf.Center(this);

                while (spf.StorageClosed == false)
                {
                    Application.DoEvents();
                }
                dataGridViewStorage.Enabled = true;
            }
            else
            {
                DialogResult dr = MessageBox.Show("There is no storage. Do tou want to create new Storage?", "Cannot create new Storage part", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (dr == DialogResult.Yes)
                {
                    CreateNewStorage();
                }
            }

            //Refres storage data grid and filter lists
            RefreshStorageTable();
            RefreshListBox();
        }

        /// <summary>
        /// Create new storage part from the existing storage part
        /// </summary>
        /// <param name="sp">Source storage part</param>
        public void CreateNewStoragePart(StoragePart sp)
        {
            if (currentStorage != null)
            {
                dataGridViewStorage.Enabled = false;

                //Use copy constructor
                sp = new StoragePart(sp);

                //Open new part in storage part form
                StorageForm spf = new StorageForm(currentStorage, sp);
                spf.Show();
                spf.Center(this);

                while (spf.StorageClosed == false)
                {
                    Application.DoEvents();
                }
                dataGridViewStorage.Enabled = true;
            }
            else
            {
                DialogResult dr = MessageBox.Show("There is no storage. Do tou want to create new Storage?", "Cannot create new Storage part", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (dr == DialogResult.Yes)
                {
                    CreateNewStorage();
                }
            }

            //Refres storage data grid and filter lists
            RefreshStorageTable();
            RefreshListBox();
        }
        
        /// <summary>
        /// Function opens storage part in the storage part form
        /// </summary>
        /// <param name="part">Storage part to edit</param>
        private void EditStoragePart(StoragePart part)
        {
            if (currentStorage != null)
            {
                dataGridViewStorage.Enabled = false;

                StorageForm spf = new StorageForm(currentStorage, part);
                spf.Show();
                spf.Center(this);

                while (spf.StorageClosed == false)
                {
                    Application.DoEvents();
                }
                dataGridViewStorage.Enabled = true;
            }

            //Refres storage data grid and filter lists
            RefreshStorageTable();
            RefreshListBox();
        }
        
        /// <summary>
        /// Function creates new storage SQLite database
        /// </summary>
        private void CreateNewStorage()
        {
            //Check if there is some storage database openned right now
            if (StorageChangesProcedure() == DialogResult.Cancel)
            {
                return;
            }

            //Get new storage database filename
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "TidyStorage Database File|*.sqlite";
            sfd.SupportMultiDottedExtensions = true;
            sfd.CheckPathExists = true;
            sfd.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory;

            DialogResult sfd_dr = sfd.ShowDialog();

            if (sfd_dr == DialogResult.OK)
            {
                string filename = sfd.FileName;
                if (Path.GetExtension(filename) != ".sqlite")
                {
                    filename += ".sqlite";
                }
                
                //Create SQLite database
                currentStorage = new Storage(filename);
                
                //Add newly created storage to the list of recent storages
                AddRecentStorage(filename);

                //Refresh storage data grid and filter lists
                RefreshStorageTable();
                RefreshListBox();
            }
        }

        /// <summary>
        /// Checks current storage for changes, shows and process save storage dialog
        /// </summary>
        /// <returns>Save file dialog result</returns>
        private DialogResult StorageChangesProcedure()
        {
            DialogResult dr = DialogResult.No;

            if (currentStorage != null)
            {
                if (currentStorage.ChangeCommited)
                {
                    dr = MessageBox.Show("Changes in the storage was not saved. Do You want to save changes?", "Save storage", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

                    if (dr == DialogResult.Yes)
                    {
                        currentStorage.Save();
                    }
                }
            }

            return dr;
        }

        /// <summary>
        /// Storage database open dialog procedure
        /// </summary>
        private void OpenStorageFileRequest()
        {
            OpenFileDialog ofd = new OpenFileDialog();

            ofd.Multiselect = false;
            ofd.Filter = "TidyStorage Database File|*.sqlite";
            ofd.SupportMultiDottedExtensions = true;
            ofd.CheckPathExists = true;
            ofd.CheckFileExists = true;
            ofd.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory;

            DialogResult ofd_dr = ofd.ShowDialog();

            if (ofd_dr == DialogResult.OK)
            {
                if (StorageChangesProcedure() == DialogResult.Cancel)
                {
                    return;
                }

                OpenStorageFile(ofd.FileName);
            }
        }

        /// <summary>
        /// Function adds new filename to the list of recently used storages
        /// </summary>
        /// <param name="filename"></param>
        void AddRecentStorage(string filename)
        {
            //Remove file in case of already in the list
            RecentStorages.Remove(filename);
            //Add filename to the list
            RecentStorages.Insert(0, filename);
            //Confirm changes
            SaveRecentFilesList();
            LoadRecentFilesList();
        }
        
        /// <summary>
        /// Storage database file open procedure
        /// </summary>
        /// <param name="fileName"></param>
        private bool OpenStorageFile(string fileName)
        {
            if (File.Exists(fileName))
            {
                AddRecentStorage(fileName);
                currentStorage = new Storage(fileName);
                RefreshStorageTable();
                RefreshListBox();
            }
            else
            {
                MessageBox.Show("Unable to open Storage. File not found.", "Open Storage Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return false;
            }

            return true;
        }
        
        /// <summary>
        /// Storage close procedure. Storage is checked for changes before close.
        /// </summary>
        private void CloseStorage()
        {
            //Check if there are any changes to save
            if (StorageChangesProcedure() == DialogResult.Cancel)
            {
                return;
            }

            //Drop current storage
            currentStorage = null;

            //Refresh GUI to display empty datagrid view and filter lists
            RefreshStorageTable();
            RefreshListBox();
        }
        
        /// <summary>
        /// Upgrades Main Form text based on the selected tab and openned file
        /// </summary>
        private void UpdateMainFormText()
        {
            string text = "TidyStorage";

            int ti = tabControl1.SelectedIndex;

            //Use current storage database name or build date and version
            if ((ti >= 0) && (ti < TabFilename.Length) && (TabFilename[ti] != null) && (TabFilename[ti] != ""))
            {
                text += " - " + TabFilename[ti];
            }
            else
            {
                text += " " + Version + " (" + BuildDate + ")";
            }

            this.Text = text;
        }
        
        /// <summary>
        /// Synchronize application data with data grid view
        /// </summary>
        public void RefreshStorageTable()
        {
            int saveRow = 0;
            int selectedRow = 0;

            //Remember selected row
            if (dataGridViewStorage.Rows.Count > 0)
            {
                saveRow = dataGridViewStorage.FirstDisplayedCell.RowIndex;
                selectedRow = (dataGridViewStorage.SelectedCells.Count > 0) ? dataGridViewStorage.SelectedCells[0].RowIndex : -1;
            }

            //Remember sorting column and direction
            DataGridViewColumn oldColumn = dataGridViewStorage.SortedColumn;
            ListSortDirection direction = (dataGridViewStorage.SortOrder == SortOrder.Ascending) ? ListSortDirection.Ascending : ListSortDirection.Descending;

            bool curr = (currentStorage != null);
            if (curr)
            { 
                string filter = part_filter;

                if (part_filter_fulltext != "")
                {
                    filter += " AND ";
                    filter += part_filter_fulltext;
                }

                //Fill sotrage data gird view with storage database table
                dataGridViewStorage.DataSource = currentStorage.GetPartTable(filter);

                //Setup column headers
                dataGridViewStorage.Columns["id_part"].HeaderText = "ID";
                dataGridViewStorage.Columns["productnumber"].HeaderText = "Part name";
                dataGridViewStorage.Columns["productnumber"].DefaultCellStyle.Font = new Font(DataGridView.DefaultFont, FontStyle.Bold);

                dataGridViewStorage.Columns["manufacturername"].HeaderText = "Manufacturer";
                dataGridViewStorage.Columns["typename"].HeaderText = "Type";
                dataGridViewStorage.Columns["packagename"].HeaderText = "Package";
                dataGridViewStorage.Columns["stock"].HeaderText = "Stock";
                dataGridViewStorage.Columns["stock"].DefaultCellStyle.Font = new Font(DataGridView.DefaultFont, FontStyle.Bold);

                dataGridViewStorage.Columns["placename"].HeaderText = "Storage place";

                dataGridViewStorage.Columns["storage_place_number"].HeaderText = "Storage number";
                dataGridViewStorage.Columns["storage_place_number"].DefaultCellStyle.Font = new Font(DataGridView.DefaultFont, FontStyle.Bold);

                dataGridViewStorage.Columns["primary_value"].HeaderText = "Primary";
                dataGridViewStorage.Columns["primary_tolerance"].HeaderText = "Tolerance";
                dataGridViewStorage.Columns["secondary_value"].HeaderText = "Secondary";
                //dataGridViewStorage.Columns["secondary_tolerance"].HeaderText = "Tolerance";
                dataGridViewStorage.Columns["tertiary_value"].HeaderText = "Third";
                //dataGridViewStorage.Columns["tertiary_tolerance"].HeaderText = "Tolerance";
                dataGridViewStorage.Columns["temperature_from"].HeaderText = "Temp\r\nMIN";
                dataGridViewStorage.Columns["temperature_to"].HeaderText = "Temp\r\nMAX";
                dataGridViewStorage.Columns["suppliername"].HeaderText = "Supplier";
                dataGridViewStorage.Columns["suppliernumber"].HeaderText = "Supplier number";
                dataGridViewStorage.Columns["price_1pcs"].HeaderText = "Price per\r\n1";
                dataGridViewStorage.Columns["price_10pcs"].HeaderText = "Price per\r\n10";
                dataGridViewStorage.Columns["price_100pcs"].HeaderText = "Price per\r\n100";
                dataGridViewStorage.Columns["price_1000pcs"].HeaderText = "Part per\r\n1000";
                dataGridViewStorage.Columns["currency"].HeaderText = "";

                TabFilename[0] = Path.GetFileNameWithoutExtension(currentStorage.Filename);
            }
            else
            {
                //No storage database loaded, clean up data
                dataGridViewStorage.DataSource = null;
                textBoxStorageFulltext.Text = "";
                TabFilename[0] = "";
            }

            //Restore sorting column and direction
            if (oldColumn != null)
            {
                DataGridViewColumn newColumn = dataGridViewStorage.Columns[oldColumn.Name.ToString()];
                dataGridViewStorage.Sort(newColumn, direction);
                newColumn.HeaderCell.SortGlyphDirection = (direction == ListSortDirection.Ascending) ? SortOrder.Ascending : SortOrder.Descending;
            }

            //Restore scroll to the position of the previously selected row
            if (saveRow > 0 && saveRow < dataGridViewStorage.Rows.Count)
            {
                dataGridViewStorage.FirstDisplayedScrollingRowIndex = saveRow;
            }

            //Restore row selection
            dataGridViewStorage.ClearSelection();
            if (selectedRow > 0 && selectedRow < dataGridViewStorage.Rows.Count)
            {
                dataGridViewStorage.Rows[selectedRow].Selected = true;
            }
                        
            //Enable buttons that are enable only if storage file is available
            saveToolStripButton.Enabled = curr;
            saveToolStripMenuItem.Enabled = curr;
            saveAsToolStripMenuItem.Enabled = curr;
            closeToolStripMenuItem.Enabled = curr;
            storageToolStripMenuItem.Enabled = curr;
            storagePartsToolStripMenuItem.Enabled = curr;
            
            UpdateMainFormText();
        }
        
        /// <summary>
        /// Refreshes storage filter listboxes
        /// </summary>
        public void RefreshListBox()
        {
            if (currentStorage != null)
            {
                listBoxFilterType.DataSource = currentStorage.GetStringIdArray(StorageTypeTables.PartType);
                listBoxFilterPackage.DataSource = currentStorage.GetStringIdArray(StorageTypeTables.Package);
                listBoxStorageTypes.DataSource = currentStorage.GetStringIdArray(StorageTypeTables.PlaceType);

                listBoxFilterType.ClearSelected();
                listBoxFilterPackage.ClearSelected();
                listBoxStorageTypes.ClearSelected();
            }
            else
            {
                listBoxFilterType.DataSource = null;
                listBoxFilterPackage.DataSource = null;
                listBoxStorageTypes.DataSource = null;
            }
        }

        /// <summary>
        /// Storage "Save as" procedure. Procedure shwos and process "Save As" dialog.
        /// </summary>
        private void StorageSaveAs()
        {
            if (currentStorage != null)
            {
                SaveFileDialog sfd = new SaveFileDialog();

                sfd.Filter = "TidyStorage Database File|*.sqlite";
                sfd.SupportMultiDottedExtensions = true;
                sfd.CheckPathExists = true;
                sfd.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory;

                DialogResult sfd_dr = sfd.ShowDialog();

                if (sfd_dr == DialogResult.OK)
                {
                    string filename = sfd.FileName;
                    if (Path.GetExtension(filename) != ".sqlite")
                    {
                        filename += ".sqlite";
                    }
                    
                    currentStorage.Save(filename);
                    currentStorage = new Storage(filename);

                    AddRecentStorage(filename);
                }
            }

            //Refresh storage data grid and filter lists
            RefreshStorageTable();
            RefreshListBox();
        }

        /// <summary>
        /// Function creates list of parts selected in storage data grid view
        /// </summary>
        /// <returns>List of selected storage parts</returns>
        public List<StoragePart> StorageSelectionToList()
        {
            List<int> ids = new List<int>();

            List<StoragePart> output = new List<StoragePart>();

            //Return empty list if there is no storage yet
            if (currentStorage == null) return output;

            //Convert selected rows to the list of part IDs
            foreach (DataGridViewRow row in dataGridViewStorage.SelectedRows)
            {
                int id = (int)(Int64)row.Cells[StorageConst.Str_Part_id].Value;
                ids.Add(id);
            }

            //Create SQL command condition if any IDs was selected
            if (ids.Count > 0)
            {
                string filter = StorageConst.Str_Part_id + " IN (";
                foreach (int i in ids)
                {
                    filter += i.ToString() + ",";
                }
                
                filter = filter.Trim(',') + ")";
                DataTable tb = currentStorage.GetTable(StorageConst.Str_Part, "*", filter);

                int rowindex = 0;

                foreach (int i in ids)
                {
                    output.Add(new StoragePart(tb, rowindex++));
                }
            }
            
            return output;
        }

        /// <summary>
        /// Function creates list of all storage parts. Filter is applied if its set.    
        /// </summary>
        /// <returns>List of all or filtered storage parts</returns>
        public List<StoragePart> StorageTableToList()
        {
            List<StoragePart> output = new List<StoragePart>();

            //Return empty list if there is no storage yet
            if (currentStorage == null) return output;

            string filter = part_filter;

            if (part_filter_fulltext != "")
            {
                filter += " AND ";
                filter += part_filter_fulltext;
            }

            DataTable tb = currentStorage.GetTable(StorageConst.Str_Part, "*", filter);

            int rowindex = 0;
            for (int i = 0; i < tb.Rows.Count; i++)
            {
                output.Add(new StoragePart(tb, rowindex++));
            }
            return output;
        }

    }
}
