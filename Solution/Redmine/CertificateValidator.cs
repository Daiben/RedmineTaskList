using System;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace Redmine
{
    public abstract class CertificateValidator : IDisposable
    {
        public static CertificateValidator NoValidation 
        {
            get { return new CertificateValidatorStub(); }
        }


        protected CertificateValidator()
        {
            ServicePointManager.ServerCertificateValidationCallback = ValidateCertificate;
        }

        public void Dispose()
        {
            ServicePointManager.ServerCertificateValidationCallback = null;
        }

        protected abstract bool ValidateCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors);


        private class CertificateValidatorStub : CertificateValidator
        {
            protected override bool ValidateCertificate(object sender, System.Security.Cryptography.X509Certificates.X509Certificate certificate, System.Security.Cryptography.X509Certificates.X509Chain chain, System.Net.Security.SslPolicyErrors sslPolicyErrors)
            {
                return true;
            }
        }
    }
}
