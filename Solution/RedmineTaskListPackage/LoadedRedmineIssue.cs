using Redmine;

namespace RedmineTaskListPackage
{
    public class LoadedRedmineIssue : RedmineIssue
    {
        public ConnectionSettings ConnectionSettings { get; set; }

        public static LoadedRedmineIssue Create(RedmineIssue issue)
        {
            return new LoadedRedmineIssue
            {
                Url = issue.Url,
                Id = issue.Id,
                ProjectId = issue.ProjectId,
                ProjectName = issue.ProjectName,
                TrackerId = issue.TrackerId,
                TrackerName = issue.TrackerName,
                StatusId = issue.StatusId,
                StatusName = issue.StatusName,
                PriorityId = issue.PriorityId,
                PriorityName = issue.PriorityName,
                AuthorId = issue.AuthorId,
                AuthorName = issue.AuthorName,
                AssigneeId = issue.AssigneeId,
                AssigneeName = issue.AssigneeName,
                Subject = issue.Subject,
                Description = issue.Description,
                StartDate = issue.StartDate,
                DueDate = issue.DueDate,
                DoneRatio = issue.DoneRatio,
                EstimatedHours = issue.EstimatedHours,
                CreationTime = issue.CreationTime,
                LastUpdateTime = issue.LastUpdateTime,
                ClosingTime = issue.ClosingTime,
                Journals = issue.Journals,
            };
        }
    }
}