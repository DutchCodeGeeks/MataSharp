using System;
using System.Collections.Generic;
using System.Globalization;

namespace MataSharp
{
    internal static class Conversions
    {
        /// <summary>
        /// Converts the current string to a DateTime.
        /// </summary>
        /// <returns>The string parsed as DateTime</returns>
        public static DateTime ToDateTime(this String original)
        {
            return (!string.IsNullOrWhiteSpace(original)) ? DateTime.Parse(original, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal) : new DateTime();
        }

        /// <summary>
        /// Convert the current Attachment[] to a List
        /// </summary>
        /// <param name="AttachmentType">AttachmentType to give every attachment in the array.</param>
        /// <returns>The array as list</returns>
        public static List<Attachment> ToList(this Attachment[] currentArray, AttachmentType AttachmentType)
        {
            var tmpList = new List<Attachment>(currentArray);
            tmpList.ForEach(a => a.Type = AttachmentType);
            return tmpList;
        }

        /// <summary>
        /// Converts the current DateTime instance to a string.
        /// </summary>
        /// <returns>The current DateTime instance as string.</returns>
        public static string ToUTCString(this DateTime original) 
        { 
            return original.ToString("yyyy-MM-ddTHH:mm:ss.0000000Z");
        }

        /// <summary>
        /// Gets the DayOfWeek from the current DateTime instance in Dutch.
        /// </summary>
        /// <returns>Dutch day of week that represents the day of the current DateTime instance.</returns>
        public static string DayOfWeekDutch(this DateTime Date)
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
    }
}
