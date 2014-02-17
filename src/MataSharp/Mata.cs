/*(c) 2014 Lieuwe Rooijakkers
MataSharp; Public C# implementation of the non public 'Mata' API. */
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MataSharp
{
    /// <summary>
    /// Type to communicate with a Magister School's server.
    /// </summary>
    public partial class Mata : IDisposable, ICloneable
    {
        public string Name { get; private set; }
        public uint UserID { get; private set; }
        public string SessionID { get; private set; }

        public string UserName { get; private set; }
        internal MagisterSchool School { get; private set; }
        public MagisterPerson Person { get; private set; }

        internal readonly MataHTTPClient HttpClient;
        internal readonly Dictionary<string, List<MagisterStylePerson>> checkedPersons = new Dictionary<string, List<MagisterStylePerson>>();

        /// <summary>
        /// <para>Creates 'Mata' instance to communicate with the Mata server of the specified school.</para>
        /// </summary>
        /// <param name="School">School which to log in to.</param>
        /// <param name="UserName">Name to log in with.</param>
        /// <param name="UserPassword">Password to log in with.</param>
        public Mata(MagisterSchool School, string UserName, string UserPassword)
        {
            this.HttpClient = new MataHTTPClient();

            this.School = School;
            this.UserName = UserName;

            string url = "https://" + School.URL + "/api/sessie";
            string response = this.HttpClient.Post(url, new NameValueCollection()
                {
                    {"Gebruikersnaam", UserName},
                    {"Wachtwoord", UserPassword}
                });
            var cleanResponse = JsonConvert.DeserializeObject<MagisterStyleMata>(response);
            this.Name = cleanResponse.Naam;
            this.UserID = uint.Parse(cleanResponse.GebruikersId);
            this.SessionID = cleanResponse.SessieId;

            this.HttpClient.Cookie = "SESSION_ID=" + this.SessionID + "&fileDownload=true"; //yummy! cookies!

            this.Person = this.GetPersons(this.Name)[0]; //Get itself as MagisterPerson from the servers.
        }

        /// <summary>
        /// Quickly composes new MagisterMessage and sends it.
        /// </summary>
        /// <param name="Subject">Subject to use</param>
        /// <param name="Body">Body to use</param>
        /// <param name="Recipients">MagisterPersons to send to</param>
        public void ComposeAndSendMessage(string Subject, string Body, IEnumerable<MagisterPerson> Recipients)
        {
            new MagisterMessage(this)
            {
                Subject = Subject,
                Body = Body,
                Recipients = Recipients.ToList(this)
            }.Send();
        }

        /// <summary>
        /// Quickly composes new MagisterMessage and sends it.
        /// </summary>
        /// <param name="Subject">Subject to use</param>
        /// <param name="Body">Body to use</param>
        /// <param name="Recipients">Name of the persons to send to</param>
        public void ComposeAndSendMessage(string Subject, string Body, IEnumerable<string> Recipients)
        {
            new MagisterMessage(this)
            {
                Subject = Subject,
                Body = Body,
                Recipients = new PersonList(Recipients, this)
            }.Send();
        }

        /// <summary>
        /// Quickly composes new MagisterMessage and sends it. Instead of throwing exceptions (ComposeAndSendMessage()) this gives back a boolean value.
        /// </summary>
        /// <param name="Subject">Subject to use</param>
        /// <param name="Body">Body to use</param>
        /// <param name="Recipients">MagisterPersons to send to</param>
        public bool ComposeAndTrySendMessage(string Subject, string Body, IEnumerable<MagisterPerson> Recipients)
        {
            return new MagisterMessage(this)
            {
                Subject = Subject,
                Body = Body,
                Recipients = Recipients.ToList(this)
            }.TrySend();
        }

        /// <summary>
        /// Quickly composes new MagisterMessage and sends it. Instead of throwing exceptions (ComposeAndSendMessage()) this gives back a boolean value.
        /// </summary>
        /// <param name="Subject">Subject to use</param>
        /// <param name="Body">Body to use</param>
        /// <param name="Recipients">MagisterPersons to send to</param>
        public bool ComposeAndTrySendMessage(string Subject, string Body, IEnumerable<string> Recipients)
        {
            return new MagisterMessage(this)
            {
                Subject = Subject,
                Body = Body,
                Recipients = new PersonList(Recipients, this)
            }.TrySend();
        }

        /// <summary>
        /// <para>Get all messagefolders linked with the current Mata instance.</para>
        /// </summary>
        public List<MagisterMessageFolder> GetMessageFolders()
        {
            string url = "https://" + this.School.URL + "/api/personen/" + this.UserID + "/communicatie/berichten/mappen?$skip=0&$top=50";

            string MessageFoldersRAW = this.HttpClient.DownloadString(url);
            var MessageFolders = JsonConvert.DeserializeObject<MagisterStyleMessageFolderListItem[]>(MessageFoldersRAW);

            var tmplst = new List<MagisterMessageFolder>();
            foreach (var messageFolder in MessageFolders)
            {
                tmplst.Add(new MagisterMessageFolder()
                {
                    Name = messageFolder.Naam,
                    UnreadMessagesCount = messageFolder.OngelezenBerichten,
                    ID = messageFolder.Id,
                    ParentID = messageFolder.ParentId,
                    Ref = messageFolder.Ref,
                    MessagesURI = messageFolder.BerichtenUri,
                    Mata = this,
                    FolderType = (MessageFolder)messageFolder.Id
                });
            }
            return tmplst;
        }

        /// <summary>
        /// Returns all Magisterpersons filtered by the given search filter as a list.
        /// </summary>
        /// <param name="SearchFilter">The search filter to use as string.</param>
        /// <returns>List containing MagisterPerson instances</returns>
        public PersonList GetPersons(string SearchFilter)
        {
            if (string.IsNullOrWhiteSpace(SearchFilter) || SearchFilter.Length < 3) return new PersonList(0, this);

            if (!this.checkedPersons.ContainsKey(SearchFilter))
            {
                string URL = "https://" + this.School.URL + "/api/personen/" + this.UserID + "/communicatie/contactpersonen?q=" + SearchFilter;

                string personsRAW = this.HttpClient.DownloadString(URL);

                var personRaw = JArray.Parse(personsRAW).ToList().ConvertAll(p => p.ToObject<MagisterStylePerson>());
                this.checkedPersons.Add(SearchFilter, personRaw);
                return new PersonList(this, personRaw, false, false);
            }
            else return new PersonList(this, this.checkedPersons.First(x => x.Key.ToUpper() == SearchFilter.ToUpper()).Value,false, false);
        }

        public List<Homework> GetHomework()
        {
            string URL = "https://" + this.School.URL + "/api/leerlingen/" + this.UserID + "/huiswerk/huiswerk";

            string homeworkListRaw = this.HttpClient.DownloadString(URL);
            var homeworkListClean = JsonConvert.DeserializeObject<HuiswerkLijst>(homeworkListRaw);

            var compactHomeworkItemDays = homeworkListClean.Items.Select(x=>x.Items);

            var tmpList = new List<Homework>();

            foreach (var compactHomeworkItemDay in compactHomeworkItemDays)
            {
                foreach (var compactHomeworkItem in compactHomeworkItemDay)
                {
                    URL = "https://" + this.School.URL + "/api/leerlingen/" + this.UserID + "/huiswerk/huiswerk/" + compactHomeworkItem.Id;
                    string homeworkItemRaw = this.HttpClient.DownloadString(URL);
                    var tmpHomework = JsonConvert.DeserializeObject<Huiswerk>(homeworkItemRaw).ToHomework(this);
                    tmpHomework.ClassAbbreviation = compactHomeworkItem.VakAfkortingen; //Full homework from the server doesn't contain the classAbbreviations.
                    tmpList.Add(tmpHomework);
                }
            }
            return tmpList;
        }

        public List<Homework> GetTasks()
        {
            string URL = "https://" + this.School.URL + "/api/leerlingen/" + this.UserID + "/huiswerk/taken";

            string homeworkListRaw = this.HttpClient.DownloadString(URL);
            var homeworkListClean = JsonConvert.DeserializeObject<HuiswerkLijst>(homeworkListRaw);

            var compactIDs_Days = homeworkListClean.Items.Select(x => x.Items.Select(y => y.Id));

            var tmpList = new List<Homework>();

            foreach (var compactIDday in compactIDs_Days)
            {
                foreach (int compactID in compactIDday)
                {
                    URL = "https://" + this.School.URL + "/api/leerlingen/" + this.UserID + "/huiswerk/huiswerk/" + compactID;
                    string homeworkItemRaw = this.HttpClient.DownloadString(URL);
                    tmpList.Add(JsonConvert.DeserializeObject<Huiswerk>(homeworkItemRaw).ToHomework(this));
                }
            }
            return tmpList;
        }

        public List<Homework> GetTests()
        {
            string URL = "https://" + this.School.URL + "/api/leerlingen/" + this.UserID + "/huiswerk/toetsen";

            string homeworkListRaw = this.HttpClient.DownloadString(URL);
            var homeworkListClean = JsonConvert.DeserializeObject<HuiswerkLijst>(homeworkListRaw);

            var compactIDs_Days = homeworkListClean.Items.Select(x => x.Items.Select(y => y.Id));

            var tmpList = new List<Homework>();

            foreach (var compactIDday in compactIDs_Days)
            {
                foreach (int compactID in compactIDday)
                {
                    URL = "https://" + this.School.URL + "/api/leerlingen/" + this.UserID + "/huiswerk/huiswerk/" + compactID;
                    string homeworkItemRaw = this.HttpClient.DownloadString(URL);
                    tmpList.Add(JsonConvert.DeserializeObject<Huiswerk>(homeworkItemRaw).ToHomework(this));
                }
            }
            return tmpList;
        }

        public List<StudyGuide> GetStudyGuides()
        {
            string URL = "https://" + this.School.URL + "/api/leerlingen/" + this.UserID + "/studiewijzers?$skip=0&$top=50";

            string compactStudyGuidesRaw = this.HttpClient.DownloadString(URL);
            var compactStudyGuidesClean = JsonConvert.DeserializeObject<StudieWijzerLijst>(compactStudyGuidesRaw).Items;

            var list = new List<StudyGuide>();
            foreach(var compactStudyGuide in compactStudyGuidesClean)
            {
                URL = "https://" + School.URL + "/api/leerlingen/" + this.UserID + "/studiewijzers/" + compactStudyGuide.Id;

                string studyGuideRaw = this.HttpClient.DownloadString(URL);
                var studyGuideClean = JsonConvert.DeserializeObject<StudieWijzer>(studyGuideRaw); studyGuideClean.Mata = this;

                list.Add(studyGuideClean.ToStudyGuide());
            }
            return list;
        }

        public List<Assignment> GetAssignments()
        {
            string URL = "https://" + this.School.URL + "/api/leerlingen/" + this.UserID + "/opdrachten/status/openstaand?$skip=0&$top=30";

            string CompactAssignmentsRaw = this.HttpClient.DownloadString(URL);
            var CompactAssignments = JsonConvert.DeserializeObject<AssignmentFolder>(CompactAssignmentsRaw);

            List<Assignment> list = new List<Assignment>();
            foreach (var CompactAssignment in CompactAssignments.Items)
            {
                URL = "https://" + School.URL + "/api/leerlingen/" + this.UserID + "/opdrachten/" + CompactAssignment.Id;
                string AssignmentRaw = this.HttpClient.DownloadString(URL);
                var AssignmentClean = JsonConvert.DeserializeObject<AssignmentFolderItem>(AssignmentRaw); AssignmentClean.Mata = this;
                list.Add(AssignmentClean.toAssignment());
            }
            return list;
        }

        public List<DigitalSchoolUtility> GetDigitalSchoolUtilities()
        {
            string URL = "https://" + this.School.URL + "/api/leerlingen/" + this.UserID + "/digitaallesmateriaal/vakken";

            string compactUtilitiesRaw = this.HttpClient.DownloadString(URL);
            var compactUtilities = JArray.Parse(compactUtilitiesRaw).ToList().ConvertAll(u => u.ToObject<DigitaalLesMatriaalLijstItem>());

            var tmpList = new List<DigitalSchoolUtility>();
            foreach(var compactUtility in compactUtilities)
            {
                URL = "https://" + this.School.URL + "/api/leerlingen/" + this.UserID + "/digitaallesmateriaal/vakken/" + compactUtility.Id;

                string utilityRaw = this.HttpClient.DownloadString(URL);
                var utility = JsonConvert.DeserializeObject<DigitaalLesMatriaal[]>(utilityRaw)[0];
                utility.Id = compactUtility.Id; //Fix for the 'small' problem in the Mata API.
                tmpList.Add(utility.ToDigitalSchoolUtility());
            }
            return tmpList;
        }

        public MagisterMessageFolder Inbox
        {
            get { return this.GetMessageFolders().First(mf => mf.FolderType == MessageFolder.Inbox); }
        }

        public MagisterMessageFolder SentMessages
        {
            get { return this.GetMessageFolders().First(mf => mf.FolderType == MessageFolder.SentMessages); }
        }

        public MagisterMessageFolder Bin
        {
            get { return this.GetMessageFolders().First(mf => mf.FolderType == MessageFolder.Bin); }
        }

        /// <summary>
        /// Clones the current Mata instance.
        /// </summary>
        /// <returns>A new Mata instance that is identical to the current one.</returns>
        public Mata Clone()
        {
            return (Mata)this.MemberwiseClone();
        }

        /// <summary>
        /// Checks if the current Mata instance is equal to the given target.
        /// </summary>
        /// <param name="Target">The other Mata instance to check if equal.</param>
        public bool Equals(Mata Target)
        {
            return (this.UserName == Target.UserName && this.UserID == Target.UserID && this.SessionID == Target.SessionID && this.Name == Target.Name);
        }

        ~Mata() { this.Dispose(); }
        /// <summary>
        /// Disposes the current Mata instance.
        /// </summary>
        public void Dispose() { this.HttpClient.Dispose(); GC.Collect(); }


        /// <summary>
        /// Checks if the given Mata instances are equal to each other.
        /// </summary>
        /// <param name="TargetA">First Mata instance</param>
        /// <param name="TargetB">Second Mata instance</param>
        public static bool Equals(Mata TargetA, Mata TargetB)
        {
            return (TargetA.UserName == TargetB.UserName && TargetA.UserID == TargetB.UserID && TargetA.SessionID == TargetB.SessionID && TargetA.Name == TargetB.Name);
        }

        public override bool Equals(object obj)
        {
            return Equals(this, (Mata)obj);
        }

        object ICloneable.Clone()
        {
            return this.Clone();
        }
    }

    internal partial struct MagisterStyleMata
    {
        public string Naam { get; set; }
        public string GebruikersId { get; set; }
        public string SessieId { get; set; }
        public string Message { get; set; }
        public string State { get; set; }
    }
}
