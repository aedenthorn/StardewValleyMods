using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Monsters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xTile.Layers;
using xTile.Tiles;

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



        public override void Entry(IModHelper helper)
        {
            Config = this.Helper.ReadConfig<ModConfig>();
            PMonitor = Monitor;
            PHelper = helper;

            helper.Events.Player.Warped += Warped;
            helper.Events.GameLoop.DayStarted += OnDayStarted;
        }

        private void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            CheckedBosses.Clear();

        }

        private void Warped(object sender, WarpedEventArgs e)
        {
            MakeBossHealthBar(100, 100);

            if (isFightingBoss && BossHere(e.NewLocation) == null)
            {
                RevertMusic();
            }
            TryAddBoss(e.NewLocation);

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

            e.SpriteBatch.Draw(healthBarTexture, new Vector2((int)Math.Round(Game1.viewport.Width * 0.13f), 100), Color.White);

            Vector2 bossPos = boss.Position;
            if (!Utility.isOnScreen(bossPos, 0))
            {
                int x = (int)Math.Max(10, Math.Min(Game1.viewport.X + Game1.viewport.Width - 58, bossPos.X) - Game1.viewport.X);
                int y = (int)Math.Max(10, Math.Min(Game1.viewport.Y + Game1.viewport.Height - 58, bossPos.Y) - Game1.viewport.Y);

                if (toggleSprite < 20)
                {
                    Texture2D texture = PHelper.Content.Load<Texture2D>("Characters/Monsters/Haunted Skull.xnb", ContentSource.GameContent);
                    ClickableTextureComponent bossIcon = new ClickableTextureComponent(new Rectangle(x, y, 48, 48), texture, new Rectangle(toggleSprite > 10 ? 16 : 0, 32, 16, 16), 3f, false);
                    bossIcon.draw(Game1.spriteBatch);
                }
                toggleSprite++;
                toggleSprite = toggleSprite % 30;
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

        internal static void RevertMusic()
        {
            Game1.changeMusicTrack(defaultMusic, false);
            isFightingBoss = false;
        }

        public static Monster BossHere(GameLocation location)
        {
            using (NetCollection<NPC>.Enumerator enumerator = location.characters.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    NPC j = enumerator.Current;
                    if (j is SerpentBoss || j is SkullBoss || j is BugBoss || j is GhostBoss || j is SkeletonBoss)
                    {
                        return (Monster)j;
                    }
                }
            }
            return null;
        }

        private static List<string> CheckedBosses = new List<string>();
        private static bool isFightingBoss;
        private static string defaultMusic;
        private Dictionary<Type, Vector2> SkeletonSpawnPos = new Dictionary<Type, Vector2>() 
        {
            {typeof(Farm), new Vector2(4832.09f,1121.761f) },
            {typeof(Town), new Vector2(1835.856f,4309.934f) },
            {typeof(Mountain), new Vector2(2777.356f,2018.429f) },
            {typeof(Forest), new Vector2(4478.214f,2683.687f) },
            {typeof(Desert), new Vector2(859.2023f,1734.465f) },
        };
        private static Texture2D healthBarTexture;

        private void TryAddBoss(GameLocation location)
        {
            if (BossHere(location) != null)
            {
                Game1.changeMusicTrack("cowboy_boss", false, Game1.MusicContext.Default);

                return;
            }
            if (CheckedBosses.Contains(location.name))
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
        }

        private void SpawnRandomBoss(GameLocation location)
        {
            float x = Game1.random.Next(location.map.DisplayWidth / 4, location.map.DisplayWidth * 3 / 4);
            float y = Game1.random.Next(location.map.DisplayHeight / 4, location.map.DisplayHeight * 3 / 4);
            Vector2 spawnPos = new Vector2(x, y);
            SpawnRandomBoss(location, spawnPos);
        }
        private void SpawnRandomBoss(GameLocation location, Vector2 spawnPos)
        {
            float difficulty = Config.BaseUndergroundDifficulty;
            if (location.Name.StartsWith("UndergroundMine"))
            {
                int diffMult = 0;
                if(int.TryParse(location.Name.Substring(15), out diffMult))
                {
                    difficulty *= diffMult / 100;
                    //Monitor.Log("boss difficulty: " + difficulty, LogLevel.Debug);
                }
            }
            else
            {
                difficulty = Game1.random.Next((int)(Config.MinOverlandDifficulty * 100), (int)(Config.MaxOverlandDifficulty * 100)+1) / 100f;
                //Monitor.Log("boss difficulty: " + difficulty, LogLevel.Debug);
            }
            int r = Game1.random.Next(0,5);
            switch (r)
            {
                case 0:
                    SerpentBoss s = new SerpentBoss(spawnPos, difficulty)
                    {
                        currentLocation = location,
                    };
                    location.characters.Add(s);
                    break;
                case 1:
                    SkullBoss k = new SkullBoss(spawnPos, difficulty)
                    {
                        currentLocation = location,
                    };
                    location.characters.Add(k);
                    break;
                case 2:
                    BugBoss b = new BugBoss(spawnPos, difficulty)
                    {
                        currentLocation = location,
                    };
                    location.characters.Add(b);
                    break;
                case 3:
                    GhostBoss g = new GhostBoss(spawnPos, difficulty)
                    {
                        currentLocation = location,
                    };
                    location.characters.Add(g);
                    break;
                case 4:
                    spawnPos = GetLandSpawnPos(location);
                    if (spawnPos == Vector2.Zero)
                    {
                        PMonitor.Log("no spawn location for boss!",LogLevel.Debug);

                        break;
                    }
                    SkeletonBoss sk = new SkeletonBoss(spawnPos, difficulty)
                    {
                        currentLocation = location,
                    };
                    location.characters.Add(sk);
                    break;
            }

            Game1.showGlobalMessage(PHelper.Translation.Get("boss-warning"));
            Game1.changeMusicTrack("cowboy_boss", false, Game1.MusicContext.Default);
            defaultMusic = Game1.getMusicTrackName();
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
                for (int x2 = 0; x2 < location.map.Layers[0].LayerWidth; x2++)
                {
                    for (int y2 = 0; y2 < location.map.Layers[0].LayerHeight; y2++)
                    {
                        Layer l = location.map.GetLayer("Paths");
                        if (l != null)
                        {
                            Tile t = l.Tiles[x2, y2];
                            if (t != null)
                            {
                                Vector2 tile2 = new Vector2((float)x2, (float)y2);
                                int m = t.TileIndex;
                                if (location.isTileLocationTotallyClearAndPlaceable(tile2))
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
            List<KeyValuePair<int,double>> loots = new List<KeyValuePair<int,double>>()
            {
                new KeyValuePair<int,double>(766,.75), new KeyValuePair<int,double>(766,.05), new KeyValuePair<int,double>(153,.1), new KeyValuePair<int,double>(66,.015), new KeyValuePair<int,double>(92,.15), new KeyValuePair<int,double>(96,.005), new KeyValuePair<int,double>(99,.001), new KeyValuePair<int,double>(382,.5), new KeyValuePair<int,double>(433,.01), new KeyValuePair<int,double>(336,.001), new KeyValuePair<int,double>(84,.02), new KeyValuePair<int,double>(414,.02), new KeyValuePair<int,double>(97,.005), new KeyValuePair<int,double>(99,.001), new KeyValuePair<int,double>(767,.9), new KeyValuePair<int,double>(767,.4), new KeyValuePair<int,double>(108,.001), new KeyValuePair<int,double>(287,.02), new KeyValuePair<int,double>(96,.005), new KeyValuePair<int,double>(99,.001), new KeyValuePair<int,double>(767,.9), new KeyValuePair<int,double>(767,.55), new KeyValuePair<int,double>(108,.001), new KeyValuePair<int,double>(287,.02), new KeyValuePair<int,double>(97,.005), new KeyValuePair<int,double>(99,.001), new KeyValuePair<int,double>(767,.9), new KeyValuePair<int,double>(767,.7), new KeyValuePair<int,double>(108,.001), new KeyValuePair<int,double>(287,.02), new KeyValuePair<int,double>(98,.005), new KeyValuePair<int,double>(99,.001), new KeyValuePair<int,double>(386,.9), new KeyValuePair<int,double>(386,.5), new KeyValuePair<int,double>(386,.25), new KeyValuePair<int,double>(386,.1), new KeyValuePair<int,double>(288,.05), new KeyValuePair<int,double>(768,.5), new KeyValuePair<int,double>(773,.05), new KeyValuePair<int,double>(349,.05), new KeyValuePair<int,double>(787,.05), new KeyValuePair<int,double>(337,.008), new KeyValuePair<int,double>(390,.9), new KeyValuePair<int,double>(80,.1), new KeyValuePair<int,double>(382,.1), new KeyValuePair<int,double>(380,.1), new KeyValuePair<int,double>(96,.005), new KeyValuePair<int,double>(99,.001), new KeyValuePair<int,double>(771,.9), new KeyValuePair<int,double>(771,.5), new KeyValuePair<int,double>(770,.5), new KeyValuePair<int,double>(382,.1), new KeyValuePair<int,double>(86,.005), new KeyValuePair<int,double>(72,.001), new KeyValuePair<int,double>(684,.6), new KeyValuePair<int,double>(273,.05), new KeyValuePair<int,double>(273,.05), new KeyValuePair<int,double>(157,.02), new KeyValuePair<int,double>(114,.005), new KeyValuePair<int,double>(96,.005), new KeyValuePair<int,double>(99,.001), new KeyValuePair<int,double>(684,.9), new KeyValuePair<int,double>(157,.02), new KeyValuePair<int,double>(114,.005), new KeyValuePair<int,double>(96,.005), new KeyValuePair<int,double>(99,.001), new KeyValuePair<int,double>(766,.75), new KeyValuePair<int,double>(412,.08), new KeyValuePair<int,double>(70,.02), new KeyValuePair<int,double>(98,.015), new KeyValuePair<int,double>(92,.5), new KeyValuePair<int,double>(97,.005), new KeyValuePair<int,double>(99,.001), new KeyValuePair<int,double>(766,.8), new KeyValuePair<int,double>(157,.1), new KeyValuePair<int,double>(-4,.1), new KeyValuePair<int,double>(72,.01), new KeyValuePair<int,double>(92,.5), new KeyValuePair<int,double>(98,.005), new KeyValuePair<int,double>(99,.001), new KeyValuePair<int,double>(769,.75), new KeyValuePair<int,double>(769,.1), new KeyValuePair<int,double>(329,.02), new KeyValuePair<int,double>(337,.002), new KeyValuePair<int,double>(336,.01), new KeyValuePair<int,double>(335,.02), new KeyValuePair<int,double>(334,.04), new KeyValuePair<int,double>(203,.04), new KeyValuePair<int,double>(293,.03), new KeyValuePair<int,double>(108,.003), new KeyValuePair<int,double>(-4,.1), new KeyValuePair<int,double>(98,.005), new KeyValuePair<int,double>(99,.001), new KeyValuePair<int,double>(768,.95), new KeyValuePair<int,double>(768,.1), new KeyValuePair<int,double>(156,.08), new KeyValuePair<int,double>(338,.08), new KeyValuePair<int,double>(-6,.2), new KeyValuePair<int,double>(97,.005), new KeyValuePair<int,double>(99,.001), new KeyValuePair<int,double>(749,.99), new KeyValuePair<int,double>(338,.1), new KeyValuePair<int,double>(286,.25), new KeyValuePair<int,double>(535,.25), new KeyValuePair<int,double>(280,.03), new KeyValuePair<int,double>(105,.02), new KeyValuePair<int,double>(86,.1), new KeyValuePair<int,double>(72,.01), new KeyValuePair<int,double>(96,.005), new KeyValuePair<int,double>(99,.001), new KeyValuePair<int,double>(717,.15), new KeyValuePair<int,double>(286,.4), new KeyValuePair<int,double>(96,.005), new KeyValuePair<int,double>(99,.001), new KeyValuePair<int,double>(717,.25), new KeyValuePair<int,double>(287,.4), new KeyValuePair<int,double>(98,.005), new KeyValuePair<int,double>(99,.001), new KeyValuePair<int,double>(732,.5), new KeyValuePair<int,double>(386,.5), new KeyValuePair<int,double>(386,.5), new KeyValuePair<int,double>(386,.5), new KeyValuePair<int,double>(72,.0000001), new KeyValuePair<int,double>(768,.75), new KeyValuePair<int,double>(814,.2), new KeyValuePair<int,double>(336,.05), new KeyValuePair<int,double>(287,.1), new KeyValuePair<int,double>(288,.05), new KeyValuePair<int,double>(98,.005), new KeyValuePair<int,double>(99,.001), new KeyValuePair<int,double>(769,.25), new KeyValuePair<int,double>(105,.03), new KeyValuePair<int,double>(106,.03), new KeyValuePair<int,double>(166,.001), new KeyValuePair<int,double>(60,.04), new KeyValuePair<int,double>(232,.04), new KeyValuePair<int,double>(72,.03), new KeyValuePair<int,double>(74,.01), new KeyValuePair<int,double>(97,.005), new KeyValuePair<int,double>(99,.001), new KeyValuePair<int,double>(-4,.9), new KeyValuePair<int,double>(-4,.9), new KeyValuePair<int,double>(-6,.001), new KeyValuePair<int,double>(769,.75), new KeyValuePair<int,double>(769,.1), new KeyValuePair<int,double>(337,.002), new KeyValuePair<int,double>(336,.01), new KeyValuePair<int,double>(335,.02), new KeyValuePair<int,double>(334,.04), new KeyValuePair<int,double>(203,.04), new KeyValuePair<int,double>(108,.003), new KeyValuePair<int,double>(-4,.1), new KeyValuePair<int,double>(98,.005), new KeyValuePair<int,double>(99,.001), new KeyValuePair<int,double>(74,.0005), new KeyValuePair<int,double>(769,.75), new KeyValuePair<int,double>(769,.2), new KeyValuePair<int,double>(337,.002), new KeyValuePair<int,double>(336,.01), new KeyValuePair<int,double>(335,.02), new KeyValuePair<int,double>(334,.04), new KeyValuePair<int,double>(108,.003), new KeyValuePair<int,double>(-4,.1), new KeyValuePair<int,double>(98,.005), new KeyValuePair<int,double>(99,.001), new KeyValuePair<int,double>(74,.0005), new KeyValuePair<int,double>(80,0), new KeyValuePair<int,double>(80,0), new KeyValuePair<int,double>(768,.65), new KeyValuePair<int,double>(378,.1), new KeyValuePair<int,double>(378,.1), new KeyValuePair<int,double>(380,.1), new KeyValuePair<int,double>(380,.1), new KeyValuePair<int,double>(382,.1), new KeyValuePair<int,double>(98,.005), new KeyValuePair<int,double>(99,.001), new KeyValuePair<int,double>(378,.1), new KeyValuePair<int,double>(378,.1), new KeyValuePair<int,double>(380,.1), new KeyValuePair<int,double>(380,.1), new KeyValuePair<int,double>(382,.1), new KeyValuePair<int,double>(684,.76), new KeyValuePair<int,double>(157,.02), new KeyValuePair<int,double>(114,.005), new KeyValuePair<int,double>(96,.005), new KeyValuePair<int,double>(99,.001), new KeyValuePair<int,double>(768,.99), new KeyValuePair<int,double>(428,.2), new KeyValuePair<int,double>(428,.05), new KeyValuePair<int,double>(768,.15), new KeyValuePair<int,double>(243,.04), new KeyValuePair<int,double>(99,.001), new KeyValuePair<int,double>(74,.001), new KeyValuePair<int,double>(766,.99), new KeyValuePair<int,double>(766,.9), new KeyValuePair<int,double>(766,.4), new KeyValuePair<int,double>(99,.001), new KeyValuePair<int,double>(769,.99), new KeyValuePair<int,double>(769,.15), new KeyValuePair<int,double>(287,.15), new KeyValuePair<int,double>(226,.06), new KeyValuePair<int,double>(446,.008), new KeyValuePair<int,double>(74,.001)
            };

            Vector2 playerPosition = new Vector2((float)Game1.player.GetBoundingBox().Center.X, (float)Game1.player.GetBoundingBox().Center.Y);

            foreach (KeyValuePair<int,double> kvp in loots)
            {
                int objectToAdd = kvp.Key;
                if(objectToAdd < 0)
                {
                    location.debris.Add(new Debris(Math.Abs(objectToAdd), Game1.random.Next(10, 40), new Vector2(x, y), playerPosition));
                    
                }
                else
                {
                    double chance = kvp.Value * difficulty;
                    while (chance > 1)
                    {
                        location.debris.Add(new Debris(objectToAdd, new Vector2(x, y), playerPosition));
                        chance--;
                    }
                    if (Game1.random.NextDouble() < kvp.Value)
                    {
                        location.debris.Add(new Debris(objectToAdd, new Vector2(x, y), playerPosition));
                    }
                }
            }
        }

        public static Vector2 RotateVector2d(Vector2 inV, float degrees)
        {
            float rads = (float)Math.PI / 180;
            Vector2 result = new Vector2();
            result.X = (float)(inV.X * Math.Cos(rads) - inV.Y * Math.Sin(rads));
            result.Y = (float)(inV.X * Math.Sin(rads) + inV.Y * Math.Cos(rads));
            return result;
        }

    }
}
