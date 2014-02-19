using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Globalization;
using System.Collections.ObjectModel;

namespace MataSharp
{
    public class StudyGuide : IComparable<StudyGuide>
    {
        public string Name { get; set; }
        public int ID { get; set; }
        public bool Archived { get; set; }
        public bool Visible { get; set; }
        public List<StudyGuidePart> StudyGuideParts { get; set; }
        public DateTime ExpireDate { get; set; }
        public List<string> ClassCodes { get; set; }
        public DateTime BeginDate { get; set; }

        public int CompareTo(StudyGuide other)
        {
            var dateCompared = this.ExpireDate.CompareTo(other.ExpireDate);
            return (dateCompared != 0) ? dateCompared : this.ClassCodes[0].CompareTo(other.ClassCodes[0]);
        }
    }

    public class StudyGuidePart : IComparable<StudyGuidePart>
    {
        public ReadOnlyCollection<Attachment> Attachments { get; set; }
        public int ID { get; set; }
        public bool Visible { get; set; }
        public string Description { get; set; }
        public object Ref { get; set; }
        public string Name { get; set; }
        public DateTime ExpireDate { get; set; }
        public DateTime BeginDate { get; set; }
        public int SerialNumber { get; set; }

        public int CompareTo(StudyGuidePart other)
        {
            return this.ExpireDate.CompareTo(other.ExpireDate);
        }
    }

    internal struct StudieWijzerLijst
    {
        public StudieWijzerLijstItem[] Items { get; set; }
        public object Paging { get; set; }
        public int TotalCount { get; set; }
    }

    internal struct StudieWijzerLijstItem
    {
        public int Id { get; set; }
        public string Omschrijving { get; set; }
        public object Ref { get; set; }
        public string Titel { get; set; }
        public string TotEnMet { get; set; }
        public string[] VakCodes { get; set; }
        public string Van { get; set; }
    }

    sealed internal class StudieWijzer
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

        public StudyGuide ToStudyGuide(Mata mata)
        {
            var tmpStudyGuideParts = new List<StudyGuidePart>();
            foreach (var StudyGuidePartsListItem in this.Onderdelen.Items)
            {
                string URL = "https://" + mata.School.URL + "/api/leerlingen/" + mata.UserID + "/studiewijzers/" + this.Id + "/onderdelen/" + StudyGuidePartsListItem.Id;

                string partRaw = mata.WebClient.DownloadString(URL);
                var partClean = JsonConvert.DeserializeObject<StudieWijzerOnderdeel>(partRaw);

                tmpStudyGuideParts.Add(partClean.ToReadableStyle(this.Id, mata));
            }

            return new StudyGuide()
            {
                ID = this.Id,
                Archived = this.InLeerlingArchief,
                Visible = this.IsZichtbaar,
                StudyGuideParts = tmpStudyGuideParts,
                Name = this.Titel,
                BeginDate= this.Van.ToDateTime(),
                ClassCodes = new List<string>(VakCodes),
                ExpireDate = this.TotEnMet.ToDateTime()
            };
        }
    }

    internal struct StudieWijzerOnderdeelLijst
    {
        public StudieWijzerOnderdeelLijstItem[] Items { get; set; }
        public object Paging { get; set; }
        public int TotalCount { get; set; }
    }

    internal struct StudieWijzerOnderdeelLijstItem
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

    sealed internal class StudieWijzerOnderdeel
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

        public StudyGuidePart ToReadableStyle(int parentID, Mata mata) //;)
        {
            var thisID = this.Id;

            var tmpAttachments = this.Bronnen.ToList(AttachmentType.StudyGuide, mata);
            tmpAttachments.ForEach(a => a.StudyGuideID = parentID);
            tmpAttachments.ForEach(a => a.StudyGuidePartID = thisID);

            return new StudyGuidePart()
            {
                Attachments = tmpAttachments,
                ID = this.Id,
                Visible = this.IsZichtbaar,
                Description = this.Omschrijving,
                Ref = this.Ref,
                Name = this.Titel,
                ExpireDate = this.TotEnMet.ToDateTime(),
                BeginDate = this.Van.ToDateTime(),
                SerialNumber = this.Volgnummer
            };
        }
    }
}