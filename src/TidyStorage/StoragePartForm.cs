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
    public partial class StoragePartForm : Form
    {
        StoragePart part;
        Storage storage;
        List<IndexedName> ManufacturerList;
        List<IndexedName> PackageList;
        List<IndexedName> PartTypeList;
        List<IndexedName> PlaceTypeList;
        List<IndexedName> SupplierList;


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

            RefreshComboBoxes();
            RefreshForm();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonOk_Click(object sender, EventArgs e)
        {
            if (ProcessFormValues())
            {
                SavePart();
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                System.Media.SystemSounds.Beep.Play();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonApply_Click(object sender, EventArgs e)
        {
            if (ProcessFormValues())
            {
                SavePart();
            }
            else
            {
                System.Media.SystemSounds.Beep.Play();
            }
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
            else if (sender == buttonEditSuplier)
            {
                spte = new StoragePartTypeEditor(storage, StorageTypeTables.Supplier);
            }
            else
            {
                return;
            }
            
            spte.StartPosition = FormStartPosition.CenterParent;
            DialogResult ds = spte.ShowDialog();

            RefreshComboBoxes();
        }


        

        /// <summary>
        /// Refresh of the editable comboboxes
        /// </summary>
        void RefreshComboBoxes()
        {
            ManufacturerList = storage.GetStringIdArray(StorageTypeTables.Manufacturer);
            comboBoxManufacturer.Items.Clear();
            comboBoxManufacturer.Items.AddRange(ManufacturerList.ToArray());

            PackageList = storage.GetStringIdArray(StorageTypeTables.Package);
            comboBoxPackage.Items.Clear();
            comboBoxPackage.Items.AddRange(PackageList.ToArray());

            PartTypeList = storage.GetStringIdArray(StorageTypeTables.PartType);
            comboBoxPartType.Items.Clear();
            comboBoxPartType.Items.AddRange(PartTypeList.ToArray());

            PlaceTypeList = storage.GetStringIdArray(StorageTypeTables.PlaceType);
            comboBoxPlaceType.Items.Clear();
            comboBoxPlaceType.Items.AddRange(PlaceTypeList.ToArray());

            SupplierList = storage.GetStringIdArray(StorageTypeTables.Supplier);
            comboBoxSupplier.Items.Clear();
            comboBoxSupplier.Items.AddRange(SupplierList.ToArray());

            

            IndexedName i = ManufacturerList.FirstOrDefault(x => x.id == this.part.id_manufacturer);
            if (i != null)
            {
                comboBoxManufacturer.SelectedItem = i;
            }
            else
            {
                comboBoxManufacturer.Text = "";
                comboBoxManufacturer.SelectedItem = null;
                this.part.id_manufacturer = -1;
            }


            i = PackageList.FirstOrDefault(x => x.id == this.part.id_part_package);
            if (i != null)
            {
                comboBoxPackage.SelectedItem = i;
            }
            else
            {
                comboBoxPackage.Text = "";
                comboBoxPackage.SelectedItem = null;
                this.part.id_part_package = -1;
            }


            i = PartTypeList.FirstOrDefault(x => x.id == this.part.id_part_type);
            if (i != null)
            {
                comboBoxPartType.SelectedItem = i;
            }
            else
            {
                comboBoxPartType.Text = "";
                comboBoxPartType.SelectedItem = null;
                this.part.id_part_type = -1;
            }


            i = PlaceTypeList.FirstOrDefault(x => x.id == this.part.id_storage_place);
            if (i != null)
            {
                comboBoxPlaceType.SelectedItem = i;
            }
            else
            {
                comboBoxPlaceType.Text = "";
                comboBoxPlaceType.SelectedItem = null;
                this.part.id_storage_place = -1;
            }



            i = SupplierList.FirstOrDefault(x => x.id == this.part.id_supplier);
            if (i != null)
            {
                comboBoxSupplier.SelectedItem = i;
            }
            else
            {
                comboBoxSupplier.Text = "";
                comboBoxSupplier.SelectedItem = null;
                this.part.id_supplier = -1;
            }

        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        string GetPartPriceString(double p)
        {
            if (double.IsNaN(p) == false)
            {
                //return p.ToString() + " " + this.part.currency;
                return p.ToString();
            }
            return "";
        }


        /// <summary>
        /// 
        /// </summary>
        void RefreshForm()
        {
            textBoxProductName.Text = this.part.productnumber;
            textBoxDatasheet.Text = this.part.datasheet_url;
            textBoxComment.Text = this.part.comment;
            textBoxCurrency.Text = this.part.currency;


            textBoxTempRangeMin.Text = this.part.temperature_from.ToString();
            textBoxTempRangeMax.Text = this.part.temperature_to.ToString();
            textBoxPlaceNumber.Text = this.part.storage_place_number.ToString();
 
            numericUpDownStock.Value = this.part.stock;

            textBoxPrice1pcs.Text = GetPartPriceString(this.part.price_1pcs);
            textBoxPrice10pcs.Text = GetPartPriceString(this.part.price_10pcs);
            textBoxPrice100pcs.Text = GetPartPriceString(this.part.price_100pcs);
            textBoxPrice1000pcs.Text = GetPartPriceString(this.part.price_1000pcs);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void comboBoxManufacturer_SelectionChangeCommitted(object sender, EventArgs e)
        {
            if (comboBoxManufacturer.SelectedItem == null)
            {
                this.part.id_manufacturer = -1;
            }
            else
            {
                this.part.id_manufacturer = ((IndexedName)comboBoxManufacturer.SelectedItem).id;
            }
        }



        /// <summary>
        /// Saves part into database. New part is inserted into database if the part is new.
        /// </summary>
        void SavePart()
        {
            this.part.productnumber = textBoxProductName.Text;

            if (part.id_part < 1)
            {
                part.id_part = storage.InsertIntoTable(StorageConst.Str_Part, "productnumber", "'" + part.productnumber + "'");
            }

            string s = part.GetUpdateString();
            string c = StorageConst.Str_Part_id + "=" + this.part.id_part.ToString();

            storage.UpdateTable(StorageConst.Str_Part, s, c);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="tb"></param>
        /// <param name="output"></param>
        /// <returns></returns>
        bool ProcessTextBox(TextBox tb, out string output)
        {
            output = "";
            string o = tb.Text;
            

            if ((tb.Text.Contains('"')) || (tb.Text.Contains('\'')))
            {
                tb.BackColor = Color.LightCoral;
                return false;
            }

            output = o;
            return true;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="tb"></param>
        /// <param name="output"></param>
        /// <returns></returns>
        bool ProcessTextBox(TextBox tb, out int output)
        {
            output = 0;
            int o = 0;
            string str = tb.Text;


            if (int.TryParse(str, out o) == false)
            {
                tb.BackColor = Color.LightCoral;
                return false;
            }

            output = o;
            return true;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="tb"></param>
        /// <param name="output"></param>
        /// <returns></returns>
        bool ProcessTextBox(TextBox tb, out double output)
        {
            output = 0;
            double o = 0;
            string str = tb.Text;

            if (str == "")
            {
                output = double.NaN;
                return true;
            }

            //TODO nano, micro, mili, piko, mega, giga, kilo

            int i = str.LastIndexOfAny("0123456789.,".ToCharArray());

            if (i > -1)
            {
                str = str.Substring(0, i + 1);

                if (double.TryParse(str, out o))
                {
                    output = o;
                    return true;
                }
            }

            tb.BackColor = Color.LightCoral;
            return false;
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="tb"></param>
        /// <param name="output"></param>
        /// <returns></returns>
        bool ProcessTextBoxPrice(TextBox tb, out double output)
        {
            output = 0;
            double o = 0;
            string str = tb.Text;

            if (str == "")
            {
                output = double.NaN;
                return true;
            }

            int i = str.LastIndexOfAny("0123456789.,".ToCharArray());

            if (i > -1)
            {
                str = str.Substring(0, i + 1);

                if (double.TryParse(str, out o))
                {
                    output = o;
                    return true;
                }
            }

            tb.BackColor = Color.LightCoral;
            return false;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="cb"></param>
        /// <returns></returns>
        int ProcessTypeComboBox(ComboBox cb)
        {
            int i = -1;
            object o = cb.SelectedItem;
            if (o != null) i = ((IndexedName)o).id;
            return i;
        }


        /// <summary>
        /// Saves values from GUI into part data structure
        /// </summary>
        /// <returns></returns>
        bool ProcessFormValues()
        {
            string s;
            int i;
            double d;

            //String texboxes
            if (ProcessTextBox(textBoxProductName, out s)) this.part.productnumber = s; else return false;
            if (ProcessTextBox(textBoxDatasheet, out s)) this.part.datasheet_url = s; else return false;
            if (ProcessTextBox(textBoxComment, out s)) this.part.comment = s; else return false;
            if (ProcessTextBox(textBoxSupplierNumber, out s)) this.part.suppliernumber = s; else return false;
            if (ProcessTextBox(textBoxCurrency, out s)) this.part.currency = s; else return false;

            //Int textboxes
            if (ProcessTextBox(textBoxTempRangeMin, out i)) this.part.temperature_from = i; else return false;
            if (ProcessTextBox(textBoxTempRangeMax, out i)) this.part.temperature_to = i; else return false;

            //Float textboxes
            if (ProcessTextBox(textBoxPrimaryValue, out d)) this.part.primary_value = d; else return false;
            if (ProcessTextBox(textBoxPrimaryTolerance, out d)) this.part.primary_tolerance = d; else return false;
            if (ProcessTextBox(textBoxSecondaryValue, out d)) this.part.secondary_value = d; else return false;
            if (ProcessTextBox(textBoxSecondaryTolerance, out d)) this.part.secondary_tolerance = d; else return false;
            if (ProcessTextBox(textBoxThridValue, out d)) this.part.tertiary_value = d; else return false;
            if (ProcessTextBox(textBoxThridTolerance, out d)) this.part.tertiary_tolerance = d; else return false;

            //Price textboxes
            if (ProcessTextBoxPrice(textBoxPrice1pcs, out d)) this.part.price_1pcs = d; else return false;
            if (ProcessTextBoxPrice(textBoxPrice10pcs, out d)) this.part.price_10pcs = d; else return false;
            if (ProcessTextBoxPrice(textBoxPrice100pcs, out d)) this.part.price_100pcs = d; else return false;
            if (ProcessTextBoxPrice(textBoxPrice1000pcs, out d)) this.part.price_1000pcs = d; else return false;


            //ComboBoxes
            this.part.id_supplier = ProcessTypeComboBox(comboBoxSupplier);
            this.part.id_manufacturer = ProcessTypeComboBox(comboBoxManufacturer);
            this.part.id_part_package = ProcessTypeComboBox(comboBoxPackage);
            this.part.id_part_type = ProcessTypeComboBox(comboBoxPartType);
            this.part.id_storage_place = ProcessTypeComboBox(comboBoxPlaceType);




            return true;
        }




        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBoxProductName_TextChanged(object sender, EventArgs e)
        {
            if (sender.GetType() == typeof(TextBox))
            {
                TextBox tb = (TextBox)sender;
                if (tb.BackColor != Color.White) tb.BackColor = Color.White;
            }
        }
    }
}
