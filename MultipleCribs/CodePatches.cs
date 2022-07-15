using HarmonyLib;
using Microsoft.Xna.Framework;
using Netcode;
using StardewValley;
using StardewValley.Events;
using StardewValley.Minigames;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Object = StardewValley.Object;

namespace MultipleCribs
{
    public partial class ModEntry
    {
        public static int namePage;

        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.performAction))]
        public class GameLocation_performAction_Patch
        {

            public static bool Prefix(GameLocation __instance, string action, Farmer who, ref bool __result, Location tileLocation)
            {
                try
                {
                    if (action.Split(' ')[0] == "Crib" && who.IsLocalPlayer)
                    {
                        Monitor.Log($"Acting on crib tile {tileLocation}");

                        FarmHouse farmHouse = __instance as FarmHouse;
                        Microsoft.Xna.Framework.Rectangle? crib_location = farmHouse.GetCribBounds();

                        if (crib_location == null)
                            return true;

                        for (int i = 0; i <= Misc.GetExtraCribs(); i++)
                        {
                            Microsoft.Xna.Framework.Rectangle rect = new Microsoft.Xna.Framework.Rectangle(crib_location.Value.X + i * 3, crib_location.Value.Y, crib_location.Value.Width, crib_location.Value.Height);
                            if (rect.Contains(tileLocation.X, tileLocation.Y))
                            {
                                Monitor.Log($"Acting on crib idx {i}");
                                using (List<NPC>.Enumerator enumerator = __instance.characters.GetEnumerator())
                                {
                                    while (enumerator.MoveNext())
                                    {
                                        NPC j = enumerator.Current;
                                        if (j is Child)
                                        {
                                            if (rect.Contains(j.getTileLocationPoint()))
                                            {
                                                if ((j as Child).Age == 1)
                                                {
                                                    Monitor.Log($"Tossing {j.Name}");
                                                    (j as Child).toss(who);
                                                }
                                                else if ((j as Child).Age == 0)
                                                {
                                                    Monitor.Log($"{j.Name} is sleeping");
                                                    Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Locations:FarmHouse_Crib_NewbornSleeping", j.displayName)));
                                                }
                                                else if ((j as Child).isInCrib() && (j as Child).Age == 2)
                                                {
                                                    Monitor.Log($"acting on {j.Name}");
                                                    return j.checkAction(who, __instance);
                                                }
                                                __result = true;
                                                return false;
                                            }
                                        }
                                    }
                                }
                                __result = true;
                                return false;
                            }
                        }

                    }
                }
                catch (Exception ex)
                {
                    Monitor.Log($"Failed in {nameof(ManorHouse_performAction_Prefix)}:\n{ex}", LogLevel.Error);
                }
                return true;
            }

        }
    }
}