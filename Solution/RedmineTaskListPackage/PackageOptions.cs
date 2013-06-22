using System;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.InteropServices;
using EnvDTE;
using Microsoft.VisualStudio.Shell;

namespace RedmineTaskListPackage
{
    [CLSCompliant(false), ComVisible(true)]
    public class PackageOptions : DialogPage
    {
        public const string Category = "Redmine Task List";
        public const string Page = "General";

        public const string DefaultLogin = "admin";
        public const string DefaultUrl = "http://localhost:3000/";
        public const string DefaultQuery = "assigned_to_id={0}";
        public const string DefaultTaskDescriptionFormat = "#{0}\\t{4}\\t{13} ({6})";

        [Category("Authentication"), DefaultValue(DefaultLogin)]
        [Description("Username used for authentication. Only issues assigned to this user will be requested.")]
        public string Username { get; set; }
        
        [Category("Authentication"), PasswordPropertyText(true)]
        [Description("Password used for authentication.")]
        public string Password { get; set; }
        
        [Category("Redmine Server"), DefaultValue(DefaultUrl)]
        [Description("Redmine server URL. (Please note that REST web service must be enabled).")]
        public string URL { get; set; }

        [Category("Misc."), DefaultValue(false)]
        [DisplayName("Request on Startup"), Description("Specifies issues will be requested on startup.")]
        public bool RequestOnStartup { get; set; }

        [Category("Query"), DefaultValue(DefaultQuery)]
        [Description("Specifies query used in request. Default is \"assigned_to_id={0}\" where {0} stands for current user ID.")]
        public string Query { get; set; }

        [Category("Formatting"), DefaultValue(DefaultTaskDescriptionFormat)]
        [DisplayName("Task Description Format"), Description("Specifies formatting used for task description. {0} - Id, {1} - ProjectId, {2} - ProjectName, {3} - TrackerId, {4} - TrackerName, {5} - StatusId, {6} - StatusName, {7} - PriorityId, {8} - PriorityName, {9} - AuthorId, {10} - AuthorName, {11} - AssigneeId, {12} - AssigneeName, {13} - Subject, {14} - Description, {15} - StartDate, {16} - DueDate, {17} - DoneRatio, {18} - EstimatedHours, {19} - CreationTime, {20} - LastUpdateTime, {21} - ClosingTime.")]
        public string TaskDescriptionFormat { get; set; }

        [Category("Misc."), DefaultValue(false)]
        [DisplayName("Use Internal Web Browser"), Description("Specifies links are opened in internal web browser.")]
        public bool UseInternalWebBrowser { get; set; }

        [Category("Misc."), DefaultValue(false)]
        [DisplayName("Open Tasks In Web Browser"), Description("Specifies double click on task opens web browser instead of issue viewer.")]
        public bool OpenTasksInWebBrowser { get; set; }


        public PackageOptions()
        {
            Initialize();
        }

        private void Initialize()
        {
            Username = DefaultLogin;
            Password = DefaultLogin;
            URL = DefaultUrl;
            RequestOnStartup = false;
            Query = DefaultQuery;
            TaskDescriptionFormat = DefaultTaskDescriptionFormat;
            UseInternalWebBrowser = false;
            OpenTasksInWebBrowser = false;
        }

        public override void ResetSettings()
        {
            base.ResetSettings();
            Initialize();
        }

        public static PackageOptions GetOptions(IServiceProvider provider)
        {
            var options = new PackageOptions();
            var dteProperties = GetDteProperties(provider);
            var publicNoInheritance = BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly;

            foreach (var property in typeof(PackageOptions).GetProperties(publicNoInheritance))
            {
                property.SetValue(options, dteProperties.Item(property.Name).Value, null);
            }

            return  options;
        }

        private static Properties GetDteProperties(IServiceProvider provider)
        {
            var dte = (DTE)provider.GetService(typeof(DTE));
            
            return dte.get_Properties(Category, Page);
        }

    }
}
