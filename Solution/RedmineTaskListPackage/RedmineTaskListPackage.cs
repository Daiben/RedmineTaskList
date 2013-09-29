using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms.Design;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Redmine;
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
        private RedmineService redmine;
        private object syncRoot;
        private bool running;

    


        public RedmineTaskListPackage()
        {
            var getTasksCommandID = new CommandID(Guids.guidRedmineCmdSet, (int)CommandIDs.cmdidGetTasks);
            getTasksMenuCommand = new MenuCommand(GetTasksMenuItemCallback, getTasksCommandID);
            
            var viewIssueCommandID = new CommandID(Guids.guidRedmineCmdSet, (int)CommandIDs.cmdidViewIssues);
            viewIssueMenuCommand = new MenuCommand(ViewIssueMenuItemCallback, viewIssueCommandID);
            
            redmine = new RedmineService();
            webBrowser = new RedmineWebBrowser() { ServiceProvider = this };
            syncRoot = new object();
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
            InitializeIssueViewerWindow();

            RefreshTasksAsync(taskProvider.Show, ShowOutputPane);
        }

        private void ViewIssueMenuItemCallback(object sender, EventArgs e)
        {
            InitializeIssueViewerWindow();
            
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
                    var action = new Action(RefreshTasks);
                    action.BeginInvoke((AsyncCallback)(x => {
                        var callback = onSuccess;

                        try
                        {
                            action.EndInvoke(x);
                        }
                        catch (Exception e)
                        {
                            OutputException(e);
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

            var options = GetOptions();
            ApplyOptions(options);

            try
            {
                foreach (var issue in GetIssues(options.Query))
                {
                    taskProvider.Tasks.Add(new RedmineTask(options, issue, this as IRedmineIssueViewer));
                }
            }
            catch (Exception e)
            {
                OutputException(e);
                throw;
            }
            finally
            {
                taskProvider.ResumeRefresh();
            }
        }

        private void ApplyOptions(PackageOptions options)
        {
            redmine.Username = options.Username;
            redmine.Password = options.Password;
            redmine.BaseUriString = options.URL;
            redmine.Proxy = options.GetProxy();
            
            CertificateValidator.ValidateAny = options.ValidateAnyCertificate;
            CertificateValidator.Thumbprint = options.CertificateThumbprint;
        }

        private RedmineIssue[] GetIssues(string query)
        {
            var success = true;

            OutputLine(String.Format("Retrieving issues from {0} as {1}...", redmine.BaseUri, redmine.Username));

            try
            {
                return redmine.GetIssues(query);
            }
            catch (Exception e)
            {
                success = false;
                OutputException(e);
                throw;
            }
            finally
            {
                OutputLine(success ? "Done." : "Error.");
            }
        }

        private PackageOptions GetOptions()
        {
            return PackageOptions.GetOptions(this);
        }

        

        private void OutputException(Exception e)
        {
            if (GetOptions().EnableDebugOutput)
            {
                OutputLine(e.ToString());
            }
        }

        private void OutputLine(string s)
        {
            GetOutputPane().OutputString(s + '\n');
        }

        private void ShowOutputPane()
        {
            var uiService = this.GetService(typeof(IUIService)) as IUIService;

            if (uiService != null)
            {
                uiService.ShowToolWindow(new Guid(ToolWindowGuids.Outputwindow));
            }

            GetOutputPane().Activate();
        }

        private IVsOutputWindowPane GetOutputPane()
        {
            return GetOutputPane(VSConstants.SID_SVsGeneralOutputWindowPane, "Redmine");
        }

        private void InitializeIssueViewerWindow()
        {
            if (issueViewerWindow != null)
            {
                return;
            }

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
            InitializeIssueViewerWindow();

            issueViewerWindow.Show(new RedmineIssueViewModel(issue)
            {
                WebBrowser = webBrowser
            });
        }
    }
}
