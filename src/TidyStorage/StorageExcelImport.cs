using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using OfficeOpenXml;

namespace TidyStorage
{
    public partial class StorageExcelImport : Form
    {
        Storage storage;

        string excelFilename;
        public string ExcelFilename
        {
            get
            {
                return excelFilename;
            }
        }

        ExcelPackage excelPackage;

        const string Str_Stock = "Stock";
        const string Str_StoragePlace = "Storage Place";
        const string Str_SupplierNumber = "Supplier Number";
        const string Str_SupplierName = "Supplier Name";
        const string Str_PartName = "Part Name";
        
        string selectedColumn;
        
        bool SupplierNameColumnSelected;
        bool SupplierNumberColumnSelected;
        bool StoragePlaceColumnSelected;
        bool StockColumnSelected;

        
        string supplierNameColumn;
        /// <summary>
        /// 
        /// </summary>
        public string SupplierNameColumn
        {
            get
            {
                return supplierNameColumn;
            }
        }
        
        string supplierNumberColumn;
        /// <summary>
        /// 
        /// </summary>
        public string SupplierNumberColumn
        {
            get
            {
                return supplierNumberColumn;
            }
        }
        
        string storagePlaceColumn;
        /// <summary>
        /// 
        /// </summary>
        public string StoragePlaceColumn
        {
            get
            {
                return storagePlaceColumn;
            }
        }

        string stockColumn;
        /// <summary>
        /// 
        /// </summary>
        public string StockColumn
        {
            get
            {
                return stockColumn;
            }
        }


        string partNameColumn;
        /// <summary>
        /// 
        /// </summary>
        public string PartNameColumn
        {
            get
            {
                return partNameColumn;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="storage"></param>
        public StorageExcelImport(Storage storage)
        {
            this.storage = storage;
            SupplierNameColumnSelected = false;
            SupplierNumberColumnSelected = false;
            StoragePlaceColumnSelected = false;
            StockColumnSelected = false;

            supplierNameColumn = "";
            supplierNumberColumn = "";
            storagePlaceColumn = "";
            stockColumn = "";
            partNameColumn = "";

            selectedColumn = "";

            InitializeComponent();

            labelSelect.Text = Str_SupplierName;
            labelSelected.Text = "None";

            this.DialogResult = DialogResult.Cancel;

            this.Width = 1200;
            this.Height = 680;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StorageExcelImport_Load(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();

            ofd.Multiselect = false;
            ofd.Filter = "Microsoft Excel file|*.xlsx";
            ofd.SupportMultiDottedExtensions = true;
            ofd.CheckPathExists = true;
            ofd.CheckFileExists = true;
            //ofd.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory;

            DialogResult ofd_dr = ofd.ShowDialog();
            
            
            if (ofd_dr == DialogResult.OK)
            {
                excelFilename = ofd.FileName;

                if (File.Exists(excelFilename))
                {
                    FileInfo fileInfo = new FileInfo(excelFilename);

                    try
                    {
                        using (ExcelPackage pck = new ExcelPackage(fileInfo))
                        {
                            ExcelWorksheet ws = pck.Workbook.Worksheets.First();
                            dataGridViewTable.DataSource = WorksheetToDataTable(ws);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Import failed. " + ex.Message, "Excel Import Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        this.Close();
                    }
                }
            }
            else
            {
                this.Close();
            }

        }
        
        /// <summary>
        /// Function for read data from Excel worksheet into DataTable
        /// </summary>
        /// <param name="ws"></param>
        /// <param name="hasHeader"></param>
        /// <returns></returns>
        private DataTable WorksheetToDataTable(ExcelWorksheet ws)
        {
            DataTable dt = new DataTable(ws.Name);
            int totalCols = ws.Dimension.End.Column;
            int totalRows = ws.Dimension.End.Row;
            int startRow = 1;
            ExcelRange wsRow;
            DataRow dr;

            string s;

            for (int i = 0; i < totalCols; i++)
            {
                s = "";
                s += (char)((int)('A') + i);

                dt.Columns.Add(s);
            }

            for (int rowNum = startRow; rowNum <= totalRows; rowNum++)
            {
                wsRow = ws.Cells[rowNum, 1, rowNum, totalCols];
                dr = dt.NewRow();
                
                foreach (var cell in wsRow)
                {
                    dr[cell.Start.Column - 1] = cell.Text;
                }

                dt.Rows.Add(dr);
            }

            return dt;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonOK_Click(object sender, EventArgs e)
        {
            if (SupplierNameColumnSelected == false)
            {
                labelSelect.Text = Str_SupplierNumber;
                labelSelected.Text = "None";
                supplierNameColumn = selectedColumn;
                SupplierNameColumnSelected = true;
                buttonUndo.Enabled = true;
                buttonOK.Text = "Select";
            }
            else if (SupplierNumberColumnSelected == false)
            {
                labelSelect.Text = Str_StoragePlace;
                labelSelected.Text = "None";
                supplierNumberColumn = selectedColumn;
                SupplierNumberColumnSelected = true;
                buttonOK.Text = "Select";
            }
            else if (StoragePlaceColumnSelected == false)
            {
                labelSelect.Text = Str_Stock;
                labelSelected.Text = "None";
                storagePlaceColumn = selectedColumn;
                StoragePlaceColumnSelected = true;
                buttonOK.Text = "Select";
            }
            else if (StockColumnSelected == false)
            {
                labelSelect.Text = Str_PartName;
                labelSelected.Text = "None";
                stockColumn = selectedColumn;
                StockColumnSelected = true;
                buttonOK.Text = "OK";
            }
            else
            {
                partNameColumn = selectedColumn;
                this.DialogResult = DialogResult.OK;
                this.Close();
            }

            selectedColumn = "";
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonUndo_Click(object sender, EventArgs e)
        {
            if (StockColumnSelected)
            {
                labelSelect.Text = Str_Stock;
                labelSelected.Text = stockColumn;
                stockColumn = "";
                StockColumnSelected = false;
                buttonOK.Text = "Select";
            }
            else if (StoragePlaceColumnSelected)
            {
                labelSelect.Text = Str_StoragePlace;
                labelSelected.Text = storagePlaceColumn;
                storagePlaceColumn = "";
                StoragePlaceColumnSelected = false;
                buttonOK.Text = "Select";
            }
            else if (SupplierNumberColumnSelected)
            {
                labelSelect.Text = Str_SupplierNumber;
                labelSelected.Text = supplierNumberColumn;
                supplierNumberColumn = "";
                SupplierNumberColumnSelected = false;
                buttonOK.Text = "Select";
            }
            else if (SupplierNameColumnSelected)
            {
                labelSelect.Text = Str_SupplierName;
                labelSelected.Text = supplierNameColumn;
                supplierNameColumn = "";
                SupplierNameColumnSelected = false;
                buttonUndo.Enabled = false;
                buttonOK.Text = "Select";
            }
            
            selectedColumn = "";
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridViewTable_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex >= 0)
            {
                selectedColumn = dataGridViewTable.Columns[e.ColumnIndex].HeaderText;
                labelSelected.Text = selectedColumn;
            }
        }
        
        /// <summary>
        /// Cancel button handler. All selected columns get cleared.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonCancel_Click(object sender, EventArgs e)
        {
            supplierNameColumn = "";
            supplierNumberColumn = "";
            storagePlaceColumn = "";
            stockColumn = "";

            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridViewTable_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex >= 0)
            {
                selectedColumn = dataGridViewTable.Columns[e.ColumnIndex].HeaderText;
                labelSelected.Text = selectedColumn;
                buttonOK_Click(null, null);
            }
        }
    }
}
