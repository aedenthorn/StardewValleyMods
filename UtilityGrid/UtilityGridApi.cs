using Microsoft.Xna.Framework;
using StardewValley;
using System;
using System.Collections.Generic;

namespace UtilityGrid
{
    public interface IUtilityGridApi
    {
        public void AddPowerFunction(Func<string, int, List<Vector2>, Vector2> function);
        public bool IsObjectPowered(GameLocation location, int x, int y);
        public bool IsObjectWorking(GameLocation location, int x, int y);
        public List<List<Vector2>> LocationElectricPipes(GameLocation location);
        public List<List<Vector2>> LocationWaterPipes(GameLocation location);
        public Vector2 ObjectWaterElectricityVector(GameLocation location, int x, int y);
        public void RefreshElectricGrid(GameLocation location);
        public void RefreshWaterGrid(GameLocation location);
        public Vector2 TileGroupElectricityVector(string location, int x, int y);
        public Vector2 TileGroupWaterVector(string location, int x, int y);
        public List<Vector2> TileElectricGrid(GameLocation location, int x, int y);
        public List<Vector2> TileWaterGrid(GameLocation location, int x, int y);
    }

    public class UtilityGridApi
    {
        public Vector2 TileGroupElectricityVector(string location, int x, int y)
        {
            return ModEntry.GetTileElectricPower(location, new Vector2(x, y));
        }
        public Vector2 TileGroupWaterVector(string location, int x, int y)
        {
            return ModEntry.GetTileWaterPower(location, new Vector2(x, y));
        }
        public List<List<Vector2>> LocationWaterPipes(GameLocation location)
        {
            if (!ModEntry.utilitySystemDict.ContainsKey(location.Name))
                return new List<List<Vector2>>();
            List<List<Vector2>> list = new List<List<Vector2>>();
            foreach(var group in ModEntry.utilitySystemDict[location.Name].waterGroups)
            {
                list.Add(group.pipes);
            }
            return list;
        }
        public List<List<Vector2>> LocationElectricPipes(GameLocation location)
        {
            if (!ModEntry.utilitySystemDict.ContainsKey(location.Name))
                return new List<List<Vector2>>();
            List<List<Vector2>> list = new List<List<Vector2>>();
            foreach(var group in ModEntry.utilitySystemDict[location.Name].electricGroups)
            {
                list.Add(group.pipes);
            }
            return list;
        }
        public Vector2 ObjectWaterElectricityVector(GameLocation location, int x, int y)
        {
            if (!ModEntry.utilitySystemDict.ContainsKey(location.Name))
                return Vector2.Zero;
            var obj = ModEntry.GetUtilityObjectAtTile(location, new Vector2(x, y));
            if (obj != null)
                return new Vector2(obj.water, obj.electric);
            return Vector2.Zero;
        }
        public bool IsObjectPowered(GameLocation location, int x, int y)
        {
            if (!ModEntry.utilitySystemDict.ContainsKey(location.Name))
                return false;
            var tile = new Vector2(x, y);
            var obj = ModEntry.GetUtilityObjectAtTile(location, tile);
            if (obj == null)
                return false;

            return ModEntry.IsObjectPowered(location.Name, tile, obj);
        }
        public bool IsObjectWorking(GameLocation location, int x, int y)
        {
            if (!ModEntry.utilitySystemDict.ContainsKey(location.Name))
                return false;
            var tile = new Vector2(x, y);
            var obj = ModEntry.GetUtilityObjectAtTile(location, tile);
            if (obj == null)
                return false;

            return ModEntry.IsObjectWorking(location, obj);
        }
        public List<Vector2> TileWaterGrid(GameLocation location, int x, int y)
        {
            if (!ModEntry.utilitySystemDict.ContainsKey(location.Name))
                return null;
            var v = new Vector2(x, y);
            foreach (var group in ModEntry.utilitySystemDict[location.Name].waterGroups)
            {
                if (group.pipes.Contains(v))
                    return group.pipes;
            }
            return null;
        }
        public List<Vector2> TileElectricGrid(GameLocation location, int x, int y)
        {
            if (!ModEntry.utilitySystemDict.ContainsKey(location.Name))
                return null;
            var v = new Vector2(x, y);
            foreach (var group in ModEntry.utilitySystemDict[location.Name].electricGroups)
            {
                if (group.pipes.Contains(v))
                    return group.pipes;
            }
            return null;
        }
        public void RefreshElectricGrid(GameLocation location)
        {
            ModEntry.RemakeGroups(location.Name, ModEntry.GridType.electric);
        }
        public void RefreshWaterGrid(GameLocation location)
        {
            ModEntry.RemakeGroups(location.Name, ModEntry.GridType.water);
        }
        public void AddPowerFunction(Func<string, int, List<Vector2>, Vector2> function)
        {
            ModEntry.powerFuctionList.Add(function);
        }
    }
}