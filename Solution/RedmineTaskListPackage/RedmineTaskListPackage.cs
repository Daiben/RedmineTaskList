using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Redmine;
using RedmineTaskListPackage.Forms;
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
    public sealed class RedmineTaskListPackage : Package, IDisposable
    {
        private RedmineTaskProvider taskProvider;
        private MenuCommand getTasksMenuCommand;
        private MenuCommand viewIssueMenuCommand;
        private MenuCommand projectSettingsMenuCommand;
        private RedmineIssueViewerToolWindow issueViewerWindow;
        private RedmineWebBrowser webBrowser;
        private object syncRoot;
        private bool running;
        private EnvDTE.SolutionEvents solutionEvents;


        public RedmineTaskListPackage()
        {
            var getTasksCommandID = new CommandID(Guids.guidRedmineCmdSet, (int)CommandIDs.cmdidGetTasks);
            getTasksMenuCommand = new MenuCommand(GetTasksMenuItemCallback, getTasksCommandID);
            
            var viewIssueCommandID = new CommandID(Guids.guidRedmineCmdSet, (int)CommandIDs.cmdidViewIssues);
            viewIssueMenuCommand = new MenuCommand(ViewIssueMenuItemCallback, viewIssueCommandID);

            var projectSettingsCommandID = new CommandID(Guids.guidRedmineCmdSet, (int)CommandIDs.cmdidProjectSettings);
            projectSettingsMenuCommand = new MenuCommand(ProjectSettingsMenuItemCallback, projectSettingsCommandID);
            
            webBrowser = new RedmineWebBrowser { ServiceProvider = this };
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
            
            var dte = (EnvDTE.DTE)GetGlobalService(typeof(EnvDTE.DTE));
            solutionEvents = dte.Events.SolutionEvents;

            solutionEvents.Opened += RefreshTasksAsync;
            solutionEvents.AfterClosing += RefreshTasksAsync;

            if (GetOptions().RequestOnStartup)
            {
                RefreshTasksAsync();
            }
        }

        private RedmineTask CreateTask(RedmineIssue issue, string format)
        {
            var task = new RedmineTask(issue, format);

            task.Navigate += (s, e) => Show(issue);

            return task;
        }

        private void AddMenuCommands()
        {
            var menuCommandService = GetService(typeof(IMenuCommandService)) as IMenuCommandService;

            if (menuCommandService != null)
            {
                menuCommandService.AddCommand(getTasksMenuCommand);
                menuCommandService.AddCommand(viewIssueMenuCommand);
                menuCommandService.AddCommand(projectSettingsMenuCommand);
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

        private void ProjectSettingsMenuItemCallback(object sender, EventArgs e)
        {
            var project = GetSelectedProject() as EnvDTE.Project;

            if (project == null)
            {
                return;
            }

            var storage = new ConnectionSettingsStorage(this, project.FullName);

            var dialog = new ConnectionSettingsDialog
            {
                ConnectionSettings = storage.Load(),
                StartPosition = FormStartPosition.CenterParent,
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                storage.Save(dialog.ConnectionSettings);
            }
        }

        private object GetSelectedProject()
        {
            var dte = (EnvDTE.DTE)GetService(typeof(EnvDTE.DTE));
            var projects = dte.ActiveSolutionProjects as Array;

            return projects.Length > 0 ? projects.GetValue(0) : null;
        }

        private void InitializeTaskProvider()
        {
            taskProvider = new RedmineTaskProvider(this);
            taskProvider.Register();
        }

        
        private void RefreshTasksAsync()
        {
            RefreshTasksAsync(null, null);
        }
        
        private void RefreshTasksAsync(Action onSuccess, Action onFailure)
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
                        catch
                        {
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
            var issues = LoadIssues();

            PopulateTaskList(issues);
        }

        private void PopulateTaskList(RedmineIssue[] issues)
        {
            var options = GetOptions();
            
            taskProvider.SuspendRefresh();
            taskProvider.Tasks.Clear();

            foreach (var issue in issues)
            {
                taskProvider.Tasks.Add(CreateTask(issue, options.TaskDescriptionFormat));
            }
         
            taskProvider.ResumeRefresh();
        }

        private RedmineIssue[] LoadIssues()
        {
            var options = GetOptions();

            CertificateValidator.ValidateAny = options.ValidateAnyCertificate;
            CertificateValidator.Thumbprint = options.CertificateThumbprint;

            var loader = new IssueLoader { Proxy = options.GetProxy() };
            
            return loader.LoadIssues(GetConnectionSettings());
        }

        private ConnectionSettings[] GetConnectionSettings()
        {
            var dte = (EnvDTE.DTE)GetService(typeof(EnvDTE.DTE));

            var solution = dte.Solution;
            var settings = new List<ConnectionSettings>();

            for (int i = 1; i < solution.Projects.Count + 1; i++)
            {
                var project = solution.Projects.Item(i);
                var storage = new ConnectionSettingsStorage(this, project.FullName);

                settings.Add(storage.Load());
            }

            var options = GetOptions();

            if (options.RequestGlobal)
            {
                settings.Insert(0, options.GetConnectionSettings());
            }

            return settings.ToArray();
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

        private void Show(RedmineIssue issue)
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
