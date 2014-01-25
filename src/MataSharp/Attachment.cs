using System;
using System.Collections.Generic;
using System.Web;
using Newtonsoft.Json;

namespace MataSharp
{
    public partial class Attachment
    {
        [JsonProperty("Id")]
        public int ID { get; internal set; }
        [JsonProperty("Ref")]
        public object Ref { get; internal set; } // Even Schoolmaster doesn't know what this is, it's mysterious. Just keep it in case.
        [JsonProperty("Naam")]
        public string Name { get; internal set; }
        [JsonProperty("Datum")]
        public DateTime Date { get; internal set; }
        [JsonProperty("Grootte")]
        public int Size { get; internal set; }

        [JsonIgnore]
        internal AttachmentType Type { get; set; }

        [JsonIgnore]
        internal int StudyGuideID { get; set; }
        [JsonIgnore]
        internal int StudyGuidePartID { get; set; }

        [JsonIgnore]
        public string MIME
        {
            get { return (!string.IsNullOrWhiteSpace(this.Name)) ? MimeMapping.GetMimeMapping(this.Name) : "application/octet-stream"; }
        }

        private string URL()
        {
            if (this.Type == AttachmentType.Message)
                return "https://" + _Session.School.URL + "/api/personen/" + _Session.Mata.UserID + "/communicatie/berichten/bijlagen/" + this.ID;

            else if (this.Type == AttachmentType.Assignment_pupil)
                return "https://" + _Session.School.URL + "/api/leerlingen/" + _Session.Mata.UserID + "/opdrachten/bijlagen/Ingeleverd/" + this.ID;

            else if (this.Type == AttachmentType.Assignment_teacher)
                return "https://" + _Session.School.URL + "/api/leerlingen/" + _Session.Mata.UserID + "/opdrachten/bijlagen/" + this.ID;

            else
                return "https://" + _Session.School.URL + "/api/leerlingen/" + _Session.Mata.UserID + "/studiewijzers/" + this.StudyGuideID + "/onderdelen/" + this.StudyGuidePartID + "/bijlagen/" + this.ID;
        }

        /// <summary>
        /// Downloads the current attachment.
        /// </summary>
        /// <param name="AddUserID">Boolean value whether to add the UserID in front of the filename or not.</param>
        /// <returns>A string containting the path to the location of the downloaded attachment.</returns>
        public string Download(bool AddUserID, string Directory = "")
        {
            string fileName = (AddUserID) ? ("(" + _Session.Mata.UserID + ") " + this.Name) : (this.Name);
            return _Session.HttpClient.DownloadFile(URL(), fileName, Directory);
        }
    }

    internal enum AttachmentType
    {
        Message,
        Assignment_teacher,
        Assignment_pupil,
        StudyGuide
    };

    static class ArrayExtension
    {
        /// <summary>
        /// Convert the current Attachment[] to a List
        /// </summary>
        /// <param name="AttachmentType">AttachmentType to give every attachment in the array.</param>
        /// <returns>The array as list</returns>
        public static List<Attachment> ToList(this Attachment[] currentArray, AttachmentType AttachmentType)
        {
            var tmpList = new List<Attachment>(currentArray);
            tmpList.ForEach(a => a.Type = AttachmentType);
            return tmpList;
        }
    }
}
