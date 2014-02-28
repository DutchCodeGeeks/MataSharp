using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MataSharp
{
    /// <summary>
    /// Type to represent a magister school.
    /// </summary>
    public class MagisterSchool : IComparable<MagisterSchool>, ICloneable
    {
        [JsonProperty("Licentie")]
        public string Name { get; set; }
        [JsonProperty("Url")]
        public string URL { get; set; }

        /// <summary>
        /// Returns all Magister/Mata schools filtered by the given search filter as a list.
        /// </summary>
        /// <param name="SearchFilter">The search filter to use as string.</param>
        /// <returns>List containing MagisterSchool instances</returns>
        public static IReadOnlyList<MagisterSchool> GetSchools(string SearchFilter)
        {
            if (string.IsNullOrWhiteSpace(SearchFilter) || SearchFilter.Length < 3) return new List<MagisterSchool>(0);

            string URL = "https://schoolkiezer.magister.net/home/query?filter=" + SearchFilter;

            string schoolsRAW = new System.Net.WebClient().DownloadString(URL);
            return JsonConvert.DeserializeObject<MagisterSchool[]>(schoolsRAW).ToList();
        }

        public MagisterSchool Clone()
        {
            return (MagisterSchool)this.MemberwiseClone();
        }

        public override bool Equals(object obj)
        {
            return this.Equals((MagisterSchool)obj);
        }

        public bool Equals(MagisterSchool School)
        {
            return (School != null && this.Name == School.Name && this.URL == School.URL);
        }

        public int CompareTo(MagisterSchool other)
        {
            return this.Name.CompareTo(other.Name);
        }

        public override string ToString()
        {
            return this.Name + " - " + this.URL;
        }

        object ICloneable.Clone()
        {
            return this.Clone();
        }
    }
}