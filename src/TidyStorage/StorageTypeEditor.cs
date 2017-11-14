using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TidyStorage
{
    public partial class StorageTypeEditor : Form
    {
        StorageTypeTables tabletype;
        Storage storage;

        bool ContentChanged = false;

        //Database delegates
        string column_id_name = "";
        string column_name_name = "";
        string table_name = "";
        
        const int IdColumnWidth = 60;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="tabletype">Table type to be edited</param>
        public StorageTypeEditor(Storage storage, StorageTypeTables tabletype)
        {
            InitializeComponent();
            this.tabletype = tabletype;
            this.storage = storage;

            switch (tabletype)
            {
                case StorageTypeTables.Manufacturer:
                    this.Text += " - Manufacturers";
                    table_name = StorageConst.Str_Manufacturer;
                    column_id_name = StorageConst.Str_Manufacturer_id;
                    column_name_name = StorageConst.Str_Manufacturer_name;
                    break;
                case StorageTypeTables.PartType:
                    this.Text += " - Part Types";
                    table_name = StorageConst.Str_PartType;
                    column_id_name = StorageConst.Str_PartType_id;
                    column_name_name = StorageConst.Str_PartType_name;
                    this.Width = 600;
                    break;
                case StorageTypeTables.Package:
                    this.Text += " - Packages";
                    table_name = StorageConst.Str_Package;
                    column_id_name = StorageConst.Str_Package_id;
                    column_name_name = StorageConst.Str_Package_name;
                    break;
                case StorageTypeTables.PlaceType:
                    this.Text += " - Storage Place Types";
                    table_name = StorageConst.Str_PlaceType;
                    column_id_name = StorageConst.Str_PlaceType_id;
                    column_name_name = StorageConst.Str_PlaceType_name;
                    break;
                case StorageTypeTables.Supplier:
                    this.Text += " - Suppliers";
                    table_name = StorageConst.Str_Supplier;
                    column_id_name = StorageConst.Str_Supplier_id;
                    column_name_name = StorageConst.Str_Supplier_name;
                    break;
                default:
                    break;
            }
        }
        
        /// <summary>
        /// Load type table content from the SQLite
        /// </summary>
        void LoadTable()
        {
            if (storage != null)
            {
                dataGridViewType.DataSource = storage.GetTable(table_name, "*", "1", column_name_name);

                if (dataGridViewType.Columns.Count > 0)
                {
                    dataGridViewType.Columns[0].Width = IdColumnWidth;
                    dataGridViewType.Columns[0].HeaderText = "ID";
                    dataGridViewType.Columns[0].ReadOnly = true;
                    dataGridViewType.Columns[0].Visible = false;

                    switch (tabletype)
                    {
                        case StorageTypeTables.Manufacturer:
                            dataGridViewType.Columns[1].HeaderText = "Manufacturer name";
                            break;

                        case StorageTypeTables.PartType:
                            dataGridViewType.Columns[1].HeaderText = "Part type name";
                            dataGridViewType.Columns[2].HeaderText = "Primary value";
                            dataGridViewType.Columns[3].HeaderText = "Secondary value";
                            dataGridViewType.Columns[4].HeaderText = "Third value";
                            break;

                        case StorageTypeTables.Package:
                            dataGridViewType.Columns[1].HeaderText = "Package name";
                            break;

                        case StorageTypeTables.PlaceType:
                            dataGridViewType.Columns[1].HeaderText = "Supplier name";
                            break;

                        case StorageTypeTables.Supplier:
                            dataGridViewType.Columns[1].HeaderText = "Supplier name";
                            dataGridViewType.Columns["read_only"].Visible = false;
                            dataGridViewType.Columns["read_only"].ReadOnly = true;
                            break;
                        default:
                            break;
                    }
                }

                ContentChanged = false;

                buttonApply.Enabled = false;
            }
            else
            {
                throw new Exception("No storage available");
            }
        }
        
        /// <summary>
        /// Handling OK button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonOk_Click(object sender, EventArgs e)
        {
            string SaveErrorMessage = "";

            if (storage.SaveTypeTable(table_name,column_id_name,(DataTable)dataGridViewType.DataSource, out SaveErrorMessage))
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                MessageBox.Show("Unable to save data: " + SaveErrorMessage, "Type editor error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            
        }
        
        /// <summary>
        /// Handling cancel button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        /// <summary>
        /// Storage part type editor OnLoad event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StoragePartTypeEditor_Load(object sender, EventArgs e)
        {
            LoadTable();
        }
        
        /// <summary>
        /// Data grid view row delete handler.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridViewType_UserDeletingRow(object sender, DataGridViewRowCancelEventArgs e)
        {
            if ((e.Row != null) && (e.Row.Cells.Count > 0))
            {
                int id = int.Parse(e.Row.Cells[0].Value.ToString());
                var o = e.Row.Cells[e.Row.Cells.Count - 1];

                //Check for readonly error. Read only is always last integer column
                if ((o.Visible == false) && (o.ValueType == typeof(Int64)))
                {
                    int read_only = (int)(Int64)o.Value;
                    if (read_only == 1)
                    {
                        e.Cancel = true;
                        MessageBox.Show("Readonly row cannot be deleted", "Row delete error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }

                if (storage.ColumnValueIsInUse(column_id_name, id))
                {
                    e.Cancel = true;
                    MessageBox.Show("Row cannot be deleted because is assigned to some parts.\r\nRemove this type from all parts first.", "Row delete error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    storage.DeleteRow(table_name, column_id_name, id);
                }
            }
        }

        /// <summary>
        /// Data grid view event after row is deleted by the user.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridViewType_UserDeletedRow(object sender, DataGridViewRowEventArgs e)
        {
            LoadTable();
        }

        /// <summary>
        /// Create New button click event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonCreateNew_Click(object sender, EventArgs e)
        {
            DialogResult dr = DialogResult.Yes;

            if (ContentChanged)
            {
                dr = MessageBox.Show("Changes was not confirmed by clicking on OK.\r\nAll changes will be lost. Do You want to continue?", "Creating New Row", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            }

            if (dr == DialogResult.Yes)
            {
                storage.InsertNewRow(table_name, column_name_name);
                LoadTable();

                //Scroll to bottom
                dataGridViewType.FirstDisplayedScrollingRowIndex = 0; //dataGridViewType.RowCount - 1;
            }
        }
        
        /// <summary>
        /// Data grid view cell value changed event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridViewType_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            ContentChanged = true;
            buttonApply.Enabled = true;
        }
                
        /// <summary>
        /// Form resize event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StoragePartTypeEditor_Resize(object sender, EventArgs e)
        {
            if (dataGridViewType.Columns.Count > 0)
            {
                dataGridViewType.Columns[0].Width = IdColumnWidth;
            }
        }

        /// <summary>
        /// Apply button click event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonApply_Click(object sender, EventArgs e)
        {
            buttonApply.Enabled = false;

            string SaveErrorMessage = "";

            if (storage.SaveTypeTable(table_name, column_id_name, (DataTable)dataGridViewType.DataSource, out SaveErrorMessage) == false)
            {
                MessageBox.Show("Unable to save data: " + SaveErrorMessage, "Type editor error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                //Save was not succesfull re-enable apply button
                buttonApply.Enabled = true;
            }
            else
            {
                ContentChanged = false;
            }
        }
    }
}
