using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Redmine;

namespace RedmineTaskListPackage
{
    public class IssueLoader
    {
        private object syncRoot;
        private Dictionary<ConnectionSettings, RedmineIssue[]> _issues;
        private IWebProxy _proxy;

        public IWebProxy Proxy
        {
            get { return _proxy; }
            set { _proxy = value; }
        }

        public IDebug Debug { get; set; }
        

        public IssueLoader()
        {
            syncRoot = new object();
        }


        public RedmineIssue[] LoadIssues(IList<ConnectionSettings> settings)
        {
            _issues = new Dictionary<ConnectionSettings, RedmineIssue[]>();

            Parallel.ForEach(settings, GetIssues);

            return _issues.OrderBy(x => settings.IndexOf(x.Key))
                .SelectMany(x => x.Value)
                .Distinct(new IssueComparer())
                .ToArray();
        }

        private void GetIssues(ConnectionSettings settings)
        {
            var issues = new RedmineIssue[0];

            var redmine = new RedmineService
            {
                BaseUriString = settings.URL,
                Username = settings.Username,
                Password = settings.Password,
                Proxy = _proxy,
            };

            try
            {
                issues = redmine.GetIssues(settings.Query);
            }
            catch (Exception e)
            {
                if (Debug != null)
                {
                    Debug.WriteLine(String.Concat("Username: ", settings.Username, "; URL: ", redmine.BaseUriString, settings.Query));
                    Debug.WriteLine(e.ToString());
                }
            }

            lock (syncRoot)
            {
                _issues.Add(settings, issues);
            }
        }


        private class IssueComparer : IEqualityComparer<RedmineIssue>
        {
            public bool Equals(RedmineIssue x, RedmineIssue y)
            {
                if (Object.ReferenceEquals(x, y))
                {
                    return true;
                }

                if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                {
                    return false;
                }

                return x.Url == y.Url;
            }

            public int GetHashCode(RedmineIssue issue)
            {
                if (issue != null && issue.Url != null)
                {
                    return issue.Url.GetHashCode();
                }

                if (issue != null)
                {
                    return issue.GetHashCode();
                }

                return 0;
            }
        }
    }
}