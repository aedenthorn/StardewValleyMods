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

namespace Swim
{
    public class ModEntry : Mod, IAssetEditor
    {
        
        private ModConfig config;
        private List<SButton> dirButtons = new List<SButton>();
        private bool myButtonDown = false;
        private ulong lastJump = 0;
        private bool swimToggle = false;
        private bool isJumping = false;
        private Vector2 startJumpLoc;
        private Vector2 endJumpLoc;
        private bool willSwim = false;
        private bool isWearingSwimSuit = false;
        private Dictionary<string,bool> changeLocations = new Dictionary<string, bool> {
            {"Mountain", false },
            {"Town", false },
            {"Forest", false },
            {"Beach", false },
        };

        public override void Entry(IModHelper helper)
        {
            config = Helper.ReadConfig<ModConfig>();
            if (!config.EnableMod)
                return;

            swimToggle = config.SwimByDefault;

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

        }

        private void GameLoop_DayStarted(object sender, DayStartedEventArgs e)
        {
            AddTreasure(Game1._locationLookup["UnderwaterBeach"]);
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
            if (Game1.player == null)
            {
                myButtonDown = false;
                return;
            }

            if (e.Button == config.DiveKey && Game1.player.swimming)
            {
                Point pos = Game1.player.getTileLocationPoint();

                Game1.warpFarmer(Game1.currentLocation.Name == "Beach" ? "UnderwaterBeach" : "Beach", pos.X, pos.Y, false);

                return;
            }
            
            if (e.Button == config.SwimKey)
            {
                swimToggle = !swimToggle;
                return;
            }

            if (e.Button == config.SwimSuitKey)
            {
                if (Game1.player.bathingClothes)
                {
                    if(!Game1.player.swimming)
                        Game1.player.changeOutOfSwimSuit();
                    isWearingSwimSuit = false;
                }
                else
                {
                    Game1.player.changeIntoSwimsuit();
                    isWearingSwimSuit = true;
                }
                return;
            }

            CheckIfMyButtonDown();
        }
        private void CheckIfMyButtonDown()
        {
            if (!swimToggle)
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
            myButtonDown = false;
        }

        private void GameLoop_UpdateTicked(object sender, UpdateTickedEventArgs e)
        {

            if (Game1.player.swimming) {
                if(Game1.player.position.Y > Game1.viewport.Y + Game1.viewport.Height)
                {
                    Game1.player.position.Value = new Vector2(Game1.player.position.X, Game1.viewport.Y + Game1.viewport.Height - 64);
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
                    Game1.player.position.Value = new Vector2(Game1.player.position.X, Game1.viewport.Y + 64);
                    if (Game1.currentLocation.Name == "Town")
                    {
                        Game1.warpFarmer("Mountain",72,40, false);
                    }
                    else if (Game1.currentLocation.Name == "Beach")
                    {
                        Game1.warpFarmer("Town", 90, 109, false);
                    }
                }
                else if(Game1.player.position.X > Game1.viewport.X + Game1.viewport.Width)
                {
                    Game1.player.position.Value = new Vector2(Game1.viewport.X + Game1.viewport.Width - 64, Game1.player.position.Y);
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
                    Game1.player.position.Value = new Vector2(Game1.viewport.X + 64, Game1.player.position.Y);
                    if (Game1.currentLocation.Name == "Town")
                    {
                        Game1.warpFarmer("Forest",119,43, false);
                    }
                    else if (Game1.currentLocation.Name == "Beach")
                    {
                        Game1.warpFarmer("Forest",119,111, false);
                    }
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
                        if(!isWearingSwimSuit)
                            Game1.player.changeOutOfSwimSuit();
                    }
                    return;
                }
                Game1.player.position.Value = new Vector2(endJumpLoc.X - (difx * completed), endJumpLoc.Y - (dify * completed) - (float)Math.Sin(completed * Math.PI) * 64);
                return;
            }

            if (!myButtonDown || Game1.player.millisecondsPlayed - lastJump < 250 || Game1.currentLocation.Name.StartsWith("Underwater"))
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
                    maxDistance = 64;
                    break;
            }

            bool nextToLand = Game1.player.swimming && Game1.currentLocation.doesTileHaveProperty((int)tiles.Last().X, (int)tiles.Last().Y,"Water","Back") != null && !Game1.currentLocation.isTilePassable(new Location((int)tiles.Last().X, (int)tiles.Last().Y), Game1.viewport) && distance < maxDistance;
            
            bool nextToWater = false;
            try
            {
                nextToWater = !Game1.player.swimming &&
                    (Game1.currentLocation.waterTiles[(int)tiles.Last().X, (int)tiles.Last().Y]
                        || (Game1.player.FacingDirection == 0 && !Game1.currentLocation.isTilePassable(new Location((int)tiles.Last().X, (int)tiles.Last().Y), Game1.viewport) && Game1.currentLocation.waterTiles[(int)tiles[tiles.Count - 2].X, (int)tiles[tiles.Count - 2].Y]))
                    && distance < maxDistance;
            }
            catch
            {

            }

            //Monitor.Log("Distance: " + distance);


