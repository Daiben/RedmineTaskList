using System;
using System.Diagnostics;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Redmine;

namespace RedmineTaskListPackage
{
    public class RedmineWebBrowser
    {
        public IServiceProvider ServiceProvider { get; set; }
        

        public void Open(RedmineIssue issue)
        {
            if (ServiceProvider == null)
            {
                return;
            }

            var options = PackageOptions.GetOptions(ServiceProvider);

            var baseUri = new Uri(options.URL);
            var issueUri = new Uri(baseUri, String.Concat("issues/", issue.Id));

            Open(issueUri.ToString(), options.UseInternalWebBrowser);
        }

        public void Open(string url, bool useInternal)
        {
            if (useInternal)
            {
                var browserService = ServiceProvider.GetService(typeof(SVsWebBrowsingService)) as IVsWebBrowsingService;
                IVsWindowFrame ppFrame;

                ErrorHandler.ThrowOnFailure(browserService.Navigate(url, 0, out ppFrame));
            }
            else
            {
                Process.Start(url);
            }
        }
    }
}
