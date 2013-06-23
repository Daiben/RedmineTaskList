using System;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.Shell;
using Redmine;

namespace RedmineTaskListPackage
{
    public class RedmineTask : Task
    {
        private string issueUrl;
        private RedmineIssue _issue;
        private IRedmineIssueViewer _viewer;

        public RedmineTask(PackageOptions options, RedmineIssue issue, IRedmineIssueViewer viewer)
        {
            _issue = issue;
            _viewer = viewer;
            issueUrl = GetIssueUrl(options.URL, issue.Id);

            CanDelete = false;
            Checked = false;

            IsCheckedEditable = false;
            IsPriorityEditable = false;
            IsTextEditable = false;

            ImageIndex = 2;

            Category = TaskCategory.Misc;
            Priority = (TaskPriority)Math.Max(3 - issue.PriorityId, 0);

            Text = String.Format(
                Regex.Unescape(options.TaskDescriptionFormat),
                issue.Id,
                issue.ProjectId,
                issue.ProjectName,
                issue.TrackerId,
                issue.TrackerName,
                issue.StatusId,
                issue.StatusName,
                issue.PriorityId,
                issue.PriorityName,
                issue.AuthorId,
                issue.AuthorName,
                issue.AssigneeId,
                issue.AssigneeName,
                issue.Subject,
                issue.Description,
                issue.StartDate,
                issue.DueDate,
                issue.DoneRatio,
                issue.EstimatedHours,
                issue.CreationTime,
                issue.LastUpdateTime,
                issue.ClosingTime
             );
        }

        protected override void OnNavigate(EventArgs e)
        {
            base.OnNavigate(e);

            _viewer.Show(_issue);
        }

        private static string GetIssueUrl(string baseUriString, int issueId)
        {
            var baseUri = new Uri(baseUriString);
            var uri = new Uri(baseUri, String.Concat("issues/", issueId));
            
            return uri.ToString();
        }
    }
}
