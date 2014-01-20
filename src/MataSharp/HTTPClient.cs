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

        private const int TimeOut = 15000;

        public string Post(string URL, NameValueCollection Values)
        {
            byte[] tmpArr = this.client.UploadValues(URL, Values);
            string unStripped = Encoding.ASCII.GetString(tmpArr);
            return Regex.Replace(unStripped, "\r|\n", "\n");
        }

        public string DownloadString(string URL)
        {
            string stripped = Regex.Replace(this.client.DownloadString(URL), "<br />|<p />|<p>", "\n");
            stripped = Regex.Replace(stripped, "&nbsp;", " ");
            stripped = Regex.Replace(stripped, "<[^>]*>|&quot;|&#x200b;", "").Replace("ā,¬", "€").Replace("Ć«", "ë").Replace("Ć©", "é");
            return stripped;
        }

        public string Post(string URL, string Content)
        {
            this.client.Headers[HttpRequestHeader.ContentType] = "application/json;charset=UTF-8";
            byte[] tmpArr = this.client.UploadData(URL, System.Text.Encoding.ASCII.GetBytes(Content));
            string unStripped = Encoding.ASCII.GetString(tmpArr);
            return Regex.Replace(unStripped, "\r|\n", "\n");
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
            return Regex.Replace(unStripped, "\r|\n", "\n");
        }

        public string DownloadFile(string URL, string Filename, string DIR)
        {
            var currentDIR = Directory.GetCurrentDirectory() + "\\";
            var dir = (!string.IsNullOrWhiteSpace(DIR)) ? currentDIR + DIR + "\\" : currentDIR;

            this.client.DownloadFile(URL, Filename);

            if (!string.IsNullOrWhiteSpace(DIR))
            {
                if (!Directory.Exists(DIR)) Directory.CreateDirectory(DIR);

                string destination = dir + Filename;
                if (File.Exists(destination)) File.Delete(destination);
                File.Move(currentDIR + Filename, destination);
            }

            return dir + Filename;
        }

        public void Dispose() { this.client.Dispose(); }
    }
}
