using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Extensions;
using StardewValley.GameData.Characters;
using StardewValley.Locations;
using StardewValley.Monsters;
using System;
using System.Linq;
using xTile.Tiles;

namespace Gravedigger
{
	public partial class ModEntry
    {

        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.checkForBuriedItem))]
        public static class GameLocation_checkForBuriedItem_Patch
        {
            public static bool Prefix(GameLocation __instance, int xLocation, int yLocation, bool explosion, bool detectOnly, Farmer who)
            {
                if (__instance is not Town || !graveTiles.Contains(new Point(xLocation, yLocation)))
                    return true;
                if (Config.NPCReactAsGarbage)
                {
                    foreach (NPC npc in Utility.GetNpcsWithinDistance(new Vector2(xLocation, yLocation), 7, __instance))
                    {
                        if (npc.IsVillager)
                        {
                            CharacterData data = null;
                            int friendshipChange = -25;
                            int? emote = ((data != null) ? data.DumpsterDiveEmote : null);
                            Dialogue dialogue = null;
                            int num = npc.Age;
                            if (num != 1)
                            {
                                if (num == 2)
                                {
                                    emote = new int?(emote.GetValueOrDefault(28));
                                    dialogue = dialogue ?? new Dialogue(npc, "Data\\ExtraDialogue:Town_DumpsterDiveComment_Child", false);
                                }
                                else
                                {
                                    emote = new int?(emote.GetValueOrDefault(12));
                                    dialogue = dialogue ?? new Dialogue(npc, "Data\\ExtraDialogue:Town_DumpsterDiveComment_Adult", false);
                                }
                            }
                            else
                            {
                                emote = new int?(emote.GetValueOrDefault(8));
                                dialogue = dialogue ?? new Dialogue(npc, "Data\\ExtraDialogue:Town_DumpsterDiveComment_Adult", false);
                            }
                            npc.doEmote(emote.Value, true);
                            who.changeFriendship(friendshipChange, npc);
                            npc.setNewDialogue(dialogue, true, true);
                            Game1.drawDialogue(npc);
                            break;
                        }
                    }
                }
                if(Game1.random.NextDouble() < Config.GhostChance / 100f)
                {
                    __instance.characters.Add(new Ghost(new Vector2(xLocation, yLocation) * 64f));
                }
                if(Game1.random.NextDouble() >= Config.DigChance / 100f)
                    return true;
                if (Game1.random.NextDouble() < Config.ArtifactChance / 100f)
                {
                    __instance.digUpArtifactSpot(xLocation, yLocation, who);
                }
                else
                {
                    var bones = Game1.objectData.Where(p => p.Value.ContextTags is not null && p.Value.ContextTags.Contains("bone_item") && !Config.NotBones.Contains(p.Key)).Select(p => p.Key);
                    if (bones.Any())
                    {
                        Game1.createObjectDebris(Game1.random.Choose(bones.ToArray()), xLocation, yLocation, -1, 0, 1f, null);
                    }
                }
                return false;
            }
        }
    }
}
