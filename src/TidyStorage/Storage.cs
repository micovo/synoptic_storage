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
 


    public class Storage
    {
        private string filename;
        private string workfile;
        


        /// <summary>
        /// Destructor
        /// </summary>
        ~Storage()
        {
           
        }


        public void Save(string target_filename = "")
        {
            if (target_filename == "")
            {
                File.Copy(this.workfile, this.filename, true);
            }
        }


        /// <summary>
        /// Checks if file exist, new file is created if not. 
        /// Constructor also checks DB tables by comparing SQL create commands stored in SQLite master table.
        /// </summary>
        /// <param name="filename">SQLite database file</param>
        public Storage(string filename)
        {
            this.filename = filename;

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
            else
            {
                File.Copy(filename, workfile, true);
            }


            bool StorageVersionVerified = false;

            //Check database content
            using (SQLiteConnection con = new SQLiteConnection(string.Format(StorageConst.sqlite_connection_str, filename + ".tmp")))
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
        public DataTable GetTable(string table, string columns = "*", string where_condition = "1")
        {
            DataTable tab = null;

            try
            {
                using (SQLiteConnection con = new SQLiteConnection(string.Format(StorageConst.sqlite_connection_str, workfile)))
                {
                    con.Open();

                    var sql = String.Format(@"SELECT {0} FROM {1} WHERE {2}", columns, table, where_condition);
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

            int strings_to_compare = 1;


            foreach (DataRow dr in dt.Rows)
            {
                bool change_found = false;

                int id = int.Parse(dr.ItemArray[0].ToString());

                DataRow drr = tab.Rows.Find(id);

                if (drr != null)
                {
                    //Not deleted, check for content changes
                    for (int i = 1; i < strings_to_compare + 1; i++)
                    {
                        if ((string)drr.ItemArray[i] != (string)dr.ItemArray[i])
                        {
                            change_found = true;
                        }
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

                        var sql = string.Format("UPDATE {0} SET {1} WHERE {2} = {3}", table, update_query, id_column_name, id);
                        var command = new SQLiteCommand(sql, con);
                        command.ExecuteNonQuery();
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
        public void InsertEmptyRow (string table, string namecolumn)
        {
            using (SQLiteConnection con = new SQLiteConnection(string.Format(StorageConst.sqlite_connection_str, workfile)))
            {
                con.Open();

                var sql = string.Format("INSERT INTO {0} ({1}) VALUES ('Enter new')", table, namecolumn);
                var command = new SQLiteCommand(sql, con);
                command.ExecuteNonQuery();

                con.Close();
            }

            GC.Collect();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="table"></param>
        /// <param name="id_column"></param>
        /// <param name="id"></param>
        public void DeleteRow(string table, string id_column, int id)
        {
            using (SQLiteConnection con = new SQLiteConnection(string.Format(StorageConst.sqlite_connection_str, workfile)))
            {
                con.Open();

                var sql = string.Format("DELETE FROM {0} WHERE ({1} = {2})", table, id_column, id);
                var command = new SQLiteCommand(sql, con);
                command.ExecuteNonQuery();

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
                con.Open();

                var sql = string.Format(@"SELECT COUNT(1) FROM part WHERE {0} = {1}", column_name, column_value);
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

                con.Close();
            }

            GC.Collect();

            return (count > 0);
        }


    }
}
