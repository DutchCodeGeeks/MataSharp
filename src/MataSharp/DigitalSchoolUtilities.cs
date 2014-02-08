using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;

namespace MataSharp
{
    public partial class DigitalSchoolUtility : IComparable<DigitalSchoolUtility>
    {
        public long EAN { get; set; }
        public DateTime End { get; set; }
        public int ID { get; set; }
        public string ImagePreviewURL { get; set; }
        public object Ref { get; set; } //I get bored of you...
        public DateTime Start { get; set; }
        public int State { get; set; }
        public string Name { get; set; }
        public string Publisher { get; set; }
        public string URL { get; set; }

        public DigitalSchoolUtilityClass Class { get; set; }

        public override string ToString() { return this.Name; }

        public int CompareTo(DigitalSchoolUtility other)
        {
            var classCompared = this.Class.CompareTo(other.Class);
            return (classCompared != 0) ? classCompared : this.Name.CompareTo(other.Name);
        }
    }

    public partial class DigitalSchoolUtilityClass : IComparable<DigitalSchoolUtilityClass>
    {
        public string ClassAbbreviation { get; set; }
        public int ID { get; set; }
        public string Description { get; set; }

        public override string ToString() { return this.Description; }

        public int CompareTo(DigitalSchoolUtilityClass other)
        {
            return this.ClassAbbreviation.CompareTo(other.ClassAbbreviation);
        }
    }

    internal partial struct DigitaalLesMatriaalLijstItem
    {
        public string Afkorting { get; set; }
        public int Id { get; set; }
        public string LicentieUrl { get; set; }
        public string Omschrijving { get; set; }
        public object Ref { get; set; }
    }

    internal partial struct DigitaalLesMatriaal
    {
        public long EAN { get; set; }
        public string Eind { get; set; }
        public int Id { get; set; }
        public string PreviewImageUrl { get; set; }
        public object Ref { get; set; }
        public string Start { get; set; }
        public int Status { get; set; }
        public string Titel { get; set; }
        public string Uitgeverij { get; set; }
        public string Url { get; set; }
        public DigitaalLesMatriaalVakDetails Vak { get; set; }

        internal DigitalSchoolUtility ToDigitalSchoolUtility()
        {
            return new DigitalSchoolUtility()
            {
                ID = this.Id,
                EAN = this.EAN,
                ImagePreviewURL = this.PreviewImageUrl,
                Class = this.Vak.ToReadable(),
                Publisher = this.Uitgeverij,
                URL = this.Url,
                Ref = this.Ref,
                End = this.Eind.ToDateTime(),
                Start = this.Start.ToDateTime(),
                Name = this.Titel,
                State = this.Status
            };
        }
    }

    internal partial struct DigitaalLesMatriaalVakDetails
    {
        public string Afkorting { get; set; }
        public int Id { get; set; }
        public string Omschrijving { get; set; }

        public DigitalSchoolUtilityClass ToReadable()
        {
            return new DigitalSchoolUtilityClass()
            {
                ID = this.Id,
                Description = this.Omschrijving,
                ClassAbbreviation = this.Afkorting
            };
        }
    }
}
