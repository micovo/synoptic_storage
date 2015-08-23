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
    public partial class StoragePartForm : Form
    {
        StoragePart part;
        Storage storage;

        public StoragePartForm(Storage storage, StoragePart part)
        {
            InitializeComponent();

            this.storage = storage;

            if  (part == null)
            {
                this.part = new StoragePart();
            }
            else
            {
                this.part = part;
            }
        }

        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void buttonOk_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void buttonImport_Click(object sender, EventArgs e)
        {
            // Process part import here
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonPriceCheck_Click(object sender, EventArgs e)
        {
            // Process Price check
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonOpenDatasheet_Click(object sender, EventArgs e)
        {
            string url = textBoxDatasheet.Text;
            if (url.ToLower().StartsWith("http://"))
            {
                System.Diagnostics.Process.Start("http://google.com");
            }
            else
            {
                textBoxDatasheet.BackColor = Color.LightSalmon;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBoxDatasheet_TextChanged(object sender, EventArgs e)
        {
            textBoxDatasheet.BackColor = Color.White;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonEdit_Click(object sender, EventArgs e)
        {
            StoragePartTypeEditor spte;

            if (sender == buttonEditManuf)
            {
                spte = new StoragePartTypeEditor(storage, StorageTypeTables.Manufacturer);
            }
            else if (sender == buttonEditPackage)
            {
                spte = new StoragePartTypeEditor(storage, StorageTypeTables.Package);
            }
            else if (sender == buttonEditPlaceType)
            {
                spte = new StoragePartTypeEditor(storage, StorageTypeTables.PlaceType);
            }
            else if (sender == buttonEditType)
            {
                spte = new StoragePartTypeEditor(storage, StorageTypeTables.PartType);
            }
            else
            {
                return;
            }

            
            spte.StartPosition = FormStartPosition.CenterParent;
            DialogResult ds = spte.ShowDialog();

            if (ds == DialogResult.OK)
            {
                LoadPart();
            }
        }


        /// <summary>
        /// Loads Part data into the GUI
        /// </summary>
        void LoadPart()
        {

        }
    }
}
