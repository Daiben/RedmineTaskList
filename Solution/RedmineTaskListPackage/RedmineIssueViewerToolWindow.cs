using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Redmine;
using RedmineTaskListPackage.ViewModel;

namespace RedmineTaskListPackage
{
    [Guid("e17deb2b-3254-42f9-83cb-ad86d57afebd")]
    public class RedmineIssueViewerToolWindow : ToolWindowPane
    {
        IssueViewer issueViewer;

        public RedmineIssueViewerToolWindow() :
            base()
        {
            issueViewer = new IssueViewer();

            Caption = Resources.ToolWindowTitle;
            Content = issueViewer;
        }

        public void Show()
        {
            var windowFrame = (IVsWindowFrame)Frame;
            ErrorHandler.ThrowOnFailure(windowFrame.Show());
        }
        
        public void Show(RedmineIssueViewModel issue)
        {
            issueViewer.Issue = issue;
            Show();
        }
    }
}
