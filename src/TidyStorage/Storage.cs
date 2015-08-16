using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Data.SQLite;
using System.Windows.Forms;
using System.Data;

namespace TidyStorage
{
    public class Storage
    {

        const string sqlite_version = "1.0.0.0"; 

        const string sqlite_connection_str = "Data Source={0};Version=3;";


        const string sql_createtable_storage_info   = @"
        CREATE TABLE storage_info (
            parameter VARCHAR(20) PRIMARY KEY NOT NULL, 
            value VARCHAR(256) NOT NULL
        )";

        const string sql_createtable_part = @"
        CREATE TABLE part ( 
            id_part INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL, 
            id_part_type INTEGER, 
            id_part_package INTEGER, 
            id_part_producer INTEGER, 
            productnumber VARCHAR(64), 
            id_supplier INTEGER, 
            suppliernumber VARCHAR(64), 
            primary_value REAL, 
            primary_tolerance REAL, 
            secondary_value REAL, 
            secondary_tolerance REAL, 
            third_value REAL, 
            third_tolerance REAL, 
            stock INTEGER, 
            currency CHAR(3), 
            price_1pcs REAL, 
            price_10pcs REAL, 
            price_100pcs REAL, 
            price_1000pcs REAL,
            comment VARCHAR(256),
            datasheet_url VARCHAR(256),
            temperature_from INTEGER,
            temperature_to INTEGER
        )";

        const string sql_createtable_part_type = @"
        CREATE TABLE part_type (
            id_part_type INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
            typename VARCHAR(64),
            primary_valuename VARCHAR(64),
            secondary_valuename VARCHAR(64),
            third_valuename VARCHAR(64)
        )";

        const string sql_createtable_part_package   = @"
        CREATE TABLE part_package (
            id_part_package INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL, 
            packagename VARCHAR(64)
        )";

        const string sql_createtable_part_producer  = @"
        CREATE TABLE part_producer (
            id_part_producer INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
            producername VARCHAR(64)
        )";

        const string sql_createtable_storage_place  = @"
        CREATE TABLE storage_place (
            id_storage_place INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
            placename VARCHAR(64)
        )";

        const string sql_createtable_supplier       = @"
        CREATE TABLE supplier (
            id_supplier INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
            suppliername VARCHAR(64)
        )";

        private string sqlite_path;


        /// <summary>
        /// Checks if file exist, new file is created if not. 
        /// Constructor also checks DB tables by comparing SQL create commands stored in SQLite master table.
        /// </summary>
        /// <param name="filename">SQLite database file</param>
        public Storage(string filename)
        {
            string[] tables = { sql_createtable_storage_info ,
                        sql_createtable_part,
                        sql_createtable_part_type,
                        sql_createtable_part_package,
                        sql_createtable_part_producer,
                        sql_createtable_storage_place,
                        sql_createtable_supplier};

            if (File.Exists(filename) == false)
            {
                //If database file do not exist

                //Create new database file
                SQLiteConnection.CreateFile(filename + ".tmp");

                using (SQLiteConnection con = new SQLiteConnection(string.Format(sqlite_connection_str, filename)))
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

                    sql = "INSERT INTO storage_info (parameter, value) VALUES ('version','" + sqlite_version + "')";
                    command = new SQLiteCommand(sql, con);
                    command.ExecuteNonQuery();


                    con.Close();
                }
            }
            else
            {
                File.Copy(filename, filename + ".tmp");
            }


            bool StorageVersionVerified = false;

            //Check database content
            using (SQLiteConnection con = new SQLiteConnection(string.Format(sqlite_connection_str, filename + ".tmp")))
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
                            if (rdr.GetString(0) == sqlite_version)
                            {
                                StorageVersionVerified = true;
                            }
                        }
                    }
                }

                con.Close();
            }


            if (StorageVersionVerified)
            {
                //Everything is OK
                sqlite_path = filename;
            }
            else
            {
                throw new Exception("TidyStorage datafile corrupted");
            }
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="dgw"></param>
        public void FillData(ref DataGridView dgw)
        {
            try
            {
                using (SQLiteConnection con = new SQLiteConnection(string.Format(sqlite_connection_str, sqlite_path)))
                {
                    con.Open();

                    var sql = @"SELECT * FROM part WHERE 1";
                    var cmd = new SQLiteCommand(sql, con);


                    using (SQLiteDataReader rdr = cmd.ExecuteReader())
                    {

                        dgw.DataSource = rdr;


                        var mAdapter = new SQLiteDataAdapter(sql, con);
                        var mTable = new DataTable(); // Don't forget initialize!
                        mAdapter.Fill(mTable);
                        
                        dgw.DataSource = mTable;
                    }

                    con.Close();
                }
            }
            catch (Exception ex)
            {
                string s = ex.Message;
            }
        }



    }
}
