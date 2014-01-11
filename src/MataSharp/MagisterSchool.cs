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
            if (string.IsNullOrWhiteSpace(SearchFilter)) return new List<MagisterSchool>();

            string URL = "https://schoolkiezer.magister.net/home/query?filter=" + SearchFilter;

            string schoolsRAW = _Session.HttpClient.client.DownloadString(URL);
            return JArray.Parse(schoolsRAW).ToList().ConvertAll(s => s.ToObject<MagisterSchool>());
        }

        public bool IsEqual(MagisterSchool School)
        {
            if (School != null && this.Name == School.Name && this.URL == School.URL)
                return true;
            else
                return false;
        }
    }
}
