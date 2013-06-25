using System;
using System.IO;
using System.Net;
using System.Text;

namespace Redmine
{
    public class RedmineWebRequest
    {
        private WebRequest _request;

        public RedmineWebRequest(string username, string password, Uri requestUri)
        {
            Initialize(username, password, requestUri);
        }

        private void Initialize(string username, string password, Uri requestUri)
        {
            var cache = new CredentialCache();
            cache.Add(requestUri, "Basic", new NetworkCredential(username, password));

            _request = WebRequest.Create(requestUri);
            _request.Credentials = cache;
        }

        public string GetResponse()
        {
            var response = default(WebResponse);

            using (OwnCertificateValidation)
            {
                using (OwnBasicAuthentication)
                {
                    response = _request.GetResponse();
                }
            }
            
            using (var reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
            {
                return reader.ReadToEnd();
            }
        }


        private IDisposable OwnCertificateValidation
        {
            get { return new CertificateValidation(); }
        }

        private IDisposable OwnBasicAuthentication
        {
            get { return new BasicAuthentication(); }
        }


        private class CertificateValidation : IDisposable
        {
            private CertificateValidator validator;

            public CertificateValidation()
            {
                if (!CertificateValidator.UseDefaultValidation)
                {
                    validator = new CertificateValidator();
                    ServicePointManager.ServerCertificateValidationCallback = validator.ValidateCertificate;
                }
            }

            public void Dispose()
            {
                ServicePointManager.ServerCertificateValidationCallback = null;
            }
        }

        private class BasicAuthentication : IDisposable
        {
            private IAuthenticationModule client;

            public BasicAuthentication()
            {
                client = new BasicClient();
                AuthenticationManager.Register(client);
            }

            public void Dispose()
            {
                AuthenticationManager.Unregister(client);
            }
        }
    }
}