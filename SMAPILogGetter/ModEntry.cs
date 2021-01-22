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

namespace CustomChestTypes
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
			string logPath = Path.Combine(Constants.DataPath, "SMAPI-latest.txt");
			
			if (!Directory.Exists(logPath))
			{
				Monitor.Log($"SMAPI log not found at {logPath}.", LogLevel.Error);
				return;
			}
			File.Copy(logPath, Helper.DirectoryPath);
			Monitor.Log($"SMAPI log not found at {logPath}.", LogLevel.Error);
		}

	}
}
