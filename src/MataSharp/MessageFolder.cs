using System.Collections.Generic;

namespace MataSharp
{
    /// <summary>
    /// Folder that contains MagisterMessage instances.
    /// </summary>
    sealed public partial class MagisterMessageFolder : IEnumerable<MagisterMessage>
    {
        public string Name { get; set; }
        public uint UnreadMessagesCount { get; set; }
        public int ID { get; set; }
        public MessageFolder FolderType { get { return (MessageFolder)this.ID; } }
        public int ParentID { get; set; }
        public string MessagesURI { get; set; }
        public object Ref { get; set; } // Even Schoolmaster doesn't know what this is, it's mysterious. Just keep it in case.

        internal Mata Mata { get; set; }
    }

    public enum MessageFolder : int
    { //Defines the folders where messages possibly are in, server handles it as ID's. We? We handle it as an enum :D
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
        public CompactMagisterMessage[] Items { get; set; }
        public int TotalCount { get; set; }
        public object Paging { get; set; }
    }

    internal partial struct CompactMagisterMessage
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
