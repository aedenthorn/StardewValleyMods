using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Network;
using StardewValley.Objects;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using xTile;
using xTile.Dimensions;
using xTile.ObjectModel;
using xTile.Tiles;
using Harmony;
using Microsoft.Xna.Framework.Graphics;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using System.IO;
using StardewValley.Monsters;
using StardewValley.Menus;
using StardewValley.Projectiles;
using System.Reflection;

namespace Swim
{
    public class ModEntry : Mod, IAssetEditor, IAssetLoader
    {
        
        private ModConfig config;
        public static IMonitor SMonitor;
        private IJsonAssetsApi JsonAssets;
        public static int scubaMaskID = -1;
        public static int scubaFinsID = -1;
        public static int scubaTankID = -1;
        public static List<int> scubaGear = new List<int>();
        private List<SButton> dirButtons = new List<SButton>();
        private bool myButtonDown = false;
        private ulong lastJump = 0;
        private int oxygen = 0;
        private int lastUpdateMs = 0;
        private bool isJumping = false;
        private Vector2 startJumpLoc;
        private Vector2 endJumpLoc;
        private bool willSwim = false;
        private bool isUnderwater = false;
        internal static NPC oldMariner;
        public static bool marinerQuestionsWrongToday = false;
        internal static Random myRand;
        private static Dictionary<string, DiveMap> diveMaps = new Dictionary<string, DiveMap>();
        public static Dictionary<string,bool> changeLocations = new Dictionary<string, bool> {
            {"UnderwaterMountain", false },
            {"Mountain", false },
            {"Town", false },
            {"Forest", false },
            {"UnderwaterBeach", false },
            {"Beach", false },
        };
        private Texture2D OxygenBarTexture;
        private List<Vector2> bubbles = new List<Vector2>();
        private string[] diveLocations = new string[] {
            "Beach",
            "Forest",
            "Mountain",
            "UnderwaterBeach",
            "UnderwaterMountain",
            "ScubaCave",
            "ScubaAbigailCave",
            "ScubaCrystalCave",
        };
        public static List<string> fishTextures = new List<string>()
        {
            "BlueFish",
            "PinkFish",
            "GreenFish",
        };
        public static List<string> bigFishTextures = new List<string>()
        {
            "BigFishBlack",
            "BigFishBlue",
            "BigFishGold",
            "BigFishGreen",
            "BigFishGreenWhite",
            "BigFishGrey",
            "BigFishRed",
            "BigFishWhite"
        };

        public override void Entry(IModHelper helper)
        {
            config = Helper.ReadConfig<ModConfig>();
            if (!config.EnableMod)
                return;

            SMonitor = Monitor;

            myRand = new Random();
            
            SwimPatches.Initialize(Monitor, helper, config);
            SwimDialog.Initialize(Monitor, helper, config);

            foreach (InputButton ib in Game1.options.moveUpButton)
            {
                dirButtons.Add(ib.ToSButton());
            }
            foreach(InputButton ib in Game1.options.moveDownButton)
            {
                dirButtons.Add(ib.ToSButton());
            }
            foreach(InputButton ib in Game1.options.moveRightButton)
            {
                dirButtons.Add(ib.ToSButton());
            }
            foreach(InputButton ib in Game1.options.moveLeftButton)
            {
                dirButtons.Add(ib.ToSButton());
            }

            helper.Events.GameLoop.UpdateTicked += GameLoop_UpdateTicked;
            helper.Events.Input.ButtonPressed += Input_ButtonPressed;
            helper.Events.Input.ButtonReleased += Input_ButtonReleased;
            helper.Events.GameLoop.DayStarted += GameLoop_DayStarted;
            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
            helper.Events.Display.RenderedHud += Display_RenderedHud;
            helper.Events.Display.RenderedWorld += Display_RenderedWorld;
            helper.Events.Player.InventoryChanged += Player_InventoryChanged;
            helper.Events.Player.Warped += Player_Warped;

            var harmony = HarmonyInstance.Create(this.ModManifest.UniqueID);

            harmony.Patch(
               original: AccessTools.Method(typeof(FarmerRenderer), nameof(FarmerRenderer.draw), new Type[] { typeof(SpriteBatch), typeof(FarmerSprite.AnimationFrame), typeof(int), typeof(Rectangle), typeof(Vector2), typeof(Vector2), typeof(float), typeof(int), typeof(Color), typeof(float), typeof(float), typeof(Farmer) }),
               prefix: new HarmonyMethod(typeof(SwimPatches), nameof(SwimPatches.FarmerRenderer_draw_Prefix)),
               postfix: new HarmonyMethod(typeof(SwimPatches), nameof(SwimPatches.FarmerRenderer_draw_Postfix))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.startEvent)),
               postfix: new HarmonyMethod(typeof(SwimPatches), nameof(SwimPatches.GameLocation_StartEvent_Postfix))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(Event), nameof(Event.exitEvent)),
               postfix: new HarmonyMethod(typeof(SwimPatches), nameof(SwimPatches.Event_exitEvent_Postfix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(Farmer), "updateCommon"),
               prefix: new HarmonyMethod(typeof(SwimPatches), nameof(SwimPatches.Farmer_updateCommon_Prefix)),
               postfix: new HarmonyMethod(typeof(SwimPatches), nameof(SwimPatches.Farmer_updateCommon_Postfix))
            );
            
            harmony.Patch(
               original: AccessTools.Method(typeof(Farmer), nameof(Farmer.changeIntoSwimsuit)),
               postfix: new HarmonyMethod(typeof(SwimPatches), nameof(SwimPatches.Farmer_changeIntoSwimsuit_Postfix))
            );
            
