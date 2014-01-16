using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using System.Globalization;
using System.Text.RegularExpressions;

namespace MataSharp
{
    public partial class MagisterMessage
    {
        #region Contents
        public int ID { get; set; }
        public object Ref { get; set; } // Even Schoolmaster doesn't know what this is, it's mysterious. Just keep it in case.
        public string Subject { get; set; }
        public MagisterPerson Sender { get; set; }
        public string Body { get; set; }
        public List<MagisterPerson> Recipients { get; set; }
        public List<MagisterPerson> CC { get; set; }
        public DateTime? SentDate { get; set; }
        internal bool _IsRead;

        public bool IsRead
        {
            get { return this._IsRead; }
            set
            {
                if (this._IsRead == value) return; //You don't have to do what's already done.

                this._IsRead = value;

                _Session.HttpClient.Put(this.URL(), JsonConvert.SerializeObject(this.ToMagisterStyle()));
            }
        }

        public int State { get; set; }
        public bool IsFlagged { get; set; }
        public int? IDOriginal { get; set; }
        public int? IDOrginalReceiver { get; set; }
        public List<Attachment> Attachments { get; internal set; }
        internal MessageFolderType _FolderType { get; set; }

        public MessageFolderType FolderType
        {
            get { return this._FolderType; }
            set
            {
                if (this._FolderType == value) return;

                this._FolderType = value;

                _Session.HttpClient.Put(this.URL(), JsonConvert.SerializeObject(this.ToMagisterStyle()));
            }
        }

        public bool Deleted { get; internal set; }
        public int IDKey { get; set; }
        public int SenderGroupID { get; set; }

        public bool CanSend { get; internal set; }
        internal MagisterSchool School { get; set; }
        internal Mata Mata { get; set; }
        #endregion

        private string URL() { return "https://" + this.School.URL + "/api/personen/" + this.Mata.UserID + "/communicatie/berichten/mappen/" + this.FolderTypeAsInt() + "/berichten/" + this.ID; }

        /// <summary>
        /// Returns new MagisterMessage.
        /// </summary>
        internal MagisterMessage()
        {
            //Auto Fill-In some magic things >:D
            this.Mata = _Session.Mata;
            this.School = _Session.School;
            this.CanSend = true;

            this.ID = 0;
            this._IsRead = false;
            this.State = 0;
            this.IDOriginal = 0;
            this.IDOrginalReceiver = 0;
            this._FolderType = 0;
            this.Deleted = false;
            this.IDKey = 0;
            this.SenderGroupID = this.Mata.Person.GroupID;
            this.Sender = this.Mata.Person;
            this.Recipients = new List<MagisterPerson>();
            this.CC = new List<MagisterPerson>();
        }

        /// <summary>
        /// Returns new MagisterMessage
        /// </summary>
        /// <param name="Mata">The mata instance to use.</param>
        /// <param name="School">The school to use.</param>
        public MagisterMessage(Mata Mata = null, MagisterSchool School = null) : this()
        {
            this.Mata = Mata ?? _Session.Mata;
            this.School = School ?? _Session.School;
        }

        internal readonly static Dictionary<MessageFolderType, int> FolderType_ID = new Dictionary<MessageFolderType, int>()
                {
                    {MessageFolderType.Inbox, -101},
                    {MessageFolderType.Bin, -102},
                    {MessageFolderType.SentMessages,-103}
                };

        private int FolderTypeAsInt()
        {
            return FolderType_ID.Where(x => x.Key == this._FolderType).ElementAt(0).Value;
        }

        internal static string DayOfWeekToString(DayOfWeek dayOfWeek)
        {
            switch(dayOfWeek)
            {
                case DayOfWeek.Monday: return "maandag";
                case DayOfWeek.Tuesday: return "dinsdag";
                case DayOfWeek.Wednesday: return "woensdag";
                case DayOfWeek.Thursday: return "donderdag";
                case DayOfWeek.Friday: return "vrijdag";
                case DayOfWeek.Saturday:return "zaterdag";
                case DayOfWeek.Sunday: return "zondag";
                default: return "maandag";
            }
        }

