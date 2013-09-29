using System;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;

namespace RedmineTaskListPackage
{
    public class ProjectPropertyStorage
    {
        private IVsBuildPropertyStorage storage;


        public ProjectPropertyStorage(IServiceProvider provider, string projectPath)
        {
            if (provider == null)
            {
                throw new ArgumentNullException("provider");
            }

            if (projectPath == null)
            {
                throw new ArgumentNullException("projectPath");
            }

            storage = GetPropertyStorage(provider, projectPath);
        }

        private static IVsBuildPropertyStorage GetPropertyStorage(IServiceProvider provider, string projectPath)
        {
            var solution = provider.GetService(typeof(SVsSolution)) as IVsSolution;
            var hierarchy = default(IVsHierarchy);

            solution.GetProjectOfUniqueName(projectPath, out hierarchy);

            return (IVsBuildPropertyStorage)hierarchy;
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
            return storage.GetPropertyValue(name, "", (uint)_PersistStorageType.PST_PROJECT_FILE, out value);
        }


        public void SetProperty(string name, string value)
        {
            ErrorHandler.ThrowOnFailure(SetProjectProperty(name, value));
        }

        private int SetProjectProperty(string name, string value)
        {
            return storage.SetPropertyValue(name, "", (uint)_PersistStorageType.PST_PROJECT_FILE, value);
        }
    }
}