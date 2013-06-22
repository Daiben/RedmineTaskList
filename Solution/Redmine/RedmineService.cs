using System;
using System.Collections.Generic;
using System.Linq;

namespace Redmine
{
    public static class RedmineService
    {
        private static Dictionary<string, int> UserCache;

        static RedmineService()
        {
            UserCache = new Dictionary<string, int>();
        }


        public static void ClearUserCache()
        {
            UserCache.Clear();
        }


        public static RedmineIssue[] GetIssues(string username, string password, string baseUriString, string query="assigned_to_id={0}")
        {
            var baseUri = new Uri(baseUriString);
            var userId = FindUserId(username, password, baseUri);

            var xml = GetXml(username, password, baseUri, String.Concat("issues.xml?", String.Format(query, userId)));
            
            return RedmineXmlParser.ParseIssues(xml);
        }


        public static RedmineProject[] GetProjects(string username, string password, string baseUriString)
        {
            var baseUri = new Uri(baseUriString);
            var userId = FindUserId(username, password, baseUri);

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


        private static int FindUserId(string username, string password, Uri baseUri)
        {
            var cacheKey = GetCacheKey(username, baseUri);
            var cachedUser = GetCachedUser(cacheKey);

            if (cachedUser.Key == null)
            {
                cachedUser = RequestAndCache(username, password, baseUri);
            }

            return cachedUser.Value;
        }

        private static KeyValuePair<string, int> RequestAndCache(string username, string password, Uri baseUri)
        {
            var cacheKey = GetCacheKey(username, baseUri);

            var user = FindUser(username, password, baseUri);
            
            if (user != null)
            {
                UserCache.Add(cacheKey, user.Id);
            }

            return GetCachedUser(cacheKey);
        }

        private static RedmineUser FindUser(string username, string password, Uri baseUri)
        {
            var count = 1;
            var offset = 0;
            var user = default(RedmineUser);

            while (user == null && offset < count)
            {
                var xml = GetXml(username, password, baseUri, "users.xml", offset);
                var header = RedmineXmlParser.ParseHeader(xml);
                var users = RedmineXmlParser.ParseUsers(xml);

                user = users.FirstOrDefault(x => x.Login.Equals(username));
                
                offset = header.Limit + header.Offset;
                count = header.Count;
            }

            return user;
        }
        
        private static string GetCacheKey(string username, Uri baseUri)
        {
            return String.Concat(username, '@', baseUri);
        }

        private static KeyValuePair<string, int> GetCachedUser(string cacheKey)
        {
            return UserCache.FirstOrDefault(x => x.Key.Equals(cacheKey));;
        }


        private static string GetXml(string username, string password, Uri baseUri, string path, int offset = 0)
        {
            var uri = new Uri(baseUri, offset == 0 ? path : String.Concat(path, "?offset=", offset));

            var request = new RedmineWebRequest(username, password, uri);
            
            return request.GetResponse();
        }
    }
}