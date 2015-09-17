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

        string selectedColumn;


        bool SupplierNameColumnSelected;
        bool SupplierNumberColumnSelected;

        string supplierNameColumn;
        public string SupplierNameColumn
        {
            get
            {
                return supplierNameColumn;
            }
        }


        string supplierNumberColumn;
        public string SupplierNumberColumn
        {
            get
            {
                return supplierNumberColumn;
            }
        }


        string storagePlaceColumn;
        public string StoragePlaceColumn
        {
            get
            {
                return storagePlaceColumn;
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

            supplierNameColumn = "";
            supplierNumberColumn = "";
            storagePlaceColumn = "";

            selectedColumn = "";

            InitializeComponent();

            this.DialogResult = DialogResult.Cancel;
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
                SupplierNameColumnSelected = true;
                labelSelect.Text = "Supplier Number";
                labelSelected.Text = "None";
                selectedColumn = "";

                supplierNameColumn = selectedColumn;
                selectedColumn = "";
            }
            else if (SupplierNumberColumnSelected == false)
            {
                SupplierNumberColumnSelected = true;
                labelSelect.Text = "Storage Place (Optional)";
                labelSelected.Text = "None";

                supplierNumberColumn = selectedColumn;
                selectedColumn = "";
            }
            else
            {
                storagePlaceColumn = selectedColumn;
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
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
    }
}
