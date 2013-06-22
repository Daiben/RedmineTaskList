using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms.Design;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Redmine;
using RedmineTaskListPackage.Tree;
using RedmineTaskListPackage.ViewModel;

namespace RedmineTaskListPackage
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    [ProvideOptionPage(typeof(PackageOptions), PackageOptions.Category, PackageOptions.Page, 101, 106, true)]
    [ProvideToolWindow(typeof(RedmineIssueViewerToolWindow))]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.NoSolution_string)] 
    [Guid(Guids.guidRedminePkgString)]
    public sealed class RedmineTaskListPackage : Package, IDisposable, IRedmineIssueViewer
    {
        private RedmineTaskProvider taskProvider;
        private MenuCommand getTasksMenuCommand;
        private MenuCommand viewIssueMenuCommand;
        private RedmineIssueViewerToolWindow issueViewerWindow;
        private RedmineWebBrowser webBrowser;
        private object syncRoot;
        private bool running;

    


        public RedmineTaskListPackage()
        {
            var getTasksCommandID = new CommandID(Guids.guidRedmineCmdSet, (int)CommandIDs.cmdidGetTasks);
            getTasksMenuCommand = new MenuCommand(GetTasksMenuItemCallback, getTasksCommandID);
            
            var viewIssueCommandID = new CommandID(Guids.guidRedmineCmdSet, (int)CommandIDs.cmdidViewIssues);
            viewIssueMenuCommand = new MenuCommand(ViewIssueMenuItemCallback, viewIssueCommandID);
            
            syncRoot = new object();
            webBrowser = new RedmineWebBrowser() { ServiceProvider = this };
        }
        

        public void Dispose()
        {
            taskProvider.Dispose();
        }

        protected override void Initialize()
        {
            base.Initialize();

            InitializeTaskProvider();
            AddMenuCommands();

            if (GetOptions().RequestOnStartup)
            {
                RefreshTasksAsync();
            }
        }

        private void AddMenuCommands()
        {
            var menuCommandService = GetService(typeof(IMenuCommandService)) as IMenuCommandService;

            if (menuCommandService != null)
            {
                menuCommandService.AddCommand(getTasksMenuCommand);
                menuCommandService.AddCommand(viewIssueMenuCommand);
            }
        }

        private void GetTasksMenuItemCallback(object sender, EventArgs e)
        {
            InitializeExplorerWindow();

            RefreshTasksAsync(taskProvider.Show, ShowOutputPane);
        }

        private void ViewIssueMenuItemCallback(object sender, EventArgs e)
        {
            issueViewerWindow.Show();
        }


        private void InitializeTaskProvider()
        {
            taskProvider = new RedmineTaskProvider(this);
            taskProvider.Register();
        }

        private void RefreshTasksAsync(Action onSuccess = null, Action onFailure = null)
        {
            lock (syncRoot)
            {
                if (!running)
                {
                    running = true;
                    Action action = new Action(RefreshTasks);
                    action.BeginInvoke((AsyncCallback)(x => {
                        Action callback = onSuccess;

                        try
                        {
                            action.EndInvoke(x);
                        }
                        catch (Exception e)
                        {
                            Debug.WriteLine(e);
                            callback = onFailure;
                        }

                        if (callback != null)
                        {
                            callback.Invoke();
                        }

                        running = false; 
                    }), null);
                }
            }
        }

        private void RefreshTasks()
        {
            taskProvider.SuspendRefresh();
            taskProvider.Tasks.Clear();

            try
            {
                var options = GetOptions();
                var issues = GetTasks(options);

                foreach (var issue in issues)
                {
                    taskProvider.Tasks.Add(new RedmineTask(options, issue, this as IRedmineIssueViewer));
                }
            }
            finally
            {
                taskProvider.ResumeRefresh();
            }
        }

        private RedmineIssue[] GetTasks(PackageOptions options)
        {
            OutputLine(String.Format("Retrieving issues from {0} as {1}...", options.URL, options.Username));

            try
            {
                var issues = RedmineService.GetIssues(options.Username, options.Password, options.URL, options.Query);
                
                OutputLine("Done");

                return issues;
            }
            catch
            {
                OutputLine("Error");

                throw;
            }
        }

        private PackageOptions GetOptions()
        {
            return PackageOptions.GetOptions(this);
        }


        private void OutputLine(string s)
        {
            GetOutputPane().OutputString(s + '\n');
        }

        private void ShowOutputPane()
        {
            if (!VsShellUtilities.ShellIsShuttingDown)
            {
                IUIService service = this.GetService(typeof(IUIService)) as IUIService;
                
                if (service != null)
                {
                    service.ShowToolWindow(new Guid(ToolWindowGuids.Outputwindow));
                }

                GetOutputPane().Activate();
            }
        }

        private IVsOutputWindowPane GetOutputPane()
        {
            return GetOutputPane(VSConstants.SID_SVsGeneralOutputWindowPane, "Redmine");
        }

        private void InitializeExplorerWindow()
        {
            issueViewerWindow = FindToolWindow(typeof(RedmineIssueViewerToolWindow), 0, true) as RedmineIssueViewerToolWindow;

            if (issueViewerWindow == null || issueViewerWindow.Frame == null)
            {
                throw new NotSupportedException(Resources.CanNotCreateWindow);
            }
        }

        void IRedmineIssueViewer.Show(RedmineIssue issue)
        {
            if (GetOptions().OpenTasksInWebBrowser)
            {
                webBrowser.Open(issue);
            }
            else
            {
                ViewIssue(issue);
            }
        }

        private void ViewIssue(RedmineIssue issue)
        {
            if (issueViewerWindow == null)
            {
                InitializeExplorerWindow();
            }

            issueViewerWindow.Show(new RedmineIssueViewModel(issue) {
                WebBrowser = webBrowser
            });
        }
    }
}
