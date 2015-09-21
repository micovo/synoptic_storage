using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TidyStorage
{
    public class StringHelpers
    {
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
        /// 
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string RemoveUnsafeCharacters(string str)
        {
            //TODO

            return str;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="target"></param>
        /// <param name="keywords"></param>
        /// <returns></returns>
        public static string LikeCondition(string target, string [] keywords, string splitter = "OR")
        {
            string o = "";

            foreach (string str in keywords)
            {
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
        /// 
        /// </summary>
        /// <param name="targets"></param>
        /// <param name="keywords"></param>
        /// <param name="splitter"></param>
        /// <returns></returns>
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
