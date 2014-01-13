using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using Newtonsoft.Json;

namespace MataSharp
{
    public partial class Assignment
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public uint? Grade { get; set; }
        public string Class { get; set; }
        public List<Attachment> Attachments { get; set; }
        public List<MagisterPerson> Teachers { get; set; }
        public int ID { get; set; }
        public DateTime HandInTime { get; set; }
        public DateTime DeadLine { get; set; }
        public int LastAssignmentVersionID { get; set; } //???
        public object Ref { get; set; } //Good old friend
        public int StateLastAssignmentVersion { get; set; } //???

        public List<AssignmentVersion> Versions { get; set; }

    }

    public partial class AssignmentVersion
    {
        public string Name { get; set; }
        public uint? Grade { get; set; }
        public string TeacherNotice { get; set; }
        public List<Attachment> FeedbackAttachments { get; set; }
        public DateTime DeadLine { get; set; }
        public DateTime HandInTime { get; set; }
        public List<Attachment> HandedInAttachments { get; set; }
        public string HandedInFooter { get; set; }
    }

    internal partial struct AssignmentFolder
    {
        public object[] Items { get; set; }
        public object Paging { get; set; }
        public int TotalCount { get; set; } //Looks like that's broken at their side.
    }

    internal partial struct AssignmentFolderListItem
    {
        public uint? Beoordeling { get; set; }
        public Attachment[] Bijlagen { get; set; }
        public MagisterStylePerson[] Docenten { get; set; } //According to the responses I got, that can be an empty array?
        public int Id { get; set; }
        public string IngeleverdOp { get; set; }
        public string InleverenVoor { get; set; }
        public int LaatsteOpdrachtVersieId { get; set; }
        public string Omschrijving { get; set; }
        public object Ref { get; set; }
        public int StatusLaatsteOpdrachtVersie { get; set; }
        public string Titel { get; set; }
        public string Vak { get; set; }
        public object VersieNavigatieItems { get; set; }
    }

    internal partial struct MagisterStyleAssignmentListVersion
    {
        public int Id { get; set; }
        public string Omschrijving { get; set; }
        public object Ref { get; set; }
    }

    internal partial struct MagisterStyleAssignmentVersion
    {
        public uint? Beoordeling { get; set; }
        public string DocentOpmerking { get; set; }
        public Attachment[] FeedbackBijlagen { get; set; }
        public string IngeleverdOp { get; set; }
        public string InleverenVoor { get; set; }
        public Attachment[] LeerlingBijlagen { get; set; }
        public string LeerlingOpmerking { get; set; }
        public string Titel { get; set; }

        public AssignmentVersion ToVersion()
        {
            var tmpHandedInAttachments = this.LeerlingBijlagen.ToList();
            tmpHandedInAttachments.ForEach(x => x.Type = AttachmentType.Assignment_pupil);

            var tmpFeedbackAttachments = this.FeedbackBijlagen.ToList();
            tmpFeedbackAttachments.ForEach(x => x.Type = AttachmentType.Assignment_teacher);

            return new AssignmentVersion()
            {
                Grade = this.Beoordeling,
                TeacherNotice = this.DocentOpmerking,
                FeedbackAttachments = tmpFeedbackAttachments,
                HandInTime = (this.IngeleverdOp != null) ? DateTime.Parse(this.IngeleverdOp, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal) : new DateTime(),
                DeadLine = (this.IngeleverdOp != null) ? DateTime.Parse(this.IngeleverdOp, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal) : new DateTime(),
                HandedInAttachments = tmpHandedInAttachments,
                HandedInFooter = this.LeerlingOpmerking,
                Name = this.Titel
            };
        }
    }

    internal partial struct AssignmentFolderItem
    {
        public uint? Beoordeling { get; set; }
        public Attachment[] Bijlagen { get; set; }
        public MagisterStylePerson[] Docenten { get; set; }
        public int Id { get; set; }
        public string IngeleverdOp { get; set; }
        public string InleverenVoor { get; set; }
        public int LaatsteOpdrachtVersieId { get; set; }
        public string Omschrijving { get; set; }
        public object Ref { get; set; }
        public int StatusLaatsteOpdrachtVersie { get; set; }
        public string Titel { get; set; }
        public string Vak { get; set; }
        public MagisterStyleAssignmentListVersion[] VersieNavigatieItems { get; set; }

        public Assignment toAssignment()
        {
            var tmpVersions = new List<AssignmentVersion>();
            foreach(var compactAssignmentVersion in this.VersieNavigatieItems)
            {
                string URL = "https://" + _Session.School.URL + "/api/leerlingen/" + _Session.Mata.UserID + "/opdrachten/" + this.Id + "/versie/" + compactAssignmentVersion.Id;

                string versionRaw = _Session.HttpClient.DownloadString(URL);
                var versionClean = JsonConvert.DeserializeObject<MagisterStyleAssignmentVersion>(versionRaw);

                tmpVersions.Add(versionClean.ToVersion());
            }

            var tmpAttachments = this.Bijlagen.ToList();
            tmpAttachments.ForEach(x => x.Type = AttachmentType.Assignment_teacher);

            return new Assignment()
            {
                Grade = this.Beoordeling,
                Attachments = tmpAttachments,
                Teachers = this.Docenten.ToList().ConvertAll(p => p.ToPerson()),
                ID = this.Id,
                HandInTime = (this.IngeleverdOp != null) ? DateTime.Parse(this.IngeleverdOp, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal) : new DateTime(),
                DeadLine = (this.InleverenVoor != null) ? DateTime.Parse(this.InleverenVoor,CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal) : new DateTime(),
                LastAssignmentVersionID = this.LaatsteOpdrachtVersieId,
                Description = this.Omschrijving,
                Ref = this.Ref,
                StateLastAssignmentVersion = this.StatusLaatsteOpdrachtVersie,
                Name = this.Titel,
                Class = this.Vak,
                Versions = tmpVersions
            };
        }
    }
}
