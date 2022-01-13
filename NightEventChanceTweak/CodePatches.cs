using HarmonyLib;
using Netcode;
using StardewValley;
using StardewValley.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace NightEventChanceTweak
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry
    {
        public static void Utility_pickFarmEvent_Postfix(ref FarmEvent __result)
        {
            if (!Config.EnableMod)
                return;

            if (__result != null && !(__result is FairyEvent) && !(__result is WitchEvent)) {
                if (!(__result is SoundInTheNightEvent))
                    return;
                int b = AccessTools.FieldRefAccess<SoundInTheNightEvent, NetInt>(__result as SoundInTheNightEvent, "behavior").Value;
                if (b != 0 && b != 1 && b != 3)
                    return;
            }
            SMonitor.Log("Checking for night event");
            Random r = new Random((int)(Game1.stats.DaysPlayed + (uint)((int)Game1.uniqueIDForThisGame / 2)));
            if (Config.CumulativeChance)
            {
                float currentWeight = 0;
                double chance = r.NextDouble();
                if (!Game1.currentSeason.Equals("winter"))
                {
                    currentWeight += Config.FairyChance;
                    if (chance < currentWeight / 100f)
                    {
                        __result = new FairyEvent();
                        SMonitor.Log("Setting fairy event");
                        return;
                    }
                }
                currentWeight += Config.WitchChance;
                if (chance < currentWeight / 100f)
                {
                    __result = new WitchEvent();
                    SMonitor.Log("Setting witch event");
                    return;
                }
                currentWeight += Config.MeteorChance;
                if (chance < currentWeight / 100f)
                {
                    __result = new SoundInTheNightEvent(1);
                    SMonitor.Log("Setting meteor event");
                    return;
                }
                currentWeight += Config.OwlChance;
                if (chance < currentWeight / 100f)
                {
                    __result = new SoundInTheNightEvent(3);
                    SMonitor.Log("Setting owl event");
                    return;
                }
                if (Game1.year > 1 && !Game1.MasterPlayer.mailReceived.Contains("Got_Capsule"))
                {
                    currentWeight += Config.CapsuleChance;
                    if (chance < currentWeight / 100f)
                    {
                        Game1.MasterPlayer.mailReceived.Add("Got_Capsule");
                        __result = new SoundInTheNightEvent(0);
                        SMonitor.Log("Setting capsule event");
                        return;
                    }
                }
            }
            else
            {
                if (r.NextDouble() < Config.FairyChance && !Game1.currentSeason.Equals("winter"))
                {
                    __result = new FairyEvent();
                    SMonitor.Log("Setting fairy event");
                    return;
                }
                if (r.NextDouble() < Config.WitchChance)
                {
                    __result = new WitchEvent();
                    SMonitor.Log("Setting witch event");
                    return;
                }
                if (r.NextDouble() < Config.MeteorChance)
                {
                    __result = new SoundInTheNightEvent(1);
                    SMonitor.Log("Setting meteor event");
                    return;
                }
                if (r.NextDouble() < Config.OwlChance)
                {
                    __result = new SoundInTheNightEvent(3);
                    SMonitor.Log("Setting owl event");
                    return;
                }
                if (r.NextDouble() < Config.CapsuleChance && Game1.year > 1 && !Game1.MasterPlayer.mailReceived.Contains("Got_Capsule"))
                {
                    Game1.MasterPlayer.mailReceived.Add("Got_Capsule");
                    __result = new SoundInTheNightEvent(0);
                    SMonitor.Log("Setting capsule event");
                    return;
                }
            }
            __result = null;
        }
    }
}