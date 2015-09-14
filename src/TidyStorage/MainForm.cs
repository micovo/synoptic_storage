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
using OfficeOpenXml;
using System.IO;

namespace TidyStorage
{
    public partial class MainForm : Form
    {
        Storage currentStorage;

        string part_filter = "1";
        string part_filter_fulltext = "";

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
        private void NoStorageError()
        {
            MessageBox.Show("There is no storage. Please open or create new storage first.", "Storage Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }


        /// <summary>
        /// 
        /// </summary>
        private void CreateNewStoragePart()
        {
            if (currentStorage != null)
            {
                StoragePartForm spf = new StoragePartForm(currentStorage, null);
                spf.StartPosition = FormStartPosition.CenterParent;
                spf.ShowDialog();
                RefreshStorageTable();
                RefreshListBox();
            }
            else
            {
                NoStorageError();
                RefreshListBox();
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="part"></param>
        private void EditStoragePart(StoragePart part)
        {
            if (currentStorage != null)
            {
                StoragePartForm spf = new StoragePartForm(currentStorage, part);
                spf.StartPosition = FormStartPosition.CenterParent;
                spf.ShowDialog();
                RefreshStorageTable();
                RefreshListBox();
            }
            else
            {
                NoStorageError();
                RefreshListBox();
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
        }
        


        /// <summary>
        /// Excel file import/export debugging
        /// </summary>
        public void ExcellTest()
        {
            return;

            FileInfo newFile = new FileInfo("sample6.xlsx");

            ExcelPackage pck = new ExcelPackage(newFile);
            //Add the Content sheet


            var ws = pck.Workbook.Worksheets["Content"];

            ws.View.ShowGridLines = true;

            //textBoxConsole.Text += ws.Cells["B1"].Value;

            ws.Cells["C2"].Formula = "SUM(B1:B5)";
            ws.Cells["B1"].Value = 6;
            ws.Cells["B2"].Value = 2;
            ws.Cells["B3"].Value = 3;
            ws.Cells["B4"].Value = 4;
            ws.Cells["B5"].Value = 5;
            
            pck.Save();

            /*
            ExcelWorkbook exw = OfficeOpenXml.ExcelWorksheets()
            Microsoft.Office.Interop.Excel.Application excelApp = null;
            Workbooks workBooks = null;
            Workbook workBook = null;
            Worksheet workSheet;

            try
            {

                excelApp = new Microsoft.Office.Interop.Excel.Application();
                excelApp.DisplayAlerts = false;

                workBooks = excelApp.Workbooks;
                workBook = workBooks.Open(@"D:\Visual Studio\TidyStorage\src\TidyStorage\bin\Debug\test.xlsx", AddToMru: false);
                workSheet = workBook.Worksheets.get_Item(1);

                int nOfColumns = workSheet.UsedRange.Columns.Count;
                int lastRowNumber = workSheet.UsedRange.Rows.Count;

                Range rng = workSheet.Range["C1"];
                rng.Formula = "=SUM(B2:B4)";
                String formula = rng.Formula; //retrieve the formula successfully


                rng.FormulaHidden = false;
                workSheet.Unprotect();

                workBook.SaveAs(@"D:\Visual Studio\TidyStorage\src\TidyStorage\bin\Debug\test.xlsx", AccessMode: XlSaveAsAccessMode.xlExclusive);

                formula = rng.Formula;  //retrieve the formula successfully
                bool hidden = rng.FormulaHidden;

            }
            catch (Exception e)
            {
                throw;
            }
            finally
            {
                if (workBook != null)
                {
                    workBook.Close();
                    workBook = null;
                }
                if (workBooks != null)
                {
                    workBooks.Close();
                    workBooks = null;
                }
                if (excelApp != null)
                {
                    excelApp.Quit();
                    excelApp = null;
                }
            }
            */
        }
        
        /// <summary>
        /// 
        /// </summary>
        public void RefreshStorageTable()
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
        }



        /// <summary>
        /// 
        /// </summary>
        public void RefreshListBox()
        {
            listBoxFilterType.DataSource = currentStorage.GetStringIdArray(StorageTypeTables.PartType);
            listBoxFilterPackage.DataSource = currentStorage.GetStringIdArray(StorageTypeTables.Package);

            listBoxFilterType.ClearSelected();
            listBoxFilterPackage.ClearSelected();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainForm_Load(object sender, EventArgs e)
        {
            ExcellTest();

            currentStorage = new Storage("test5.sqlite");
            RefreshStorageTable();
            RefreshListBox();

            Rectangle rec = Screen.FromControl(this).Bounds;

            if ((rec.Width <= 1280) && (rec.Height <= 800))
            {
                this.WindowState = FormWindowState.Maximized;
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
            currentStorage.Save();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void saveToolStripButton_Click(object sender, EventArgs e)
        {
            currentStorage.Save();
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
            StoragePartTypeEditor spte = new StoragePartTypeEditor(currentStorage, StorageTypeTables.Manufacturer);
            spte.ShowDialog();
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void editPartTypesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StoragePartTypeEditor spte = new StoragePartTypeEditor(currentStorage, StorageTypeTables.PartType);
            spte.ShowDialog();
            RefreshListBox();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void editPackagesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StoragePartTypeEditor spte = new StoragePartTypeEditor(currentStorage, StorageTypeTables.Package);
            spte.ShowDialog();
            RefreshListBox();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void editPlaceTypesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StoragePartTypeEditor spte = new StoragePartTypeEditor(currentStorage, StorageTypeTables.PlaceType);
            spte.ShowDialog();
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
    }
}
