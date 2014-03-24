using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using System.Collections.ObjectModel;

namespace MataSharp
{
    sealed public partial class MagisterMessage : IComparable<MagisterMessage>, ICloneable
    {
        #region Contents
        public int ID { get; set; }
        public object Ref { get; set; } // Even Schoolmaster doesn't know what this is, it's mysterious. Just keep it in case.
        public string Subject { get; set; }
        public MagisterPerson Sender { get; internal set; }
        public string Body { get; set; }
        public PersonList Recipients { get; set; }
        public PersonList CC { get; set; }
        public DateTime SentDate { get; set; }
        internal bool _IsRead;

        public bool IsRead
        {
            get { return this._IsRead; }
            set
            {
                if (this._IsRead == value) return; //You don't have to do what's already done.

                this._IsRead = value;

                this.Mata.WebClient.Put(this.URL, JsonConvert.SerializeObject(this.ToMagisterStyle()));
            }
        }

        public int State { get; set; }
        public bool IsFlagged { get; set; }
        public int? IDOriginal { get; set; }
        public int? IDOrginalReceiver { get; set; }
        public ReadOnlyCollection<Attachment> Attachments { get; internal set; }
        internal int _Folder { get; set; }

        public MessageFolder Folder 
        { 
            get { return (MessageFolder)this._Folder; }
            set { this.Move(value); }
        }

        public bool Deleted { get; internal set; }
        public int IDKey { get; set; }
        public int SenderGroupID { get; set; }

        internal bool _CanSend { get; set; }
        public bool CanSend
        {
            get { return (this._CanSend == true && this.Sender != null && this.Recipients != null && !string.IsNullOrEmpty(this.Body) && !string.IsNullOrEmpty(this.Subject)); }
        }

        public int Index { get; internal set; }

        private Mata Mata { get; set; }
        #endregion

        private string URL { get { return "https://" + this.Mata.School.URL + "/api/personen/" + this.Mata.UserID + "/communicatie/berichten/mappen/" + this._Folder + "/berichten/" + this.ID; } }

        /// <summary>
        /// Returns new MagisterMessage
        /// </summary>
        /// <param name="Mata">The mata instance to use.</param>
        public MagisterMessage(Mata Mata)
        {
            if (Mata == null)
                throw new NullReferenceException("Mata instance is null!");

            this.Mata = Mata;
            this._CanSend = true;

            this.ID = 0;
            this._IsRead = false;
            this.State = 0;
            this.IDOriginal = 0;
            this.IDOrginalReceiver = 0;
            this._Folder = 0;
            this.Deleted = false;
            this.IDKey = 0;
            this.SenderGroupID = this.Mata.Person._GroupID;
            this.Sender = this.Mata.Person;
            this.Recipients = new PersonList(this.Mata);
            this.CC = new PersonList(this.Mata);
        }

        /// <summary>
        /// Creates new MagisterMessage that forwards the current message.
        /// </summary>
        /// <returns>A new MagisterMessage instance ready to be send.</returns>
        public MagisterMessage CreateForwardMessage()
        {
            var tmpSubject = (this.Subject[0] != 'F' || this.Subject[1] != 'W' || this.Subject[2] != ':' || this.Subject[3] != ' ') ? "FW: " + this.Subject : this.Subject;

            return new MagisterMessage(this.Mata)
            {
                Sender = this.Sender,//Magister's logic
                _Folder = this._Folder,
                Attachments = new ReadOnlyCollection<Attachment>(new List<Attachment>()),
                CC = null,
                IsFlagged = this.IsFlagged,
                ID = this.ID,
                SenderGroupID = 4,
                IDKey = this.IDKey,
                IDOrginalReceiver = null,
                IDOriginal = null,
                Body = "<b>Van:</b> " + this.Sender.Description + "<br><b>Verzonden:</b> " + this.SentDate.ToString(true) + "<br><b>Aan:</b> " + String.Join(", ", this.Recipients.Select(x => x.Name)) + "<br><b>Onderwerp:</b> " + this.Subject + "<br><br>\"" + this.Body + "\"<br><br>",
                Deleted = false,
                _IsRead = true,
                Subject = tmpSubject,
                Recipients = new PersonList(this.Mata),
                Ref = null,
                State = 0,
                SentDate = DateTime.UtcNow,
                _CanSend = true
            };
        }

