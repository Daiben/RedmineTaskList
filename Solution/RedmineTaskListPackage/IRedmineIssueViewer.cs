using Redmine;

namespace RedmineTaskListPackage
{
    public interface IRedmineIssueViewer
    {
        void Show(RedmineIssue issue);
    }
}
