using System;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace MataSharp
{
    public class MessageList<Message> : IEnumerable<Message> where Message : MagisterMessage
    {
        private MagisterMessageFolder Sender { get; set; }

        public MessageList() { throw new Exception("MessageLists are only able to be internally created."); }

        internal MessageList(MagisterMessageFolder Sender)
        {
            this.Sender = Sender;
        }

        /// <summary>
        /// Gets the item on the given index.
        /// </summary>
        /// <param name="index">The zero-based index of the item to get.</param>
        /// <returns>The item on the given index.</returns>
        public Message this[int index] { get { return this.GetSpecificEnumerator().GetAt(index); } }

        public IEnumerator<Message> GetEnumerator()
        {
            return new Enumerator<Message>(this.Sender.Mata, this.Sender);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private Enumerator<Message> GetSpecificEnumerator()
        {
            return new Enumerator<Message>(this.Sender.Mata, this.Sender);
        }

        public Message ElementAt(int index)
        {
            return this[index];
        }

        public List<Message> Take(int count)
        {
            return this.GetRange(0, count);
        }

        /// <summary>
        /// Checks for new messages on the parent's mata server.
        /// </summary>
        /// <param name="Ammount">Ammount to ask for on the server.</param>
        /// <param name="Skip">Ammount of messages to skip | Default = 0</param>
        /// <returns>List of unread MagisterMessages.</returns>
        public List<Message> WhereUnread(uint Ammount, uint Skip = 0)
        {
            return this.GetSpecificEnumerator().GetUnread(Ammount, Skip);
        }

        /// <summary>
        /// Gets ALL the new messages on the parent's mata server.
        /// </summary>
        /// <returns>List of unread MagisterMessages.</returns>
        public List<Message> WhereUnread()
        {
            return this.GetSpecificEnumerator().GetUnread();
        }

        /// <summary>
        /// CAUTION: Permanently deletes the given message from the server.
        /// </summary>
        /// <param name="index">The zero-based index of the element to remove.</param>
        public void RemoveAt(int index)
        {
            this.GetSpecificEnumerator().GetAt(index).Delete();
        }

        /// <summary>
        /// CAUTION: Permanently deletes the given messages from the server.
        /// </summary>
        /// <param name="index">The zero-based starting index of the range of elements to remove.</param>
        /// <param name="count">The number of elements to remove.</param>
        public void RemoveRange(int index, int count)
        {
            this.GetSpecificEnumerator().GetRange(count, index).ForEach(m => m.Delete());
        }

        /// <summary>
        /// Gets the given range of MagisterMessages.
        /// </summary>
        /// <param name="index">The zero-based index at which the range starts.</param>
        /// <param name="count">The number of elements in the range.</param>
        /// <returns>The given range of MagisterMessages as a List</returns>
        public List<Message> GetRange(int index, int count)
        {
            return this.GetSpecificEnumerator().GetRange(count, index);
        }

        /// <summary>
        /// Get's the zero-based position of the given item on the server.
        /// </summary>
        /// <param name="item">The item to get its position from.</param>
        /// <returns>A zero-based index of the position of the given item.</returns>
        public int IndexOf(Message item)
        {
            var enumerator = this.GetSpecificEnumerator();
            Message currentItem = null;
            int pos = -1;
            do
            {
                enumerator.MoveNext();
                currentItem = enumerator.Current;
                pos++;
            } while (currentItem.ID != item.ID);
            return pos;
        }

        /// <summary>
        /// CAUTION: Permanently deletes the given messages from the server.
        /// </summary>
        /// <param name="max">The ammount of messages to check for on the server.</param>
        /// <param name="predicate">The predicate the messages must match to.</param>
        public void RemoveAll(int max, Predicate<Message> predicate)
        {
            this.GetSpecificEnumerator().GetRange(max, 0).Where(m => predicate(m)).ToList().ForEach(m => m.Delete());
        }

        /// <summary>
        /// Gets the messages that matches the given predicate.
        /// </summary>
        /// <param name="max">The max value to check for on the server.</param>
        /// <param name="predicate">The predicate the messages must match</param>
        /// <returns>A List containing the messages that matched the predicate.</returns>
        public List<Message> Where(int max, Func<Message,bool> predicate)
        {
            return this.GetSpecificEnumerator().GetRange(max, 0).Where(m => predicate(m)).ToList();
        }

        /// <summary>
        /// Gets the first message that matches the given predicate. Throws exception when nothing is found.
        /// </summary>
        /// <param name="max">The max value to check for on the server.</param>
        /// <param name="predicate">The predicate the message must match.</param>
        /// <returns>The first message on the server that matches the predicate.</returns>
        public Message First(int max, Func<Message,bool> predicate)
        {
            var enumerator = this.GetSpecificEnumerator();
            for (int i = 0; i < max; i++)
            {
                var msg = enumerator.GetAt(i);
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
        public Message Last(int max, Func<Message,bool> predicate)
        {
            return this.GetSpecificEnumerator().GetRange(max, 0).Last(m => predicate(m));
        }

        /// <summary>
        /// Gets the first message that matches the given predicate. Gives back the default of the object if nothing is found.
        /// </summary>
        /// <param name="max">The max value to check for on the server.</param>
        /// <param name="predicate">The predicate the message must match.</param>
        /// <returns>The first message on the server that matches the predicate.</returns>
        public Message FirstOrDefault(int max, Func<Message, bool> predicate)
        {
            var enumerator = this.GetSpecificEnumerator();
            for (int i = 0; i < max; i++)
            {
                var msg = enumerator.GetAt(i);
                if (predicate(msg)) return msg;
            } 
            return default(Message);
        }

        /// <summary>
        /// Gets the last message that matches the given predicate. Gives back the default of the object if nothing is found.
        /// </summary>
        /// <param name="max">The max value to check for on the server.</param>
        /// <param name="predicate">The predicate the message must match.</param>
        /// <returns>The last message on the server that matches the predicate.</returns>
        public Message LastOrDefault(int max, Func<Message, bool> predicate)
        {
            var enumerator = this.GetSpecificEnumerator();
            Message msg = null;
            for(int i = 0; i < max; i++)
            {
                var tmpMsg = enumerator.GetAt(i);
                if (predicate(tmpMsg)) msg = tmpMsg;
            }
            if (msg != null) return msg;
            else return default(Message);
        }

        /// <summary>
        /// Checks if there is a message on the server that matches the given predicate.
        /// </summary>
        /// <param name="max">The max value to check for on the server.</param>
        /// <param name="predicate">The predicate the message must match.</param>
        /// <returns>A boolean value that tells if there is a message matching the given predicate.</returns>
        public bool Any(int max, Func<Message, bool> predicate)
        {
            var enumerator = this.GetSpecificEnumerator();
            for(int i = 0; i < max; i++)
            {
                var msg = enumerator.GetAt(i);
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
        public Message Single(int max, Func<Message, bool> predicate)
        {
            var enumerator = this.GetSpecificEnumerator();
            Message msg = null;
            for(int i = 0; i <= max; i++)
            {
                var tmpMsg = enumerator.GetAt(i);
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
        public Message SingleOrDefault(int max, Func<Message, bool> predicate)
        {
            var enumerator = this.GetSpecificEnumerator();
            Message msg = null;
            for (int i = 0; i <= max; i++)
            {
                var tmpMsg = enumerator.GetAt(i);
                if (predicate(tmpMsg))
                {
                    if (msg == null) msg = tmpMsg;
                    else throw new Exception("More than 1 message matches the predicate.");
                }
            }
            if (msg != null) return msg;
            else return default(Message);
        }

        /// <summary>
        /// Checks if the given messages exist on the servers.
        /// </summary>
        /// <param name="item">The item to check if it exists.</param>
        /// <returns>A boolean value telling if the given message exists.</returns>
        public bool Contains(Message item)
        {
            return (!item.Deleted && this.Sender.FolderType == item.Folder);
        }

        private class Enumerator<T> : IEnumerator<T>, IDisposable where T : Message
        {
            private int Next = 0;
            private int Skip = -1;
            private Mata Mata { get; set; }
            private MagisterMessageFolder Sender { get; set; }
            private const int MaxMessages = 750;

            public Enumerator(Mata Mata, MagisterMessageFolder Sender)
            {
                this.Mata = Mata;
                this.Sender = Sender;
            }

            public T Current
            {
                get
                {
                    string URL = "https://" + Mata.School.URL + "/api/personen/" + Mata.UserID + "/communicatie/berichten/mappen/" + Sender.ID + "/berichten?$skip=" + Skip + "&$top=" + Next;

                    string CompactMessagesRAW = _Session.HttpClient.DownloadString(URL);
                    var CompactMessage = JsonConvert.DeserializeObject<MagisterStyleMessageFolder>(CompactMessagesRAW).Items[0];

                    URL = "https://" + Mata.School.URL + "/api/personen/" + Mata.UserID + "/communicatie/berichten/mappen/" + Sender.ID + "/berichten/" + CompactMessage.Id;
                    string MessageRAW = _Session.HttpClient.DownloadString(URL);
                    var MessageClean = JsonConvert.DeserializeObject<MagisterStyleMessage>(MessageRAW);
                    return (T)MessageClean.ToMagisterMessage(this.Skip);
                }
            }

            public T GetAt(int index)
            {
                string URL = "https://" + Mata.School.URL + "/api/personen/" + Mata.UserID + "/communicatie/berichten/mappen/" + Sender.ID + "/berichten?$skip=" + index + "&$top=" + index + 1;

                string CompactMessagesRAW = _Session.HttpClient.DownloadString(URL);
                var CompactMessage = JsonConvert.DeserializeObject<MagisterStyleMessageFolder>(CompactMessagesRAW).Items[0];

                URL = "https://" + Mata.School.URL + "/api/personen/" + Mata.UserID + "/communicatie/berichten/mappen/" + Sender.ID + "/berichten/" + CompactMessage.Id;
                string MessageRAW = _Session.HttpClient.DownloadString(URL);
                var MessageClean = JsonConvert.DeserializeObject<MagisterStyleMessage>(MessageRAW);
                return (T)MessageClean.ToMagisterMessage(index);
            }

            public List<T> GetRange(int Ammount, int Skip)
            {
                string URL = "https://" + Mata.School.URL + "/api/personen/" + this.Mata.UserID + "/communicatie/berichten/mappen/" + Sender.ID + "/berichten?$skip=" + Skip + "&$top=" + Ammount;

                string CompactMessagesRAW = _Session.HttpClient.DownloadString(URL);
                var CompactMessages = JsonConvert.DeserializeObject<MagisterStyleMessageFolder>(CompactMessagesRAW);

                var list = new List<T>(); int i = 0;
                foreach (var CompactMessage in CompactMessages.Items)
                {
                    URL = "https://" + Mata.School.URL + "/api/personen/" + this.Mata.UserID + "/communicatie/berichten/mappen/" + Sender.ID + "/berichten/" + CompactMessage.Id;
                    string MessageRAW = _Session.HttpClient.DownloadString(URL);
                    var MessageClean = JsonConvert.DeserializeObject<MagisterStyleMessage>(MessageRAW);
                    list.Add((T)MessageClean.ToMagisterMessage(i));
                    i++;
                }
                return list;
            }

            public List<T> GetUnread(uint Ammount, uint Skip = 0)
            {
                string URL = "https://" + Mata.School.URL + "/api/personen/" + this.Mata.UserID + "/communicatie/berichten/mappen/" + Sender.ID + "/berichten?$skip=" + Skip + "&$top=" + Ammount;

                string CompactMessagesRAW = _Session.HttpClient.DownloadString(URL);
                var CompactMessages = JsonConvert.DeserializeObject<MagisterStyleMessageFolder>(CompactMessagesRAW);

                var list = new List<T>(); int i = 0;
                foreach (var CompactMessage in CompactMessages.Items.Where(m => !m.IsGelezen))
                {
                    URL = "https://" + Mata.School.URL + "/api/personen/" + this.Mata.UserID + "/communicatie/berichten/mappen/" + Sender.ID + "/berichten/" + CompactMessage.Id;
                    string MessageRAW = _Session.HttpClient.DownloadString(URL);
                    var MessageClean = JsonConvert.DeserializeObject<MagisterStyleMessage>(MessageRAW);
                    list.Add((T)MessageClean.ToMagisterMessage(i));
                    i++;
                }
                return list;
            }

            public List<T> GetUnread()
            {
                var list = new List<T>(); int index = 0;

                for (uint i = 0; (list.Count != this.Sender.UnreadMessagesCount - 1); i++)
                {
                    string URL = "https://" + Mata.School.URL + "/api/personen/" + this.Mata.UserID + "/communicatie/berichten/mappen/" + Sender.ID + "/berichten?$skip=" + (i * 25) + "&$top=" + ((i * 25) + 25);

                    string CompactMessagesRAW = _Session.HttpClient.DownloadString(URL);
                    var CompactMessages = JsonConvert.DeserializeObject<MagisterStyleMessageFolder>(CompactMessagesRAW);
                    foreach (var CompactMessage in CompactMessages.Items.Where(m => !m.IsGelezen))
                    {
                        URL = "https://" + Mata.School.URL + "/api/personen/" + this.Mata.UserID + "/communicatie/berichten/mappen/" + Sender.ID + "/berichten/" + CompactMessage.Id;
                        string MessageRAW = _Session.HttpClient.DownloadString(URL);
                        var MessageClean = JsonConvert.DeserializeObject<MagisterStyleMessage>(MessageRAW);
                        list.Add((T)MessageClean.ToMagisterMessage(index));
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
                return (Next < MaxMessages);
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
                this.Mata = null;
                this.Sender = null;
                GC.Collect();
            }
        }
    }
}