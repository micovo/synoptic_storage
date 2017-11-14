using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TidyStorage.Suppliers.Data
{
    public class PartPrice
    {
        public int amount_max;
        public int amount_min;

        public float price;
        
        /// <summary>
        /// Part Price constructor.
        /// </summary>
        /// <param name="amount_min"></param>
        /// <param name="price"></param>
        public PartPrice(int amount_min, float price)
        {
            this.amount_max = 0;
            this.amount_min = amount_min;
            this.price = price;
        }

        /// <summary>
        /// Part Price constructor.
        /// </summary>
        /// <param name="amount_min"></param>
        /// <param name="amount_max"></param>
        /// <param name="price"></param>
        public PartPrice(int amount_min, int amount_max, float price)
        {
            this.amount_max = amount_max;
            this.amount_min = amount_min;
            this.price = price;
        }

        /// <summary>
        /// Part Price constructor.
        /// </summary>
        /// <param name="amount_min"></param>
        /// <param name="amount_max"></param>
        /// <param name="price"></param>
        public PartPrice(long amount_min, long amount_max, double price) :  this((int) amount_min, (int) amount_max, (float) price)
        {
            //All code was handled in float price constructor. Nothing to do here.
        }

        /// <summary>
        /// Get part price based on the requested amount.
        /// </summary>
        /// <param name="amount">Count of parts</param>
        /// <returns>Price for the requested amount of parts</returns>
        public float GetPrice(int amount)
        {
            if (amount < amount_min) return float.NaN;

            return price;
        }

        /// <summary>
        /// Custom ToString function
        /// </summary>
        /// <returns>Amount and price in string format</returns>
        public override string ToString()
        {
            return amount_min.ToString() + " - " + amount_max.ToString() + " = " + price.ToString();
        }

    }
}
