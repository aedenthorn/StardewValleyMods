using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using Newtonsoft.Json.Linq;
using StardewValley;
using StardewValley.Buffs;
using StardewValley.Menus;
using StardewValley.Triggers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Object = StardewValley.Object;

namespace PeopleSeeds
{
    public partial class ModEntry
    {
        public static string whichSeasoning = null;
        public static IClickableMenu currentMenu;
        public static ClickableTextureComponent clicked;
        public static Item crafted;
        public static Object buffing;
        public static bool sound;
        [HarmonyPatch(typeof(CraftingPage), "clickCraftingRecipe")]
        public static class CraftingPage_clickCraftingRecipe_Patch
        {
            public static bool Prefix(CraftingPage __instance, ClickableTextureComponent c, bool playSound)
            {
                if(!Config.ModEnabled || !__instance.cooking || whichSeasoning != null)
                    return true;
                List<Response> responses = new List<Response>();
                if (Game1.player.Items.CountId("917") > 0)
                {
                    responses.Add(new Response("917", Object.GetObjectDisplayName("(O)917", null, null)));
                }
                var containers = (IList<Item>)AccessTools.Method(typeof(CraftingPage), "getContainerContents").Invoke(__instance, new object[0]);
                foreach (var key in SeasoningDict.Keys)
                {
                    List<KeyValuePair<string, int>> ingredients = new List<KeyValuePair<string, int>>() { new(key, 1) };
                    if (CraftingRecipe.DoesFarmerHaveAdditionalIngredientsInInventory(ingredients, containers))
                    {
                        responses.Add(new Response(key, Object.GetObjectDisplayName("(O)"+key, null, null)));
                    }
                }
                if(responses.Count == 0)
                {
                    return true;
                }
                responses.Add(new Response("none", SHelper.Translation.Get("none")));
                currentMenu = Game1.activeClickableMenu;
                clicked = c;
                sound = playSound;
                Game1.currentLocation.createQuestionDialogue(SHelper.Translation.Get("choose-seasoning"), responses.ToArray(), "seasoning");
                return false;
            }
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling CraftingPage_clickCraftingRecipe");

                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (codes[i].opcode == OpCodes.Ldnull && codes[i + 1].opcode == OpCodes.Stloc_1)
                    {
                        SMonitor.Log($"Inserting method to add seasoning");
                        codes[i].opcode = OpCodes.Call;
                        codes[i].operand = AccessTools.Method(typeof(ModEntry), nameof(ModEntry.AddSeasoning));
                        break;
                    }
                }

