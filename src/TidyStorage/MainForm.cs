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
            }
            else
            {
                NoStorageError();
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
            }
            else
            {
                NoStorageError();
            }
        }


        private void newToolStripButton_Click(object sender, EventArgs e)
        {
            if (tabControl1.SelectedIndex == 0)
            {
                CreateNewStoragePart();
            }
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {
           

        }


        public void ExcellTest()
        {
            FileInfo newFile = new FileInfo("sample6.xlsx");

            ExcelPackage pck = new ExcelPackage(newFile);
            //Add the Content sheet


            var ws = pck.Workbook.Worksheets["Content"];

            ws.View.ShowGridLines = true;

            textBox8.Text += ws.Cells["B1"].Value;

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
        



        private void MainForm_Load(object sender, EventArgs e)
        {
            ExcellTest();

            currentStorage = new Storage("test2.sqlite");

            dataGridViewStorage.DataSource = currentStorage.GetTable("part");
        }

        private void dataGridViewStorage_UserDeletingRow(object sender, DataGridViewRowCancelEventArgs e)
        {
            e.Cancel = true;
        }

        private void partToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CreateNewStoragePart();
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            currentStorage.Save();
        }

        private void saveToolStripButton_Click(object sender, EventArgs e)
        {
            currentStorage.Save();
        }
    }
}
