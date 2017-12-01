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
        SupplierPart part;

        public override string Name { get { return "GME CZ"; } }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="part_number"></param>
        public GMESupplier(string part_number):base(part_number)
        {
            //Nothing to do here
        }

        /// <summary>
        /// Function creates URL for the provided supplier part number
        /// </summary>
        /// <returns>Part URL</returns>
        public override string GetLink()
        {
            string error = "";
            string responce = "";
            string url = "https://www.gme.cz/vysledky-vyhledavani?search_keyword=" + part_number + "&page=1";

            string output = "";

            using (HttpWebResponse resp = WebClient.Request(url, out error, null))
            {
                if (resp != null)
                {
                    output = resp.ResponseUri.ToString();
                }
            }
            return output;
        }
        
        /// <summary>
        /// Function downloads part details for the provided supplier part number
        /// </summary>
        /// <returns>Supplier Part</returns>
        public override SupplierPart DownloadPart()
        {
            part = new SupplierPart();

            part.order_num = part_number;

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
                    
                    if (document != null)
                    {
                        GetPrices(document);
                        GetProductDescriptors(document);
                    }

                }
            }

            return part;
        }

        /// <summary>
        /// Parser for reading product details from the prvided HTML document
        /// </summary>
        /// <param name="document">HTML document to be parsed</param>
        private void GetProductDescriptors(HtmlDocument document)
        {
            part.rows = new List<PartRow>();

            
            HtmlNodeCollection hnc = document.DocumentNode.SelectNodes("//div[@class='product_info']");

            if (hnc != null)
            {
                string product_infos = hnc.First().InnerText;

                var prod_split = product_infos.Split('|');
                foreach (string stuff in prod_split)
                {
                    var stufff = stuff.Split(':');
                    string name = stufff[0].Trim();
                    string value = stufff[1].Trim();
                    part.rows.Add(new PartRow(name, value));
                }
            }


            hnc = document.DocumentNode.SelectNodes("//a[@class='extensionPDF']");

            if (hnc != null)
            {
                string value = "http://www.gme.cz" + hnc.First().Attributes["href"].Value;
                part.rows.Add(new PartRow("Datasheet", value));
            }

            hnc = document.DocumentNode.SelectNodes("//div[@id='tabs-specification']");

            if (hnc != null)
            {
                var info_divs = hnc.First().ChildNodes.Where(x => (x.Name == "div")).ToArray();

                if (info_divs != null)
                {
                    foreach (HtmlNode hn in info_divs)
                    {
                        var info = hn.ChildNodes.FirstOrDefault(x => (x.Name == "p") || (x.Name == "table"));
                        if (info.Name == "p")
                        {
                            part.rows.Add(new PartRow("Comment", info.InnerText));
                        }
                        else if (info.Name == "table")
                        {
                            var rows = info.ChildNodes.Where(x => x.Name == "tr").ToArray();
                            foreach (HtmlNode table_tr in rows)
                            {
                                var cells = table_tr.ChildNodes.Where(x => (x.Name == "th") || (x.Name == "td")).ToArray();

                                string value = "";
                                string name = cells[0].InnerText.Trim();

                                value = cells[1].InnerText.Trim().Trim('%');
                                if (cells[2].InnerText.ToLower() == "ohm")
                                {
                                    value = value.Trim('R'); 
                                }

                                value += cells[2].InnerText.Trim();


                                part.rows.Add(new PartRow(name, value));
                            }
                        }
                        
                    }
                }
            }
        }

        /// <summary>
        /// Parser for reading product prices from the prvided HTML document
        /// </summary>
        /// <param name="document">HTML document to be parsed</param>
        private void GetPrices(HtmlDocument document)
        {

            HtmlNode main = document.GetElementbyId("ProductContent");

            //Failed
            if (main == null) return;
               
            HtmlNode hn = main.ChildNodes.Where(x => x.Name == "div").ElementAt(2);
            hn = hn.ChildNodes.Where(x => x.Name == "div").First();
            hn = hn.ChildNodes.Where(x => x.Name == "span").ElementAt(2);

            hn = document.DocumentNode.SelectNodes("//dd").First();

            part.prices = new List<PartPrice>();


            string stemp = hn.InnerText;
            stemp = stemp.Substring(0, stemp.IndexOf("Kč"));

            int min = 1;
            float price = float.Parse(stemp);


            part.prices.Add(new PartPrice(min, price));

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

                        part.prices.Add(new PartPrice(min, price));
                    }
                }


                for (int i = 0; i < part.prices.Count - 1; i++)
                {
                    part.prices[i].amount_max = part.prices[i+1].amount_min - 1;
                }

                part.prices[part.prices.Count - 1].amount_max = int.MaxValue;
            }
        }
    }
}
