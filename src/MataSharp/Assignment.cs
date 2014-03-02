using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Newtonsoft.Json;

namespace MataSharp
{
    public partial class Assignment : IComparable<Assignment>
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public uint? Grade { get; set; }
        public string Class { get; set; }
        public ReadOnlyCollection<Attachment> Attachments { get; internal set; }
        public PersonList Teachers { get; internal set; }
        public int ID { get; set; }
        public DateTime HandInTime { get; set; }
        public DateTime DeadLine { get; set; }
        public int LastAssignmentVersionID { get; set; } //???
        public object Ref { get; set; } //Good old friend
        public int StateLastAssignmentVersion { get; set; } //???

        public List<AssignmentVersion> Versions { get; internal set; }

        public override string ToString() { return this.Name; }

        public int CompareTo(Assignment other)
        {
            var dateCompared = this.DeadLine.CompareTo(other.DeadLine);
            return (dateCompared != 0) ? dateCompared : this.Class.CompareTo(other.Class);
        }
    }

    public partial class AssignmentVersion
    {
        public string Name { get; set; }
        public uint? Grade { get; set; }
        public string TeacherNotice { get; set; }
        public ReadOnlyCollection<Attachment> FeedbackAttachments { get; internal set; }
        public DateTime DeadLine { get; set; }
        public DateTime HandInTime { get; set; }
        public ReadOnlyCollection<Attachment> HandedInAttachments { get; internal set; }
        public string HandedInFooter { get; set; }

        public override string ToString() { return this.Name; }
    }

    internal struct AssignmentFolder
    {
        public AssignmentFolderItem[] Items { get; set; }
        public object Paging { get; set; }
        public int TotalCount { get; set; } //Looks like that's broken at their side.
    }

    internal struct AssignmentFolderListItem
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

    internal struct MagisterStyleAssignmentListVersion
    {
        public int Id { get; set; }
        public string Omschrijving { get; set; }
        public object Ref { get; set; }
    }

    sealed internal class MagisterStyleAssignmentVersion
    {
        public uint? Beoordeling { get; set; }
        public string DocentOpmerking { get; set; }
        public Attachment[] FeedbackBijlagen { get; set; }
        public string IngeleverdOp { get; set; }
        public string InleverenVoor { get; set; }
        public Attachment[] LeerlingBijlagen { get; set; }
        public string LeerlingOpmerking { get; set; }
        public string Titel { get; set; }

        public AssignmentVersion ToVersion(Mata mata)
        {
            return new AssignmentVersion()
            {
                Grade = this.Beoordeling,
                TeacherNotice = this.DocentOpmerking,
                FeedbackAttachments = this.FeedbackBijlagen.ToList(AttachmentType.Assignment_teacher, mata),
                HandInTime = this.IngeleverdOp.ToDateTime(),
                DeadLine = this.InleverenVoor.ToDateTime(),
                HandedInAttachments = this.LeerlingBijlagen.ToList(AttachmentType.Assignment_pupil, mata),
                HandedInFooter = this.LeerlingOpmerking,
                Name = this.Titel
            };
        }
    }

    sealed internal class AssignmentFolderItem
    {
        public uint? Beoordeling { get; set; }
        public Attachment[] Bijlagen { get; set; }
        public List<MagisterStylePerson> Docenten { get; set; }
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

        public Assignment toAssignment(Mata mata)
        {
            var tmpVersions = new List<AssignmentVersion>();
            foreach(var compactAssignmentVersion in this.VersieNavigatieItems)
            {
                string URL = "https://" + mata.School.URL + "/api/leerlingen/" + mata.UserID + "/opdrachten/" + this.Id + "/versie/" + compactAssignmentVersion.Id;

                string versionRaw = mata.WebClient.DownloadString(URL);
                var versionClean = JsonConvert.DeserializeObject<MagisterStyleAssignmentVersion>(versionRaw);

                tmpVersions.Add(versionClean.ToVersion(mata));
            }

            return new Assignment()
            {
                Grade = this.Beoordeling,
                Attachments = this.Bijlagen.ToList(AttachmentType.Assignment_teacher, mata),
                Teachers = this.Docenten.ToList(true, true, mata),
                ID = this.Id,
                HandInTime = this.IngeleverdOp.ToDateTime(),
                DeadLine = this.InleverenVoor.ToDateTime(),
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
