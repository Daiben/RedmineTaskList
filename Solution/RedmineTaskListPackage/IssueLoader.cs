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
        private Dictionary<ConnectionSettings, RedmineIssue[]> _issuesBySource;
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

        public static RedmineService GetRedmineService(ConnectionSettings settings, IWebProxy proxy)
        {
            return new RedmineService
            {
                BaseUriString = settings.URL,
                Username = settings.Username,
                Password = settings.Password,
                Proxy = proxy,
            };
        }

        public LoadedRedmineIssue[] LoadIssues(IList<ConnectionSettings> settings)
        {
            _issuesBySource = new Dictionary<ConnectionSettings, RedmineIssue[]>();

            Parallel.ForEach(settings.Where(IsValid), GetIssues);

            return _issuesBySource.OrderBy(x => settings.IndexOf(x.Key))
                .SelectMany(AsLoaded)
                .Distinct(new IssueComparer())
                .ToArray();
        }

        private static LoadedRedmineIssue[] AsLoaded(KeyValuePair<ConnectionSettings, RedmineIssue[]> source)
        {
            var issues = source.Value.Select(LoadedRedmineIssue.Create).ToArray();

            foreach (var issue in issues)
            {
                issue.ConnectionSettings = source.Key;
            }

            return issues.ToArray();
        }

        private bool IsValid(ConnectionSettings settings)
        {
            return !String.IsNullOrEmpty(settings.URL);
        }

        private void GetIssues(ConnectionSettings settings)
        {
            var issues = new RedmineIssue[0];
            var redmine = GetRedmineService(settings, _proxy);

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
                _issuesBySource.Add(settings, issues);
            }
        }


        private class IssueComparer : IEqualityComparer<LoadedRedmineIssue>
        {
            public bool Equals(LoadedRedmineIssue x, LoadedRedmineIssue y)
            {
                if (Object.ReferenceEquals(x, y))
                {
                    return true;
                }

                if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                {
                    return false;
                }

                return GetHashCode(x) == GetHashCode(y);
            }

            public int GetHashCode(LoadedRedmineIssue issue)
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