        /// <summary>
        /// Creates new MagisterMessage that forwards the current message.
        /// </summary>
        /// <param name="ContentAdd">The content to add infront of the original message.</param>
        /// <returns>A new MagisterMessage instance ready to be send.</returns>
        public MagisterMessage CreateForwardMessage(string ContentAdd)
        {
            var tmpSubject = (this.Subject[0] != 'F' || this.Subject[1] != 'W' || this.Subject[2] != ':' || this.Subject[3] != ' ') ? "FW: " + this.Subject : this.Subject;

            return new MagisterMessage(this.Mata)
            {
                Sender = this.Sender,//Magister's logic
                _Folder = this._Folder,
                Attachments = new ReadOnlyCollection<Attachment>(new List<Attachment>()),
                IsFlagged = this.IsFlagged,
                ID = this.ID,
                SenderGroupID = 4,
                IDKey = this.IDKey,
                IDOrginalReceiver = null,
                IDOriginal = null,
                Body = ContentAdd + "<br><br>---------------<br><b>Van:</b> " + this.Sender.Description + "<br><b>Verzonden:</b> " + this.SentDate.ToString(true) + "<br><b>Aan:</b> " + String.Join(", ", this.Recipients.Select(x => x.Name)) + "<br><b>Onderwerp:</b> " + this.Subject + "<br><br>\"" + this.Body + "\"<br><br>",
                Deleted = false,
                _IsRead = true,
                Subject = tmpSubject,
                Recipients = new PersonList(this.Mata),
                Ref = null,
                State = 0,
                SentDate = DateTime.UtcNow,
                _CanSend = true
            };
        }

        /// <summary>
        /// Creates Message that replies to the sender and all the receiptents of the current message.
        /// </summary>
        /// <param name="ContentAdd">The content to add infront of the original message.</param>
        /// <returns>A new MagisterMessage instance ready to be send.</returns>
        public MagisterMessage CreateReplyToAllMessage(string ContentAdd)
        {
            var tmpSubject = (this.Subject[0] != 'R' || this.Subject[1] != 'E' || this.Subject[2] != ':' || this.Subject[3] != ' ') ? "RE: " + this.Subject : this.Subject;

            var tmpCC = this.Recipients.Where(p => p.ID != this.Mata.Person.ID).ToList(); //Should get the current receivers and pull itself out. :)
            if (this.CC != null) tmpCC.AddRange(this.CC.Where(p => p.ID != this.Mata.Person.ID));
            tmpCC.Sort();

            return new MagisterMessage(this.Mata)
            {
                Sender = this.Sender,
                _Folder = this._Folder,
                Attachments = new ReadOnlyCollection<Attachment>(new List<Attachment>()),
                CC = new PersonList(tmpCC, this.Mata),
                IsFlagged = this.IsFlagged,
                ID = this.ID,
                SenderGroupID = 4,
                IDKey = this.IDKey,
                IDOrginalReceiver = null,
                IDOriginal = null,
                Body = ContentAdd + "<br><br>---------------<br><b>Van:</b> " + this.Sender.Description + "<br><b>Verzonden:</b> " + this.SentDate.ToString(true) + "<br><b>Aan:</b> " + String.Join(", ", this.Recipients.Select(x => x.Name)) + "<br><b>Onderwerp:</b> " + this.Subject + "<br><br>\"" + this.Body + "\"<br><br>",
                Deleted = false,
                _IsRead = true,
                Subject = tmpSubject,
                Recipients = new PersonList(this.Mata) { this.Sender },
                Ref = null,
                State = 0,
                SentDate = DateTime.UtcNow,
                _CanSend = true
            };
        }

