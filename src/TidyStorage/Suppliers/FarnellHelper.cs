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
            return "http://export.farnell.com/jsp/search/productdetail.jsp?SKU=" + part_number;
        }


        public override SupplierPart DownloadPart()
        {
            SupplierPart p = new SupplierPart();

            p.order_num = part_number;

            string error = "";
            string responce = "";
            string url = "http://export.farnell.com/jsp/search/productdetail.jsp?SKU=" + part_number;

            using (HttpWebResponse resp = WebClient.Request(url, out error, null))
            {
                if (resp != null)
                {
                    StreamReader reader = new StreamReader(resp.GetResponseStream());

                    responce = reader.ReadToEnd();

                    HtmlDocument document = new HtmlDocument();
                    document.LoadHtml(responce);

                    HtmlNodeCollection hnc = document.DocumentNode.SelectNodes("//table[@class='pricing ']");
                     
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

                        
                    }

                }
            }

            return p;


        }

    }
}