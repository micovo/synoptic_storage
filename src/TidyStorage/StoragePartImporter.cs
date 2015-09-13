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
        LoadingForm loadingForm;
        Task downloadTask;
        public SupplierPart supplierPart;

        List<PartRow> unusedPartRows = new List<PartRow>();
        List<PartRow> downloadedPartRows = new List<PartRow>();
        PartRow nullItem;

        public string PrimaryValueUnit = "";
        public string PrimaryValueTolernceUnit = "%";
        public string SecondaryValueUnit = "";
        public string ThirdValueUnit = "";

        public StoragePartImporter(Supplier supplier)
        {
            this.supplier = supplier;

            nullItem = new PartRow("Do no update");

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

            Thread.Sleep(250);

            loadingForm.UpdateProgress(0);

            loadingForm.UpdateLabel("Downloading part " + supplier.PartNumber + " from " + this.supplier.Name);

            supplierPart = supplier.DownloadPart();

            loadingForm.UpdateProgress(25);

            loadingForm.UpdateLabel("Saving and processing");

            downloadedPartRows.AddRange(supplierPart.rows);


            loadingForm.UpdateProgress(50);


            loadingForm.UpdateLabel("Loading into form");

            while ((DownloadDone == false) && (timeout < 10000))
            {

                this.Invoke(new Action(UpdateComboBoxes));

                //supplierPart = supplier.DownloadPart();
                Thread.Sleep(500);
                timeout += 15000;
            }

            loadingForm.UpdateProgress(100);
            loadingForm.UpdateLabel("Done");

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

            if (supplier.GetType() == typeof(FarnellSupplier))
            {
                comboBox1.SelectedItem = unusedPartRows[2];
                comboBox2.SelectedItem = unusedPartRows[0];
                comboBox3.SelectedItem = unusedPartRows.FirstOrDefault(x => (x.Name.Contains("Pouzdr")));
                comboBox4.SelectedItem = unusedPartRows.FirstOrDefault(x => (x.Name == "Datasheet"));
            }

           

            if (PrimaryValueUnit != "") comboBox6.SelectedItem = unusedPartRows.FirstOrDefault(x => (x.Value.Contains(PrimaryValueUnit)));
            if (PrimaryValueTolernceUnit != "") comboBox7.SelectedItem = unusedPartRows.FirstOrDefault(x => (x.Value.Contains(PrimaryValueTolernceUnit)));
            if (SecondaryValueUnit != "") comboBox8.SelectedItem = unusedPartRows.FirstOrDefault(x => (x.Value.Contains(SecondaryValueUnit)));
            if (ThirdValueUnit != "") comboBox10.SelectedItem = unusedPartRows.FirstOrDefault(x => (x.Value.Contains(ThirdValueUnit)));

            comboBox12.SelectedItem = unusedPartRows.FirstOrDefault(x => (x.Value.Contains("°C") && (x.Value.Contains("ppm") == false) && x.Value[0] == '-')) ?? nullItem;
            comboBox13.SelectedItem = unusedPartRows.FirstOrDefault(x => (x.Value.Contains("°C") && (x.Value.Contains("ppm") == false) && x.Value[0] != '-')) ?? nullItem ;


        }


    }
}
