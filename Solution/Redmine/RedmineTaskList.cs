﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Redmine
{
    public static class RedmineTaskList
    {
        private static Dictionary<string, int> UserCache;

        static RedmineTaskList()
        {
            UserCache = new Dictionary<string, int>();
        }

        public static void ClearUserCache()
        {
            UserCache.Clear();
        }

        public static RedmineIssue[] Get(string username, string password, string baseUriString, string query="assigned_to_id={0}")
        {
            var baseUri = new Uri(baseUriString);
            var userId = FindUserId(username, password, baseUri);

            var path = String.Concat("issues.xml?", String.Format(query, userId));
            var uri = new Uri(baseUri, path);
            var xml = new RedmineWebRequest(username, password, uri).GetResponse();
            
            return RedmineXmlParser.ParseIssues(xml);
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
            var path = "users.xml";

            var user = FindUser(username, password, baseUri, path);
            
            if (user != null)
            {
                UserCache.Add(cacheKey, user.Id);
            }

            return GetCachedUser(cacheKey);
        }

        private static RedmineUser FindUser(string username, string password, Uri baseUri, string path)
        {
            var count = 1;
            var offset = 0;
            var user = default(RedmineUser);

            while (user == null && offset < count)
            {
                var uri = new Uri(baseUri, offset == 0 ? path : String.Concat(path, "?offset=", offset));
                var xml = new RedmineWebRequest(username, password, uri).GetResponse();
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
    }
}