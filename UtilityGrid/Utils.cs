using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace UtilityGrid
{
    public partial class ModEntry
    {

        public static void DrawTile(SpriteBatch b, Vector2 tile, GridPipe which, Color color)
        {
            float layerDepth = (tile.Y * (16 * Game1.pixelZoom) + 16 * Game1.pixelZoom) / 10000f;

            b.Draw(pipeTexture, Game1.GlobalToLocal(Game1.viewport, tile * 64), new Rectangle(which.rotation * 64, which.index * 64, 64, 64), color, 0, Vector2.Zero, 1, SpriteEffects.None, layerDepth);
        }

        public static bool PipesAreJoined(Vector2 tile, Vector2 tile2, GridType gridType)
        {
            Dictionary<Vector2, GridPipe> pipeDict;
            if (gridType == GridType.electric)
            {
                pipeDict = electricPipes;
            }
            else
            {
                pipeDict = waterPipes;
            }

            if (!pipeDict.ContainsKey(tile2))
                return false;
            if (tile2.X == tile.X)
            {
                if (tile.Y == tile2.Y + 1)
                    return HasIntake(pipeDict[tile], 0) && HasIntake(pipeDict[tile2], 2);
                else if (tile.Y == tile2.Y - 1)
                    return HasIntake(pipeDict[tile], 2) && HasIntake(pipeDict[tile2], 0);
            }
            else if (tile2.Y == tile.Y)
            {
                if (tile.X == tile2.X + 1)
                    return HasIntake(pipeDict[tile], 3) && HasIntake(pipeDict[tile2], 1);
                else if (tile.X == tile2.X - 1)
                    return HasIntake(pipeDict[tile], 1) && HasIntake(pipeDict[tile2], 3);
            }
            return false;
        }

        public static bool HasIntake(GridPipe pipe, int which)
        {
            return intakeArray[pipe.index][(which + pipe.rotation) % 4] == 1;
        }
        public static void RemakeAllGroups()
        {
            RemakeGroups(GridType.water);
            RemakeGroups(GridType.electric);
        }
        public static void RemakeGroups(GridType gridType)
        {
            Dictionary<Vector2, GridPipe> pipeDict;
            List<PipeGroup> groupList;
            if (gridType == GridType.electric)
            {
                pipeDict = new Dictionary<Vector2, GridPipe>(electricPipes);
                groupList = electricGroups;
            }
            else
            {
                pipeDict = new Dictionary<Vector2, GridPipe>(waterPipes);
                groupList = waterGroups;
            }
            groupList.Clear();

            while(pipeDict.Count > 0)
            {
                var tile = pipeDict.Keys.ToArray()[0];
                var group = new PipeGroup { pipes = new List<Vector2>() { tile } };
                var obj = GetUtilityObjectAtTile(tile);
                if(obj != null)
                {
                    group.objects[tile] = obj;
                }

                //SMonitor.Log($"Creating new group; power: {group.input}");
                pipeDict.Remove(tile);
                AddTilesToGroup(tile, ref group, pipeDict, gridType);
                groupList.Add(group);
            }
        }

        public static void AddTilesToGroup(Vector2 tile, ref PipeGroup group, Dictionary<Vector2, GridPipe> pipeDict, GridType gridType)
        {
            Vector2[] adjecents = new Vector2[] { tile + new Vector2(0,1),tile + new Vector2(1,0),tile + new Vector2(-1,0),tile + new Vector2(0,-1)};

            foreach(var a in adjecents)
            {
                if (group.pipes.Contains(a) || !pipeDict.ContainsKey(a))
                    continue;
                if (PipesAreJoined(tile, a, gridType))
                {
                    group.pipes.Add(a);
                    var obj = GetUtilityObjectAtTile(a);
                    if (obj != null)
                    {
                        group.objects[a] = obj;
                    }
                    pipeDict.Remove(a);
                    //SMonitor.Log($"Adding pipe to group; {group.pipes.Count} pipes in group; total power: {group.input}");
                    AddTilesToGroup(a, ref group, pipeDict, gridType);
                }
            }
        }
        public Vector2 GetGroupPower(PipeGroup group, GridType gridType)
        {
            if (gridType == GridType.water)
                return GetGroupWaterPower(group);

            return GetGroupElectricPower(group);
        }
        public static float GetTileNetElectricPower(Vector2 tile)
        {
            Vector2 power = GetTileElectricPower(tile);
            return power.X + power.Y;
        }
        public static float GetTileNetWaterPower(Vector2 tile)
        {
            Vector2 power = GetTileWaterPower(tile);
            return power.X + power.Y;
        }
        public static Vector2 GetTileElectricPower(Vector2 tile)
        {
            Vector2 power = Vector2.Zero;
            foreach (var group in electricGroups)
            {
                if (group.objects.ContainsKey(tile))
                {

                    return GetGroupElectricPower(group);
                }
            }
            return power;
        }
        public static Vector2 GetGroupElectricPower(PipeGroup group)
        {
            Vector2 power = Vector2.Zero;
            foreach (var obj in group.objects.Values)
            {
                if (obj.mustBeOn && !obj.worldObj.IsOn)
                    continue;
                if (obj.electric > 0)
                    power.X += obj.electric;
                else if (obj.electric < 0)
                    power.Y += obj.electric;
            }
            return power;
        }
        public static Vector2 GetTileWaterPower(Vector2 tile)
        {
            Vector2 power = Vector2.Zero;
            foreach (var group in waterGroups)
            {
                if (group.objects.ContainsKey(tile))
                {
                    return GetGroupWaterPower(group);
                }
            }
            return power;
        }
        public static Vector2 GetGroupWaterPower(PipeGroup group)
        {
            Vector2 power = Vector2.Zero;
            foreach (var kvp in group.objects)
            {
                if (kvp.Value.electric < 0)
                {
                    Vector2 ePower = GetTileElectricPower(kvp.Key);
                    if (ePower.X + ePower.Y < 0) // unpowered
                        continue;
                }
                var obj = kvp.Value;

                if (obj.water > 0)
                    power.X += obj.water;
                else if (obj.water < 0)
                    power.Y += obj.water;
            }
            return power;
        }

        public static bool IsObjectPowered(Vector2 tile, UtilityObject obj)
        {
            if(obj.water < 0)
            {
                var netPower = GetTileNetWaterPower(tile);
                if (netPower < 0)
                    return false;
            }
            if(obj.electric < 0)
            {
                var netPower = GetTileNetElectricPower(tile);
                if (netPower < 0)
                    return false;
            }
            return true;
        }

        public static UtilityObject GetUtilityObjectAtTile(Vector2 tile)
        {
            if (!Game1.getFarm().Objects.ContainsKey(tile))
                return null;
            var obj = Game1.getFarm().Objects[tile];
            if (!objectDict.ContainsKey(obj.Name) && (obj.modData.ContainsKey("aedenthorn.UtilityGrid/" + GridType.water) || obj.modData.ContainsKey("aedenthorn.UtilityGrid/" + GridType.electric)))
            {
                objectDict[obj.Name] = new UtilityObject();
                if (obj.modData.ContainsKey("aedenthorn.UtilityGrid/" + GridType.water))
                {
                    objectDict[obj.Name].water = float.Parse(obj.modData["aedenthorn.UtilityGrid/" + GridType.water], CultureInfo.InvariantCulture);
                }
                if (obj.modData.ContainsKey("aedenthorn.UtilityGrid/" + GridType.electric))
                {
                    objectDict[obj.Name].electric = float.Parse(obj.modData["aedenthorn.UtilityGrid/" + GridType.electric], CultureInfo.InvariantCulture);
                }
            }
            if (objectDict.ContainsKey(obj.Name))
            {
                UtilityObject outObj = objectDict[obj.Name];
                outObj.worldObj = obj;
                return outObj;
            }
            return null;
        }
    }
}