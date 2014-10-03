using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

namespace Redmine
{
    public class RedmineXmlParser
    {
        public static RedmineResponseHeader ParseHeader(string xml)
        {
            var document = XDocument.Parse(xml);

            return new RedmineResponseHeader
            {
                Count = ParseInt32(document.Root.Attribute("total_count").Value),
                Offset = ParseInt32(document.Root.Attribute("offset").Value),
                Limit = ParseInt32(document.Root.Attribute("limit").Value),
            };
        }

        public static RedmineUser[] ParseUsers(string xml)
        {
            var users = new List<RedmineUser>();
            var document = XDocument.Parse(xml);
            
            foreach (var element in document.Descendants("user"))
            {
                users.Add(new RedmineUser
                {
                    Id = GetInt32(element, "id"),

                    Login = GetString(element, "login"),
                    FirstName = GetString(element, "firstname"),
                    LastName = GetString(element, "lastname"),
                    Email = GetString(element, "mail"),
                    
                    CreationTime = GetDateTime(element, "created_on"),
                    LastLoginTime = GetDateTime(element, "last_login_on"),
                });
            }

            return users.ToArray();
        }

        public static RedmineProject[] ParseProjects(string xml)
        {
            var projects = new List<RedmineProject>();
            var document = XDocument.Parse(xml);
            
            foreach (var element in document.Descendants("project"))
            {
                projects.Add(new RedmineProject
                {
                    Id = GetInt32(element, "id"),

                    Name = GetString(element, "name"),
                    Identifier = GetString(element, "identifier"),
                    Description = GetString(element, "description"),

                    CreationTime = GetDateTime(element, "created_on"),
                    LastUpdateTime = GetDateTime(element, "updated_on"),

                    ParentId = GetInt32(element, "parent", "id"),
                    ParentName = GetString(element, "parent", "name")
                });
            }

            return projects.ToArray();
        }

        public static RedmineIssue[] ParseIssues(string xml)
        {
            var issues = new List<RedmineIssue>();
            var document = XDocument.Parse(xml);
            
            foreach (var element in document.Descendants("issue"))
            {
                issues.Add(new RedmineIssue
                {
                    Id = GetInt32(element, "id"),
                    
                    ProjectId = GetInt32(element, "project", "id"),
                    TrackerId = GetInt32(element, "tracker", "id"),
                    StatusId = GetInt32(element, "status", "id"),
                    PriorityId = GetInt32(element, "priority", "id"),
                    AuthorId = GetInt32(element, "author", "id"),
                    AssigneeId = GetInt32(element, "assigned_to", "id"),
                                        
                    ProjectName = GetString(element, "project", "name"),
                    TrackerName = GetString(element, "tracker", "name"),
                    StatusName = GetString(element, "status", "name"),
                    PriorityName = GetString(element, "priority", "name"),
                    AuthorName = GetString(element, "author", "name"),
                    AssigneeName = GetString(element, "assigned_to", "name"),

                    Subject = GetString(element, "subject"),
                    Description = GetString(element, "description"),

                    StartDate = GetDateTime(element, "start_date").Date,
                    DueDate = GetDateTime(element, "due_date").Date,

                    DoneRatio = GetInt32(element, "done_ratio"),
                    EstimatedHours = GetDouble(element, "estimated_hours"),
                    
                    CreationTime = GetDateTime(element, "created_on"),
                    LastUpdateTime = GetDateTime(element, "updated_on"),
                    ClosingTime = GetDateTime(element, "closed_on"),
                });
            }

            return issues.ToArray();
        }

        private static string GetString(XElement element, string descendantName)
        {
            var descendantElement = element.Descendants(descendantName).FirstOrDefault();

            return descendantElement != null ?  descendantElement.Value : "";
        }

        private static string GetString(XElement element, string descendantName, string attributeName)
        {
            var descendantElement = element.Descendants(descendantName).FirstOrDefault();

            return descendantElement != null ?  descendantElement.Attribute(attributeName).Value : "";
        }

        private static int GetInt32(XElement element, string descendantName)
        {
            return ParseInt32(GetString(element, descendantName));
        }

        private static int GetInt32(XElement element, string descendantName, string attributeName)
        {
            return ParseInt32(GetString(element, descendantName, attributeName));
        }

        private static double GetDouble(XElement element, string descendantName)
        {
            return ParseDouble(GetString(element, descendantName));
        }

        private static DateTime GetDateTime(XElement element, string descendantName)
        {
            return ParseDateTime(GetString(element, descendantName));
        }
        

        private static int ParseInt32(string s)
        {
            return !String.IsNullOrEmpty(s) ? Int32.Parse(s, CultureInfo.InvariantCulture) : 0;
        }

        private static double ParseDouble(string s)
        {
            return !String.IsNullOrEmpty(s) ? Double.Parse(s, CultureInfo.InvariantCulture) : 0;
        }

        private static DateTime ParseDateTime(string s)
        {
            var formats = new[]
            {
                "yyyy-MM-dd",
                "yyyy-MM-ddTHH:mm:ssZ",
                "yyyy-MM-dd HH:mm:ss.fff UTC",
            };

            return !String.IsNullOrEmpty(s) ? DateTime.ParseExact(s, formats, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal) : default(DateTime);
        }
    }
}