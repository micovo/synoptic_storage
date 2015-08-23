using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TidyStorage
{

    public enum StorageTypeTables
    {
        Manufacturer,
        PartType,
        Package,
        PlaceType,
        Supplier
    }


    public class StorageConst
    {

        public const string sqlite_version = "1.0.0.0";

        public const string sqlite_connection_str = "Data Source={0};Version=3;";


        public const string Str_Manufacturer = "manufacturer";
        public const string Str_Manufacturer_id = "id_manufacturer";
        public const string Str_Manufacturer_name = "manufacturername";


        public const string Str_Package = "part_package";
        public const string Str_Package_id = "id_part_package";
        public const string Str_Package_name = "packagename";


        public const string Str_PartType = "part_type";
        public const string Str_PartType_id = "id_part_type";
        public const string Str_PartType_name = "typename";


        public const string Str_PlaceType = "storage_place";
        public const string Str_PlaceType_id = "id_storage_place";
        public const string Str_PlaceType_name = "placename";


        public const string Str_Supplier = "supplier";
        public const string Str_Supplier_id = "id_supplier";
        public const string Str_Supplier_name = "id_supplier";



        public const string sql_createtable_storage_info = @"
        CREATE TABLE storage_info (
            parameter VARCHAR(20) PRIMARY KEY NOT NULL, 
            value VARCHAR(256) NOT NULL
        )";

        public const string sql_createtable_part = @"
        CREATE TABLE part ( 
            id_part INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL, 
            " + Str_PartType_id + @" INTEGER, 
            " + Str_Package + @" INTEGER, 
            " + Str_Manufacturer_id + @" INTEGER, 
            productnumber VARCHAR(64), 
            " + Str_Supplier_id + @" INTEGER, 
            suppliernumber VARCHAR(64), 
            primary_value REAL, 
            primary_tolerance REAL, 
            secondary_value REAL, 
            secondary_tolerance REAL, 
            tertiary_value REAL, 
            tertiary_tolerance REAL, 
            stock INTEGER, 
            currency CHAR(3), 
            price_1pcs REAL, 
            price_10pcs REAL, 
            price_100pcs REAL, 
            price_1000pcs REAL,
            comment VARCHAR(256),
            datasheet_url VARCHAR(256),
            temperature_from INTEGER,
            temperature_to INTEGER,
            " + Str_PlaceType_id + @" INTEGER
            storage_place_number INTEGER
        )";


        public const string sql_createtable_manufacturer = @"
        CREATE TABLE " + Str_Manufacturer + @" (
            " + Str_Manufacturer_id + @" INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
            " + Str_Manufacturer_name + @" VARCHAR(64) NOT NULL

        )";

        public const string sql_createtable_part_package = @"
        CREATE TABLE " + Str_Package + @" (
            " + Str_Package_id + @" INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL, 
            " + Str_Package_name + @" VARCHAR(64) NOT NULL
        )";

        public const string sql_createtable_part_type = @"
        CREATE TABLE " + Str_PartType + @" (
            " + Str_PartType_id + @" INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
            " + Str_PartType_name + @"  VARCHAR(64) NOT NULL,
            primary_valuename VARCHAR(64),
            secondary_valuename VARCHAR(64),
            tertiary_valuename VARCHAR(64)
        )";



        public const string sql_createtable_storage_place = @"
        CREATE TABLE " + Str_PlaceType + @" (
            " + Str_PlaceType_id + @"  INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
            " + Str_PlaceType_name + @"  VARCHAR(64) NOT NULL
        )";


        public const string sql_createtable_supplier = @"
        CREATE TABLE " + Str_Supplier + @" (
            " + Str_Supplier_id + @" INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
            " + Str_Supplier_name + @" VARCHAR(64) NOT NULL
        )";
    }
}
