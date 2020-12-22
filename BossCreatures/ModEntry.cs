using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Monsters;
using StardewValley.Network;
using System;
using System.Collections.Generic;
using System.Threading;
using xTile.Dimensions;
using xTile.Layers;
using xTile.Tiles;
using Object = StardewValley.Object;

namespace BossCreatures
{
    public class ModEntry : Mod
    {
        public static ModConfig Config;

        public static IMonitor PMonitor;
        public static IModHelper PHelper;

        public static Texture2D[] darknessTextures = new Texture2D[9];
        private static int toggleSprite = 0;
        private static int darknessTimer = 600;
        private static bool isDarkness = false;

        private static List<string> CheckedBosses = new List<string>();
        private static bool isFightingBoss;
        private static string defaultMusic = "none";

        private static Texture2D healthBarTexture;
        private static Dictionary<Type,string> BossTypes = new Dictionary<Type, string>() {
            { typeof(BugBoss), "Armored Bug"},
            { typeof(GhostBoss), "Ghost"},
            { typeof(SerpentBoss), "Serpent"},
            { typeof(SkeletonBoss), "Skeleton"},
            { typeof(SkullBoss), "Haunted Skull"},
            { typeof(SquidKidBoss), "Squid Kid"},
            { typeof(SlimeBoss), "Big Slime"},
        };
        private static LootList BossLootList;
        private static int lastBossHealth;

        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();
            PMonitor = Monitor;
            PHelper = helper;

            helper.Events.Player.Warped += Warped;
            helper.Events.GameLoop.DayStarted += OnDayStarted;
            helper.Events.GameLoop.DayEnding += OnDayEnding;

            MakeDarkness();
            BossLootList = Helper.Data.ReadJsonFile<LootList>("assets/boss_loot.json") ?? new LootList();
            if(BossLootList.loot.Count == 0)
            {
                Monitor.Log("No boss loot!", LogLevel.Warn);
            }

        }

