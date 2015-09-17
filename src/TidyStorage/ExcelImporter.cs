using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;

using OfficeOpenXml;

using TidyStorage;
using TidyStorage.Suppliers;
using TidyStorage.Suppliers.Data;

namespace TidyStorage
{
    public class ExcelImporter
    {
        Storage storage;
        LoadingForm loadingForm;
        Task importTask;

        bool ImportFinished;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="storage"></param>
        public ExcelImporter(Storage storage)
        {
            this.storage = storage;
            ImportFinished = false;
        }

           
        /// <summary>
        /// Starts import procedure. Processing GUI and starting import worker
        /// </summary>
        public void Start()
        {
            StorageExcelImport sei = new StorageExcelImport(storage);

            if (sei.ShowDialog() == DialogResult.OK)
            {
                loadingForm = new LoadingForm();
                importTask = new Task(new Action(Worker));

                importTask.Start();

                loadingForm.Show();

                while (ImportFinished == false)
                {
                    Application.DoEvents();
                }

                loadingForm.AllowedToClose = true;
                loadingForm.Close();
            }
        }


        /// <summary>
        /// Import worker. Main import algorhithm implementation.
        /// </summary>
        public void Worker()
        {
            int test = 10;

            while (test-- > 0)
            {
                Thread.Sleep(1000);
            }

            ImportFinished = true;
        }


        /// <summary>
        /// Excel file import/export debugging
        /// </summary>
        public void ExcellTest()
        {
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
    }
}
