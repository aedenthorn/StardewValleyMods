using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;
using System;
using System.IO;
using System.Linq;

namespace MobilePhone
{
    public class PhoneGameLoop
    {
        private static IMonitor Monitor;
        private static IModHelper Helper;
        private static ModConfig Config;

        public static void Initialize(IModHelper helper, IMonitor monitor, ModConfig config)
        {
            Monitor = monitor;
            Helper = helper;
            Config = config;
        }
        public static void GameLoop_GameLaunched(object sender, GameLaunchedEventArgs e)
        {
            foreach (IContentPack contentPack in Helper.ContentPacks.GetOwned())
            {
                Monitor.Log($"Reading content pack: {contentPack.Manifest.Name} {contentPack.Manifest.Version} from {contentPack.DirectoryPath}");
                try
                {
                    MobilePhonePackJSON json = contentPack.ReadJsonFile<MobilePhonePackJSON>("content.json") ?? null;
                    
                    if(json != null)
                    {
                        if (json.apps != null && json.apps.Any())
                        {
                            foreach (AppJSON app in json.apps)
                            {
                                Texture2D tex = contentPack.ModContent.Load<Texture2D>(app.iconPath);
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
                            Texture2D icon = contentPack.ModContent.Load<Texture2D>(json.iconPath);
                            if (icon == null)
                            {
                                continue;
                            }
                            ModEntry.apps.Add(json.id, new MobileApp(json.name, json.keyPress, json.closePhone, icon));
                            Monitor.Log($"Added app {json.name} from {contentPack.DirectoryPath}");
                        }
                        if (json.invites != null && json.invites.Any())
                        {
                            foreach (EventInvite invite in json.invites)
                            {
                                MobilePhoneCall.eventInvites.Add(invite);
                                Monitor.Log($"Added event invite {invite.name} from {contentPack.DirectoryPath}");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Monitor.Log($"error reading content.json file in content pack {contentPack.Manifest.Name}.\r\n{ex}", LogLevel.Error);
                }
                if (Directory.Exists(Path.Combine(contentPack.DirectoryPath, "assets", "events")))
                {
                    Monitor.Log($"Adding events");
                    string[] events = Directory.GetFiles(Path.Combine(contentPack.DirectoryPath, "assets", "events"), "*.json");
                    Monitor.Log($"CP has {events.Length} events");
                    foreach (string eventFile in events)
                    {
                        try
                        {
                            string eventPath = Path.Combine("assets", "events", Path.GetFileName(eventFile));
                            Monitor.Log($"Adding events {Path.GetFileName(eventFile)} from {contentPack.DirectoryPath}");
                            Reminiscence r = contentPack.ReadJsonFile<Reminiscence>(eventPath);
                            var key = Path.GetFileName(eventFile).Replace(".json", "");
                            MobilePhoneCall.contentPackReminiscences.TryAdd(key, new Reminiscence());
                            MobilePhoneCall.contentPackReminiscences[key].events.AddRange(r.events);

                            Monitor.Log($"Added event {Path.GetFileName(eventFile)} from {contentPack.DirectoryPath}");
                        }
                        catch { }
                    }
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
                            Texture2D skin = contentPack.ModContent.Load<Texture2D>(skinPath.Replace("_landscape.png", ".png"));
                            Texture2D skinl = contentPack.ModContent.Load<Texture2D>(skinPath);
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
                            Texture2D back = contentPack.ModContent.Load<Texture2D>(backPath.Replace("_landscape.png", ".png"));
                            Texture2D backl = contentPack.ModContent.Load<Texture2D>(backPath);
                            ThemeApp.backgroundDict.Add(contentPack.Manifest.UniqueID + ":" + Path.GetFileName(backFile).Replace("_landscape.png", ""), new Texture2D[] { back, backl });
                            ThemeApp.backgroundList.Add(contentPack.Manifest.UniqueID + ":" + Path.GetFileName(backFile).Replace("_landscape.png", ""));
                            Monitor.Log($"Added background {Path.GetFileName(backFile).Replace("_landscape.png", "")} from {contentPack.DirectoryPath}");
                        }
                        catch { }
                    }
                }
                if (Directory.Exists(Path.Combine(contentPack.DirectoryPath, "assets", "ringtones")))
                {
                    Monitor.Log($"Adding ringtones");
                    string[] rings = Directory.GetFiles(Path.Combine(contentPack.DirectoryPath, "assets", "ringtones"), "*.wav");
                    Monitor.Log($"CP has {rings.Length} ringtones");
                    foreach (string path in rings)
                    {
                        try
                        {
                            object ring;
                            try
                            {
                                var type = Type.GetType("System.Media.SoundPlayer, System");
                                ring = Activator.CreateInstance(type, new object[] { path });
                            }
                            catch
                            {
                                ring = SoundEffect.FromStream(new FileStream(path, FileMode.Open));
                            }
                            if (ring != null)
                            {
                                ThemeApp.ringDict.Add(string.Concat(contentPack.Manifest.UniqueID,":", Path.GetFileName(path).Replace(".wav", "")), ring);
                                ThemeApp.ringList.Add(string.Concat(contentPack.Manifest.UniqueID,":", Path.GetFileName(path).Replace(".wav", "")));
                                Monitor.Log($"loaded ring {path}");
                            }
                            else
                                Monitor.Log($"Couldn't load ring {path}");
                        }
                        catch (Exception ex)
                        {
                            Monitor.Log($"Couldn't load ring {path}:\r\n{ex}", LogLevel.Error);
                        }
                    }
                }
            }

            ModEntry.listHeight = Config.IconMarginY + (int)Math.Ceiling(ModEntry.apps.Count / (float)ModEntry.gridWidth) * (Config.IconHeight + Config.IconMarginY);
            PhoneVisuals.CreatePhoneTextures();
            PhoneUtils.RefreshPhoneLayout();

            if (Helper.ModRegistry.IsLoaded("purrplingcat.npcadventure"))
            {
                try
                {
                    INpcAdventureModApi api = Helper.ModRegistry.GetApi<INpcAdventureModApi>("purrplingcat.npcadventure");
                    if (api != null)
                    {
                        Monitor.Log("Loaded NpcAdventureModApi successfully");
                        ModEntry.npcAdventureModApi = api;
                    }
                }
                catch { }
            }
            if (Helper.ModRegistry.IsLoaded("tlitookilakin.HDPortraits"))
            {
                try
                {
                    IHDPortraitsAPI api = Helper.ModRegistry.GetApi<IHDPortraitsAPI>("tlitookilakin.HDPortraits");
                    if (api != null)
                    {
                        Monitor.Log("Loaded HD Portraits api successfully");
                        ModEntry.iHDPortraitsAPI = api;
                    }
                }
                catch { }
            }
        }


        public static void GameLoop_DayStarted(object sender, DayStartedEventArgs e)
        {
            ModEntry.calledToday.Clear();
        }

        public static void GameLoop_SaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            Monitor.Log($"total apps: {ModEntry.apps.Count}");
            PhoneUtils.OrderApps();
            PhoneUtils.RefreshPhoneLayout();
            Helper.Events.Display.RenderedWorld += PhoneVisuals.Display_RenderedWorld;
            ModEntry.calledToday.Clear();

            // !Changed code.
            /* Changes:
             * 1. Removed testing 'NPC Adventures API' calling. 
             */

            // Changes end. 
        }

        internal static void ReturnedToTitle(object sender, ReturnedToTitleEventArgs e)
        {
            ModEntry.ClosePhone();
        }

        public static void GameLoop_TimeChanged(object sender, TimeChangedEventArgs e)
        {
            if (e.OldTime >= e.NewTime || ModEntry.callingNPC != null || ModEntry.inCall || !Config.EnableIncomingCalls || (!Config.ReceiveCallsUnderground && Game1.player.currentLocation is MineShaft))
                return;

            if(Game1.random.NextDouble() < Config.FriendCallChance)
            {
                Monitor.Log($"Receiving random call", LogLevel.Debug);
                MobilePhoneApp.ReceiveRandomCall();
            }
        }
        public static void GameLoop_OneSecondUpdateTicked(object sender, OneSecondUpdateTickedEventArgs e)
        {
            if (ModEntry.callingNPC != null && Game1.activeClickableMenu == null)
            {
                if (ModEntry.currentCallRings < ModEntry.currentCallMaxRings && !ModEntry.inCall)
                {
                    if(ModEntry.currentCallRings == 0 && Config.NotifyOnRing)
                        Game1.addHUDMessage(new HUDMessage(Helper.Translation.Get("phone-ringing"), 2));

                    if (ModEntry.ringToggle == Config.PhoneRingInterval)
                    {
                        Monitor.Log($"Phone ringing, {ModEntry.callingNPC.displayName} calling", LogLevel.Debug);
                        PhoneUtils.PlayRingTone();
                        ModEntry.currentCallRings++;
                        ModEntry.ringToggle = 0;
                    }
                    else
                        ModEntry.ringToggle++;
                }
                else
                {
                    PhoneUtils.StopRingTone();
                    if (!ModEntry.inCall)
                        ModEntry.callingNPC = null;
                    ModEntry.currentCallRings = 0;
                }
            }
        }
    }
}
