using System;
using System.IO;
using System.Collections.Specialized;
using System.Net;
using System.Text;
using System.Collections.Generic;
using System.Web;
using System.Security.Cryptography;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;
using System.Collections;
using System.Diagnostics;

namespace TidyStorage.Suppliers
{
    class WebClient
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        public static string RandomString(int size)
        {
            Random _rng = new Random();
            const string _chars = "aaaabcdeeeefffffghiiiiijklmnoooooopqrstuuuuuuuuvwwwwwwwwxyz--------";

            char[] buffer = new char[size];

            for (int i = 0; i < size; i++)
            {
                buffer[i] = _chars[_rng.Next(_chars.Length)];
            }
            return new string(buffer);
        }

        

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="certificate"></param>
        /// <param name="chain"></param>
        /// <param name="policyErrors"></param>
        /// <returns></returns>
        public static bool ValidateRemoteCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors policyErrors)
        {
            // allow all certificates
            return true;
        }


        static public bool AcceptAllCertifications(object sender, System.Security.Cryptography.X509Certificates.X509Certificate certification, System.Security.Cryptography.X509Certificates.X509Chain chain, System.Net.Security.SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="errorresponce"></param>
        /// <param name="postdata"></param>
        /// <returns></returns>
        public static HttpWebResponse GetResponse(HttpWebRequest request, out string errorresponce, NameValueCollection postdata = null)
        {
            Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("en-US");

            errorresponce = "";

            //request.UnsafeAuthenticatedConnectionSharing = true;
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback(AcceptAllCertifications);

            try
            {

                if (postdata != null)
                {
                    string dataString = NameValueToHttpString(postdata);

                    byte[] dataBytes = Encoding.ASCII.GetBytes(dataString);
                    request.ContentLength = dataBytes.Length;

                    using (Stream requestStream = request.GetRequestStream())
                    {
                        requestStream.Write(dataBytes, 0, dataBytes.Length);
                    }
                }

                return (HttpWebResponse)request.GetResponse();
            }
            catch (WebException e)
            {


                if (e.Status == WebExceptionStatus.ProtocolError)
                {
                    using (WebResponse resp = e.Response)
                    {
                        using (StreamReader sr = new StreamReader(resp.GetResponseStream()))
                        {
                            string r = sr.ReadToEnd();
                            if (r != "")
                            {
                                if ((r[0] == '{') && (r[r.Length - 1] == '}'))
                                {
                                    errorresponce = r;
                                }
                            }
                        }
                    }
                }

                if (errorresponce == "") errorresponce = e.Message;

                return null;
            }
        }

        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="postdata"></param>
        /// <returns></returns>
        public static string NameValueToHttpString(NameValueCollection postdata)
        {
            return String.Join("&", Array.ConvertAll(postdata.AllKeys, key =>
                        String.Format("{0}={1}", HttpUtility.UrlEncode(key), HttpUtility.UrlEncode(postdata[key]))
                    )
                    );
        }
        



        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static HttpWebResponse Request(string url)
        {
            string error = "";

            return Request(url, out error);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static HttpWebResponse Request(string url, out string error, CookieContainer cookieContainer = null, NameValueCollection postdata = null, string host = "", string referer = "", string origin = "")
        {
            if (url == "")
            {
                error = "No url address was provided";
                return null;
            }
            HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;

            error = "";

            request.Method = (postdata == null) ? "GET" : "POST";
            request.Timeout = 20000;
            request.KeepAlive = false;
            request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8";
            request.Headers.Add("accept-language", "en-US");
            request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/61.0.3163.100 Safari/537.36";
            request.Proxy = null;
            request.Headers.Add("Upgrade-Insecure-Requests", "1");

            if (host != "") request.Host = host;
            if (referer != "") request.Referer = referer;
            if (origin != "") request.Headers.Add("Origin", origin);

            if (request.Method == "POST")
            {
                request.Headers.Add("X-Requested-With", "XMLHttpRequest");
            }

            request.CookieContainer = cookieContainer ?? new CookieContainer();

            request.AllowAutoRedirect = true;
            request.MaximumAutomaticRedirections = 10;

            System.Net.ServicePointManager.Expect100Continue = false;

            return WebClient.GetResponse(request, out error, postdata);
        }

        

        public static HttpWebResponse RequestExchangeAPI(string url, NameValueCollection postdata, out string error)
        {
            HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;

            error = "";

            request.Method = "POST";
            request.Timeout = 15000;
            request.KeepAlive = false;
            request.Accept = "*/*";
            request.ContentType = "application/x-www-form-urlencoded";
            request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:20.0) Gecko/20100101 Firefox/20.1";
            request.Proxy = null;

            System.Net.ServicePointManager.Expect100Continue = false;

            return WebClient.GetResponse(request, out error, postdata);
        }
    }
}
