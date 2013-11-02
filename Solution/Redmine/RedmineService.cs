using System;
using System.Linq;
using System.Net;

namespace Redmine
{
    public class RedmineService
    {
        private Uri _baseUri;

        public IWebProxy Proxy { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        public Uri BaseUri
        {
            get { return _baseUri; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("BaseUri");
                }

                _baseUri = value;
            }
        }

        public string BaseUriString
        {
            get { return BaseUri != null ? BaseUri.ToString() : null; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("BaseUriString");
                }

                if (value == "")
                {
                    throw new ArgumentException("Can't set BaseUriString to an empty string");
                }

                BaseUri = new Uri(value.Last() == '/' ? value : value + '/');
            }
        }


        public RedmineService()
        {
            Proxy = WebRequest.DefaultWebProxy;
        }

        public RedmineIssue[] GetIssues(string query="assigned_to_id=me")
        {
            if (BaseUri == null)
            {
                throw new InvalidOperationException("BaseUri is not set");
            }

            var xml = GetXml(String.Concat("issues.xml?", query));
            var issues = RedmineXmlParser.ParseIssues(xml);

            foreach (var issue in issues)
            {
                issue.Url = GetIssueUrl(issue);
            }
            
            return issues;
        }

        private string GetIssueUrl(RedmineIssue issue)
        {
            var baseUri = BaseUriString;
            var index = baseUri.IndexOf("projects");

            if (index != -1)
            {
                baseUri = baseUri.Substring(0, index);
            }

            return String.Concat(baseUri, "issues/", issue.Id).ToString();
        }

        public RedmineProject[] GetProjects()
        {
            if (BaseUri == null)
            {
                throw new InvalidOperationException("BaseUri is not set");
            }

            var count = 1;
            var offset = 0;
            var projects = new RedmineProject[0].AsEnumerable();

            while (offset < count)
            {
                var xml = GetXml("projects.xml", offset);
                var header = RedmineXmlParser.ParseHeader(xml);

                projects = projects.Concat(RedmineXmlParser.ParseProjects(xml));

                offset = header.Limit + header.Offset;
                count = header.Count;
            }

            return projects.ToArray();
        }

        private string GetXml(string path, int offset = 0)
        {
            var uri = new Uri(BaseUri, offset == 0 ? path : String.Concat(path, "?offset=", offset));

            var request = new RedmineWebRequest(Username, Password, uri, Proxy);
            
            return request.GetResponse();
        }
    }
}