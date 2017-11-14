using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TidyStorage.Suppliers.Data
{
    /// <summary>
    /// Class used for by the web importer for passing parameters.
    /// </summary>
    public class PartRow
    {
        string name;
        string value;

        /// <summary>
        /// Parameter value
        /// </summary>
        public string Value
        {
            get { return value; }
        }

        /// <summary>
        /// Parameter name
        /// </summary>
        public string Name
        {
            get { return name; }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public PartRow(string name, string value = "")
        {
            this.name = name;
            this.value = value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string s = name;
            if (value != "") s += " = " + value;
            return s;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj.GetType() == typeof(PartRow))
            {
                PartRow pr = (PartRow)obj;
                return (this.name == pr.name) && (this.value == pr.value);
            }
            return false;
        }
    }
}
