using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TidyStorage.Suppliers.Data
{
    public class PartRow
    {
        string name;
        string value;

        public string Value
        {
            get { return value; }
        }

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
