using Microsoft.Xna.Framework;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;

namespace UtilityGrid
{
    public interface IUtilityGridApi
    {
        public void AddPowerFunction(Func<string, int, List<Vector2>, Vector2> function);
        public void AddHideGridAction(EventHandler<KeyValuePair<GameLocation, int>> handler);
        public void AddRefreshAction(EventHandler<KeyValuePair<GameLocation, int>> handler);
        public void AddShowGridAction(EventHandler<KeyValuePair<GameLocation, int>> handler);
        public bool IsObjectPowered(GameLocation location, int x, int y);
        public bool IsObjectWorking(GameLocation location, int x, int y);
        public List<List<Vector2>> LocationElectricObjects(GameLocation location);
        public List<List<Vector2>> LocationElectricPipes(GameLocation location);
        public List<List<Vector2>> LocationWaterObjects(GameLocation location);
        public List<List<Vector2>> LocationWaterPipes(GameLocation location);
        public Vector2 ObjectWaterElectricityVector(GameLocation location, int x, int y);
        public void RefreshElectricGrid(GameLocation location);
        public void RefreshWaterGrid(GameLocation location);
        public bool SetObjectElectricity(GameLocation location, int x, int y, int amount);
        public bool SetObjectWater(GameLocation location, int x, int y, int amount);
        public bool ShowingElectrictyGrid();
        public bool ShowingWaterGrid();
        public List<Vector2> TileGroupElectricityObjects(GameLocation location, int x, int y);
        public Vector2 TileGroupElectricityVector(GameLocation location, int x, int y);
        public List<Vector2> TileGroupWaterObjects(GameLocation location, int x, int y);
        public Vector2 TileGroupWaterVector(GameLocation location, int x, int y);
        public List<Vector2> TileElectricGrid(GameLocation location, int x, int y);
        public List<Vector2> TileWaterGrid(GameLocation location, int x, int y);
    }

    public class UtilityGridApi
    {
        public Vector2 TileGroupElectricityVector(GameLocation location, int x, int y)
        {
            return ModEntry.GetTileElectricPower(location.Name, new Vector2(x, y));
        }
        public Vector2 TileGroupWaterVector(GameLocation location, int x, int y)
        {
            return ModEntry.GetTileWaterPower(location.Name, new Vector2(x, y));
        }
        public List<Vector2> TileGroupElectricityObjects(GameLocation location, int x, int y)
        {
            if (!ModEntry.utilitySystemDict.ContainsKey(location.Name))
                return new List<Vector2>();
            Vector2 tile = new Vector2(x, y);
            foreach (var group in ModEntry.utilitySystemDict[location.Name].electricGroups)
            {
                if (group.pipes.Contains(tile))
                    return group.objects.Keys.ToList();
            }
            return new List<Vector2>();
        }
        public List<Vector2> TileGroupWaterObjects(GameLocation location, int x, int y)
        {
            if (!ModEntry.utilitySystemDict.ContainsKey(location.Name))
                return new List<Vector2>();
            Vector2 tile = new Vector2(x, y);
            foreach (var group in ModEntry.utilitySystemDict[location.Name].waterGroups)
            {
                if (group.pipes.Contains(tile))
                    return group.objects.Keys.ToList();
            }
            return new List<Vector2>();
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
        public List<List<Vector2>> LocationElectricObjects(GameLocation location)
        {
            if (!ModEntry.utilitySystemDict.ContainsKey(location.Name))
                return new List<List<Vector2>>();
            List<List<Vector2>> list = new List<List<Vector2>>();
            foreach(var pipeGroup in ModEntry.utilitySystemDict[location.Name].electricGroups)
            {
                list.Add(pipeGroup.objects.Keys.ToList());
            }
            return list;
        }
        public List<List<Vector2>> LocationWaterObjects(GameLocation location)
        {
            if (!ModEntry.utilitySystemDict.ContainsKey(location.Name))
                return new List<List<Vector2>>();
            List<List<Vector2>> list = new List<List<Vector2>>();
            foreach(var pipeGroup in ModEntry.utilitySystemDict[location.Name].waterGroups)
            {
                list.Add(pipeGroup.objects.Keys.ToList());
            }
            return list;
        }
        public Vector2 ObjectWaterElectricityVector(GameLocation location, int x, int y)
        {
            if (!ModEntry.utilitySystemDict.ContainsKey(location.Name))
                return Vector2.Zero;
            var obj = ModEntry.GetUtilityObjectAtTile(location, new Vector2(x, y));
            if (obj != null)
                return new Vector2(obj.Template.water, obj.Template.electric);
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

            return ModEntry.IsObjectPowered(location.Name, tile, obj.Template);
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
        public void AddRefreshAction(EventHandler<KeyValuePair<GameLocation, int>> handler)
        {
            ModEntry.refreshEventHandler += handler;
        }
        public void AddShowGridAction(EventHandler<KeyValuePair<GameLocation, int>> handler)
        {
            ModEntry.showEventHandler += handler;
        }
        public void AddHideGridAction(EventHandler<KeyValuePair<GameLocation, int>> handler)
        {
            ModEntry.hideEventHandler += handler;
        }
        public bool ShowingWaterGrid()
        {
            return ModEntry.ShowingGrid && ModEntry.CurrentGrid == ModEntry.GridType.water;
        }
        public bool ShowingElectrictyGrid()
        {
            return ModEntry.ShowingGrid && ModEntry.CurrentGrid == ModEntry.GridType.electric;
        }
        public bool SetObjectWater(GameLocation location, int x, int y, float amount)
        {
            if (!ModEntry.utilitySystemDict.ContainsKey(location.Name))
                return false;
            var v = new Vector2(x, y);
            foreach (var group in ModEntry.utilitySystemDict[location.Name].waterGroups)
            {
                if (group.objects.ContainsKey(v))
                {
                    float current = group.objects[v].Template.water;
                    float change = amount - current;
                    Vector2 power = TileGroupWaterVector(location, x, y);
                    if (change < 0 && power.X + power.Y - change < 0)
                        return false;
                    group.objects[v].Template.water = amount;
                    return true;
                }
            }
            return false;
        }
        public bool SetObjectElectricity(GameLocation location, int x, int y, int amount)
        {
            if (!ModEntry.utilitySystemDict.ContainsKey(location.Name))
                return false;
            var v = new Vector2(x, y);
            foreach (var group in ModEntry.utilitySystemDict[location.Name].electricGroups)
            {
                if (group.pipes.Contains(v))
                {
                    float current = group.objects[v].Template.electric;
                    float change = amount - current;
                    Vector2 power = TileGroupElectricityVector(location, x, y);
                    if (change < 0 && power.X + power.Y - change < 0)
                        return false;
                    group.objects[v].Template.electric = amount;
                    return true;
                }
            }
            return false;
        }
    }
}