using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Win32;
namespace Martridge.Models {
    [SuppressMessage("Interoperability", "CA1416:Validate platform compatibility")]
    public static class WindowsHelper {
        public static object? ReadRegistryKeyLocalMachine(string keyPath, string valueName) {
            try {
                using RegistryKey? key = Registry.LocalMachine.OpenSubKey(keyPath);
                object? value = key?.GetValue(valueName);
                return value;
            } catch (Exception) {
                return null;
            }
        }
        
        public static object? ReadRegistryKeyCurrentUser(string keyPath, string valueName) {
            try {
                using RegistryKey? key = Registry.CurrentUser.OpenSubKey(keyPath);
                object? value = key?.GetValue(valueName);
                return value;
            } catch (Exception) {
                return null;
            }
        }
    }
}
