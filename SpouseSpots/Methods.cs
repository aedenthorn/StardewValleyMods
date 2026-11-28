using HarmonyLib;
using StardewValley;
using StardewValley.Locations;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SpouseSpots
{
    public partial class ModEntry
    {
        public static bool SetRandomSpot(NPC npc, FarmHouse fh, bool dialogue = true, bool animation = true)
        {
            if (npc is null || npc.currentLocation != fh)
                return false;
            KeyValuePair<string, SpotData> kvp = GetRandomSpot(npc.Name, fh);
            if (kvp.Key is null || kvp.Value == null)
                return false;
            var d = kvp.Value;
            DoSpot(fh, npc, d, dialogue, animation);
            return true;
        }

        public static void DoSpot(FarmHouse fh, NPC npc, SpotData d, bool dialogue, bool animation)
        {
            npc.Sprite.StopAnimation();
            npc.isSleeping.Value = false;
            AccessTools.Field(typeof(NPC), "isPlayingSleepingAnimation").SetValue(npc, false);
            npc.setTilePosition(d.Tile);
            npc.faceDirection(d.Facing);
            if (dialogue && d.Dialogue is not null)
            {
                npc.currentMarriageDialogue.Clear();
                npc.addMarriageDialogue("MarriageDialogue", d.Dialogue, false, Array.Empty<string>());
            }
            if (animation && d.Animation is not null)
            {
                npc.endOfRouteBehaviorName.Value = d.Animation;
                AccessTools.Method(typeof(NPC), "loadEndOfRouteBehavior").Invoke(npc, new object[] { d.Animation });
                AccessTools.Method(typeof(NPC), "doAnimationAtEndOfScheduleRoute").Invoke(npc, new object[] { npc, fh });

            }
            if (d.StayTime > 0)
            {
                npc.modData[stayKey] = (Game1.timeOfDay + d.StayTime).ToString();
            }
        }

        private static void DoBehaviour(Character c, GameLocation location)
        {
            if (!Config.ModEnabled || !c.modData.TryGetValue(spotKey, out var key) || c is not NPC npc)
                return;
            c.modData.Remove(spotKey);
            var d = GetSpotFromName(npc, key);
            DoSpot(location as FarmHouse, npc, d, true, true);
        }
        public static SpotData GetSpotFromName(NPC npc, string str)
        {
            if (!Config.Spots.TryGetValue(npc.Name, out var dict) || !dict.TryGetValue(str, out var d))
            {
                if (!SpotDict.TryGetValue(npc.Name, out dict) || !dict.TryGetValue(str, out d))
                    return null;
            }
            return d;
        }

        public static KeyValuePair<string, SpotData> GetRandomSpot(string name, FarmHouse fh)
        {
            if (Config.Spots.TryGetValue(name, out var dict))
            {
                var npc = Game1.getCharacterFromName(name);
                var poss = dict.Where(kvp => kvp.Value.StartTime <= Game1.timeOfDay && kvp.Value.EndTime > Game1.timeOfDay && kvp.Value.MinFarmHouseLevel <= fh.upgradeLevel && kvp.Value.MaxFarmHouseLevel >= fh.upgradeLevel && fh.CanItemBePlacedHere(kvp.Value.Tile.ToVector2(), false, CollisionMask.All, ~CollisionMask.Objects, false, false));
                if (poss.Any())
                {
                    KeyValuePair<string, SpotData> kvp = GetRandomSpot(poss);
                    if (kvp.Key != null)
                        return kvp;
                }
            }
            if (SpotDict.TryGetValue(name, out dict))
            {
                var npc = Game1.getCharacterFromName(name);
                var poss = dict.Where(kvp => kvp.Value.StartTime <= Game1.timeOfDay && kvp.Value.EndTime > Game1.timeOfDay && kvp.Value.MinFarmHouseLevel <= fh.upgradeLevel && kvp.Value.MaxFarmHouseLevel >= fh.upgradeLevel && fh.CanItemBePlacedHere(kvp.Value.Tile.ToVector2(), false, CollisionMask.All, ~CollisionMask.Objects, false, false));
                if (poss.Any())
                {
                    return GetRandomSpot(poss);
                }

            }
            return new KeyValuePair<string, SpotData>(null, null);
        }

        private static KeyValuePair<string, SpotData> GetRandomSpot(IEnumerable<KeyValuePair<string, SpotData>> poss)
        {
            int totalWeight = poss.Sum(d => d.Value.Weight);
            int roll = Game1.random.Next(totalWeight);
            int addWeight = 0;
            foreach (var kvp in poss)
            {
                addWeight += kvp.Value.Weight;
                if (addWeight > roll)
                {

                    return kvp;
                }
            }
            return new KeyValuePair<string, SpotData>(null, null);
        }
    }
}