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
        private Random rand;
        public static IMonitor PMonitor;
        public static IModHelper PHelper;
        public override void Entry(IModHelper helper)
        {
            Config = this.Helper.ReadConfig<ModConfig>();
            helper.Events.Player.Warped += Warped;
            rand = new Random();
            PMonitor = Monitor;
            PHelper = helper;
        }

        private static int toggleSprite = 0;

        public static void OnRenderedHud(object sender, RenderedHudEventArgs e)
        {
            Monster boss = BossHere(Game1.player.currentLocation);
            if (boss == null)
            {
                return;
            }
            Vector2 bossPos = boss.Position;
            if (!Utility.isOnScreen(bossPos, 0))
            {
                int x = (int)Math.Max(0, Math.Min(Game1.viewport.X + Game1.viewport.Width-48, bossPos.X) - Game1.viewport.X);
                int y = (int)Math.Max(0, Math.Min(Game1.viewport.Y + Game1.viewport.Height-48, bossPos.Y) - Game1.viewport.Y);

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

        private void Warped(object sender, WarpedEventArgs e)
        {

            TryAddBoss(e.NewLocation);
        }

        public static Monster BossHere(GameLocation location)
        {
            using (NetCollection<NPC>.Enumerator enumerator = location.characters.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    NPC j = enumerator.Current;
                    if (j is SerpentBoss)
                    {
                        return (Monster)j;
                    }
                }
            }
            return null;
        }
        private void TryAddBoss(GameLocation location)
        {
            if (BossHere(location) != null)
            {
                return;
            }

            if ((location is MineShaft) && (location as MineShaft).mustKillAllMonstersToAdvance() && rand.Next(0, 100) < Config.ChanceOfBossInMonsterArea)
            {
                this.Monitor.Log("Adding Boss to location", LogLevel.Alert);
                float x = rand.Next(location.map.DisplayWidth / 8, location.map.DisplayWidth * 7 / 8);
                float y = rand.Next(location.map.DisplayHeight / 8, location.map.DisplayHeight * 7 / 8);
                SerpentBoss s = new SerpentBoss(new Microsoft.Xna.Framework.Vector2(), Game1.getMusicTrackName())
                {
                    currentLocation = location,
                };

                location.characters.Add(s);
                PHelper.Events.Display.RenderedHud += OnRenderedHud;
            }
            else if ((location is Town) && rand.Next(0, 100) < Config.ChanceOfBossInTown)
            {
                this.Monitor.Log("Adding Boss to location", LogLevel.Alert);
                SerpentBoss s = new SerpentBoss(new Microsoft.Xna.Framework.Vector2(location.map.DisplayWidth / 2, location.map.DisplayHeight / 2), Game1.getMusicTrackName())
                {
                    currentLocation = location,
                };
                Game1.changeMusicTrack("cowboy_boss", false, Game1.MusicContext.Default);
                location.characters.Add(s);
                PHelper.Events.Display.RenderedHud += OnRenderedHud;
            }
        }
    }
}
