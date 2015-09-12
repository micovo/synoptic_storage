using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using TidyStorage.Suppliers;
using TidyStorage.Suppliers.Data;

namespace TidyStorage
{
    public partial class StoragePartImporter : Form
    {
        Supplier supplier;
        string suppliernumber;
        LoadingForm loadingForm;
        Task downloadTask;
        public SupplierPart supplierPart;

        List<PartRow> unusedPartRows = new List<PartRow>();
        List<PartRow> downloadedPartRows = new List<PartRow>();
        PartRow nullItem;


        public StoragePartImporter(string supplier, string suppliernumber)
        {
            this.suppliernumber = suppliernumber;
            nullItem = new PartRow("Do no update");
           

            switch (supplier)
            {
                case "Farnell": this.supplier = new FarnellSupplier(suppliernumber); break;
                case "Mouser": this.supplier = new MouserSupplier(suppliernumber); break;
                case "GME": this.supplier = new GMESupplier(suppliernumber); break;
                case "TME": this.supplier = new TMESupplier(suppliernumber); break;
                default: MessageBox.Show("Supplier is not implemented, please enter parameters manualy or contact developers.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation); break;
            }

            InitializeComponent();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonOk_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StoragePartImporter_Load(object sender, EventArgs e)
        {
            if (supplier == null)
            {
                this.Close();
                return;
            }

            loadingForm = new LoadingForm();
            downloadTask = new Task(new Action(DownloadWorker));
            downloadTask.Start();
            loadingForm.ShowDialog();
        }


        /// <summary>
        /// 
        /// </summary>
        private void DownloadWorker()
        {
            int timeout = 0;
            bool DownloadDone = false;
            int progress = 0;


            supplierPart = supplier.DownloadPart();

            downloadedPartRows.AddRange(supplierPart.rows);

            while ((DownloadDone == false) && (timeout < 10000))
            {
                loadingForm.UpdateProgress(progress++);
                loadingForm.UpdateLabel("Test");
                this.Invoke(new Action(UpdateComboBoxes));

                //supplierPart = supplier.DownloadPart();
                Thread.Sleep(500);
                timeout += 15000;
            }

            loadingForm.AllowedToClose = true;
            loadingForm.Invoke(new Action(loadingForm.Close));
        }

   
        


        /// <summary>
        /// 
        /// </summary>
        private void UpdateComboBoxes()
        {
            unusedPartRows.Clear();
            unusedPartRows.AddRange(downloadedPartRows);

            foreach (Control x in this.tableLayoutPanel1.Controls)
            {
                if (x.GetType() == typeof(ComboBox))
                {
                    ComboBox cb = (ComboBox)x;
                    cb.Items.Clear();
                    cb.Items.Add(nullItem);
                    cb.Items.AddRange(unusedPartRows.ToArray());
                    cb.SelectedIndex = 0;
                }
            }
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void comboBox_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void comboBox_SelectedValueChanged(object sender, EventArgs e)
        {

        }

    }
}
