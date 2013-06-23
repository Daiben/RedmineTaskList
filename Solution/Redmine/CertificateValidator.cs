using System;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace Redmine
{
    public class CertificateValidator
    {
        public static bool ValidateAny { get; set; }

        public static string Thumbprint { get; set; }


        public bool ValidateCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return ValidateAny || CheckThumbprint(certificate); 
        }

        private static bool CheckThumbprint(X509Certificate certificate)
        {
            var x509 = certificate as X509Certificate2;

            return !String.IsNullOrEmpty(Thumbprint) && 
                x509 != null && 
                x509.Thumbprint.Equals(Thumbprint.Replace(":", ""), StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
