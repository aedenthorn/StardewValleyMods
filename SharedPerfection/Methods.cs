using StardewModdingAPI;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.GameData;
using StardewValley.GameData.Characters;
using StardewValley.GameData.Objects;
using StardewValley.ItemTypeDefinitions;
using System;
using System.Collections.Generic;
using System.Linq;
using Object = StardewValley.Object;

namespace SharedPerfection
{
    public partial class ModEntry : Mod
    {
        public static float GetFarmerItemsShippedPercent()
        {
            Utility.recentlyDiscoveredMissingBasicShippedItem = null;
            int farmerShipped = 0;
            int total = 0;
            foreach (ParsedItemData data in ItemRegistry.GetObjectTypeDefinition().GetAllData())
            {
                int category = data.Category;
                if (category != -7 && category != -2 && Object.isPotentialBasicShipped(data.ItemId, data.Category, data.ObjectType))
                {
                    total++;
                    foreach (var who in Game1.getAllFarmers())
                    {
                        if (who.basicShipped.ContainsKey(data.ItemId))
                        {
                            farmerShipped++;
                            break;
                        }
                        else if (Utility.recentlyDiscoveredMissingBasicShippedItem == null)
                        {
                            Utility.recentlyDiscoveredMissingBasicShippedItem = ItemRegistry.Create(data.QualifiedItemId, 1, 0, false);
                        }
                    }
                }
            }
            return farmerShipped / total;
        }
        public static bool HasCompletedAllMonsterSlayerQuests()
        {
            foreach (MonsterSlayerQuestData questData in DataLoader.MonsterSlayerQuests(Game1.content).Values)
            {
                int count = 0;
                if (questData.Targets != null)
                {
                    foreach (string targetType in questData.Targets)
                    {
                        foreach (var who in Game1.getAllFarmers())
                            count += who.stats.getMonstersKilled(targetType);
                        if (count >= questData.Count)
                        {
                            break;
                        }
                    }
                    if (count < questData.Count)
                    {
                        return false;
                    }
                }
            }
            return true;
        }
        public static float GetMaxedFriendshipPercent()
        {
            int maxedFriends = 0;
            int totalFriends = 0;
            foreach (KeyValuePair<string, CharacterData> pair in Game1.characterData)
            {
                string npcName = pair.Key;
                CharacterData data = pair.Value;
                if (data.PerfectionScore && !GameStateQuery.IsImmutablyFalse(data.CanSocialize))
                {
                    totalFriends++;

                    foreach (var who in Game1.getAllFarmers())
                    {
                        Friendship friendship;
                        if (who.friendshipData.TryGetValue(npcName, out friendship))
                        {
                            int maxPoints = (data.CanBeRomanced ? 8 : 10) * 250;
                            if (friendship != null && friendship.Points >= maxPoints)
                            {
                                maxedFriends++;
                                break;
                            }
                        }
                    }
                }
            }
            return (float)maxedFriends / ((float)totalFriends * 1f);
        }
        public static int MaxFarmerLevel()
        {
            int maxLevel = 0;
            foreach (var who in Game1.getAllFarmers())
            {
                if (who.Level > maxLevel)
                    maxLevel = who.Level;
            }
            return maxLevel;
        }
        public static bool FoundAllStardrops()
        {
            foreach (var who in Game1.getAllFarmers())
            {
                if (who.mailReceived.Contains("gotMaxStamina") || (who.hasOrWillReceiveMail("CF_Fair") && who.hasOrWillReceiveMail("CF_Fish") && (who.hasOrWillReceiveMail("CF_Mines") || who.chestConsumedMineLevels.GetValueOrDefault(100, false)) && who.hasOrWillReceiveMail("CF_Sewer") && who.hasOrWillReceiveMail("museumComplete") && who.hasOrWillReceiveMail("CF_Spouse") && who.hasOrWillReceiveMail("CF_Statue")))
                {
                    return true;
                }
            }
            return false;
        }
        public static float GetCookedRecipesPercent()
        {
            Dictionary<string, string> recipes = CraftingRecipe.cookingRecipes;
            float numberOfRecipesCooked = 0f;
            foreach (KeyValuePair<string, string> v in recipes)
            {
                string recipeKey = v.Key;
                foreach (var who in Game1.getAllFarmers())
                {
                    if (who.cookingRecipes.ContainsKey(recipeKey))
                    {
                        string recipe = ArgUtility.SplitBySpaceAndGet(ArgUtility.Get(v.Value.Split('/', StringSplitOptions.None), 2, null, true), 0, null);
                        if (who.recipesCooked.ContainsKey(recipe))
                        {
                            numberOfRecipesCooked += 1f;
                            break;
                        }
                    }
                }
            }
            return numberOfRecipesCooked / recipes.Count;
        }
        public static float GetCraftedRecipesPercent()
        {
            Dictionary<string, string> recipes = CraftingRecipe.craftingRecipes;
            float numberOfRecipesMade = 0f;
            foreach (string s in recipes.Keys)
            {
                foreach (var who in Game1.getAllFarmers())
                {
                    int timesCrafted;
                    if (!(s == "Wedding Ring") && who.craftingRecipes.TryGetValue(s, out timesCrafted) && timesCrafted > 0)
                    {
                        numberOfRecipesMade += 1f;
                        break;
                    }
                }
            }
            return numberOfRecipesMade / (recipes.Count - 1f);
        }
        public static float GetFishCaughtPercent()
        {
            float fishCaught = 0f;
            float totalFish = 0f;
            foreach (ParsedItemData data in ItemRegistry.GetObjectTypeDefinition().GetAllData())
            {
                if (data.ObjectType == "Fish")
                {
                    foreach (var who in Game1.getAllFarmers())
                    {
                        ObjectData objData = data.RawData as ObjectData;
                        if (objData == null || !objData.ExcludeFromFishingCollection)
                        {
                            totalFish += 1f;
                            if (who.fishCaught.ContainsKey(data.QualifiedItemId))
                            {
                                fishCaught += 1f;
                                break;
                            }
                        }
                    }
                }
            }
            return fishCaught / totalFish;
        }
    }
}
