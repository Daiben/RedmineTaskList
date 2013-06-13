﻿using System;
using System.IO;
using System.Net;
using System.Text;

namespace Redmine
{
    public class RedmineWebRequest
    {
        private WebRequest _request;

        public RedmineWebRequest(string username, string password, string requestUriString)
        {
            Initialize(username, password, new Uri(requestUriString));
        }

        public RedmineWebRequest(string username, string password, Uri requestUri)
        {
            Initialize(username, password, requestUri);
        }

        private void Initialize(string username, string password, Uri requestUri)
        {
            var authString = String.Concat(username, ':', password);

            _request = WebRequest.Create(requestUri);
            _request.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes(authString)));
        }

        public string GetResponse()
        {
            WebResponse response = _request.GetResponse();
            
            using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
            {
                return reader.ReadToEnd();
            }
        }
    }
}