        private void OnDayEnding(object sender, DayEndingEventArgs e)
        {
            foreach (GameLocation location in Game1.locations)
            {
                for (int i = 0; i < location.characters.Count; i++)
                {
                    if (BossTypes.ContainsKey(location.characters[i].GetType()) || location.characters[i] is ToughFly || location.characters[i] is ToughGhost)
                    {
                        location.characters.RemoveAt(i);
                    }

                }
            }        
            foreach (GameLocation location in Game1._locationLookup.Values)
            {
                for (int i = 0; i < location.characters.Count; i++)
                {
                    if (BossTypes.ContainsKey(location.characters[i].GetType()) || location.characters[i] is ToughFly || location.characters[i] is ToughGhost)
                    {
                        location.characters.RemoveAt(i);
                    }

                }
            }
        }
        private void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            CheckedBosses.Clear();
        }

        private void Warped(object sender, WarpedEventArgs e)
        {

            PMonitor.Log("Entered location: " + e.NewLocation.Name);
            //defaultMusic = Game1.getMusicTrackName();

            foreach (GameLocation location in Game1.locations)
            {
                for (int i = 0; i < location.characters.Count; i++)
                {
                    if (location.characters[i] is ToughFly || location.characters[i] is ToughGhost)
                    {
                        location.characters.RemoveAt(i);
                    }

                }
            }


            MakeBossHealthBar(100, 100);

            if (isFightingBoss && BossHere(e.NewLocation) == null)
            {
                RevertMusic(e.NewLocation);
            }

            if (!Game1.eventUp)
                TryAddBoss(e.NewLocation);

        }

        public static string GetBossTexture(Type type)
        {
            string texturePath = $"Characters\\Monsters\\{BossTypes[type]}";
            if (Config.UseAlternateTextures)
            {
                try
                {
                    Texture2D spriteTexture = ModEntry.PHelper.Content.Load<Texture2D>($"Characters/Monsters/{type.Name}", ContentSource.GameContent);
                    if(spriteTexture != null)
                    {
                        texturePath = $"Characters\\Monsters\\{type.Name}";
                    }
                }
                catch
                {
                    PMonitor.Log($"texture not found: Characters\\Monsters\\{type.Name}", LogLevel.Debug);
                }
            }
            return texturePath;
        }

        public static void BossDeath(GameLocation currentLocation, Monster monster, float difficulty)
        {
            PHelper.Events.Display.RenderedHud -= ModEntry.OnRenderedHud;

            Microsoft.Xna.Framework.Rectangle monsterBox = monster.GetBoundingBox();

            SpawnBossLoot(currentLocation, monsterBox.Center.X, monsterBox.Center.Y, difficulty);

            Game1.playSound("Cowboy_Secret");
            RevertMusic(currentLocation);
        }

        public static void OnRenderingHud(object sender, RenderingHudEventArgs e)
        {
            Monster boss = BossHere(Game1.player.currentLocation);
            if (boss == null)
            {
                PHelper.Events.Display.RenderingHud -= OnRenderingHud;
                return;
            }

            // Darkness

            darknessTimer -= 1;

            if (darknessTimer < 300)
            {
                if (isDarkness)
                {
                    if(darknessTimer < darknessTextures.Length)
                    {
                        e.SpriteBatch.Draw(darknessTextures[Math.Max(0, darknessTimer)], new Vector2(0, 0), Color.Black);
                    }
                    else
                    {
                        e.SpriteBatch.Draw(darknessTextures[Math.Min(8, 300 - darknessTimer)], new Vector2(0, 0), Color.Black);
                    }

                }
                else
                {
                    isDarkness = Game1.random.NextDouble() < 0.5;
                    if (isDarkness)
                    {
                        boss.currentLocation.localSound("Duggy");
                    }
                }
            }
            if (darknessTimer <= 0)
            {
                isDarkness = false;
                darknessTimer = 600;
                boss.currentLocation.localSound("Duggy");
            }
        }
        public static void OnRenderedHud(object sender, RenderedHudEventArgs e)
        {
            Monster boss = BossHere(Game1.player.currentLocation);
            if (boss == null)
            {
                PHelper.Events.Display.RenderedHud -= OnRenderedHud;
                return;
            }

            if(boss.Health != lastBossHealth)
            {
                lastBossHealth = boss.Health;
                MakeBossHealthBar(boss.Health, boss.MaxHealth);
            }

            e.SpriteBatch.Draw(healthBarTexture, new Vector2((int)Math.Round(Game1.viewport.Width * 0.13f), 100), Color.White);

            Vector2 bossPos = boss.Position;
            if (!Utility.isOnScreen(bossPos, 0))
            {
                int x = (int)Math.Max(10, Math.Min(Game1.viewport.X + Game1.viewport.Width - 90, bossPos.X) - Game1.viewport.X);
                int y = (int)Math.Max(10, Math.Min(Game1.viewport.Y + Game1.viewport.Height - 90, bossPos.Y) - Game1.viewport.Y);

                if (toggleSprite < 20)
                {
                    Texture2D texture = PHelper.Content.Load<Texture2D>("Characters/Monsters/Haunted Skull", ContentSource.GameContent);
                    ClickableTextureComponent bossIcon = new ClickableTextureComponent(new Microsoft.Xna.Framework.Rectangle(x, y, 80, 80), texture, new Microsoft.Xna.Framework.Rectangle(toggleSprite > 10 ? 16 : 0, 32, 16, 16), 5f, false);
                    bossIcon.draw(Game1.spriteBatch);
                }
                toggleSprite++;
                toggleSprite %= 30;
            }
        }

        public static void MakeBossHealthBar(int Health, int MaxHealth)
        {
            healthBarTexture = new Texture2D(Game1.graphics.GraphicsDevice, (int)Math.Round(Game1.viewport.Width * 0.74f), 30);
            Color[] data = new Color[healthBarTexture.Width * healthBarTexture.Height];
            healthBarTexture.GetData(data);
            for (int i = 0; i < data.Length; i++)
            {
                if (i <= healthBarTexture.Width || i % healthBarTexture.Width == healthBarTexture.Width - 1)
                {
                    data[i] = new Color(1f, 0.5f, 0.5f);
                }
                else if (data.Length - i < healthBarTexture.Width || i % healthBarTexture.Width == 0)
                {
                    data[i] = new Color(0.5f, 0, 0);
                }
                else if ((i % healthBarTexture.Width) / (float)healthBarTexture.Width < (float)Health / (float)MaxHealth)
                {
                    data[i] = Color.Red;
                }
                else
                {
                    data[i] = Color.Black;
                }
            }
            healthBarTexture.SetData<Color>(data);
        }

        private void MakeDarkness()
        {
            for (int j = 0; j < 9; j++)
            {
                darknessTextures[j] = new Texture2D(Game1.graphics.GraphicsDevice, Game1.viewport.Width, Game1.viewport.Height);
                Color[] color = new Color[Game1.viewport.Width * Game1.viewport.Height];
                for (int i = 0; i < Game1.viewport.Width * Game1.viewport.Height; i++)
                {
                    color[i] = new Color(0, 0, 0, (j + 1) / 10f);
                }
                darknessTextures[j].SetData<Color>(color);
            }
        }

        internal static void RevertMusic(GameLocation location)
        {
            Game1.changeMusicTrack(defaultMusic, true, Game1.MusicContext.Default);
            location.checkForMusic(new GameTime());
            isFightingBoss = false;
        }

        public static Monster BossHere(GameLocation location)
        {
            using (List<NPC>.Enumerator enumerator = location.characters.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    NPC j = enumerator.Current;
                    if (BossTypes.ContainsKey(j.GetType()))
                    {
                        return (Monster)j;
                    }
                }
            }
            return null;
        }

        private void TryAddBoss(GameLocation location)
        {
            Monster boss = BossHere(location);
            if (boss != null && boss.Health > 0)
            {
                Game1.changeMusicTrack("cowboy_boss", false, Game1.MusicContext.Default);
                PHelper.Events.Display.RenderedHud += OnRenderedHud;
                return;
            }
            if (CheckedBosses.Contains(location.Name))
            {
                return;
            }

            CheckedBosses.Add(location.Name);

            if ((location is MineShaft) && (location as MineShaft).mustKillAllMonstersToAdvance() && Game1.random.Next(0, 100) < Config.PercentChanceOfBossInMonsterArea)
            {
                SpawnRandomBoss(location);
            }
            else if ((location is Farm) && Game1.random.Next(0, 100) < Config.PercentChanceOfBossInFarm)
            {
                SpawnRandomBoss(location);
            }
            else if ((location is Town) && Game1.random.Next(0, 100) < Config.PercentChanceOfBossInTown)
            {
                SpawnRandomBoss(location);
            }
            else if ((location is Forest) && Game1.random.Next(0, 100) < Config.PercentChanceOfBossInForest)
            {
                SpawnRandomBoss(location);
            }
            else if ((location is Mountain) && Game1.random.Next(0, 100) < Config.PercentChanceOfBossInMountain)
            {
                SpawnRandomBoss(location);
            }
            else if ((location is Desert) && Game1.random.Next(0, 100) < Config.PercentChanceOfBossInDesert)
            {
                SpawnRandomBoss(location);
            }
            else if ((location.Name == "CrimsonBadlands") && Game1.random.Next(0, 100) < Config.PercentChanceOfBossInCrimsonBadlands)
            {
                SpawnRandomBoss(location);
            }
        }

        private void SpawnRandomBoss(GameLocation location)
        {

            Vector2 spawnPos = GetLandSpawnPos(location);
            if (spawnPos == Vector2.Zero)
            {
                PMonitor.Log("no spawn location for boss!", LogLevel.Debug);
                return;
            }

            float difficulty = Config.BaseUndergroundDifficulty;
            if (location is MineShaft)
            {
                difficulty *= (location as MineShaft).mineLevel / 100f;
                Monitor.Log("boss difficulty: " + difficulty, LogLevel.Debug);
            }
            else
            {
                difficulty = Game1.random.Next((int)Math.Round(Config.MinOverlandDifficulty * 1000), (int)Math.Round(Config.MaxOverlandDifficulty * 1000)+1) / 1000f;
                Monitor.Log("boss difficulty: " + difficulty, LogLevel.Debug);
            }

            int r = Game1.random.Next(0, (int)Math.Round(Config.WeightSkullBossChance * 100 + Config.WeightSerpentBossChance * 100 + Config.WeightBugBossChance * 100 + Config.WeightGhostBossChance * 100 + Config.WeightSkeletonBossChance * 100 + Config.WeightSquidBossChance * 100 + Config.WeightSlimeBossChance * 100));
            if(r < Config.WeightSkullBossChance * 100)
            {
                SkullBoss k = new SkullBoss(spawnPos, difficulty)
                {
                    currentLocation = location,
                };
                location.characters.Add(k);
            }
            else if (r < Config.WeightSkullBossChance * 100 + Config.WeightSerpentBossChance * 100)
            {
                SerpentBoss s = new SerpentBoss(spawnPos, difficulty)
                {
                    currentLocation = location,
                };
                location.characters.Add(s);
            }
            else if (r < Config.WeightSkullBossChance * 100 + Config.WeightSerpentBossChance * 100 + Config.WeightBugBossChance * 100)
            {
                BugBoss b = new BugBoss(spawnPos, difficulty)
                {
                    currentLocation = location,
                };
                location.characters.Add(b);
            }
            else if (r < Config.WeightSkullBossChance * 100 + Config.WeightSerpentBossChance * 100 + Config.WeightBugBossChance * 100 + Config.WeightGhostBossChance * 100)
            {
                GhostBoss g = new GhostBoss(spawnPos, difficulty)
                {
                    currentLocation = location,
                };
                location.characters.Add(g);
            }
            else if (r < Config.WeightSkullBossChance * 100 + Config.WeightSerpentBossChance * 100 + Config.WeightBugBossChance * 100 + Config.WeightGhostBossChance * 100 + Config.WeightSkeletonBossChance * 100)
            {

                SkeletonBoss sk = new SkeletonBoss(spawnPos, difficulty)
                {
                    currentLocation = location,
                };
                location.characters.Add(sk);
            }
            else if (r < Config.WeightSkullBossChance * 100 + Config.WeightSerpentBossChance * 100 + Config.WeightBugBossChance * 100 + Config.WeightGhostBossChance * 100 + Config.WeightSkeletonBossChance * 100 + Config.WeightSquidBossChance * 100)
            {
                SquidKidBoss sq = new SquidKidBoss(spawnPos, difficulty)
                {
                    currentLocation = location,
                };
                location.characters.Add(sq);
            }
            else
            {
                SlimeBoss sl = new SlimeBoss(spawnPos, difficulty)
                {
                    currentLocation = location,
                };
                location.characters.Add(sl);
            }

            Game1.showGlobalMessage(PHelper.Translation.Get("boss-warning"));
            Game1.changeMusicTrack("cowboy_boss", false, Game1.MusicContext.Default);
            PHelper.Events.Display.RenderedHud += OnRenderedHud;
        }

        private Vector2 GetLandSpawnPos(GameLocation location)
        {
            List<Vector2> tiles = new List<Vector2>();
            if (location is MineShaft)
            {
                for (int x2 = 0; x2 < location.map.Layers[0].LayerWidth; x2++)
                {
                    for (int y2 = 0; y2 < location.map.Layers[0].LayerHeight; y2++)
                    {
                        Tile t = location.map.Layers[0].Tiles[x2, y2];
                        if (t != null)
                        {
                            Vector2 tile2 = new Vector2((float)x2, (float)y2);
                            int m = t.TileIndex;
                            if ((location as MineShaft).isTileClearForMineObjects(tile2))
                            {
                                tiles.Add(tile2);
                            }
                        }
                    }
                }
            }
            else
            {
                for (int x2 = (int)Math.Round(location.map.Layers[0].LayerWidth *0.1f); x2 < (int)Math.Round(location.map.Layers[0].LayerWidth * 0.9f); x2++)
                {
                    for (int y2 = (int)Math.Round(location.map.Layers[0].LayerHeight * 0.1f); y2 < (int)Math.Round(location.map.Layers[0].LayerHeight * 0.9f); y2++)
                    {
                        Layer l = location.map.GetLayer("Paths");
                        if (l != null)
                        {
                            Tile t = l.Tiles[x2, y2];
                            if (t != null)
                            {
                                Vector2 tile2 = new Vector2((float)x2, (float)y2);
                                if (location.isTileLocationTotallyClearAndPlaceable(tile2))
                                {
                                    tiles.Add(tile2);
                                }
                            }
                        }

                        if(tiles.Count == 0)
                        {
                            Tile t = location.map.Layers[0].Tiles[x2, y2];
                            if (t != null)
                            {
                                Vector2 tile2 = new Vector2((float)x2, (float)y2);
                                if (location.isTilePassable(new Location((int)tile2.X, (int)tile2.Y),Game1.viewport))
                                {
                                    tiles.Add(tile2);
                                }
                            }
                        }
                    }
                }
            }
            if(tiles.Count == 0)
            {
                return Vector2.Zero;
            }
            Vector2 posT = tiles[Game1.random.Next(0,tiles.Count)];
            return new Vector2(posT.X * 64f, posT.Y * 64f);
        }

        public static void SpawnBossLoot(GameLocation location, float x, float y, float difficulty)
        {
            Vector2 playerPosition = new Vector2((float)Game1.player.GetBoundingBox().Center.X, (float)Game1.player.GetBoundingBox().Center.Y);

            foreach (string loot in BossLootList.loot)
            {
                string[] loota = loot.Split('/');
                if (!int.TryParse(loota[0], out int objectToAdd))
                {
                    PMonitor.Log($"loot object {loota[0]} is invalid", LogLevel.Error);
                    continue;
                }
                Object o = new Object(objectToAdd, 1);
                if (objectToAdd >= 0)
                {
                    if (o.Name == Game1.content.LoadString("Strings\\StringsFromCSFiles:CraftingRecipe.cs.575"))
                    {
                        PMonitor.Log($"object {objectToAdd} is error item");
                        continue;
                    }
                }

                if (!double.TryParse(loota[1], out double chance))
                {
                    PMonitor.Log($"loot chance {loota[1]} is invalid", LogLevel.Error);
                    continue;
                }

                while (chance > 1 || (chance > 0 && Game1.random.NextDouble() < chance))
                {
                    if (objectToAdd < 0)
                    {
                        Game1.createDebris(Math.Abs(objectToAdd), (int)x, (int)y, (int)Math.Round(Game1.random.Next(10, 40) * difficulty), location);
                    }
                    else
                    {
                        location.debris.Add(Game1.createItemDebris(o, new Vector2(x, y), Game1.random.Next(4)));
                    }

                    chance -= 1;
                }
            }
        }

        public static Vector2 RotateVector2d(Vector2 inV, float degrees)
        {
            float rads = (float)Math.PI / 180 * degrees;
            Vector2 result = new Vector2();
            result.X = (float)(inV.X * Math.Cos(rads) - inV.Y * Math.Sin(rads));
            result.Y = (float)(inV.X * Math.Sin(rads) + inV.Y * Math.Cos(rads));
            return result;
        }

        public static Vector2 RotateVector(Vector2 v, float degrees)
        {
            double radians = Math.PI / 180 * degrees;
            double sin = Math.Sin(radians);
            double cos = Math.Cos(radians);

            float tx = v.X;
            float ty = v.Y;

            return new Vector2((float)cos * tx - (float)sin * ty, (float)sin * tx + (float)cos * ty);
        }

        public static Vector2 VectorFromDegree(int degrees)
        {
            double radians = Math.PI / 180 * degrees;
            return new Vector2((float)Math.Cos(radians), (float)Math.Sin(radians));
        }
        public static bool IsLessThanHalfHealth(Monster m)
        {
            return m.Health < m.MaxHealth / 2;
        }
    }
}
