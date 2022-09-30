using HarmonyLib;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace LogUploader
{
    public partial class ModEntry
    {
        [DllImport("user32.dll")]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        const UInt32 SWP_NOSIZE = 0x0001;
        const UInt32 SWP_NOMOVE = 0x0002;

        static readonly IntPtr HWND_BOTTOM = new IntPtr(1);

        private const int SW_MAXIMIZE = 3;
        private const int SW_MINIMIZE = 6;

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private static void OpenPage(string page)
        {
            SMonitor.Log($"Opening temp log page");

            if (Config.OpenBehind)
                SetWindowPos(Process.GetCurrentProcess().MainWindowHandle, HWND_BOTTOM, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE);

                //ShowWindow(Process.GetCurrentProcess().MainWindowHandle, SW_MINIMIZE);

            var ps = new ProcessStartInfo(page)
            {
                UseShellExecute = true,
                Verb = "open"
            };
            Process.Start(ps);
        }
        private static void SendLog(object lm)
        {
            object lfm = AccessTools.Field(lm.GetType(), "LogFile").GetValue(lm);
            StreamWriter Stream = (StreamWriter)AccessTools.Field(lfm.GetType(), "Stream").GetValue(lfm);
            Stream.Close();
            string path = (string)AccessTools.Property(lfm.GetType(), "Path").GetValue(lfm);
            string log = File.ReadAllText(path);
            AccessTools.Field(lfm.GetType(), "Stream").SetValue(lfm, new StreamWriter(path, true) { AutoFlush = true });
            
            string html = File.ReadAllText(Path.Combine(SHelper.DirectoryPath, "assets", "log.html")).Replace("{{HERE}}", log);
            string tempPath = Path.Combine(SHelper.DirectoryPath, "temp", "log.html");
            Directory.CreateDirectory(Path.Combine(SHelper.DirectoryPath, "temp"));
            File.WriteAllText(tempPath, html);
            var uri = new Uri(tempPath);
            var converted = uri.AbsoluteUri;
            OpenPage(converted);
        }

    }
}