using System;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace MataSharp
{
    /// <summary>
    /// Folder that contains MagisterMessage instances.
    /// </summary>
    sealed public partial class MagisterMessageFolder : IReadOnlyList<MagisterMessage>
    {
        /// <summary>
        /// Gets the item on the given index.
        /// </summary>
        /// <param name="index">The zero-based index of the item to get.</param>
        /// <returns>The item on the given index.</returns>
        public MagisterMessage this[int index] { get { return this.ElementAt(index); } }

        public IEnumerator<MagisterMessage> GetEnumerator()
        {
            return new Enumerator(this);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private Enumerator GetSpecificEnumerator()
        {
            return new Enumerator(this);
        }

        public MagisterMessage ElementAt(int index, bool download = true)
        {
            return this.GetSpecificEnumerator().GetAt(download, index);
        }

        public IEnumerable<MagisterMessage> Take(int count)
        {
            return this.GetRange(0, count, true);
        }

        public IEnumerable<MagisterMessage> Take(int count, bool download)
        {
            return this.GetRange(0, count, download);
        }

        /// <summary>
        /// Checks for new messages on the parent's mata server.
        /// </summary>
        /// <param name="Ammount">Ammount to ask for on the server.</param>
        /// <param name="Skip">Ammount of messages to skip | Default = 0</param>
        /// <returns>List of unread MagisterMessages.</returns>
        public IList<MagisterMessage> WhereUnread(uint Ammount, bool download = true, uint Skip = 0)
        {
            return this.GetSpecificEnumerator().GetUnread(download, Ammount, Skip);
        }

        /// <summary>
        /// Gets ALL the new messages on the parent's mata server.
        /// </summary>
        /// <returns>List of unread MagisterMessages.</returns>
        public IList<MagisterMessage> WhereUnread(bool download = true)
        {
            return this.GetSpecificEnumerator().GetUnread(download);
        }

        /// <summary>
        /// CAUTION: Permanently deletes the given message from the server.
        /// </summary>
        /// <param name="index">The zero-based index of the element to remove.</param>
        public void RemoveAt(int index)
        {
            this.GetSpecificEnumerator().GetAt(false, index).Delete();
        }

        /// <summary>
        /// CAUTION: Permanently deletes the given messages from the server.
        /// </summary>
        /// <param name="index">The zero-based starting index of the range of elements to remove.</param>
        /// <param name="count">The number of elements to remove.</param>
        public void RemoveRange(int index, int count)
        {
            this.GetSpecificEnumerator().GetRange(false, count, index).ForEach(m => m.Delete());
        }

        /// <summary>
        /// Gets the given range of MagisterMessages.
        /// </summary>
        /// <param name="index">The zero-based index at which the range starts.</param>
        /// <param name="count">The number of elements in the range.</param>
        /// <returns>The given range of MagisterMessages as a List</returns>
        public IEnumerable<MagisterMessage> GetRange(int index, int count, bool download = true)
        {
            return this.GetSpecificEnumerator().GetRange(download, count, index);
        }

        /// <summary>
        /// Gets the zero-based position of the given item on the server.
        /// </summary>
        /// <param name="item">The item to get its position from.</param>
        /// <returns>A zero-based index of the position of the given item.</returns>
        public int IndexOf(MagisterMessage item)
        {
            return item.Index;
        }

        /// <summary>
        /// CAUTION: Permanently deletes the given messages from the server.
        /// </summary>
        /// <param name="max">The ammount of messages to check for on the server.</param>
        /// <param name="predicate">The predicate the messages must match to.</param>
        public void RemoveAll(int max, Predicate<MagisterMessage> predicate)
        {
            this.Take(max, true).Where(m => predicate(m)).ForEach(m => m.Delete());
        }

        /// <summary>
        /// Gets the messages that matches the given predicate.
        /// </summary>
        /// <param name="max">The max value to check for on the server.</param>
        /// <param name="predicate">The predicate the messages must match</param>
        /// <returns>A List containing the messages that matched the predicate.</returns>
        public IEnumerable<MagisterMessage> Where(int max, Func<MagisterMessage, bool> predicate, bool download = true)
        {
            return this.Take(max, download).Where(m => predicate(m)).ToList();
        }

        /// <summary>
        /// Gets the first message that matches the given predicate. Throws exception when nothing is found.
        /// </summary>
        /// <param name="max">The max value to check for on the server.</param>
        /// <param name="predicate">The predicate the message must match.</param>
        /// <returns>The first message on the server that matches the predicate.</returns>
        public MagisterMessage First(int max, Func<MagisterMessage, bool> predicate, bool download = true)
        {
            var enumerator = this.GetSpecificEnumerator();
            for (int i = 0; i < max; i++)
            {
                var msg = enumerator.GetAt(download, i);
                if (predicate(msg)) return msg;
            }
            throw new Exception("No messages found.");
        }

        /// <summary>
        /// Gets the last message that matches the given predicate. Throws exception when nothing is found.
        /// </summary>
        /// <param name="max">The max value to check for on the server.</param>
        /// <param name="predicate">The predicate the message must match.</param>
        /// <returns>The last message on the server that matches the predicate.</returns>
        public MagisterMessage Last(int max, Func<MagisterMessage, bool> predicate, bool download = true)
        {
            return this.GetSpecificEnumerator().GetRange(download, max, 0).Last(m => predicate(m));
        }

        /// <summary>
        /// Gets the first message that matches the given predicate. Gives back the default of the object if nothing is found.
        /// </summary>
        /// <param name="max">The max value to check for on the server.</param>
        /// <param name="predicate">The predicate the message must match.</param>
        /// <returns>The first message on the server that matches the predicate.</returns>
        public MagisterMessage FirstOrDefault(int max, Func<MagisterMessage, bool> predicate, bool download = true)
        {
            var enumerator = this.GetSpecificEnumerator();
            for (int i = 0; i < max; i++)
            {
                var msg = enumerator.GetAt(download, i);
                if (predicate(msg)) return msg;
            }
            return default(MagisterMessage);
        }

        /// <summary>
        /// Gets the last message that matches the given predicate. Gives back the default of the object if nothing is found.
        /// </summary>
        /// <param name="max">The max value to check for on the server.</param>
        /// <param name="predicate">The predicate the message must match.</param>
        /// <returns>The last message on the server that matches the predicate.</returns>
        public MagisterMessage LastOrDefault(int max, Func<MagisterMessage, bool> predicate, bool download = true)
        {
            var enumerator = this.GetSpecificEnumerator();
            MagisterMessage msg = null;
            for(int i = 0; i < max; i++)
            {
                var tmpMsg = enumerator.GetAt(download, i);
                if (predicate(tmpMsg)) msg = tmpMsg;
            }
            if (msg != null) return msg;
            else return default(MagisterMessage);
        }

        /// <summary>
        /// Checks if there is a message on the server that matches the given predicate.
        /// </summary>
        /// <param name="max">The max value to check for on the server.</param>
        /// <param name="predicate">The predicate the message must match.</param>
        /// <returns>A boolean value that tells if there is a message matching the given predicate.</returns>
        public bool Any(int max, Func<MagisterMessage, bool> predicate, bool download = true)
        {
            var enumerator = this.GetSpecificEnumerator();
            for(int i = 0; i < max; i++)
            {
                var msg = enumerator.GetAt(download, i);
                if (predicate(msg)) return true;
            }
            return false;
        }

        /// <summary>
        /// Returns the single message on the server that matches the given predicate. Throws expception when more matching messages or none are found.
        /// </summary>
        /// <param name="predicate">The predicate that the message must match to.</param>
        /// <param name="max">The max ammount of messages to get from the server.</param>
        /// <returns>A single MagisterMessage matching the given predicate.</returns>
        public MagisterMessage Single(int max, Func<MagisterMessage, bool> predicate, bool download = true)
        {
            var enumerator = this.GetSpecificEnumerator();
            MagisterMessage msg = null;
            for(int i = 0; i <= max; i++)
            {
                var tmpMsg = enumerator.GetAt(download, i);
                if (predicate(tmpMsg))
                {
                    if (msg == null) msg = tmpMsg;
                    else throw new Exception("More than 1 message matches the predicate.");
                }
            }
            if (msg != null) return msg;
            else throw new Exception("No messages found that matches the predicate.");
        }

        /// <summary>
        /// Returns the single message on the server that matches the given predicate. Throws expception when more matching messages are found.
        /// When no matching messages are found, returns the default value.
        /// </summary>
        /// <param name="max">The max ammount of messages to get from the server.</param>
        /// <param name="predicate">The predicate that the message must match to.</param>
        /// <returns>A single MagisterMessage matching the given predicate.</returns>
        public MagisterMessage SingleOrDefault(int max, Func<MagisterMessage, bool> predicate, bool download = true)
        {
            var enumerator = this.GetSpecificEnumerator();
            MagisterMessage msg = null;
            for (int i = 0; i <= max; i++)
            {
                var tmpMsg = enumerator.GetAt(download, i);
                if (predicate(tmpMsg))
                {
                    if (msg == null) msg = tmpMsg;
                    else throw new Exception("More than 1 message matches the predicate.");
                }
            }
            if (msg != null) return msg;
            else return default(MagisterMessage);
        }

        /// <summary>
        /// Checks if the given messages exist on the servers.
        /// </summary>
        /// <param name="item">The item to check if it exists.</param>
        /// <returns>A boolean value telling if the given message exists.</returns>
        public bool Contains(MagisterMessage item)
        {
            return (!item.Deleted && this.FolderType == item.Folder);
        }

        /// <summary>
        /// Count is unknown.
        /// </summary>
        [Obsolete("Count is unknown.")]
        public int Count
        {
            get { return -1; }
        }

        private class Enumerator : IEnumerator<MagisterMessage>, IDisposable
        {
            private int Next = 0;
            private int Skip = -1;
            private Mata Mata { get { return this.Sender.Mata; } }
            private MagisterMessageFolder Sender { get; set; }
            private const ushort MAX_MESSAGES = 750;

            public Enumerator(MagisterMessageFolder Sender)
            {
                this.Sender = Sender;
            }

            public MagisterMessage Current
            {
                get
                {
                    string URL = "https://" + Mata.School.URL + "/api/personen/" + Mata.UserID + "/communicatie/berichten/mappen/" + Sender.ID + "/berichten?$skip=" + Skip + "&$top=" + Next;

                    string CompactMessageRAW = this.Mata.WebClient.DownloadString(URL);
                    var CompactMessage = JsonConvert.DeserializeObject<MagisterStyleMessageFolder>(CompactMessageRAW).Items[0];

                    URL = "https://" + Mata.School.URL + "/api/personen/" + Mata.UserID + "/communicatie/berichten/mappen/" + Sender.ID + "/berichten/" + CompactMessage.Id;
                    string MessageRAW = this.Mata.WebClient.DownloadString(URL);
                    var MessageClean = JsonConvert.DeserializeObject<MagisterStyleMessage>(MessageRAW);
                    return MessageClean.ToMagisterMessage(this.Mata, true, this.Skip);
                }
            }

            public MagisterMessage GetAt(bool download, int index)
            {
                string URL = "https://" + Mata.School.URL + "/api/personen/" + Mata.UserID + "/communicatie/berichten/mappen/" + Sender.ID + "/berichten?$skip=" + index + "&$top=" + index + 1;

                string CompactMessagesRAW = this.Mata.WebClient.DownloadString(URL);
                var CompactMessage = JsonConvert.DeserializeObject<MagisterStyleMessageFolder>(CompactMessagesRAW).Items[0];

                URL = "https://" + Mata.School.URL + "/api/personen/" + Mata.UserID + "/communicatie/berichten/mappen/" + Sender.ID + "/berichten/" + CompactMessage.Id;
                string MessageRAW = this.Mata.WebClient.DownloadString(URL);
                var MessageClean = JsonConvert.DeserializeObject<MagisterStyleMessage>(MessageRAW);
                return MessageClean.ToMagisterMessage(this.Mata, download, index);
            }

            public List<MagisterMessage> GetRange(bool download, int Ammount, int Skip)
            {
                string URL = "https://" + Mata.School.URL + "/api/personen/" + this.Mata.UserID + "/communicatie/berichten/mappen/" + Sender.ID + "/berichten?$skip=" + Skip + "&$top=" + Ammount;

                string CompactMessagesRAW = this.Mata.WebClient.DownloadString(URL);
                var CompactMessages = JsonConvert.DeserializeObject<MagisterStyleMessageFolder>(CompactMessagesRAW);

                var list = new List<MagisterMessage>(); int i = 0;
                foreach (var CompactMessage in CompactMessages.Items)
                {
                    URL = "https://" + Mata.School.URL + "/api/personen/" + this.Mata.UserID + "/communicatie/berichten/mappen/" + Sender.ID + "/berichten/" + CompactMessage.Id;
                    string MessageRAW = this.Mata.WebClient.DownloadString(URL);
                    var MessageClean = JsonConvert.DeserializeObject<MagisterStyleMessage>(MessageRAW);
                    list.Add(MessageClean.ToMagisterMessage(this.Mata, download, i));
                    i++;
                }
                return list;
            }

            public List<MagisterMessage> GetUnread(bool download, uint Ammount, uint Skip = 0)
            {
                string URL = "https://" + Mata.School.URL + "/api/personen/" + this.Mata.UserID + "/communicatie/berichten/mappen/" + Sender.ID + "/berichten?$skip=" + Skip + "&$top=" + Ammount;

                string CompactMessagesRAW = this.Mata.WebClient.DownloadString(URL);
                var CompactMessages = JsonConvert.DeserializeObject<MagisterStyleMessageFolder>(CompactMessagesRAW);

                var list = new List<MagisterMessage>(); int i = 0;
                foreach (var CompactMessage in CompactMessages.Items.Where(m => !m.IsGelezen))
                {
                    URL = "https://" + Mata.School.URL + "/api/personen/" + this.Mata.UserID + "/communicatie/berichten/mappen/" + Sender.ID + "/berichten/" + CompactMessage.Id;
                    string MessageRAW = this.Mata.WebClient.DownloadString(URL);
                    var MessageClean = JsonConvert.DeserializeObject<MagisterStyleMessage>(MessageRAW);
                    list.Add(MessageClean.ToMagisterMessage(this.Mata, download, i));
                    i++;
                }
                return list;
            }

            public List<MagisterMessage> GetUnread(bool download)
            {
                var list = new List<MagisterMessage>(); int index = 0;

                for (uint i = 0; (list.Count != this.Sender.UnreadMessagesCount - 1); i++)
                {
                    string URL = "https://" + Mata.School.URL + "/api/personen/" + this.Mata.UserID + "/communicatie/berichten/mappen/" + Sender.ID + "/berichten?$skip=" + (i * 25) + "&$top=" + ((i * 25) + 25);

                    string CompactMessagesRAW = this.Mata.WebClient.DownloadString(URL);
                    var CompactMessages = JsonConvert.DeserializeObject<MagisterStyleMessageFolder>(CompactMessagesRAW);
                    foreach (var CompactMessage in CompactMessages.Items.Where(m => !m.IsGelezen))
                    {
                        URL = "https://" + Mata.School.URL + "/api/personen/" + this.Mata.UserID + "/communicatie/berichten/mappen/" + Sender.ID + "/berichten/" + CompactMessage.Id;
                        string MessageRAW = this.Mata.WebClient.DownloadString(URL);
                        var MessageClean = JsonConvert.DeserializeObject<MagisterStyleMessage>(MessageRAW);
                        list.Add(MessageClean.ToMagisterMessage(this.Mata, download, index));
                        i++;
                    }
                }
                return list;
            }

            object System.Collections.IEnumerator.Current
            {
                get { return Current; }
            }

            public bool MoveNext()
            {
                Next++;
                Skip++;
                return (Next < MAX_MESSAGES);
            }

            public void Reset()
            {
                Next = 0;
                Skip = -1;
            }

            ~Enumerator() { this.Dispose(); }
            public void Dispose()
            {
                this.Reset();
                this.Sender = null;
                GC.SuppressFinalize(this);
                GC.Collect();
            }
        }
    }
}