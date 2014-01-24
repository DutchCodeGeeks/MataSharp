using System;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;

namespace MataSharp
{
    public class MataHTTPClient
    {
        internal WebClient client = new WebClient();

        private static string stripString(string original)
        {
            string stripped = Regex.Replace(original, "<br />|<p />|<p>", "\n");
            stripped = stripped.Replace("&nbsp;", " ");
            stripped = Regex.Replace(stripped, "<[^>]*>|&quot;|&#x200b;", "").Replace("ā,¬", "€").Replace("Ć«", "ë").Replace("Ć©", "é");
            return stripped;
        }

        public string DownloadString(string URL)
        {
            return stripString(this.client.DownloadString(URL));
        }

        public void Put(string URL, string Data)
        {
            this.client.Headers[HttpRequestHeader.ContentType] = "application/json;charset=UTF-8";
            this.client.UploadData(URL, "PUT", System.Text.Encoding.ASCII.GetBytes(Data));
        }

        public string Post(string URL, NameValueCollection Values)
        {
            byte[] tmpArr = this.client.UploadValues(URL, Values);
            return stripString(Encoding.ASCII.GetString(tmpArr));
        }

        public void Post(string URL, string Content)
        {
            this.client.Headers[HttpRequestHeader.ContentType] = "application/json;charset=UTF-8";
            this.client.UploadData(URL, System.Text.Encoding.ASCII.GetBytes(Content));
        }

        public void Delete(string URL)
        {
            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(URL);
            req.Method = "DELETE";
            req.Headers[HttpRequestHeader.Cookie] = "SESSION_ID=" + _Session.Mata.SessionID + "&fileDownload=true";
            req.Timeout = 15000;
            req.GetResponse();
        }

        public string DownloadFile(string URL, string Filename, string DIR)
        {
            var currentDIR = Directory.GetCurrentDirectory() + "\\";
            var destination = (!string.IsNullOrWhiteSpace(DIR)) ? currentDIR + DIR + "\\" : currentDIR;
            var fullPath = destination + Filename;

            if (!string.IsNullOrWhiteSpace(DIR))
            {
                if (!Directory.Exists(DIR)) Directory.CreateDirectory(DIR);
                this.client.DownloadFile(URL, fullPath);
            }

            else this.client.DownloadFile(URL, Filename);

            return fullPath;
        }

        public void Dispose() { this.client.Dispose(); }
    }
}
