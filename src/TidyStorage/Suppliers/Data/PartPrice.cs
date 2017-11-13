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


        public PartPrice(int amount_min, int amount_max, float price)
        {
            this.amount_max = amount_max;
            this.amount_min = amount_min;
            this.price = price;
        }


        public PartPrice(long amount_min, long amount_max, double price) :  this((int) amount_min, (int) amount_max, (float) price)
        {

        }



        public PartPrice(int amount_min, float price)
        {
            this.amount_max = 0;
            this.amount_min = amount_min;
            this.price = price;
        }

        public float GetPrice(int amount)
        {
            if (amount < amount_min) return float.NaN;

            return price;
        }

        public override string ToString()
        {
            return amount_min.ToString() + " - " + amount_max.ToString() + " = " + price.ToString();
        }

    }
}
