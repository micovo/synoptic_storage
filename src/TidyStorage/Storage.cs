using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Data.SQLite;
using System.Windows.Forms;
using System.Data;
using System.Threading;

namespace TidyStorage
{
    public class IndexedName
    {
        public int id;
        public string name;

        public IndexedName(int id, string name)
        {
            this.id = id;
            this.name = name;
        }

        public override string ToString()
        {
            return name;
        }
    }


    public class Storage
    {
        private string workfile;

        private string filename;
        public string Filename
        {
            get
            {
                return filename;
            }
        }



        bool changeCommited;
        public bool ChangeCommited
        {
            get
            {
                 return changeCommited;
            }
        }
        


        /// <summary>
        /// Destructor
        /// </summary>
        ~Storage()
        {
           //TODO: Delete database temporary file
        }


        public void Save(string target_filename = "")
        {
            if (target_filename == "")
            {
                target_filename = this.filename;
            }

            File.Copy(this.workfile, target_filename, true);
            changeCommited = false;
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="ex"></param>
        private void DatabaseError(string sql, Exception ex)
        {
            if (sql.Length > 512) sql = sql.Substring(0, 512);
            MessageBox.Show("Failed to commit SQL query:\r\n" + sql + "\r\n\r\n" + ex.Message + "\r\n\r\n" + ex.StackTrace, "Storage database error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }


        /// <summary>
        /// Checks if file exist, new file is created if not. 
        /// Constructor also checks DB tables by comparing SQL create commands stored in SQLite master table.
        /// </summary>
        /// <param name="filename">SQLite database file</param>
        public Storage(string filename)
        {
            this.filename = filename;
            this.changeCommited = false;

            workfile = filename + ".tmp";

            string[] tables = { StorageConst.sql_createtable_storage_info ,
                        StorageConst.sql_createtable_part,
                        StorageConst.sql_createtable_part_type,
                        StorageConst.sql_createtable_part_package,
                        StorageConst.sql_createtable_manufacturer,
                        StorageConst.sql_createtable_storage_place,
                        StorageConst.sql_createtable_supplier};

            if (File.Exists(filename) == false)
            {
                //If database file do not exist
                changeCommited = true;

                //Create new database file
                SQLiteConnection.CreateFile(workfile);

                using (SQLiteConnection con = new SQLiteConnection(string.Format(StorageConst.sqlite_connection_str, filename)))
                {
                    con.Open();

                    string sql;
                    SQLiteCommand command;

                    foreach (string s in tables)
                    {
                        sql = s;
                        command = new SQLiteCommand(sql, con);
                        command.ExecuteNonQuery();
                    }

                    sql = "INSERT INTO storage_info (parameter, value) VALUES ('version','" + StorageConst.sqlite_version + "')";
                    command = new SQLiteCommand(sql, con);
                    command.ExecuteNonQuery();


                    con.Close();
                }

                GC.Collect();
            }


            File.Copy(filename, workfile, true);


            bool StorageVersionVerified = false;

            //Check database content
            using (SQLiteConnection con = new SQLiteConnection(string.Format(StorageConst.sqlite_connection_str, workfile)))
            {
                con.Open();

                var sql = @"SELECT value FROM storage_info WHERE parameter = 'version'";
                var command = new SQLiteCommand(sql, con);

                using (SQLiteCommand cmd = new SQLiteCommand(sql, con))
                {
                    using (SQLiteDataReader rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            if (rdr.GetString(0) == StorageConst.sqlite_version)
                            {
                                StorageVersionVerified = true;
                            }
                        }
                    }
                }

                con.Close();
            }

            GC.Collect();


            if (StorageVersionVerified)
            {
                //Everything is OK
                //Nothing to do actualy
            }
            else
            {
                throw new Exception("TidyStorage datafile corrupted");
            }
        }








        /// <summary>
        /// 
        /// </summary>
        /// <param name="table"></param>
        /// <param name="columns"></param>
        /// <param name="where_condition"></param>
        /// <returns></returns>
        public DataTable GetTable(string table, string columns = "*", string where_condition = "1", string order_by = "")
        {
            DataTable tab = null;

            try
            {
                using (SQLiteConnection con = new SQLiteConnection(string.Format(StorageConst.sqlite_connection_str, workfile)))
                {
                    con.Open();

                    var sql = String.Format(@"SELECT {0} FROM {1} WHERE {2} {3} {4}", 
                        columns, 
                        table, 
                        where_condition, 
                        (order_by != "") ? "ORDER BY" : "",
                        order_by);
                    var cmd = new SQLiteCommand(sql, con);


                    using (SQLiteDataReader rdr = cmd.ExecuteReader())
                    {
                        var mAdapter = new SQLiteDataAdapter(sql, con);
                        tab = new DataTable(); // Don't forget initialize!
                        mAdapter.Fill(tab);
                    }

                    con.Close();
                }

                GC.Collect();
            }
            catch (Exception ex)
            {
                string s = ex.Message;
            }

            return tab;
        }

        

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public DataTable GetPartTable(string where = "1" )
        {
            DataTable tab = null;

            try
            {
                using (SQLiteConnection con = new SQLiteConnection(string.Format(StorageConst.sqlite_connection_str, workfile)))
                {
                    con.Open();

                    var sql = @"SELECT 
`id_part`,
	`productnumber`,
	manufacturer.manufacturername AS manufacturername,
	part_type.typename AS typename,
	part_package.packagename AS packagename,
	`stock`	 ,
	storage_place.placename AS placename,
	`storage_place_number`,
	`primary_value`	,
	`primary_tolerance`	,
	`secondary_value`	,
	`tertiary_value`	,
	`temperature_from`,
	`temperature_to`,
	supplier.suppliername AS suppliername,
	`suppliernumber`,
	`price_1pcs`	,
	`price_10pcs`	,
	`price_100pcs`	,
	`price_1000pcs`	,
currency
	
	FROM `part` 
	LEFT JOIN manufacturer ON manufacturer.id_manufacturer = part.id_manufacturer
	LEFT JOIN part_package ON part_package.id_part_package = part.id_part_package
	LEFT JOIN part_type ON part_type.id_part_type = part.id_part_type
	LEFT JOIN supplier ON supplier.id_supplier = part.id_supplier
	LEFT JOIN storage_place ON storage_place.id_storage_place = part.id_storage_place

    WHERE " + where;

                    var cmd = new SQLiteCommand(sql, con);


                    using (SQLiteDataReader rdr = cmd.ExecuteReader())
                    {
                        var mAdapter = new SQLiteDataAdapter(sql, con);
                        tab = new DataTable(); // Don't forget initialize!
                        mAdapter.Fill(tab);
                    }

                    con.Close();
                }

                GC.Collect();
            }
            catch (Exception ex)
            {
                string s = ex.Message;
            }

            return tab;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public string[] GetPartTypeStrings(int i)
        {
            string[] o = new string[3];

            string cond = string.Format("{0}={1}", StorageConst.Str_PartType_id, i);
            DataTable dt = GetTable(StorageConst.Str_PartType, "primary_valuename,secondary_valuename,tertiary_valuename", cond);
            
            if ((dt != null) && (dt.Rows.Count > 0))
            {
                DataRow dr = dt.Rows[0];
                o[0] = (dr.ItemArray[0].GetType() == typeof(System.DBNull)) ? "" : (string)dr.ItemArray[0];
                o[1] = (dr.ItemArray[1].GetType() == typeof(System.DBNull)) ? "" : (string)dr.ItemArray[1];
                o[2] = (dr.ItemArray[2].GetType() == typeof(System.DBNull)) ? "" : (string)dr.ItemArray[2];
            }

            return o;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="table"></param>
        /// <param name="columns"></param>
        /// <param name="where_condition"></param>
        public int InsertIntoTable(string table, string columns, string values)
        {
            DataTable tab = null;

            int new_id = -1;
            

            try
            {
                using (SQLiteConnection con = new SQLiteConnection(string.Format(StorageConst.sqlite_connection_str, workfile)))
                {
                    con.Open();

                    var sql = String.Format(@"INSERT INTO {0} ({1}) VALUES ({2})", table, columns, values);
                    var cmd = new SQLiteCommand(sql, con);

                    cmd.ExecuteNonQuery();
                    changeCommited = true;

                    sql = String.Format(@"SELECT last_insert_rowid()");
                    cmd = new SQLiteCommand(sql, con);


                    using (SQLiteDataReader rdr = cmd.ExecuteReader())
                    {
                        if (rdr.Read())
                        {
                            new_id = (int)rdr.GetInt64(0);
                        }
                    }

                    con.Close();
                }

                GC.Collect();
            }
            catch (Exception ex)
            {
                string s = ex.Message;
            }

            return new_id;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="table"></param>
        /// <param name="columns"></param>
        /// <param name="where_condition"></param>
        public void UpdateTable(string table, string columns, string where_condition)
        {
            DataTable tab = null;
            string sql = "";
            changeCommited = true;

            using (SQLiteConnection con = new SQLiteConnection(string.Format(StorageConst.sqlite_connection_str, workfile)))
            {
                try
                {
                    con.Open();

                    sql = String.Format(@"UPDATE {0} SET {1} WHERE {2}", table, columns, where_condition);
                    var cmd = new SQLiteCommand(sql, con);

                    cmd.ExecuteNonQuery();
                    changeCommited = true;
                }
                catch (SQLiteException ex)
                {
                    DatabaseError(sql, ex);
                }

                con.Close();
            }
            


            GC.Collect();

        }

        


        /// <summary>
        /// 
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public List<IndexedName> GetStringIdArray(StorageTypeTables t)
        {
            string table = "";
            string columns = "";
            string namecolumn = "";

            switch (t)
            {
                case StorageTypeTables.Manufacturer:
                    table = StorageConst.Str_Manufacturer;
                    columns = StorageConst.Str_Manufacturer_id + "," + StorageConst.Str_Manufacturer_name;
                    namecolumn = StorageConst.Str_Manufacturer_name;
                    break;
                case StorageTypeTables.PartType:
                    table = StorageConst.Str_PartType;
                    columns = StorageConst.Str_PartType_id + "," + StorageConst.Str_PartType_name;
                    namecolumn = StorageConst.Str_PartType_name;
                    break;
                case StorageTypeTables.Package:
                    table = StorageConst.Str_Package;
                    columns = StorageConst.Str_Package_id + "," + StorageConst.Str_Package_name;
                    namecolumn = StorageConst.Str_Package_name;
                    break;
                case StorageTypeTables.PlaceType:
                    table = StorageConst.Str_PlaceType;
                    columns = StorageConst.Str_PlaceType_id + "," + StorageConst.Str_PlaceType_name;
                    namecolumn = StorageConst.Str_PlaceType_name;
                    break;
                case StorageTypeTables.Supplier:
                    table = StorageConst.Str_Supplier;
                    columns = StorageConst.Str_Supplier_id + "," + StorageConst.Str_Supplier_name;
                    namecolumn = StorageConst.Str_Supplier_name;
                    break;
            }

            DataTable dt = GetTable(table, columns, "1", namecolumn);
            List<IndexedName> lst = new List<IndexedName>();

            foreach (DataRow dr in dt.Rows)
            {
                if (dr.ItemArray.Length >= 2)
                {
                    var id = dr.ItemArray[0];
                    var name = dr.ItemArray[1];
                    if ((id.GetType() == typeof(Int64)) && (name.GetType() == typeof(string)))
                    {
                        lst.Add(new IndexedName((int)(Int64)id, (string)name));
                    }
                }
            }

            return lst;
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="table"></param>
        /// <param name="id_column_name"></param>
        /// <param name="tab"></param>
        /// <param name="err"></param>
        /// <returns></returns>
        public bool SaveTypeTable(string table, string id_column_name, DataTable tab, out string err)
        {
            err = "";

            List<DataRow> ToUpdate = new List<DataRow>();

            DataTable dt = GetTable(table);

            DataColumn[] columns = new DataColumn[1];
            columns[0] = tab.Columns[0];
            tab.PrimaryKey = columns;

           


            foreach (DataRow dr in dt.Rows)
            {
                bool change_found = false;

                int id = int.Parse(dr.ItemArray[0].ToString());

                DataRow drr = tab.Rows.Find(id);

                int strings_to_compare = dr.ItemArray.Length - 1;

                if (drr != null)
                {
                    //Not deleted, check for content changes
                    for (int i = 1; i < strings_to_compare + 1; i++)
                    {
                        if (dr.ItemArray[i].GetType() != typeof(System.DBNull))
                        {
                            if (dr.ItemArray[i].GetType() == typeof(string))
                            {
                                if ((string)drr.ItemArray[i] != (string)dr.ItemArray[i])
                                {
                                    change_found = true;
                                }
                            }
                        }
                        else
                        {
                            change_found = true;
                        }
                    }
                }

                //Check for read only 
                if (dr.ItemArray.Last().GetType() == typeof(Int64))
                {
                    if ((Int64)dr.ItemArray.Last() == 1)
                    {
                        change_found = false;
                    }
                }


                if (change_found)
                {
                    ToUpdate.Add(drr);
                }
            }

            if (ToUpdate.Count > 0)
            {
                using (SQLiteConnection con = new SQLiteConnection(string.Format(StorageConst.sqlite_connection_str, workfile)))
                {
                    string sql = "";

                    try
                    {
                        con.Open();

                        foreach (DataRow dr in ToUpdate)
                        {
                            int id = (int)(Int64)dr.ItemArray[0];

                            string update_query = "";
                            for (int i = 1; i < tab.Columns.Count; i++)
                            {
                                if (dr.ItemArray[i].GetType() != typeof(System.DBNull))
                                {
                                    update_query += tab.Columns[i].Caption + "=\"" + (string)dr.ItemArray[i] + "\",";
                                }
                                else
                                {
                                    update_query += tab.Columns[i].Caption + "=NULL,";
                                }
                            }

                            update_query = update_query.Trim(',');


                            sql = string.Format("UPDATE {0} SET {1} WHERE {2} = {3}", table, update_query, id_column_name, id);
                            var command = new SQLiteCommand(sql, con);
                            command.ExecuteNonQuery();
                            changeCommited = true;
                        }
                    }
                    catch (SQLiteException ex)
                    {
                        DatabaseError(sql, ex);
                    }

                    con.Close();
                }
            }


            return true;
        }




        


        /// <summary>
        /// 
        /// </summary>
        /// <param name="table"></param>
        /// <param name="namecolumn"></param>
        public int InsertNewRow (string table, string namecolumn, string name = "###### Enter new #####")
        {
            int new_id = -1;

            using (SQLiteConnection con = new SQLiteConnection(string.Format(StorageConst.sqlite_connection_str, workfile)))
            {
                string sql = "";

                try
                {
                    con.Open();


                    sql = string.Format("INSERT INTO {0} ({1}) VALUES ('" + name + "')", table, namecolumn);
                    var cmd = new SQLiteCommand(sql, con);
                    cmd.ExecuteNonQuery();
                    changeCommited = true;

                    sql = String.Format(@"SELECT last_insert_rowid()");
                    cmd = new SQLiteCommand(sql, con);


                    using (SQLiteDataReader rdr = cmd.ExecuteReader())
                    {
                        if (rdr.Read())
                        {
                            new_id = (int)rdr.GetInt64(0);
                        }
                    }

                }
                catch (SQLiteException ex)
                {
                    DatabaseError(sql, ex);
                }

            con.Close();
            }

            GC.Collect();

            return new_id;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="table"></param>
        /// <param name="id_column"></param>
        /// <param name="id"></param>
        public void DeleteRow(string table, string id_column, int id)
        {
            string sql = "";

            using (SQLiteConnection con = new SQLiteConnection(string.Format(StorageConst.sqlite_connection_str, workfile)))
            {
                try
                {
                    con.Open();

                    sql = string.Format("DELETE FROM {0} WHERE ({1} = {2})", table, id_column, id);
                    var command = new SQLiteCommand(sql, con);
                    command.ExecuteNonQuery();
                    changeCommited = true;

                }
                catch (SQLiteException ex)
                {
                    DatabaseError(sql, ex);
                }

            con.Close();
            }

            GC.Collect();
        }




        /// <summary>
        /// 
        /// </summary>
        /// <param name="column_name"></param>
        /// <returns></returns>
        public bool ColumnValueIsInUse(string column_name, int column_value)
        {
            int count = 0;

            //Check database content
            using (SQLiteConnection con = new SQLiteConnection(string.Format(StorageConst.sqlite_connection_str, workfile)))
            {
                string sql = "";

                try
                {
                    con.Open();

                    sql = string.Format(@"SELECT COUNT(1) FROM part WHERE {0} = {1}", column_name, column_value);
                    var command = new SQLiteCommand(sql, con);

                    using (SQLiteCommand cmd = new SQLiteCommand(sql, con))
                    {
                        using (SQLiteDataReader rdr = cmd.ExecuteReader())
                        {
                            while (rdr.Read())
                            {
                                count = rdr.GetInt32(0);
                            }
                        }
                    }
                }
                catch (SQLiteException ex)
                {
                    DatabaseError(sql, ex);
                }

                con.Close();
            }

            GC.Collect();

            return (count > 0);
        }


    }
}
