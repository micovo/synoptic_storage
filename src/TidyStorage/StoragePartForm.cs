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
            LoadPartTypeStrings();
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
            if (comboBoxSupplier.SelectedIndex > -1)
            {
                if (textBoxSupplierNumber.Text.Length > 0)
                {
                    string supp = comboBoxSupplier.Text;
                    string suppnum = textBoxSupplierNumber.Text;

                    StoragePartImporter spi = new StoragePartImporter(supp, suppnum);
                    if (spi.ShowDialog() == DialogResult.OK)
                    {
                        string manuf = "";
                        string pack = "";

                        if (spi.comboBox1.Text != "") textBoxProductName.Text = spi.comboBox1.Text;
                        if (spi.comboBox2.Text != "") manuf = spi.comboBox2.Text;
                        if (spi.comboBox3.Text != "") pack = spi.comboBox3.Text;
                        if (spi.comboBox4.Text != "") textBoxDatasheet.Text = spi.comboBox4.Text;
                        if (spi.comboBox5.Text != "") textBoxComment.Text = spi.comboBox5.Text;
                        if (spi.comboBox6.Text != "") textBoxPrimaryValue.Text = spi.comboBox6.Text;
                        if (spi.comboBox7.Text != "") textBoxPrimaryTolerance.Text = spi.comboBox7.Text;
                        if (spi.comboBox8.Text != "") textBoxSecondaryValue.Text = spi.comboBox8.Text;
                        if (spi.comboBox9.Text != "") textBoxSecondaryTolerance.Text = spi.comboBox9.Text;
                        if (spi.comboBox10.Text != "") textBoxThridValue.Text = spi.comboBox10.Text;
                        if (spi.comboBox11.Text != "") textBoxThridTolerance.Text = spi.comboBox11.Text;
                        if (spi.comboBox12.Text != "") textBoxTempRangeMin.Text = spi.comboBox12.Text;
                        if (spi.comboBox13.Text != "") textBoxTempRangeMax.Text = spi.comboBox13.Text;


                    }
                }
                else
                {
                    MessageBox.Show("Please enter supplier number first", "Import from Web", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                }
            }
            else
            {
                MessageBox.Show("Please choose supplier first", "Import from Web", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
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
                System.Diagnostics.Process.Start(url);
            }
            else
            {
                textBoxDatasheet.BackColor = Color.LightCoral;
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
            LoadPartTypeStrings();
        }



        /// <summary>
        /// 
        /// </summary>
        void LoadPartTypeStrings()
        {
            int i = ProcessTypeComboBox(comboBoxPartType);

            string[] s = new string[3];

            s = storage.GetPartTypeStrings(i);

            if (s[0] == null) s[0] = "Primary";
            if (s[1] == null) s[1] = "Secondary";
            if (s[2] == null) s[2] = "Third";

            if (s[0].Length > 17) s[0] = s[0].Substring(0, 17);
            if (s[1].Length > 17) s[1] = s[1].Substring(0, 17);
            if (s[2].Length > 17) s[2] = s[2].Substring(0, 17);

            labelValuePrimary.Text = s[0];
            labelValueSecondary.Text = s[1];
            labelValueThird.Text = s[2];

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
        /// <param name="primary_tolerance"></param>
        /// <returns></returns>
        private string GetPartToleranceString(double p)
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
        /// <param name="primary_value"></param>
        /// <returns></returns>
        private string GetPartValueString(double p)
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonSupplierOpen_Click(object sender, EventArgs e)
        {
            string url = "";
            //TODO get supplier link

           
            if (url != "")
            {
                System.Diagnostics.Process.Start(url);
            }
            else
            {
                MessageBox.Show("Importer or URL formatter is not available for this supplier.\r\nRequest importer or URL formatter on TidiStorage github.", "Not supported", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StoragePartForm_Load(object sender, EventArgs e)
        {
            textBoxProductName.Focus();
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void comboBoxPartType_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadPartTypeStrings();
        }

        private void numericUpDownStock_KeyPress(object sender, KeyPressEventArgs e)
        {
            
        }

        private void numericUpDownStock_KeyDown(object sender, KeyEventArgs e)
        {
            //if (e.KeyChar == '+')
            if (e.KeyCode == Keys.Add)
            {
                numericUpDownStock.Value++;
                e.Handled = true;
                e.SuppressKeyPress = true;
            }

            //if (e.KeyChar == '-')
            if (e.KeyCode == Keys.Subtract)
            {
                if (numericUpDownStock.Value > 0) numericUpDownStock.Value--;
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
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

            if (ProcessTextBox(textBoxPlaceNumber, out s)) this.part.storage_place_number = s; else return false;

            if (ProcessTextBox(textBoxPrimaryValue, out s)) this.part.primary_value = s; else return false;
            if (ProcessTextBox(textBoxPrimaryTolerance, out s)) this.part.primary_tolerance = s; else return false;
            if (ProcessTextBox(textBoxSecondaryValue, out s)) this.part.secondary_value = s; else return false;
            if (ProcessTextBox(textBoxSecondaryTolerance, out s)) this.part.secondary_tolerance = s; else return false;
            if (ProcessTextBox(textBoxThridValue, out s)) this.part.tertiary_value = s; else return false;
            if (ProcessTextBox(textBoxThridTolerance, out s)) this.part.tertiary_tolerance = s; else return false;


            //Int textboxes
            if (ProcessTextBox(textBoxTempRangeMin, out i)) this.part.temperature_from = i; else return false;
            if (ProcessTextBox(textBoxTempRangeMax, out i)) this.part.temperature_to = i; else return false;


            //Float textboxes
            /*
            if (ProcessTextBox(textBoxPrimaryValue, out d)) this.part.primary_value = d; else return false;
            if (ProcessTextBox(textBoxPrimaryTolerance, out d)) this.part.primary_tolerance = d; else return false;
            if (ProcessTextBox(textBoxSecondaryValue, out d)) this.part.secondary_value = d; else return false;
            if (ProcessTextBox(textBoxSecondaryTolerance, out d)) this.part.secondary_tolerance = d; else return false;
            if (ProcessTextBox(textBoxThridValue, out d)) this.part.tertiary_value = d; else return false;
            if (ProcessTextBox(textBoxThridTolerance, out d)) this.part.tertiary_tolerance = d; else return false;
            */

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

            this.part.stock = (int)numericUpDownStock.Value;




            return true;
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
            textBoxPlaceNumber.Text = this.part.storage_place_number;
            textBoxSupplierNumber.Text = this.part.suppliernumber;


            textBoxTempRangeMin.Text = this.part.temperature_from.ToString();
            textBoxTempRangeMax.Text = this.part.temperature_to.ToString();

            numericUpDownStock.Value = this.part.stock;

            textBoxPrice1pcs.Text = GetPartPriceString(this.part.price_1pcs);
            textBoxPrice10pcs.Text = GetPartPriceString(this.part.price_10pcs);
            textBoxPrice100pcs.Text = GetPartPriceString(this.part.price_100pcs);
            textBoxPrice1000pcs.Text = GetPartPriceString(this.part.price_1000pcs);

            /*
            textBoxPrimaryValue.Text = GetPartValueString(this.part.primary_value);
            textBoxSecondaryValue.Text = GetPartValueString(this.part.secondary_value);
            textBoxThridValue.Text = GetPartValueString(this.part.tertiary_value);


            textBoxPrimaryTolerance.Text = GetPartToleranceString(this.part.primary_tolerance);
            textBoxSecondaryTolerance.Text = GetPartToleranceString(this.part.secondary_tolerance);
            textBoxThridTolerance.Text = GetPartToleranceString(this.part.tertiary_tolerance);
            */


            textBoxPrimaryValue.Text = this.part.primary_value;
            textBoxSecondaryValue.Text = this.part.secondary_value;
            textBoxThridValue.Text = this.part.tertiary_value;


            textBoxPrimaryTolerance.Text = this.part.primary_tolerance;
            textBoxSecondaryTolerance.Text = this.part.secondary_tolerance;
            textBoxThridTolerance.Text = this.part.tertiary_tolerance;

        }

    }
}
