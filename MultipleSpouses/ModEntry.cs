using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Events;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Network;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using xTile;
using Object = StardewValley.Object;

namespace MultipleSpouses
{
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod, IAssetLoader, IAssetEditor
    {

        public static IMonitor PMonitor;
        public static IModHelper PHelper;
        public static ModConfig config;

        public static string spouseToDivorce = null;
        public static int divorceHeartsLost;
        public static Multiplayer mp;
        public static Random myRand;
        public static int bedSleepOffset = 76;
        public static string farmHelperSpouse = null;
        internal static NPC tempOfficialSpouse;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            config = Helper.ReadConfig<ModConfig>();

            if (!config.EnableMod)
                return;

            PMonitor = Monitor;
            PHelper = helper;

            mp = helper.Reflection.GetField<Multiplayer>(typeof(Game1), "multiplayer").GetValue();
            myRand = new Random();

            helper.Events.GameLoop.GameLaunched += HelperEvents.GameLoop_GameLaunched;
            helper.Events.GameLoop.SaveLoaded += HelperEvents.GameLoop_SaveLoaded;
            helper.Events.Input.ButtonPressed += HelperEvents.Input_ButtonPressed;
            helper.Events.GameLoop.DayStarted += HelperEvents.GameLoop_DayStarted;
            helper.Events.GameLoop.DayEnding += HelperEvents.GameLoop_DayEnding;
            helper.Events.GameLoop.ReturnedToTitle += HelperEvents.GameLoop_ReturnedToTitle;

            NPCPatches.Initialize(Monitor, config);
            LocationPatches.Initialize(Monitor);
            FarmerPatches.Initialize(Monitor, Helper);
            Maps.Initialize(Monitor);
            Kissing.Initialize(Monitor);
            UIPatches.Initialize(Monitor, Helper);
            EventPatches.Initialize(Monitor, Helper);
            HelperEvents.Initialize(Monitor, Helper);
            FileIO.Initialize(Monitor, Helper);
            Misc.Initialize(Monitor, Helper, config);
            Divorce.Initialize(Monitor, Helper);
            FurniturePatches.Initialize(Monitor, Helper, config);
            ObjectPatches.Initialize(Monitor, Helper, config);
            NetWorldStatePatches.Initialize(Monitor, Helper, config);

            var harmony = new Harmony(ModManifest.UniqueID);


            // npc patches

            harmony.Patch(
               original: AccessTools.Method(typeof(NPC), nameof(NPC.marriageDuties)),
               postfix: new HarmonyMethod(typeof(NPCPatches), nameof(NPCPatches.NPC_marriageDuties_Postfix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(NPC), nameof(NPC.getSpouse)),
               prefix: new HarmonyMethod(typeof(NPCPatches), nameof(NPCPatches.NPC_getSpouse_Prefix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(NPC), nameof(NPC.isRoommate)),
               prefix: new HarmonyMethod(typeof(NPCPatches), nameof(NPCPatches.NPC_isRoommate_Prefix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(NPC), nameof(NPC.isMarried)),
               prefix: new HarmonyMethod(typeof(NPCPatches), nameof(NPCPatches.NPC_isMarried_Prefix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(NPC), nameof(NPC.isMarriedOrEngaged)),
               prefix: new HarmonyMethod(typeof(NPCPatches), nameof(NPCPatches.NPC_isMarriedOrEngaged_Prefix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(NPC), nameof(NPC.tryToReceiveActiveObject)),
               prefix: new HarmonyMethod(typeof(NPCPatches), nameof(NPCPatches.NPC_tryToReceiveActiveObject_Prefix)),
               transpiler: new HarmonyMethod(typeof(NPCPatches), nameof(NPCPatches.NPC_tryToReceiveActiveObject_Transpiler)),
               postfix: new HarmonyMethod(typeof(NPCPatches), nameof(NPCPatches.NPC_tryToReceiveActiveObject_Postfix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(NPC), nameof(NPC.checkAction)),
               prefix: new HarmonyMethod(typeof(NPCPatches), nameof(NPCPatches.NPC_checkAction_Prefix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(NPC), nameof(NPC.spouseObstacleCheck)),
               postfix: new HarmonyMethod(typeof(NPCPatches), nameof(NPCPatches.NPC_spouseObstacleCheck_Postfix))
            );


            harmony.Patch(
               original: AccessTools.Method(typeof(NPC), "engagementResponse"),
               prefix: new HarmonyMethod(typeof(NPCPatches), nameof(NPCPatches.NPC_engagementResponse_Prefix)),
               postfix: new HarmonyMethod(typeof(NPCPatches), nameof(NPCPatches.NPC_engagementResponse_Postfix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(NPC), nameof(NPC.playSleepingAnimation)),
               postfix: new HarmonyMethod(typeof(NPCPatches), nameof(NPCPatches.NPC_playSleepingAnimation_Postfix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(NPC), nameof(NPC.GetDispositionModifiedString)),
               prefix: new HarmonyMethod(typeof(NPCPatches), nameof(NPCPatches.NPC_GetDispositionModifiedString_Prefix)),
               postfix: new HarmonyMethod(typeof(NPCPatches), nameof(NPCPatches.NPC_GetDispositionModifiedString_Postfix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(NPC), "loadCurrentDialogue"),
               prefix: new HarmonyMethod(typeof(NPCPatches), nameof(NPCPatches.NPC_loadCurrentDialogue_Prefix)),
               postfix: new HarmonyMethod(typeof(NPCPatches), nameof(NPCPatches.NPC_loadCurrentDialogue_Postfix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(NPC), nameof(NPC.tryToRetrieveDialogue)),
               prefix: new HarmonyMethod(typeof(NPCPatches), nameof(NPCPatches.NPC_tryToRetrieveDialogue_Prefix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(NPC), nameof(NPC.setSpouseRoomMarriageDialogue)),
               prefix: new HarmonyMethod(typeof(NPCPatches), nameof(NPCPatches.NPC_setSpouseRoomMarriageDialogue_Prefix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(NPC), nameof(NPC.setRandomAfternoonMarriageDialogue)),
               prefix: new HarmonyMethod(typeof(NPCPatches), nameof(NPCPatches.NPC_setRandomAfternoonMarriageDialogue_Prefix))
            );


            // Child patches

            harmony.Patch(
               original: typeof(Character).GetProperty("displayName").GetMethod,
               postfix: new HarmonyMethod(typeof(NPCPatches), nameof(NPCPatches.Character_displayName_Getter_Postfix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(Child), nameof(Child.reloadSprite)),
               postfix: new HarmonyMethod(typeof(NPCPatches), nameof(NPCPatches.Child_reloadSprite_Postfix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(Child), nameof(Child.resetForPlayerEntry)),
               postfix: new HarmonyMethod(typeof(NPCPatches), nameof(NPCPatches.Child_resetForPlayerEntry_Postfix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(Child), nameof(Child.dayUpdate)),
               prefix: new HarmonyMethod(typeof(NPCPatches), nameof(NPCPatches.Child_dayUpdate_Prefix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(Child), nameof(Child.isInCrib)),
               prefix: new HarmonyMethod(typeof(NPCPatches), nameof(NPCPatches.Child_isInCrib_Prefix))
            );
            /*
            harmony.Patch(
               original: AccessTools.Method(typeof(Child), nameof(Child.tenMinuteUpdate)),
               postfix: new HarmonyMethod(typeof(NPCPatches), nameof(NPCPatches.Child_tenMinuteUpdate_Postfix))
            );
            */

            // Location patches

            harmony.Patch(
               original: AccessTools.Method(typeof(Beach), nameof(Beach.checkAction)),
               prefix: new HarmonyMethod(typeof(LocationPatches), nameof(LocationPatches.Beach_checkAction_Prefix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(ManorHouse), nameof(ManorHouse.performAction)),
               prefix: new HarmonyMethod(typeof(LocationPatches), nameof(LocationPatches.ManorHouse_performAction_Prefix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(FarmHouse), nameof(FarmHouse.checkAction)),
               postfix: new HarmonyMethod(typeof(LocationPatches), nameof(LocationPatches.FarmHouse_checkAction_Postfix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(FarmHouse), nameof(FarmHouse.updateFarmLayout)),
               postfix: new HarmonyMethod(typeof(LocationPatches), nameof(LocationPatches.FarmHouse_updateFarmLayout_Postfix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(FarmHouse), nameof(FarmHouse.getWalls)),
               postfix: new HarmonyMethod(typeof(LocationPatches), nameof(LocationPatches.FarmHouse_getWalls_Postfix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(FarmHouse), nameof(FarmHouse.getFloors)),
               postfix: new HarmonyMethod(typeof(LocationPatches), nameof(LocationPatches.FarmHouse_getFloors_Postfix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.performAction)),
               prefix: new HarmonyMethod(typeof(LocationPatches), nameof(LocationPatches.GameLocation_performAction_Prefix))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.answerDialogue)),
               prefix: new HarmonyMethod(typeof(LocationPatches), nameof(LocationPatches.GameLocation_answerDialogue_prefix))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(FarmHouse), "resetLocalState"),
               postfix: new HarmonyMethod(typeof(LocationPatches), nameof(LocationPatches.FarmHouse_resetLocalState_Postfix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(Beach), "resetLocalState"),
               postfix: new HarmonyMethod(typeof(LocationPatches), nameof(LocationPatches.Beach_resetLocalState_Postfix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(GameLocation), "checkEventPrecondition"),
               prefix: new HarmonyMethod(typeof(LocationPatches), nameof(LocationPatches.GameLocation_checkEventPrecondition_Prefix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(FarmHouse), nameof(FarmHouse.performTenMinuteUpdate)),
               postfix: new HarmonyMethod(typeof(LocationPatches), nameof(LocationPatches.FarmHouse_performTenMinuteUpdate_Postfix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(Desert), nameof(Desert.getDesertMerchantTradeStock)),
               postfix: new HarmonyMethod(typeof(LocationPatches), nameof(LocationPatches.Desert_getDesertMerchantTradeStock_Postfix))
            );



            // pregnancy patches

            harmony.Patch(
               original: AccessTools.Method(typeof(Utility), nameof(Utility.pickPersonalFarmEvent)),
               prefix: new HarmonyMethod(typeof(Pregnancy), nameof(Pregnancy.Utility_pickPersonalFarmEvent_Prefix))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(QuestionEvent), nameof(QuestionEvent.setUp)),
               prefix: new HarmonyMethod(typeof(Pregnancy), nameof(Pregnancy.QuestionEvent_setUp_Prefix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(BirthingEvent), nameof(BirthingEvent.setUp)),
               prefix: new HarmonyMethod(typeof(Pregnancy), nameof(Pregnancy.BirthingEvent_setUp_Prefix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(BirthingEvent), nameof(BirthingEvent.tickUpdate)),
               prefix: new HarmonyMethod(typeof(Pregnancy), nameof(Pregnancy.BirthingEvent_tickUpdate_Prefix))
            );


            // Farmer patches

            harmony.Patch(
               original: AccessTools.Method(typeof(Farmer), nameof(Farmer.doDivorce)),
               prefix: new HarmonyMethod(typeof(FarmerPatches), nameof(FarmerPatches.Farmer_doDivorce_Prefix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(Farmer), nameof(Farmer.isMarried)),
               prefix: new HarmonyMethod(typeof(FarmerPatches), nameof(FarmerPatches.Farmer_isMarried_Prefix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(Farmer), nameof(Farmer.getSpouse)),
               prefix: new HarmonyMethod(typeof(FarmerPatches), nameof(FarmerPatches.Farmer_getSpouse_Prefix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(Farmer), nameof(Farmer.checkAction)),
               prefix: new HarmonyMethod(typeof(FarmerPatches), nameof(FarmerPatches.Farmer_checkAction_Prefix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(Farmer), nameof(Farmer.GetSpouseFriendship)),
               prefix: new HarmonyMethod(typeof(FarmerPatches), nameof(FarmerPatches.Farmer_GetSpouseFriendship_Prefix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(Farmer), nameof(Farmer.getChildren)),
               prefix: new HarmonyMethod(typeof(FarmerPatches), nameof(FarmerPatches.Farmer_getChildren_Prefix))
            );


            // UI patches

            harmony.Patch(
               original: AccessTools.Method(typeof(SocialPage), "drawNPCSlot"),
               prefix: new HarmonyMethod(typeof(UIPatches), nameof(UIPatches.SocialPage_drawNPCSlot_prefix)),
               transpiler: new HarmonyMethod(typeof(UIPatches), nameof(UIPatches.SocialPage_drawSlot_transpiler))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(SocialPage), "drawFarmerSlot"),
               transpiler: new HarmonyMethod(typeof(UIPatches), nameof(UIPatches.SocialPage_drawSlot_transpiler))
            );

            harmony.Patch(
               original: typeof(DialogueBox).GetConstructor(new Type[] { typeof(List<string>) }),
               prefix: new HarmonyMethod(typeof(UIPatches), nameof(UIPatches.DialogueBox_Prefix))
            );


            // Event patches

            harmony.Patch(
               original: AccessTools.Method(typeof(Event), nameof(Event.answerDialogueQuestion)),
               prefix: new HarmonyMethod(typeof(EventPatches), nameof(EventPatches.Event_answerDialogueQuestion_Prefix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(Event), "setUpCharacters"),
               postfix: new HarmonyMethod(typeof(EventPatches), nameof(EventPatches.Event_setUpCharacters_Postfix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(Event), nameof(Event.command_playSound)),
               prefix: new HarmonyMethod(typeof(EventPatches), nameof(EventPatches.Event_command_playSound_Prefix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(Event), nameof(Event.command_loadActors)),
               prefix: new HarmonyMethod(typeof(EventPatches), nameof(EventPatches.Event_command_loadActors_Prefix)),
               postfix: new HarmonyMethod(typeof(EventPatches), nameof(EventPatches.Event_command_loadActors_Postfix))
            );


            // Object patches

            harmony.Patch(
               original: AccessTools.Method(typeof(Object), nameof(Object.draw), new Type[] { typeof(SpriteBatch), typeof(int), typeof(int), typeof(float) }),
               prefix: new HarmonyMethod(typeof(ObjectPatches), nameof(ObjectPatches.Object_draw_Prefix))
            );

            // Furniture patches

            harmony.Patch(
               original: AccessTools.Method(typeof(BedFurniture), nameof(BedFurniture.draw), new Type[] { typeof(SpriteBatch), typeof(int), typeof(int), typeof(float) }),
               prefix: new HarmonyMethod(typeof(FurniturePatches), nameof(FurniturePatches.BedFurniture_draw_Prefix))
            );

            // Game1 patches

            harmony.Patch(
               original: AccessTools.Method(typeof(Game1), nameof(Game1.prepareSpouseForWedding)),
               prefix: new HarmonyMethod(typeof(Game1Patches), nameof(Game1Patches.prepareSpouseForWedding_Prefix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(Game1), nameof(Game1.getCharacterFromName), new Type[] { typeof(string), typeof(bool), typeof(bool) }), 
               prefix: new HarmonyMethod(typeof(Game1Patches), nameof(Game1Patches.getCharacterFromName_Prefix))
            );


            // NetWorldState patch 

            harmony.Patch(
               original: AccessTools.Method(typeof(NetWorldState), nameof(NetWorldState.hasWorldStateID)), 
               prefix: new HarmonyMethod(typeof(NetWorldStatePatches), nameof(NetWorldStatePatches.hasWorldStateID_Prefix))
            );

        }

        /// <summary>Get whether this instance can load the initial version of the given asset.</summary>
        /// <param name="asset">Basic metadata about the asset being loaded.</param>
        public bool CanLoad<T>(IAssetInfo asset)
        {
            if (!config.EnableMod)
                return false;

            string[] names = asset.AssetName.Split('_');
            if (config.ChildrenHaveHairOfSpouse && (names[0].Equals("Characters\\Baby") || names[0].Equals("Characters\\Toddler") || names[0].Equals("Characters/Baby") || names[0].Equals("Characters/Toddler")))
            {
                Monitor.Log($"can load child asset for {asset.AssetName}");
                return true;
            }

            return false;
        }

        /// <summary>Load a matched asset.</summary>
        /// <param name="asset">Basic metadata about the asset being loaded.</param>
        public T Load<T>(IAssetInfo asset)
        {
            Monitor.Log($"loading asset for {asset.AssetName}");

            if (asset.AssetName.StartsWith("Characters\\Baby") || asset.AssetName.StartsWith("Characters\\Toddler") || asset.AssetName.StartsWith("Characters/Baby") || asset.AssetName.StartsWith("Characters/Toddler"))
            {
                if(asset.AssetNameEquals("Characters\\Baby") || asset.AssetNameEquals("Characters\\Baby_dark") || asset.AssetNameEquals("Characters\\Toddler") || asset.AssetNameEquals("Characters\\Toddler_dark") || asset.AssetNameEquals("Characters\\Toddler_girl") || asset.AssetNameEquals("Characters\\Toddler_girl_dark"))
                {
                    Monitor.Log($"loading default child asset for {asset.AssetName}");
                    return (T)(object)Helper.Content.Load<Texture2D>($"assets/{asset.AssetName.Replace("Characters\\", "").Replace("Characters/", "")}.png", ContentSource.ModFolder);
                }
                if(asset.AssetNameEquals("Characters/Baby") || asset.AssetNameEquals("Characters/Baby_dark") || asset.AssetNameEquals("Characters/Toddler") || asset.AssetNameEquals("Characters/Toddler_dark") || asset.AssetNameEquals("Characters/Toddler_girl") || asset.AssetNameEquals("Characters/Toddler_girl_dark"))
                {
                    Monitor.Log($"loading default child asset for {asset.AssetName}");
                    return (T)(object)Helper.Content.Load<Texture2D>($"assets/{asset.AssetName.Replace("Characters/", "")}.png", ContentSource.ModFolder);
                }

                Monitor.Log($"loading child asset for {asset.AssetName}");

                string[] names = asset.AssetName.Split('_');
                Texture2D babySheet = Helper.Content.Load<Texture2D>(string.Join("_", names.Take(names.Length - 1)), ContentSource.GameContent);
                Texture2D parentTexSheet = null;
                string parent = names[names.Length - 1];
                try
                {
                    parentTexSheet = Helper.Content.Load<Texture2D>($"Characters/{parent}", ContentSource.GameContent);
                }
                catch
                {
                    return (T)(object)babySheet;
                }
                if (parentTexSheet == null)
                {
                    Monitor.Log($"couldn't find parent sheet for {asset.AssetName}");
                    return (T)(object)babySheet;
                }
                Rectangle newBounds = parentTexSheet.Bounds;
                newBounds.X = 0;
                newBounds.Y = 64;
                newBounds.Width = 16;
                newBounds.Height = 32;
                Texture2D parentTex = new Texture2D(Game1.graphics.GraphicsDevice, 16, 32);
                Color[] data = new Color[parentTex.Width * parentTex.Height];
                parentTexSheet.GetData(0, newBounds, data, 0, newBounds.Width * newBounds.Height);
                
                int start = -1;
                Dictionary<Color, int> colorCounts = new Dictionary<Color, int>();
                for (int i = 0; i < data.Length; i++)
                {
                    if(data[i] != Color.Transparent)
                    {
                        if(start == -1)
                        {
                            start = i / 16;
                        }
                        else
                        {
                            if (i / 16 - start > 8)
                                break;
                        }
                        if (colorCounts.ContainsKey(data[i]))
                        {
                            colorCounts[data[i]]++;
                        }
                        else
                        {
                            colorCounts.Add(data[i], 1);
                            Monitor.Log($"got hair color: {data[i]}");
                        }
                    }
                }

                if(colorCounts.Count == 0)
                {
                    Monitor.Log($"parent sheet empty for {asset.AssetName}");
                    return (T)(object)babySheet;
                }

                var countsList = colorCounts.ToList();

                countsList.Sort((pair1, pair2) => pair2.Value.CompareTo(pair1.Value));

                List<Color> hairColors = new List<Color>();
                for (int k = 0; k < Math.Min(countsList.Count, 4); k++)
                {
                    Monitor.Log($"using hair color: {countsList[k].Key} {countsList[k].Value}");
                    hairColors.Add(countsList[k].Key);
                }
                hairColors.Sort((color1, color2) => (color1.R + color1.G + color1.B).CompareTo(color2.R + color2.G + color2.B));

                Texture2D hairSheet = Helper.Content.Load<Texture2D>($"assets/hair/{string.Join("_", names.Take(names.Length - 1)).Replace("Characters\\","").Replace("Characters/","").Replace("_dark","")}.png", ContentSource.ModFolder);
                Color[] babyData = new Color[babySheet.Width * babySheet.Height];
                Color[] hairData = new Color[babySheet.Width * babySheet.Height];
                babySheet.GetData(babyData);
                hairSheet.GetData(hairData);

                for(int i = 0; i < babyData.Length; i++)
                {
                    if(hairData[i] != Color.Transparent)
                    {
                        if(hairColors.Count == 1)
                        {
                            hairColors.Add(hairColors[0]);
                            hairColors.Add(hairColors[0]);
                            hairColors.Add(hairColors[0]);
                        }
                        else if(hairColors.Count == 2)
                        {
                            hairColors.Add(hairColors[1]);
                            hairColors.Add(hairColors[1]);
                            hairColors[1] = new Color((hairColors[0].R + hairColors[0].R + hairColors[1].R) / 3, (hairColors[0].G + hairColors[0].G + hairColors[1].G) / 3, (hairColors[0].B + hairColors[0].B + hairColors[1].B) / 3);
                            hairColors[2] = new Color((hairColors[0].R + hairColors[2].R + hairColors[2].R) / 3, (hairColors[0].G + hairColors[2].G + hairColors[2].G) / 3, (hairColors[0].B + hairColors[2].B + hairColors[2].B) / 3);
                        }
                        else if(hairColors.Count == 3)
                        {
                            hairColors.Add(hairColors[2]);
                            hairColors[2] = new Color((hairColors[1].R + hairColors[2].R + hairColors[2].R) / 3, (hairColors[1].G + hairColors[2].G + hairColors[2].G) / 3, (hairColors[1].B + hairColors[2].B + hairColors[2].B) / 3);
                            hairColors[1] = new Color((hairColors[0].R + hairColors[0].R + hairColors[1].R) / 3, (hairColors[0].G + hairColors[0].G + hairColors[1].G) / 3, (hairColors[0].B + hairColors[0].B + hairColors[1].B) / 3);
                        }
                        //Monitor.Log($"Hair grey: {hairData[i].R}");
                        switch (hairData[i].R)
                        {
                            case 42:
                                babyData[i] = hairColors[0];
                                break;
                            case 60:
                                babyData[i] = hairColors[1];
                                break;
                            case 66:
                                babyData[i] = hairColors[1];
                                break;
                            case 82:
                                babyData[i] = hairColors[2];
                                break;
                            case 93:
                                babyData[i] = hairColors[2];
                                break;
                            case 114:
                                babyData[i] = hairColors[3];
                                break;
                        }
                            //Monitor.Log($"Hair color: {babyData[i]}");
                    }
                }
                babySheet.SetData(babyData);
                return (T)(object)babySheet;
            }
            throw new InvalidOperationException($"Unexpected asset '{asset.AssetName}'.");
        }

        /// <summary>Get whether this instance can edit the given asset.</summary>
        /// <param name="asset">Basic metadata about the asset being loaded.</param>
        public bool CanEdit<T>(IAssetInfo asset)
        {
            if (!config.EnableMod)
                return false;

            if (asset.AssetNameEquals("Data/Events/HaleyHouse") || asset.AssetNameEquals("Data/Events/Saloon") || asset.AssetNameEquals("Data/EngagementDialogue") || asset.AssetNameEquals("Strings/StringsFromCSFiles") || asset.AssetNameEquals("Data/animationDescriptions"))
            {
                return true;
            }
            if (config.RomanceAllVillagers && (asset.AssetName.StartsWith("Characters/schedules") || asset.AssetName.StartsWith("Characters\\schedules")))
            {
                try
                {
                    string name = asset.AssetName.Replace("Characters/schedules/", "").Replace("Characters\\schedules\\", "");
                    NPC npc = Game1.getCharacterFromName(name);
                    if (npc != null && npc.Age < 2 && !(npc is Child))
                    {
                        string dispo = Helper.Content.Load<Dictionary<string, string>>("Data/NPCDispositions", ContentSource.GameContent)[name];
                        if (dispo.Split('/')[5] != "datable")
                        {
                            Monitor.Log($"can edit schedule for {name}");
                            return true;
                        }
                    }
                }
                catch
                {
                    return false;
                }
            }

            return false;
        }

        /// <summary>Edit a matched asset.</summary>
        /// <param name="asset">A helper which encapsulates metadata about an asset and enables changes to it.</param>
        public void Edit<T>(IAssetData asset)
        {
            Monitor.Log("Editing asset " + asset.AssetName);
            if (asset.AssetNameEquals("Data/Events/HaleyHouse"))
            {
                IDictionary<string, string> data = asset.AsDictionary<string, string>().Data;

                data["195012/f Haley 2500/f Emily 2500/f Penny 2500/f Abigail 2500/f Leah 2500/f Maru 2500/o Abigail/o Penny/o Leah/o Emily/o Maru/o Haley/o Shane/o Harvey/o Sebastian/o Sam/o Elliott/o Alex/e 38/e 2123343/e 10/e 901756/e 54/e 15/k 195019"] = Regex.Replace(data["195012/f Haley 2500/f Emily 2500/f Penny 2500/f Abigail 2500/f Leah 2500/f Maru 2500/o Abigail/o Penny/o Leah/o Emily/o Maru/o Haley/o Shane/o Harvey/o Sebastian/o Sam/o Elliott/o Alex/e 38/e 2123343/e 10/e 901756/e 54/e 15/k 195019"], "(pause 1000/speak Maru \\\")[^$]+.a\\\"",$"$1{PHelper.Translation.Get("confrontation-female")}$h\"/emote Haley 21 true/emote Emily 21 true/emote Penny 21 true/emote Maru 21 true/emote Leah 21 true/emote Abigail 21").Replace("/dump girls 3", "");
                data["choseToExplain"] = Regex.Replace(data["choseToExplain"], "(pause 1000/speak Maru \\\")[^$]+.a\\\"",$"$1{PHelper.Translation.Get("confrontation-female")}$h\"/emote Haley 21 true/emote Emily 21 true/emote Penny 21 true/emote Maru 21 true/emote Leah 21 true/emote Abigail 21").Replace("/dump girls 4", "");
                data["lifestyleChoice"] = Regex.Replace(data["lifestyleChoice"], "(pause 1000/speak Maru \\\")[^$]+.a\\\"",$"$1{PHelper.Translation.Get("confrontation-female")}$h\"/emote Haley 21 true/emote Emily 21 true/emote Penny 21 true/emote Maru 21 true/emote Leah 21 true/emote Abigail 21").Replace("/dump girls 4", "");
            }
            else if (asset.AssetNameEquals("Data/Events/Saloon"))
            {
                IDictionary<string, string> data = asset.AsDictionary<string, string>().Data;

                string aData = data["195013/f Shane 2500/f Sebastian 2500/f Sam 2500/f Harvey 2500/f Alex 2500/f Elliott 2500/o Abigail/o Penny/o Leah/o Emily/o Maru/o Haley/o Shane/o Harvey/o Sebastian/o Sam/o Elliott/o Alex/e 911526/e 528052/e 9581348/e 43/e 384882/e 233104/k 195099"];
                data["195013/f Shane 2500/f Sebastian 2500/f Sam 2500/f Harvey 2500/f Alex 2500/f Elliott 2500/o Abigail/o Penny/o Leah/o Emily/o Maru/o Haley/o Shane/o Harvey/o Sebastian/o Sam/o Elliott/o Alex/e 911526/e 528052/e 9581348/e 43/e 384882/e 233104/k 195099"] = Regex.Replace(aData, "(pause 1000/speak Sam \\\")[^$]+.a\\\"",$"$1{PHelper.Translation.Get("confrontation-male")}$h\"/emote Shane 21 true/emote Sebastian 21 true/emote Sam 21 true/emote Harvey 21 true/emote Alex 21 true/emote Elliott 21").Replace("/dump guys 3", "");
                aData = data["choseToExplain"];
                data["choseToExplain"] = Regex.Replace(aData, "(pause 1000/speak Sam \\\")[^$]+.a\\\"", $"$1{PHelper.Translation.Get("confrontation-male")}$h\"/emote Shane 21 true/emote Sebastian 21 true/emote Sam 21 true/emote Harvey 21 true/emote Alex 21 true/emote Elliott 21").Replace("/dump guys 4", "");
                aData = data["crying"];
                data["crying"] = Regex.Replace(aData, "(pause 1000/speak Sam \\\")[^$]+.a\\\"",$"$1{PHelper.Translation.Get("confrontation-male")}$h\"/emote Shane 21 true/emote Sebastian 21 true/emote Sam 21 true/emote Harvey 21 true/emote Alex 21 true/emote Elliott 21").Replace("/dump guys 4", "");
            }
            else if (asset.AssetNameEquals("Strings/StringsFromCSFiles"))
            {
                IDictionary<string, string> data = asset.AsDictionary<string, string>().Data;
                data["NPC.cs.3985"] = Regex.Replace(data["NPC.cs.3985"],  @"\.\.\.\$s.+", $"$n#$b#$c 0.5#{data["ResourceCollectionQuest.cs.13681"]}#{data["ResourceCollectionQuest.cs.13683"]}");
                Monitor.Log($"NPC.cs.3985 is set to \"{data["NPC.cs.3985"]}\"");
            }
            else if (asset.AssetNameEquals("Data/animationDescriptions"))
            {
                IDictionary<string, string> data = asset.AsDictionary<string, string>().Data;
                List<string> sleepKeys = data.Keys.ToList().FindAll((s) => s.EndsWith("_Sleep"));
                foreach(string key in sleepKeys)
                {
                    if (!data.ContainsKey(key.ToLower()))
                    {
                        Monitor.Log($"adding {key.ToLower()} to animationDescriptions");
                        data.Add(key.ToLower(), data[key]);
                    }
                }
            }
            else if (asset.AssetNameEquals("Data/EngagementDialogue"))
            {
                IDictionary<string, string> data = asset.AsDictionary<string, string>().Data;
                if (!config.RomanceAllVillagers)
                    return;
                Farmer f = Game1.player;
                if (f == null)
                {
                    return;
                }
                foreach (string friend in f.friendshipData.Keys)
                {
                    if (!data.ContainsKey(friend+"0"))
                    {
                        data[friend + "0"] = "";
                    }
                    if (!data.ContainsKey(friend+"1"))
                    {
                        data[friend + "1"] = "";
                    }
                }
            }
            else if (asset.AssetName.StartsWith("Characters/schedules") || asset.AssetName.StartsWith("Characters\\schedules"))
            {


                IDictionary<string, string> data = asset.AsDictionary<string, string>().Data;
                List<string> keys = new List<string>(data.Keys);
                foreach (string key in keys)
                {
                    if(!data.ContainsKey($"marriage_{key}"))
                        data[$"marriage_{key}"] = data[key]; 
                }
            }
        }

    }
}