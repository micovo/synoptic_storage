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
    public class FarnellSupplier:Supplier
    {
        SupplierPart part;

        public override string Name { get { return "Farnell CZ"; } }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="part_number"></param>
        public FarnellSupplier(string part_number):base(part_number)
        {
            //Nothing to do here
        }

        /// <summary>
        /// Function creates URL for the provided supplier part number
        /// </summary>
        /// <returns>Part URL</returns>
        public override string GetLink()
        {
            return "http://cz.farnell.com/jsp/search/productdetail.jsp?SKU=" + part_number;
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
                        GetPrice(document);
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
            HtmlNodeCollection hnc = document.DocumentNode.SelectNodes("//div[@class='productDescription packaging']");

            if (hnc != null)
            {
                var ul_node = hnc.First().ChildNodes.FirstOrDefault(x => (x.Name == "dl"));

                if (ul_node != null)
                {
                    HtmlNode[] rows = ul_node.ChildNodes.Where(x => (x.Name == "div")).ToArray();

                    foreach (HtmlNode li_node in rows)
                    {
                        string[] spl = li_node.InnerText.Split(':');

                        if (spl.Length == 2)
                        {
                            string name = spl[0].Trim();
                            string value = spl[1].Trim().Split('\n')[0].Trim();

                            if ((name.Length > 0)&& (value.Length > 0))
                            {
                                part.rows.Add(new PartRow(name, value));
                            }
                        }
                    }
                }
            }

            hnc = document.DocumentNode.SelectNodes("//ul[@id='technicalData']");

            if (hnc != null)
            {
                var li_node = hnc.First().ChildNodes.FirstOrDefault(x => (x.Name == "li"));
                if (li_node != null)
                {
                    var a_node = li_node.ChildNodes.FirstOrDefault(x => (x.Name == "a"));

                    if (a_node != null)
                    {
                        if (a_node.Attributes["href"] != null)
                        {
                            string name = "Datasheet";
                            string value = a_node.Attributes["href"].Value;
                            part.rows.Add(new PartRow(name, value));
                        }
                    }
                }
            }


            hnc = document.DocumentNode.SelectNodes("//ul[@class='productAttributes']");

            if (hnc != null)
            {
                HtmlNode hn = hnc.First();

                HtmlNode[] rows = hn.ChildNodes.Where(x => (x.Name == "li")).ToArray();

                foreach (HtmlNode li_node in rows)
                {
                    HtmlNode[] spans = li_node.ChildNodes.Where(cond => (cond.Name == "span")).ToArray();

                    if (spans.Count() == 2)
                    {
                        string value = spans[1].InnerText;
                        value = value.Replace('µ', 'u');
                        value = value.Replace('±', ' ');
                        value = value.Trim();


                        part.rows.Add(new PartRow(spans[0].InnerText.Trim(), value));
                    }
                }
            }

            hnc = document.DocumentNode.SelectNodes("//dl");

            if (hnc != null)
            {
                foreach (HtmlNode dl_node in hnc)
                {
                    HtmlNode[] dt_nodes = dl_node.ChildNodes.Where(x => (x.Name == "dt")).ToArray();
                    HtmlNode[] dd_nodes = dl_node.ChildNodes.Where(x => (x.Name == "dd")).ToArray();
                    
                    for (int i = 0; i < dt_nodes.Count(); i++)
                    {
                        string value = dd_nodes[i].InnerText;
                        value = value.Replace('µ', 'u');
                        value = value.Replace('±', ' ');
                        value = value.Trim();


                        part.rows.Add(new PartRow(dt_nodes[i].InnerText.Trim(), value));
                    }
                }
            }
        }

        /// <summary>
        /// Parser for reading product prices from the prvided HTML document
        /// </summary>
        /// <param name="document">HTML document to be parsed</param>
        private void GetPrice(HtmlDocument document)
        {

            HtmlNodeCollection hnc = document.DocumentNode.SelectNodes("//table[@class='tableProductDetailPrice pricing']");

            if (hnc != null)
            {
                HtmlNode hn = hnc.First().ChildNodes.Where(x => x.Name == "tbody").First();
                part.prices = new List<PartPrice>();

                

                foreach (HtmlNode x in hn.ChildNodes)
                {
                    if ((x.Name == "tr") && (x.ChildNodes.Count > 0) && (x.ChildNodes[1].Name == "td"))
                    {
                        if (x.ChildNodes.Count >= 5)
                        {
                            string tda = HttpUtility.HtmlDecode(x.ChildNodes[1].InnerText).Trim();
                            string tdp = HttpUtility.HtmlDecode(x.ChildNodes[3].InnerText).Trim().Replace(".", ",");

                            if (tdp.Contains("Kč"))
                            {
                                part.currency = "CZK";

                                var aaa = tda.Split('-');
                                var min = int.Parse(aaa[0].Trim().Trim('+'));

                                int ix = tdp.IndexOfAny(("0123456789").ToCharArray());
                                var price = float.Parse(tdp.Substring(ix, tdp.Length - ix - 3));

                                part.prices.Add(new PartPrice(min, int.MaxValue, price));
                            }
                        }
                    }
                }
 
                for (int i = 0; i < part.prices.Count - 1; i++)
                {
                    part.prices[i].amount_max = part.prices[i + 1].amount_min - 1;
                }
            }
        }
    }
}