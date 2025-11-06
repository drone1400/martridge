using System.Diagnostics.CodeAnalysis;
namespace Martridge.Models.Steam {
    public static class Constants {
        public enum VdfDataType : byte{
            ObjStart = 0x00,
            String = 0x01,
            Int32 = 0x02,
            Float32 = 0x03,
            Ptr32 = 0x04,
            WString = 0x05,
            Color = 0x06,
            UInt64 = 0x07,
            ObjEnd = 0x08,
        }

        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public enum ShortcutsEntryFields {
            appid,
            AppName,
            Exe,
            StartDir,
            icon,
            ShortcutPath,
            LaunchOptions,
            IsHidden,
            AllowDesktopConfig,
            AllowOverlay,
            OpenVR,
            Devkit,
            DevkitGameID,
            DevkitOverrideAppID,
            LastPlayTime,
            FlatpakAppID,
            sortas,
            tags,
        }
    }
}
