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
    public partial class StorageExcelImportMenu : Form
    {
        ExcelImporter excelImporter;
        public bool AllowedToClose;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="excelImporter"></param>
        public StorageExcelImportMenu(ExcelImporter excelImporter)
        {
            this.excelImporter = excelImporter;
            this.AllowedToClose = false;
            InitializeComponent();
        }

        /// <summary>
        /// Form close event handler. Close have to enabled by the app.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StorageExcelImportMenu_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (AllowedToClose == false)
            {
                e.Cancel = true;
            }
        }
    }
}