        /// <summary>
        /// Creates new MagisterMessage that forwards the current message.
        /// </summary>
        /// <returns>A new MagisterMessage instance ready to be send.</returns>
        public MagisterMessage CreateForwardMessage()
        {
            var tmpSubject = (this.Subject[0] != 'F' || this.Subject[1] != 'W' || this.Subject[2] != ':' || this.Subject[3] != ' ') ? "FW: " + this.Subject : this.Subject;

            return new MagisterMessage()
            {
                Sender = this.Sender,//Magister's logic
                _FolderType = this._FolderType,
                Attachments = new List<Attachment>(),
                CC = null,
                IsFlagged = this.IsFlagged,
                ID = this.ID,
                SenderGroupID = 4,
                IDKey = this.IDKey,
                IDOrginalReceiver = null,
                IDOriginal = null,
                Body = this.Body,
                Deleted = false,
                _IsRead = true,
                Subject = tmpSubject,
                Recipients = new List<MagisterPerson>(),
                Ref = null,
                State = 0,
                SentDate = DateTime.UtcNow,
                CanSend = true
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

            return new MagisterMessage()
            {
                Sender = this.Sender,//Magister's logic
                _FolderType = this._FolderType,
                Attachments = new List<Attachment>(),
                IsFlagged = this.IsFlagged,
                ID = this.ID,
                SenderGroupID = 4,
                IDKey = this.IDKey,
                IDOrginalReceiver = null,
                IDOriginal = null,
                Body = ContentAdd + "<br><br>---------------<br>Van: " + this.Sender.Name + "<br>Verzonden: " + DayOfWeekToString(this.SentDate.Value.DayOfWeek) + " " + this.SentDate.Value.ToString() + "<br>Aan: " + String.Join(", ",this.Recipients.Select(x=>x.Name)) + "<br>Onderwerp: " + this.Subject + "<br><br>\"" + this.Body + "\"<br><br>",
                Deleted = false,
                _IsRead = true,
                Subject = tmpSubject,
                Recipients = new List<MagisterPerson>(),
                Ref = null,
                State = 0,
                SentDate = DateTime.UtcNow,
                CanSend = true
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

            var tmpCC = this.Recipients.ToList().Where(p => p.ID != _Session.Mata.Person.ID).ToList(); //Should get the current receivers and pull itself out. :)
            if (this.CC != null) tmpCC.AddRange(this.CC.ToList().Where(p => p.ID != _Session.Mata.Person.ID).ToList());
            tmpCC.OrderBy(p => p.SurName);

            return new MagisterMessage()
            {
                Sender = this.Sender,
                _FolderType = this._FolderType,
                Attachments = new List<Attachment>(),
                CC = tmpCC,
                IsFlagged = this.IsFlagged,
                ID = this.ID,
                SenderGroupID = 4,
                IDKey = this.IDKey,
                IDOrginalReceiver = null,
                IDOriginal = null,
                Body = ContentAdd + "<br><br>---------------<br>Van: " + this.Sender.Name + "<br>Verzonden: " + DayOfWeekToString(this.SentDate.Value.DayOfWeek) + " " + this.SentDate.Value.ToString() + "<br>Aan: " + String.Join(", ", this.Recipients.Select(x => x.Name)) + "<br>Onderwerp: " + this.Subject + "<br><br>\"" + this.Body + "\"<br><br>",
                Deleted = false,
                _IsRead = true,
                Subject = tmpSubject,
                Recipients = new List<MagisterPerson>(){ this.Sender },
                Ref = null,
                State = 0,
                SentDate = DateTime.UtcNow,
                CanSend = true
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

            return new MagisterMessage()
            {
                Sender = this.Sender,//Magister's logic
                _FolderType = this._FolderType,
                Attachments = new List<Attachment>(),
                CC = null,
                IsFlagged = this.IsFlagged,
                ID = this.ID,
                SenderGroupID = 4,
                IDKey = this.IDKey,
                IDOrginalReceiver = null,
                IDOriginal = null,
                Body = ContentAdd + "<br><br>---------------<br>Van: " + this.Sender.Name + "<br>Verzonden: " + DayOfWeekToString(this.SentDate.Value.DayOfWeek) + " " + this.SentDate.Value.ToString() + "<br>Aan: " + String.Join(", ", this.Recipients.Select(x => x.Name)) + "<br>Onderwerp: " + this.Subject + "<br><br>\"" + this.Body + "\"<br><br>",
                Deleted = false,
                _IsRead = true,
                Subject = tmpSubject,
                Recipients = new List<MagisterPerson>() { this.Sender },
                Ref = null,
                State = 0,
                SentDate = DateTime.UtcNow,
                CanSend = true
            };
        }

        /// <summary>
        /// Permanently deletes the current message on the server.
        /// </summary>
        public void Delete()
        {
            if (this.Deleted == true) return;

            this.Deleted = true;
            _Session.HttpClient.Delete(this.URL());
        }

        /// <summary>
        /// <para>Sends current message instance.</para>
        /// </summary>
        public void Send()
        {
            if (this.CanSend == false) throw new System.Security.Authentication.AuthenticationException("This message is marked as not sendable!");
            if (this.Sender == null || this.Recipients == null) throw new Exception("Sender and/or Recipients cannot be null!");
            if (string.IsNullOrEmpty(this.Body) || string.IsNullOrEmpty(this.Subject)) throw new Exception("Body and/or Subject cannot be null or empty!");

            this.ToMagisterStyle().Send();
        }

        /// <summary>
        /// Sends current message instance. Except for throwing expections (MagsiterMessage.Send()), this method returns a boolean value.
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
            var tmpReceivers = (this.CanSend == true) ? this.Recipients.ConvertAll(p => p.ToMagisterStyle()).ToArray() : this.Recipients.ConvertAll(p => p.ToMagisterStyle()).ToArray();
            var tmpCC = (this.CC != null && this.CanSend == true) ? this.CC.ConvertAll(p => p.ToMagisterStyle()).ToArray() : (this.CC == null && this.CanSend == true) ? new MagisterStylePerson[0] : this.CC.ConvertAll(p => p.ToMagisterStyle()).ToArray();

            var tmpSentDate = (this.SentDate != null) ? this.SentDate.Value.ToString("yyyy-MM-ddTHH:mm:ss.0000000Z") : DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.0000000Z"); //2013-12-30T00:08:16.0000000Z

            return new MagisterStyleMessage()
            {
                Id = this.ID,
                Ref = this.Ref,
                Onderwerp = this.Subject,
                Afzender = this.Sender.ToMagisterStyle(),
                Inhoud = this.Body,
                Ontvangers = tmpReceivers,
                KopieOntvangers = tmpCC,
                CC = tmpCC,
                VerstuurdOp = tmpSentDate,
                IsGelezen = this._IsRead,
                Status = this.State,
                HeeftPrioriteit = this.IsFlagged,
                IdOorspronkelijkeBericht = this.IDOriginal,
                IdOntvangerOorspronkelijkeBericht = this.IDOrginalReceiver,
                Bijlagen = new Attachment[0],
                BerichtMapId = this.FolderTypeAsInt(),
                IsDefinitiefVerwijderd = this.Deleted,
                IdKey = this.IDKey,
                IdDeelNameSoort = this.SenderGroupID,
            };
        }
    }

    internal partial class MagisterStyleMessage
    {
        #region Content
        public int Id { get; set; }
        public object Ref { get; set; } //???
        public string Onderwerp { get; set; }
        public MagisterStylePerson Afzender { get; set; }
        public string Inhoud { get; set; }
        public MagisterStylePerson[] Ontvangers { get; set; }
        public MagisterStylePerson[] KopieOntvangers { get; set; }
        public MagisterStylePerson[] CC { get; set; }
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

        public MagisterMessage ToMagisterMessage()
        {
            var tmpReceivers = this.Ontvangers.OrderBy(p => p.Achternaam).ToList().ConvertAll(p => p.ToPerson());

            var tmpCopiedReceivers = this.KopieOntvangers.OrderBy(p => p.Achternaam).ToList().ConvertAll(p => p.ToPerson());

            var tmpAttachments = this.Bijlagen.ToList();
            tmpAttachments.ForEach(x => x.Type = AttachmentType.Message);

            var tmpFolderType = MagisterMessage.FolderType_ID.Where(x => x.Value == this.BerichtMapId).ElementAt(0).Key;

            return new MagisterMessage()
            {
                ID = this.Id,
                Ref = this.Ref,
                Subject = this.Onderwerp,
                Sender = this.Afzender.ToPerson(),
                Body = this.Inhoud,
                Recipients = tmpReceivers,
                CC = tmpCopiedReceivers,
                SentDate = DateTime.Parse(this.VerstuurdOp, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal),
                _IsRead = this.IsGelezen,
                State = this.Status,
                IsFlagged = this.HeeftPrioriteit,
                IDOriginal = this.IdOorspronkelijkeBericht,
                IDOrginalReceiver = this.IdOntvangerOorspronkelijkeBericht,
                Attachments = tmpAttachments,
                _FolderType = tmpFolderType,
                Deleted = this.IsDefinitiefVerwijderd,
                IDKey = this.IdKey,
                SenderGroupID = this.IdDeelNameSoort,
                CanSend = false
            };
        }

        public void Send()
        {
            string URL = "https://" + _Session.School.URL + "/api/personen/" + _Session.Mata.UserID + "/communicatie/berichten/";
            _Session.HttpClient.Post(URL, JsonConvert.SerializeObject(this));
        }
    }
}
