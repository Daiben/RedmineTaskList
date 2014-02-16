using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace RedmineTaskListPackage
{
    public class ConnectionSettingsStorage
    {
        private static byte[] entropy = { 1, 6, 3, 0, 4, 5 };
        private ProjectPropertyStorage _storage;


        public ConnectionSettingsStorage(IVsBuildPropertyStorage storage)
        {
            _storage = new ProjectPropertyStorage(storage);
        }


        public ConnectionSettings Load()
        {
            var settings = new ConnectionSettings
            {
                URL = GetProperty("RedmineURL"),
                Username = GetProperty("RedmineUsername"),
                Password = GetProtectedProperty("RedminePassword"),
                Query = GetProperty("RedmineQuery"),
                CertificateThumbprint = GetProperty("RedmineCertificateThumbprint"),
                ValidateAnyCertificate = GetBooleanProperty("RedmineValidateAnyCertificate"),
            };

            return settings;
        }

        public void Save(ConnectionSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException("settings");
            }

            SetProperty("RedmineURL", settings.URL);
            SetProperty("RedmineUsername", settings.Username);
            SetProtectedProperty("RedminePassword", settings.Password);
            SetProperty("RedmineQuery", settings.Query);
            SetProperty("RedmineCertificateThumbprint", settings.CertificateThumbprint);
            SetProperty("RedmineValidateAnyCertificate", settings.ValidateAnyCertificate);
        }


        private string GetProperty(string name)
        {
            string value;

            _storage.TryGetProperty(name, out value);

            return value;
        }

        private bool GetBooleanProperty(string name)
        {
            bool value;

            Boolean.TryParse(GetProperty(name), out value);

            return value;
        }


        private string GetProtectedProperty(string name)
        {
            string value;

            if (_storage.TryGetProperty(name, out value))
            {
                value = TryUnprotect(value);
            }

            return value;
        }

        private string TryUnprotect(string value)
        {
            try
            {
                return Unprotect(value);
            }
            catch
            {
                return null;
            }
        }


        private void SetProperty(string name, string value)
        {
            _storage.SetProperty(name, value);
        }

        private void SetProperty(string name, bool value)
        {
            SetProperty(name, value.ToString(CultureInfo.InvariantCulture));
        }
        

        private void SetProtectedProperty(string name, string value)
        {
            SetProperty(name, Protect(value));
        }


        private string Protect(string value)
        {
            var bytes = Encoding.UTF8.GetBytes(value);
            var protectedBytes = ProtectedData.Protect(bytes, entropy, DataProtectionScope.CurrentUser);
            
            return Convert.ToBase64String(protectedBytes);
        }

        private string Unprotect(string value)
        {
            var protectedBytes = Convert.FromBase64String(value);
            var bytes = ProtectedData.Unprotect(protectedBytes, entropy, DataProtectionScope.CurrentUser);

            return Encoding.UTF8.GetString(bytes);
        }
    }
}