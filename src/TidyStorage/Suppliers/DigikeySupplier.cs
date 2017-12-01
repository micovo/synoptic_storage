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
    public class DigikeySupplier : Supplier
    {
        SupplierPart part;

        public override string Name { get { return "Digikey"; } }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="part_number"></param>
        public DigikeySupplier(string part_number) : base(part_number)
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
            string url = "https://www.digikey.com/products/en?keywords=" + part_number;

            string output = "";


            output = url;

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

            HtmlNodeCollection hnc;


            hnc = document.DocumentNode.SelectNodes("//table[@id='product-details']");

            if (hnc != null)
            {
                var specs_table = hnc.First();

                var rows = specs_table.SelectNodes(".//tr");

                for (int i = 1; i < rows.Count; i++)
                {
                    HtmlNode row = rows[i];
                    var head_cell = row.SelectNodes(".//th");
                    var value_cell = row.SelectNodes(".//td");

                    if ((head_cell != null) && (value_cell != null))
                    {
                        string name = head_cell.First().InnerText.Trim().Trim(':');
                        string value = value_cell.First().InnerText.Trim().Replace("&nbsp;", "");

                        part.rows.Add(new PartRow(name, value));
                    }
                }
            }

            hnc = document.DocumentNode.SelectNodes("//table[@id='prod-att-table']");

            if (hnc != null)
            {
                var specs_table = hnc.First();

                var rows = specs_table.SelectNodes(".//tr");

                for (int i = 1; i < rows.Count; i++)
                {
                    HtmlNode row = rows[i];
                    var head_cell = row.SelectNodes(".//th");
                    var value_cell = row.SelectNodes(".//td");

                    if ((head_cell != null) && (value_cell != null))
                    {
                        string name = head_cell.First().InnerText.Trim().Trim(':');
                        string value = value_cell.First().InnerText.Trim().Replace("&nbsp;", "");

                        part.rows.Add(new PartRow(name, value));
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
            var hnc = document.DocumentNode.SelectNodes("//table[contains(@id,'product-dollars')]");

            if (hnc != null)
            {
                var main = hnc.First();

                var rows = main.SelectNodes(".//tr");


                if ((rows != null) && (rows.Count > 1))
                {
                    part.prices = new List<PartPrice>();

                    for (int i = 1; i < rows.Count; i++)
                    {
                        var cells = rows[i].SelectNodes(".//td");

                        string quantity_str = cells[0].InnerText.Trim();
                        string price_str = cells[1].InnerText.Trim();


                        int min;
                        float price;

                        part.currency = "USD";

                        price_str = price_str.Trim();
                        price_str = price_str.Replace(",", System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);
                        price_str = price_str.Replace(".", System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);
                        quantity_str = quantity_str.Replace(".", "");
                        quantity_str = quantity_str.Replace(",", "");

                        if (int.TryParse(quantity_str, out min) && float.TryParse(price_str, out price))
                        {
                            part.prices.Add(new PartPrice(min, price));
                        }

                    }
                }
                    
                
            }
        }



    }
}
