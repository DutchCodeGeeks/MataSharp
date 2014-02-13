using System;
using System.Web;
using Newtonsoft.Json;

namespace MataSharp
{
    public partial class Attachment : IComparable<Attachment>
    {
        [JsonProperty("Id")]
        public int ID { get; internal set; }
        [JsonProperty("Ref")]
        public object Ref { get; internal set; } // Even Schoolmaster doesn't know what this is, it's mysterious. Just keep it in case.
        [JsonProperty("Naam")]
        public string Name { get; internal set; }
        [JsonProperty("Datum")]
        internal DateTime _Date { get; set; }
        [JsonIgnore]
        public DateTime Date { get { return this._Date.Date; } }
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

        private string URL
        {
            get
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
        }

        /// <summary>
        /// Downloads the current attachment.
        /// </summary>
        /// <param name="AddUserID">Boolean value whether to add the UserID in front of the filename or not.</param>
        /// <param name="Directory">The directory to save the file to.</param>
        /// <returns>A string containting the path to the location of the downloaded attachment.</returns>
        public string Download(bool AddUserID, string Directory = "")
        {
            string fileName = (AddUserID) ? ("(" + _Session.Mata.UserID + ") " + this.Name) : (this.Name);
            return _Session.HttpClient.DownloadFile(this.URL, fileName, Directory);
        }

        public int CompareTo(Attachment other)
        {
            var nameCompared = this.Name.CompareTo(other.Name);
            return (nameCompared != 0) ? nameCompared : this.Date.CompareTo(other.Date);
        }

        public override string ToString() { return this.Name; }
    }

    internal enum AttachmentType
    {
        Message,
        Assignment_teacher,
        Assignment_pupil,
        StudyGuide
    }
}
