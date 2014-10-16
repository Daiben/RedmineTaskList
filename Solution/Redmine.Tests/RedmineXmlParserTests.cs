using System;
using NUnit.Framework;

namespace Redmine.Tests
{
    [TestFixture]
    class RedmineXmlParserTests
    {
        [Test]
        public void ParseHeader_Users()
        {
            var xml = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><users total_count=\"456\" offset=\"50\" limit=\"1\" type=\"array\"><user><id>1</id><login>lemon</login><firstname>Dmitry</firstname><lastname>Popov</lastname><mail>lemon@yandex.ru</mail><created_on>2013-06-13T21:30:03Z</created_on><last_login_on>2013-06-13T22:15:54Z</last_login_on></user></users>";
            
            var header = RedmineXmlParser.ParseHeader(xml);

            Assert.AreEqual(456, header.Count);
            Assert.AreEqual(50, header.Offset);
            Assert.AreEqual(1, header.Limit);
        }

        [Test]
        public void ParseUsers()
        {
            var xml = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><users total_count=\"1\" offset=\"0\" limit=\"25\" type=\"array\"><user><id>1</id><login>lemon</login><firstname>Dmitry</firstname><lastname>Popov</lastname><mail>lemon@yandex.ru</mail><created_on>2013-06-13T21:30:03Z</created_on><last_login_on>2013-06-13T22:15:54Z</last_login_on></user></users>";

            var users = RedmineXmlParser.ParseUsers(xml);
            var user = users[0];

            Assert.AreEqual(1, users.Length);
            Assert.AreEqual(1, user.Id);
            Assert.AreEqual("lemon", user.Login);
            Assert.AreEqual("Dmitry", user.FirstName);
            Assert.AreEqual("Popov", user.LastName);
            Assert.AreEqual("lemon@yandex.ru", user.Email);
            Assert.AreEqual(new DateTime(2013, 6, 13, 21, 30, 03, DateTimeKind.Utc), user.CreationTime.ToUniversalTime());
            Assert.AreEqual(new DateTime(2013, 6, 13, 22, 15, 54, DateTimeKind.Utc), user.LastLoginTime.ToUniversalTime());
        }
        
        [Test]
        public void ParseUsers_MissingLastLoginTime()
        {
            var xml = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><users total_count=\"1\" offset=\"0\" limit=\"25\" type=\"array\"><user><id>1</id><login>lemon</login><firstname>Dmitry</firstname><lastname>Popov</lastname><mail>lemon@yandex.ru</mail><created_on>2013-06-13T21:30:03Z</created_on><last_login_on/></user></users>";
            
            var user = RedmineXmlParser.ParseUsers(xml)[0];
            
            Assert.AreEqual(default(DateTime), user.LastLoginTime);
        }

        [Test]
        public void ParseProjects()
        {
            var xml = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><projects total_count=\"1\" offset=\"0\" limit=\"25\" type=\"array\"><project><id>1</id><name>VS Redmine</name><identifier>vsredmine</identifier><description>Some text</description><created_on>2012-10-20T11:10:15Z</created_on><updated_on>2012-12-29T08:18:15Z</updated_on></project></projects>";
            
            var projects = RedmineXmlParser.ParseProjects(xml);
            var project = projects[0];
            
            Assert.AreEqual(1, projects.Length);
            Assert.AreEqual(1, project.Id);
            Assert.AreEqual("VS Redmine", project.Name);
            Assert.AreEqual("vsredmine", project.Identifier);
            Assert.AreEqual("Some text", project.Description);
            Assert.AreEqual(new DateTime(2012, 10, 20, 11, 10, 15, DateTimeKind.Utc), project.CreationTime.ToUniversalTime());
            Assert.AreEqual(new DateTime(2012, 12, 29, 8, 18, 15, DateTimeKind.Utc), project.LastUpdateTime.ToUniversalTime());
        }

        [Test]
        public void ParseProjects_MissingDescription()
        {
            var xml = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><projects total_count=\"1\" offset=\"0\" limit=\"25\" type=\"array\"><project><id>1</id><name>VS Redmine</name><identifier>vsredmine</identifier><description/><created_on>2012-10-20T11:10:15Z</created_on><updated_on>2012-12-29T08:18:15Z</updated_on></project></projects>";
            
            var projects = RedmineXmlParser.ParseProjects(xml);
            var project = projects[0];
            
            Assert.AreEqual("", project.Description);
        }

