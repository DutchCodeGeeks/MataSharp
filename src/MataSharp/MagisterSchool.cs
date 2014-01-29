using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MataSharp
{
    /// <summary>
    /// Type to represent a magister school.
    /// </summary>
    public class MagisterSchool
    {
        [JsonProperty("Licentie")]
        public string Name { get; set; }
        [JsonProperty("Url")]
        public string URL { get; set; }

        /// <summary>
        /// Returns all Magister/Mata schools filterd by the given search filter as a list.
        /// </summary>
        /// <param name="SearchFilter">The search filter to use as string.</param>
        /// <returns>List containing MagisterSchool instances</returns>
        public static List<MagisterSchool> GetSchools(string SearchFilter)
        {
            if (string.IsNullOrWhiteSpace(SearchFilter) || SearchFilter.Count() < 3) return new List<MagisterSchool>();

            string URL = "https://schoolkiezer.magister.net/home/query?filter=" + SearchFilter;

            string schoolsRAW = _Session.HttpClient.DownloadString(URL);
            return JsonConvert.DeserializeObject<MagisterSchool[]>(schoolsRAW).ToList();
        }

        public MagisterSchool Clone()
        {
            return (MagisterSchool)this.MemberwiseClone();
        }

        public bool Equals(MagisterSchool School)
        {
            return (School != null && this.Name == School.Name && this.URL == School.URL);
        }
    }
}
