using System;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.Shell;
using Redmine;

namespace RedmineTaskListPackage
{
    public class RedmineTask : Task
    {
        public RedmineTask(RedmineIssue issue, string format)
        {
            CanDelete = false;
            Checked = false;

            IsCheckedEditable = false;
            IsPriorityEditable = false;
            IsTextEditable = false;

            ImageIndex = 2;

            Category = TaskCategory.Misc;
            Document = issue.ProjectName;
            Line = issue.Id;
            Priority = (TaskPriority)Math.Max(3 - issue.PriorityId, 0);

            Text = String.Format
               (Regex.Unescape(format),
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
                issue.ClosingTime);
        }
    }
}