        [Test]
        public void ParseProjects_ParentId()
        {
            var xml = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><projects total_count=\"2\" offset=\"1\" limit=\"1\" type=\"array\"><project><id>2</id><name>Redmine API Library</name><identifier>redmineapi</identifier><description/><parent id=\"1\" name=\"VS Redmine\"/><created_on>2012-10-20T11:10:15Z</created_on><updated_on>2012-12-29T08:18:15Z</updated_on></project></projects>";
            
            var projects = RedmineXmlParser.ParseProjects(xml);
            var project = projects[0];
            
            Assert.AreEqual(1, project.ParentId);
            Assert.AreEqual("VS Redmine", project.ParentName);
        }

        [Test]
        public void ParseIssues()
        {
            var xml = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><issues total_count=\"1\" offset=\"0\" limit=\"25\" type=\"array\"><issue><id>1</id><project id=\"2\" name=\"Redmine API Library\"/><tracker id=\"2\" name=\"Feature\"/><status id=\"3\" name=\"Resolved\"/><priority id=\"2\" name=\"Normal\"/><author id=\"1\" name=\"Dmitry Popov\"/><assigned_to id=\"1\" name=\"Dmitry Popov\"/><subject>Parse Redmine API XML</subject><description>Users, projects and issues</description><start_date>2013-06-13</start_date><due_date>2013-06-14</due_date><done_ratio>100</done_ratio><estimated_hours>2</estimated_hours><created_on>2013-06-13T22:10:24Z</created_on><updated_on>2013-06-13T22:10:24Z</updated_on><closed_on>2013-06-14T00:15:03Z</closed_on></issue></issues>";

            var issues = RedmineXmlParser.ParseIssues(xml);
            var issue = issues[0];

            Assert.AreEqual(1, issues.Length);
            Assert.AreEqual(1, issue.Id);
            Assert.AreEqual(2, issue.ProjectId);
            Assert.AreEqual("Redmine API Library", issue.ProjectName);
            Assert.AreEqual(2, issue.TrackerId);
            Assert.AreEqual("Feature", issue.TrackerName);
            Assert.AreEqual(3, issue.StatusId);
            Assert.AreEqual("Resolved", issue.StatusName);
            Assert.AreEqual(2, issue.PriorityId);
            Assert.AreEqual("Normal", issue.PriorityName);
            Assert.AreEqual(1, issue.AuthorId);
            Assert.AreEqual("Dmitry Popov", issue.AuthorName);
            Assert.AreEqual(1, issue.AssigneeId);
            Assert.AreEqual("Dmitry Popov", issue.AssigneeName);
            Assert.AreEqual("Parse Redmine API XML", issue.Subject);
            Assert.AreEqual("Users, projects and issues", issue.Description);
            Assert.AreEqual(new DateTime(2013, 6, 13), issue.StartDate);
            Assert.AreEqual(new DateTime(2013, 6, 14), issue.DueDate);
            Assert.AreEqual(100, issue.DoneRatio);
            Assert.AreEqual(2, issue.EstimatedHours);
            Assert.AreEqual(new DateTime(2013, 6, 13, 22, 10, 24, DateTimeKind.Utc), issue.CreationTime.ToUniversalTime());
            Assert.AreEqual(new DateTime(2013, 6, 13, 22, 10, 24, DateTimeKind.Utc), issue.LastUpdateTime.ToUniversalTime());
            Assert.AreEqual(new DateTime(2013, 6, 14, 00, 15, 03, DateTimeKind.Utc), issue.ClosingTime.ToUniversalTime());
        }

