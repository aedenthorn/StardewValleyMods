using StardewValley;
using StardewValley.GameData.Machines;
using System;
using System.Collections.Generic;
using Object = StardewValley.Object;

namespace AdvancedSmoking
{
    public partial class ModEntry
    {


        public static void FinishProcess(Item furnace, string inputID, int inputAmount, float speed)
        {
            if (Game1.eventUp || Game1.player?.currentLocation == null)
                return;
            Object template = (Object)ItemRegistry.Create("(BC)13");
            template.Location = Game1.player.currentLocation;
            Item input = ItemRegistry.Create(inputID, inputAmount);
            MachineData data = DataLoader.Machines(Game1.content).GetValueOrDefault("(BC)13");

            if (!MachineDataUtility.TryGetMachineOutputRule(template, data, MachineOutputTrigger.ItemPlacedInMachine, input, Game1.player, Game1.currentLocation, out var rule, out var trigger, out var ignore, out var triggerIgnore))
                return;
            MachineItemOutput outputData = MachineDataUtility.GetOutputData(template, data, rule, input, Game1.player, Game1.player.currentLocation);
            var output = MachineDataUtility.GetOutputItem(template, outputData, input, Game1.player, true, out var overrideMinutesUntilReady);
            if (output == null)
                return;
            if (!Game1.player.addItemToInventoryBool(output))
            {
                Game1.player.currentLocation.debris.Add(new Debris(output, Game1.player.Position));
            }
            furnace.modData.Remove(itemKey);
            furnace.modData.Remove(amountKey);
            furnace.modData.Remove(timeKey);
            if (furnace.modData.ContainsKey(repeatKey))
            {
                Item nullItem = null;
                TryStartFurnace(furnace, inputID, inputAmount, speed, ref nullItem);
            }
        }

        public static bool TryStartFurnace(Item furnace, string inputID, int inputAmount, float speed, ref Item heldItem)
        {
            MachineData data = DataLoader.Machines(Game1.content).GetValueOrDefault("(BC)13");
            Object template = (Object)ItemRegistry.Create("(BC)13");
            template.Location = Game1.player.currentLocation;
            Item input = ItemRegistry.Create(inputID, inputAmount);

            if (!MachineDataUtility.TryGetMachineOutputRule(template, data, MachineOutputTrigger.ItemPlacedInMachine, input, Game1.player, Game1.player.currentLocation, out var outputRule, out var triggerRule, out var outputRuleIgnoringCount, out var triggerIgnoringCount))
            {
                return false;
            }

            // success

            if (heldItem is not null)
            {
                heldItem = ((Object)heldItem).ConsumeStack(triggerRule.RequiredCount);
            }
            else 
            {
                Game1.player.Items.ReduceId(inputID, triggerRule.RequiredCount);
            }
            if (data.AdditionalConsumedItems?.Count > 0)
            {
                foreach (var req in data.AdditionalConsumedItems)
                {
                    Game1.player.Items.ReduceId(req.ItemId, req.RequiredCount);
                }
            }
            if (!string.IsNullOrEmpty(Config.PlaceSound))
            {
                Game1.playSound(Config.PlaceSound);
            }
            furnace.modData[itemKey] = inputID;
            furnace.modData[amountKey] = triggerRule.RequiredCount.ToString();
            furnace.modData[timeKey] = GetTimeTotal(outputRule, speed).ToString();
            return true;
        }

        public static MachineOutputRule GetRule(string itemID)
        {
            MachineData data = DataLoader.Machines(Game1.content).GetValueOrDefault("(BC)13");
            foreach (var rule in data.OutputRules)
            {
                foreach (var t in rule.Triggers)
                {
                    if (t.RequiredItemId == itemID)
                    {
                        return rule;
                    }
                }
            }
            return null;
        }

        public static int GetTimeTotal(MachineOutputRule rule, float speed)
        {
            return rule == null ? -1 : (int)Math.Round(rule.MinutesUntilReady / speed);
        }

        public static float GetFurnaceSpeed(Item item)
        {
            if (item == null || !item.Name.StartsWith(SHelper.ModRegistry.ModID))
                return -1;
            switch (item.Name)
            {
                case "aedenthorn.AdvancedSmoking_CopperFurnace":
                    return Config.TimeMultCopper;
                case "aedenthorn.AdvancedSmoking_IronFurnace":
                    return Config.TimeMultIron;
                case "aedenthorn.AdvancedSmoking_GoldFurnace":
                    return Config.TimeMultGold;
                case "aedenthorn.AdvancedSmoking_IridiumFurnace":
                    return Config.TimeMultIridium;
                default:
                    return -1;
            }
        }

    }
}