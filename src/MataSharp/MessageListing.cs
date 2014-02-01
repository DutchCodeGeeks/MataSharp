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