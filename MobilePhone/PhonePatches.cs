using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Characters;
using StardewValley.Locations;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using xTile.Dimensions;
using xTile.ObjectModel;

namespace MobilePhone
{
    public class PhonePatches
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
        public static bool Game1_pressSwitchToolButton_prefix()
        {
            if (ModEntry.phoneOpen && ModEntry.screenRect.Contains(Game1.getMousePosition()))
            {
                return false;
            }
            return true;
        }
        public static bool Farmer_addItemToInventory_prefix(Item item, ref Item __result)
        {
            if (ModEntry.isReminiscing)
            {
                Monitor.Log($"Reminiscing, will not add item {item.Name}");
                __result = null;
                return false;
            }
            return true;
        }
        public static bool Farmer_changeFriendship_prefix(int amount, NPC n)
        {
            if (ModEntry.isReminiscing)
            {
                Monitor.Log($"Reminiscing, will not change friendship with {n.Name} by {amount}");
                return false;
            }
            return true;
        }
        public static void GameLocation_resetLocalState_postfix(GameLocation __instance)
        {
            if (ModEntry.isReminiscing)
            {
                if (ModEntry.isReminiscingAtNight)
                {
                    Monitor.Log($"Reminiscing at night");
                    __instance.LightLevel = 0f;
                    //Game1.globalOutdoorLighting = 1f;
                    float transparency = Math.Min(0.93f, 0.75f + (2400 - Game1.getTrulyDarkTime() + Game1.gameTimeInterval / 7000f * 16.6f) * 0.000625f);
                    Game1.outdoorLight = Game1.eveningColor * transparency;
                    if (!(__instance is MineShaft) && !(__instance is Woods))
                    {
                        __instance.lightGlows.Clear();
                    }
                    PropertyValue nightTiles;
                    __instance.map.Properties.TryGetValue("NightTiles", out nightTiles);
                    if (nightTiles != null)
                    {
                        string[] split6 = nightTiles.ToString().Split(new char[]
                        {
                        ' '
                        });
                        for (int i3 = 0; i3 < split6.Length; i3 += 4)
                        {
                            if ((!split6[i3 + 3].Equals("726") || !Game1.MasterPlayer.mailReceived.Contains("pamHouseUpgrade")) && __instance.map.GetLayer(split6[i3]).Tiles[int.Parse(split6[i3 + 1]), int.Parse(split6[i3 + 2])] != null)
                            {
                                __instance.map.GetLayer(split6[i3]).Tiles[int.Parse(split6[i3 + 1]), int.Parse(split6[i3 + 2])].TileIndex = int.Parse(split6[i3 + 3]);
                            }
                        }
                    }
                }
                else
                {
                    Monitor.Log($"Reminiscing during the day");
                    return;
                    __instance.LightLevel = 1f;
                    //Game1.globalOutdoorLighting = 1f;
                    Game1.outdoorLight = Color.White;
                    Game1.ambientLight = Color.White;
                    PropertyValue dayTiles;
                    __instance.map.Properties.TryGetValue("DayTiles", out dayTiles);
                    if (dayTiles != null)
                    {
                        string[] split5 = dayTiles.ToString().Trim().Split(new char[]
                        {
                        ' '
                        });
                        for (int i2 = 0; i2 < split5.Length; i2 += 4)
                        {
                            if ((!split5[i2 + 3].Equals("720") || !Game1.MasterPlayer.mailReceived.Contains("pamHouseUpgrade")) && __instance.map.GetLayer(split5[i2]).Tiles[Convert.ToInt32(split5[i2 + 1]), Convert.ToInt32(split5[i2 + 2])] != null)
                            {
                                __instance.map.GetLayer(split5[i2]).Tiles[Convert.ToInt32(split5[i2 + 1]), Convert.ToInt32(split5[i2 + 2])].TileIndex = Convert.ToInt32(split5[i2 + 3]);
                            }
                        }
                    }

                }
            }
        }
        public static bool Event_command_cutscene_prefix(ref Event __instance, GameLocation location, GameTime time, string[] split, ref ICustomEventScript ___currentCustomEventScript)
        {
            if (!ModEntry.isInviting)
                return true;
            string text = split[1];
            if (___currentCustomEventScript != null)
            {
                if (___currentCustomEventScript.update(time, __instance))
                {
                    ___currentCustomEventScript = null;
                    int num = __instance.CurrentCommand;
                    __instance.CurrentCommand = num + 1;
                    return false;
                }
            }

            if (Game1.currentMinigame != null)
                return true;

            if (text == "EventInvite_balloonChangeMap")
            {
                location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(0, 1183, 84, 160), 10000f, 1, 99999, new Vector2(22f, 36f) * 64f + new Vector2(-23f, 0f) * 4f, false, false, 2E-05f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
                {
                    motion = new Vector2(0f, -2f),
                    yStopCoordinate = 576,
                    reachedStopCoordinate = new TemporaryAnimatedSprite.endBehavior(balloonInSky),
                    attachedCharacter = __instance.farmer,
                    id = 1
                });
                location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(84, 1205, 38, 26), 10000f, 1, 99999, new Vector2(22f, 36f) * 64f + new Vector2(0f, 134f) * 4f, false, false, 0.2625f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
                {
                    motion = new Vector2(0f, -2f),
                    id = 2,
                    attachedCharacter = __instance.getActorByName(ModEntry.invitedNPC.Name)
                });
                int num = __instance.CurrentCommand;
                __instance.CurrentCommand = num + 1;
                Game1.globalFadeToClear(null, 0.01f);
                return false;
            }
            if (text == "EventInvite_balloonDepart")
            {
                TemporaryAnimatedSprite temporarySpriteByID = location.getTemporarySpriteByID(1);
                temporarySpriteByID.attachedCharacter = __instance.farmer;
                temporarySpriteByID.motion = new Vector2(0f, -2f);
                TemporaryAnimatedSprite temporarySpriteByID2 = location.getTemporarySpriteByID(2);
                temporarySpriteByID2.attachedCharacter = __instance.getActorByName(ModEntry.invitedNPC.Name);
                temporarySpriteByID2.motion = new Vector2(0f, -2f);
                location.getTemporarySpriteByID(3).scaleChange = -0.01f;
                int num = __instance.CurrentCommand;
                __instance.CurrentCommand = num + 1;
                return false;
            }
            return true;
        }

        private static void balloonInSky(int extraInfo)
        {
            TemporaryAnimatedSprite t = Game1.currentLocation.getTemporarySpriteByID(2);
            if (t != null)
            {
                t.motion = Vector2.Zero;
            }
            t = Game1.currentLocation.getTemporarySpriteByID(1);
            if (t != null)
            {
                t.motion = Vector2.Zero;
            }
        }

        public static bool Event_command_prefix(Event __instance, string[] split)
        {
            if (ModEntry.isReminiscing)
            {
                Monitor.Log($"Reminiscing, will not execute event command {string.Join(" ",split)}");
                int num = __instance.CurrentCommand;
                __instance.CurrentCommand = num + 1;
                return false;
            }
            return true;
        }
        public static bool Event_endBehaviors_prefix(Event __instance, string[] split)
        {
            if (ModEntry.isReminiscing)
            {
                Monitor.Log($"Reminiscing, will not execute end behaviors {string.Join(" ", split)}");
                __instance.exitEvent();
                return false;
            }
            return true;
        }
        public static bool Event_namePet_prefix(Event __instance, string name)
        {
            if (ModEntry.isReminiscing)
            {
                Monitor.Log($"Reminiscing, renaming pet to {name}");
                if (Game1.player.catPerson)
                {
                    Pet cat = Game1.getFarm().characters.FirstOrDefault(n => n is Pet) as Pet;
                    if(cat == null)
                    {
                        cat = Utility.getHomeOfFarmer(Game1.player).characters.FirstOrDefault(n => n is Pet) as Pet;
                    }
                    if (cat != null)
                    {
                        cat.Name = name;
                        cat.displayName = cat.Name;
                    }
                }
                else
                {
                    Pet dog = Game1.getFarm().characters.FirstOrDefault(n => n is Pet) as Pet;
                    if (dog == null)
                    {
                        dog = Utility.getHomeOfFarmer(Game1.player).characters.FirstOrDefault(n => n is Pet) as Pet;
                    }
                    if (dog != null)
                    {
                        dog.Name = name;
                        dog.displayName = dog.Name;
                    }

                }
                Game1.exitActiveMenu();
                int num = __instance.CurrentCommand;
                __instance.CurrentCommand = num + 1;
                return false;
            }
            return true;
        }
        public static bool Event_skipEvent_prefix(Event __instance, ref Dictionary<string, Vector3> ___actorPositionsAfterMove)
        {
            if (ModEntry.isReminiscing)
            {
                Monitor.Log($"Reminiscing, will not execute skip functions");
                Game1.playSound("drumkit6");
                ___actorPositionsAfterMove.Clear();
                foreach (NPC i in __instance.actors)
                {
                    i.Halt();
                    __instance.resetDialogueIfNecessary(i);
                }
                __instance.farmer.Halt();
                __instance.farmer.ignoreCollisions = false;
                Game1.exitActiveMenu();
                Game1.dialogueUp = false;
                Game1.dialogueTyping = false;
                Game1.pauseTime = 0f;
                __instance.exitEvent();
                return false;
            }
            return true;
        }

        public static bool CarpenterMenu_returnToCarpentryMenu_prefix()
        {
            if (!ModEntry.inCall)
                return true;
            LocationRequest locationRequest = ModEntry.callLocation;

            locationRequest.OnWarp += delegate ()
            {
                RefreshView1();
            };
            Game1.warpFarmer(locationRequest.Location.Name, Game1.player.TilePoint.X, Game1.player.TilePoint.Y, Game1.player.FacingDirection);
            return false;
        }

        public static bool CarpenterMenu_returnToCarpentryMenuAfterSuccessfulBuild_prefix()
        {
            if (!ModEntry.inCall)
                return true;
            LocationRequest locationRequest = ModEntry.callLocation;
            locationRequest.OnWarp += delegate ()
            {
                RefreshView2();

            };
            Game1.warpFarmer(locationRequest.Location.Name, Game1.player.TilePoint.X, Game1.player.TilePoint.Y, Game1.player.FacingDirection);
            return false;
        }
        private static void RefreshView1()
        {
            if (!(Game1.activeClickableMenu is CarpenterMenu))
                return;

            Helper.Reflection.GetField<bool>(Game1.activeClickableMenu, "onFarm").SetValue(false);
            Game1.player.viewingLocation.Value = null;
            Helper.Reflection.GetMethod(Game1.activeClickableMenu, "resetBounds").Invoke(new object[] { });
            Helper.Reflection.GetField<bool>(Game1.activeClickableMenu, "upgrading").SetValue(false);
            Helper.Reflection.GetField<bool>(Game1.activeClickableMenu, "moving").SetValue(false);
            Helper.Reflection.GetField<Building>(Game1.activeClickableMenu, "buildingToMove").SetValue(null);
            Helper.Reflection.GetField<bool>(Game1.activeClickableMenu, "freeze").SetValue(false);
            Game1.displayHUD = true;
            Game1.viewportFreeze = false;
            Game1.viewport.Location = ModEntry.callViewportLocation;
            Helper.Reflection.GetField<bool>(Game1.activeClickableMenu, "drawBG").SetValue(true);
            Helper.Reflection.GetField<bool>(Game1.activeClickableMenu, "demolishing").SetValue(false);
            Game1.displayFarmer = true;
            if (Game1.options.SnappyMenus)
            {
                Game1.activeClickableMenu.populateClickableComponentList();
                Game1.activeClickableMenu.snapToDefaultClickableComponent();
            }
        }
        private static void RefreshView2()
        {
            if (!(Game1.activeClickableMenu is CarpenterMenu))
                return;


            Game1.displayHUD = true;
            Game1.player.viewingLocation.Value = null;
            Game1.viewportFreeze = false;
            Game1.viewport.Location = new Location(320, 1536);
            Helper.Reflection.GetField<bool>(Game1.activeClickableMenu, "freeze").SetValue(false);
            Game1.displayFarmer = true;
            robinPhoneConstructionMessage(Game1.activeClickableMenu, (Game1.activeClickableMenu as CarpenterMenu).Blueprint);
        }

        private static async void robinPhoneConstructionMessage(IClickableMenu instance, CarpenterMenu.BlueprintEntry CurrentBlueprint)
        {
            Game1.player.forceCanMove();
            string dialoguePath = "Data\\ExtraDialogue:Robin_" + (Helper.Reflection.GetField<bool>(instance, "upgrading").GetValue() ? "Upgrade" : "New") + "Construction";
            if (Utility.isFestivalDay(Game1.dayOfMonth + 1, Game1.season))
            {
                dialoguePath += "_Festival";
            }
            if (CurrentBlueprint.BuildDays <= 0)
            {
                Game1.DrawDialogue(Game1.getCharacterFromName("Robin", true), Game1.content.LoadString("Data\\ExtraDialogue:Robin_Instant", (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.de) ? CurrentBlueprint.DisplayName : CurrentBlueprint.DisplayName.ToLower()));
            }
            else
            {
                Game1.DrawDialogue(Game1.getCharacterFromName("Robin", true), Game1.content.LoadString(dialoguePath, (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.de) ? CurrentBlueprint.DisplayName : CurrentBlueprint.DisplayName.ToLower(), (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.de) ? CurrentBlueprint.DisplayName.Split(' ').Last().Split('-').Last() : ((LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.pt || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.es || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.it) ? CurrentBlueprint.DisplayName.ToLower().Split(' ').First() : CurrentBlueprint.DisplayName.ToLower().Split(' ').Last())));
            }

            while (Game1.activeClickableMenu is DialogueBox)
            {
                await Task.Delay(50);
            }
            MobilePhoneCall.ShowMainCallDialogue(ModEntry.callingNPC);
        }

        public static void GameLocation_answerDialogue_prefix(GameLocation __instance, Response answer)
        {
            try
            {
                if (answer.responseKey.StartsWith("PhoneApp_InCall_"))
                    __instance.afterQuestion = MobilePhoneCall.CallDialogueAnswer;

            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(GameLocation_answerDialogue_prefix)}:\n{ex}", LogLevel.Error);
            }
        }
    }
}