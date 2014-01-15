using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Globalization;

namespace MataSharp
{
    public partial class StudyGuide
    {
        public string Name { get; set; }
        public int ID { get; set; }
        public bool Archived { get; set; }
        public bool Visible { get; set; }
        public List<StudyGuidePart> StudyGuideParts { get; set; }
        public DateTime ExpireDate { get; set; }
        public List<string> ClassCodes { get; set; }
        public DateTime BeginDate { get; set; }
    }

    public partial class StudyGuidePart
    {
        public List<Attachment> Attachments { get; set; }
        public int ID { get; set; }
        public bool Visible { get; set; }
        public string Description { get; set; }
        public object Ref { get; set; }
        public string Name { get; set; }
        public DateTime ExpireDate { get; set; }
        public DateTime BeginDate { get; set; }
        public int SerialNumber { get; set; }
    }

    internal partial struct StudieWijzerLijst
    {
        public StudieWijzerLijstItem[] Items { get; set; }
        public object Paging { get; set; }
        public int TotalCount { get; set; }
    }

    internal partial struct StudieWijzerLijstItem
    {
        public int Id { get; set; }
        public string Omschrijving { get; set; }
        public object Ref { get; set; }
        public string Titel { get; set; }
        public string TotEnMet { get; set; }
        public string[] VakCodes { get; set; }
        public string Van { get; set; }
    }

    internal partial struct StudieWijzer
    {
        public int Id { get; set; }
        public bool InLeerlingArchief { get; set; }
        public bool IsZichtbaar { get; set; }
        public string Omschrijving { get; set; }
        public StudieWijzerOnderdeelLijst Onderdelen { get; set; }
        public object Ref { get; set; }
        public string Titel { get; set; }
        public string TotEnMet { get; set; }
        public string[] VakCodes { get; set; }
        public string Van { get; set; }

        public StudyGuide ToStudyGuide()
        {
            var tmpStudyGuideParts = new List<StudyGuidePart>();
            foreach(var StudyGuidePartsListItem in this.Onderdelen.Items)
            {
                string URL = "https://" + _Session.School.URL + "/api/leerlingen/" + _Session.Mata.UserID + "/studiewijzers/" + this.Id + "/onderdelen/" + StudyGuidePartsListItem.Id;

                string partRaw = _Session.HttpClient.DownloadString(URL);
                var partClean = JsonConvert.DeserializeObject<StudieWijzerOnderdeel>(partRaw);

                tmpStudyGuideParts.Add(partClean.ToReadableStyle());
            }

            var expireDate = (!string.IsNullOrWhiteSpace(this.TotEnMet)) ? DateTime.Parse(this.TotEnMet, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal) : new DateTime();
            var beginDate = (!string.IsNullOrWhiteSpace(this.Van)) ? DateTime.Parse(this.Van, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal) : new DateTime();

            return new StudyGuide()
            {
                ID = this.Id,
                Archived = this.InLeerlingArchief,
                Visible = this.IsZichtbaar,
                StudyGuideParts = tmpStudyGuideParts,
                Name = this.Titel,
                BeginDate= beginDate,
                ClassCodes = new List<string>(VakCodes),
                ExpireDate = expireDate
            };
        }
    }

    internal partial struct StudieWijzerOnderdeelLijst
    {
        public StudieWijzerOnderdeelLijstItem[] Items { get; set; }
        public object Paging { get; set; }
        public int TotalCount { get; set; }
    }

    internal partial struct StudieWijzerOnderdeelLijstItem
    {
        public object Bronnen { get; set; } //Have to take a look at that
        public int Id { get; set; }
        public bool IsZichtbaar { get; set; }
        public string Kleur { get; set; } //Irrelevant
        public string Omschrijving { get; set; }
        public object Ref { get; set; }
        public string Titel { get; set; }
        public string TotEnMet { get; set; }
        public string Van { get; set; }
        public int Volgnummer { get; set; }
    }

    internal partial struct StudieWijzerOnderdeel
    {
        public Attachment[] Bronnen { get; set; }
        public int Id { get; set; }
        public bool IsZichtbaar { get; set; }
        public string Kleur { get; set; }
        public string Omschrijving { get; set; }
        public object Ref { get; set; }
        public string Titel { get; set; }
        public string TotEnMet { get; set; }
        public string Van { get; set; }
        public int Volgnummer { get; set; }

        public StudyGuidePart ToReadableStyle() //;)
        {
            var expireDate = (!string.IsNullOrWhiteSpace(this.TotEnMet)) ? DateTime.Parse(this.TotEnMet, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal) : new DateTime();
            var beginDate = (!string.IsNullOrWhiteSpace(this.Van)) ? DateTime.Parse(this.Van, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal) : new DateTime();
            
            return new StudyGuidePart()
            {
                Attachments = new List<Attachment>(this.Bronnen),
                ID = this.Id,
                Visible = this.IsZichtbaar,
                Description = this.Omschrijving,
                Ref = this.Ref,
                Name = this.Titel,
                ExpireDate = expireDate,
                BeginDate = beginDate,
                SerialNumber = this.Volgnummer
            };
        }
    }
}
