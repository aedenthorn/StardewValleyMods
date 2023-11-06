using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MobilePhone.Api;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Network;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MobilePhone
{
    public class ModEntry : Mod
    {
        public static ModEntry context;

        public static ModConfig Config;
        public static IModHelper SHelper;
        public static IMonitor SMonitor;

        public static bool phoneOpen;
        public static bool phoneRotated;
        public static bool appRunning;
        public static bool phoneAppRunning;

        public static Texture2D phoneTexture;
        public static Texture2D backgroundTexture;
        public static Texture2D phoneRotatedTexture;
        public static Texture2D backgroundRotatedTexture;
        public static Texture2D upArrowTexture;
        public static Texture2D downArrowTexture;
        public static Texture2D iconTexture;

        public static int phoneWidth;
        public static int phoneHeight;
        public static int screenWidth;
        public static int screenHeight;
        public static int phoneOffsetX;
        public static int phoneOffsetY;
        public static int screenOffsetX;
        public static int screenOffsetY;
        public static Rectangle screenRect;
        public static Rectangle phoneRect;
        public static Vector2 phonePosition;
        public static Vector2 phoneIconPosition;
        public static Vector2 screenPosition;
        public static Vector2 upArrowPosition;
        public static Vector2 downArrowPosition;

        public static MobilePhoneApi api;
        public static int appColumns;
        public static int appRows;
        public static int gridWidth;
        public static int gridHeight;
        public static int themeGridWidth;
        public static int themeGridHeight;

        public static Texture2D phoneBookTexture;
        public static Texture2D phoneBookHeaderTexture;

        public static string npcDictPath = "aedenthorn.MobilePhone/npcs";

        public static Dictionary<string, MobileApp> apps = new Dictionary<string, MobileApp>();
        public static List<string> appOrder;
        public static string runningApp;
        public static int listHeight;

        public static bool clicking;
        public static bool draggingPhone;
        public static bool draggingIcons;
        public static bool clickingPhoneIcon;
        public static bool draggingPhoneIcon;
        public static bool movingAppIcon;

        public static Point lastMousePosition;
        public static Point movingAppIconOffset;

        public static float yOffset;

        public static int clickingApp = -1;
        public static int switchingApp = -1;

        public static int clickingTicks;
        public static int ticksSinceMoved;

        public static Texture2D themesHighlightTexture;
        public static Texture2D themesHeaderTexture;
        public static Texture2D answerTexture;
        public static Texture2D declineTexture;

        public static int currentCallRings;
        public static int currentCallMaxRings;

        public static int ringToggle;
        public static NPC callingNPC;
        public static bool inCall;
        public static object ringSound;
        public static object notificationSound;
        public static bool isReminiscing;
        public static bool isInviting;
        public static INpcAdventureModApi npcAdventureModApi;
        public static IHDPortraitsAPI iHDPortraitsAPI;

        public static LocationRequest callLocation;
        public static NetPosition callPosition;
        public static xTile.Dimensions.Location callViewportLocation;
        public static NPC invitedNPC;
        public static Texture2D ringListBackgroundTexture;
        public static Texture2D ringListHighlightTexture;
        public static bool isReminiscingAtNight;
        public static Event reminisceEvent;
        public static bool buildingInCall;
        
        public static List<string> calledToday = new List<string>();

        public static event EventHandler OnScreenRotated;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            context = this;
            Config = helper.ReadConfig<ModConfig>();
            SHelper = helper;
            SMonitor = Monitor;
            if (!Config.EnableMod)
                return;

            api = (MobilePhoneApi)GetApi();

            MobilePhoneApp.Initialize(Helper, Monitor, Config);
            MobilePhoneCall.Initialize(Helper, Monitor, Config);
            ThemeApp.Initialize(Helper, Monitor, Config);
            PhoneVisuals.Initialize(Helper, Monitor, Config);
            PhoneInput.Initialize(Helper, Monitor, Config);
            PhoneGameLoop.Initialize(Helper, Monitor, Config);
            PhoneUtils.Initialize(Helper, Monitor, Config);
            PhonePatches.Initialize(Helper, Monitor, Config);

            if(Config.AddRotateApp)
                apps.Add(Helper.ModRegistry.ModID+"_Rotate", new MobileApp(helper.Translation.Get("rotate-phone"), null, helper.ModContent.Load<Texture2D>("assets/rotate_icon.png")));

            var harmony = new Harmony(ModManifest.UniqueID);

            harmony.Patch(
                original: AccessTools.Method(typeof(Game1), nameof(Game1.pressSwitchToolButton)),
                prefix: new HarmonyMethod(typeof(PhonePatches), nameof(PhonePatches.Game1_pressSwitchToolButton_prefix))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(Event), nameof(Event.endBehaviors)),
                prefix: new HarmonyMethod(typeof(PhonePatches), nameof(PhonePatches.Event_endBehaviors_prefix))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(Farmer), nameof(Farmer.addItemToInventory), new Type[] {typeof(Item), typeof(List<Item>) }),
                prefix: new HarmonyMethod(typeof(PhonePatches), nameof(PhonePatches.Farmer_addItemToInventory_prefix))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(Farmer), nameof(Farmer.changeFriendship)),
                prefix: new HarmonyMethod(typeof(PhonePatches), nameof(PhonePatches.Farmer_changeFriendship_prefix))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(GameLocation), "resetLocalState"),
                postfix: new HarmonyMethod(typeof(PhonePatches), nameof(PhonePatches.GameLocation_resetLocalState_postfix))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.answerDialogue)),
                prefix: new HarmonyMethod(typeof(PhonePatches), nameof(PhonePatches.GameLocation_answerDialogue_prefix))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(Event), nameof(Event.DefaultCommands.AwardFestivalPrize)),
                prefix: new HarmonyMethod(typeof(PhonePatches), nameof(PhonePatches.Event_command_prefix))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(Event), nameof(Event.DefaultCommands.AddTool)),
                prefix: new HarmonyMethod(typeof(PhonePatches), nameof(PhonePatches.Event_command_prefix))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(Event), nameof(Event.DefaultCommands.AddConversationTopic)),
                prefix: new HarmonyMethod(typeof(PhonePatches), nameof(PhonePatches.Event_command_prefix))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(Event), nameof(Event.DefaultCommands.AddCookingRecipe)),
                prefix: new HarmonyMethod(typeof(PhonePatches), nameof(PhonePatches.Event_command_prefix))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(Event), nameof(Event.DefaultCommands.AddCraftingRecipe)),
                prefix: new HarmonyMethod(typeof(PhonePatches), nameof(PhonePatches.Event_command_prefix))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(Event), nameof(Event.DefaultCommands.Money)),
                prefix: new HarmonyMethod(typeof(PhonePatches), nameof(PhonePatches.Event_command_prefix))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(Event), nameof(Event.DefaultCommands.RemoveItem)),
                prefix: new HarmonyMethod(typeof(PhonePatches), nameof(PhonePatches.Event_command_prefix))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(Event), nameof(Event.DefaultCommands.Friendship)),
                prefix: new HarmonyMethod(typeof(PhonePatches), nameof(PhonePatches.Event_command_prefix))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(Event), nameof(Event.DefaultCommands.Dump)),
                prefix: new HarmonyMethod(typeof(PhonePatches), nameof(PhonePatches.Event_command_prefix))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(Event), nameof(Event.DefaultCommands.AddQuest)),
                prefix: new HarmonyMethod(typeof(PhonePatches), nameof(PhonePatches.Event_command_prefix))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(Event), nameof(Event.DefaultCommands.Cutscene)),
                prefix: new HarmonyMethod(typeof(PhonePatches), nameof(PhonePatches.Event_command_cutscene_prefix))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(Event), nameof(Event.skipEvent)),
                prefix: new HarmonyMethod(typeof(PhonePatches), nameof(PhonePatches.Event_skipEvent_prefix))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(Event), "namePet"),
                prefix: new HarmonyMethod(typeof(PhonePatches), nameof(PhonePatches.Event_namePet_prefix))
            );

            harmony.Patch(
                original: AccessTools.Method(typeof(CarpenterMenu), nameof(CarpenterMenu.returnToCarpentryMenu)),
                prefix: new HarmonyMethod(typeof(PhonePatches), nameof(PhonePatches.CarpenterMenu_returnToCarpentryMenu_prefix))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(CarpenterMenu), nameof(CarpenterMenu.returnToCarpentryMenuAfterSuccessfulBuild)),
                prefix: new HarmonyMethod(typeof(PhonePatches), nameof(PhonePatches.CarpenterMenu_returnToCarpentryMenuAfterSuccessfulBuild_prefix))
            );

            Helper.Events.Input.ButtonPressed += PhoneInput.Input_ButtonPressed;
            Helper.Events.GameLoop.SaveLoaded += PhoneGameLoop.GameLoop_SaveLoaded;
            Helper.Events.GameLoop.GameLaunched += PhoneGameLoop.GameLoop_GameLaunched;
            Helper.Events.GameLoop.ReturnedToTitle += PhoneGameLoop.ReturnedToTitle;
            Helper.Events.GameLoop.DayStarted += PhoneGameLoop.GameLoop_DayStarted; 
            Helper.Events.GameLoop.TimeChanged += PhoneGameLoop.GameLoop_TimeChanged;
            Helper.Events.GameLoop.OneSecondUpdateTicked += PhoneGameLoop.GameLoop_OneSecondUpdateTicked;
            Helper.Events.Display.WindowResized += PhoneVisuals.Display_WindowResized;

            Helper.Events.Content.AssetRequested += Content_AssetRequested;
            /*
            var files = Directory.GetFiles(Path.Combine(Helper.DirectoryPath, Path.Combine("assets", "events")));
            foreach(string file in files)
            {
                Reminiscence r = Helper.Data.ReadJsonFile<Reminiscence>(Path.Combine("assets", Path.Combine("events", Path.GetFileName(file))));
                for(int i = 0; i < r.events.Count; i++)
                {
                    string t = $"{Path.GetFileName(file).Replace(".json", "")}-event-{i}";
                    Monitor.Log($"\"{t}\":\"{r.events[i].name}\"");
                    r.events[i].name = t;
                }
                Helper.Data.WriteJsonFile(Path.Combine("tmp", Path.GetFileName(file)),r);
            }
            */
        }

        private void Content_AssetRequested(object sender, StardewModdingAPI.Events.AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo(npcDictPath))
            {
                e.LoadFrom(() => new Dictionary<string, CustomNPCData>(), StardewModdingAPI.Events.AssetLoadPriority.Exclusive);
            }
            else if (e.NameWithoutLocale.BaseName.Contains("Events") && isInviting && invitedNPC is not null)
            {
                foreach (EventInvite invite in MobilePhoneCall.eventInvites)
                {
                    if (invite.CanInvite(invitedNPC) && invite.forks?.Any() == true && e.NameWithoutLocale.IsEquivalentTo($"Data/Events/{invite.location}"))
                    {
                        foreach (EventFork fork in invite.forks)
                        {
                            var f = fork;
                            e.Edit(delegate(IAssetData obj) {
                                var dict = obj.AsDictionary<string, string>();
                                dict.Data.Add(fork.key, MobilePhoneCall.CreateEventString(fork.nodes, invitedNPC));

                            });
                        }
                    }
                }
            }
        }

        public override object GetApi()
        {
            return new MobilePhoneApi();
        }

        public static void ClosePhone()
        {
            phoneOpen = false;
            appRunning = false;
            runningApp = null;
        }

    }
}
