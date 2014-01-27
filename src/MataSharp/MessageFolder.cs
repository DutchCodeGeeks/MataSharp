using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace MataSharp
{
    public partial class MagisterMessageFolder
    {
        public string Name { get; set; }
        public uint UnreadMessagesCount { get; set; }
        public int ID { get; set; }
        public MessageFolderType FolderType { get; set; }
        public int ParentID { get; set; }
        public string MessagesURI { get; set; }
        public object Ref { get; set; } // Even Schoolmaster doesn't know what this is, it's mysterious. Just keep it in case.

        internal Mata Mata { get; set; }
        internal MagisterSchool School { get { return this.Mata.School; } }

        /// <summary>
        /// Checks for messages on the parent's mata server.
        /// </summary>
        /// <param name="Ammount">Ammount of messages to return.</param>
        /// <param name="Skip">Ammount of messages to skip | Default = 0</param>
        /// <returns>List of MagisterMessages.</returns>
        public List<MagisterMessage> GetMessages(uint Ammount, uint Skip = 0)
        {
            if (Ammount == 0) return new List<MagisterMessage>();

            string URL = "https://" + School.URL + "/api/personen/" + this.Mata.UserID + "/communicatie/berichten/mappen/" + this.ID + "/berichten?$skip=" + Skip + "&$top=" + Ammount;

            string CompactMessagesRAW = _Session.HttpClient.DownloadString(URL);
            var CompactMessages = JsonConvert.DeserializeObject<MagisterStyleMessageFolder>(CompactMessagesRAW);

            List<MagisterMessage> list = new List<MagisterMessage>();
            foreach (var CompactMessage in CompactMessages.Items)
            {
                URL = "https://" + School.URL + "/api/personen/" + this.Mata.UserID + "/communicatie/berichten/mappen/" + this.ID + "/berichten/" + CompactMessage.Id;
                string MessageRAW = _Session.HttpClient.DownloadString(URL);
                var MessageClean = JsonConvert.DeserializeObject<MagisterStyleMessage>(MessageRAW);
                list.Add(MessageClean.ToMagisterMessage());
            }
            return list;
        }

        /// <summary>
        /// Checks for new messages on the parent's mata server.
        /// </summary>
        /// <param name="Ammount">Ammount to ask for on the server.</param>
        /// <param name="Skip">Ammount of messages to skip | Default = 0</param>
        /// <returns>List of unread MagisterMessages.</returns>
        public List<MagisterMessage> GetUnreadMessages(uint Ammount, uint Skip = 0)
        {
            if (Ammount == 0) return new List<MagisterMessage>();

            string URL = "https://" + School.URL + "/api/personen/" + this.Mata.UserID + "/communicatie/berichten/mappen/" + this.ID + "/berichten?$skip=" + Skip + "&$top=" + Ammount;

            string CompactMessagesRAW = _Session.HttpClient.DownloadString(URL);
            var CompactMessages = JsonConvert.DeserializeObject<MagisterStyleMessageFolder>(CompactMessagesRAW);

            List<MagisterMessage> list = new List<MagisterMessage>();
            foreach (var CompactMessage in CompactMessages.Items.Where(m => m.IsGelezen == false))
            {
                URL = "https://" + School.URL + "/api/personen/" + this.Mata.UserID + "/communicatie/berichten/mappen/" + this.ID + "/berichten/" + CompactMessage.Id;
                string MessageRAW = _Session.HttpClient.DownloadString(URL);
                var MessageClean = JsonConvert.DeserializeObject<MagisterStyleMessage>(MessageRAW);
                list.Add(MessageClean.ToMagisterMessage());
            }
            return list;
        }
        
        /// <summary>
        /// Gets ALL the new messages on the parent's mata server.
        /// </summary>
        /// <returns>List of unread MagisterMessages.</returns>
        public List<MagisterMessage> GetUnreadMessages()
        {
            List<MagisterMessage> list = new List<MagisterMessage>();

            for (uint i = 0; (list.Count == this.UnreadMessagesCount - 1); i++)
            {
                string URL = "https://" + School.URL + "/api/personen/" + this.Mata.UserID + "/communicatie/berichten/mappen/" + this.ID + "/berichten?$skip=" + (i * 25) + "&$top=" + ((i * 25) + 25);

                string CompactMessagesRAW = _Session.HttpClient.DownloadString(URL);
                var CompactMessages = JsonConvert.DeserializeObject<MagisterStyleMessageFolder>(CompactMessagesRAW);

                foreach (var CompactMessage in CompactMessages.Items.Where(m => m.IsGelezen == false))
                {
                    URL = "https://" + School.URL + "/api/personen/" + this.Mata.UserID + "/communicatie/berichten/mappen/" + this.ID + "/berichten/" + CompactMessage.Id;
                    string MessageRAW = _Session.HttpClient.DownloadString(URL);
                    var MessageClean = JsonConvert.DeserializeObject<MagisterStyleMessage>(MessageRAW);
                    list.Add(MessageClean.ToMagisterMessage());
                }
            }
            return list;
        }
    }

    public enum MessageFolderType
    { //Defines the folders where messages can be in, server handles it as ID's. We? We handle it as an enum :D
        Inbox,
        SentMessages,
        Bin
    }

    internal partial struct MagisterStyleMessageFolderListItem
    {
        public string Naam { get; set; }
        public uint OngelezenBerichten { get; set; }
        public int Id { get; set; }
        public int ParentId { get; set; }
        public string BerichtenUri { get; set; }
        public object Ref { get; set; } // Even Schoolmaster doesn't know what this is, it's mysterious. Just keep it in case.
    }

    internal partial struct MagisterStyleMessageFolder
    {
        public MessageFolderItem[] Items { get; set; }
        public int TotalCount { get; set; }
        public object Paging { get; set; }
    }

    internal partial struct MessageFolderItem
    {
        public int Id { get; set; }
        public object Ref {get; set; }
        public string Onderwerp { get; set; }
        public MagisterStylePerson Afzender { get; set; }
        public string IngekortBericht { get; set; }
        public MagisterStylePerson[] Ontvangers { get; set; }
        public string Ontvangen { get; set; }
        public bool IsGelezen { get; set; }
        public int Status { get; set; }
        public bool HeeftPrioriteit { get; set; }
        public bool HeeftBijlagen { get; set; }
    }
}