        [Test]
        public void ParseIssues_DateTimeFormats()
        {
            var xml = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><issues total_count=\"5\" offset=\"0\" limit=\"3\" type=\"array\"><issue><id>1</id><project id=\"1\" name=\"Project\"/><tracker id=\"1\" name=\"Bug\"/><status id=\"1\" name=\"New\"/><priority id=\"2\" name=\"Normal\"/><author id=\"1\" name=\"User\"/><assigned_to id=\"1\" name=\"User\"/><subject>ISO 8601 DateTime Format</subject><description></description><start_date></start_date><done_ratio></done_ratio><estimated_hours></estimated_hours><created_on>2014-10-16T21:51:00Z</created_on><updated_on></updated_on><closed_on></closed_on></issue><issue><id>2</id><project id=\"1\" name=\"Project\"/><tracker id=\"1\" name=\"Bug\"/><status id=\"1\" name=\"New\"/><priority id=\"2\" name=\"Normal\"/><author id=\"1\" name=\"User\"/><assigned_to id=\"1\" name=\"User\"/><subject>Weird UTC Format 1</subject><description></description><start_date></start_date><done_ratio></done_ratio><estimated_hours></estimated_hours><created_on>2014-10-16 21:51:01.000 UTC</created_on><updated_on></updated_on><closed_on></closed_on></issue><issue><id>3</id><project id=\"1\" name=\"Project\"/><tracker id=\"1\" name=\"Bug\"/><status id=\"1\" name=\"New\"/><priority id=\"2\" name=\"Normal\"/><author id=\"1\" name=\"User\"/><assigned_to id=\"1\" name=\"User\"/><subject>Weird UTC Format 2</subject><description></description><start_date></start_date><done_ratio></done_ratio><estimated_hours></estimated_hours><created_on>2014-10-16 21:51:02 UTC</created_on><updated_on></updated_on><closed_on></closed_on></issue></issues>";

            var issues = RedmineXmlParser.ParseIssues(xml);
            
            Assert.AreEqual(new DateTime(2014, 10, 16, 21, 51, 00, DateTimeKind.Utc), issues[0].CreationTime.ToUniversalTime());
            Assert.AreEqual(new DateTime(2014, 10, 16, 21, 51, 01, DateTimeKind.Utc), issues[1].CreationTime.ToUniversalTime());
            Assert.AreEqual(new DateTime(2014, 10, 16, 21, 51, 02, DateTimeKind.Utc), issues[2].CreationTime.ToUniversalTime());
        }

        [Test]
        public void ParseIssues_UnknownDateTimeFormat()
        {
            var xml = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><issues total_count=\"1\" offset=\"0\" limit=\"25\" type=\"array\"><issue><id>1</id><project id=\"1\" name=\"Project\"/><tracker id=\"1\" name=\"Bug\"/><status id=\"1\" name=\"New\"/><priority id=\"2\" name=\"Normal\"/><author id=\"1\" name=\"User\"/><assigned_to id=\"1\" name=\"User\"/><subject>Unknown DateTime Format</subject><description></description><start_date></start_date><done_ratio></done_ratio><estimated_hours></estimated_hours><created_on>16.10.2014 22:35:00</created_on><updated_on></updated_on><closed_on></closed_on></issue></issues>";

            var issues = RedmineXmlParser.ParseIssues(xml);
            
            Assert.AreEqual(default(DateTime), issues[0].CreationTime);
        }

        [Test]
        public void ParseIssues_InvalidId()
        {
            var xml = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><issues total_count=\"1\" offset=\"0\" limit=\"25\" type=\"array\"><issue><id>1</id><project id=\"I\" name=\"Project\"/><tracker id=\"1\" name=\"Bug\"/><status id=\"1\" name=\"New\"/><priority id=\"2\" name=\"Normal\"/><author id=\"1\" name=\"User\"/><assigned_to id=\"1\" name=\"User\"/><subject>Invalid Int32 Format</subject><description></description><start_date></start_date><done_ratio></done_ratio><estimated_hours></estimated_hours><created_on>2014-10-16T22:35:00Z</created_on><updated_on></updated_on><closed_on></closed_on></issue></issues>";

            var issues = RedmineXmlParser.ParseIssues(xml);
            
            Assert.AreEqual(0, issues[0].ProjectId);
        }

        [Test]
        public void ParseIssues_InvalidEstimatedHours()
        {
            var xml = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><issues total_count=\"1\" offset=\"0\" limit=\"25\" type=\"array\"><issue><id>1</id><project id=\"I\" name=\"Project\"/><tracker id=\"1\" name=\"Bug\"/><status id=\"1\" name=\"New\"/><priority id=\"2\" name=\"Normal\"/><author id=\"1\" name=\"User\"/><assigned_to id=\"1\" name=\"User\"/><subject>Invalid Double Format</subject><description></description><start_date></start_date><done_ratio></done_ratio><estimated_hours>1,5</estimated_hours><created_on>2014-10-16T22:35:00Z</created_on><updated_on></updated_on><closed_on></closed_on></issue></issues>";

            var issues = RedmineXmlParser.ParseIssues(xml);
            
            Assert.AreEqual(0, issues[0].EstimatedHours);
        }