            harmony.Patch(
               original: AccessTools.Method(typeof(Toolbar), nameof(Toolbar.draw), new Type[] { typeof(SpriteBatch) }),
               prefix: new HarmonyMethod(typeof(SwimPatches), nameof(SwimPatches.Toolbar_draw_Prefix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(Wand), nameof(Wand.DoFunction)),
               transpiler: new HarmonyMethod(typeof(SwimPatches), nameof(SwimPatches.Wand_DoFunction_Transpiler))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.draw)),
               prefix: new HarmonyMethod(typeof(SwimPatches), nameof(SwimPatches.GameLocation_draw_Prefix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.UpdateWhenCurrentLocation)),
               postfix: new HarmonyMethod(typeof(SwimPatches), nameof(SwimPatches.GameLocation_UpdateWhenCurrentLocation_Postfix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.resetForPlayerEntry)),
               prefix: new HarmonyMethod(typeof(SwimPatches), nameof(SwimPatches.GameLocation_resetForPlayerEntry_Prefix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.isCollidingPosition), new Type[] { typeof(Rectangle), typeof(xTile.Dimensions.Rectangle), typeof(bool), typeof(int), typeof(bool), typeof(Character) }),
               prefix: new HarmonyMethod(typeof(SwimPatches), nameof(SwimPatches.GameLocation_isCollidingPosition_Prefix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.performTouchAction)),
               prefix: new HarmonyMethod(typeof(SwimPatches), nameof(SwimPatches.GameLocation_performTouchAction_Prefix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.checkAction)),
               prefix: new HarmonyMethod(typeof(SwimPatches), nameof(SwimPatches.GameLocation_checkAction_Prefix))
            );

        }
        public override object GetApi()
        {
            return new SwimModApi(Monitor, this);
        }

        private void Player_Warped(object sender, WarpedEventArgs e)
        {
            if (e.NewLocation.Name == "ScubaAbigailCave")
            {
                abigailTicks = 0;
                e.NewLocation.characters.Clear();


                Game1.player.changeOutOfSwimSuit();
                if(Game1.player.hat.Value != null && Game1.player.hat.Value.parentSheetIndex != 0)
                    Game1.player.addItemToInventory(Game1.player.hat.Value);
                Game1.player.hat.Value = new Hat(0);
                Game1.player.doEmote(9);
            }
        }

        private void Player_InventoryChanged(object sender, InventoryChangedEventArgs e)
        {
            if (e.Player != Game1.player)
                return;

            if (!Game1.player.mailReceived.Contains("ScubaTank") && scubaTankID != -1 && e.Added != null && e.Added.Count() > 0 && e.Added.FirstOrDefault() != null && e.Added.FirstOrDefault().parentSheetIndex != null && e.Added.FirstOrDefault().GetType() == typeof(Clothing) && e.Added.FirstOrDefault().parentSheetIndex == scubaTankID)
            {
                Monitor.Log("Player found scuba tank");
                Game1.player.mailReceived.Add("ScubaTank");
            }
            if (!Game1.player.mailReceived.Contains("ScubaMask") && scubaMaskID != -1 && e.Added != null && e.Added.Count() > 0 && e.Added.FirstOrDefault() != null && e.Added.FirstOrDefault().parentSheetIndex != null && e.Added.FirstOrDefault().GetType() == typeof(Hat) && (e.Added.FirstOrDefault() as Hat).which == scubaMaskID)
            {
                Monitor.Log("Player found scuba mask");
                Game1.player.mailReceived.Add("ScubaMask");
            }
            if (!Game1.player.mailReceived.Contains("ScubaFins") && scubaFinsID != -1 && e.Added != null && e.Added.Count() > 0 && e.Added.FirstOrDefault() != null && e.Added.FirstOrDefault().parentSheetIndex != null && e.Added.FirstOrDefault().GetType() == typeof(Boots) && e.Added.FirstOrDefault().parentSheetIndex == scubaFinsID)
            {
                Monitor.Log("Player found scuba fins");
                Game1.player.mailReceived.Add("ScubaFins");
            }
        }
        private void GameLoop_SaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            // load dive maps

            foreach (IContentPack contentPack in Helper.ContentPacks.GetOwned())
            {
                try
                {
                    Monitor.Log($"Reading content pack: {contentPack.Manifest.Name} {contentPack.Manifest.Version} from {contentPack.DirectoryPath}");
                    DiveMapData data = contentPack.ReadJsonFile<DiveMapData>("content.json");
                    ReadDiveMapData(data);
                }
                catch
                {
                    Monitor.Log($"couldn't read content.json in content pack {contentPack.Manifest.Name}", LogLevel.Warn);
                }
            }

            Monitor.Log($"Reading content pack from assets/swim-map-content.json");

            try
            {
                DiveMapData myData = Helper.Data.ReadJsonFile<DiveMapData>("assets/swim-map-content.json");
                ReadDiveMapData(myData);
            }
            catch (Exception ex)
            {
                Monitor.Log($"assets/swim-map-content.json file read error. Exception: {ex}", LogLevel.Warn);
            }


            // load scuba gear ids

            scubaGear.Clear();
            if (JsonAssets != null)
            {
                scubaMaskID = JsonAssets.GetHatId("Scuba Mask");
                scubaTankID = JsonAssets.GetClothingId("Scuba Tank");

                if (scubaMaskID == -1)
                {
                    Monitor.Log("Can't get ID for Scuba Mask. Some functionality will be lost.", LogLevel.Warn);
                }
                else
                {
                    Monitor.Log(string.Format("Scuba Mask ID is {0}.", scubaMaskID), LogLevel.Debug);
                    scubaGear.Add(scubaMaskID);
                }

                if (scubaTankID == -1)
                {
                    Monitor.Log("Can't get ID for Scuba Tank. Some functionality will be lost.", LogLevel.Warn);
                }
                else
                {
                    Monitor.Log(string.Format("Scuba Tank ID is {0}.", scubaTankID), LogLevel.Debug);
                    scubaGear.Add(scubaTankID);
                }

                try
                {
                    scubaFinsID = Helper.Content.Load<Dictionary<int, string>>(@"Data/Boots", ContentSource.GameContent).First(x => x.Value.StartsWith("Scuba Fins")).Key;
                }
                catch
                {
                    Monitor.Log("Can't get ID for Scuba Fins. Some functionality will be lost.", LogLevel.Warn);
                }
                if (scubaFinsID != -1)
                {
                    Monitor.Log(string.Format("Scuba Fins ID is {0}.", scubaFinsID), LogLevel.Debug);
                    scubaGear.Add(scubaFinsID);
                }
            }
            if (!IsWearingScubaGear() && config.SwimSuitAlways)
                Game1.player.changeIntoSwimsuit();
        }

        public void ReadDiveMapData(DiveMapData data)
        {
            foreach (DiveMap map in data.Maps)
            {
                if (Game1._locationLookup.ContainsKey(map.Name))
                {
                    if (!diveMaps.ContainsKey(map.Name))
                    {
                        diveMaps.Add(map.Name, map);
                        Monitor.Log($"added dive map info for {map.Name}", LogLevel.Debug);
                    }
                    else
                    {
                        Monitor.Log($"dive map info already exists for {map.Name}", LogLevel.Warn);
                    }

                }
                else
                {
                    Monitor.Log($"dive map info not loaded for {map.Name}, check you have the map installed", LogLevel.Warn);
                }
            }
        }



        public static int ticksUnderwater = 0;
        public static int bubbleOffset = 0;

        private void Display_RenderedWorld(object sender, RenderedWorldEventArgs e)
        {
            if (isUnderwater && IsMapUnderwater(Game1.currentLocation.Name))
            {
                Texture2D tex = Helper.Content.Load<Texture2D>("LooseSprites/temporary_sprites_1", ContentSource.GameContent);
                if ((ticksUnderwater % 100 / Math.Min(100, config.BubbleMult)) - bubbleOffset == 0)
                {
                    Game1.playSound("tinyWhip");
                    bubbles.Add(new Vector2(Game1.player.position.X + Game1.random.Next(-24,25), Game1.player.position.Y - 96));
                    bubbleOffset = Game1.random.Next(30/ Math.Min(100, config.BubbleMult));
                }

                for (int k = 0; k < bubbles.Count; k++) 
                {
                    bubbles[k] = new Vector2(bubbles[k].X, bubbles[k].Y - 2);
                }
                foreach (Vector2 v in bubbles)
                {
                    e.SpriteBatch.Draw(tex, v + new Vector2((float)Math.Sin(ticksUnderwater / 20f) * 10f - Game1.viewport.X, -Game1.viewport.Y), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(132, 20, 8, 8)), new Color(1,1,1,0.5f), 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.001f);
                }
                ticksUnderwater++;
            }
            else
            {
                ticksUnderwater = 0;
            }
        }

        private void Display_RenderedHud(object sender, RenderedHudEventArgs e)
        {
            if(Game1.currentLocation.Name == "ScubaAbigailCave")
            {
                if (abigailTicks > 0 && abigailTicks < 80000 / 16)
                    MakeOxygenBar((80000 / 16) - abigailTicks, 80000 / 16);
                e.SpriteBatch.Draw(OxygenBarTexture, new Vector2((int)Math.Round(Game1.viewport.Width * 0.13f), 100), Color.White);
                return;
            }
            int maxOx = MaxOxygen();
            if (oxygen < maxOx)
            {
                MakeOxygenBar(oxygen, maxOx);
                e.SpriteBatch.Draw(OxygenBarTexture, new Vector2((int)Math.Round(Game1.viewport.Width * 0.13f), 100), Color.White);
            }
        }

        private void GameLoop_GameLaunched(object sender, GameLaunchedEventArgs e)
        {
            IContentPack TMXcontentPack = this.Helper.ContentPacks.CreateFake(Path.Combine(this.Helper.DirectoryPath, "assets/tmx-pack"));

            object api = Helper.ModRegistry.GetApi("Platonymous.TMXLoader");
            if (api != null)
            {
                Helper.Reflection.GetMethod(api, "AddContentPack").Invoke(TMXcontentPack);
            }

            JsonAssets = Helper.ModRegistry.GetApi<IJsonAssetsApi>("spacechase0.JsonAssets");
            bool flag = this.JsonAssets == null;
            if (flag)
            {
                base.Monitor.Log("Can't load Json Assets API for scuba gear");
            }
            else
            {
                JsonAssets.LoadAssets(Path.Combine(base.Helper.DirectoryPath, "assets/json-assets"));
            }

            // fix dive maps

            foreach (IContentPack contentPack in Helper.ContentPacks.GetOwned())
            {
                try
                {
                    Monitor.Log($"Reading content pack: {contentPack.Manifest.Name} {contentPack.Manifest.Version} from {contentPack.DirectoryPath}");
                    DiveMapData data = contentPack.ReadJsonFile<DiveMapData>("content.json");
                    foreach(DiveMap map in data.Maps)
                    {
                        if (map.Features.Contains("FixWaterTiles") && !changeLocations.ContainsKey(map.Name))
                        {
                            changeLocations.Add(map.Name, false);
                        }
                    }
                }
                catch
                {
                    Monitor.Log($"couldn't read content.json in content pack {contentPack.Manifest.Name}", LogLevel.Warn);
                }
            }
        }

        private void GameLoop_DayStarted(object sender, DayStartedEventArgs e)
        {
            foreach(KeyValuePair<string, DiveMap> kvp in diveMaps)
            {
                if (!Game1._locationLookup.ContainsKey(kvp.Key))
                    continue;
                if (kvp.Value.Features.Contains("OceanTreasure") || kvp.Value.Features.Contains("OceanResources") || kvp.Value.Features.Contains("Minerals"))
                {
                    Game1._locationLookup[kvp.Key].overlayObjects.Clear();
                }
                if (kvp.Value.Features.Contains("OceanTreasure"))
                {
                    AddOceanTreasure(Game1._locationLookup[kvp.Key]);
                }
                if (kvp.Value.Features.Contains("OceanResources"))
                {
                    AddOceanForage(Game1._locationLookup[kvp.Key]);
                }
                if (kvp.Value.Features.Contains("SmolFishies"))
                {
                    AddFishies(Game1._locationLookup[kvp.Key]);
                }
                if (kvp.Value.Features.Contains("BigFishies"))
                {
                    AddFishies(Game1._locationLookup[kvp.Key], false);
                }
                if (kvp.Value.Features.Contains("Minerals"))
                {
                    AddMinerals(Game1._locationLookup[kvp.Key]);
                }
                if (kvp.Value.Features.Contains("WaterTiles"))
                {
                    AddWaterTiles(Game1._locationLookup[kvp.Key]);
                }
                if (kvp.Value.Features.Contains("Underwater"))
                {
                    RemoveWaterTiles(Game1._locationLookup[kvp.Key]);
                }
            }
            if (Game1._locationLookup.ContainsKey("ScubaCave"))
            {
                AddScubaChest(Game1._locationLookup["ScubaCave"], new Vector2(10,14), "ScubaMask");
            }
            marinerQuestionsWrongToday = false;
            oxygen = MaxOxygen();
        }

        public static void AddScubaChest(GameLocation gameLocation, Vector2 pos, string which)
        {
            if (which == "ScubaTank" && !Game1.player.mailReceived.Contains(which))
            {
                gameLocation.overlayObjects[pos] = new Chest(0, new List<Item>() { new Clothing(scubaTankID) }, pos, false, 0);
            }
            else if (which == "ScubaMask" && !Game1.player.mailReceived.Contains(which))
            {
                gameLocation.overlayObjects[pos] = new Chest(0, new List<Item>() { new Hat(scubaMaskID) }, pos, false, 0);
            }
            else if (which == "ScubaFins" && !Game1.player.mailReceived.Contains(which))
            {
                gameLocation.overlayObjects[pos] = new Chest(0, new List<Item>() { new Boots(scubaFinsID) }, pos, false, 0);
            }
        }
        private void AddWaterTiles(GameLocation gameLocation)
        {
            gameLocation.waterTiles = new bool[gameLocation.map.Layers[0].LayerWidth, gameLocation.map.Layers[0].LayerHeight];
            bool foundAnyWater = false;
            for (int x = 0; x < gameLocation.map.Layers[0].LayerWidth; x++)
            {
                for (int y = 0; y < gameLocation.map.Layers[0].LayerHeight; y++)
                {
                    if (gameLocation.doesTileHaveProperty(x, y, "Water", "Back") != null)
                    {
                        foundAnyWater = true;
                        gameLocation.waterTiles[x, y] = true;
                    }
                }
            }
            if (!foundAnyWater)
            {
                Monitor.Log($"{Game1.currentLocation.Name} has no water tiles");
                gameLocation.waterTiles = null;
            }
            else
            {
                Monitor.Log($"Gave {Game1.currentLocation.Name} water tiles");
            }
        }

        private void Input_ButtonReleased(object sender, ButtonReleasedEventArgs e)
        {
            if(Game1.player == null)
            {
                myButtonDown = false;
                return;
            }
            CheckIfMyButtonDown();
        }

        ulong lastProjectile = 0;

        private void Input_ButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (Game1.player == null || Game1.currentLocation == null)
            {
                myButtonDown = false;
                return;
            }

            if (Game1.activeClickableMenu != null && Game1.currentLocation.Name == "ScubaCrystalCave" && Game1.currentLocation.lastQuestionKey.StartsWith("SwimMod_Mariner_"))
            {
                IClickableMenu menu = Game1.activeClickableMenu;
                if (menu == null || menu.GetType() != typeof(DialogueBox))
                    return;
                int resp = (int)typeof(DialogueBox).GetField("selectedResponse", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(menu);
                List<Response> resps = (List<Response>)typeof(DialogueBox).GetField("responses", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(menu);

                if (resp < 0 || resps == null || resp >= resps.Count || resps[resp] == null)
                    return;
                Game1.currentLocation.lastQuestionKey = ""; 
                
                SwimDialog.OldMarinerDialogue(resps[resp].responseKey);
                return;
            }



            if(false && e.Button == SButton.Q)
            {
                Game1.currentLocation.overlayObjects[Game1.player.getTileLocation() + new Vector2(0, 1)] = new Chest(0, new List<Item>() { Helper.Input.IsDown(SButton.LeftShift) ? (Item)(new StardewValley.Object(434, 1)) : (new Hat(scubaMaskID)) }, Game1.player.getTileLocation() + new Vector2(0, 1), false, 0);
            }

            if (e.Button == config.DiveKey && diveMaps.ContainsKey(Game1.currentLocation.Name) && diveMaps[Game1.currentLocation.Name].DiveLocations.Count > 0)
            {
                Point pos = Game1.player.getTileLocationPoint();
                Location loc = new Location(pos.X, pos.Y);

                if (!IsInWater())
                {
                    return;
                }

                DiveMap dm = diveMaps[Game1.currentLocation.Name];
                DiveLocation diveLocation = null;
                foreach(DiveLocation dl in dm.DiveLocations)
                {
                    if (dl.GetRectangle().X == -1 || dl.GetRectangle().Contains(loc))
                    {
                        diveLocation = dl;
                        break;
                    }
                }

                if (diveLocation == null)
                {
                    Monitor.Log($"No dive destination for this point on this map", LogLevel.Debug);
                    return;
                }

                if (!Game1._locationLookup.ContainsKey(diveLocation.OtherMapName))
                {
                    Monitor.Log($"Can't find destination map named {diveLocation.OtherMapName}", LogLevel.Warn);
                    return;
                }

                Monitor.Log($"warping to {diveLocation.OtherMapName}", LogLevel.Debug);
                DiveTo(diveLocation);
                return; 
            }
            
            if (e.Button == config.SwimKey)
            {
                config.ReadyToSwim = !config.ReadyToSwim;
                Helper.WriteConfig<ModConfig>(config);
                Monitor.Log($"Ready to swim: {config.ReadyToSwim}");
                return;
            }

            if (e.Button == config.SwimSuitKey)
            {
                config.SwimSuitAlways = !config.SwimSuitAlways;
                Helper.WriteConfig<ModConfig>(config);
                if (!Game1.player.swimming)
                {
                    if(!config.SwimSuitAlways)
                        Game1.player.changeOutOfSwimSuit();
                    else
                        Game1.player.changeIntoSwimsuit();
                }
                return;
            }

        }
        private void CheckIfMyButtonDown()
        {
            if(Game1.player == null || Game1.currentLocation == null || !config.ReadyToSwim || Game1.currentLocation.waterTiles == null || Helper.Input.IsDown(SButton.LeftShift) || Helper.Input.IsDown(SButton.RightShift))
            {
                myButtonDown = false;
                return;
            }

            foreach (SButton b in dirButtons)
            {
                if (Helper.Input.IsDown(b))
                {
                    myButtonDown = true;
                    return;
                }
            }

            if (Helper.Input.IsDown(SButton.MouseLeft))
            {
                myButtonDown = true;
                return;
            }

            myButtonDown = false;
        }

        private void GameLoop_UpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (Game1.currentLocation == null || Game1.player == null || !Game1.displayFarmer || Game1.player.position == null)
                return;

            if(Game1.currentLocation.Name == "ScubaAbigailCave")
            {
                AbigailCaveTick();
            }

            if (Game1.activeClickableMenu == null)
            {
                if (IsMapUnderwater(Game1.currentLocation.Name))
                {
                    if (isUnderwater)
                    {
                        if (oxygen > 0)
                        {
                            if (!IsWearingScubaGear())
                                oxygen--;
                        }
                        else
                        {
                            Game1.playSound("pullItemFromWater");
                            isUnderwater = false;
                            DiveLocation diveLocation = diveMaps[Game1.currentLocation.Name].DiveLocations.Last();
                            DiveTo(diveLocation);
                        }
                    }
                }
                else
                {
                    if (oxygen < MaxOxygen())
                        oxygen++;
                    if (oxygen < MaxOxygen())
                        oxygen++;
                }
            }

            if (isJumping)
            {
                float difx = endJumpLoc.X - startJumpLoc.X;
                float dify = endJumpLoc.Y - startJumpLoc.Y;
                float completed = Game1.player.freezePause / (float)config.JumpTimeInMilliseconds;
                if (Game1.player.freezePause <= 0)
                {
                    Game1.player.position.Value = endJumpLoc;
                    isJumping = false;
                    if (willSwim)
                    {
                        Game1.currentLocation.playSound("waterSlosh", NetAudio.SoundContext.Default);
                        Game1.player.swimming.Value = true;
                    }
                    else
                    {
                        if (!config.SwimSuitAlways)
                            Game1.player.changeOutOfSwimSuit();
                    }
                    return;
                }
                Game1.player.position.Value = new Vector2(endJumpLoc.X - (difx * completed), endJumpLoc.Y - (dify * completed) - (float)Math.Sin(completed * Math.PI) * 64);
                return;
            }



            if (!config.ReadyToSwim)
            {
                return;
            }

            if (Game1.player.swimming) {
                if (!IsInWater() && !isJumping)
                {
                    Monitor.Log("Swimming out of water");
                    Game1.player.swimming.Value = false;
                    if (Game1.player.bathingClothes && !config.SwimSuitAlways)
                        Game1.player.changeOutOfSwimSuit();
                }

                DiveMap dm = null;
                Point edgePos = Game1.player.getTileLocationPoint();

                if (diveMaps.ContainsKey(Game1.currentLocation.Name))
                {
                    dm = diveMaps[Game1.currentLocation.Name];
                }

                if (Game1.player.position.Y > Game1.viewport.Y + Game1.viewport.Height - 16)
                {

                    Game1.player.position.Value = new Vector2(Game1.player.position.X, Game1.viewport.Y + Game1.viewport.Height - 17);
                    if (dm != null)
                    {
                        EdgeWarp edge = dm.EdgeWarps.Find((x) => x.ThisMapEdge == "Bottom" && x.FirstTile <= edgePos.X && x.LastTile >= edgePos.X);
                        if (edge != null)
                        {
                            Point pos = GetEdgeWarpDestination(edgePos.X, edge);
                            if (pos != Point.Zero)
                            {
                                Monitor.Log("warping south");
                                Game1.warpFarmer(edge.OtherMapName, pos.X, pos.Y, false);
                            }
                        }
                    }
                }
                else if (Game1.player.position.Y < Game1.viewport.Y - 16)
                {
                    Game1.player.position.Value = new Vector2(Game1.player.position.X, Game1.viewport.Y - 15);

                    if (dm != null)
                    {
                        EdgeWarp edge = dm.EdgeWarps.Find((x) => x.ThisMapEdge == "Top" && x.FirstTile <= edgePos.X && x.LastTile >= edgePos.X);
                        if (edge != null)
                        {
                            Point pos = GetEdgeWarpDestination(edgePos.X, edge);
                            if (pos != Point.Zero)
                            {
                                Monitor.Log("warping north");
                                Game1.warpFarmer(edge.OtherMapName, pos.X, pos.Y, false);
                            }
                        }
                    }
                }
                else if (Game1.player.position.X > Game1.viewport.X + Game1.viewport.Width - 32)
                {
                    Game1.player.position.Value = new Vector2(Game1.viewport.X + Game1.viewport.Width - 33, Game1.player.position.Y);

                    if (dm != null)
                    {
                        EdgeWarp edge = dm.EdgeWarps.Find((x) => x.ThisMapEdge == "Right" && x.FirstTile <= edgePos.Y && x.LastTile >= edgePos.Y);
                        if (edge != null)
                        {
                            Point pos = GetEdgeWarpDestination(edgePos.Y, edge);
                            if (pos != Point.Zero)
                            {
                                Monitor.Log("warping east");
                                Game1.warpFarmer(edge.OtherMapName, pos.X, pos.Y, false);
                            }
                        }
                    }

                    if (Game1.currentLocation.Name == "Forest")
                    {
                        if (Game1.player.position.Y / 64 > 74)
                            Game1.warpFarmer("Beach", 0, 13, false);
                        else
                            Game1.warpFarmer("Town", 0, 100, false);
                    }
                }
                else if (Game1.player.position.X < Game1.viewport.X - 32)
                {
                    Game1.player.position.Value = new Vector2(Game1.viewport.X - 31, Game1.player.position.Y);

                    if (dm != null)
                    {
                        EdgeWarp edge = dm.EdgeWarps.Find((x) => x.ThisMapEdge == "Left" && x.FirstTile <= edgePos.X && x.LastTile >= edgePos.X);
                        if (edge != null)
                        {
                            Point pos = GetEdgeWarpDestination(edgePos.Y, edge);
                            if (pos != Point.Zero)
                            {
                                Monitor.Log("warping west");
                                Game1.warpFarmer(edge.OtherMapName, pos.X, pos.Y, false);
                            }
                        }
                    }

                    if (Game1.currentLocation.Name == "Town")
                    {
                        Game1.warpFarmer("Forest", 119, 43, false);
                    }
                    else if (Game1.currentLocation.Name == "Beach")
                    {
                        Game1.warpFarmer("Forest", 119, 111, false);
                    }
                }


                if (Game1.player.bathingClothes && IsWearingScubaGear() && !config.SwimSuitAlways)
                    Game1.player.changeOutOfSwimSuit();
                else if (!Game1.player.bathingClothes && (!IsWearingScubaGear() || config.SwimSuitAlways))
                    Game1.player.changeIntoSwimsuit();

                if (Game1.player.boots.Value != null && Game1.player.boots.Value.indexInTileSheet == scubaFinsID)
                {
                    int buffId = 42883167;
                    Buff buff = Game1.buffsDisplay.otherBuffs.FirstOrDefault((Buff p) => p.which == buffId);
                    if (buff == null)
                    {
                        BuffsDisplay buffsDisplay = Game1.buffsDisplay;
                        Buff buff2 = new Buff(0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 0, 0, 1, "Scuba Fins", Helper.Translation.Get("scuba-fins"));
                        buff2.which = buffId;
                        buff = buff2;
                        buffsDisplay.addOtherBuff(buff2);
                    }
                    buff.millisecondsDuration = 50;
                }
            }
            else
            {
                if (IsInWater() && !isJumping)
                {
                    Monitor.Log("In water not swimming");
                    Game1.player.swimming.Value = true;
                    if (!Game1.player.bathingClothes && !IsWearingScubaGear())
                        Game1.player.changeIntoSwimsuit();
                }

            }

            CheckIfMyButtonDown();

            if (!myButtonDown || Game1.player.millisecondsPlayed - lastJump < 250 || IsMapUnderwater(Game1.currentLocation.Name))
                return;

            if (Helper.Input.IsDown(SButton.MouseLeft) && !Game1.player.swimming && Game1.player.CurrentTool is WateringCan)
                return;


            List<Vector2> tiles = getSurroundingTiles();
            Vector2 jumpLocation = Vector2.Zero;

            double distance = -1;
            int maxDistance = 0;
            switch (Game1.player.FacingDirection)
            {
                case 0:
                    distance = Math.Abs(Game1.player.position.Y - tiles.Last().Y * Game1.tileSize);
                    maxDistance = 72;
                    break;
                case 2:
                    distance = Math.Abs(Game1.player.position.Y - tiles.Last().Y * Game1.tileSize);
                    maxDistance = 48;
                    break;
                case 1:
                case 3:
                    distance = Math.Abs(Game1.player.position.X - tiles.Last().X * Game1.tileSize);
                    maxDistance = 65;
                    break;
            }
            if (Helper.Input.IsDown(SButton.MouseLeft))
            {
                try
                {
                    int xTile = (Game1.viewport.X + Game1.getOldMouseX()) / 64;
                    int yTile = (Game1.viewport.Y + Game1.getOldMouseY()) / 64;
                    bool water = Game1.currentLocation.waterTiles[xTile, yTile];
                    if (Game1.player.swimming != water)
                    {
                        distance = -1;
                    }
                }
                catch
                {

                }
            }
            //Monitor.Log("Distance: " + distance);

            bool nextToLand = Game1.player.swimming && !Game1.currentLocation.isTilePassable(new Location((int)tiles.Last().X, (int)tiles.Last().Y), Game1.viewport) && distance < maxDistance;
            
            bool nextToWater = false;
            try
            {
                nextToWater = !Game1.player.swimming &&
                    (Game1.currentLocation.waterTiles[(int)tiles.Last().X, (int)tiles.Last().Y]
                        || !Game1.currentLocation.isTilePassable(new Location((int)tiles.Last().X, (int)tiles.Last().Y), Game1.viewport) && Game1.currentLocation.waterTiles[(int)tiles[tiles.Count - 2].X, (int)tiles[tiles.Count - 2].Y])
                    && distance < maxDistance;
            }
            catch
            {
                //Monitor.Log($"exception trying to get next to water: {ex}");
            }

            //Monitor.Log($"next to land: {nextToLand}, next to water: {nextToWater}");


            if (Helper.Input.IsDown(config.SwimKey) || nextToLand || nextToWater)
            {
                //Monitor.Log("okay to jump");
                foreach (Vector2 tile in tiles)
                {
                    bool isWater = false;
                    bool isPassable = false;
                    try
                    {
                        isPassable = Game1.currentLocation.isTilePassable(new Location((int)tile.X, (int)tile.Y), Game1.viewport);
                        isWater = Game1.currentLocation.waterTiles[(int)tile.X, (int)tile.Y];
                    }
                    catch (Exception exception)
                    {
                    }

                    if (nextToLand && !isWater && isPassable)
                    {
                        jumpLocation = tile;

                    }

                    if (nextToWater && isWater && isPassable)
                    {
                        jumpLocation = tile;

                    }


                }
            }

            if (jumpLocation != Vector2.Zero)
            {
                lastJump = Game1.player.millisecondsPlayed;
                //Monitor.Log("got swim location");
                if (Game1.player.swimming)
                {
                    willSwim = false;
                    Game1.player.swimming.Value = false;
                    Game1.player.freezePause = config.JumpTimeInMilliseconds;
                    Game1.currentLocation.playSound("dwop", NetAudio.SoundContext.Default);
                    Game1.currentLocation.playSound("waterSlosh", NetAudio.SoundContext.Default);
                }
                else
                {
                    willSwim = true;
                    if(!IsWearingScubaGear())
                        Game1.player.changeIntoSwimsuit();
                    
                    Game1.player.freezePause = config.JumpTimeInMilliseconds;
                    Game1.currentLocation.playSound("dwop", NetAudio.SoundContext.Default);
                }
                isJumping = true;
                startJumpLoc = Game1.player.position.Value;
                endJumpLoc = new Vector2(jumpLocation.X * Game1.tileSize, jumpLocation.Y * Game1.tileSize);
            }

        }

        private Point GetEdgeWarpDestination(int idxPos, EdgeWarp edge)
        {
            try
            {
                int idx = 1 + idxPos - edge.FirstTile;
                int length = 1 + edge.LastTile - edge.FirstTile;
                int otherLength = 1 + edge.OtherMapLastTile - edge.OtherMapFirstTile;
                int otherIdx = (int)Math.Round((idx / (float)length) * otherLength);
                int tileIdx = edge.OtherMapFirstTile - 1 + otherIdx;
                if (edge.DestinationHorizontal == true)
                {
                    Monitor.Log($"idx {idx} length {length} otherIdx {otherIdx} tileIdx {tileIdx} warp point: {tileIdx},{edge.OtherMapIndex}");
                    return new Point(tileIdx, edge.OtherMapIndex);
                }
                else
                {
                    Monitor.Log($"warp point: {edge.OtherMapIndex},{tileIdx}");
                    return new Point(edge.OtherMapIndex, tileIdx);
                }
            }
            catch
            {

            }
            return Point.Zero;
        }

        private void DiveTo(DiveLocation diveLocation)
        {
            DivePosition dp = diveLocation.OtherMapPos;
            if (dp == null)
            {
                Point pos = Game1.player.getTileLocationPoint();
                dp = new DivePosition()
                {
                    X = pos.X,
                    Y = pos.Y
                };
            }
            if (!IsMapUnderwater(Game1.currentLocation.Name))
            {
                bubbles.Clear();
                isUnderwater = true;
            }
            else
            {
                isUnderwater = false;
            }

            Game1.playSound("pullItemFromWater");
            Game1.warpFarmer(diveLocation.OtherMapName, dp.X, dp.Y, false);
        }

        private bool IsMapUnderwater(string name)
        {
            return diveMaps.ContainsKey(name) && diveMaps[name].Features.Contains("Underwater"); 
        }

        public static int abigailTicks;
        private SButton[] abigailShootButtons = new SButton[] { 
            SButton.Left,
            SButton.Right,
            SButton.Up,
            SButton.Down
        };

        private void AbigailCaveTick()
        {

            Game1.player.CurrentToolIndex = Game1.player.items.Count;

            List<NPC> list = Game1.currentLocation.characters.ToList().FindAll((n) => (n is Monster) && (n as Monster).Health <= 0);
            foreach(NPC n in list)
            {
                Game1.currentLocation.characters.Remove(n);
            }

            if (abigailTicks < 0)
            {
                return;
            }
            Game1.exitActiveMenu();

            if (abigailTicks == 0)
            {
                FieldInfo f1 = Game1.currentLocation.characters.GetType().GetField("OnValueRemoved", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                f1.SetValue(Game1.currentLocation.characters, null);
            }

            Vector2 v = Vector2.Zero;
            float yrt = (float)(1/Math.Sqrt(2));
            if (Helper.Input.IsDown(SButton.Up))
            {
                if(Helper.Input.IsDown(SButton.Right))
                    v = new Vector2(yrt, -yrt);
                else if(Helper.Input.IsDown(SButton.Left))
                    v = new Vector2(-yrt, -yrt);
                else 
                    v = new Vector2(0, -1);
            }
            else if (Helper.Input.IsDown(SButton.Down))
            {
                if(Helper.Input.IsDown(SButton.Right))
                    v = new Vector2(yrt, yrt);
                else if(Helper.Input.IsDown(SButton.Left))
                    v = new Vector2(-yrt, yrt);
                else 
                    v = new Vector2(0, 1);
            }
            else if (Helper.Input.IsDown(SButton.Right))
                v = new Vector2(1, 0);
            else if (Helper.Input.IsDown(SButton.Left))
                v = new Vector2(-1, 0);
            else if (Helper.Input.IsDown(SButton.MouseLeft))
            {
                float x = Game1.viewport.X + Game1.getOldMouseX() - Game1.player.position.X;
                float y = Game1.viewport.Y + Game1.getOldMouseY() - Game1.player.position.Y;
                float dx = Math.Abs(x);
                float dy = Math.Abs(y);
                if(y < 0)
                {
                    if(x > 0)
                    {
                        if(dy > dx)
                        {
                            if (dy-dx > dy / 2)
                                v = new Vector2(0, -1);
                            else
                                v = new Vector2(yrt, -yrt);

                        }
                        else
                        {
                            if (dx - dy > x / 2)
                                v = new Vector2(1, 0);
                            else
                                v = new Vector2(yrt, -yrt);
                        }
                    }
                    else
                    {
                        if (dy > dx)
                        {
                            if (dy - dx > dy / 2)
                                v = new Vector2(0, -1);
                            else
                                v = new Vector2(-yrt, -yrt);

                        }
                        else
                        {
                            if (dx - dy > x / 2)
                                v = new Vector2(-1, 0);
                            else
                                v = new Vector2(-yrt, -yrt);
                        }
                    }
                }
                else
                {
                    if (x > 0)
                    {
                        if (dy > dx)
                        {
                            if (dy - dx > dy / 2)
                                v = new Vector2(0, 1);
                            else
                                v = new Vector2(yrt, yrt);

                        }
                        else
                        {
                            if (dx - dy > x / 2)
                                v = new Vector2(1, 0);
                            else
                                v = new Vector2(yrt, yrt);
                        }
                    }
                    else
                    {
                        if (dy > dx)
                        {
                            if (dy - dx > dy / 2)
                                v = new Vector2(0, -1);
                            else
                                v = new Vector2(-yrt, yrt);

                        }
                        else
                        {
                            if (dx - dy > x / 2)
                                v = new Vector2(-1, 0);
                            else
                                v = new Vector2(-yrt, yrt);
                        }
                    }
                }
            }

            if (v != Vector2.Zero && Game1.player.millisecondsPlayed - lastProjectile > 350)
            {
                Game1.currentLocation.projectiles.Add(new AbigailProjectile(1, 383, 0, 0, 0, v.X * 6, v.Y * 6, new Vector2(Game1.player.getStandingX() - 24, Game1.player.getStandingY() - 48), "Cowboy_monsterDie", "Cowboy_gunshot", false, true, Game1.currentLocation, Game1.player, true)); 
                lastProjectile = Game1.player.millisecondsPlayed;
            }

            foreach (SButton button in abigailShootButtons)
            {
                if (Helper.Input.IsDown(button))
                {
                    switch (button)
                    {
                        case SButton.Up:
                            break;
                        case SButton.Right:
                            v = new Vector2(1, 0);
                            break;
                        case SButton.Down:
                            v = new Vector2(0, 1);
                            break;
                        default:
                            v = new Vector2(-1, 0);
                            break;
                    }
                }
            }
             

            abigailTicks++;
            if(abigailTicks > 80000 / 16f)
            {
                if (Game1.currentLocation.characters.ToList().FindAll((n) => (n is Monster)).Count > 0)
                    return;

                abigailTicks = -1;
                Game1.player.hat.Value = null;
                Game1.stopMusicTrack(Game1.MusicContext.Default);
                Game1.playSound("Cowboy_Secret");
                AddScubaChest(Game1.currentLocation, new Vector2(8, 8), "ScubaFins");
                Game1.currentLocation.setMapTile(8, 16, 91, "Buildings", null);
                Game1.currentLocation.setMapTile(9, 16, 92, "Buildings", null);
                Game1.currentLocation.setTileProperty(9, 16, "Back", "Water", "T");
                Game1.currentLocation.setMapTile(10, 16, 93, "Buildings", null);
                Game1.currentLocation.setMapTile(8, 17, 107, "Buildings", null);
                Game1.currentLocation.setMapTile(9, 17, 108, "Back", null);
                Game1.currentLocation.setTileProperty(9, 17, "Back", "Water", "T");
                Game1.currentLocation.removeTile(9, 17, "Buildings");
                Game1.currentLocation.setMapTile(10, 17, 109, "Buildings", null); 
                Game1.currentLocation.setMapTile(8, 18, 139, "Buildings", null);
                Game1.currentLocation.setMapTile(9, 18, 140, "Buildings", null);
                Game1.currentLocation.setMapTile(10, 18, 141, "Buildings", null);
                AddWaterTiles(Game1.currentLocation);
            }
            else
            {
                if (Game1.random.NextDouble() < 0.04)
                {
                    int which = Game1.random.Next(3);
                    Point p = new Point();
                    switch (Game1.random.Next(4))
                    {
                        case 0:
                            p = new Point(8 + which, 1);
                            break;
                        case 1:
                            p = new Point(1, 8 + which);
                            break;
                        case 2:
                            p = new Point(8 + which, 16);
                            break;
                        case 3:
                            p = new Point(16, 8 + which);
                            break;
                    }
                    Game1.currentLocation.characters.Add(new AbigailMetalHead(new Vector2(p.X * Game1.tileSize, p.Y * Game1.tileSize), 0));
                }

            }
        }

        private bool IsWearingScubaGear()
        {
            bool tank = scubaTankID != -1 && Game1.player.shirtItem != null && Game1.player.shirtItem.Value != null && Game1.player.shirtItem.Value.parentSheetIndex != null &&  Game1.player.shirtItem.Value.parentSheetIndex == scubaTankID;
            bool mask = scubaMaskID != -1 && Game1.player.hat != null && Game1.player.hat.Value != null && Game1.player.hat.Value.which != null &&  Game1.player.hat.Value.which == scubaMaskID;

            return tank && mask;
        }

        private bool IsInWater()
        {
            var tiles = Game1.player.currentLocation.waterTiles;
            Point p = Game1.player.getTileLocationPoint();

            if (!Game1.player.swimming && Game1.currentLocation.map.GetLayer("Buildings").PickTile(new Location(p.X, p.Y) * Game1.tileSize, Game1.viewport.Size) != null)
                return false;

            return IsMapUnderwater(Game1.currentLocation.Name)
                ||
                (tiles != null
                    && (
                            (p.X >= 0 && p.Y >= 0 && tiles.GetLength(0) > p.X && tiles.GetLength(1) > p.Y && tiles[p.X, p.Y])
                            ||
                            (Game1.player.swimming &&
                                (p.X < 0 || p.Y < 0 || tiles.GetLength(0) <= p.X || tiles.GetLength(1) <= p.Y)
                            )
                    )
                );
        }

        private int MaxOxygen()
        {
            return Game1.player.MaxStamina * Math.Max(1,config.OxygenMult);
        }

        private List<Vector2> getSurroundingTiles()
        {
            List<Vector2> tiles = new List<Vector2>();
            int dir = Game1.player.facingDirection;
            if (dir == 1)
            {

                for (int i = 4; i > 0; i--)
                {
                    tiles.Add(Game1.player.getTileLocation() + new Vector2(i, 0));
                }

            }

            if (dir == 2)
            {

                for (int i = 4; i > 0; i--)
                {
                    tiles.Add(Game1.player.getTileLocation() + new Vector2(0, i));
                }

            }

            if (dir == 3)
            {

                for (int i = 4; i > 0; i--)
                {
                    tiles.Add(Game1.player.getTileLocation() - new Vector2(i, 0));
                }

            }

            if (dir == 0)
            {

                for (int i = 4; i > 0; i--)
                {
                    tiles.Add(Game1.player.getTileLocation() - new Vector2(0, i));
                }

            }

            return tiles;

        }

        public void MakeOxygenBar(int current, int max)
        {
            OxygenBarTexture = new Texture2D(Game1.graphics.GraphicsDevice, (int)Math.Round(Game1.viewport.Width * 0.74f), 30);
            Color[] data = new Color[OxygenBarTexture.Width * OxygenBarTexture.Height];
            OxygenBarTexture.GetData(data);
            for (int i = 0; i < data.Length; i++)
            {
                if (i <= OxygenBarTexture.Width || i % OxygenBarTexture.Width == OxygenBarTexture.Width - 1)
                {
                    data[i] = new Color(0.5f, 1f, 0.5f);
                }
                else if (data.Length - i < OxygenBarTexture.Width || i % OxygenBarTexture.Width == 0)
                {
                    data[i] = new Color(0, 0.5f, 0);
                }
                else if ((i % OxygenBarTexture.Width) / (float)OxygenBarTexture.Width < (float)current / (float)max)
                {
                    data[i] = Color.Green;
                }
                else
                {
                    data[i] = Color.Black;
                }
            }
            OxygenBarTexture.SetData<Color>(data);
        }
        
        private void AddMinerals(GameLocation l)
        {
            List<Vector2> spots = new List<Vector2>();
            for (int x = 0; x < l.map.Layers[0].LayerWidth; x++)
            {
                for (int y = 0; y < l.map.Layers[0].LayerHeight; y++)
                {
                    Tile tile = l.map.GetLayer("Back").PickTile(new Location(x, y) * Game1.tileSize, Game1.viewport.Size);
                    if (tile != null && l.map.GetLayer("Buildings").PickTile(new Location(x, y) * Game1.tileSize, Game1.viewport.Size) == null && l.map.GetLayer("Front").PickTile(new Location(x, y) * Game1.tileSize, Game1.viewport.Size) == null)
                    {
                        spots.Add(new Vector2(x, y));
                    }
                }
            }
            int n = spots.Count;
            while (n > 1)
            {
                n--;
                int k = Game1.random.Next(n + 1);
                var value = spots[k];
                spots[k] = spots[n];
                spots[n] = value;
            }

            int mineralNo = Game1.random.Next(10, 30);
            List<Vector2> mineralSpots = spots.Take(mineralNo).ToList();

            foreach (Vector2 tile in mineralSpots)
            {
                double chance = Game1.random.NextDouble();

                if (chance < 0.2)
                {
                    l.map.GetLayer("Back").Tiles[(int)tile.X, (int)tile.Y].TileIndex = 1299;
                    l.map.GetLayer("Back").Tiles[(int)tile.X, (int)tile.Y].Properties.Add("Treasure", new PropertyValue("Object " + checkForBuriedItem(Game1.player)));
                    l.map.GetLayer("Back").Tiles[(int)tile.X, (int)tile.Y].Properties.Add("Diggable", new PropertyValue("T"));
                }
                else if (chance < 0.4)
                {
                    l.overlayObjects[tile] = new StardewValley.Object(tile, 751, "Stone", true, false, false, false)
                    {
                        MinutesUntilReady = 2
                    };
                }
                else if (chance < 0.5)
                {
                    l.overlayObjects[tile] = new StardewValley.Object(tile, 290, "Stone", true, false, false, false)
                    {
                        MinutesUntilReady = 4 
                    };
                }
                else if (chance < 0.55)
                {
                    l.overlayObjects[tile] = new StardewValley.Object(tile, 764, "Stone", true, false, false, false)
                    {
                        MinutesUntilReady = 8
                    };
                }
                else if (chance < 0.56)
                {
                    l.overlayObjects[tile] = new StardewValley.Object(tile, 765, "Stone", true, false, false, false)
                    {
                        MinutesUntilReady = 16
                    };
                }
                else if (chance < 0.65)
                {
                    l.overlayObjects[tile] = new StardewValley.Object(tile, 80, "Stone", true, true, false, true);
                }
                else if (chance < 0.74)
                {
                    l.overlayObjects[tile] = new StardewValley.Object(tile, 82, "Stone", true, true, false, true);
                }
                else if (chance < 0.83)
                {
                    l.overlayObjects[tile] = new StardewValley.Object(tile, 84, "Stone", true, true, false, true);
                }
                else if (chance < 0.90)
                {
                    l.overlayObjects[tile] = new StardewValley.Object(tile, 86, "Stone", true, true, false, true);
                }
                else
                {
                    int[] gems = { 4,6,8,10,12,14,40 };
                    int whichGem = gems[Game1.random.Next(gems.Length)];
                    l.overlayObjects[tile] = new StardewValley.Object(tile, whichGem, "Stone", true, false, false, false)
				    {
                        MinutesUntilReady = 5

                    };
                }
            }
        }

        public int checkForBuriedItem(Farmer who)
        {
            int objectIndex = 330;
            if (Game1.random.NextDouble() < 0.1)
            {
                if (Game1.random.NextDouble() < 0.75)
                {
                    switch (Game1.random.Next(5))
                    {
                        case 0:
                            objectIndex = 96;
                            break;
                        case 1:
                            objectIndex = (who.hasOrWillReceiveMail("lostBookFound") ? ((Game1.netWorldState.Value.LostBooksFound < 21) ? 102 : 770) : 770);
                            break;
                        case 2:
                            objectIndex = 110;
                            break;
                        case 3:
                            objectIndex = 112;
                            break;
                        case 4:
                            objectIndex = 585;
                            break;
                    }
                }
                else if (Game1.random.NextDouble() < 0.75)
                {
                    var r = Game1.random.NextDouble();
                    
                    if (r < 0.75)
                    {
                        objectIndex = ((Game1.random.NextDouble() < 0.5) ? 121 : 97);
                    }
                    else if (r < 0.80)
                    {
                        objectIndex = 99;
                    }
                    else
                    {
                        objectIndex = ((Game1.random.NextDouble() < 0.5) ? 122 : 336);
                    }
                }
                else
                {
                    objectIndex = ((Game1.random.NextDouble() < 0.5) ? 126 : 127);
                }
            }
            else
            {
                if (Game1.random.NextDouble() < 0.5)
                {
                    objectIndex = 330;
                }
                else
                {
                    if (Game1.random.NextDouble() < 0.25)
                    {
                        objectIndex = 749;
                    }
                    else if (Game1.random.NextDouble() < 0.5)
                    {
                        var r = Game1.random.NextDouble();
                        if (r < 0.7)
                        {
                            objectIndex = 535;
                        }
                        else if (r < 8.5)
                        {
                            objectIndex = 537;
                        }
                        else
                        {
                            objectIndex = 536;
                        }
                    }
                }
            }
            return objectIndex;
        }
        private void AddFishies(GameLocation l, bool smol = true)
        {
            if (config.AddFishies)
            {
                List<Vector2> spots = new List<Vector2>();
                for (int x = 0; x < l.map.Layers[0].LayerWidth; x++)
                {
                    for (int y = 0; y < l.map.Layers[0].LayerHeight; y++)
                    {
                        Tile tile = l.map.GetLayer("Back").PickTile(new Location(x, y) * Game1.tileSize, Game1.viewport.Size);
                        if (tile != null && l.map.GetLayer("Buildings").PickTile(new Location(x, y) * Game1.tileSize, Game1.viewport.Size) == null && l.map.GetLayer("Front").PickTile(new Location(x, y) * Game1.tileSize, Game1.viewport.Size) == null && !l.overlayObjects.ContainsKey(new Vector2(x, y)))
                        {
                            spots.Add(new Vector2(x, y));
                        }
                    }
                }
                int n = spots.Count;
                while (n > 1)
                {
                    n--;
                    int k = Game1.random.Next(n + 1);
                    var value = spots[k];
                    spots[k] = spots[n];
                    spots[n] = value;
                }
                l.characters.Clear();
                if (smol)
                {
                    int fishes = Game1.random.Next(50, 100);
                    for (int i = 0; i < fishes; i++)
                    {
                        int idx = Game1.random.Next(spots.Count);
                        l.characters.Add(new Fishie(new Vector2(spots[idx].X * Game1.tileSize, spots[idx].Y * Game1.tileSize)));
                    }
                }
                else
                {
                    int bigFishes = Game1.random.Next(10, 20);
                    for (int i = 0; i < bigFishes; i++)
                    {
                        int idx = Game1.random.Next(spots.Count);
                        l.characters.Add(new BigFishie(new Vector2(spots[idx].X * Game1.tileSize, spots[idx].Y * Game1.tileSize)));
                    }
                }
            }
        }
        private void AddOceanForage(GameLocation l)
        {
            List<Vector2> spots = new List<Vector2>();
            for (int x = 0; x < l.map.Layers[0].LayerWidth; x++)
            {
                for (int y = 0; y < l.map.Layers[0].LayerHeight; y++)
                {
                    Tile tile = l.map.GetLayer("Back").PickTile(new Location(x, y) * Game1.tileSize, Game1.viewport.Size);
                    if (tile != null && l.map.GetLayer("Buildings").PickTile(new Location(x, y) * Game1.tileSize, Game1.viewport.Size) == null && l.map.GetLayer("Front").PickTile(new Location(x, y) * Game1.tileSize, Game1.viewport.Size) == null && !l.overlayObjects.ContainsKey(new Vector2(x, y)))
                    {
                        spots.Add(new Vector2(x, y));
                    }
                }
            }
            int n = spots.Count;
            while (n > 1)
            {
                n--;
                int k = Game1.random.Next(n + 1);
                var value = spots[k];
                spots[k] = spots[n];
                spots[n] = value;
            }
            int forageNo = Game1.random.Next(5, 20);
            List<Vector2> forageSpots = spots.Take(forageNo).ToList();

            foreach (Vector2 v in forageSpots)
            {
                double chance = Game1.random.NextDouble();
                if (chance < 0.25)
                {
                    l.overlayObjects[v] = new StardewValley.Object(v, 152, "Seaweed", true, true, false, true)
                    {
                        CanBeGrabbed = true
                    };
                }
                else if (chance < 0.4)
                {
                    l.overlayObjects[v] = new StardewValley.Object(v, 153, "Green Algae", true, true, false, true)
                    {
                        CanBeGrabbed = true
                    };
                }
                else if (chance < 0.6)
                {
                    l.overlayObjects[v] = new StardewValley.Object(v, 157, "White Algae", true, true, false, true)
                    {
                        CanBeGrabbed = true
                    };
                }
                else if (chance < 0.75)
                {
                    l.overlayObjects[v] = new StardewValley.Object(v, 372, "Clam", true, true, false, true)
                    {
                        CanBeGrabbed = true
                    };
                }
                else if (chance < 0.85)
                {
                    l.overlayObjects[v] = new StardewValley.Object(v, 393, "Coral", true, true, false, true)
                    {
                        CanBeGrabbed = true
                    };
                }
                else if (chance < 0.94)
                {
                    l.overlayObjects[v] = new StardewValley.Object(v, 397, "Sea Urchin", true, true, false, true)
                    {
                        CanBeGrabbed = true
                    };
                }
                else if (chance < 0.97)
                {
                    l.overlayObjects[v] = new StardewValley.Object(v, 394, "Rainbow Shell", true, true, false, true)
                    {
                        CanBeGrabbed = true
                    };
                }
                else
                {
                    l.overlayObjects[v] = new StardewValley.Object(v, 392, "Nautilus Shell", true, true, false, true)
                    {
                        CanBeGrabbed = true
                    };
                }
            }
        }
        private void AddOceanTreasure(GameLocation l)
        {
            List<Vector2> spots = new List<Vector2>();
            for (int x = 0; x < l.map.Layers[0].LayerWidth; x++)
            {
                for (int y = 0; y < l.map.Layers[0].LayerHeight; y++)
                {
                    Tile tile = l.map.GetLayer("Back").PickTile(new Location(x, y) * Game1.tileSize, Game1.viewport.Size);
                    if (tile != null && l.map.GetLayer("Buildings").PickTile(new Location(x, y) * Game1.tileSize, Game1.viewport.Size) == null && l.map.GetLayer("Front").PickTile(new Location(x, y) * Game1.tileSize, Game1.viewport.Size) == null && !l.overlayObjects.ContainsKey(new Vector2(x, y)))
                    {
                        spots.Add(new Vector2(x, y));
                    }
                }
            }
            int n = spots.Count;
            while (n > 1)
            {
                n--;
                int k = Game1.random.Next(n + 1);
                var value = spots[k];
                spots[k] = spots[n];
                spots[n] = value;
            }
            List<Vector2> treasureSpots = new List<Vector2>();

            foreach (Vector2 v in treasureSpots)
            {

                List<Item> treasures = new List<Item>();
                float chance = 1f;
                while (Game1.random.NextDouble() <= (double)chance)
                {
                    chance *= 0.4f;
                    if (Game1.random.NextDouble() < 0.5)
                    {
                        treasures.Add(new StardewValley.Object(774, 2 + ((Game1.random.NextDouble() < 0.25) ? 2 : 0), false, -1, 0));
                    }
                    switch (Game1.random.Next(4))
                    {
                        case 0:
                            if (Game1.random.NextDouble() < 0.03)
                            {
                                treasures.Add(new StardewValley.Object(386, Game1.random.Next(1, 3), false, -1, 0));
                            }
                            else
                            {
                                List<int> possibles = new List<int>();
                                possibles.Add(384);
                                if (possibles.Count == 0 || Game1.random.NextDouble() < 0.6)
                                {
                                    possibles.Add(380);
                                }
                                if (possibles.Count == 0 || Game1.random.NextDouble() < 0.6)
                                {
                                    possibles.Add(378);
                                }
                                if (possibles.Count == 0 || Game1.random.NextDouble() < 0.6)
                                {
                                    possibles.Add(388);
                                }
                                if (possibles.Count == 0 || Game1.random.NextDouble() < 0.6)
                                {
                                    possibles.Add(390);
                                }
                                possibles.Add(382);
                                treasures.Add(new StardewValley.Object(possibles.ElementAt(Game1.random.Next(possibles.Count)), Game1.random.Next(2, 7) * ((Game1.random.NextDouble() < 0.05 + (double)Game1.player.luckLevel * 0.015) ? 2 : 1), false, -1, 0));
                                if (Game1.random.NextDouble() < 0.05 + (double)Game1.player.LuckLevel * 0.03)
                                {
                                    treasures.Last<Item>().Stack *= 2;
                                }
                            }
                            break;
                        case 1:
                            if (Game1.random.NextDouble() < 0.1)
                            {
                                treasures.Add(new StardewValley.Object(687, 1, false, -1, 0));
                            }
                            else if (Game1.random.NextDouble() < 0.25 && Game1.player.craftingRecipes.ContainsKey("Wild Bait"))
                            {
                                treasures.Add(new StardewValley.Object(774, 5 + ((Game1.random.NextDouble() < 0.25) ? 5 : 0), false, -1, 0));
                            }
                            else
                            {
                                treasures.Add(new StardewValley.Object(685, 10, false, -1, 0));
                            }
                            break;
                        case 2:
                            if (Game1.random.NextDouble() < 0.1 && Game1.netWorldState.Value.LostBooksFound < 21 && Game1.player.hasOrWillReceiveMail("lostBookFound"))
                            {
                                treasures.Add(new StardewValley.Object(102, 1, false, -1, 0));
                            }
                            else if (Game1.player.archaeologyFound.Count() > 0)
                            {
                                if (Game1.random.NextDouble() < 0.25)
                                {
                                    treasures.Add(new StardewValley.Object(Game1.random.Next(585, 588), 1, false, -1, 0));
                                }
                                else if (Game1.random.NextDouble() < 0.5)
                                {
                                    treasures.Add(new StardewValley.Object(Game1.random.Next(103, 120), 1, false, -1, 0));
                                }
                                else
                                {
                                    treasures.Add(new StardewValley.Object(535, 1, false, -1, 0));
                                }
                            }
                            else
                            {
                                treasures.Add(new StardewValley.Object(382, Game1.random.Next(1, 3), false, -1, 0));
                            }
                            break;
                        case 3:
                            switch (Game1.random.Next(3))
                            {
                                case 0:
                                    switch (Game1.random.Next(3))
                                    {
                                        case 0:
                                            treasures.Add(new StardewValley.Object(537 + ((Game1.random.NextDouble() < 0.4) ? Game1.random.Next(-2, 0) : 0), Game1.random.Next(1, 4), false, -1, 0));
                                            break;
                                        case 1:
                                            treasures.Add(new StardewValley.Object(536 + ((Game1.random.NextDouble() < 0.4) ? -1 : 0), Game1.random.Next(1, 4), false, -1, 0));
                                            break;
                                        case 2:
                                            treasures.Add(new StardewValley.Object(535, Game1.random.Next(1, 4), false, -1, 0));
                                            break;
                                    }
                                    if (Game1.random.NextDouble() < 0.05 + (double)Game1.player.LuckLevel * 0.03)
                                    {
                                        treasures.Last<Item>().Stack *= 2;
                                    }
                                    break;
                                case 1:
                                    switch (Game1.random.Next(4))
                                    {
                                        case 0:
                                            treasures.Add(new StardewValley.Object(382, Game1.random.Next(1, 4), false, -1, 0));
                                            break;
                                        case 1:
                                            treasures.Add(new StardewValley.Object((Game1.random.NextDouble() < 0.3) ? 82 : ((Game1.random.NextDouble() < 0.5) ? 64 : 60), Game1.random.Next(1, 3), false, -1, 0));
                                            break;
                                        case 2:
                                            treasures.Add(new StardewValley.Object((Game1.random.NextDouble() < 0.3) ? 84 : ((Game1.random.NextDouble() < 0.5) ? 70 : 62), Game1.random.Next(1, 3), false, -1, 0));
                                            break;
                                        case 3:
                                            treasures.Add(new StardewValley.Object((Game1.random.NextDouble() < 0.3) ? 86 : ((Game1.random.NextDouble() < 0.5) ? 66 : 68), Game1.random.Next(1, 3), false, -1, 0));
                                            break;
                                    }
                                    if (Game1.random.NextDouble() < 0.05)
                                    {
                                        treasures.Add(new StardewValley.Object(72, 1, false, -1, 0));
                                    }
                                    if (Game1.random.NextDouble() < 0.05)
                                    {
                                        treasures.Last<Item>().Stack *= 2;
                                    }
                                    break;
                                case 2:
                                    if (Game1.player.FishingLevel < 2)
                                    {
                                        treasures.Add(new StardewValley.Object(770, Game1.random.Next(1, 4), false, -1, 0));
                                    }
                                    else
                                    {
                                        float luckModifier = (1f + (float)Game1.player.DailyLuck);
                                        if (Game1.random.NextDouble() < 0.05 * (double)luckModifier && !Game1.player.specialItems.Contains(14))
                                        {
                                            treasures.Add(new MeleeWeapon(14)
                                            {
                                                specialItem = true
                                            });
                                        }
                                        if (Game1.random.NextDouble() < 0.05 * (double)luckModifier && !Game1.player.specialItems.Contains(51))
                                        {
                                            treasures.Add(new MeleeWeapon(51)
                                            {
                                                specialItem = true
                                            });
                                        }
                                        if (Game1.random.NextDouble() < 0.07 * (double)luckModifier)
                                        {
                                            switch (Game1.random.Next(3))
                                            {
                                                case 0:
                                                    treasures.Add(new Ring(516 + ((Game1.random.NextDouble() < (double)((float)Game1.player.LuckLevel / 11f)) ? 1 : 0)));
                                                    break;
                                                case 1:
                                                    treasures.Add(new Ring(518 + ((Game1.random.NextDouble() < (double)((float)Game1.player.LuckLevel / 11f)) ? 1 : 0)));
                                                    break;
                                                case 2:
                                                    treasures.Add(new Ring(Game1.random.Next(529, 535)));
                                                    break;
                                            }
                                        }
                                        if (Game1.random.NextDouble() < 0.02 * (double)luckModifier)
                                        {
                                            treasures.Add(new StardewValley.Object(166, 1, false, -1, 0));
                                        }
                                        if (Game1.random.NextDouble() < 0.001 * (double)luckModifier)
                                        {
                                            treasures.Add(new StardewValley.Object(74, 1, false, -1, 0));
                                        }
                                        if (Game1.random.NextDouble() < 0.01 * (double)luckModifier)
                                        {
                                            treasures.Add(new StardewValley.Object(127, 1, false, -1, 0));
                                        }
                                        if (Game1.random.NextDouble() < 0.01 * (double)luckModifier)
                                        {
                                            treasures.Add(new StardewValley.Object(126, 1, false, -1, 0));
                                        }
                                        if (Game1.random.NextDouble() < 0.01 * (double)luckModifier)
                                        {
                                            treasures.Add(new Ring(527));
                                        }
                                        if (Game1.random.NextDouble() < 0.01 * (double)luckModifier)
                                        {
                                            treasures.Add(new Boots(Game1.random.Next(504, 514)));
                                        }
                                        if (treasures.Count == 1)
                                        {
                                            treasures.Add(new StardewValley.Object(72, 1, false, -1, 0));
                                        }
                                    }
                                    break;
                            }
                            break;
                    }
                }
                if (treasures.Count == 0)
                {
                    treasures.Add(new StardewValley.Object(685, Game1.random.Next(1, 4) * 5, false, -1, 0));
                }
                if (treasures.Count > 0)
                {
                    Color tint = Color.White;
                    l.overlayObjects[v] = new Chest(Game1.random.Next(0, 1000), treasures, v, false, 0)
                    {
                        Tint = tint
                    };
                }
            }
        }

        /// <summary>Get whether this instance can load the initial version of the given asset.</summary>
        /// <param name="asset">Basic metadata about the asset being loaded.</param>
        public bool CanLoad<T>(IAssetInfo asset)
        {
            if (!config.EnableMod)
                return false;

            if (asset.AssetName.StartsWith("Fishies"))
            {
                return true;
            }

            string name = asset.AssetName.Replace("/", "\\");

            if (name.Equals("Maps\\CrystalCave") || name.Equals("Maps\\CrystalCaveDark"))
            {
                return true;
            }


            return false;
        }

        /// <summary>Load a matched asset.</summary>
        /// <param name="asset">Basic metadata about the asset being loaded.</param>
        public T Load<T>(IAssetInfo asset)
        {
            Monitor.Log($"loading asset for {asset.AssetName}");


            string name = asset.AssetName.Replace("/", "\\");

            if (name.Equals("Maps\\CrystalCave") || name.Equals("Maps\\CrystalCaveDark"))
            {
                return (T)(object)Helper.Content.Load<Map>($"assets/tmx-pack/assets/{name.Substring(5)}.tbin");
            }

             
            if (asset.AssetName.StartsWith("Fishies"))
            {
                return (T)(object)Helper.Content.Load<Texture2D>($"assets/{asset.AssetName}.png");
            }
            throw new InvalidDataException(); 
        }

                /// <summary>Get whether this instance can edit the given asset.</summary>
                /// <param name="asset">Basic metadata about the asset being loaded.</param>
        public bool CanEdit<T>(IAssetInfo asset)
        {
            if (!config.EnableMod)
                return false;

            foreach(string key in changeLocations.Keys)
            {
                if (asset.AssetNameEquals($"Maps/{key}"))
                    return true;

            }

            return false;
        }

        /// <summary>Edit a matched asset.</summary>
        /// <param name="asset">A helper which encapsulates metadata about an asset and enables changes to it.</param>
        public void Edit<T>(IAssetData asset)
        {
            Monitor.Log("Editing asset: " + asset.AssetName);

            string mapName = asset.AssetName.Replace("Maps/", "").Replace("Maps\\", "");

            if (changeLocations.ContainsKey(mapName))
            {
                IAssetDataForMap map = asset.AsMap();
                for (int x = 0; x < map.Data.Layers[0].LayerWidth; x++)
                {
                    for (int y = 0; y < map.Data.Layers[0].LayerHeight; y++)
                    {
                        if (doesTileHaveProperty(map.Data, x, y, "Water", "Back") != null)
                        {
                            Tile tile = map.Data.GetLayer("Back").PickTile(new Location(x, y) * Game1.tileSize, Game1.viewport.Size);
                            if (tile != null && (((mapName == "Beach" || mapName == "UnderwaterBeach") && x > 58 && x < 61 && y > 11 && y < 15) || mapName != "Beach"))
                            {
                                if (tile.TileIndexProperties.ContainsKey("Passable"))
                                {
                                    tile.TileIndexProperties.Remove("Passable");
                                }
                            }
                            tile = map.Data.GetLayer("Front").PickTile(new Location(x, y) * Game1.tileSize, Game1.viewport.Size);
                            if (tile != null)
                            {
                                if (tile.TileIndexProperties.ContainsKey("Passable"))
                                {
                                    //tile.TileIndexProperties.Remove("Passable");
                                }
                            }
                            if (map.Data.GetLayer("AlwaysFront") != null)
                            {
                                tile = map.Data.GetLayer("AlwaysFront").PickTile(new Location(x, y) * Game1.tileSize, Game1.viewport.Size);
                                if (tile != null)
                                {
                                    if (tile.TileIndexProperties.ContainsKey("Passable"))
                                    {
                                        //tile.TileIndexProperties.Remove("Passable");
                                    }
                                }
                            }
                            tile = map.Data.GetLayer("Buildings").PickTile(new Location(x, y) * Game1.tileSize, Game1.viewport.Size);
                            if (tile != null)
                            {
                                if (
                                    ((mapName == "Beach" || mapName == "UnderwaterBeach") && x > 58 && x < 61 && y > 11 && y < 15) ||
                                    (mapName != "Beach" && mapName != "UnderwaterBeach"
                                        && ((tile.TileIndex > 1292 && tile.TileIndex < 1297) || (tile.TileIndex > 1317 && tile.TileIndex < 1322)
                                            || (tile.TileIndex % 25 > 17 && tile.TileIndex / 25 < 53 && tile.TileIndex / 25 > 48)
                                            || (tile.TileIndex % 25 > 1 && tile.TileIndex % 25 < 7 && tile.TileIndex / 25 < 53 && tile.TileIndex / 25 > 48)
                                            || (tile.TileIndex % 25 > 11 && tile.TileIndex / 25 < 51 && tile.TileIndex / 25 > 48)
                                            || (tile.TileIndex % 25 > 10 && tile.TileIndex % 25 < 14 && tile.TileIndex / 25 < 49 && tile.TileIndex / 25 > 46)
                                            || tile.TileIndex == 734 || tile.TileIndex == 759
                                            || tile.TileIndex == 628 || tile.TileIndex == 629
                                            || (mapName == "Forest" && x == 119 && ((y > 42 && y < 48) || (y > 104 && y < 119)))
                                    
                                        )
                                    )
                                )
                                {
                                    if (tile.TileIndexProperties.ContainsKey("Passable"))
                                    {
                                        tile.TileIndexProperties["Passable"] = "T";
                                    }
                                    else
                                    {
                                        tile.TileIndexProperties.Add("Passable", "T");
                                    }

                                }
                            }
                        }
                    }
                }
            }
        }

        public void RemoveWaterTiles(GameLocation l)
        {
            if (l == null || l.map == null)
                return;
            Map map = l.map;
            string mapName = l.Name;
            for (int x = 0; x < map.Layers[0].LayerWidth; x++)
            {
                for (int y = 0; y < map.Layers[0].LayerHeight; y++)
                {
                    if (doesTileHaveProperty(map, x, y, "Water", "Back") != null)
                    {
                        Tile tile = map.GetLayer("Back").PickTile(new Location(x, y) * Game1.tileSize, Game1.viewport.Size);
                        if (tile != null)
                            tile.TileIndexProperties.Remove("Water");
                    }
                }
            }
        }

        public string doesTileHaveProperty(Map map, int xTile, int yTile, string propertyName, string layerName)
        {
            PropertyValue property = null;
            if (map != null && map.GetLayer(layerName) != null)
            {
                Tile tmp = map.GetLayer(layerName).PickTile(new Location(xTile * 64, yTile * 64), Game1.viewport.Size);
                if (tmp != null)
                {
                    tmp.TileIndexProperties.TryGetValue(propertyName, out property);
                }
                if (property == null && tmp != null)
                {
                    map.GetLayer(layerName).PickTile(new Location(xTile * 64, yTile * 64), Game1.viewport.Size).Properties.TryGetValue(propertyName, out property);
                }
            }
            if (property != null)
            {
                return property.ToString();
            }
            return null;
        }
    }
}
