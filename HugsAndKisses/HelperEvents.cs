using Microsoft.Xna.Framework.Audio;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using System;
using System.IO;

namespace HugsAndKisses
{
    /// <summary>The mod entry point.</summary>
    public class HelperEvents
    {
        private static IMonitor Monitor;
        private static IModHelper Helper;
        private static ModConfig Config;

        // call this method from your Entry class
        public static void Initialize(IMonitor monitor, ModConfig config, IModHelper helper)
        {
            Monitor = monitor;
            Helper = helper;
            Config = config;
        }

        public static void GameLoop_GameLaunched(object sender, GameLaunchedEventArgs e)
        {
            if (!Config.EnableMod)
                return;
            //Integrations.LoadModApis();

            if(Config.CustomKissSound.Length > 0)
            {
                string filePath = Path.Combine(Helper.DirectoryPath, "assets", $"{Config.CustomKissSound}");
                Monitor.Log("Kissing audio path: " + filePath);
                if (File.Exists(filePath))
                {
                    try
                    {
                        Kissing.kissEffect = SoundEffect.FromStream(new FileStream(filePath, FileMode.Open));
                    }
                    catch (Exception ex)
                    {
                        Monitor.Log("Error loading kissing audio: " + ex, LogLevel.Error);
                    }
                }
                else
                {
                    Monitor.Log("Kissing audio not found at path: " + filePath);
                }
            }
            if(Config.CustomHugSound.Length > 0)
            {
                string filePath = Path.Combine(Helper.DirectoryPath, "assets", $"{Config.CustomKissSound}.wav");
                Monitor.Log("Hug audio path: " + filePath);
                if (File.Exists(filePath))
                {
                    try
                    {
                        Kissing.hugEffect = SoundEffect.FromStream(new FileStream(filePath, FileMode.Open));
                    }
                    catch (Exception ex)
                    {
                        Monitor.Log("Error loading hug audio: " + ex, LogLevel.Error);
                    }
                }
                else
                {
                    Monitor.Log("Hug audio not found at path: " + filePath);
                }
            }
        }

        public static void GameLoop_OneSecondUpdateTicked(object sender, OneSecondUpdateTickedEventArgs e)
        {
            if (!Config.EnableMod)
                return;

            Kissing.TrySpousesKiss(Game1.player.currentLocation);
        }
        public static void GameLoop_SaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            if (!Config.EnableMod)
                return;

            Misc.SetNPCRelations();
        }
    }
}