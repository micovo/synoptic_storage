using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TidyStorage.Suppliers.Data;

namespace TidyStorage.Suppliers
{
    public abstract class Supplier
    {
        protected string part_number;

        public string PartNumber
        {
            get { return part_number; }
        }
        
        public Supplier(string part_number)
        {
            this.part_number = part_number;
        }

        public abstract string Name { get; }
        public abstract string GetLink();
        public abstract SupplierPart DownloadPart();
    }
}
