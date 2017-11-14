using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TidyStorage.Suppliers.Data
{
    public class SupplierPart
    {
        public List<PartPrice> prices;
        public List<PartRow> rows;

        public string name;
        public string order_num;

        public string currency;

        //PriceListMaster variable
        public string supplier;

        /// <summary>
        /// Get part price based on the requested amount.
        /// </summary>
        /// <param name="amount">Count of parts</param>
        /// <returns>Price for the requested amount of parts</returns>
        public float GetPrice(int amount)
        {
            if ((prices != null) && (prices.Count > 0))
            {
                float price = 0;

                if (prices[0].amount_min >= amount) return prices[0].price;
                                
                foreach (PartPrice pp in prices)
                {
                    if (amount >= pp.amount_min)
                    {
                        price = pp.price;
                    }
                }

                return price;
            }

            return float.NaN;
        }
    }
}
