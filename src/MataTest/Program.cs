//MataSharp Showcase/Test Program.
using System;
using System.Linq;
using System.Collections.Generic;
using MataSharp;

namespace MataTest
{
    class Program
    {
        static void Main(string[] args)
        {
            #region MagisterSchool
            Console.WriteLine("Typ gedeelte van je school in: ");
            var schools = MagisterSchool.GetSchools(Console.ReadLine());

            for (int i = 0; i < schools.Count; i++)
                Console.WriteLine(i + ": " + schools[i].Name + "   " + schools[i].URL);

            MagisterSchool school = schools[Convert.ToInt32(Console.ReadLine())];
            #endregion

            Console.Write("UserName: "); var userName = Console.ReadLine();
            Console.Write("Password: "); var password = Console.ReadLine();

            #region Mata
            using (Mata mata = new Mata(school, userName, password))
            {
                #region GeneralInformation
                Console.WriteLine("User's Name: " + mata.Name);
                Console.WriteLine("User's ID: " + mata.UserID);
                #endregion

                //Let's pull 20 messages already!
                //WARNING ALWAYS USE .Take(int). OR ELSE YOU WILL PICK UP 750 MESSAGES!
                var twentyMessages = mata.Inbox.Messages.Take(20);

                twentyMessages.First(m => m.Attachments.Count != 0).Attachments[0].Download(true);

                var utilitiesTest = mata.GetDigitalSchoolUtilities();

                var homeWork = mata.GetHomework();
                foreach (var homework in homeWork)
                    Console.WriteLine(homework.Start.DayOfWeek + " " + homework.BeginBySchoolHour + " " + homework.ClassAbbreviation + "   " + homework.Content + "\n");

                var studyGuides = mata.GetStudyGuides();

                var assignmentTest = mata.GetAssignments().Where(x => x.Versions.Any(y => y.HandedInAttachments.Count != 0) && x.Attachments.Count != 0); //Because we all love linq so much
                foreach (var assignment in assignmentTest)
                {
                    Console.WriteLine(assignment.Description);
                    assignment.Attachments.ForEach(a => a.Download(true));
                }
                assignmentTest.ElementAt(0).Attachments.ForEach(a => a.Download(true));

                new MagisterMessage()
                {
                    Subject = "Hallotjees :D",
                    Body = "TESSST D:",
                    Recipients = new List<MagisterPerson>() { mata.Person },
                    CC = new List<MagisterPerson>() { mata.Person },
                }.Send();

                #region Message
                var Inbox = mata.Inbox;

                var allUnreadMessages = Inbox.Messages.WhereUnread();

                //Console.WriteLine("Last unread message in inbox: " + allUnreadMessages[0].Content);
                Console.WriteLine("Unread Messages in inbox: " + allUnreadMessages.Count);

                MagisterMessage msg = Inbox.Messages.First(m => m.Attachments.Count() != 0); //Take first message with at least 1 attachment. :)
                Console.WriteLine("First message in inbox with at least 1 attachment: " + msg.Body);
                Console.WriteLine("It's attachment count: " + msg.Attachments.Count());
                Console.WriteLine("");

                #region Attachments
                foreach (Attachment attachment in msg.Attachments)
                {
                    Console.WriteLine("Attachment's name: " + attachment.Name + ", MIME Type: " + attachment.MIME);
                    attachment.Download(true); //Download the attachment and add the UserID infront of the file name.
                }
                #endregion

                #region ForwardMessage
                var ForwardMSG = msg.CreateForwardMessage();
                ForwardMSG.Recipients = new List<MagisterPerson>() { mata.Person };
                ForwardMSG.Send();
                #endregion
                #endregion
            }
            #endregion
            Console.ReadLine();
        }
    }
}
