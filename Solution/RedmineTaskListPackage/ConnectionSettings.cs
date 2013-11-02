using System.ComponentModel;

namespace RedmineTaskListPackage
{
    public class ConnectionSettings
    {
        [Category(PackageStrings.RedmineServer)]
        [DisplayName(PackageStrings.Url), Description(PackageStrings.UrlDescription)]
        public string URL { get; set; }

        [Category(PackageStrings.Authentication)]
        [DisplayName(PackageStrings.Username), Description(PackageStrings.UsernameDescription)]
        public string Username { get; set; }

        [Category(PackageStrings.Authentication), PasswordPropertyText(true)]
        [DisplayName(PackageStrings.Password), Description(PackageStrings.PasswordDescription)]
        public string Password { get; set; }

        [Category(PackageStrings.Query)]
        [DisplayName(PackageStrings.Query), Description(PackageStrings.QueryDescription)]
        public string Query { get; set; }

        [Category(PackageStrings.RedmineServer)]
        [DisplayName(PackageStrings.ValidateAnyCertificate), Description(PackageStrings.ValidateAnyCertificateDescription)]
        public bool ValidateAnyCertificate { get; set; }

        [Category(PackageStrings.RedmineServer)]
        [DisplayName(PackageStrings.CertificateThumbprint), Description(PackageStrings.CertificateThumbprintDescription)]
        public string CertificateThumbprint { get; set; }
    }
}