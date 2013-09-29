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

        [Category(PackageStrings.Authentication), DefaultValue(DefaultLogin)]
        [DisplayName(PackageStrings.Username), Description(PackageStrings.UsernameDescription)]
        public string Username { get; set; }
        
        [Category(PackageStrings.Authentication), PasswordPropertyText(true)]
        [DisplayName(PackageStrings.Password), Description(PackageStrings.PasswordDescription)]
        public string Password { get; set; }
        
        [Category(PackageStrings.RedmineServer), DefaultValue(DefaultUrl)]
        [DisplayName(PackageStrings.Url), Description(PackageStrings.UrlDescription)]
        public string URL { get; set; }

        [Category(PackageStrings.RedmineServer), DefaultValue(false)]
        [DisplayName(PackageStrings.ValidateAnyCertificate), Description(PackageStrings.ValidateAnyCertificateDescription)]
        public bool ValidateAnyCertificate { get; set; }

        [Category(PackageStrings.RedmineServer), DefaultValue("")]
        [DisplayName(PackageStrings.CertificateThumbprint), Description(PackageStrings.CertificateThumbprintDescription)]
        public string CertificateThumbprint { get; set; }

        [Category(PackageStrings.Misc), DefaultValue(false)]
        [DisplayName(PackageStrings.RequestOnStartup), Description(PackageStrings.RequestOnStartupDescription)]
        public bool RequestOnStartup { get; set; }

        [Category(PackageStrings.Query), DefaultValue(DefaultQuery)]
        [DisplayName(PackageStrings.Query), Description(PackageStrings.QueryDescription)]
        public string Query { get; set; }

        [Category(PackageStrings.Formatting), DefaultValue(DefaultTaskDescriptionFormat)]
        [DisplayName(PackageStrings.TaskFormat), Description(PackageStrings.TaskFormatDescription)]
        public string TaskDescriptionFormat { get; set; }

        [Category(PackageStrings.Misc), DefaultValue(false)]
        [DisplayName(PackageStrings.UseInternalWebBrowser), Description(PackageStrings.UseInternalWebBrowserDescription)]
        public bool UseInternalWebBrowser { get; set; }

        [Category(PackageStrings.Misc), DefaultValue(false)]
        [DisplayName(PackageStrings.OpenTasksInWebBrowser), Description(PackageStrings.OpenTasksInWebBrowserDescription)]
        public bool OpenTasksInWebBrowser { get; set; }

        [Category(PackageStrings.Proxy), DefaultValue(ProxyOptions.Default)]
        [DisplayName(PackageStrings.ProxyOptions), Description(PackageStrings.ProxyOptionsDescription)]
        public ProxyOptions ProxyOptions { get; set; }

        [Category(PackageStrings.Proxy), DefaultValue(ProxyOptions.Default)]
        [DisplayName(PackageStrings.ProxyServer), Description(PackageStrings.ProxyServerDescription)]
        public string ProxyServer { get; set; }

        [Category(PackageStrings.Proxy), DefaultValue(ProxyOptions.Default)]
        [DisplayName(PackageStrings.ProxyAuthentication), Description(PackageStrings.ProxyAuthenticationDescription)]
        public ProxyOptions ProxyAuthentication { get; set; }

        [Category(PackageStrings.Proxy), DefaultValue("")]
        [DisplayName(PackageStrings.ProxyUsername), Description(PackageStrings.ProxyUsernameDescription)]
        public string ProxyUsername { get; set; }

        [Category(PackageStrings.Proxy), PasswordPropertyText(true)]
        [DisplayName(PackageStrings.ProxyPassword), Description(PackageStrings.ProxyPasswordDescription)]
        public string ProxyPassword { get; set; }

        [Category(PackageStrings.Debug), DefaultValue(false)]
        [DisplayName(PackageStrings.EnableDebugOutput), Description(PackageStrings.EnableDebugOutputDescription)]
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
            var proxy = new WebProxy { Address = uri };

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
