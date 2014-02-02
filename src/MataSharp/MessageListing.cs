using System;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace MataSharp
{
    public class MessageList<Message> : IEnumerable<Message> where Message : MagisterMessage
    {
        private Mata Mata { get; set; }
        private MagisterMessageFolder Sender { get; set; }

        public MessageList(Mata Mata, MagisterMessageFolder Sender)
        {
            this.Mata = Mata;
            this.Sender = Sender;
        }

        public IEnumerator<Message> GetEnumerator()
        {
            return new Enumerator<Message>(this.Mata, this.Sender);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private Enumerator<Message> GetSpecificEnumerator()
        {
            return new Enumerator<Message>(this.Mata, this.Sender);
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
            var enumerator = this.GetSpecificEnumerator();

            for (int i = 0; i < count; i++)
                enumerator.GetAt(index + i).Delete();
        }

        /// <summary>
        /// Gets the given range of MagisterMessages.
        /// </summary>
        /// <param name="index">The zero-based index at which the range starts.</param>
        /// <param name="count">The number of elements in the range.</param>
        /// <returns>The given range of MagisterMessages as a List</returns>
        public List<Message> GetRange(int index, int count)
        {
            var tmpList = new List<Message>();
            var enumerator = this.GetSpecificEnumerator();

            for (int i = 0; i < count; i++)
                tmpList.Add(enumerator.GetAt(index + i));

            return tmpList;
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

        public void RemoveAll(int max, Predicate<Message> predicate)
        {
            var enumerator = this.GetSpecificEnumerator();
            for(int i = 0; i < max; i++)
            {
                var message = enumerator.GetAt(i);
                if (predicate(message)) message.Delete();
            }
        }

        /// <summary>
        /// Gets the messages that matches the given predicate.
        /// </summary>
        /// <param name="max">The max value to check for on the server.</param>
        /// <param name="predicate">The predicate the messages must match</param>
        /// <returns>A List containing the messages that matched the predicate.</returns>
        public List<Message> Where(int max, Func<Message,bool> predicate)
        {
            var enumerator = this.GetSpecificEnumerator();
            var tmpList = new List<Message>();
            for(int i = 0; i < max; i++)
            {
                var msg = enumerator.GetAt(i);
                if (predicate(msg)) tmpList.Add((Message)msg);
            }
            return tmpList;
        }

        /// <summary>
        /// Gets the first message that matches the given predicate. Throws exception when nothing is found.
        /// </summary>
        /// <param name="max">The max value to check for on the server.</param>
        /// <param name="predicate">The predicate the messages must match></param>
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
        /// Gets the first message that matches the given predicate. Gives back the default of the object if nothing is found.
        /// </summary>
        /// <param name="max">The max value to check for on the server.</param>
        /// <param name="predicate">The predicate the messages must match></param>
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

        private class Enumerator<T> : IEnumerator<T> where T : MagisterMessage
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
                    return (T)MessageClean.ToMagisterMessage();
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
                return (T)MessageClean.ToMagisterMessage();
            }

            public List<T> GetUnread(uint Ammount, uint Skip = 0)
            {
                string URL = "https://" + Mata.School.URL + "/api/personen/" + this.Mata.UserID + "/communicatie/berichten/mappen/" + Sender.ID + "/berichten?$skip=" + Skip + "&$top=" + Ammount;

                string CompactMessagesRAW = _Session.HttpClient.DownloadString(URL);
                var CompactMessages = JsonConvert.DeserializeObject<MagisterStyleMessageFolder>(CompactMessagesRAW);

                var list = new List<T>();
                foreach (var CompactMessage in CompactMessages.Items.Where(m => !m.IsGelezen))
                {
                    URL = "https://" + Mata.School.URL + "/api/personen/" + this.Mata.UserID + "/communicatie/berichten/mappen/" + Sender.ID + "/berichten/" + CompactMessage.Id;
                    string MessageRAW = _Session.HttpClient.DownloadString(URL);
                    var MessageClean = JsonConvert.DeserializeObject<MagisterStyleMessage>(MessageRAW);
                    list.Add((T)MessageClean.ToMagisterMessage());
                }
                return list;
            }

            public List<T> GetUnread()
            {
                var list = new List<T>();

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
                        list.Add((T)MessageClean.ToMagisterMessage());
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

            public void Dispose() { }
        }
    }
}