        [Test]
        public void ParseIssues_MissingAssigneeEtc()
        {
            var xml = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><issues total_count=\"1\" offset=\"0\" limit=\"25\" type=\"array\"><issue><id>1</id><project id=\"2\" name=\"Redmine API Library\"/><tracker id=\"2\" name=\"Feature\"/><status id=\"3\" name=\"Resolved\"/><priority id=\"2\" name=\"Normal\"/><author id=\"1\" name=\"Dmitry Popov\"/><subject>Parse Redmine API XML</subject><description>Users, projects and issues</description><start_date></start_date><due_date></due_date><done_ratio></done_ratio><estimated_hours></estimated_hours><created_on>2013-06-13T22:10:24Z</created_on><updated_on>2013-06-13T22:10:24Z</updated_on><closed_on></closed_on></issue></issues>";

            var issue = RedmineXmlParser.ParseIssues(xml)[0];
            
            Assert.AreEqual(0, issue.AssigneeId);
            Assert.AreEqual("", issue.AssigneeName);
            Assert.AreEqual(0, issue.EstimatedHours);
            Assert.AreEqual(0, issue.DoneRatio);
            Assert.AreEqual(default(DateTime), issue.StartDate);
            Assert.AreEqual(default(DateTime), issue.DueDate);
            Assert.AreEqual(default(DateTime), issue.ClosingTime);
        }
        

        [Test]
        public void ParseIssues_EstimatedHours()
        {
            var xml = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><issues total_count=\"1\" offset=\"0\" limit=\"25\" type=\"array\"><issue><id>1</id><project id=\"2\" name=\"Redmine API Library\"/><tracker id=\"2\" name=\"Feature\"/><status id=\"3\" name=\"Resolved\"/><priority id=\"2\" name=\"Normal\"/><author id=\"1\" name=\"Dmitry Popov\"/><subject>Parse Redmine API XML</subject><description>Users, projects and issues</description><start_date></start_date><due_date></due_date><done_ratio></done_ratio><estimated_hours>1.5</estimated_hours><created_on>2013-06-13T22:10:24Z</created_on><updated_on>2013-06-13T22:10:24Z</updated_on><closed_on></closed_on></issue></issues>";

            var issue = RedmineXmlParser.ParseIssues(xml)[0];
            
            Assert.AreEqual(0, issue.AssigneeId);
            Assert.AreEqual(1.5, issue.EstimatedHours);
        }


        [Test]
        public void ParseIssues_WithJournals()
        {
            var xml = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><issue><id>1</id><project id=\"1\" name=\"Blah blah\"/><tracker id=\"1\" name=\"Bug\"/><status id=\"2\" name=\"In Progress\"/><priority id=\"2\" name=\"Normal\"/><author id=\"1\" name=\"Who Cares\"/><assigned_to id=\"1\" name=\"Who Cares\"/><subject>Parse journals</subject><description/><start_date/><due_date/><done_ratio>0</done_ratio><estimated_hours/><created_on>2014-10-03T09:45:20Z</created_on><updated_on>2014-10-03T11:34:02Z</updated_on><closed_on/><attachments type=\"array\"/><journals type=\"array\"><journal id=\"1\"><user id=\"1\" name=\"Who Cares\"/><notes>Now that's important.</notes><created_on>2014-10-03T11:34:02Z</created_on><details type=\"array\"><detail property=\"attr\" name=\"status_id\"><old_value>1</old_value><new_value>2</new_value></detail></details></journal></journals></issue>";

            var issue = RedmineXmlParser.ParseIssues(xml)[0];

            Assert.IsNotNull(issue.Journals);
            Assert.AreEqual(1, issue.Journals.Length);

            var journal = issue.Journals[0];

            Assert.AreEqual(1, journal.Id);
            Assert.AreEqual(1, journal.AuthorId);
            Assert.AreEqual("Who Cares", journal.AuthorName);
            Assert.AreEqual("Now that's important.", journal.Notes);
            Assert.AreEqual(new DateTime(2014, 10, 3, 11, 34, 02, DateTimeKind.Utc), journal.CreationTime.ToUniversalTime());

            Assert.IsNotNull(journal.Details);
            Assert.AreEqual(1, journal.Details.Length);

            var detail = journal.Details[0];

            Assert.AreEqual("attr", detail.Property);
            Assert.AreEqual("status_id", detail.Name);
            Assert.AreEqual("1", detail.OldValue);
            Assert.AreEqual("2", detail.NewValue);
        }
    }
}
