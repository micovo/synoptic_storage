using System;
using System.Collections.Generic;

using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Security;
using System.Web;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Diagnostics;
using HtmlAgilityPack;
using TidyStorage.Suppliers.Data;

namespace TidyStorage.Suppliers
{
    class TMESupplier : Supplier
    {
        public TMESupplier(string part_number) : base(part_number)
        {

        }

        public override string GetLink()
        {
            return "http://www.tme.eu/cz/details/" + part_number + "/";
        }

        public override SupplierPart DownloadPart()
        {
            SupplierPart p = new SupplierPart();

            p.order_num = part_number;

            string refferer = "http://www.tme.eu/cz/details/"+ part_number + "/";
            string error = "";
            string responce = "";
            string origin = "http://www.tme.eu";
            string host = "www.tme.eu";
            string url = refferer;
            CookieContainer cc = new CookieContainer();

            using (HttpWebResponse resp = WebClient.Request(url, out error, cc))
            {
                if (resp != null)
                {
                    StreamReader reader = new StreamReader(resp.GetResponseStream());

                    responce = reader.ReadToEnd();
                }
            }


            url = "http://www.tme.eu/cz/_ajax/ProductInformationPage/_getStocks.html";

            NameValueCollection nvc = new NameValueCollection();

            nvc["symbol"] = part_number;

            using (HttpWebResponse resp = WebClient.Request(url, out error, cc,nvc, host, refferer, origin))
            {
                if (resp != null)
                {
                    StreamReader reader = new StreamReader(resp.GetResponseStream());

                    responce = reader.ReadToEnd();

                    if (responce != "")
                    {
                        JToken jt = JToken.Parse(responce);
                        JArray ja = (JArray)jt["Products"][0]["Prices"];

                        if (ja != null)
                        {
                            p.prices = new List<PartPrice>();

                            foreach (JToken jat in ja)
                            {

                                int min = (int)jat["Amount"];
                                string temp = (string)jat["Price"];
                                temp = temp.Replace(" Kč","").Replace('.',',');
                                float price = float.Parse(temp);

                                p.prices.Add(new PartPrice(min, price));
                            }

                        }

                    }
                }
            }

           

            return p;


        }
    }
}
