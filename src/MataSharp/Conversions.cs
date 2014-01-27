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
    }
}
