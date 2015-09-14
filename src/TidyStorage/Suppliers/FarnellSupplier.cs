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
        public FarnellSupplier(string part_number):base(part_number)
        {

        }

        public override string GetLink()
        {
            return "http://cz.farnell.com/jsp/search/productdetail.jsp?SKU=" + part_number;
        }


        public override string Name { get { return "Farnell CZ"; } }


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

                    HtmlNodeCollection hnc = document.DocumentNode.SelectNodes("//table[@class='tableProductDetailPrice pricing ']");
                     
                    if (hnc != null)
                    {
                        HtmlNode hn = hnc.First().ChildNodes.Where(x => x.Name == "tbody").First();
                        p.prices = new List<PartPrice>();

                        foreach (HtmlNode x in hn.ChildNodes)
                        {
                            if ((x.Name == "tr") && (x.ChildNodes.Count > 0) && (x.ChildNodes[1].Name == "td"))
                            {
                                if (x.ChildNodes.Count >= 4)
                                {
                                    string tda = HttpUtility.HtmlDecode(x.ChildNodes[1].InnerText).Trim();
                                    string tdp = HttpUtility.HtmlDecode(x.ChildNodes[3].InnerText).Trim().Replace(".", ",");

                                    if (tdp.Contains("Kč"))
                                    {
                                        p.currency = "CZK";

                                        var aaa = tda.Split('-');
                                        var min = int.Parse(aaa[0].Trim().Trim('+'));
                                        var max = int.MaxValue;

                                        if (aaa.Length > 1)
                                        {
                                            max = int.Parse(aaa[1].Trim());
                                        }


                                        int ix = tdp.IndexOfAny(("0123456789").ToCharArray());
                                        var price = float.Parse(tdp.Substring(ix, tdp.Length - ix - 3));

                                        p.prices.Add(new PartPrice(min, max, price));
                                    }
                                }
                            }
                        }


                        p.rows = new List<PartRow>();
                        hnc = document.DocumentNode.SelectNodes("//div[@id='productDescription']");

                        if (hnc != null)
                        {
                            var ul_node = hnc.First().ChildNodes.First(x => (x.Name == "ul"));

                            if (ul_node != null)
                            {
                                HtmlNode[] rows = ul_node.ChildNodes.Where(x => (x.Name == "li")).ToArray();

                                foreach (HtmlNode li_node in rows)
                                {
                                    HtmlNode[] childs = li_node.ChildNodes.ToArray();

                                    if (childs[1].Name == "strong")
                                    {
                                        string name = childs[1].InnerText.Trim();
                                        string value = li_node.InnerText.Replace(name, "").Trim();

                                        p.rows.Add(new PartRow(name, value));
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
                                    string name = "Datasheet";
                                    string value = a_node.Attributes["href"].Value;
                                    p.rows.Add(new PartRow(name, value));
                                }
                            }
                        }


                        hnc = document.DocumentNode.SelectNodes("//ul[@class='productAttributes']");

                        if (hnc != null)
                        {
                            hn = hnc.First();

                            HtmlNode [] rows  = hn.ChildNodes.Where(x => (x.Name == "li")).ToArray();

                            foreach (HtmlNode li_node in rows)
                            {
                                HtmlNode[] spans = li_node.ChildNodes.Where(cond => (cond.Name == "span")).ToArray();

                                if (spans.Count() == 2)
                                {
                                    string value = spans[1].InnerText;
                                    value = value.Replace('µ', 'u');
                                    value = value.Replace('±', ' ');
                                    value = value.Trim();

                                    
                                    p.rows.Add(new PartRow(spans[0].InnerText.Trim(), value));
                                }
                            }
                        }
 


                    }

                }
            }

             return p;


        }

    }
}