using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TidyStorage
{
    public class StringHelpers
    {
        /// <summary>
        /// Function reads string from STR which is between FirstString and LastString.
        /// This function is used for extracting units from the part parameters
        /// For example: StringHelpers. Between("Resistivity [Ohm]", "[", "]") returns "Ohm"
        /// </summary>
        /// <param name="STR">Input string</param>
        /// <param name="FirstString">Start string</param>
        /// <param name="LastString">Stop string</param>
        /// <returns>String between FirstString and LastString</returns>
        public static string Between(string STR, string FirstString, string LastString)
        {
            string FinalString = "";
            if ((STR != null) && (FirstString != null) && (LastString != null))
            {
                int Pos1 = STR.IndexOf(FirstString) + FirstString.Length;
                int Pos2 = STR.IndexOf(LastString);
                if ((Pos1 > 0) && (Pos2 > 0))
                {
                    FinalString = STR.Substring(Pos1, Pos2 - Pos1);
                }
            }
            return FinalString;
        }
       
        /// <summary>
        /// Function remove characters that are unsafe for SQL from input string
        /// </summary>
        /// <param name="str">Input string to be trimmed</param>
        /// <returns>String without unsafe characters</returns>
        public static string RemoveUnsafeCharacters(string str)
        {
            //TODO more dangerous characters
            str = str.Trim(",'\"".ToCharArray());
            return str;
        }

        /// <summary>
        /// Function creates SQL LIKE condition for the array of keywords for given target column
        /// </summary>
        /// <param name="target">Column that should be used in query</param>
        /// <param name="keywords">Keywords to be searched in column values</param>
        /// <param name="splitter">Spliter between keywords</param>
        /// <returns>SQL LIKE condition for the target column and keywords</returns>
        public static string LikeCondition(string target, string [] keywords, string splitter = "OR")
        {
            string o = "";

            foreach (string str in keywords)
            {
                if (str.Length < 2) continue;

                var str2 = StringHelpers.RemoveUnsafeCharacters(str);

                o += target;
                o += " LIKE '%";
                o += str2;
                o += "%' ";
                o += splitter;
                o += " ";
            }

            o = o.TrimEnd((splitter + " ").ToCharArray());

            return o;
        }

        /// <summary>
        /// Function creates SQL LIKE condition for the array of keywords for multiple target columns
        /// </summary>
        /// <param name="target">Array of columns that should be used in query</param>
        /// <param name="keywords">Keywords to be searched in columns values</param>
        /// <param name="target_splitter">Spliter between keywords</param>
        /// <param name="keywords_splitter">Spliter between columns condition</param>
        /// <returns>SQL LIKE condition for the target columns and keywords</returns>
        public static string LikeCondition(string [] targets, string[] keywords, string target_splitter = "OR", string keywords_splitter = "AND" )
        {
            string o = "";

            foreach (string strkey in keywords)
            {
                var strkey2 = StringHelpers.RemoveUnsafeCharacters(strkey);

                o += "(";
                foreach (string tarstr in targets)
                {

                    o += tarstr;
                    o += " LIKE '%";
                    o += strkey2;
                    o += "%' ";
                    o += target_splitter;
                    o += " ";
                }
                
                o = o.TrimEnd((target_splitter + keywords_splitter + " ").ToCharArray());

                o += ") ";
                o += keywords_splitter;
               
            }

            o = o.TrimEnd((target_splitter + keywords_splitter + " ").ToCharArray());

            return o;
        }
    }
}
