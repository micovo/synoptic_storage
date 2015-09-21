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

        public StorageExcelImportMenu(ExcelImporter excelImporter)
        {
            this.excelImporter = excelImporter;
            InitializeComponent();
        }

        private void labelCount_Click(object sender, EventArgs e)
        {

        }
    }
}
