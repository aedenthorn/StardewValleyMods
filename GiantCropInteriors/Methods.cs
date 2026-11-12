using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.TerrainFeatures;
using System;
using System.Linq;

namespace GiantCropInteriors
{
    public partial class ModEntry
    {
        private void CheckCrop(GiantCrop crop)
        {
            if (crop.Location is null)
                return;
            if (!Config.ModEnabled || !BuildingDict.ContainsKey(crop.Id))
            {
                if (crop.modData.ContainsKey(builtAtKey) && crop.Location.getBuildingAt(crop.Tile) is Building old)
                {
                    crop.Location.buildings.Remove(old);
                }
                return;
            }
            if (crop.Location.buildings.FirstOrDefault(i => (int)crop.Tile.X == i.tileX.Value && (int)crop.Tile.Y == i.tileY.Value) is Building exists)
            {
                if (exists.buildingType.Value == modPrefix + crop.Id)
                {
                    return;
                }
                crop.Location.buildings.Remove(exists);
            }
            else if (crop.modData.TryGetValue(builtAtKey, out var str) && crop.Location.buildings.FirstOrDefault(i => str == $"{i.tileX},{i.tileY}") is Building moved && moved.buildingType.Value == modPrefix + crop.Id)
            {
                moved.tileX.Value = (int)crop.Tile.X;
                moved.tileY.Value = (int)crop.Tile.Y;
                return;
            }
            Building b = new Building(modPrefix + crop.Id, crop.Tile);
            b.FinishConstruction(true);
            b.LoadFromBuildingData(b.GetData(), false, true);
            b.load();
            b.modData[cropKey] = crop.Id;
            crop.modData[builtAtKey] = $"{crop.Tile.X},{crop.Tile.Y}";
            crop.Location.buildings.Add(b);
        }

        private void CheckBuilding(Building b)
        {
            if (b.GetParentLocation() is not GameLocation l)
                return;
            if (!b.modData.TryGetValue(cropKey, out var cropType))
                return;
            if (!Config.ModEnabled)
            {
                l.buildings.Remove(b);
                return;
            }
            if (b.GetParentLocation().resourceClumps.FirstOrDefault(rc => rc is GiantCrop g && g.Id == cropType && g.Tile.X == b.tileX.Value && g.Tile.Y == b.tileY.Value) == null)
            {
                ToRemove.Add(b);
                SHelper.Events.GameLoop.UpdateTicked += GameLoop_UpdateTicked;
            }
        }
        public static void RemoveAllBuildings()
        {
            if (!Context.IsWorldReady)
                return;
            Utility.ForEachLocation((GameLocation l) =>
            {
                for(int i = l.buildings.Count - 1; i >= 0; i--)
                {
                    if (l.buildings[i]?.modData.ContainsKey(cropKey) == true)
                    {
                        l.buildings.RemoveAt(i);
                    }
                }
                return true;
            });
        }
    }
}