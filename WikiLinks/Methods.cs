using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace WikiLinks
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
            SMonitor.Log($"Opening wiki page for {page}");

            if (Config.SendToBack)
                SetWindowPos(Process.GetCurrentProcess().MainWindowHandle, HWND_BOTTOM, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE);

                //ShowWindow(Process.GetCurrentProcess().MainWindowHandle, SW_MINIMIZE);

            var ps = new ProcessStartInfo($"https://stardewvalleywiki.com/{page}")
            {
                UseShellExecute = true,
                Verb = "open"
            };
            Process.Start(ps);
        }

    }
}