                return codes.AsEnumerable();
            }
            public static void Postfix()
            {
                whichSeasoning = null;
            }

        }

        [HarmonyPatch(typeof(CraftingRecipe), nameof(CraftingRecipe.ConsumeAdditionalIngredients))]
        public static class CraftingRecipe_ConsumeAdditionalIngredients_Patch
        {
            public static void Postfix()
            {
                whichSeasoning = null;
            }
        }

        [HarmonyPatch(typeof(DialogueBox), nameof(DialogueBox.draw), new Type[] { typeof(SpriteBatch) })]
        public static class DialogueBox_draw_Patch
        {
            public static void Postfix(DialogueBox __instance, SpriteBatch b)
            {
                if (!Config.ModEnabled || __instance.responses == null || __instance.responseCC == null || Game1.currentLocation?.lastQuestionKey != "seasoning")
                    return;
                foreach(var cc in __instance.responseCC)
                {
                    if(cc.containsPoint(Game1.getMouseX(), Game1.getMouseY()))
                    {
                        var key = __instance.responses[cc.myID].responseKey;
                        if (key == "none")
                            return;
                        else if (key == "917")
                        {
                            IClickableMenu.drawHoverText(b, SHelper.Translation.Get("gold"), Game1.smallFont);
                            return;
                        }
                        if (!SeasoningDict.TryGetValue(key, out var seasoningData))
                            return;
                        List<string> tooltip = new();
                        if(seasoningData.Quality != 0)
                        {
                            string quality = seasoningData.Quality switch
                            {
                                1 => SHelper.Translation.Get("silver"),
                                2 => SHelper.Translation.Get("gold"),
                                4 => SHelper.Translation.Get("iridium"),
                                _ => seasoningData.Quality.ToString()
                            };
                            tooltip.Add(quality);
                        }
                        if(seasoningData.DurationMultiplier != 1)
                        {
                            tooltip.Add(string.Format(SHelper.Translation.Get("duration"), seasoningData.DurationMultiplier));
                        }
                        if(seasoningData.BuffData != null)
                        {
                            if (seasoningData.BuffData.CombatLevel != 0)
                                tooltip.Add(string.Format(SHelper.Translation.Get("combat"), seasoningData.BuffData.CombatLevel));
                            if (seasoningData.BuffData.FarmingLevel != 0)
                                tooltip.Add(string.Format(SHelper.Translation.Get("farming"), seasoningData.BuffData.FarmingLevel));
                            if (seasoningData.BuffData.FishingLevel != 0)
                                tooltip.Add(string.Format(SHelper.Translation.Get("fishing"), seasoningData.BuffData.FishingLevel));
                            if (seasoningData.BuffData.MiningLevel != 0)
                                tooltip.Add(string.Format(SHelper.Translation.Get("mining"), seasoningData.BuffData.MiningLevel));
                            if (seasoningData.BuffData.LuckLevel != 0)
                                tooltip.Add(string.Format(SHelper.Translation.Get("luck"), seasoningData.BuffData.LuckLevel));
                            if (seasoningData.BuffData.ForagingLevel != 0)
                                tooltip.Add(string.Format(SHelper.Translation.Get("foraging"), seasoningData.BuffData.ForagingLevel));
                            if (seasoningData.BuffData.MaxStamina != 0)
                                tooltip.Add(string.Format(SHelper.Translation.Get("maxstamina"), seasoningData.BuffData.MaxStamina));
                            if (seasoningData.BuffData.MagneticRadius != 0)
                                tooltip.Add(string.Format(SHelper.Translation.Get("magneticradius"), seasoningData.BuffData.MagneticRadius));
                            if (seasoningData.BuffData.Speed != 0)
                                tooltip.Add(string.Format(SHelper.Translation.Get("speed"), seasoningData.BuffData.Speed));
                            if (seasoningData.BuffData.Defense != 0)
                                tooltip.Add(string.Format(SHelper.Translation.Get("defense"), seasoningData.BuffData.Defense));
                            if (seasoningData.BuffData.Attack != 0)
                                tooltip.Add(string.Format(SHelper.Translation.Get("attack"), seasoningData.BuffData.Attack));
                            if (seasoningData.BuffData.AttackMultiplier != 0)
                                tooltip.Add(string.Format(SHelper.Translation.Get("attackmultiplier"), seasoningData.BuffData.AttackMultiplier));
                            if (seasoningData.BuffData.Immunity != 0)
                                tooltip.Add(string.Format(SHelper.Translation.Get("immunity"), seasoningData.BuffData.Immunity));
                            if (seasoningData.BuffData.KnockbackMultiplier != 0)
                                tooltip.Add(string.Format(SHelper.Translation.Get("knockback"), seasoningData.BuffData.KnockbackMultiplier));
                            if (seasoningData.BuffData.WeaponSpeedMultiplier != 0)
                                tooltip.Add(string.Format(SHelper.Translation.Get("weaponspeed"), seasoningData.BuffData.WeaponSpeedMultiplier));
                            if (seasoningData.BuffData.CriticalChanceMultiplier != 0)
                                tooltip.Add(string.Format(SHelper.Translation.Get("critchance"), seasoningData.BuffData.CriticalChanceMultiplier));
                            if (seasoningData.BuffData.CriticalPowerMultiplier != 0)
                                tooltip.Add(string.Format(SHelper.Translation.Get("critpower"), seasoningData.BuffData.CriticalPowerMultiplier));
                            if (seasoningData.BuffData.WeaponPrecisionMultiplier != 0)
                                tooltip.Add(string.Format(SHelper.Translation.Get("weaponprecision"), seasoningData.BuffData.WeaponPrecisionMultiplier));

                        }
                        if(tooltip.Any())
                            IClickableMenu.drawHoverText(b, string.Join('\n', tooltip), Game1.smallFont);
                        return;
                    }
                }
            }
        }

        [HarmonyPatch(typeof(Farmer), nameof(Farmer.CanMove))]
        [HarmonyPatch(MethodType.Setter)]
        public static class Farmer_CanMove_Patch
        {
            public static void Prefix(bool value)
            {
                var x = Environment.StackTrace;
            }
        }

        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.answerDialogueAction))]
        public static class GameLocation_answerDialogueAction_Patch
        {
            public static bool Prefix(string questionAndAnswer, string[] questionParams)
            {
                if(!Config.ModEnabled || currentMenu == null || !questionAndAnswer.StartsWith("seasoning_"))
                    return true;
                whichSeasoning = questionAndAnswer[("seasoning_".Length)..];
                Game1.player.CanMove = true;
                Game1.activeClickableMenu = currentMenu;
                currentMenu = null;
                var page = Game1.activeClickableMenu as CraftingPage;
                AccessTools.Method(typeof(CraftingPage), "clickCraftingRecipe").Invoke(page, new object[] { clicked, sound });
                clicked = null;
                sound = false;
                return false;
            }
        }
        [HarmonyPatch(typeof(CraftingRecipe), nameof(CraftingRecipe.DoesFarmerHaveAdditionalIngredientsInInventory))]
        public static class CraftingRecipe_DoesFarmerHaveAdditionalIngredientsInInventory_Patch
        {
            public static bool Prefix(ref bool __result)
            {
                if(!Config.ModEnabled || whichSeasoning == null)
                    return true;
                __result = false;
                return false;
            }
        }
        [HarmonyPatch(typeof(CraftingRecipe), nameof(CraftingRecipe.createItem))]
        public static class CraftingRecipe_createItem_Patch
        {
            public static void Postfix(Item __result)
            {
                if(!Config.ModEnabled || whichSeasoning == null)
                    return;
                crafted = __result;
            }
        }
        [HarmonyPatch(typeof(Farmer), nameof(Farmer.doneEating))]
        public static class Farmer_doneEating_Patch
        {
            public static void Postfix(Farmer __instance)
            {
                if(!Config.ModEnabled || !GetSeasoningData(__instance.itemToEat, out SeasoningData seasoningData) || seasoningData.TriggerAction == null)
                    return;
                TriggerActionManager.Raise(seasoningData.TriggerAction, new[] { __instance.UniqueMultiplayerID.ToString(), __instance.itemToEat.ItemId });
            }
        }
        [HarmonyPatch(typeof(Object), nameof(Object.drawInMenu), new Type[] { typeof(SpriteBatch), typeof(Vector2), typeof(float), typeof(float), typeof(float), typeof(StackDrawType), typeof(Color), typeof(bool) })]
        public static class Object_drawInMenu_Patch
        {
            public static void Prefix(Object __instance, ref Color color)
            {
                if(!Config.ModEnabled || !GetSeasoningData(__instance, out SeasoningData seasoningData) || seasoningData.Tint == Color.Transparent || seasoningData.TintAlpha <= 0f)
                    return;
                color = Color.Lerp(color, seasoningData.Tint, seasoningData.TintAlpha);
            }
        }
        [HarmonyPatch(typeof(Object), nameof(Object.DisplayName))]
        [HarmonyPatch(MethodType.Getter)]
        public static class Object_DisplayName_Patch
        {
            public static void Postfix(Object __instance, ref string __result)
            {
                if(!Config.ModEnabled || !GetSeasoningData(__instance, out SeasoningData seasoningData) || seasoningData.ChangedName == null)
                    return;
                __result = string.Format(seasoningData.ChangedName, __result);
            }
        }
        [HarmonyPatch(typeof(Item), nameof(Item.canStackWith))]
        public static class Item_canStackWith_Patch
        {
            public static void Postfix(Item __instance, ISalable other, ref bool __result)
            {
                if(!Config.ModEnabled || !__result || !__instance.modData.TryGetValue(seasoningKey, out string seasoning) || other is not Item item)
                    return;
                
                __result = item.modData.TryGetValue(seasoningKey, out string otherSeasoning) && seasoning == otherSeasoning;
            }
        }
        [HarmonyPatch(typeof(Object), nameof(Object.GetFoodOrDrinkBuffs))]
        public static class Object_GetFoodOrDrinkBuffs_Patch
        {
            public static void Prefix(Object __instance)
            {
                buffing = null;
                if (!Config.ModEnabled || !GetSeasoningData(__instance, out SeasoningData seasoningData))
                    return;
                buffing = __instance;
            }
        }
        [HarmonyPatch(typeof(Object), nameof(Object.TryCreateBuffsFromData))]
        public static class Object_TryCreateBuffsFromData_Patch
        {
            public static void Prefix(ref float durationMultiplier)
            {
                if (!Config.ModEnabled || buffing == null || !GetSeasoningData(buffing, out SeasoningData seasoningData))
                    return;
                durationMultiplier = (1 + buffing.Quality / 4f) * seasoningData.DurationMultiplier;
                buffing = null;
            }
        }
        [HarmonyPatch(typeof(Object), nameof(Object.ModifyItemBuffs))]
        public static class Object_ModifyItemBuffs_Patch
        {
            public static bool Prefix(Object __instance, BuffEffects effects)
            {
                if(!Config.ModEnabled || !GetSeasoningData(__instance, out SeasoningData seasoningData) || effects == null || __instance.Category != -7)
                    return true;
                int buff_bonus = __instance.Quality / 2;
                if (buff_bonus > 0)
                {
                    foreach (NetFloat effect in new NetFloat[] { effects.FarmingLevel, effects.FishingLevel, effects.MiningLevel, effects.LuckLevel, effects.ForagingLevel, effects.MaxStamina, effects.MagneticRadius, effects.Defense, effects.Attack })
                    {
                        if (effect.Value != 0f)
                        {
                            effect.Value += buff_bonus;
                        }
                    }
                }
                if (seasoningData.BuffData != null)
                {
                    effects.CombatLevel.Value += seasoningData.BuffData.CombatLevel;
                    effects.FarmingLevel.Value += seasoningData.BuffData.FarmingLevel;
                    effects.FishingLevel.Value += seasoningData.BuffData.FishingLevel;
                    effects.MiningLevel.Value += seasoningData.BuffData.MiningLevel;
                    effects.LuckLevel.Value += seasoningData.BuffData.LuckLevel;
                    effects.ForagingLevel.Value += seasoningData.BuffData.ForagingLevel;
                    effects.MaxStamina.Value += seasoningData.BuffData.MaxStamina;
                    effects.MagneticRadius.Value += seasoningData.BuffData.MagneticRadius;
                    effects.Speed.Value += seasoningData.BuffData.Speed;
                    effects.Defense.Value += seasoningData.BuffData.Defense;
                    effects.Attack.Value += seasoningData.BuffData.Attack;
                    effects.AttackMultiplier.Value += seasoningData.BuffData.AttackMultiplier;
                    effects.Immunity.Value += seasoningData.BuffData.Immunity;
                    effects.KnockbackMultiplier.Value += seasoningData.BuffData.KnockbackMultiplier;
                    effects.WeaponSpeedMultiplier.Value += seasoningData.BuffData.WeaponSpeedMultiplier;
                    effects.CriticalChanceMultiplier.Value += seasoningData.BuffData.CriticalChanceMultiplier;
                    effects.CriticalPowerMultiplier.Value += seasoningData.BuffData.CriticalPowerMultiplier;
                    effects.WeaponPrecisionMultiplier.Value += seasoningData.BuffData.WeaponPrecisionMultiplier;
                }
                return false;
            }
        }
        [HarmonyPatch(typeof(IClickableMenu), nameof(IClickableMenu.drawToolTip))]
        public static class IClickableMenu_drawToolTip_Patch
        {
            public static void Prefix(Item hoveredItem)
            {
                buffing = null;
                if (!Config.ModEnabled || hoveredItem is not Object obj || !GetSeasoningData(hoveredItem, out SeasoningData seasoningData))
                    return;
                buffing = obj;
            }
        }
    }
}