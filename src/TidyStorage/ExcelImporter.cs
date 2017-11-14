using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;
using System.Data;

using OfficeOpenXml;

using TidyStorage;
using TidyStorage.Suppliers;
using TidyStorage.Suppliers.Data;

namespace TidyStorage
{
    public class ExcelImporter
    {
        Storage storage;
        MainForm mainForm;
        
        bool CancelRequested;

        StorageExcelImportMenu seim;

        bool AutoMode;
        
        /// <summary>
        /// Storage parts excel importer contructor
        /// </summary>
        /// <param name="storage">Storage dabase handler</param>
        /// <param name="mainForm">TidyStorage Main form</param>
        public ExcelImporter(Storage storage, MainForm mainForm)
        {
            this.storage = storage;
            this.mainForm = mainForm;

            AutoMode = false;
           
            CancelRequested = false;
        }

        /// <summary>
        /// Cancel button event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExcelImportMenuCancel_Click(object sender, EventArgs e)
        {
            CancelRequested = true;

        }

        /// <summary>
        /// Handling button that is switching between Auto import mode and Manual import mode.
        /// In manual import mode all parts have to be confirmed in Storage part form.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExcelImportMenuMode_Click(object sender, EventArgs e)
        {
            Button bt = (Button)sender;
            AutoMode = !AutoMode;
            bt.Text = (AutoMode) ? "Auto" : "Manual";
        }
        
