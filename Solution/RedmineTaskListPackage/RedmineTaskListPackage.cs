using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
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
    public sealed class RedmineTaskListPackage : Package, IDisposable, IDebug
    {
        private RedmineTaskProvider taskProvider;
        private MenuCommand getTasksMenuCommand;
        private MenuCommand viewIssueMenuCommand;
        private MenuCommand projectSettingsMenuCommand;
        private RedmineIssueViewerToolWindow issueViewerWindow;
        private RedmineWebBrowser webBrowser;
        private object syncRoot;
        private bool refreshing;
        private EnvDTE.SolutionEvents solutionEvents;
        private EnvDTE.WindowEvents windowEvents;

        private PackageOptions Options
        {
            get { return PackageOptions.GetOptions(this); }
        }


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
            windowEvents = dte.Events.WindowEvents;

            solutionEvents.Opened += RefreshTasksAsync;
            solutionEvents.AfterClosing += () => taskProvider.Tasks.Clear();

            if (Options.RequestOnStartup)
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
            var menuService = GetService(typeof(IMenuCommandService)) as IMenuCommandService;

            if (menuService == null)
            {
                return;
            }

            menuService.AddCommand(getTasksMenuCommand);
            menuService.AddCommand(viewIssueMenuCommand);
            menuService.AddCommand(projectSettingsMenuCommand);
        }

        private void GetTasksMenuItemCallback(object sender, EventArgs e)
        {
            InitializeIssueViewerWindow();

            RefreshTasksAsync(taskProvider.Show);
        }

        private void ViewIssueMenuItemCallback(object sender, EventArgs e)
        {
            InitializeIssueViewerWindow();
            
            issueViewerWindow.Show();
        }

        private void ProjectSettingsMenuItemCallback(object sender, EventArgs e)
        {
            var project = GetSelectedProject() as EnvDTE.Project;
            var storage = GetConnectionSettingsStorage(project);

            if (storage == null)
            {
                return;
            }
            
            var dialog = new ConnectionSettingsDialog
            {
                ConnectionSettings = storage.Load(),
                StartPosition = FormStartPosition.CenterParent,
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                storage.Save(dialog.ConnectionSettings);
                RefreshTasksAsync();
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
            RefreshTasksAsync(null);
        }

        private void RefreshTasksAsync(Action callback)
        {
            lock (syncRoot)
            {
                BeginRefreshTasks(callback);
            }
        }

        private void BeginRefreshTasks(Action callback)
        {
            if (refreshing)
            {
                return;
            }
                
            refreshing = true;

            var refresh = new Action(RefreshTasks);

            callback = callback ?? (() => { });

            refresh.BeginInvoke((AsyncCallback)(x => 
            {
                refresh.EndInvoke(x);
                callback.Invoke();
                refreshing = false;
            }), null);
        }

        private void RefreshTasks()
        {
            var issues = LoadIssues();

            PopulateTaskList(issues);
        }

        private void PopulateTaskList(RedmineIssue[] issues)
        {
            taskProvider.SuspendRefresh();
            taskProvider.Tasks.Clear();

            foreach (var issue in issues)
            {
                taskProvider.Tasks.Add(CreateTask(issue, Options.TaskDescriptionFormat));
            }
         
            taskProvider.ResumeRefresh();
        }

        private RedmineIssue[] LoadIssues()
        {
            CertificateValidator.ValidateAny = Options.ValidateAnyCertificate;
            CertificateValidator.Thumbprint = Options.CertificateThumbprint;

            var loader = new IssueLoader
            { 
                Proxy = Options.GetProxy(),
                Debug = this as IDebug,
            };
            
            return loader.LoadIssues(GetConnectionSettings());
        }

        private ConnectionSettings[] GetConnectionSettings()
        {
            var settings = GetProjectConnectionSettings();

            if (Options.RequestGlobal)
            {
                settings.Insert(0, Options.GetConnectionSettings());
            }

            return settings.ToArray();
        }

        private List<ConnectionSettings> GetProjectConnectionSettings()
        {
            var dte = (EnvDTE.DTE)GetService(typeof(EnvDTE.DTE));

            return dte.Solution.Projects.Cast<EnvDTE.Project>()
                .Select(LoadConectionSettings).Where(x => x != null).ToList();
        }


        private ConnectionSettings LoadConectionSettings(EnvDTE.Project project)
        {
            var storage = GetConnectionSettingsStorage(project);

            return storage != null ? storage.Load() : null;
        }

        private ConnectionSettingsStorage GetConnectionSettingsStorage(EnvDTE.Project project)
        {
            var projectPath = "";

            if (project == null || !TryGetProjectFullName(project, out projectPath))
            {
                return null;
            }

            return GetConnectionSettingsStorage(projectPath);
        }

        private ConnectionSettingsStorage GetConnectionSettingsStorage(string projectPath)
        {
            var propertyStorage = GetPropertyStorage(projectPath);

            if (propertyStorage == null)
            {
                return null;
            }

            return new ConnectionSettingsStorage(propertyStorage);
        }

        private IVsBuildPropertyStorage GetPropertyStorage(string projectPath)
        {
            var solution = GetService(typeof(SVsSolution)) as IVsSolution;
            var hierarchy = default(IVsHierarchy);

            solution.GetProjectOfUniqueName(projectPath, out hierarchy);

            return hierarchy as IVsBuildPropertyStorage;
        }

        private bool TryGetProjectFullName(EnvDTE.Project project, out string fullName)
        {
            var result = true;
            fullName = null;

            try
            {
                fullName = project.FullName;
            }
            catch
            {
                result = false;
            }

            return result;
        }

        
        

        void IDebug.WriteLine(string s)
        {
            if (Options.EnableDebugOutput)
            {
                OutputLine(s);
            }
        }

        private void OutputLine(string s)
        {
            GetOutputPane().OutputString(s + Environment.NewLine);
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
            if (Options.OpenTasksInWebBrowser)
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
