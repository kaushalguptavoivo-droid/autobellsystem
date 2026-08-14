using System;
using System.IO;
using Microsoft.Win32;

namespace AutoBellSystem.Services
{
    public class StartupService
    {
        private const string RegistryKeyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
        private const string AppName = "AutoBellSystem";

        public void SetAutoStart(bool enable)
        {
            try
            {
                using (RegistryKey? key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath, true))
                {
                    if (key != null)
                    {
                        if (enable)
                        {
                            // Path to the current running executable
                            string? exePath = Environment.ProcessPath;
                            if (!string.IsNullOrEmpty(exePath))
                            {
                                key.SetValue(AppName, $"\"{exePath}\" -hidden");
                            }
                        }
                        else
                        {
                            key.DeleteValue(AppName, false);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error setting auto-start: {ex.Message}");
            }
        }
    }
}
