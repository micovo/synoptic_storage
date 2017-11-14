using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TidyStorage.Suppliers;
using TidyStorage.Suppliers.Data;


namespace TidyStorage
{
    public partial class StorageForm : Form
    {
        StoragePart part;
        Storage storage;
        Supplier supplier;

        List<IndexedName> ManufacturerList;
        List<IndexedName> PackageList;
        List<IndexedName> PartTypeList;
        List<IndexedName> PlaceTypeList;
        List<IndexedName> SupplierList;
        
        public StorageWebImport spi;

        //Background color for textbox if it contains error
        Color ERROR_COLOR = Color.LightCoral;
        
        bool storage_closed;
        /// <summary>
        /// Storage Database Closed status. This property is used by modal forms.
        /// </summary>
        public bool StorageClosed
        {
            get
            {
                return storage_closed;
            }
        }

        /// <summary>
        /// Storage part form constructor
        /// </summary>
        /// <param name="storage">Storage SQLite database</param>
        /// <param name="part">Storage part to copy. Set null to create blank part.</param>
        /// <param name="PartName">Manufacturer part name</param>
        /// <param name="SupplierName">Name of the part supplier such as Farnell or Mouser</param>
        /// <param name="SupplierNumber">Part name or id used by the part supplier</param>
        /// <param name="StoragePlace">Place where the part is stored</param>
        /// <param name="Stock">Quantity of parts in stock</param>
        public StorageForm(Storage storage, StoragePart part, string PartName = "", string SupplierName = "", string SupplierNumber = "", string StoragePlace = "", string Stock = "")
        {
            InitializeComponent();

            this.storage = storage;

            storage_closed = false;

            if  (part == null)
            {
                this.part = new StoragePart();
            }
            else
            {
                this.part = part;
            }

            //Refresh part types, packages, etc. from the database
            RefreshComboBoxes();
            LoadPartTypeStrings();
            RefreshForm();

            //Use input values if any
            if (PartName != "") textBoxProductName.Text = PartName;
            if (SupplierNumber != "") textBoxSupplierNumber.Text = SupplierNumber;
            if (StoragePlace != "") textBoxPlaceNumber.Text = StoragePlace;

            int stock = 0;
            if ((Stock != "") && (int.TryParse(Stock, out stock))) numericUpDownStock.Value = stock;
            
            if (SupplierName != "") comboBoxSupplier.SelectedItem = SupplierList.FirstOrDefault(x => x.name == SupplierName);
        }

