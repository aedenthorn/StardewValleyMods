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

namespace BossCreatures
{
    public class ModEntry : Mod
    {
        public static ModConfig Config;
        private static Random rand;
        public static IMonitor PMonitor;
        public static IModHelper PHelper;
        public override void Entry(IModHelper helper)
        {
            Config = this.Helper.ReadConfig<ModConfig>();
            helper.Events.Player.Warped += Warped;
            helper.Events.GameLoop.DayStarted += OnDayStarted;
            rand = new Random();
            PMonitor = Monitor;
            PHelper = helper;
        }

        private void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            CheckedBosses.Clear();
        }

        private static int toggleSprite = 0;

        public static void OnRenderedHud(object sender, RenderedHudEventArgs e)
        {
            Monster boss = BossHere(Game1.player.currentLocation);
            if (boss == null)
            {
                return;
            }


            Texture2D healthBarTexture = new Texture2D(Game1.graphics.GraphicsDevice, (int)Math.Round(Game1.viewport.Width * 0.74f), 30);
            Color[] data = new Color[healthBarTexture.Width * healthBarTexture.Height];
            healthBarTexture.GetData(data);
            for (int i = 0; i < data.Length; i++)
            {
                if(i <= healthBarTexture.Width || i % healthBarTexture.Width == healthBarTexture.Width - 1)
                {
                    data[i] = new Color(1f,0.5f,0.5f);
                }
                else if(data.Length - i < healthBarTexture.Width || i % healthBarTexture.Width == 0)
                {
                    data[i] = new Color(0.5f,0,0);
                }
                else if((i % healthBarTexture.Width) / (float)healthBarTexture.Width < (float)boss.Health / (float)boss.MaxHealth)
                {
                    data[i] = Color.Red;
                }
                else
                {
                    data[i] = Color.Black;
                }
            }
            healthBarTexture.SetData<Color>(data);
            e.SpriteBatch.Draw(healthBarTexture, new Vector2((int)Math.Round(Game1.viewport.Width * 0.13f), 100), Color.White);

            Vector2 bossPos = boss.Position;
            if (!Utility.isOnScreen(bossPos, 0))
            {
                int x = (int)Math.Max(10, Math.Min(Game1.viewport.X + Game1.viewport.Width-58, bossPos.X) - Game1.viewport.X);
                int y = (int)Math.Max(10, Math.Min(Game1.viewport.Y + Game1.viewport.Height-58, bossPos.Y) - Game1.viewport.Y);

                if(toggleSprite < 20)
                {
                    Texture2D texture = PHelper.Content.Load<Texture2D>("Characters/Monsters/Haunted Skull.xnb", ContentSource.GameContent);
                    ClickableTextureComponent bossIcon = new ClickableTextureComponent(new Rectangle(x, y, 48, 48), texture, new Rectangle(toggleSprite > 10 ? 16 : 0, 32, 16, 16), 3f, false);
                    bossIcon.draw(Game1.spriteBatch);
                }
                toggleSprite++;
                toggleSprite = toggleSprite % 30;
            }
        }

        internal static void RevertMusic()
        {
            Game1.changeMusicTrack(defaultMusic, false);
            isFightingBoss = false;
        }

        private void Warped(object sender, WarpedEventArgs e)
        {
            if (isFightingBoss)
            {
                RevertMusic();
            }

            TryAddBoss(e.NewLocation);
        }

        public static Monster BossHere(GameLocation location)
        {
            using (NetCollection<NPC>.Enumerator enumerator = location.characters.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    NPC j = enumerator.Current;
                    if (j is SerpentBoss || j is SkullBoss || j is BugBoss)
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

            CheckedBosses.Add(location.name);

            if ((location is MineShaft) && (location as MineShaft).mustKillAllMonstersToAdvance() && rand.Next(0, 100) < Config.PercentChanceOfBossInMonsterArea)
            {
                SpawnRandomBoss(location);
            }
            else if ((location is Town) && rand.Next(0, 100) < Config.PercentChanceOfBossInTown)
            {
                SpawnRandomBoss(location);
            }
            else if ((location is Forest) && rand.Next(0, 100) < Config.PercentChanceOfBossInForest)
            {
                SpawnRandomBoss(location);
            }
            else if ((location is Mountain) && rand.Next(0, 100) < Config.PercentChanceOfBossInMountain)
            {
                SpawnRandomBoss(location);
            }
            else if ((location is Desert) && rand.Next(0, 100) < Config.PercentChanceOfBossInDesert)
            {
                SpawnRandomBoss(location);
            }
        }

        private void SpawnRandomBoss(GameLocation location)
        {
            float x = rand.Next(location.map.DisplayWidth / 4, location.map.DisplayWidth * 3 / 4);
            float y = rand.Next(location.map.DisplayHeight / 4, location.map.DisplayHeight * 3 / 4);
            Vector2 spawnPos = new Vector2(x, y);
            SpawnRandomBoss(location, spawnPos);
        }
        private void SpawnRandomBoss(GameLocation location, Vector2 spawnPos)
        {

            int r = rand.Next(0, 3);
            switch (r)
            {
                case 0:
                    SerpentBoss s = new SerpentBoss(spawnPos)
                    {
                        currentLocation = location,
                    };
                    location.characters.Add(s);
                    break;
                case 1:
                    SkullBoss k = new SkullBoss(spawnPos)
                    {
                        currentLocation = location,
                    };
                    location.characters.Add(k);
                    break;
                case 2:
                    BugBoss b = new BugBoss(spawnPos)
                    {
                        currentLocation = location,
                    };
                    location.characters.Add(b);
                    break;
            }

            Game1.showGlobalMessage($"A boss creature has appeared!");
            Game1.changeMusicTrack("cowboy_boss", false, Game1.MusicContext.Default);
            defaultMusic = Game1.getMusicTrackName();
            PHelper.Events.Display.RenderedHud += OnRenderedHud;
        }

        public static void SpawnBossLoot(GameLocation location, float x, float y)
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
                    location.debris.Add(new Debris(Math.Abs(objectToAdd), rand.Next(10, 40), new Vector2(x, y), playerPosition));
                    
                }
                else if (Game1.random.NextDouble() < kvp.Value)
                {
                    location.debris.Add(new Debris(objectToAdd, new Vector2(x, y), playerPosition));
                }
            }
        }
    }
}
