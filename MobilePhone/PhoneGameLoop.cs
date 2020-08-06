using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace MobilePhone
{
    public class PhoneGameLoop
    {
        private static IMonitor Monitor;
        private static IModHelper Helper;
        private static ModConfig Config;

        // call this method from your Entry class
        public static void Initialize(IModHelper helper, IMonitor monitor, ModConfig config)
        {
            Monitor = monitor;
            Helper = helper;
            Config = config;
        }
        public static void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {
            foreach (IContentPack contentPack in Helper.ContentPacks.GetOwned())
            {
                Monitor.Log($"Reading content pack: {contentPack.Manifest.Name} {contentPack.Manifest.Version} from {contentPack.DirectoryPath}");
                try
                {
                    MobileAppJSON json = contentPack.ReadJsonFile<MobileAppJSON>("content.json") ?? null;
                    
                    if(json != null)
                    {
                        if (json.apps != null && json.apps.Count > 0)
                        {
                            foreach (AppJSON app in json.apps)
                            {
                                Texture2D tex = contentPack.LoadAsset<Texture2D>(app.iconPath);
                                if (tex == null)
                                {
                                    continue;
                                }
                                ModEntry.apps.Add(app.id, new MobileApp(app.name, app.keyPress, app.closePhone, tex));
                                Monitor.Log($"Added app {app.name} from {contentPack.DirectoryPath}");
                            }
                        }
                        else if (json.iconPath != null)
                        {
                            Texture2D icon = contentPack.LoadAsset<Texture2D>(json.iconPath);
                            if (icon == null)
                            {
                                continue;
                            }
                            ModEntry.apps.Add(json.id, new MobileApp(json.name, json.keyPress, json.closePhone, icon));
                            Monitor.Log($"Added app {json.name} from {contentPack.DirectoryPath}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Monitor.Log($"error reading content.json file in content pack {contentPack.Manifest.Name}.\r\n{ex}", LogLevel.Error);
                }
                if (Directory.Exists(Path.Combine(contentPack.DirectoryPath, "assets", "skins")))
                {
                    Monitor.Log($"Adding skins");
                    string[] skins = Directory.GetFiles(Path.Combine(contentPack.DirectoryPath, "assets", "skins"), "*_landscape.png");
                    Monitor.Log($"CP has {skins.Length} skins");
                    foreach (string skinFile in skins)
                    {
                        try
                        {
                            string skinPath = Path.Combine("assets", "skins", Path.GetFileName(skinFile));
                            Monitor.Log($"Adding skin {Path.GetFileName(skinFile).Replace("_landscape.png", "")} from {contentPack.DirectoryPath}");
                            Texture2D skin = contentPack.LoadAsset<Texture2D>(skinPath.Replace("_landscape.png", ".png"));
                            Texture2D skinl = contentPack.LoadAsset<Texture2D>(skinPath.Replace("_landscape.png", ".png"));
                            ThemeApp.skinList.Add(contentPack.Manifest.UniqueID + ":" + Path.GetFileName(skinFile).Replace("_landscape.png", ""));
                            ThemeApp.skinDict.Add(contentPack.Manifest.UniqueID + ":" + Path.GetFileName(skinFile).Replace("_landscape.png", ""), new Texture2D[] { skin, skinl});
                            Monitor.Log($"Added skin {Path.GetFileName(skinFile).Replace("_landscape.png", "")} from {contentPack.DirectoryPath}");
                        }
                        catch { }
                    }
                }
                if (Directory.Exists(Path.Combine(contentPack.DirectoryPath, "assets", "backgrounds")))
                {
                    Monitor.Log($"Adding backgrounds");
                    string[] backgrounds = Directory.GetFiles(Path.Combine(contentPack.DirectoryPath, "assets", "backgrounds"), "*_landscape.png");
                    Monitor.Log($"CP has {backgrounds.Length} backgrounds");
                    foreach (string backFile in backgrounds)
                    {
                        try
                        {
                            string backPath = Path.Combine("assets", "backgrounds", Path.GetFileName(backFile));
                            Monitor.Log($"Adding background {Path.GetFileName(backFile).Replace("_landscape.png", "")} from {contentPack.DirectoryPath}");
                            Texture2D back = contentPack.LoadAsset<Texture2D>(backPath.Replace("_landscape.png", ".png"));
                            Texture2D backl = contentPack.LoadAsset<Texture2D>(backPath);
                            ThemeApp.backgroundDict.Add(contentPack.Manifest.UniqueID + ":" + Path.GetFileName(backFile).Replace("_landscape.png", ""), new Texture2D[] { back, backl });
                            ThemeApp.backgroundList.Add(contentPack.Manifest.UniqueID + ":" + Path.GetFileName(backFile).Replace("_landscape.png", ""));
                            Monitor.Log($"Added background {Path.GetFileName(backFile).Replace("_landscape.png", "")} from {contentPack.DirectoryPath}");
                        }
                        catch { }
                    }
                }
            }
            ModEntry.listHeight = Config.IconMarginY + (int)Math.Ceiling(ModEntry.apps.Count / (float)ModEntry.gridWidth) * (Config.IconHeight + Config.IconMarginY);
            PhoneVisuals.CreatePhoneTextures();
            PhoneUtils.RefreshPhoneLayout();
            PhoneUtils.CreateTones();
        }



        public static void GameLoop_SaveLoaded(object sender, StardewModdingAPI.Events.SaveLoadedEventArgs e)
        {
            Monitor.Log($"total apps: {ModEntry.apps.Count}");
            PhoneUtils.OrderApps();
            PhoneUtils.RefreshPhoneLayout();
            Helper.Events.Display.RenderedWorld += PhoneVisuals.Display_RenderedWorld;
        }
        public static void GameLoop_TimeChanged(object sender, TimeChangedEventArgs e)
        {
            if (e.OldTime >= e.NewTime || ModEntry.callingNPC != null || !Config.EnableIncomingCalls)
                return;

            if(Game1.random.NextDouble() < Config.FriendCallChance)
            {
                Monitor.Log($"Receiving random call", LogLevel.Debug);
                MobilePhoneApp.ReceiveRandomCall();
            }
        }
        public static void GameLoop_OneSecondUpdateTicked(object sender, OneSecondUpdateTickedEventArgs e)
        {
            if (ModEntry.callingNPC != null)
            {
                if (ModEntry.currentCallRings < ModEntry.currentCallMaxRings && !ModEntry.inCall)
                {
                    if (ModEntry.ringToggle == Config.PhoneRingInterval)
                    {
                        Monitor.Log($"Phone ringing, {ModEntry.callingNPC.displayName} calling", LogLevel.Debug);
                        Game1.playSound(Config.PhoneRingTone);
                        ModEntry.currentCallRings++;
                        ModEntry.ringToggle = 0;
                    }
                    else
                        ModEntry.ringToggle++;
                }
                else
                {
                    if(!ModEntry.inCall)
                        ModEntry.callingNPC = null;
                    ModEntry.currentCallRings = 0;
                }
            }
        }
    }
}
