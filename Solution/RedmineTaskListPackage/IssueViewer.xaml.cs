using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using Redmine;
using RedmineTaskListPackage.Tree;
using RedmineTaskListPackage.ViewModel;

namespace RedmineTaskListPackage
{
    public partial class IssueViewer : UserControl
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


        public IssueViewer()
        {
            InitializeComponent();
        }


        private void OpenInBrowser(object sender, RoutedEventArgs e)
        {
            Issue.OpenInBrowser();
        }
    }
}
