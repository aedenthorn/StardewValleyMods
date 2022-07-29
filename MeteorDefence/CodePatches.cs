using HarmonyLib;
using Microsoft.Xna.Framework;
using Netcode;
using StardewValley;
using StardewValley.Events;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MeteorDefence
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry
    {
        public static bool playedDefence;
        public static bool started;
        public static bool playedExplode;
        public static int struckSound;
        public static int struck;
        public static int defence;
        public static int total;
        public static List<Vector2> strikeLocations;
        public static bool SoundInTheNightEvent_setUp_Prefix(SoundInTheNightEvent __instance, ref int ___timer, NetInt ___behavior, ref string ___soundName, ref string ___message, ref Vector2 ___targetLocation, ref bool __result)
        {
            if (!Config.EnableMod || ___behavior != 1)
                return true;
            Random r = new Random((int)Game1.uniqueIDForThisGame + (int)Game1.stats.DaysPlayed);
            Farm f = Game1.getLocationFromName("Farm") as Farm;

            strikeLocations = new List<Vector2>();
            playedDefence = false;
            playedExplode = false;
            struck = 0;
            struckSound = 0;
            total = r.Next(Config.MinMeteorites, Config.MaxMeteorites + 1);
            defence = Game1.getFarm().objects.Values.ToList().FindAll(o => o.ParentSheetIndex + "" == Config.DefenceObject || o.Name == Config.DefenceObject || o.Name.EndsWith("/"+Config.DefenceObject)).Count * Config.MeteorsPerObject;

            ___soundName = "Meteorite";
            ___message = Game1.content.LoadString("Strings\\Events:SoundInTheNight_Meteorite");
            __result = true;
            for (int i = 0; i < total; i++)
            {
                var target = new Vector2(r.Next(5, f.map.GetLayer("Back").TileWidth - 20), r.Next(5, f.map.GetLayer("Back").TileHeight - 4));
                if (!Config.StrikeAnywhere)
                {
                    while (i <= target.X + 1f)
                    {
                        int j = (int)target.Y;
                        while (j <= target.Y + 1f)
                        {
                            Vector2 v = new Vector2(i, j);
                            j++;
                            if (!f.isTileOpenBesidesTerrainFeatures(v) || !f.isTileOpenBesidesTerrainFeatures(new Vector2(v.X + 1f, v.Y)) || !f.isTileOpenBesidesTerrainFeatures(new Vector2(v.X + 1f, v.Y - 1f)) || !f.isTileOpenBesidesTerrainFeatures(new Vector2(v.X, v.Y - 1f)) || f.doesTileHaveProperty((int)v.X, (int)v.Y, "Water", "Back") != null || f.doesTileHaveProperty((int)v.X + 1, (int)v.Y, "Water", "Back") != null)
                            {
                                continue;
                            }
                        }
                        i++;
                    }
                }
                if (strikeLocations.Count == 0)
                    ___targetLocation = target;
                strikeLocations.Add(target);
                __result = false;
            }
            if(!__result)
                SMonitor.Log($"Starting meteorite strike with {strikeLocations.Count}/{total} meteorites, {defence} defence objects found");
            else
                SMonitor.Log($"Cancelling meteorite strike with {strikeLocations.Count}/{total} meteorites, {defence} defence objects found");
            return false;
        }
        public static bool SoundInTheNightEvent_makeChangesToLocation_Prefix(SoundInTheNightEvent __instance, NetInt ___behavior, ref Vector2 ___targetLocation)
        {
            if (!Config.EnableMod || ___behavior != 1 || (defence > -1 && struck >= defence))
            {
                SMonitor.Log($"dropping meteor {struck} at {___targetLocation}");
                return true;
            }
            SMonitor.Log($"dropping debris for struck meteor {struck} at {___targetLocation}");
            Game1.createMultipleObjectDebris(386, (int)___targetLocation.X, (int)___targetLocation.Y, 6, Game1.MasterPlayer.UniqueMultiplayerID, Game1.getFarm());
            Game1.createMultipleObjectDebris(390, (int)___targetLocation.X, (int)___targetLocation.Y, 6, Game1.MasterPlayer.UniqueMultiplayerID, Game1.getFarm());
            Game1.createMultipleObjectDebris(535, (int)___targetLocation.X, (int)___targetLocation.Y, 2, Game1.MasterPlayer.UniqueMultiplayerID, Game1.getFarm());
            return false;
        }
        public static void SoundInTheNightEvent_makeChangesToLocation_Postfix(SoundInTheNightEvent __instance, NetInt ___behavior, ref Vector2 ___targetLocation)
        {
            if (!Config.EnableMod || ___behavior != 1 || struck >= strikeLocations.Count - 1)
                return;
            ___targetLocation = strikeLocations[++struck];
            //SMonitor.Log($"continuing with meteor {struck} at {___targetLocation}");
            __instance.makeChangesToLocation();
        }
        public static void SoundInTheNightEvent_tickUpdate_Prefix(SoundInTheNightEvent __instance, NetInt ___behavior, ref int ___timer, ref bool ___playedSound, GameTime time)
        {
            if (!Config.EnableMod || ___behavior != 1)
                return;

            var e = time.ElapsedGameTime.Milliseconds;
            if(___timer + e > 3500 && !playedDefence && (defence < 0 || defence > struckSound))
            {
                SMonitor.Log("Playing defence sound");
                Game1.playSound(Config.DefenceSound);
                playedDefence = true;
            }
            if(___timer + e > 5300 && !playedExplode && (defence < 0 || defence > struckSound))
            {
                SMonitor.Log("Playing explode sound");
                Game1.playSound(Config.ExplodeSound);
                playedExplode = true;
            }
            if (___timer + e > 5300 && struckSound < strikeLocations.Count - 1)
            {
                struckSound++;
                playedDefence = false;
                playedExplode = false;
                ___playedSound = false;
                ___timer = 1000;
            }
        }
    }
}
