using System;

namespace Redmine
{
    public class RedmineJournal
    {
        public int Id { get; set; }
        public int AuthorId { get; set; }
        public string AuthorName { get; set; }
        public DateTime CreationTime { get; set; }
        public string Notes { get; set; }
        public RedmineJournalDetail[] Details { get; set; } 
    }
}
