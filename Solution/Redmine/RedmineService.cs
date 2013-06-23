using System;
using System.Linq;

namespace Redmine
{
    public static class RedmineService
    {
        public static RedmineIssue[] GetIssues(string username, string password, string baseUriString, string query="assigned_to_id=me")
        {
            var baseUri = new Uri(baseUriString);

            var xml = GetXml(username, password, baseUri, String.Concat("issues.xml?", query));
            
            return RedmineXmlParser.ParseIssues(xml);
        }

        public static RedmineProject[] GetProjects(string username, string password, string baseUriString)
        {
            var baseUri = new Uri(baseUriString);

            return GetProjects(username, password, baseUri);
        }

        private static RedmineProject[] GetProjects(string username, string password, Uri baseUri)
        {
            var count = 1;
            var offset = 0;
            var projects = new RedmineProject[0].AsEnumerable();

            while (offset < count)
            {
                var xml = GetXml(username, password, baseUri, "projects.xml", offset);
                var header = RedmineXmlParser.ParseHeader(xml);

                projects = projects.Concat(RedmineXmlParser.ParseProjects(xml));

                offset = header.Limit + header.Offset;
                count = header.Count;
            }

            return projects.ToArray();
        }

        private static string GetXml(string username, string password, Uri baseUri, string path, int offset = 0)
        {
            var uri = new Uri(baseUri, offset == 0 ? path : String.Concat(path, "?offset=", offset));

            var request = new RedmineWebRequest(username, password, uri);
            
            return request.GetResponse();
        }
    }
}