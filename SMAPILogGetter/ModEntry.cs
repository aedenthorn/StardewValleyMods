using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Network;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Object = StardewValley.Object;

namespace SMAPILogGetter
{
    public class ModEntry : Mod
	{
		public static ModEntry context;
		private static ModConfig Config;

        public override void Entry(IModHelper helper)
		{
            context = this;
			Config = Helper.ReadConfig<ModConfig>();
			if (!Config.EnableMod)
				return;

			Helper.ConsoleCommands.Add("GetLogFile", "Get SMAPI log file.", new Action<string, string[]>(GetLogFile));
			Helper.ConsoleCommands.Add("glf", "Get SMAPI log file.", new Action<string, string[]>(GetLogFile));

		}

        private void GetLogFile(string arg1, string[] arg2)
        {
			string logPath = Path.Combine(Constants.DataPath, "ErrorLogs", "SMAPI-latest.txt");
			
			if (!File.Exists(logPath))
			{
				Monitor.Log($"SMAPI log not found at {logPath}.", LogLevel.Error);
				return;
			}

			if (arg2.Length == 0)
			{
				File.Copy(logPath, Environment.CurrentDirectory);
				Monitor.Log($"Copied SMAPI log to game folder {Environment.CurrentDirectory}.", LogLevel.Alert);

			}
			else 
			{
				string cmd = arg2[0].ToLower();
                switch (cmd)
                {
					case "desktop":
					case "dt":
						File.Copy(logPath, Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
						Monitor.Log($"Copied SMAPI log to Desktop {Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}.", LogLevel.Alert);
						break;
					case "copy":
					case "cp":
						string log = File.ReadAllText(logPath);
                        if (DesktopClipboard.SetText(log))
                        {
							Monitor.Log($"Copied SMAPI log to Clipboard.", LogLevel.Alert);
						}
						else
						{
							Monitor.Log($"Coulding copy SMAPI log to Clipboard!", LogLevel.Error);
						}

						break;
				}
			}

		}

	}
}
