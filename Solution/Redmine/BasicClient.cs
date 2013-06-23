using System;
using System.Net;
using System.Text;

namespace Redmine
{
    public class BasicClient : IAuthenticationModule
    {
        public string AuthenticationType
        {
            get { return "Basic"; }
        }

        public bool CanPreAuthenticate
        {
            get { return true; }
        }


        public BasicClient() { }


        public Authorization PreAuthenticate(WebRequest request, ICredentials credentials)
        {
            return Authenticate(request, credentials);
        }

        public Authorization Authenticate(string challenge, WebRequest request, ICredentials credentials)
        {
            if (!CheckChallenge(challenge, ""))
            {
                return null;
            }

            return Authenticate(request, credentials);
        }

        private bool CheckChallenge(string challenge, string domain)
        {
            if (challenge.IndexOf(AuthenticationType, StringComparison.InvariantCultureIgnoreCase) == -1)
            {
                return false;
            }

            if (!String.IsNullOrEmpty(domain) && challenge.IndexOf(domain, StringComparison.InvariantCultureIgnoreCase) == -1)
            {
                return false;
            }

            return true;
        }

        private Authorization Authenticate(WebRequest webRequest, ICredentials credentials)
        {
            var credential = credentials.GetCredential(webRequest.RequestUri, AuthenticationType);
            var combined = String.Format("{0}:{1}", credential.UserName, credential.Password);
            var encoded = Convert.ToBase64String(Encoding.ASCII.GetBytes(combined));
            var message = String.Format("{0} {1}", AuthenticationType, encoded);

            return new Authorization(message);
        }
    }
}
