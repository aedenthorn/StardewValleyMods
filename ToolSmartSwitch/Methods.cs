using HarmonyLib;
using Microsoft.Xna.Framework;
using Netcode;
using Newtonsoft.Json;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.Monsters;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using xTile.Dimensions;
using Object = StardewValley.Object;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace ToolSmartSwitch
{
    public partial class ModEntry
    {

        private static bool SwitchToolType(Farmer f, Type? type, Dictionary<int, Tool> tools)
        {
            if (CheckTool(f, type))
                return true;
            foreach(var kvp in tools)
            {
                if(type == null)
                {
                    if(kvp.Value.GetType() == typeof(MeleeWeapon) && (kvp.Value as MeleeWeapon).isScythe())
                    {
                        SwitchTool(f, kvp.Key);
                        return true;
                    }
                }
                else if(type == typeof(MeleeWeapon))
                {
                    if(kvp.Value.GetType() == typeof(MeleeWeapon) && !(kvp.Value as MeleeWeapon).isScythe())
                    {
                        SwitchTool(f, kvp.Key);
                        return true;
                    }
                }
                else if(kvp.Value.GetType() == type)
                {
                    SwitchTool(f, kvp.Key);
                    return true;
                }
            }
            return false;
        }

        private static bool CheckTool(Farmer f, Type? type)
        {
            if (f.CurrentTool is null)
                return false;

            if (type == null)
            {
                return f.CurrentTool.GetType() == typeof(MeleeWeapon) && (f.CurrentTool as MeleeWeapon).isScythe();
            }
            if (type == typeof(MeleeWeapon))
            {
                return f.CurrentTool.GetType() == typeof(MeleeWeapon) && !(f.CurrentTool as MeleeWeapon).isScythe();
            }
            return f.CurrentTool.GetType() == type;
        }

        // GetMeleeAttackArea - Methods.cs
        // Returns the rectangular area that a melee weapon swing would hit based on player position and facing direction
        private static Rectangle GetMeleeAttackArea(Farmer f)
        {
            int areaWidth = 96;  // Width of melee attack arc
            int areaLength = 96; // Length/reach of melee attack
            Rectangle playerBox = f.GetBoundingBox();
            int centerX = playerBox.Center.X;
            int centerY = playerBox.Center.Y;

            return f.FacingDirection switch
            {
                0 => new Rectangle(centerX - areaWidth / 2, playerBox.Top - areaLength, areaWidth, areaLength), // Up
                1 => new Rectangle(playerBox.Right, centerY - areaWidth / 2, areaLength, areaWidth),            // Right
                2 => new Rectangle(centerX - areaWidth / 2, playerBox.Bottom, areaWidth, areaLength),           // Down
                3 => new Rectangle(playerBox.Left - areaLength, centerY - areaWidth / 2, areaLength, areaWidth),// Left
                _ => new Rectangle(centerX - areaWidth / 2, centerY - areaWidth / 2, areaWidth, areaWidth)
            };
        }

        public static void SwitchTool(Farmer f, int which)
        {
            Game1.player.CurrentToolIndex = which;
            Game1.playSound("toolSwap");
        }

        // SmartSwitch - Methods.cs
        public static void SmartSwitch(Farmer f)
        {
            if (!Config.FromWeapon && f.CurrentTool is MeleeWeapon && !(f.CurrentTool as MeleeWeapon).isScythe())
                return;

            Vector2 position = ((!Game1.wasMouseVisibleThisFrame) ? Game1.player.GetToolLocation(false) : new Vector2((float)(Game1.getOldMouseX() + Game1.viewport.X), (float)(Game1.getOldMouseY() + Game1.viewport.Y)));

            var tile = f.GetToolLocation(position, false) / 64;
            tile = new Vector2((int)tile.X, (int)tile.Y);

            Dictionary<int, Tool> tools = GetTools(f);

            if (Config.SwitchForMonsters)
            {
                foreach (var c in f.currentLocation.characters)
                {
                    if (c is Monster)
                    {
                        if (c is RockCrab r && !AccessTools.FieldRefAccess<RockCrab, NetBool>(r, "shellGone").Value)
                            continue;
                        var distance = Vector2.Distance(c.GetBoundingBox().Center.ToVector2(), f.GetBoundingBox().Center.ToVector2());
                        if (distance > Config.MonsterMaxDistance)
                            continue;
                        // Check if monster intersects with the player's potential melee weapon hit area
                        // Melee weapons have a wider swing arc, so we use a more lenient check
                        Rectangle attackArea = GetMeleeAttackArea(f);
                        if (!c.GetBoundingBox().Intersects(attackArea))
                            continue;
                        if (SwitchToolType(f, typeof(MeleeWeapon), tools))
                            return;
                    }
                }
            }
            if (Config.SwitchForAnimals)
            {
                FarmAnimal[] animals = new FarmAnimal[0];
                if (f.currentLocation is Farm)
                {
                    animals = (f.currentLocation as Farm).animals.Values.ToArray();
                }
                else if (f.currentLocation is AnimalHouse)
                {
                    animals = (f.currentLocation as AnimalHouse).animals.Values.ToArray();
                }
                foreach (var c in animals)
                {
                    Rectangle r = new Rectangle((int)tile.X * 64 - 32, (int)tile.Y * 64 - 32, 64, 64);
                    if (c.GetHarvestBoundingBox().Intersects(r))
                    {
                        if (SwitchForAnimal(f, c, tools))
                            return;
                    }
                }
            }
            if (Config.SwitchForObjects && f.currentLocation.objects.TryGetValue(tile, out Object obj))
            {
                if (SwitchForObject(f, obj, tools))
                    return;
            }
            if (f.currentLocation.terrainFeatures.TryGetValue(tile, out TerrainFeature tf))
            {
                if (SwitchForTerrainFeature(f, tf, tools))
                    return;
                if (f.currentLocation is Farm && Config.SwitchForWatering && tf is HoeDirt && (tf as HoeDirt).state.Value == 0)
                {
                    if (SwitchToolType(f, typeof(WateringCan), tools))
                        return;
                }
                if (Config.SwitchForCrops && tf is HoeDirt && (tf as HoeDirt).crop != null)
                {
                    var crop = (tf as HoeDirt).crop;
                    if (crop.forageCrop.Value == false && (crop.GetHarvestMethod() == StardewValley.GameData.Crops.HarvestMethod.Grab || Config.HarvestWithScythe) && crop.currentPhase.Value >= crop.phaseDays.Count - 1 && (!crop.fullyGrown.Value || crop.dayOfCurrentPhase.Value <= 0))
                    {
                        if (SwitchToolType(f, null, tools))
                            return;
                    }
                    else if ((tf as HoeDirt).crop.forageCrop.Value == true && (tf as HoeDirt).crop.whichForageCrop.Value == Crop.forageCrop_ginger.ToString())
                    {
                        if (SwitchToolType(f, typeof(Hoe), tools))
                            return;
                    }
                }
            }
            if (Config.SwitchForResourceClumps)
            {
                Rectangle tileRect = new Rectangle((int)tile.X * 64, (int)tile.Y * 64, 64, 64);

                foreach (ResourceClump clump in f.currentLocation.resourceClumps)
                {
                    var bb = clump.getBoundingBox();
                    if (bb.Intersects(tileRect))
                    {
                        if (SwitchForClump(f, clump, tools))
                            return;
                    }
                }
            }
            if (Config.SwitchForPan)
            {
                Rectangle orePanRect = new Rectangle(f.currentLocation.orePanPoint.X * 64 - 64, f.currentLocation.orePanPoint.Y * 64 - 64, 256, 256);
                if (orePanRect.Contains((int)tile.X * 64, (int)tile.Y * 64) && Utility.distance((float)f.getStandingPosition().X, (float)orePanRect.Center.X, (float)f.getStandingPosition().Y, (float)orePanRect.Center.Y) <= 192f)
                {
                    if (SwitchToolType(f, typeof(Pan), tools))
                        return;
                }

            }
            if (Config.SwitchForWateringCan)
            {
                if (f.currentLocation is VolcanoDungeon && (f.currentLocation as VolcanoDungeon).level.Value != 5 && f.currentLocation.isTileOnMap(new Vector2(tile.X, tile.Y)) && f.currentLocation.waterTiles[(int)tile.X, (int)tile.Y] && !(f.currentLocation as VolcanoDungeon).cooledLavaTiles.ContainsKey(new Vector2(tile.X, tile.Y)))
                {
                    if (SwitchToolType(f, typeof(WateringCan), tools)) 
                        return;
                }

                if(f.currentLocation.isTileOnMap(new Vector2(tile.X, tile.Y)) && (!Config.SwitchForFishing || f.CurrentTool is not FishingRod || f.currentLocation.waterTiles is null || !f.currentLocation.waterTiles[(int)tile.X, (int)tile.Y]) && f.currentLocation.CanRefillWateringCanOnTile((int)tile.X, (int)tile.Y))
                {
                    if (SwitchToolType(f, typeof(WateringCan), tools))
                        return;
                }
                foreach (Building building in f.currentLocation.buildings)
                {
                    if (!building.isMoving && building.occupiesTile(tile, true))
                    {
                        string tileProperty = null;
                        if (building.doesTileHaveProperty((int)tile.X, (int)tile.Y, "PetBowl", "Buildings", ref tileProperty))
                        {
                            if (SwitchToolType(f, typeof(WateringCan), tools))
                                return;
                        }
                        break;
                    }
                }
                if (f.currentLocation.objects.TryGetValue(tile, out obj) && obj.Name.EndsWith("Pet Bowl"))
                {
                    if (SwitchToolType(f, typeof(WateringCan), tools))
                        return;
                }
            }

            if (Config.SwitchForFishing && f.currentLocation.waterTiles is not null)
            {
                try
                {
                    if(f.currentLocation.waterTiles[(int)tile.X, (int)tile.Y])
                    {
                        if (SwitchToolType(f, typeof(FishingRod), tools))
                            return;
                    }
                }
                catch { }
            }
            
            if (Config.SwitchForTilling && f.currentLocation.doesTileHaveProperty((int)tile.X, (int)tile.Y, "Diggable", "Back") != null && !f.currentLocation.IsTileOccupiedBy(tile) && f.currentLocation.isTilePassable(new Location((int)tile.X, (int)tile.Y), Game1.viewport))
            {
                if (SwitchToolType(f, typeof(Hoe), tools))
                    return;

            }

        }


        private static Dictionary<int, Tool> GetTools(Farmer f)
        {
            Dictionary<int, Tool> tools = new();
            for (int i = 0; i < 12; i++)
            {
                if (f.Items[i] is Tool)
                    tools.Add(i, (Tool)f.Items[i]);
            }
            return tools;
        }

        public static bool SwitchForAnimal(Farmer f, FarmAnimal c, Dictionary<int, Tool> tools)
        {
            if (c.GetAnimalData().HarvestTool.Equals("Shears")) 
            { 
                return SwitchToolType(f, typeof(Shears), tools); 
            }
            else if (c.GetAnimalData().HarvestTool.Equals("Milk Pail")) 
            {
                return SwitchToolType(f, typeof(MilkPail), tools);

            }

            return false;
        }


        public static bool SwitchForClump(Farmer f, ResourceClump clump, Dictionary<int, Tool> tools)
        {
            if ((clump.parentSheetIndex.Value == 600 || clump.parentSheetIndex.Value == 602))
            {
                return SwitchToolType(f, typeof(Axe), tools);
            }
            else if (new int[] { 622, 672, 752, 754, 756, 758 }.Contains(clump.parentSheetIndex.Value)) 
            {
                return SwitchToolType(f, typeof(Pickaxe), tools);

            }
            return false;
        }

        public static bool SwitchForObject(Farmer f, Object obj, Dictionary<int, Tool> tools)
        {
            if (obj.Name.Equals("Stone"))
            {
                return SwitchToolType(f, typeof(Pickaxe), tools);

            }
            else if (obj.Name.Contains("Twig"))
            {
                return SwitchToolType(f, typeof(Axe), tools);

            }
            else if (obj.ParentSheetIndex == 590)
            {
                return SwitchToolType(f, typeof(Hoe), tools);

            }
            else if (obj.Name.Contains("Weeds"))
            {
                return SwitchToolType(f, null, tools);

            }
            else if (obj is BreakableContainer)
            {
                if (SwitchToolType(f, typeof(MeleeWeapon), tools) || SwitchToolType(f, typeof(Hoe), tools) || SwitchToolType(f, typeof(Axe), tools) || SwitchToolType(f, typeof(Pickaxe), tools))
                    return true;
            }
            return false;
        }

        public static bool SwitchForTerrainFeature(Farmer f, TerrainFeature tf, Dictionary<int, Tool> tools)
        {
            if (Config.SwitchForTrees && tf is Tree) 
            { 
                if((tf as Tree).growthStage.Value >= 3)
                {
                    return SwitchToolType(f, typeof(Axe), tools);
                }
                else if((tf as Tree).growthStage.Value == 1)
                {
                    return SwitchToolType(f, null, tools);
                }
                else
                {
                    if (SwitchToolType(f, typeof(Axe), tools))
                        return true;
                    if (SwitchToolType(f, typeof(Pickaxe), tools))
                        return true;
                    if (SwitchToolType(f, typeof(Hoe), tools))
                        return true;
                }
            }
            else if (Config.SwitchForGrass && tf is Grass) 
            {
                return SwitchToolType(f, null, tools);
            }
            else if (Config.SwitchForCrops && tf is HoeDirt && (tf as HoeDirt).crop?.GetHarvestMethod() == StardewValley.GameData.Crops.HarvestMethod.Scythe && (tf as HoeDirt).crop.currentPhase.Value >= (tf as HoeDirt).crop.phaseDays.Count - 1 && (!(tf as HoeDirt).crop.fullyGrown.Value || (tf as HoeDirt).crop.dayOfCurrentPhase.Value <= 0)) 
            {
                return SwitchToolType(f, null, tools);
            }
            return false;
        }

    }
}