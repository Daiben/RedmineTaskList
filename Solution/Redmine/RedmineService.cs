using System;
using System.Linq;
using System.Net;

namespace Redmine
{
    public class RedmineService
    {
        public IWebProxy Proxy { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        public Uri BaseUri { get; set; }

        public string BaseUriString
        {
            get { return BaseUri.ToString(); }
            set { BaseUri = new Uri(value); }
        }


        public RedmineService()
        {
            Proxy = WebRequest.DefaultWebProxy;
        }

        public RedmineIssue[] GetIssues(string query="assigned_to_id=me")
        {
            var xml = GetXml(String.Concat("issues.xml?", query));
            
            return RedmineXmlParser.ParseIssues(xml);
        }

        public RedmineProject[] GetProjects()
        {
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