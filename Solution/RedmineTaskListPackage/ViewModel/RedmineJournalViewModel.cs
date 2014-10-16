using System;
using Redmine;

namespace RedmineTaskListPackage.ViewModel
{
    public class RedmineJournalViewModel : ObservableObject
    {
        private RedmineJournal _journal;
        
        public int Id
		{
			get { return _journal.Id; }
			set { _journal.Id = value; } 
		}
		
        public int AuthorId
        {
        	get { return _journal.AuthorId; }
        	set { _journal.AuthorId = value; } 
        }
        
        public string AuthorName
        {
        	get { return _journal.AuthorName; }
        	set { _journal.AuthorName = value; } 
        }
        
        public DateTime CreationTime
        {
        	get { return _journal.CreationTime; }
        	set { _journal.CreationTime = value; } 
        }
        
        public string Notes
        {
        	get { return _journal.Notes; }
        	set { _journal.Notes = value; } 
        }
        

        public RedmineJournalViewModel()
        {
            Initialize(new RedmineJournal());
        }

        public RedmineJournalViewModel(RedmineJournal journal)
        {
            Initialize(journal);
        }

        private void Initialize(RedmineJournal journal)
        {
            if (journal == null)
            {
                throw new ArgumentNullException("journal");
            }

            _journal = journal;
        }
    }
}