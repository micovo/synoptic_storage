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
        /// Function for the validation of the SSL certificates. This function validates all certificates.
        /// Pass this function to ServicePointManager.ServerCertificateValidationCallback
        /// </summary>
        /// <param name="sender">Function sender</param>
        /// <param name="certificate">Certificated to be checked</param>
        /// <param name="chain">Certificate chain</param>
        /// <param name="policyErrors">Certificate policy errors</param>
        /// <returns>Returns true if certificate is valid. Always true.</returns>
        public static bool ValidateRemoteCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors policyErrors)
        {
            //Allow all certificates
            return true;
        }

        /// <summary>
        /// HTTP request and responce wrapper that is trying to act as Chrome web browser.
        /// </summary>
        /// <param name="request">HTTP request to be processed</param>
        /// <param name="errorresponce">HTTP error string if the web server returns any error</param>
        /// <param name="postdata">POST data collection to be sent. GET request is sent if this parameter is null</param>
        /// <returns>Response of the HTTP server or null if the request failed</returns>
        public static HttpWebResponse GetResponse(HttpWebRequest request, out string errorresponce, NameValueCollection postdata = null)
        {
            errorresponce = "";

            //Force US english
            Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("en-US");

            //request.UnsafeAuthenticatedConnectionSharing = true;
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            //Enable SSL
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback(ValidateRemoteCertificate);

            try
            {
                //Process POST data if any
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
                    //Read error details
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
        /// Function converts collection of POST data to HTTP encoded form string
        /// </summary>
        /// <param name="postdata">Collection of POST data to be converted</param>
        /// <returns>HTTP encoded string</returns>
        public static string NameValueToHttpString(NameValueCollection postdata)
        {
            return String.Join("&", 
                Array.ConvertAll(postdata.AllKeys, key =>
                        String.Format("{0}={1}", 
                            HttpUtility.UrlEncode(key), 
                            HttpUtility.UrlEncode(postdata[key]))
                            )
                        );
        }
        
        /// <summary>
        /// Simple HTTP request
        /// </summary>
        /// <param name="url">URL to be processed by the HTTP client</param>
        /// <returns>HTTP response or null in case of error</returns>
        public static HttpWebResponse Request(string url)
        {
            string error = "";

            return Request(url, out error);
        }

        /// <summary>
        /// Comples HTTP request function
        /// </summary>
        /// <param name="url">URL link to be requested</param>
        /// <param name="error">Error defails in case of request failure</param>
        /// <param name="cookieContainer">HTTP client cookie container for keeping authentication etc.</param>
        /// <param name="postdata">POST data collection. GET request is sent if this variable is null.</param>
        /// <param name="host">Host value used in HTTP request header</param>
        /// <param name="referer">Referer value used in HTTP request header</param>
        /// <param name="origin">Origin value used in HTTP request header</param>
        /// <returns>HTTP response or null in case of error</returns>
        public static HttpWebResponse Request(string url, out string error, CookieContainer cookieContainer = null, NameValueCollection postdata = null, string host = "", string referer = "", string origin = "", string accept = "", string accept_encoding = "")
        {
            if (url == "")
            {
                error = "No url address was provided";
                return null;
            }
            HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;

            error = "";

            request.Method = (postdata == null) ? "GET" : "POST";
            request.Timeout = 60000;
            request.KeepAlive = false;
            if (accept == "") request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8"; else request.Accept = accept;
            request.Headers.Add("accept-language", "en-US");
            if (accept_encoding != "") request.Headers.Add("accept-encoding", accept_encoding);
            
            request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/61.0.3163.100 Safari/537.36";
            request.Proxy = null;
            request.Headers.Add("Upgrade-Insecure-Requests", "1");

            if (host != "") request.Host = host;
            if (referer != "") request.Referer = referer;
            if (origin != "") request.Headers.Add("Origin", origin);

            if (request.Method == "POST")
            {
                request.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
                request.Headers.Add("X-Requested-With", "XMLHttpRequest");
                request.Headers.Add("DNT", "1");
            }

            //Create new cookie container if no container was provided
            request.CookieContainer = cookieContainer ?? new CookieContainer();

            //Allow redirection
            request.AllowAutoRedirect = true;
            request.MaximumAutomaticRedirections = 10;

            System.Net.ServicePointManager.Expect100Continue = false;

            return WebClient.GetResponse(request, out error, postdata);
        }
    }
}
