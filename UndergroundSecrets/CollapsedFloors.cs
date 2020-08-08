using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Network;
using System;
using System.Collections.Generic;
using xTile.Tiles;

namespace UndergroundSecrets
{
    public class CollapsedFloors
    {
        private static IModHelper helper;
        private static IMonitor monitor;
        private static ModConfig config;
        private static int[] hole = new int[]
        {
             243, 244, 245, 259, -1, 261, 278, 279, 280
        };

        public static void Initialize(IModHelper _helper, IMonitor _monitor, ModConfig _config)
        {
            helper = _helper;
            monitor = _monitor;
            config = _config;
        }

        public static void Start(MineShaft shaft, ref List<Vector2> superClearCenters, ref List<Vector2> clearCenters, ref List<Vector2> clearSpots)
        {

            int max =  (int)Math.Round(clearCenters.Count * Math.Min(Math.Max(0, config.CollapsedFloorMaxPortion),1));

            int num = Math.Min(clearSpots.Count, Game1.random.Next(0, max));

            monitor.Log($"adding {num} collapsing floor(s) of {max} max");

            List<Vector2> rSpots = Utils.ShuffleList(clearCenters);

            for(int i = 0; i < num; i++)
            {

                monitor.Log($"adding collapsing floor at {(int)rSpots[i].X},{(int)rSpots[i].Y}");
                shaft.setTileProperty((int)rSpots[i].X, (int)rSpots[i].Y, "Back", "TouchAction", "collapseFloor");
                //shaft.setMapTileIndex((int)rSpots[i].X, (int)rSpots[i].Y,49,"Back");
                foreach (Vector2 v in Utils.GetSurroundingTiles(rSpots[i], 4))
                {
                    superClearCenters.Remove(v);
                    if (Math.Abs(v.X - rSpots[i].X) < 3 && Math.Abs(v.Y - rSpots[i].Y) < 3)
                    {
                        clearCenters.Remove(v);
                        if (Math.Abs(v.X - rSpots[i].X) < 2 && Math.Abs(v.Y - rSpots[i].Y) < 2)
                            clearSpots.Remove(v);
                    }
                }
            }
        }
        internal static async void collapseFloor(MineShaft shaft, Vector2 position)
        {
            monitor.Log($"Collapsing floor at {position} {shaft.Name}");

            Vector2[] spots = Utils.getCenteredSpots(position);

            for(int i = 0; i < spots.Length; i++)
            {
                try
                {
                    shaft.removeTile((int)spots[i].X, (int)spots[i].Y, "Back");
                    shaft.setMapTileIndex((int)spots[i].X, (int)spots[i].Y, hole[i], "Buildings");
                }
                catch (Exception ex)
                {
                }
            }
            shaft.playSound("boulderBreak", NetAudio.SoundContext.Default);
            Farmer who = Game1.player;
            who.completelyStopAnimatingOrDoingAction();
            List<FarmerSprite.AnimationFrame> animationFrames = new List<FarmerSprite.AnimationFrame>(){
                new FarmerSprite.AnimationFrame(94, 100, false, false, null, false)
            };
            who.FarmerSprite.setCurrentAnimation(animationFrames.ToArray());
            who.FarmerSprite.PauseForSingleAnimation = true;
            who.FarmerSprite.loop = true;
            who.FarmerSprite.loopThisAnimation = true;
            who.Sprite.currentFrame = 94;
            await System.Threading.Tasks.Task.Delay(1000);
            shaft.enterMineShaft();
            who.completelyStopAnimatingOrDoingAction();
        }
    }
}