using Microsoft.Xna.Framework;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using static UtilityGrid.ModEntry;

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
            return GetTileElectricPower(location.NameOrUniqueName, new Vector2(x, y));
        }
        public Vector2 TileGroupWaterVector(GameLocation location, int x, int y)
        {
            return GetTileWaterPower(location.NameOrUniqueName, new Vector2(x, y));
        }
        public List<Vector2> TileGroupElectricityObjects(GameLocation location, int x, int y)
        {
            if (!utilitySystemDict.ContainsKey(location.NameOrUniqueName))
                return new List<Vector2>();
            Vector2 tile = new Vector2(x, y);
            foreach (var group in utilitySystemDict[location.NameOrUniqueName][GridType.electric].groups)
            {
                if (group.pipes.Contains(tile))
                    return utilitySystemDict[location.NameOrUniqueName][GridType.electric].objects.Keys.Where(v => group.pipes.Contains(v)).ToList();
            }
            return new List<Vector2>();
        }
        public List<Vector2> TileGroupWaterObjects(GameLocation location, int x, int y)
        {
            if (!utilitySystemDict.ContainsKey(location.NameOrUniqueName))
                return new List<Vector2>();
            Vector2 tile = new Vector2(x, y);
            foreach (var group in utilitySystemDict[location.NameOrUniqueName][GridType.water].groups)
            {
                if (group.pipes.Contains(tile))
                    return utilitySystemDict[location.NameOrUniqueName][GridType.water].objects.Keys.Where(v => group.pipes.Contains(v)).ToList();
            }
            return new List<Vector2>();
        }
        public List<List<Vector2>> LocationWaterPipes(GameLocation location)
        {
            if (!utilitySystemDict.ContainsKey(location.NameOrUniqueName))
                return new List<List<Vector2>>();
            List<List<Vector2>> list = new List<List<Vector2>>();
            foreach(var group in utilitySystemDict[location.NameOrUniqueName][GridType.water].groups)
            {
                list.Add(group.pipes);
            }
            return list;
        }
        public List<List<Vector2>> LocationElectricPipes(GameLocation location)
        {
            if (!utilitySystemDict.ContainsKey(location.NameOrUniqueName))
                return new List<List<Vector2>>();
            List<List<Vector2>> list = new List<List<Vector2>>();
            foreach(var group in utilitySystemDict[location.NameOrUniqueName][GridType.electric].groups)
            {
                list.Add(group.pipes);
            }
            return list;
        }
        public List<List<Vector2>> LocationElectricObjects(GameLocation location)
        {
            if (!utilitySystemDict.ContainsKey(location.NameOrUniqueName))
                return new List<List<Vector2>>();
            List<List<Vector2>> list = new List<List<Vector2>>();
            foreach (var group in utilitySystemDict[location.NameOrUniqueName][GridType.electric].groups)
            {
                list.Add(utilitySystemDict[location.NameOrUniqueName][GridType.electric].objects.Keys.Where(v => group.pipes.Contains(v)).ToList());
            }
            return list;
        }
        public List<List<Vector2>> LocationWaterObjects(GameLocation location)
        {
            if (!utilitySystemDict.ContainsKey(location.NameOrUniqueName))
                return new List<List<Vector2>>();
            List<List<Vector2>> list = new List<List<Vector2>>();
            foreach (var group in utilitySystemDict[location.NameOrUniqueName][GridType.water].groups)
            {
                list.Add(utilitySystemDict[location.NameOrUniqueName][GridType.water].objects.Keys.Where(v => group.pipes.Contains(v)).ToList());
            }
            return list;
        }
        public Vector2 ObjectWaterElectricityVector(GameLocation location, int x, int y)
        {
            if (!utilitySystemDict.ContainsKey(location.NameOrUniqueName))
                return Vector2.Zero;
            var obj = GetUtilityObjectAtTile(location, new Vector2(x, y));
            if (obj != null)
                return new Vector2(obj.Template.water, obj.Template.electric);
            return Vector2.Zero;
        }
        public bool IsObjectPowered(GameLocation location, int x, int y)
        {
            if (!utilitySystemDict.ContainsKey(location.NameOrUniqueName))
                return false;
            var tile = new Vector2(x, y);
            var obj = GetUtilityObjectAtTile(location, tile);
            if (obj == null)
                return false;

            return ModEntry.IsObjectPowered(location.NameOrUniqueName, tile, obj.Template);
        }
        public bool IsObjectWorking(GameLocation location, int x, int y)
        {
            if (!utilitySystemDict.ContainsKey(location.NameOrUniqueName))
                return false;
            var tile = new Vector2(x, y);
            var obj = GetUtilityObjectAtTile(location, tile);
            if (obj == null)
                return false;

            return ModEntry.IsObjectWorking(location, obj);
        }
        public List<Vector2> TileWaterGrid(GameLocation location, int x, int y)
        {
            if (!utilitySystemDict.ContainsKey(location.NameOrUniqueName))
                return null;
            var v = new Vector2(x, y);
            foreach (var group in utilitySystemDict[location.NameOrUniqueName][GridType.water].groups)
            {
                if (group.pipes.Contains(v))
                    return group.pipes;
            }
            return null;
        }
        public List<Vector2> TileElectricGrid(GameLocation location, int x, int y)
        {
            if (!utilitySystemDict.ContainsKey(location.NameOrUniqueName))
                return null;
            var v = new Vector2(x, y);
            foreach (var group in utilitySystemDict[location.NameOrUniqueName][GridType.electric].groups)
            {
                if (group.pipes.Contains(v))
                    return group.pipes;
            }
            return null;
        }
        public void RefreshElectricGrid(GameLocation location)
        {
            RemakeGroups(location.NameOrUniqueName, GridType.electric);
        }
        public void RefreshWaterGrid(GameLocation location)
        {
            RemakeGroups(location.NameOrUniqueName, GridType.water);
        }
        public void AddPowerFunction(Func<string, int, List<Vector2>, Vector2> function)
        {
            powerFuctionList.Add(function);
        }
        public void AddRefreshAction(EventHandler<KeyValuePair<GameLocation, int>> handler)
        {
            refreshEventHandler += handler;
        }
        public void AddShowGridAction(EventHandler<KeyValuePair<GameLocation, int>> handler)
        {
            showEventHandler += handler;
        }
        public void AddHideGridAction(EventHandler<KeyValuePair<GameLocation, int>> handler)
        {
            hideEventHandler += handler;
        }
        public bool ShowingWaterGrid()
        {
            return ShowingGrid && CurrentGrid == GridType.water;
        }
        public bool ShowingElectrictyGrid()
        {
            return ShowingGrid && CurrentGrid == GridType.electric;
        }
        public bool SetObjectWater(GameLocation location, int x, int y, float amount)
        {
            var v = new Vector2(x, y);
            if (!utilitySystemDict.TryGetValue(location.NameOrUniqueName, out var dict) || !dict.TryGetValue(GridType.water, out var system) || !system.objects.TryGetValue(v, out var obj))
                return false;

            obj.Template.water = amount;
            utilitySystemDict[location.NameOrUniqueName][GridType.water].objects[v] = obj;
            return true;
        }
        public bool SetObjectElectricity(GameLocation location, int x, int y, int amount)
        {
            var v = new Vector2(x, y);
            if (!utilitySystemDict.TryGetValue(location.NameOrUniqueName, out var dict) || !dict.TryGetValue(GridType.electric, out var system) || !system.objects.TryGetValue(v, out var obj))
                return false;

            obj.Template.electric = amount;
            utilitySystemDict[location.NameOrUniqueName][GridType.electric].objects[v] = obj;
            return true;
        }
    }
}