using System.ComponentModel;

namespace RedmineTaskListPackage
{
    public class ConnectionSettings
    {
        [DisplayName(PackageStrings.Url), Description(PackageStrings.UrlDescription)]
        public string URL { get; set; }

        [DisplayName(PackageStrings.Username), Description(PackageStrings.UsernameDescription)]
        public string Username { get; set; }

        [DisplayName(PackageStrings.Password), Description(PackageStrings.PasswordDescription)]
        public string Password { get; set; }

        [DisplayName(PackageStrings.Query), Description(PackageStrings.QueryDescription)]
        public string Query { get; set; }

        [DisplayName(PackageStrings.ValidateAnyCertificate), Description(PackageStrings.ValidateAnyCertificateDescription)]
        public bool ValidateAnyCertificate { get; set; }

        [DisplayName(PackageStrings.CertificateThumbprint), Description(PackageStrings.CertificateThumbprintDescription)]
        public string CertificateThumbprint { get; set; }
    }
}