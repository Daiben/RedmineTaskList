using System;
using System.ComponentModel;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
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
        public const string DefaultQuery = "assigned_to_id=me";
        public const string DefaultTaskDescriptionFormat = "#{0}\\t{4}\\t{13} ({6})";

        [Category("Authentication"), DefaultValue(DefaultLogin)]
        [Description("Specifies username used for authentication.")]
        public string Username { get; set; }
        
        [Category("Authentication"), PasswordPropertyText(true)]
        [Description("Specifies password used for authentication.")]
        public string Password { get; set; }
        
        [Category("Redmine Server"), DefaultValue(DefaultUrl)]
        [Description("Specifies redmine server URL. (Please note that REST web service must be enabled).")]
        public string URL { get; set; }

        [Category("Redmine Server"), DefaultValue(false)]
        [DisplayName("Validate Any Certificate"), Description("Specifies any certificate will be validated. To be used with care.")]
        public bool ValidateAnyCertificate { get; set; }

        [Category("Redmine Server"), DefaultValue("")]
        [DisplayName("Certificate Thumbprint"), Description("Specifies certificate thumbprint used for validation.")]
        public string CertificateThumbprint { get; set; }

        [Category("Misc."), DefaultValue(false)]
        [DisplayName("Request on Startup"), Description("Specifies issues will be requested on startup.")]
        public bool RequestOnStartup { get; set; }

        [Category("Query"), DefaultValue(DefaultQuery)]
        [Description("Specifies query used in request. Default is \"assigned_to_id=me\".")]
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

        [Category("Proxy"), DefaultValue(ProxyOptions.Default)]
        [DisplayName("Proxy Options"), Description("Specifies proxy options.")]
        public ProxyOptions ProxyOptions { get; set; }

        [Category("Proxy"), DefaultValue(ProxyOptions.Default)]
        [DisplayName("Proxy Server"), Description("Specifies custom proxy server address.")]
        public string ProxyServer { get; set; }

        [Category("Proxy"), DefaultValue(ProxyOptions.Default)]
        [DisplayName("Proxy Authentication"), Description("Specifies proxy authentication.")]
        public ProxyOptions ProxyAuthentication { get; set; }

        [Category("Proxy"), DefaultValue("")]
        [DisplayName("Proxy Username"), Description("Specifies username used for proxy authentication.")]
        public string ProxyUsername { get; set; }

        [Category("Proxy"), PasswordPropertyText(true)]
        [DisplayName("Proxy Password"), Description("Specifies password used for proxy authentication.")]
        public string ProxyPassword { get; set; }

        [Category("Debug"), DefaultValue(false)]
        [DisplayName("Enable Debug Output"), Description("Specifies exception information is written to output.")]
        public bool EnableDebugOutput { get; set; }


        public PackageOptions()
        {
            Initialize();
        }

        private void Initialize()
        {
            Username = DefaultLogin;
            Password = DefaultLogin;
            URL = DefaultUrl;
            ValidateAnyCertificate = false;
            CertificateThumbprint = "";
            RequestOnStartup = false;
            Query = DefaultQuery;
            TaskDescriptionFormat = DefaultTaskDescriptionFormat;
            UseInternalWebBrowser = false;
            OpenTasksInWebBrowser = false;
            ProxyOptions = ProxyOptions.Default;
            ProxyAuthentication = ProxyOptions.Default;
            ProxyServer = "";
            ProxyUsername = "";
            ProxyPassword = "";
            EnableDebugOutput = false;
        }

        public override void ResetSettings()
        {
            base.ResetSettings();
            Initialize();
        }

        public IWebProxy GetProxy()
        {
            IWebProxy proxy = null;

            if (ProxyOptions == ProxyOptions.Default)
            {
                proxy = HttpWebRequest.DefaultWebProxy;
            }
            else if (ProxyOptions == ProxyOptions.Custom && !String.IsNullOrEmpty(ProxyServer))
            {
                proxy = GetCustomProxy();
            }

            return proxy;
        }

        private IWebProxy GetCustomProxy()
        {
            var uri = new Uri(ProxyServer);
            var proxy = new WebProxy() {
                Address = uri,
            };

            if (ProxyAuthentication == ProxyOptions.Custom)
            {
                var cache = new CredentialCache();
                cache.Add(uri, "Basic", new NetworkCredential(ProxyUsername, ProxyPassword));
                proxy.Credentials = cache;
            }
            else if (ProxyAuthentication == ProxyOptions.Default)
            {
                proxy.Credentials = CredentialCache.DefaultNetworkCredentials;
            }

            return proxy;
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

        private static EnvDTE.Properties GetDteProperties(IServiceProvider provider)
        {
            var dte = (EnvDTE.DTE)provider.GetService(typeof(EnvDTE.DTE));
            
            return dte.get_Properties(Category, Page);
        }
    }
}
