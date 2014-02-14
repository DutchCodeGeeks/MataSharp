using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace MataSharp
{
    public partial class MagisterMessageFolder : IEnumerable<MagisterMessage>
    {
        public string Name { get; set; }
        public uint UnreadMessagesCount { get; set; }
        public int ID { get; set; }
        public MessageFolder FolderType { get; set; }
        public int ParentID { get; set; }
        public string MessagesURI { get; set; }
        public object Ref { get; set; } // Even Schoolmaster doesn't know what this is, it's mysterious. Just keep it in case.

        internal Mata Mata { get; set; }
        internal MagisterSchool School { get { return this.Mata.School; } }
    }

    public enum MessageFolder : int
    { //Defines the folders where messages can be in, server handles it as ID's. We? We handle it as an enum :D
        Inbox = -101,
        SentMessages = -103,
        Bin = -102
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