        /// <summary>
        /// OK button event click. Part is stored in the temporary database if all values are valid.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void buttonOk_Click(object sender, EventArgs e)
        {
            //Check if all parameters are valid
            if (ProcessFormValues())
            {
                //Store part in temporary database
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
        /// Apply button event click. Part is stored in the temporary database if all values are valid.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonApply_Click(object sender, EventArgs e)
        {
            //Check if all parameters are valid
            if (ProcessFormValues())
            {
                //Store part in temporary database
                SavePart();
                LoadPartTypeStrings();
            }
            else
            {
                System.Media.SystemSounds.Beep.Play();
            }
        }

        /// <summary>
        /// Cancel button event click. All changes will be ignored.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
        
        /// <summary>
        /// Function checks if selected supplier have implemented the web import 
        /// and if the user provided the supplier part number.
        /// </summary>
        /// <returns>True if part name and supplier web importer is available</returns>
        bool PrepareSupplier()
        {
            supplier = null;

            //Check if any supplier is selected
            if (comboBoxSupplier.SelectedIndex > -1)
            {
                if (textBoxSupplierNumber.Text.Length > 0)
                {
                    string supp = comboBoxSupplier.Text;
                    string suppnum = textBoxSupplierNumber.Text;

                    //Get supplier class from the supplier name
                    switch (supp)
                    {
                        case "Farnell": this.supplier = new FarnellSupplier(suppnum); break;
                        case "Mouser": this.supplier = new MouserSupplier(suppnum); break;
                        case "GME": this.supplier = new GMESupplier(suppnum); break;
                        case "TME": this.supplier = new TMESupplier(suppnum); break;
                        default: MessageBox.Show("Supplier is not implemented, please enter parameters manualy or contact developers.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                            return false;
                    }
                    
                    return true;
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

            return false;
        }
        
        /// <summary>
        /// Import from Web button event click. This function imports part price and
        /// parameters downloaded from website of the supplier.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void buttonImport_Click(object sender, EventArgs e)
        {         
            //Check if supplier and supplier part name is valid.
            //This function also sets this.supplier variable.
            if (PrepareSupplier())
            {
                //Open web import form
                spi = new StorageWebImport(storage, supplier, (sender is ExcelImporter));
                
                //Set units of the part type values if the part type is selected in current form
                int SelectedPartType = ProcessTypeComboBox(comboBoxPartType);
                if (SelectedPartType > -1)
                {
                    string[] s = new string[3];

                    s = storage.GetPartTypeStrings(SelectedPartType);

                    //Units are stored as a string with following format: 
                    //Parameter name [Unit], for example Resistivity [Ohm] or Capacity [F]
                    spi.PrimaryValueUnit = StringHelpers.Between(s[0], "[", "]");
                    spi.SecondaryValueUnit = StringHelpers.Between(s[1], "[", "]");
                    spi.ThirdValueUnit = StringHelpers.Between(s[2], "[", "]");
                }
                
                //Pass values from the web import to supplier part form
                if (spi.ShowDialog() == DialogResult.OK)
                {
                    string v = "";
                    string manuf = "";
                    string pack = "";

                    //Process manufacturer name provided by the web importer
                    v = ((PartRow)spi.comboBox2.SelectedItem).Value;

                    if (v != "")
                    {
                        manuf = v;

                        string cond = StorageConst.Str_Manufacturer_name + " LIKE '" + manuf.Split(' ')[0] + "%'";

                        DataTable dt = storage.GetTable(StorageConst.Str_Manufacturer, "*", cond);

                        if ((dt != null) && (dt.Rows.Count > 0))
                        {
                            var manuf_id = (int)(Int64)dt.Rows[0].ItemArray[0];

                            SelectNamedIndexComboBox(comboBoxManufacturer, manuf_id);
                        }
                        else
                        {
                            DialogResult dr;

                            if (sender == null)
                            {
                                dr = DialogResult.Yes;
                            }
                            else
                            {
                                dr = MessageBox.Show("Manufacturer \"" + manuf + "\" was not found in database." + System.Environment.NewLine + "Do you want to add new manufacturer?", "New manufacturer", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                            }

                            if (dr == DialogResult.Yes)
                            {
                                var new_id = storage.InsertNewRow(StorageConst.Str_Manufacturer, StorageConst.Str_Manufacturer_name, manuf);

                                if (new_id > -1)
                                {
                                    RefreshComboBoxes();
                                    SelectNamedIndexComboBox(comboBoxManufacturer, new_id);
                                }
                            }
                        }
                    }

                    //Process part name provided by the web importer
                    v = ((PartRow)spi.comboBox3.SelectedItem).Value;
                    if (v != "")
                    {
                        pack = v;

                        //Remove extra package information
                        int epi = pack.IndexOfAny(("({[").ToCharArray());
                        if (epi > 0) pack = pack.Substring(0, epi).Trim();

                        /*
                        List<string> keywords = new List<string>();
                        keywords.AddRange(pack.Split(' '));

                        keywords.RemoveAll(x => (x.Length < 2) || (x.Contains("SMD")));    
                        string cond = StringHelpers.LikeCondition(StorageConst.Str_Package_name, keywords.ToArray(), "OR");
                        */

                        pack = pack.Replace("SMD", "").Trim(", ".ToCharArray());

                        string cond = StorageConst.Str_Package_name + " = '" + pack + "'";
                        DataTable dt = storage.GetTable(StorageConst.Str_Package, "*", cond);

                        if ((dt != null) && (dt.Rows.Count > 0))
                        {
                            var package_id = (int)(Int64)dt.Rows[0].ItemArray[0];

                            SelectNamedIndexComboBox(comboBoxPackage, package_id);
                        }
                        else
                        {
                            DialogResult dr;

                            if (sender == null)
                            {
                                dr = DialogResult.Yes;
                            }
                            else
                            {
                                dr = MessageBox.Show("Package \"" + pack + "\" was not found in database." + System.Environment.NewLine + "Do you want to add new package?", "New package", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                            }

                            if (dr == DialogResult.Yes)
                            {
                                var new_id = storage.InsertNewRow(StorageConst.Str_Package, StorageConst.Str_Package_name, pack);

                                if (new_id > -1)
                                {
                                    RefreshComboBoxes();
                                    SelectNamedIndexComboBox(comboBoxPackage, new_id);
                                }
                            }
                        }
                    }
                    
                    //Pass values from the web import to storage form textboxes
                    v = ((PartRow)spi.comboBox1.SelectedItem).Value;
                    if (v != "") textBoxProductName.Text = v;
                    
                    v = ((PartRow)spi.comboBox4.SelectedItem).Value;
                    if (v != "") textBoxDatasheet.Text = v;

                    v = ((PartRow)spi.comboBox5.SelectedItem).Value;
                    if (v != "") textBoxComment.Text = v;

                    v = ((PartRow)spi.comboBox6.SelectedItem).Value;
                    if (v != "") textBoxPrimaryValue.Text = v;

                    v = ((PartRow)spi.comboBox7.SelectedItem).Value;
                    if (v != "") textBoxPrimaryTolerance.Text = v;

                    v = ((PartRow)spi.comboBox8.SelectedItem).Value;
                    if (v != "") textBoxSecondaryValue.Text = v;

                    v = ((PartRow)spi.comboBox9.SelectedItem).Value;
                    if (v != "") textBoxSecondaryTolerance.Text = v;

                    v = ((PartRow)spi.comboBox10.SelectedItem).Value;
                    if (v != "") textBoxThridValue.Text = v;

                    v = ((PartRow)spi.comboBox11.SelectedItem).Value;
                    if (v != "") textBoxThridTolerance.Text = v;

                    v = ((PartRow)spi.comboBox12.SelectedItem).Value;
                    if (v != "") textBoxTempRangeMin.Text = v.Replace("°C", "").Trim();

                    v = ((PartRow)spi.comboBox13.SelectedItem).Value;
                    if (v != "") textBoxTempRangeMax.Text = v.Replace("°C","").Trim();
                
                    //Prices are stored in the supplierPart variable
                    UpdatePrices(spi.supplierPart);
                    
                    if (SelectedPartType < 0)
                    {
                        SelectNamedIndexComboBox(comboBoxPartType, spi.FoundPartType);
                    }
                }
            }
        }


        /// <summary>
        /// Procedure for filling the prices textboxes from the supplier part usualy provided by the web importer.
        /// </summary>
        /// <param name="sp">Supplier part to be used as a source of prices</param>
        void UpdatePrices(SupplierPart sp)
        {
            textBoxCurrency.Text = sp.currency;

            textBoxPrice1pcs.Text = "";
            textBoxPrice10pcs.Text = "";
            textBoxPrice100pcs.Text = "";
            textBoxPrice1000pcs.Text = "";

            if ((sp.prices != null) && (sp.prices.Count > 0))
            {
                var priceobj = sp.prices.FirstOrDefault(x => (x.amount_max < 10));
                textBoxPrice1pcs.Text = (priceobj != null) ? priceobj.price.ToString() : "";

                priceobj = sp.prices.LastOrDefault(x => (x.amount_min <= 10));
                textBoxPrice10pcs.Text = (priceobj != null) ? priceobj.price.ToString() : "";
                
                priceobj = sp.prices.LastOrDefault(x => (x.amount_min <= 100));
                textBoxPrice100pcs.Text = (priceobj != null) ? priceobj.price.ToString() : "";
                
                priceobj = sp.prices.LastOrDefault(x => (x.amount_min <= 1000));
                textBoxPrice1000pcs.Text = (priceobj != null) ? priceobj.price.ToString() : "";
            }
        }
        
        /// <summary>
        /// Price check button click event handler. 
        /// This function updates price of the part via web importer.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void buttonPriceCheck_Click(object sender, EventArgs e)
        {
            if (sender != null) ((Button)sender).Enabled = false;

            if (PrepareSupplier())
            {
                LoadingForm lf = new LoadingForm();
                lf.Show();
                lf.Center(this);
                Application.DoEvents();

                SupplierPart supplierPart = supplier.DownloadPart();
                UpdatePrices(supplierPart);

                lf.UpdateProgress(100);
                lf.UpdateLabel("Done");
                Application.DoEvents();
           
                lf.AllowedToClose = true;
                lf.Close();
            }

            if (sender != null) ((Button)sender).Enabled = true;
        }

        /// <summary>
        /// Open button in Datasheet row click event handler.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonOpenDatasheet_Click(object sender, EventArgs e)
        {
            string url = textBoxDatasheet.Text;

            //Check if the link in datasheet textbox is valid URL
            if ((url.ToLower().StartsWith("http://")) || (url.ToLower().StartsWith("https://")))
            {
                System.Diagnostics.Process.Start(url);
            }
            else
            {
                //Invalid URL, display error
                textBoxDatasheet.BackColor = ERROR_COLOR;
            }
        }

        /// <summary>
        /// Datasheet TextBox change event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBoxDatasheet_TextChanged(object sender, EventArgs e)
        {
            //Clean up error color
            textBoxDatasheet.BackColor = Color.White;
        }

        /// <summary>
        /// Storage Type row Edit button click handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonEdit_Click(object sender, EventArgs e)
        {
            StorageTypeEditor spte;

            if (sender == buttonEditManuf)
            {
                spte = new StorageTypeEditor(storage, StorageTypeTables.Manufacturer);
            }
            else if (sender == buttonEditPackage)
            {
                spte = new StorageTypeEditor(storage, StorageTypeTables.Package);
            }
            else if (sender == buttonEditPlaceType)
            {
                spte = new StorageTypeEditor(storage, StorageTypeTables.PlaceType);
            }
            else if (sender == buttonEditType)
            {
                spte = new StorageTypeEditor(storage, StorageTypeTables.PartType);
            }
            else if (sender == buttonEditSuplier)
            {
                spte = new StorageTypeEditor(storage, StorageTypeTables.Supplier);
            }
            else
            {
                return;
            }
            
            DialogResult ds = spte.ShowDialog();

            //Refresh sync possible changes made in the type editor
            RefreshComboBoxes();
            LoadPartTypeStrings();
        }
        
        /// <summary>
        /// Function loads part type parameters or default text
        /// </summary>
        void LoadPartTypeStrings()
        {
            int i = ProcessTypeComboBox(comboBoxPartType);

            string[] s = new string[3];

            s = storage.GetPartTypeStrings(i);

            if ((s[0] == null) || (s[0].Length == 0)) s[0] = "Primary Value";
            if ((s[1] == null) || (s[1].Length == 0)) s[1] = "Secondary Value";
            if ((s[2] == null) || (s[2].Length == 0)) s[2] = "Third Value";

            if (s[0].Length > 17) s[0] = s[0].Substring(0, 17);
            if (s[1].Length > 17) s[1] = s[1].Substring(0, 17);
            if (s[2].Length > 17) s[2] = s[2].Substring(0, 17);

            labelValuePrimary.Text = s[0];
            labelValueSecondary.Text = s[1];
            labelValueThird.Text = s[2];

            textBoxThridValue.Enabled = (s[2] != "");
            textBoxThridTolerance.Enabled = (s[2] != "");
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
        /// Function for uniform conversion of the part price to text string.
        /// </summary>
        /// <param name="p">Value to be converted to string</param>
        /// <returns>Converted string</returns>
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
        /// Function for uniform conversion of the part tolerance value to text string.
        /// </summary>
        /// <param name="primary_tolerance">Value to be converted to string</param>
        /// <returns>Converted string</returns>
        private string GetPartToleranceString(double p)
        {
            if (double.IsNaN(p) == false)
            {
                return p.ToString();
            }
            return "";
        }

        /// <summary>
        /// Function for uniform conversion of the part value to text string.
        /// </summary>
        /// <param name="primary_value">Value to be converted to string</param>
        /// <returns>Converted string</returns>
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
        /// Manufacturer combobox selection change event handler.
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
        /// Function for checking strings from textboxes.
        /// Error color background is set if the textbox text is invalid.
        /// </summary>
        /// <param name="tb">TextBox to be checked</param>
        /// <param name="output">Output string.</param>
        /// <returns>True if TextBox text is valid.</returns>
        bool ProcessTextBox(TextBox tb, out string output)
        {
            output = "";
            string o = tb.Text;            

            //Check forbidden characters
            if ((tb.Text.Contains('"')) || (tb.Text.Contains('\'')))
            {
                tb.BackColor = ERROR_COLOR;
                return false;
            }

            output = o;
            return true;
        }

        /// <summary>
        /// Function for parsing integer strings from textboxes.
        /// Error color background is set if the textbox text is invalid.
        /// </summary>
        /// <param name="tb">TextBox to be checked</param>
        /// <param name="output">Output integer value.</param>
        /// <returns>True if TextBox text is valid.</returns>
        bool ProcessTextBox(TextBox tb, out int output)
        {
            output = 0;
            int o = 0;
            string str = tb.Text;


            if (int.TryParse(str, out o) == false)
            {
                tb.BackColor = ERROR_COLOR;
                return false;
            }

            output = o;
            return true;
        }

        /// <summary>
        /// Function for parsing double strings from textboxes.
        /// Error color background is set if the textbox text is invalid.
        /// </summary>
        /// <param name="tb">TextBox to be checked</param>
        /// <param name="output">Output double value.</param>
        /// <returns>True if TextBox text is valid.</returns>
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

            tb.BackColor = ERROR_COLOR;
            return false;
        }

        /// <summary>
        /// Function for parsing currency strings from textboxes.
        /// Error color background is set if the textbox text is invalid.
        /// </summary>
        /// <param name="tb">TextBox to be checked</param>
        /// <param name="output">Output currency value.</param>
        /// <returns>True if TextBox text is valid.</returns>
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

            tb.BackColor = ERROR_COLOR;
            return false;
        }
        
        /// <summary>
        /// Function for safe ComboBox processing.
        /// </summary>
        /// <param name="cb">ComboBox to be processed</param>
        /// <returns>Selected item index</returns>
        int ProcessTypeComboBox(ComboBox cb)
        {
            int i = -1;
            object o = cb.SelectedItem;
            if (o != null) i = ((IndexedName)o).id;
            return i;
        }

        /// <summary>
        /// Function for selecting ComboBox item by its database id.
        /// </summary>
        /// <param name="cb">ComboBox to be processed</param>
        /// <param name="index">Database index to be selected</param>
        void SelectNamedIndexComboBox(ComboBox cb, int index)
        {
            if (index != -1)
            {
                for (int x = 0; x < cb.Items.Count; x++)
                {
                    var ob = (IndexedName)cb.Items[x];

                    if (ob.id == index)
                    {
                        cb.SelectedItem = ob;
                        break;
                    }
                }
            }
        }
        
        /// <summary>
        /// Handling user input for Part name TextBox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBoxProductName_TextChanged(object sender, EventArgs e)
        {
            //Check if the event is caused by app itself or user input
            if (sender.GetType() == typeof(TextBox))
            {
                TextBox tb = (TextBox)sender;
                if (tb.BackColor != Color.White) tb.BackColor = Color.White;
            }
        }

        /// <summary>
        /// Open button in Supplier Part Number row.
        /// Click on this button opens supplier website for the part.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonSupplierOpen_Click(object sender, EventArgs e)
        {
            if (PrepareSupplier())
            {
                string url = supplier.GetLink();

                if (url != "")
                {
                    System.Diagnostics.Process.Start(url);
                }
                else
                {
                    MessageBox.Show("Importer or URL formatter is not available for this supplier.\r\nRequest importer or URL formatter on TidiStorage github.", "Not supported", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }
        }
        
        /// <summary>
        /// Storage Part Form OnLoad event handler.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StoragePartForm_Load(object sender, EventArgs e)
        {
            textBoxProductName.Focus();
        }
        
        /// <summary>
        /// Storage part form close event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StorageForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            storage_closed = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void comboBoxPartType_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox s = (ComboBox)sender;
            if (s.SelectedIndex > -1)
            {
                this.part.id_part_type = ((IndexedName)s.SelectedItem).id;
                LoadPartTypeStrings();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void numericUpDownStock_KeyPress(object sender, KeyPressEventArgs e)
        {
            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// Supplier ComboBox seleted index change event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void comboBoxSupplier_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox s = (ComboBox)sender;
            if (s.SelectedIndex > -1)
            {
                this.part.id_supplier = ((IndexedName)s.SelectedItem).id;
            }
        }

        /// <summary>
        /// Manufacturer ComboBox seleted index change event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void comboBoxManufacturer_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox s = (ComboBox)sender;
            if (s.SelectedIndex > -1)
            {
                this.part.id_manufacturer = ((IndexedName)s.SelectedItem).id;
            }
        }

        /// <summary>
        /// Part package ComboBox seleted index change event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void comboBoxPackage_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox s = (ComboBox)sender;
            if (s.SelectedIndex > -1)
            {
                this.part.id_part_package = ((IndexedName)s.SelectedItem).id;
            }
        }

        /// <summary>
        /// Part place type ComboBox seleted index change event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void comboBoxPlaceType_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox s = (ComboBox)sender;
            if (s.SelectedIndex > -1)
            {
                this.part.id_storage_place = ((IndexedName)s.SelectedItem).id;
            }
        }

        /// <summary>
        /// Set position of this form in the center of the requested form
        /// </summary>
        /// <param name="form"></param>
        public void Center(Form form)
        {
            this.Left = form.Left + form.Width / 2 - this.Width / 2;
            this.Top = form.Top + form.Height / 2 - this.Height / 2;
        }
    }
}