        /// <summary>
        /// Starts import procedure. Processing GUI and starting import worker
        /// </summary>
        public void Start()
        {
            StorageExcelImport sei = new StorageExcelImport(storage);

            if (sei.ShowDialog() == DialogResult.OK)
            {
                if ((sei.SupplierNameColumn != "") && (sei.SupplierNumberColumn != "") ||
                    (sei.PartNameColumn != "") ||
                    (sei.StoragePlaceColumn != ""))
                {
                    string excelFilename = sei.ExcelFilename;

                    if (File.Exists(excelFilename))
                    {
                        FileInfo fileInfo = new FileInfo(excelFilename);

                        try
                        {
                            //Read excel file
                            using (ExcelPackage pck = new ExcelPackage(fileInfo))
                            {
                                ExcelWorksheet ws = pck.Workbook.Worksheets.First();
      
                                int totalRows = ws.Dimension.End.Row;
                                int totalCols = ws.Dimension.End.Column;

                                seim = new StorageExcelImportMenu(this);
                                seim.buttonCancel.Click += ExcelImportMenuCancel_Click;
                                seim.buttonMode.Click += ExcelImportMenuMode_Click;
                                
                                string SupplierName;
                                string SupplierNumber;
                                string StoragePlace;
                                string Stock;
                                string PartName;
                                string r;

                                string columns = string.Format("{0},{1},{2},{3}", StorageConst.Str_Part_id, "suppliernumber", "productnumber", "storage_place_number");

                                //Open import progress form
                                seim.Show();

                                seim.Top = mainForm.Top;
                                seim.Left = mainForm.Left;

                                int totalValidRows = 0;

                                for (int rowNum = 1; rowNum <= totalRows; rowNum++)
                                {
                                    r = rowNum.ToString();

                                    SupplierName = (sei.SupplierNameColumn != "") ? ws.Cells[sei.SupplierNameColumn + r].Text : "";
                                    SupplierNumber = (sei.SupplierNumberColumn != "") ? ws.Cells[sei.SupplierNumberColumn + r].Text : "";
                                    StoragePlace = (sei.StoragePlaceColumn != "") ? ws.Cells[sei.StoragePlaceColumn + r].Text.ToUpper() : "";
                                    Stock = (sei.StockColumn != "") ? ws.Cells[sei.StockColumn + r].Text : "";
                                    PartName = (sei.PartNameColumn != "") ? ws.Cells[sei.PartNameColumn + r].Text.ToUpper() : "";

                                    if (((SupplierName != "") && (SupplierNumber != "")) || (PartName != "") || (StoragePlace != ""))
                                    {
                                        totalValidRows++;
                                    }
                                }


                                for (int rowNum = 1; rowNum <= totalRows; rowNum++)
                                {
                                    if (CancelRequested) break;

                                    StoragePart part = null;

                                    seim.labelCount.Text = rowNum.ToString() + " / " + totalValidRows.ToString();

                                    r = rowNum.ToString();

                                    SupplierName    = (sei.SupplierNameColumn != "")    ? ws.Cells[sei.SupplierNameColumn + r].Text : "";
                                    SupplierNumber  = (sei.SupplierNumberColumn != "")  ? ws.Cells[sei.SupplierNumberColumn + r].Text : "";
                                    StoragePlace    = (sei.StoragePlaceColumn != "")    ? ws.Cells[sei.StoragePlaceColumn + r].Text.ToUpper() : "";
                                    Stock           = (sei.StockColumn != "")           ? ws.Cells[sei.StockColumn + r].Text : "";
                                    PartName        = (sei.PartNameColumn != "")        ? ws.Cells[sei.PartNameColumn + r].Text.ToUpper() : "";

                                    seim.labelSupplierName.Text = SupplierName;
                                    seim.labelSupplierNumber.Text = SupplierNumber;
                                    seim.labelStoragePlace.Text = StoragePlace;
                                    seim.labelStock.Text = Stock;
                                    seim.labelPartName.Text = PartName;



                                    string where_cond = "0 ";

                                    if ((SupplierName != "") && (SupplierNumber != ""))
                                    {
                                        where_cond += " OR suppliernumber LIKE '" + SupplierNumber + "'";
                                    }

                                    if (PartName != "") 
                                    {
                                        where_cond += " OR productnumber LIKE '" + PartName + "'";
                                    }

                                    if (StoragePlace != "")
                                    {
                                        where_cond += " OR storage_place_number LIKE '" + StoragePlace + "'";
                                    }


                                    if (where_cond.Length > 3)
                                    {
                                        DataTable dt = storage.GetTable(StorageConst.Str_Part, columns , where_cond);

                                        if (dt.Rows.Count > 0)
                                        {
                                            DialogResult dr;


                                            int id = (int)(Int64)dt.Rows[0].ItemArray[0];

                                            if (AutoMode)
                                            {
                                                dr = DialogResult.Yes;
                                            }
                                            else
                                            {
                                                dr = MessageBox.Show("Part with the same part name, supplier number or storage place found:"
                                                    + System.Environment.NewLine + "Do You want to import values?"
                                                    + System.Environment.NewLine
                                                    + System.Environment.NewLine + "Press \"Cancel\" to skip this part. Press \"No\" to create new part.",
                                                    "Similar Part Found", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                                            }

                                            if (dr == DialogResult.Cancel)
                                            {
                                                continue;
                                            }
                                            else if (dr == DialogResult.Yes)
                                            {
                                                part = new StoragePart(storage, id);
                                            }
                                        }


                                        
                                        StorageForm spf = new StorageForm(storage, part, PartName, SupplierName, SupplierNumber, StoragePlace, Stock);
                                        spf.Show();
                                        spf.Center(mainForm);
                                        spf.Left += 100;

                                        if (AutoMode)
                                        {
                                            if ((SupplierName != "") && (SupplierNumber != ""))
                                            {
                                                spf.buttonImport_Click(this, null);
                                            }
                                            Application.DoEvents();
                                        }

                                        if (AutoMode)
                                        {
                                            spf.buttonOk_Click(null, null);
                                            Application.DoEvents();
                                        }


                                        while ((CancelRequested == false) && (spf.StorageClosed == false) && (AutoMode == false))
                                        {
                                            Application.DoEvents();

                                            if (AutoMode)
                                            {
                                                if ((SupplierName != "") && (SupplierNumber != ""))
                                                {
                                                    spf.buttonImport_Click(this, null);
                                                }
                                                Application.DoEvents();
                                                spf.buttonOk_Click(null, null);
                                                Application.DoEvents();
                                            }
                                        }

                                        if (spf.StorageClosed == false)
                                        {
                                            spf.Close();
                                        }

                                    }
                                }

                                mainForm.RefreshStorageTable();
                                mainForm.RefreshListBox();



                                seim.labelCount.Text = "Done";
                                seim.AllowedToClose = true;
                                seim.Close();

                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Import failed. " + ex.Message, "Excel Import Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Unable to import Excel file. Not enough columns selected", "Excel Import Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }
        }
    }
}
