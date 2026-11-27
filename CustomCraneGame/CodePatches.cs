using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.Minigames;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CustomCraneGame
{
    public partial class ModEntry
    {
        public static bool skip;
        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.createQuestionDialogue), new Type[] { typeof(string), typeof(Response[]), typeof(GameLocation.afterQuestionBehavior), typeof(NPC) })]
        public class GameLocation_createQuestionDialogue_Patch
        {
            public static void Prefix(GameLocation __instance, ref string question, ref Response[] answerChoices, ref GameLocation.afterQuestionBehavior afterDialogueBehavior)
            {
                if (!Config.ModEnabled || skip)
                {
                    return;
                }
                var game = GetCraneGameAtCursor();
                if (!game && afterDialogueBehavior?.Method.Name.ToLower() != "trytostartcranegame")
                    return;
                lastQuestion.Value = question;
                lastBehavior.Value = afterDialogueBehavior;
                question = SHelper.Translation.Get("which-game");
                answerChoices = GetResponses();
                afterDialogueBehavior = new GameLocation.afterQuestionBehavior(RedirectToOriginal);
            }
        }

        [HarmonyPatch(typeof(CraneGame), new Type[0])]
        [HarmonyPatch(MethodType.Constructor)]
        public class CraneGame_Patch
        {
            public static void Postfix(CraneGame __instance, Dictionary<Type, List<CraneGame.CraneGameObject>> ____gameObjectsByType)
            {
                if (!Config.ModEnabled || lastGameChoice.Value == null || lastGameChoice.Value == normalKey || !____gameObjectsByType.TryGetValue(typeof(CraneGame.Prize), out var list))
                    return;
                list.Clear();

                var data = GameDataDict[lastGameChoice.Value];
                int level_width = 17;

                for (int j = 0; j < data.PrizeMap.Length; j++)
                {
                    if (data.PrizeMap[j] != 0)
                    {
                        int x = j % level_width + 1;
                        int y = j / level_width + 3;
                        Item item = null;
                        int prize_rarity = j;
                        while (prize_rarity > 0 && item == null)
                        {
                            int index = data.PrizeMap[j];
                            if (index - 1 <= 2 && data.NormalPrizes.TryGetValue(index + "", out var prizes))
                            {
                                var itemData = ChoosePrize(prizes);
                                item = ItemRegistry.Create(itemData.ItemId, itemData.Amount, itemData.Quality, true);
                            }
                            prize_rarity--;
                        }
                        if (item is not null)
                        {
                            var prize = new CraneGame.Prize(__instance, item)
                            {
                                position = new(x * 16 + 8, y * 16 + 8)
                            };
                        }
                    }
                }
                foreach(var kvp in data.SpecialPrizes)
                {
                    if (Game1.random.NextDouble() > kvp.Value.Chance)
                        continue;
                    var itemData = ChoosePrize(kvp.Value.Prizes);
                    var item = ItemRegistry.Create(itemData.ItemId, itemData.Amount, itemData.Quality, true);
                    if (item is not null)
                    {
                        var xy = kvp.Key.Split(',');
                        var prize = new CraneGame.Prize(__instance, item)
                        {
                            position = new Vector2(int.Parse(xy[0].Trim()), int.Parse(xy[1].Trim()))
                        };
                    }
                }
                lastQuestion.Value = null;
                lastBehavior.Value = null;
                lastGameChoice.Value = null;
            }

        }
    }
}