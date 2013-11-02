using System;
using System.IO;
using System.Net;
using System.Text;

namespace Redmine
{
    public class RedmineWebRequest
    {
        private static readonly object syncRoot = new object();
        
        private WebRequest _request;

        public RedmineWebRequest(string username, string password, Uri requestUri, IWebProxy proxy = null)
        {
            Initialize(username, password, requestUri, proxy);
        }

        private void Initialize(string username, string password, Uri requestUri, IWebProxy proxy)
        {
            var cache = new CredentialCache();
            cache.Add(requestUri, "Basic", new NetworkCredential(username, password));

            _request = WebRequest.Create(requestUri);
            _request.Proxy = proxy;
            _request.Credentials = cache;
        }

        public string GetResponse()
        {
            lock (syncRoot) // Thread-safe access to System.Net static resources
            {
                return ReadToEnd(GetResponseStream());
            }
        }

        private Stream GetResponseStream()
        {
            var response = ValidateCertificateAndGetResponse();

            return response.GetResponseStream();
        }

        private WebResponse ValidateCertificateAndGetResponse()
        {
            using (OwnCertificateValidation)
            {
                return AuthenticateAndGetResponse();
            }
        }

        private WebResponse AuthenticateAndGetResponse()
        {
            using (OwnBasicAuthentication)
            {
                return _request.GetResponse();
            }
        }

        private string ReadToEnd(Stream stream)
        {
            using (var reader = new StreamReader(stream, Encoding.UTF8))
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