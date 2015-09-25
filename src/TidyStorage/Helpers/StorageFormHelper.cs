using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Drawing;
using System.Threading.Tasks;
using System.ComponentModel;

namespace TidyStorage
{
    public partial class MainForm
    {
        /// <summary>
        /// 
        /// </summary>
        public void CreateNewStoragePart(string PartName = "", string SupplierName = "", string SupplierNumber = "", string StoragePlace = "", string Stock = "")
        {
            if (currentStorage != null)
            {
                dataGridViewStorage.Enabled = false;

                StorageForm spf = new StorageForm(currentStorage, null, PartName, SupplierName, SupplierNumber, StoragePlace, Stock);
                spf.Show();
                spf.Center(this);

                while (spf.Closed == false)
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

            RefreshStorageTable();
            RefreshListBox();
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="part"></param>
        private void EditStoragePart(StoragePart part)
        {
            if (currentStorage != null)
            {
                dataGridViewStorage.Enabled = false;

                StorageForm spf = new StorageForm(currentStorage, part);
                spf.Show();
                spf.Center(this);

                while (spf.Closed == false)
                {
                    Application.DoEvents();
                }
                dataGridViewStorage.Enabled = true;

            }

            RefreshStorageTable();
            RefreshListBox();
        }





        /// <summary>
        /// 
        /// </summary>
        private void CreateNewStorage()
        {
            if (StorageChangesProcedure() == DialogResult.Cancel)
            {
                return;
            }

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

                currentStorage = new Storage(filename);


                AddRecentStorage(filename);

                RefreshStorageTable();
                RefreshListBox();
            }
        }




        /// <summary>
        /// Checks current storage for changes, Shows and process save storage dialog
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
        /// 
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
        /// 
        /// </summary>
        /// <param name="filename"></param>
        void AddRecentStorage(string filename)
        {
            RecentStorages.Remove(filename);
            RecentStorages.Insert(0, filename);
            SaveRecentFilesList();
            LoadRecentFilesList();
        }


        /// <summary>
        /// 
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
            if (StorageChangesProcedure() == DialogResult.Cancel)
            {
                return;
            }

            currentStorage = null;
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

            if ((ti >= 0) && (ti < TabFilename.Length))
            {
                var x = TabFilename[ti];
                if ((x != null) && (x != ""))
                {
                    text += " - " + x;
                }
            }

            this.Text = text;
        }


        /// <summary>
        /// 
        /// </summary>
        public void RefreshStorageTable()
        {
            int saveRow = 0;
            int selectedRow = 0;
            if (dataGridViewStorage.Rows.Count > 0)
            {
                saveRow = dataGridViewStorage.FirstDisplayedCell.RowIndex;
                selectedRow = (dataGridViewStorage.SelectedCells.Count > 0) ? dataGridViewStorage.SelectedCells[0].RowIndex : -1;
            }

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

                dataGridViewStorage.DataSource = currentStorage.GetPartTable(filter);

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
                dataGridViewStorage.DataSource = null;
                textBoxStorageFulltext.Text = "";
                TabFilename[0] = "";
            }

            if (oldColumn != null)
            {
                DataGridViewColumn newColumn = dataGridViewStorage.Columns[oldColumn.Name.ToString()];
                dataGridViewStorage.Sort(newColumn, direction);
                newColumn.HeaderCell.SortGlyphDirection = (direction == ListSortDirection.Ascending) ? SortOrder.Ascending : SortOrder.Descending;
            }

            if (saveRow > 0 && saveRow < dataGridViewStorage.Rows.Count)
            {
                dataGridViewStorage.FirstDisplayedScrollingRowIndex = saveRow;
            }


            dataGridViewStorage.ClearSelection();

            if (selectedRow > 0 && selectedRow < dataGridViewStorage.Rows.Count)
            {
                dataGridViewStorage.Rows[selectedRow].Selected = true;
            }


            

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

                listBoxFilterType.ClearSelected();
                listBoxFilterPackage.ClearSelected();
            }
            else
            {
                listBoxFilterType.DataSource = null;
                listBoxFilterPackage.DataSource = null;
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

            RefreshStorageTable();
            RefreshListBox();
        }




        /// <summary>
        /// 
        /// </summary>
        /// <param name="visible"></param>
        private void ChangeConsoleVisibility(bool visible)
        {
            if (visible)
            {
                groupBoxConsole.Visible = true;
                dataGridViewStorage.Height -= groupBoxConsole.Height + 12;
            }
            else
            {
                groupBoxConsole.Visible = false;
                dataGridViewStorage.Height += groupBoxConsole.Height + 12;
            }

        }


    }
}
