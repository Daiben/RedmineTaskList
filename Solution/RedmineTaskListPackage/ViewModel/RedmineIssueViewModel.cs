using System;
using System.Linq;
using Redmine;

namespace RedmineTaskListPackage.ViewModel
{
    public class RedmineIssueViewModel : ObservableObject
    {
        private RedmineIssue _issue;
        private RedmineJournalViewModel[] _journals;

        public int AssigneeId
        {
            get { return _issue.AssigneeId; }
            set
            {
                if (_issue.AssigneeId != value)
                {
                    _issue.AssigneeId = value;
                    OnPropertyChanged("AssigneeId");
                }
            }
        }

        public string AssigneeName
        {
            get { return _issue.AssigneeName; }
            set
            {
                if (_issue.AssigneeName != value)
                {
                    _issue.AssigneeName = value;
                    OnPropertyChanged("AssigneeName");
                }
            }
        }

        public int AuthorId
        {
            get { return _issue.AuthorId; }
            set
            {
                if (_issue.AuthorId != value)
                {
                    _issue.AuthorId = value;
                    OnPropertyChanged("AuthorId");
                }
            }
        }

        public string AuthorName
        {
            get { return _issue.AuthorName; }
            set
            {
                if (_issue.AuthorName != value)
                {
                    _issue.AuthorName = value;
                    OnPropertyChanged("AuthorName");
                }
            }
        }

        public DateTime ClosingTime
        {
            get { return _issue.ClosingTime; }
            set
            {
                if (_issue.ClosingTime != value)
                {
                    _issue.ClosingTime = value;
                    OnPropertyChanged("ClosingTime");
                }
            }
        }

        public DateTime CreationTime
        {
            get { return _issue.CreationTime; }
            set
            {
                if (_issue.CreationTime != value)
                {
                    _issue.CreationTime = value;
                    OnPropertyChanged("CreationTime");
                }
            }
        }

        public string Description
        {
            get { return _issue.Description; }
            set
            {
                if (_issue.Description != value)
                {
                    _issue.Description = value;
                    OnPropertyChanged("Description");
                }
            }
        }

        public bool DescriptionVisible
        {
            get { return !String.IsNullOrWhiteSpace(_issue.Description); }
        }

        public int DoneRatio
        {
            get { return _issue.DoneRatio; }
            set
            {
                if (_issue.DoneRatio != value)
                {
                    _issue.DoneRatio = value;
                    OnPropertyChanged("DoneRatio");
                }
            }
        }

        public DateTime DueDate
        {
            get { return _issue.DueDate; }
            set
            {
                if (_issue.DueDate != value)
                {
                    _issue.DueDate = value;
                    OnPropertyChanged("DueDate");
                }
            }
        }

        public double EstimatedHours
        {
            get { return _issue.EstimatedHours; }
            set
            {
                if (_issue.EstimatedHours != value)
                {
                    _issue.EstimatedHours = value;
                    OnPropertyChanged("EstimatedHours");
                }
            }
        }

        public int Id
        {
            get { return _issue.Id; }
            set
            {
                if (_issue.Id != value)
                {
                    _issue.Id = value;
                    OnPropertyChanged("Id");
                }
            }
        }

        public DateTime LastUpdateTime
        {
            get { return _issue.LastUpdateTime; }
            set
            {
                if (_issue.LastUpdateTime != value)
                {
                    _issue.LastUpdateTime = value;
                    OnPropertyChanged("LastUpdateTime");
                }
            }
        }

        public int PriorityId
        {
            get { return _issue.PriorityId; }
            set
            {
                if (_issue.PriorityId != value)
                {
                    _issue.PriorityId = value;
                    OnPropertyChanged("PriorityId");
                }
            }
        }

        public string PriorityName
        {
            get { return _issue.PriorityName; }
            set
            {
                if (_issue.PriorityName != value)
                {
                    _issue.PriorityName = value;
                    OnPropertyChanged("PriorityName");
                }
            }
        }

        public int ProjectId
        {
            get { return _issue.ProjectId; }
            set
            {
                if (_issue.ProjectId != value)
                {
                    _issue.ProjectId = value;
                    OnPropertyChanged("ProjectId");
                }
            }
        }

        public string ProjectName
        {
            get { return _issue.ProjectName; }
            set
            {
                if (_issue.ProjectName != value)
                {
                    _issue.ProjectName = value;
                    OnPropertyChanged("ProjectName");
                }
            }
        }

        public DateTime StartDate
        {
            get { return _issue.StartDate; }
            set
            {
                if (_issue.StartDate != value)
                {
                    _issue.StartDate = value;
                    OnPropertyChanged("StartDate");
                }
            }
        }

        public int StatusId
        {
            get { return _issue.StatusId; }
            set
            {
                if (_issue.StatusId != value)
                {
                    _issue.StatusId = value;
                    OnPropertyChanged("StatusId");
                }
            }
        }

        public string StatusName
        {
            get { return _issue.StatusName; }
            set
            {
                if (_issue.StatusName != value)
                {
                    _issue.StatusName = value;
                    OnPropertyChanged("StatusName");
                }
            }
        }

        public string Subject
        {
            get { return _issue.Subject; }
            set
            {
                if (_issue.Subject != value)
                {
                    _issue.Subject = value;
                    OnPropertyChanged("Subject");
                }
            }
        }

        public int TrackerId
        {
            get { return _issue.TrackerId; }
            set
            {
                if (_issue.TrackerId != value)
                {
                    _issue.TrackerId = value;
                    OnPropertyChanged("TrackerId");
                }
            }
        }

        public string TrackerName
        {
            get { return _issue.TrackerName; }
            set
            {
                if (_issue.TrackerName != value)
                {
                    _issue.TrackerName = value;
                    OnPropertyChanged("TrackerName");
                }
            }
        }

        public RedmineJournalViewModel[] Journals
        {
            get { return _journals; }
            set { _journals = value; }
        }

        public bool JournalsVisible
        {
            get { return _journals.Length > 0; }
        }


        public RedmineWebBrowser WebBrowser { get; set; }


        public RedmineIssueViewModel()
        {
            Initialize(new RedmineIssue());
        }

        public RedmineIssueViewModel(RedmineIssue issue)
        {
            Initialize(issue);
        }

        private void Initialize(RedmineIssue issue)
        {
            if (issue == null)
            {
                throw new ArgumentNullException("issue");
            }

            _issue = issue;
            _journals = issue.Journals == null 
                ? new RedmineJournalViewModel[0]
                : issue.Journals
                    .Where(x => x.Notes != null && x.Notes.Length > 0)
                    .Select(x => new RedmineJournalViewModel(x)).ToArray();
        }

        public void OpenInBrowser()
        {
            WebBrowser.Open(_issue);
        }
    }
}