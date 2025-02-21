// RegisterHelper.cs
// This file contains the RegistryHelper class, which provides static methods for reading from and writing to the Windows Registry.
// The class is designed to store and retrieve application-specific information under the "SOFTWARE\\SpeechTranscriberUI" registry key.
// It includes methods to write a string value to the registry and to read a string value from the registry.

using Microsoft.Win32;

namespace SpeechTranscriberUI;

// Helper class for registry operations
internal static class RegistryHelper
{
    // Registry Key
    private const string AppKey = "SOFTWARE\\SpeechTranscriberUI";

    // Write/Read Registry
    public static void WriteAppInfo(string key, string value)
    {
        using (RegistryKey registryKey = Registry.CurrentUser.CreateSubKey(AppKey))
            registryKey.SetValue(key, value);
    }

    public static string? ReadAppInfo(string key)
    {
        using (RegistryKey? registryKey = Registry.CurrentUser.OpenSubKey(AppKey))
            if (registryKey == null)
                return null;
            else
                return (string?)registryKey.GetValue(key);
    }
}