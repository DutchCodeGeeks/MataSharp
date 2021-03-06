﻿using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace MataSharp
{
    sealed public class MagisterPerson : IComparable<MagisterPerson>, ICloneable, IEqualityComparer<MagisterPerson>
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
        internal int _GroupID;
        public PersonType PersonType { get { return (PersonType)this._GroupID; } }

        internal MagisterPerson Original { get; set; }

        public override bool Equals(object obj)
        {
            var person = obj as MagisterPerson;
            if (person == null) throw new ArgumentException("Argument must implicitly convertible to MagisterPerson");

            return this.Equals(person);
        }

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
            var tmpPerson = (!this.Original.Equals(this)) ? this.Original : this;

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
                    Type = tmpPerson._GroupID
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

        public override string ToString() { return this.Description; }

        public int CompareTo(MagisterPerson other)
        {
            var surNameCompared = this.SurName.CompareTo(other.SurName);
            return (surNameCompared != 0) ? surNameCompared : this.FirstName.CompareTo(other.FirstName);
        }

        object ICloneable.Clone()
        {
            return this.Clone();
        }

        public bool Equals(MagisterPerson x, MagisterPerson y)
        {
            return x.Equals(y);
        }

        public int GetHashCode(MagisterPerson obj)
        {
            return obj.Original.GetHashCode();
        }
    }

    public enum PersonType : int
    {
        Pupil = 4,
        Teacher = 3,
        Group = 1,
        Project = 8
    }

    sealed internal class MagisterStylePerson
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

        private static MagisterStylePerson[] GetPersons(string SearchFilter, Mata mata)
        {
            if (string.IsNullOrWhiteSpace(SearchFilter) || SearchFilter.Length < 3) return new MagisterStylePerson[0];

            if (!mata.CheckedPersons.ContainsKey(SearchFilter))
            {
                string URL = "https://" + mata.School.URL + "/api/personen/" + mata.UserID + "/communicatie/contactpersonen?q=" + SearchFilter;

                string personsRAW = mata.WebClient.DownloadString(URL);

                var persons = JsonConvert.DeserializeObject<MagisterStylePerson[]>(personsRAW);
                mata.CheckedPersons.Add(SearchFilter, persons);
                return persons;
            }
            else return mata.CheckedPersons.First(x => x.Key.ToUpper() == SearchFilter.ToUpper()).Value;
        }

        public MagisterPerson ToPerson(bool download, Mata mata)
        {
            MagisterStylePerson tmpPerson;
            if (download)
            {
                var downloadedPersons = GetPersons(this.Naam, mata);

                try { tmpPerson = (downloadedPersons.Length == 1) ? downloadedPersons.Single() : this; } //Main building ground.
                catch { tmpPerson = this; }
            }
            else tmpPerson = this;

            List<string> splitted = (tmpPerson.Naam != null) ? tmpPerson.Naam.Split(' ').ToList() : null;

            string tmpFirstName = (tmpPerson.Voornaam == null && splitted != null) ? splitted[0] : tmpPerson.Voornaam + tmpPerson.Voornamen;
            string tmpSurName = (tmpPerson.Achternaam == null && splitted != null) ? splitted[splitted.Count - 1] : tmpPerson.Achternaam;
            string tmpPrefix = tmpPerson.Tussenvoegsel;

            if (tmpPrefix == null)
            {
                try
                {
                    splitted.RemoveAt(0); splitted.Remove(splitted.Last());

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
                Description = tmpPerson.Omschrijving ?? tmpName,
                Group = tmpPerson.Groep,
                TeacherCode = (!string.IsNullOrWhiteSpace(tmpPerson.DocentCode)) ? tmpPerson.DocentCode : null,
                _GroupID = tmpPerson.Type,
                Initials = tmpPerson.Voorletters,
            };

            person.Original = person.Clone();
            return person;
        }
    }
}