        /// <summary>
        /// Creates Message that replies to the sender of the current message.
        /// </summary>
        /// <param name="ContentAdd">The content to add infront of the original message.</param>
        /// <returns>A new MagisterMessage instance ready to be send.</returns>
        public MagisterMessage CreateReplyMessage(string ContentAdd)
        {
            var tmpSubject = (this.Subject[0] != 'R' || this.Subject[1] != 'E' || this.Subject[2] != ':' || this.Subject[3] != ' ') ? "RE: " + this.Subject : this.Subject;

            return new MagisterMessage(this.Mata)
            {
                Sender = this.Sender,//Magister's logic
                _Folder = this._Folder,
                Attachments = new ReadOnlyCollection<Attachment>(new List<Attachment>()),
                CC = null,
                IsFlagged = this.IsFlagged,
                ID = this.ID,
                SenderGroupID = 4,
                IDKey = this.IDKey,
                IDOrginalReceiver = null,
                IDOriginal = null,
                Body = ContentAdd + "<br><br>---------------<br><b>Van:</b> " + this.Sender.Description + "<br><b>Verzonden:</b> " + this.SentDate.ToString(true) + "<br><b>Aan:</b> " + String.Join(", ", this.Recipients.Select(x => x.Name)) + "<br><b>Onderwerp:</b> " + this.Subject + "<br><br>\"" + this.Body + "\"<br><br>",
                Deleted = false,
                _IsRead = true,
                Subject = tmpSubject,
                Recipients = new PersonList(this.Mata) { this.Sender },
                Ref = null,
                State = 0,
                SentDate = DateTime.UtcNow,
                _CanSend = true
            };
        }

        /// <summary>
        /// Moves the current Message to the given folder.
        /// </summary>
        /// <param name="Folder">The folder to move the current message to.</param>
        public void Move(MessageFolder Folder) { this.Move((int)Folder); }

        /// <summary>
        /// Moves the current Message to the given folder.
        /// </summary>
        /// <param name="Folder">The folder to move the current message to.</param>
        public void Move(MagisterMessageFolder Folder) { this.Move(Folder.ID); }

        /// <summary>
        /// Moves the current Message to the given folder.
        /// </summary>
        /// <param name="FolderID">The folder to move the current message to.</param>
        public void Move(int FolderID)
        {
            if (this._Folder == FolderID) return;

            var thisCopied = (MagisterMessage)this.MemberwiseClone();

            this._Folder = FolderID;

            this.Mata.WebClient.Put(this.URL, JsonConvert.SerializeObject(this.ToMagisterStyle()));
            thisCopied.Delete();
        }

        /// <summary>
        /// CAUTION: Permanently deletes the current message on the server.
        /// </summary>
        public void Delete()
        {
            if (this.Deleted) return;

            this.Deleted = true;
            this.Mata.WebClient.Delete(this.URL);
        }

        /// <summary>
        /// <para>Sends current message instance.</para>
        /// </summary>
        public void Send()
        {
            if (this._CanSend == false) throw new System.Security.Authentication.AuthenticationException("This message is marked as not sendable!");
            if (this.Sender == null || this.Recipients == null) throw new Exception("Sender and/or Recipients cannot be null!");
            if (string.IsNullOrEmpty(this.Body) || string.IsNullOrEmpty(this.Subject)) throw new Exception("Body and/or Subject cannot be null or empty!");

            this.ToMagisterStyle().Send(this.Mata);
        }

        /// <summary>
        /// Sends current message instance. Instead for throwing expections (MagisterMessage.Send()), this method returns a boolean value.
        /// </summary>
        /// <returns>Boolean value that shows if sending the current instance succeded.</returns>
        public bool TrySend()
        {
            try
            {
                this.Send();
                return true;
            }
            catch { return false; }
        }

        /// <summary>
        /// Converts the current MagisterMessage instance to a sendable one.
        /// </summary>
        /// <returns>A MagisterSendableMessage instance.</returns>
        internal MagisterStyleMessage ToMagisterStyle()
        {
            //Get the sendable from the server and add that instead of the given MagisterPerson instances, so that we will be asured that it will be sendable.
            var tmpReceivers = this.Recipients.ConvertAll(p => p.ToMagisterStyle());
            var tmpCC = (this.CC != null) ? this.CC.ConvertAll(p => p.ToMagisterStyle()) : new List<MagisterStylePerson>();

            return new MagisterStyleMessage()
            {
                Id = this.ID,
                Ref = this.Ref,
                Onderwerp = this.Subject,
                Afzender = this.Sender.ToMagisterStyle(),
                Inhoud = Regex.Replace(this.Body, "[\r\n]", "<br>"),
                Ontvangers = tmpReceivers,
                KopieOntvangers = tmpCC,
                CC = tmpCC,
                VerstuurdOp = this.SentDate.ToUTCString(),
                IsGelezen = this._IsRead,
                Status = this.State,
                HeeftPrioriteit = this.IsFlagged,
                IdOorspronkelijkeBericht = this.IDOriginal,
                IdOntvangerOorspronkelijkeBericht = this.IDOrginalReceiver,
                Bijlagen = new Attachment[0],
                BerichtMapId = this._Folder,
                IsDefinitiefVerwijderd = this.Deleted,
                IdKey = this.IDKey,
                IdDeelNameSoort = this.SenderGroupID
            };
        }

