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
using StardewValley.Locations;

namespace Swim
{
    public class ModEntry : Mod, IAssetEditor, IAssetLoader
    {
        
        private ModConfig config;
        private IJsonAssetsApi JsonAssets;
        private int scubaMaskID = -1;
        private int scubaTankID = -1;
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
            "Mountain",
            "UnderwaterBeach",
            "UnderwaterMountain",
            "ScubaCave",
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


            SwimPatches.Initialize(Monitor, helper, config);

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
        }

        private void Player_InventoryChanged(object sender, InventoryChangedEventArgs e)
        {
            if (e.Player != Game1.player)
                return;

            if (!Game1.player.mailReceived.Contains("ScubaGear") && scubaMaskID != -1 && scubaTankID != -1)
            {
                if(e.Added.First().parentSheetIndex == scubaMaskID || e.Added.First().parentSheetIndex == scubaTankID)
                {
                    Monitor.Log("Player found scuba gear");
                    Game1.player.mailReceived.Add("ScubaGear");
                }
            }
        }

        private void GameLoop_SaveLoaded(object sender, SaveLoadedEventArgs e)
        {
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
                }
                if (scubaTankID == -1)
                {
                    Monitor.Log("Can't get ID for Scuba Tank. Some functionality will be lost.", LogLevel.Warn);
                }
                else
                {
                    Monitor.Log(string.Format("Scuba Tank ID is {0}.", scubaTankID), LogLevel.Debug);
                }
            }
        }

        public static int ticksUnderwater = 0;
        public static int bubbleOffset = 0;

        private void Display_RenderedWorld(object sender, RenderedWorldEventArgs e)
        {
            if (isUnderwater && Game1.currentLocation.Name.StartsWith("Underwater"))
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
            int maxOx = MaxOxygen();
            if (oxygen < maxOx)
            {
                MakeOxygenBar();
                e.SpriteBatch.Draw(OxygenBarTexture, new Vector2((int)Math.Round(Game1.viewport.Width * 0.13f), 100), Color.White);
            }
        }

        private void GameLoop_GameLaunched(object sender, GameLaunchedEventArgs e)
        {
            IContentPack contentPack = this.Helper.ContentPacks.CreateFake(Path.Combine(this.Helper.DirectoryPath, "assets/tmx-pack"));

            object api = Helper.ModRegistry.GetApi("Platonymous.TMXLoader");
            if (api != null)
            {
                Helper.Reflection.GetMethod(api, "AddContentPack").Invoke(contentPack);
            }

            JsonAssets = Helper.ModRegistry.GetApi<IJsonAssetsApi>("spacechase0.JsonAssets");
            bool flag = this.JsonAssets == null;
            if (flag)
            {
                base.Monitor.Log("Can't load Json Assets API for scuba gear");
            }
            else
            {
                this.JsonAssets.LoadAssets(Path.Combine(base.Helper.DirectoryPath, "assets/json-assets"));
            }
        }

        private void GameLoop_DayStarted(object sender, DayStartedEventArgs e)
        {
            if (Game1._locationLookup.ContainsKey("UnderwaterBeach"))
            {
                AddTreasure(Game1._locationLookup["UnderwaterBeach"]);
            }
            if (Game1._locationLookup.ContainsKey("UnderwaterMountain"))
            {
                AddMinerals(Game1._locationLookup["UnderwaterMountain"]);
            }
            if (Game1._locationLookup.ContainsKey("ScubaCave"))
            {
                AddWaterTiles(Game1._locationLookup["ScubaCave"]);
                AddScubaChest(Game1._locationLookup["ScubaCave"]);
            }
            oxygen = MaxOxygen();
        }

        private void AddScubaChest(GameLocation gameLocation)
        {
            if (!Game1.player.mailReceived.Contains("ScubaGear") && scubaMaskID != -1 && scubaTankID != -1)
            {
                var loc = new Vector2(10, 14);
                var loc2 = new Vector2(11, 14);
                gameLocation.overlayObjects[loc] = new Chest(0, new List<Item>() { new Clothing(scubaTankID) }, loc, false, 0);
                gameLocation.overlayObjects[loc2] = new Chest(0, new List<Item>() { new Hat(scubaMaskID) }, loc2, false, 0);
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
                gameLocation.waterTiles = null;
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


        private void Input_ButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (Game1.player == null || Game1.currentLocation == null || Game1.activeClickableMenu != null)
            {
                myButtonDown = false;
                return;
            }

            if (e.Button == config.DiveKey && diveLocations.Contains(Game1.currentLocation.Name))
            {
                Point pos = Game1.player.getTileLocationPoint();

                if (Game1.currentLocation.waterTiles != null && !Game1.currentLocation.waterTiles[pos.X, pos.Y] && !Game1.currentLocation.Name.StartsWith("Underwater"))
                    return;

                Game1.playSound("pullItemFromWater");

                string newName;

                if (Game1.currentLocation.Name == "UnderwaterBeach" && pos.X > 2 && pos.X < 7 && pos.Y > 26 && pos.Y < 29)
                {
                    newName = "ScubaCave";
                    pos = new Point(9,6);
                }
                else if(Game1.currentLocation.Name == "ScubaCave")
                {
                    newName = "UnderwaterBeach";
                    pos = new Point(5,28);
                }
                else
                    newName = Game1.currentLocation.Name.StartsWith("Underwater") ? Game1.currentLocation.Name.Replace("Underwater", "") : $"Underwater{Game1.currentLocation.Name}";


                if (!Game1._locationLookup.ContainsKey(newName))
                {
                    Monitor.Log($"Can't find map named {newName}", LogLevel.Warn);
                    return;
                }
                Game1.warpFarmer(newName, pos.X, pos.Y, false);
                if (!Game1.currentLocation.Name.StartsWith("Underwater"))
                {
                    bubbles.Clear();
                    isUnderwater = true;
                }
                else
                {
                    isUnderwater = false;
                }
                return; 
            }
            
            if (e.Button == config.SwimKey)
            {
                config.ReadyToSwim = !config.ReadyToSwim;
                Helper.WriteConfig<ModConfig>(config);
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
            if (Game1.currentLocation == null || Game1.player == null)
                return;

            if (Game1.player.swimming) {
                if(Game1.player.position.Y > Game1.viewport.Y + Game1.viewport.Height + 64)
                {
                    Game1.player.position.Value = new Vector2(Game1.player.position.X, Game1.viewport.Y + Game1.viewport.Height + 63);
                    if (Game1.currentLocation.Name == "Mountain")
                    {
                        Game1.warpFarmer("Town",94,0, false);
                    }
                    else if (Game1.currentLocation.Name == "Town")
                    {
                        Game1.warpFarmer("Beach", 59, 0, false);
                    }
                }
                else if(Game1.player.position.Y < Game1.viewport.Y - 64)
                {
                    Game1.player.position.Value = new Vector2(Game1.player.position.X, Game1.viewport.Y - 63);
                    if (Game1.currentLocation.Name == "Town")
                    {
                        Game1.warpFarmer("Mountain",72,40, false);
                    }
                    else if (Game1.currentLocation.Name == "Beach")
                    {
                        Game1.warpFarmer("Town", 90, 109, false);
                    }
                }
                else if(Game1.player.position.X > Game1.viewport.X + Game1.viewport.Width + 64)
                {
                    Game1.player.position.Value = new Vector2(Game1.viewport.X + Game1.viewport.Width + 63, Game1.player.position.Y);
                    if (Game1.currentLocation.Name == "Forest")
                    {
                        if(Game1.player.position.Y / 64 > 74)
                            Game1.warpFarmer("Beach", 0,13, false);
                        else
                            Game1.warpFarmer("Town", 0,100, false);
                    }
                }
                else if(Game1.player.position.X < Game1.viewport.X - 64)
                {
                    Game1.player.position.Value = new Vector2(Game1.viewport.X - 63, Game1.player.position.Y);
                    if (Game1.currentLocation.Name == "Town")
                    {
                        Game1.warpFarmer("Forest",119,43, false);
                    }
                    else if (Game1.currentLocation.Name == "Beach")
                    {
                        Game1.warpFarmer("Forest",119,111, false);
                    }
                }

                if (Game1.player.bathingClothes && IsWearingScubaGear() && !config.SwimSuitAlways)
                    Game1.player.changeOutOfSwimSuit();
                else if (!Game1.player.bathingClothes && (!IsWearingScubaGear() || config.SwimSuitAlways))
                    Game1.player.changeIntoSwimsuit();


            }
            if (Game1.activeClickableMenu == null)
            {
                if (Game1.currentLocation.Name.StartsWith("Underwater"))
                {
                    if (isUnderwater)
                    {
                        if (oxygen > 0)
                        {
                            if(!IsWearingScubaGear())
                                oxygen--;
                        }
                        else
                        {
                            Game1.playSound("pullItemFromWater");
                            isUnderwater = false;
                            Point pos = Game1.player.getTileLocationPoint();
                            Game1.warpFarmer(Game1.currentLocation.Name.Replace("Underwater", ""), pos.X, pos.Y, false);
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
                if(Game1.player.freezePause <= 0)
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
                        if(!config.SwimSuitAlways)
                            Game1.player.changeOutOfSwimSuit();
                    }
                    return;
                }
                Game1.player.position.Value = new Vector2(endJumpLoc.X - (difx * completed), endJumpLoc.Y - (dify * completed) - (float)Math.Sin(completed * Math.PI) * 64);
                return;
            }
            CheckIfMyButtonDown();

            if (!myButtonDown || Game1.player.millisecondsPlayed - lastJump < 250 || Game1.currentLocation.Name.StartsWith("Underwater"))
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
                int xTile = (Game1.viewport.X + Game1.getOldMouseX()) / 64;
                int yTile = (Game1.viewport.Y + Game1.getOldMouseY()) / 64;
                bool water = Game1.currentLocation.waterTiles[xTile, yTile];
                if (Game1.player.swimming != water)
                {
                    distance = -1;
                }
            }
            //Monitor.Log("Distance: " + distance);

            bool nextToLand = Game1.player.swimming && !Game1.currentLocation.isTilePassable(new Location((int)tiles.Last().X, (int)tiles.Last().Y), Game1.viewport) && distance < maxDistance;
            
            bool nextToWater = false;
            try
            {
                nextToWater = !Game1.player.swimming &&
                    (Game1.currentLocation.waterTiles[(int)tiles.Last().X, (int)tiles.Last().Y]
                        || (Game1.player.FacingDirection == 0 && !Game1.currentLocation.isTilePassable(new Location((int)tiles.Last().X, (int)tiles.Last().Y), Game1.viewport) && Game1.currentLocation.waterTiles[(int)tiles[tiles.Count - 2].X, (int)tiles[tiles.Count - 2].Y]))
                    && distance < maxDistance;
            }
            catch(Exception ex)
            {
                Monitor.Log($"exception trying to get next to water: {ex}");
            }

            //Monitor.Log("Distance: " + distance);


            if (Helper.Input.IsDown(config.SwimKey) || nextToLand || nextToWater)
            {
                Monitor.Log("okay to jump");
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
                Monitor.Log("got swim location");
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

        private bool IsWearingScubaGear()
        {
            bool tank = scubaTankID != -1 && Game1.player.shirtItem != null && Game1.player.shirtItem.Value != null && Game1.player.shirtItem.Value.parentSheetIndex != null &&  Game1.player.shirtItem.Value.parentSheetIndex == scubaTankID;
            bool mask = scubaMaskID != -1 && Game1.player.hat != null && Game1.player.hat.Value != null && Game1.player.hat.Value.which != null &&  Game1.player.hat.Value.which == scubaMaskID;

            return tank && mask;
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

        public void MakeOxygenBar()
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
                else if ((i % OxygenBarTexture.Width) / (float)OxygenBarTexture.Width < (float)oxygen / (float)MaxOxygen())
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

            if (config.AddFishies)
            {
                l.characters.Clear();
                int bigFishes = Game1.random.Next(10, 20);
                for (int i = 0; i < bigFishes; i++)
                {
                    int idx = Game1.random.Next(spots.Count);
                    l.characters.Add(new BigFishie(new Vector2(spots[idx].X * Game1.tileSize, spots[idx].Y * Game1.tileSize)));
                }
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
        private void AddTreasure(GameLocation l)
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
            int forageNo = Game1.random.Next(5, 20);
            List<Vector2> forageSpots = spots.Take(forageNo).ToList();
            List<Vector2> treasureSpots = new List<Vector2>();

            if (config.AddFishies)
            {
                l.characters.Clear();
                int fishes = Game1.random.Next(50, 100);
                for (int i = 0; i < fishes; i++)
                {
                    int idx = Game1.random.Next(spots.Count);
                    l.characters.Add(new Fishie(new Vector2(spots[idx].X * Game1.tileSize, spots[idx].Y * Game1.tileSize)));
                }
            }

            foreach (Vector2 v in forageSpots)
            {
                double chance = Game1.random.NextDouble();
                if(chance < 0.25)
                {
                    l.overlayObjects[v] = new StardewValley.Object(v, 152, "Seaweed", true, true, false, true)
                    {
                        CanBeGrabbed = true
                    };
                }
                else if(chance < 0.4)
                {
                    l.overlayObjects[v] = new StardewValley.Object(v, 153, "Green Algae", true, true, false, true)
                    {
                        CanBeGrabbed = true
                    };
                }
                else if(chance < 0.55)
                {
                    l.overlayObjects[v] = new StardewValley.Object(v, 157, "White Algae", true, true, false, true)
                    {
                        CanBeGrabbed = true
                    };
                }
                else if(chance < 0.65)
                {
                    l.overlayObjects[v] = new StardewValley.Object(v, 372, "Clam", true, true, false, true)
                    {
                        CanBeGrabbed = true
                    };
                }
                else if(chance < 0.75)
                {
                    l.overlayObjects[v] = new StardewValley.Object(v, 393, "Coral", true, true, false, true)
                    {
                        CanBeGrabbed = true
                    };
                }
                else if(chance < 0.85)
                {
                    l.overlayObjects[v] = new StardewValley.Object(v, 397, "Sea Urchin", true, true, false, true)
                    {
                        CanBeGrabbed = true
                    };
                }
                else if(chance < 0.9)
                {
                    l.overlayObjects[v] = new StardewValley.Object(v, 394, "Rainbow Shell", true, true, false, true)
                    {
                        CanBeGrabbed = true
                    };
                }
                else if(chance < 0.95)
                {
                    l.overlayObjects[v] = new StardewValley.Object(v, 392, "Nautilus Shell", true, true, false, true)
                    {
                        CanBeGrabbed = true
                    };
                }
                else
                {
                    treasureSpots.Add(v);
                }
            }

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


            return false;
        }

        /// <summary>Load a matched asset.</summary>
        /// <param name="asset">Basic metadata about the asset being loaded.</param>
        public T Load<T>(IAssetInfo asset)
        {
            Monitor.Log($"loading asset for {asset.AssetName}");

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
            Monitor.Log("Editing asset" + asset.AssetName);

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
                            if (tile != null && mapName != "Beach")
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
                                    tile.TileIndexProperties.Remove("Passable");
                                }
                            }
                            if (map.Data.GetLayer("AlwaysFront") != null)
                            {
                                tile = map.Data.GetLayer("AlwaysFront").PickTile(new Location(x, y) * Game1.tileSize, Game1.viewport.Size);
                                if (tile != null)
                                {
                                    if (tile.TileIndexProperties.ContainsKey("Passable"))
                                    {
                                        tile.TileIndexProperties.Remove("Passable");
                                    }
                                }
                            }
                            tile = map.Data.GetLayer("Buildings").PickTile(new Location(x, y) * Game1.tileSize, Game1.viewport.Size);
                            if (tile != null)
                            {
                                if ((mapName == "Beach" && x > 58 && x < 61 && y > 11 && y < 15) ||
                                    (mapName != "Beach"
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
                                else if (mapName == "Beach")
                                {
                                    tile.TileIndexProperties.Remove("Passable");
                                }
                            }
                        }
                    }
                }
                if (asset.AssetName.Contains("Underwater"))
                {
                    for (int x = 0; x < map.Data.Layers[0].LayerWidth; x++)
                    {
                        for (int y = 0; y < map.Data.Layers[0].LayerHeight; y++)
                        {
                            if (doesTileHaveProperty(map.Data, x, y, "Water", "Back") != null)
                            {
                                Tile tile = map.Data.GetLayer("Back").PickTile(new Location(x, y) * Game1.tileSize, Game1.viewport.Size);
                                if(tile != null)
                                    tile.TileIndexProperties.Remove("Water");
                            }
                        }
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
