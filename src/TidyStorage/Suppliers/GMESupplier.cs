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
    public class GMESupplier : Supplier
    {
        public GMESupplier(string part_number):base(part_number)
        {

        }


        public override string GetLink()
        {
            string error = "";
            string responce = "";
            string url = "http://www.gme.cz/products/search?term=" + part_number;

            string output = "";

            using (HttpWebResponse resp = WebClient.Request(url, out error, null))
            {
                if (resp != null)
                {
                    StreamReader reader = new StreamReader(resp.GetResponseStream());

                    responce = reader.ReadToEnd();

                    HtmlDocument document = new HtmlDocument();
                    document.LoadHtml(responce);

                    HtmlNodeCollection nodes = document.DocumentNode.SelectNodes("//article[@class='ProductTile']");

                    if (nodes != null)
                    {
                        HtmlNode hn = nodes[0];
                        string n = nodes[0].ChildNodes.Where(x => x.Name == "h2").First().ChildNodes.First().Attributes["href"].Value;

                        output = "http://www.gme.cz" + n;
                    }
                }
            }


            return output;
        }


        public override SupplierPart DownloadPart()
        {
            SupplierPart p = new SupplierPart();

            p.order_num = part_number;

            string error = "";
            string responce = "";
            string url = GetLink();

            using (HttpWebResponse resp = WebClient.Request(url, out error, null))
            {
                if (resp != null)
                {
                    StreamReader reader = new StreamReader(resp.GetResponseStream());

                    responce = reader.ReadToEnd();

                    HtmlDocument document = new HtmlDocument();
                    document.LoadHtml(responce);

                    HtmlNode main = document.GetElementbyId("ProductContent");

                    HtmlNode hn = main.ChildNodes.Where(x => x.Name == "div").ElementAt(2);
                    hn = hn.ChildNodes.Where(x => x.Name == "div").First();
                    hn = hn.ChildNodes.Where(x => x.Name == "span").ElementAt(2);

                    hn = document.DocumentNode.SelectNodes("//dd").First();
                 
                    p.prices = new List<PartPrice>();


                    string stemp = hn.InnerText;
                    stemp = stemp.Substring(0, stemp.IndexOf("Kč"));

                    int min = 1;
                    float price = float.Parse(stemp);


                    p.prices.Add(new PartPrice(min, price));

                    hn = document.DocumentNode.SelectNodes("//div[@class='ProductDiscounts']").First().ChildNodes.Where(x => x.Name == "table").First();
       
                    

                    if (hn != null)
                    {
                        IEnumerable<HtmlNode> hnc = hn.ChildNodes.Where(x => x.Name == "tr");

                        foreach (HtmlNode hh in hnc)
                        {
                            if (hh.FirstChild.Name == "td")
                            {
                                stemp = hh.ChildNodes.Where(x => (x.Name == "td") && (x.Attributes["class"].Value == "QuantityColumn")).First().InnerText;
                                min = int.Parse(stemp);

                                stemp = hh.ChildNodes.Where(x => (x.Name == "td") && (x.Attributes["class"].Value == "DiscountPrices")).First().InnerText;
                                stemp = stemp.Replace("Kč", "");
                                price = float.Parse(stemp);

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