        public override string ToString()
        {
            return "From: " + this.Sender.Description +
                "\nSent: " + this.SentDate.ToString(false) + 
                "\nTo: " + String.Join(", ", this.Recipients.Select(x => x.Name)) +
                ((this.CC.Count > 0) ? "\nCC: " + string.Join(", ", this.CC) : "") +
                ((this.Attachments.Count > 0) ? ("\nAttachments (" + this.Attachments.Count + "): " + String.Join(", ", this.Attachments)) : "") +
                "\nSubject: " + this.Subject +
                "\n\n\"" + this.Body + "\"";
        }

        public int CompareTo(MagisterMessage other)
        {
            return this.SentDate.CompareTo(other.SentDate);
        }

        public MagisterMessage Clone()
        {
            return (MagisterMessage)this.MemberwiseClone();
        }

        object ICloneable.Clone()
        {
            return this.Clone();
        }
    }

    sealed internal partial class MagisterStyleMessage
    {
        #region Content
        public int Id { get; set; }
        public object Ref { get; set; } //???
        public string Onderwerp { get; set; }
        public MagisterStylePerson Afzender { get; set; }
        public string Inhoud { get; set; }
        public List<MagisterStylePerson> Ontvangers { get; set; }
        public List<MagisterStylePerson> KopieOntvangers { get; set; }
        public List<MagisterStylePerson> CC { get; set; }
        public string VerstuurdOp { get; set; }
        public bool IsGelezen { get; set; }
        public int Status { get; set; }
        public bool HeeftPrioriteit { get; set; }
        public int? IdOorspronkelijkeBericht { get; set; }
        public int? IdOntvangerOorspronkelijkeBericht { get; set; }
        public Attachment[] Bijlagen { get; set; }
        public int BerichtMapId { get; set; }
        public bool IsDefinitiefVerwijderd { get; set; }
        public int IdKey { get; set; }
        public int IdDeelNameSoort { get; set; }
        #endregion

        public MagisterMessage ToMagisterMessage(Mata mata, bool download, int index)
        {
            var tmpReceivers = this.Ontvangers.ToList(download, true, mata);
            tmpReceivers.Sort();

            var tmpCopiedReceivers = this.KopieOntvangers.ToList(download, true, mata);
            tmpCopiedReceivers.Sort();

            return new MagisterMessage(mata)
            {
                ID = this.Id,
                Ref = this.Ref,
                Subject = this.Onderwerp,
                Sender = this.Afzender.ToPerson(download, mata),
                Body = this.Inhoud.Trim(),
                Recipients = tmpReceivers,
                CC = tmpCopiedReceivers,
                SentDate = this.VerstuurdOp.ToDateTime(),
                _IsRead = this.IsGelezen,
                State = this.Status,
                IsFlagged = this.HeeftPrioriteit,
                IDOriginal = this.IdOorspronkelijkeBericht,
                IDOrginalReceiver = this.IdOntvangerOorspronkelijkeBericht,
                Attachments = this.Bijlagen.ToList(AttachmentType.Message, mata),
                _Folder = this.BerichtMapId,
                Deleted = this.IsDefinitiefVerwijderd,
                IDKey = this.IdKey,
                SenderGroupID = this.IdDeelNameSoort,
                _CanSend = false,
                Index = index
            };
        }

        internal void Send(Mata mata)
        {
            string URL = "https://" + mata.School.URL + "/api/personen/" + mata.UserID + "/communicatie/berichten/";
            mata.WebClient.Post(URL, JsonConvert.SerializeObject(this));
        }
    }
}
