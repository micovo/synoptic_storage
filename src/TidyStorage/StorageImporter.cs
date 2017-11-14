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
    public class StorageImporter
    {
        Storage storage;
        MainForm mainForm;
        StorageImportMenu seim;

        bool AutoMode;
        bool PriceCheckOnly;
        bool CancelRequested;

        List<StoragePart> parts;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="storage"></param>
        public StorageImporter(Storage storage, MainForm mainForm, List<StoragePart> parts, bool PriceCheckOnly)
        {
            this.storage = storage;
            this.mainForm = mainForm;
            this.PriceCheckOnly = PriceCheckOnly;

            if (PriceCheckOnly)
            {
                AutoMode = true;
            }
            else
            {
                AutoMode = false;
            }


            seim = new StorageImportMenu();
            seim.buttonMode.Text = (AutoMode) ? "Auto" : "Manual";
            seim.AllowedToClose = false;

            CancelRequested = false;

            this.parts = parts;
        }

        /// <summary>
        /// Cancel button click event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ImportMenuCancel_Click(object sender, EventArgs e)
        {
            CancelRequested = true;
        }

        /// <summary>
        /// Auto/Manual button click event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ImportMenuMode_Click(object sender, EventArgs e)
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
            if ((parts == null) || (parts.Count() == 0)) return;

            seim.labelCount.Text = "1/" + parts.Count.ToString();
            seim.Show();

            try
            {
                for (int i = 0; i < parts.Count; i++)
                {
                    seim.labelCount.Text = (i + 1).ToString() + "/" + parts.Count.ToString();

                    StoragePart part = parts[i];

                    seim.buttonCancel.Click += ImportMenuCancel_Click;
                    seim.buttonMode.Click += ImportMenuMode_Click;
                    
                    StorageForm spf = new StorageForm(storage, part);
                    spf.Show();
                    spf.Center(mainForm);
                    spf.Left += 100;


                    Application.DoEvents();
                    if (PriceCheckOnly)
                    {
                        spf.buttonPriceCheck_Click(null, null);
                    }
                    else
                    {
                        spf.buttonImport_Click(null, null);
                    }
                    Application.DoEvents();
                    
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
                            spf.buttonOk_Click(null, null);
                            Application.DoEvents();
                        }
                    }
                    if (spf.StorageClosed == false)
                    {
                        spf.Close();
                    }
                    
                    if (CancelRequested) break;
                }
                
                mainForm.RefreshStorageTable();
                mainForm.RefreshListBox();

                seim.labelCount.Text = "Done";
                seim.AllowedToClose = true;
                seim.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Import failed. " + ex.Message, "Excel Import Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
