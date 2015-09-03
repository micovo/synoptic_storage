using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace TidyStorage
{
    public class StoragePart
    {

        private DataTable dt;

        public int id_part;

        public int id_manufacturer;
        public int id_storage_place;
        public int id_part_package;
        public int id_part_type;

        public int id_supplier;


        public string productnumber;
        public string suppliernumber;
        public string comment;
        public string datasheet_url;
        public string currency;


        public int temperature_from;
        public int temperature_to;
        public int storage_place_number;
        public int stock;

        public double primary_value;
        public double primary_tolerance;
        public double secondary_value;
        public double secondary_tolerance;
        public double tertiary_value;
        public double tertiary_tolerance;
        public double price_1pcs;
        public double price_10pcs;
        public double price_100pcs;
        public double price_1000pcs;

        


        /// <summary>
        /// 
        /// </summary>
        public StoragePart()
        {
            this.id_manufacturer = -1;
            this.id_storage_place = -1;
            this.id_part_type = -1;
            this.id_part_package = -1;
            this.id_supplier = -1;

            dt = null;

            
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="colname"></param>
        /// <returns></returns>
        public int GetColumnIndex(DataTable dt, string colname)
        {
            var c = dt.Columns[colname];
            if (c != null)
            {
                return c.Ordinal;
            }
            return -1;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="dr"></param>
        /// <param name="colname"></param>
        /// <returns></returns>
        public int GetCellINTEGER(DataTable dt, DataRow dr, string colname)
        {
            int x = GetColumnIndex(dt, colname);

            if (x > -1)
            {
                var y = dr.ItemArray[x];
                if (y.GetType() == typeof(Int64))
                {
                    return (int)(Int64)y;
                }
            }

            return -1;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="dr"></param>
        /// <param name="colname"></param>
        /// <returns></returns>
        public string GetCellVARCHAR(DataTable dt, DataRow dr, string colname)
        {
            int x = GetColumnIndex(dt, colname);

            if (x > -1)
            {
                var y = dr.ItemArray[x];
                if (y.GetType() == typeof(string))
                {
                    return (string)y;
                }
            }

            return "";
            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="dr"></param>
        /// <param name="colname"></param>
        /// <returns></returns>
        public double GetCellREAL(DataTable dt, DataRow dr, string colname)
        {
            int x = GetColumnIndex(dt, colname);

            if (x > -1)
            {
                var y = dr.ItemArray[x];
                if (y.GetType() == typeof(double))
                {
                    return (double)y;
                }
            }

            return double.NaN;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="dt"></param>
        public StoragePart(DataTable dt)
        {
            this.dt = dt;

            if (dt.Rows.Count > 0)
            {
                DataRow dr = dt.Rows[0];

                this.id_part = GetCellINTEGER(dt, dr, StorageConst.Str_Part_id);
                this.id_manufacturer = GetCellINTEGER(dt, dr, StorageConst.Str_Manufacturer_id);
                this.id_part_type = GetCellINTEGER(dt, dr, StorageConst.Str_PartType_id);
                this.id_part_package = GetCellINTEGER(dt, dr, StorageConst.Str_Package_id);
                this.id_storage_place = GetCellINTEGER(dt, dr, StorageConst.Str_PlaceType_id);
                this.id_supplier = GetCellINTEGER(dt, dr, StorageConst.Str_Supplier_id);

                this.temperature_from = GetCellINTEGER(dt, dr, "temperature_from");
                this.temperature_to = GetCellINTEGER(dt, dr, "temperature_to");
                this.storage_place_number = GetCellINTEGER(dt, dr, "storage_place_number");

                this.stock = GetCellINTEGER(dt, dr, "stock");
                if (this.stock < 0) this.stock = 0;

                this.productnumber = GetCellVARCHAR(dt, dr, "productnumber");
                this.suppliernumber = GetCellVARCHAR(dt, dr, "suppliernumber");
                this.comment = GetCellVARCHAR(dt, dr, "comment");
                this.datasheet_url = GetCellVARCHAR(dt, dr, "datasheet_url");
                this.currency = GetCellVARCHAR(dt, dr, "currency");

                this.primary_value = GetCellREAL(dt, dr, "primary_value");
                this.primary_tolerance = GetCellREAL(dt, dr, "primary_tolerance");
                this.secondary_value = GetCellREAL(dt, dr, "secondary_value");
                this.secondary_tolerance = GetCellREAL(dt, dr, "secondary_tolerance");
                this.tertiary_value = GetCellREAL(dt, dr, "tertiary_value");
                this.tertiary_tolerance = GetCellREAL(dt, dr, "tertiary_tolerance");

                this.price_1pcs = GetCellREAL(dt, dr, "price_1pcs");
                this.price_10pcs = GetCellREAL(dt, dr, "price_10pcs");
                this.price_100pcs = GetCellREAL(dt, dr, "price_100pcs");
                this.price_1000pcs = GetCellREAL(dt, dr, "price_1000pcs");
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string GetUpdateString()
        {
            string o = "";


            Type t = typeof(StoragePart);

            StringBuilder sb = new StringBuilder();
            
            foreach(System.Reflection.FieldInfo fi in t.GetFields())
            {
                sb.Append("`" + fi.Name + "`=");

                if (fi.FieldType == typeof(int))
                {
                    int val = (int)fi.GetValue(this);
                    sb.Append(val.ToString());
                }
                else if (fi.FieldType == typeof(string))
                {
                    string val = (string)fi.GetValue(this);
                    if (val == null) val = "";
                    val = val.Replace('"', ' ');
                    val = val.Replace('\'', ' ');

                    sb.Append("'" + val.ToString() + "'");
                }
                else if (fi.FieldType == typeof(double))
                {
                    double val = (double)fi.GetValue(this);

                    if (double.IsNaN(val) == false)
                    {
                        sb.Append(val.ToString().Replace(',', '.'));
                    }
                    else
                    {
                        sb.Append("NULL");
                    }
                }
                else
                {
                    throw new NotImplementedException();
                }

                sb.Append(",");
            }

            o = sb.ToString();
            o = o.TrimEnd(',');

            return o;
        }


    }
}
