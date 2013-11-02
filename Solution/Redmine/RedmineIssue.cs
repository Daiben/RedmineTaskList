using System;

namespace Redmine
{
    public class RedmineIssue
    {
        public string Url { get; set; }
        
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public string ProjectName { get; set; }
        public int TrackerId { get; set; }
        public string TrackerName { get; set; }
        public int StatusId { get; set; }
        public string StatusName { get; set; }
        public int PriorityId { get; set; }
        public string PriorityName { get; set; }
        public int AuthorId { get; set; }
        public string AuthorName { get; set; }
        public int AssigneeId { get; set; }
        public string AssigneeName { get; set; }
        public string Subject { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime DueDate { get; set; }
        public int DoneRatio { get; set; }
        public double EstimatedHours { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime LastUpdateTime { get; set; }
        public DateTime ClosingTime { get; set; }
    }
}