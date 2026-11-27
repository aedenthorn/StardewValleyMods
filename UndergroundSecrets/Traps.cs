using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Monsters;
using StardewValley.Network;
using StardewValley.Projectiles;
using System;
using System.Collections.Generic;
using static StardewValley.Network.NetAudio;
using Object = StardewValley.Object;

namespace UndergroundSecrets
{
    internal class Traps
    {
        private static IModHelper helper;
        private static IMonitor monitor;
        private static ModConfig config;
        private static int trapTypes = 4;

        public static void Initialize(IModHelper _helper, IMonitor _monitor, ModConfig _config)
        {
            helper = _helper;
            monitor = _monitor;
            config = _config;
        }

        internal static void Start(MineShaft shaft, ref List<Vector2> superClearCenters, ref List<Vector2> clearCenters, ref List<Vector2> clearSpots)
        {
            int max = (int)Math.Round(clearSpots.Count * Math.Min(Math.Max(0, config.TrapsBaseMaxPortion * Math.Pow(shaft.mineLevel, config.PuzzleChanceIncreaseRate)), 1));

            int num = Math.Min(clearSpots.Count - 1, Game1.random.Next(0, max));

            monitor.Log($"adding {num} traps of {max} max");

            List<Vector2> rSpots = Utils.ShuffleList(clearSpots);

            for (int i = num - 1; i >= 0; i--)
            {
                //monitor.Log($"adding trap at {(int)rSpots[i].X},{(int)rSpots[i].Y}");
                shaft.setTileProperty((int)rSpots[i].X, (int)rSpots[i].Y, "Back", "TouchAction", "randomTrap");
                //shaft.setMapTileIndex((int)rSpots[i].X, (int)rSpots[i].Y,0,"Back");

                foreach (Vector2 v in Utils.GetSurroundingTiles(rSpots[i], 3))
                {
                    superClearCenters.Remove(v);
                    if (Math.Abs(v.X - rSpots[i].X) < 2 && Math.Abs(v.Y - rSpots[i].Y) < 2)
                        clearCenters.Remove(v);
                }
                clearSpots.Remove(rSpots[i]);
            }
        }


        public static void TriggerRandomTrap(MineShaft shaft, Vector2 position, bool remove)
        {
            int which = Game1.random.Next(0, trapTypes);
            switch (which)
            {
                case 0:
                    FireballTrap(shaft, position);
                    break;
                case 1:
                    ExplosionTrap(shaft, position);
                    break;
                case 2:
                    SlimeTrap(shaft, position);
                    break;
                case 3:
                    LightningTrap(shaft, position * Game1.tileSize);
                    break;
            }
            if(remove)
                shaft.removeTileProperty((int)position.X, (int)position.Y, "Back", "TouchAction");
            if(config.ShowTrapNotifications)
                Game1.addHUDMessage(new HUDMessage(helper.Translation.Get("triggered-trap"), 2));
        }

        public static void FireballTrap(MineShaft shaft, Vector2 position)
        {
            shaft.playSound("furnace", SoundContext.Default);

            foreach (Vector2 v in Utils.getCenteredSpots(position))
            {
                try
                {
                    float x = position.X - v.X;
                    float y = position.Y - v.Y;
                    BasicProjectile projectile = new BasicProjectile((int)Math.Ceiling(Math.Sqrt(shaft.mineLevel * config.TrapDamageMult)), 10, 0, 1, 0.5f, x, y, v * Game1.tileSize, "", "", false, false, shaft, Game1.player, false, null);
                    projectile.ignoreTravelGracePeriod.Value = true;
                    projectile.maxTravelDistance.Value = 100;
                    shaft.projectiles.Add(projectile);
                }
                catch(Exception ex) 
                { 
                    monitor.Log($"error creating fire: {ex}"); 
                }
            }

        }
        public static void ExplosionTrap(MineShaft shaft, Vector2 position)
        {
            shaft.playSound("explosion", SoundContext.Default);

            shaft.explode(position, Game1.random.Next(2,8), Game1.player);
        }
        public static void SlimeTrap(MineShaft shaft, Vector2 position)
        {
            shaft.playSound("slime", SoundContext.Default);
            foreach (Vector2 v in Utils.getCenteredSpots(position, true))
            {
                try
                {
                    if (Game1.random.NextDouble() < 0.2)
                    {
                        shaft.characters.Add(new BigSlime(v * Game1.tileSize, shaft.getMineArea(-1)));
                    }
                    else
                    {
                        shaft.characters.Add(new GreenSlime(v * Game1.tileSize, shaft.mineLevel));
                    }
                }
                catch { }
            }

        }
        private static void LightningTrap(MineShaft shaft, Vector2 position)
        {

            Microsoft.Xna.Framework.Rectangle lightningSourceRect = new Rectangle(0, 0, 16, 16);
            float markerScale = 8f;
            Vector2 drawPosition = position + new Vector2(-16 * markerScale / 2 + 32f, -16 * markerScale / 2 + 32f);

            shaft.temporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\Projectiles", lightningSourceRect, 9999f, 1, 999, drawPosition, false, Game1.random.NextDouble() < 0.5, (position.Y + 32f) / 10000f + 0.001f, 0.025f, Color.White, markerScale, 0f, 0f, 0f, false)
            {
                light = true,
                lightRadius = 2f,
                delayBeforeAnimationStart = 0,
                lightcolor = Color.Black
            });
            shaft.playSound("thunder");
            Utility.drawLightningBolt(position + new Vector2(32f, 32f), shaft);

            FarmerCollection.Enumerator enumerator = shaft.farmers.GetEnumerator();
            while (enumerator.MoveNext())
            {
                if (enumerator.Current.currentLocation == shaft && enumerator.Current.GetBoundingBox().Intersects(new Rectangle((int)Math.Round(position.X - 32), (int)Math.Round(position.Y - 32), 64, 64)))
                {
                    enumerator.Current.takeDamage((int)Math.Ceiling(Math.Sqrt(shaft.mineLevel * config.TrapDamageMult)), true, null);
                }
            }
        }

    }
}