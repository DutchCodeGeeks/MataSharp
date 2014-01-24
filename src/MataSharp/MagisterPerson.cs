using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MataSharp
{
    public partial class MagisterPerson
    {
        public uint ID { get; set; }
        public object Ref { get; set; } // Even Schoolmaster doesn't know what this is, it's mysterious. Just keep it in case.
        public string Initials { get; set; }
        public string SurName { get; set; }
        public string FirstName { get; set; }
        public string NamePrefix { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Group { get; set; }
        public string TeacherCode { get; set; }
        public int GroupID { get; internal set; }

        internal MagisterPerson Original { get; set; }

        public bool Equals(MagisterPerson Person)
        {
            return (this.ID == Person.ID && this.Initials == Person.Initials && this.SurName == Person.SurName &&
                this.FirstName == Person.FirstName && this.NamePrefix == Person.NamePrefix &&
                this.Name == Person.Name && this.Description == Person.Description &&
                this.Group == Person.Group && this.TeacherCode == Person.TeacherCode);
        }

        /// <summary>
        /// Converts the current MagisterPerson instance to a MagisterStylePerson
        /// </summary>
        /// <returns>A MagisterStylePerson instance.</returns>
        internal MagisterStylePerson ToMagisterStyle()
        {
            var tmpPerson = (!this.Original.Equals(this)) ?
                (_Session.Mata.GetPersons(this.Name).Count == 1) ? _Session.Mata.GetPersons(this.Name)[0] : this
                : this;
            return new MagisterStylePerson()
                {
                    Id = tmpPerson.ID,
                    Ref = tmpPerson.Ref,
                    Achternaam = tmpPerson.SurName,
                    Voornaam = tmpPerson.FirstName,
                    Tussenvoegsel = tmpPerson.NamePrefix,
                    Naam = tmpPerson.Name,
                    Omschrijving = tmpPerson.Description,
                    Groep = tmpPerson.Group,
                    DocentCode = tmpPerson.TeacherCode,
                    Type = tmpPerson.GroupID
                };
        }

        /// <summary>
        /// Clones the current MagisterPerson instance.
        /// </summary>
        /// <returns>A new MagisterPerson instance that's identical to the current one.</returns>
        public MagisterPerson Clone()
        {
            return (MagisterPerson)this.MemberwiseClone();
        }
    }
    internal partial struct MagisterStylePerson
    {
        public uint Id { get; set; }
        public object Ref { get; set; } // Even Schoolmaster doesn't know what this is, it's mysterious. Just keep it in case.
        public string Achternaam { get; set; }
        public string Voornaam { get; set; }
        public string Tussenvoegsel { get; set; }
        public string Naam { get; set; }
        public string Omschrijving { get; set; }
        public string Groep { get; set; }
        public string DocentCode { get; set; }
        public int Type { get; set; }
        public string Voornamen { get; set; }
        public string Voorletters { get; set; }

        private static List<MagisterStylePerson> GetPersons(string SearchFilter)
        {
            if (SearchFilter == null || !_Session.checkedPersons.ContainsKey(SearchFilter))
            {
                if (string.IsNullOrWhiteSpace(SearchFilter) || SearchFilter.Count() < 3) return new List<MagisterStylePerson>();

                string URL = "https://" + _Session.School.URL + "/api/personen/" + _Session.Mata.UserID + "/communicatie/contactpersonen?q=" + SearchFilter;

                string personsRAW = _Session.HttpClient.DownloadString(URL);

                var persons = JsonConvert.DeserializeObject<MagisterStylePerson[]>(personsRAW).ToList();
                _Session.checkedPersons.Add(SearchFilter, persons);
                return persons;
            }
            else
            {
                return _Session.checkedPersons.First(x => x.Key == SearchFilter).Value;
            }
        }

        public MagisterPerson ToPerson(bool download)
        {
            MagisterStylePerson tmpPerson;
            if (download)
            {
                try { tmpPerson = (MagisterStylePerson.GetPersons(this.Naam).Count == 1) ? MagisterStylePerson.GetPersons(this.Naam)[0] : this; } //Main building ground.
                catch { tmpPerson = this; }
            }
            else { tmpPerson = this; }

            List<string> splitted = (tmpPerson.Naam != null) ? tmpPerson.Naam.Split(' ').ToList() : null;

            string tmpFirstName = (tmpPerson.Voornaam == null && splitted != null) ? splitted[0] : tmpPerson.Voornaam + tmpPerson.Voornamen;
            string tmpSurName = (tmpPerson.Achternaam == null && splitted != null) ? splitted[splitted.Count - 1] : tmpPerson.Achternaam;
            string tmpPrefix = tmpPerson.Tussenvoegsel;

            if (tmpPrefix == null)
            {
                try
                {
                    splitted.RemoveAt(0); splitted.RemoveAt(splitted.Count - 1);

                    if (splitted != null && splitted.Count != 0 && splitted[0] != "")
                        tmpPrefix = string.Join(" ", splitted);
                }
                catch {/*When there isn't a prefix*/}
            }

            string tmpName = (tmpPerson.Naam == null && tmpFirstName != null && tmpSurName != null && tmpPrefix != null) ? 
                string.Concat(tmpFirstName, " ", tmpPrefix, " ", tmpSurName) :
                (tmpPerson.Naam == null && tmpFirstName != null && tmpSurName != null) ? string.Concat(tmpFirstName, " ", tmpSurName) : 
                tmpPerson.Naam;
            //This all above ISN'T needed but makes everything a bit nicer :)

            var person = new MagisterPerson()
            {
                ID = tmpPerson.Id,
                Ref = tmpPerson.Ref,
                SurName = tmpSurName,
                FirstName = tmpFirstName,
                NamePrefix = tmpPrefix,
                Name = tmpName,
                Description = tmpPerson.Omschrijving,
                Group = tmpPerson.Groep,
                TeacherCode = tmpPerson.DocentCode,
                GroupID = tmpPerson.Type,
                Initials = tmpPerson.Voorletters,
            };

            person.Original = person.Clone();
            return person;
        }
    }
}