            if (Helper.Input.IsDown(config.SwimKey) || nextToLand || nextToWater)
            {
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
                    Game1.player.changeIntoSwimsuit();
                    Game1.player.freezePause = config.JumpTimeInMilliseconds;
                    Game1.currentLocation.playSound("dwop", NetAudio.SoundContext.Default);
                }
                isJumping = true;
                startJumpLoc = Game1.player.position.Value;
                endJumpLoc = new Vector2(jumpLocation.X * Game1.tileSize, jumpLocation.Y * Game1.tileSize);
            }

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

        private void AddTreasure(GameLocation l)
        {
            List<Vector2> treasureSpots = new List<Vector2>();
            for (int x = 0; x < l.map.Layers[0].LayerWidth; x++)
            {
                for (int y = 0; y < l.map.Layers[0].LayerHeight; y++)
                {
                    Tile tile = l.map.GetLayer("Back").PickTile(new Location(x, y) * Game1.tileSize, Game1.viewport.Size);
                    if (tile != null && l.map.GetLayer("Buildings").PickTile(new Location(x, y) * Game1.tileSize, Game1.viewport.Size) == null && l.map.GetLayer("Front").PickTile(new Location(x, y) * Game1.tileSize, Game1.viewport.Size) == null)
                    {
                        treasureSpots.Add(new Vector2(x, y));
                    }
                }
            }
            int n = treasureSpots.Count;
            while (n > 1)
            {
                n--;
                int k = Game1.random.Next(n + 1);
                var value = treasureSpots[k];
                treasureSpots[k] = treasureSpots[n];
                treasureSpots[n] = value;
            }
            treasureSpots = treasureSpots.Take(Game1.random.Next(1,5)).ToList();
            foreach(Vector2 v in treasureSpots)
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


        /// <summary>Get whether this instance can edit the given asset.</summary>
        /// <param name="asset">Basic metadata about the asset being loaded.</param>
        public bool CanEdit<T>(IAssetInfo asset)
        {
            if (!config.EnableMod)
                return false;

            if (asset.AssetNameEquals("Maps/Beach") || asset.AssetNameEquals("Maps/Town") || asset.AssetNameEquals("Maps/Forest") || asset.AssetNameEquals("Maps/Mountain"))
            {
                return true;
            }
            return false;
        }

        /// <summary>Edit a matched asset.</summary>
        /// <param name="asset">A helper which encapsulates metadata about an asset and enables changes to it.</param>
        public void Edit<T>(IAssetData asset)
        {
            Monitor.Log("Editing asset" + asset.AssetName);
            if (asset.AssetNameEquals("Maps/Beach") || asset.AssetNameEquals("Maps/Town") || asset.AssetNameEquals("Maps/Forest") || asset.AssetNameEquals("Maps/Mountain"))
            {
                IAssetDataForMap map = asset.AsMap();
                for (int x = 0; x < map.Data.Layers[0].LayerWidth; x++)
                {
                    for (int y = 0; y < map.Data.Layers[0].LayerHeight; y++)
                    {
                        if (doesTileHaveProperty(map.Data, x, y, "Water", "Back") != null)
                        {
                            Tile tile = map.Data.GetLayer("Back").PickTile(new Location(x, y) * Game1.tileSize, Game1.viewport.Size);
                            if(tile != null)
                            {
                                if (tile.TileIndexProperties.ContainsKey("Passable"))
                                {
                                    tile.TileIndexProperties.Remove("Passable");
                                }
                            }
                            tile = map.Data.GetLayer("Front").PickTile(new Location(x, y) * Game1.tileSize, Game1.viewport.Size);
                            if(tile != null)
                            {
                                if (tile.TileIndexProperties.ContainsKey("Passable"))
                                {
                                    tile.TileIndexProperties.Remove("Passable");
                                }
                            }
                            if(map.Data.GetLayer("AlwaysFront") != null)
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
                            if (asset.AssetNameEquals("Maps/Mountain"))
                            {
                                tile = map.Data.GetLayer("Buildings").PickTile(new Location(x, y) * Game1.tileSize, Game1.viewport.Size);
                                if (tile != null)
                                {
                                    if ((tile.TileIndex > 1292 && tile.TileIndex < 1297) || (tile.TileIndex > 1317 && tile.TileIndex < 1322)
                                        || (tile.TileIndex % 25 > 17 && tile.TileIndex / 25 < 53 && tile.TileIndex / 25 > 48)
                                        || (tile.TileIndex % 25 > 1 && tile.TileIndex % 25 < 7 && tile.TileIndex / 25 < 53 && tile.TileIndex / 25 > 48)
                                        || (tile.TileIndex % 25 > 11 && tile.TileIndex / 25 < 51 && tile.TileIndex / 25 > 48)
                                        || (tile.TileIndex % 25 > 10 && tile.TileIndex % 25 < 14 && tile.TileIndex / 25 < 49 && tile.TileIndex / 25 > 46)
                                        || tile.TileIndex == 734 || tile.TileIndex == 759
                                        || tile.TileIndex == 628 || tile.TileIndex == 629
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
