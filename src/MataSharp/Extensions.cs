using System;
using System.Globalization;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Newtonsoft.Json;

namespace MataSharp
{
    public static class Extensions
    {
        #region General
        public static void ForEach<T>(this IEnumerable<T> collection, Action<T> action)
        {
            foreach (var item in collection) action(item);
        }

        public static List<Tout> ConvertAll<Tin, Tout>(this IEnumerable<Tin> current, Converter<Tin, Tout> converter)
        {
            var tmpList = new List<Tout>();
            foreach (var item in current) tmpList.Add(converter(item));
            return tmpList;
        }
        #endregion

        #region DateTime
        /// <summary>
        /// Converts the current DateTime instance to a string.
        /// </summary>
        /// <returns>The current DateTime instance as string.</returns>
        internal static string ToUTCString(this DateTime original)
        {
            return original.ToString("yyyy-MM-ddTHH:mm:ss.0000000Z");
        }

        /// <summary>
        /// Gets the DayOfWeek from the current DateTime instance in Dutch.
        /// </summary>
        /// <returns>Dutch day of week that represents the day of the current DateTime instance.</returns>
        internal static string DayOfWeekDutch(this DateTime Date)
        {
            switch (Date.DayOfWeek)
            {
                case DayOfWeek.Monday: return "maandag";
                case DayOfWeek.Tuesday: return "dinsdag";
                case DayOfWeek.Wednesday: return "woensdag";
                case DayOfWeek.Thursday: return "donderdag";
                case DayOfWeek.Friday: return "vrijdag";
                case DayOfWeek.Saturday: return "zaterdag";
                case DayOfWeek.Sunday: return "zondag";
                default: return "";
            }
        }

        /// <summary>
        /// Converts the current DateTime instance to a string.
        /// </summary>
        /// <param name="dutch">If the day should be in Dutch or in English</param>
        /// <returns>The current DateTime instance as a string.</returns>
        internal static string ToString(this DateTime Date, bool dutch)
        {
            return (dutch) ? (Date.DayOfWeekDutch() + " " + Date.ToString()) : (Date.DayOfWeek + " " + Date.ToString());
        }

        /// <summary>
        /// Converts the current string to a DateTime.
        /// </summary>
        /// <returns>The string parsed as DateTime</returns>
        internal static DateTime ToDateTime(this String original)
        {
            return (!string.IsNullOrWhiteSpace(original)) ? DateTime.Parse(original, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal) : new DateTime();
        }
        #endregion

        #region Attachments
        /// <summary>
        /// Convert the current Attachment[] to a List
        /// </summary>
        /// <param name="attachmentType">AttachmentType to give every attachment in the array.</param>
        /// <returns>The array as list</returns>
        internal static ReadOnlyCollection<Attachment> ToList(this Attachment[] currentArray, AttachmentType attachmentType, Mata mata)
        {
            var tmpList = new List<Attachment>(currentArray);
            tmpList.ForEach(a => a.Type = attachmentType);
            tmpList.ForEach(a => a.Mata = mata);
            return new ReadOnlyCollection<Attachment>(tmpList);
        }
        #endregion

        #region PersonList
        internal static PersonList ToList(this IEnumerable<MagisterStylePerson> collection, bool download, bool ReadOnly, Mata mata)
        {
            return new PersonList(mata, collection, ReadOnly, download);
        }

        public  static PersonList ToList(this IEnumerable<MagisterPerson> collection, Mata mata)
        {
            return new PersonList(collection, mata);
        }
        #endregion
    }
}