using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;

namespace MataSharp
{
    public static class Extensions
    {
        /// <summary>
        /// Converts the current string to a DateTime.
        /// </summary>
        /// <returns>The string parsed as DateTime</returns>
        internal static DateTime ToDateTime(this String original)
        {
            return (!string.IsNullOrWhiteSpace(original)) ? DateTime.Parse(original, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal) : new DateTime();
        }

        /// <summary>
        /// Convert the current Attachment[] to a List
        /// </summary>
        /// <param name="AttachmentType">AttachmentType to give every attachment in the array.</param>
        /// <returns>The array as list</returns>
        internal static ReadOnlyCollection<Attachment> ToList(this Attachment[] currentArray, AttachmentType AttachmentType)
        {
            var tmpList = new List<Attachment>(currentArray);
            tmpList.ForEach(a => a.Type = AttachmentType);
            return new ReadOnlyCollection<Attachment>(tmpList);
        }

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
            switch(Date.DayOfWeek)
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

        public static void ForEach<T>(this IEnumerable<T> current, Action<T> action)
        {
            foreach (var item in current) action(item);
        }
    }
}
