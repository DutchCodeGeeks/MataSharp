using System;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace MataSharp
{
    public class MataHTTPClient
    {
        internal WebClient client = new WebClient();

        private const int TimeOut = 15000;

        public string Post(string URL, NameValueCollection Values)
        {
            byte[] tmpArr = this.client.UploadValues(URL, Values);
            string unStripped = Encoding.ASCII.GetString(tmpArr);
            return Regex.Replace(unStripped, "\r|\n", string.Empty);
        }

        public string Post(string URL, string Content)
        {
            this.client.Headers[HttpRequestHeader.ContentType] = "application/json;charset=UTF-8";
            byte[] tmpArr = this.client.UploadData(URL, System.Text.Encoding.ASCII.GetBytes(Content));
            string unStripped = Encoding.ASCII.GetString(tmpArr);
            return Regex.Replace(unStripped, "\r|\n", string.Empty);
        }

        public void Delete(string URL)
        {
            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(URL);
            req.Method = "DELETE";
            req.Headers[HttpRequestHeader.Cookie] = "SESSION_ID=" + _Session.Mata.SessionID + "&fileDownload=true";
            req.Timeout = TimeOut;
            req.GetResponse();
        }

        public string Put(string URL, string Data)
        {
            this.client.Headers[HttpRequestHeader.ContentType] = "application/json;charset=UTF-8";
            byte[] tmpArr = this.client.UploadData(URL, "PUT", System.Text.Encoding.ASCII.GetBytes(Data));
            string unStripped = Encoding.ASCII.GetString(tmpArr);
            return Regex.Replace(unStripped, "\r|\n", string.Empty);
        }

        public string DownloadFile(string URL, string filename, string Dir)
        {
            var currentDIR = Directory.GetCurrentDirectory() + "\\";
            var dir = (!string.IsNullOrWhiteSpace(Dir)) ? currentDIR + Dir + "\\" : currentDIR;

            this.client.DownloadFile(URL, filename);

            if (!string.IsNullOrWhiteSpace(Dir))
            {
                if (!Directory.Exists(Dir)) Directory.CreateDirectory(Dir);
                File.Move(currentDIR + filename, dir + filename);
            }

            return dir + filename;
        }
        public void Dispose() { this.client.Dispose(); }
    }
}
