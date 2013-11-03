using System;
using System.Drawing;
using System.Reflection;

namespace RedmineTaskListPackage.ViewModel
{
    public static class EnvironmentColors
    {
        public static object ToolWindowTextBrushKey { get; private set; }
        
        public static object ToolWindowBackgroundBrushKey { get; private set; }

        
        static EnvironmentColors()
        {
            try
            {
                LoadVisualStudioShell11();
            }
            catch
            {
                ToolWindowTextBrushKey = Color.Black;
                ToolWindowBackgroundBrushKey = Color.WhiteSmoke;
            }
        }

        private static void LoadVisualStudioShell11()
        {
            var assembly = Assembly.Load("Microsoft.VisualStudio.Shell.11.0, Version=11.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a, processorArchitecture=MSIL");
            var env = assembly.GetType("Microsoft.VisualStudio.PlatformUI.EnvironmentColors");

            ToolWindowTextBrushKey = GetValue(env, "ToolWindowTextBrushKey");
            ToolWindowBackgroundBrushKey = GetValue(env, "ToolWindowBackgroundBrushKey");
        }

        private static object GetValue(Type type, string name)
        {
            return type.GetProperty(name).GetValue(null, BindingFlags.Static, null, null, null);
        }
    }
}
