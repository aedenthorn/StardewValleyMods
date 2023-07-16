using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System;
using System.Text;
using System.Xml;
using StardewValley.Buildings;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace OpenFolder
{
    public partial class ModEntry
    {
        private static void TryOpenFolder(string folder)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = folder,
                    UseShellExecute = true,
                    Verb = "open"
                });
            }
            catch(Exception ex)
            {
                SMonitor.Log($"Error opening folder {folder}: \n\n{ex}");
            }
        }

    }
}