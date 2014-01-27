using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Globalization;
using Newtonsoft.Json;

namespace MataSharp
{
    public partial class Homework
    {
        public string Notes { get; set; }
        internal bool _Done;
        public bool Done
        {
            get { return this._Done; }
            set
            {
                if (this._Done == value) return;

                this._Done = value;
                _Session.HttpClient.Put(this.URL(), JsonConvert.SerializeObject(this.ToMagisterStyle()));
            }
        }
        public List<MagisterPerson> Teachers { get; set; }
        public DateTime End { get; set; }
        public int ID { get; set; }
        internal int InfoType;
        public HomeworkType Type
        {
            get { return HomeworkType_ID.First(x => x.Value == this.InfoType).Key; }
        }
        public string Content { get; set; }
        public int EndBySchoolHour { get; set; }
        public int BeginBySchoolHour { get; set; }
        public string Location { get; set; }
        public string ClassDescription { get; set; }
        public DateTime Start { get; set; }
        public int State { get; set; }
        public string ClassName { get; set; }
        public string ClassAbbreviation { get; set; }

        internal readonly static Dictionary<HomeworkType, int> HomeworkType_ID = new Dictionary<HomeworkType, int>()
        {
            {HomeworkType.Unknown, 0},
            {HomeworkType.Normal, 1},
            {HomeworkType.Test, 2},
            {HomeworkType.Exam, 3},
            {HomeworkType.Quiz, 4},
            {HomeworkType.OralTest, 5},
            {HomeworkType.Information, 6}
        };

        internal string URL() { return "https://" + _Session.School.URL + "/api/leerlingen/" + _Session.Mata.UserID + "/huiswerk/huiswerk/" + this.ID; }

        internal Huiswerk ToMagisterStyle()
        {
            return new Huiswerk()
            {
                AantekeningLeerling = this.Notes,
                Afgerond  = this.Done,
                Id = this.ID,
                Docenten = this.Teachers.ConvertAll(p => p.ToMagisterStyle()).ToArray(),
                Einde = this.End.ToString("yyyy-MM-ddTHH:mm:ss.0000000Z"),
                Start = this.Start.ToString("yyyy-MM-ddTHH:mm:ss.0000000Z"),
                InfoType = this.InfoType,
                Inhoud = this.Content,
                LesuurTM = this.EndBySchoolHour,
                LesuurVan = this.BeginBySchoolHour,
                Lokatie = this.Location,
                Omschrijving = this.ClassName,
                VakOmschrijvingen = this.ClassDescription,
                Status = this.State
            };
        }
    }

    public enum HomeworkType
    {
        Unknown,
        Normal,
        Test,
        Exam,
        Quiz,
        OralTest,
        Information
    }

    internal partial struct HuiswerkLijst
    {
        public HuiswerkItemLijstDag[] Items { get; set; }
        public object Paging { get; set; }
        public int TotalCount { get; set; }
    }

    internal partial struct HuiswerkItemLijstDag
    {
        public string Datum { get; set; }
        public HuiswerkItemLijstDagItem[] Items { get; set; }
    }
    
    internal partial struct HuiswerkItemLijstDagItem
    {
        public bool Afgerond { get; set; }
        public string Datum { get; set; }
        public int Id { get; set; }
        public int InfoType { get; set; }
        public string Inhoud { get; set; }
        public int Lesuur { get; set; } //Can't wait to merge this with GEPRO_OSIsharp ;D
        public string Omschrijving { get; set; }
        public object Ref { get; set; }
        public string VakAfkortingen { get; set; }
        public string VakOmschrijvingen { get; set; }
    }

    internal partial class Huiswerk
    {
        public string AantekeningLeerling { get; set; }
        public bool Afgerond { get; set; }
        public MagisterStylePerson[] Docenten { get; set; }
        public string Einde { get; set; }
        public int Id { get; set; }
        public int InfoType { get; set; }
        public string Inhoud { get; set; }
        public int LesuurTM { get; set; } //? TradeMark?! ;)
        public int LesuurVan { get; set; }
        public string Lokatie { get; set; }
        public string Omschrijving { get; set; }
        public string Start { get; set; }
        public int Status { get; set; }
        public string VakOmschrijvingen { get; set; }

        public Homework ToHomework()
        {
            return new Homework()
            {
                Notes = this.AantekeningLeerling,
                _Done = this.Afgerond,
                Teachers = this.Docenten.ToList().ConvertAll(p => p.ToPerson(true)),
                End = this.Einde.ToDateTime(),
                ID = this.Id,
                InfoType = this.InfoType,
                Content = this.Inhoud,
                EndBySchoolHour = this.LesuurTM,
                BeginBySchoolHour = this.LesuurVan,
                Location = this.Lokatie,
                ClassDescription = this.Omschrijving,
                Start = this.Start.ToDateTime(),
                State = this.Status,
                ClassName = this.VakOmschrijvingen
            };
        }
    }
}