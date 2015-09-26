﻿using System;
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

        public StorageExcelImportMenu(ExcelImporter excelImporter)
        {
            this.excelImporter = excelImporter;
            this.AllowedToClose = false;
            InitializeComponent();
        }

        private void StorageExcelImportMenu_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (AllowedToClose == false)
            {
                e.Cancel = true;
            }
        }
    }
}
