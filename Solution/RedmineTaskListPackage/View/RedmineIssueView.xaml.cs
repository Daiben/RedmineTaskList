using System.Windows;
using System.Windows.Controls;
using RedmineTaskListPackage.ViewModel;

namespace RedmineTaskListPackage.View
{
    public partial class RedmineIssueView : UserControl
    {
        private RedmineIssueViewModel _issue;
        
        public RedmineIssueViewModel Issue
        {
            get { return _issue; }
            set
            {
                _issue = value;
                DataContext = value;
            }
        }


        public RedmineIssueView()
        {
            InitializeComponent();
        }


        private void OpenInBrowser(object sender, RoutedEventArgs e)
        {
            Issue.OpenInBrowser();
        }
    }
}
