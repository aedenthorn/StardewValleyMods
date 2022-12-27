using HarmonyLib;
using Microsoft.Xna.Framework;
using Netcode;
using Newtonsoft.Json;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using Object = StardewValley.Object;

namespace OmniTools
{
    public partial class ModEntry
    {
        public static string GetToolSound(Tool t)
        {
            if(t is MeleeWeapon)
            {
                if ((t as MeleeWeapon).type.Value == 3)
                    return "swordswipe";
                if ((t as MeleeWeapon).type.Value == 2)
                    return "clubswipe";
                if ((t as MeleeWeapon).type.Value == 1)
                    return "daggerswipe";
            }
            return toolSoundList[toolList.IndexOf(t.GetType())];
        }

        public static bool SwitchTool(Farmer f, Type type, List<ToolInfo> tools)
        {
            int index = type is null ? toolList.IndexOf(typeof(MeleeWeapon)) : toolList.IndexOf(type);
            if (index < 0)
                return false;
            for (int i = tools.Count - 1; i >= 0; i--)
            {
                if (tools[i].description.index == index)
                {
                    Tool t = f.CurrentTool;
                    Tool newTool = GetToolFromInfo(tools[i]);
                    if(newTool is null)
                    {
                        SMonitor.Log($"Invalid tool {tools[i].displayName}, removing", StardewModdingAPI.LogLevel.Warn);
                        tools.RemoveAt(i);
                        if(tools.Count > 0)
                        {
                            f.CurrentTool.modData[toolsKey] = JsonConvert.SerializeObject(tools);
                        }
                        else
                        {
                            f.CurrentTool.modData.Remove(toolsKey);
                        }
                        return false;
                    }
                    if ((type == typeof(MeleeWeapon) && (newTool as MeleeWeapon).isScythe(newTool.ParentSheetIndex)) ||(type is null && !(newTool as MeleeWeapon).isScythe(newTool.ParentSheetIndex)))
                        continue;
                    f.CurrentTool = newTool;
                    tools.RemoveAt(i);
                    tools.Add(new ToolInfo(t));
                    var outTools = new List<ToolInfo>();
                    for (int j = 0; j < tools.Count; j++)
                    {
                        var idx = (j + i) % tools.Count;
                        outTools.Add(tools[idx]);
                    }
                    f.CurrentTool.modData[toolsKey] = JsonConvert.SerializeObject(outTools);
                    return true;
                }
            }
            return false;
        }

        public static Tool CycleTool(Tool currentTool, string toolsString)
        {
            List<ToolInfo> tools = JsonConvert.DeserializeObject<List<ToolInfo>>(toolsString);
            Tool t = GetToolFromInfo(tools[0]);
            if (t is null)
            {
                SMonitor.Log($"Invalid tool {tools[0].displayName}, removing", StardewModdingAPI.LogLevel.Warn);
                tools.RemoveAt(0);
                if (tools.Count > 0)
                {
                    currentTool.modData[toolsKey] = JsonConvert.SerializeObject(tools);
                }
                else
                {
                    currentTool.modData.Remove(toolsKey);
                }
                return currentTool;
            }

            tools.Add(new ToolInfo(currentTool));
            t.modData[toolsKey] = JsonConvert.SerializeObject(tools.Skip(1));
            Game1.playSound(GetToolSound(t));
            return t;
        }

        public static Tool RemoveTool(Tool currentTool, string toolsString)
        {
            List<ToolInfo> tools = JsonConvert.DeserializeObject<List<ToolInfo>>(toolsString);
            Tool t = GetToolFromInfo(tools[0]);
            if (t is null)
            {
                SMonitor.Log($"Invalid tool {tools[0].displayName}, removing", StardewModdingAPI.LogLevel.Warn);
                tools.RemoveAt(0);
                if (tools.Count > 0)
                {
                    currentTool.modData[toolsKey] = JsonConvert.SerializeObject(tools);
                }
                else
                {
                    currentTool.modData.Remove(toolsKey);
                }
                return currentTool;
            }
            if (tools.Count > 1)
            {
                t.modData[toolsKey] = JsonConvert.SerializeObject(tools.Skip(1));
            }
            else
            {
                t.modData.Remove(toolsKey);
            }
            Game1.playSound(GetToolSound(t));
            currentTool.modData.Remove(toolsKey);
            if (!Game1.player.addItemToInventoryBool(currentTool))
            {
                Game1.createItemDebris(currentTool, Game1.player.getStandingPosition(), Game1.player.FacingDirection, null, -1);
            }
            return t;
        }

        public static Tool GetToolFromInfo(ToolInfo toolInfo)
        {
            Tool t = GetToolFromDescription(toolInfo.description.index, toolInfo.description.upgradeLevel);
            foreach (var s in toolInfo.enchantments)
            {
                try
                {
                    var type = typeof(Game1).Assembly.GetType(s);
                    AccessTools.Method(t.enchantments.GetType(), "Add").Invoke(t.enchantments, new object[] { Activator.CreateInstance(type) });
                }
                catch { }
            }
            return t;
        }

        public static ToolDescription? GetDescriptionFromTool(Tool t)
        {
            if(!toolList.Contains(t.GetType()))
                return null;
            if(t is MeleeWeapon)
            {
                return new ToolDescription((byte)toolList.IndexOf(typeof(MeleeWeapon)), t.ParentSheetIndex == -1 ? (byte)(t as MeleeWeapon).InitialParentTileIndex : (byte)t.ParentSheetIndex);
            }
            return new ToolDescription((byte)toolList.IndexOf(t.GetType()), (byte)t.UpgradeLevel);
        }

        public static Tool GetToolFromDescription(int index, byte upgradeLevel)
        {
            Tool t = null;
            switch (index)
            {
                case 0:
                    t = new Axe();
                    break;
                case 1:
                    t = new Hoe();
                    break;
                case 2:
                    t = new FishingRod();
                    break;
                case 3:
                    t = new Pickaxe();
                    break;
                case 4:
                    t = new WateringCan();
                    break;
                case 5:
                    return new MeleeWeapon(upgradeLevel);
                case 6:
                    t = new Slingshot();
                    break;
                case 7:
                    t = new MilkPail();
                    break;
                case 8:
                    t = new Pan();
                    break;
                case 9:
                    t = new Shears();
                    break;
                case 10:
                    t = new Wand();
                    break;
            }
            if(t is not null)
                t.UpgradeLevel = upgradeLevel;
            return t;
        }
    }
}