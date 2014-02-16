using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using System;

namespace RedmineTaskListPackage
{
    public class ProjectPropertyStorage
    {
        private const uint storageType = (uint)_PersistStorageType.PST_USER_FILE;
        private IVsBuildPropertyStorage _storage;

        public ProjectPropertyStorage(IVsBuildPropertyStorage storage)
        {
            if (storage == null)
            {
                throw new ArgumentNullException("storage");
            }

            _storage = storage;
        }


        public bool TryGetProperty(string name, out string value)
        {
            bool result = true;

            try
            {
                value = GetProperty(name);
            }
            catch
            {
                value = null;
                result = false;
            }

            return result;
        }

        public string GetProperty(string name)
        {
            string value;

            ErrorHandler.ThrowOnFailure(GetProjectProperty(name, out value));

            return value;
        }

        private int GetProjectProperty(string name, out string value)
        {
            return _storage.GetPropertyValue(name, "", storageType, out value);
        }


        public void SetProperty(string name, string value)
        {
            ErrorHandler.ThrowOnFailure(SetProjectProperty(name, value));
        }

        private int SetProjectProperty(string name, string value)
        {
            return _storage.SetPropertyValue(name, "", storageType, value ?? "");
        }
    }
}