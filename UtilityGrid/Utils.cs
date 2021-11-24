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

        private void DrawTile(SpriteBatch b, Vector2 tile, Point which, bool electric, Color color)
        {
            float layerDepth = (tile.Y * (16 * Game1.pixelZoom) + 16 * Game1.pixelZoom) / 10000f;

            b.Draw(pipeTexture, Game1.GlobalToLocal(Game1.viewport, tile * 64), new Rectangle(which.Y * 64, which.X * 64, 64, 64), color, 0, Vector2.Zero, 1, SpriteEffects.None, layerDepth);
        }

        private bool PipeIsPowered(Vector2 tile, bool electric)
        {
            List<PipeGroup> groupList;
            if (electric)
            {
                groupList = electricGroups;
            }
            else
            {
                groupList = waterGroups;
            }
            foreach (var g in groupList)
            {
                if (g.pipes.Contains(tile))
                    return g.power > 0;
            }
            return false;
        }
        private bool PipesAreJoined(Vector2 tile, Vector2 tile2, bool electric)
        {
            Dictionary<Vector2, Point> pipeDict;
            if (electric)
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

        private bool HasIntake(Point pipeRot, int which)
        {
            return intakeArray[pipeRot.X][(which + pipeRot.Y) % 4] == 1;
        }
        private void RemakeGroups(bool electric)
        {
            Dictionary<Vector2, Point> pipeDict;
            List<PipeGroup> groupList;
            if (electric)
            {
                pipeDict = new Dictionary<Vector2, Point>(electricPipes);
                groupList = electricGroups;
            }
            else
            {
                pipeDict = new Dictionary<Vector2, Point>(waterPipes);
                groupList = waterGroups;
            }
            groupList.Clear();

            while(pipeDict.Count > 0)
            {
                var tile = pipeDict.Keys.ToArray()[0];
                var group = new PipeGroup { pipes = new List<Vector2>() { tile }, power = PipePower(tile, electric) };
                Monitor.Log($"Creating new group; power: {group.power}");
                pipeDict.Remove(tile);
                AddTilesToGroup(tile, ref group, pipeDict, electric);
                groupList.Add(group);
            }
        }

        private void AddTilesToGroup(Vector2 tile, ref PipeGroup group, Dictionary<Vector2, Point> pipeDict, bool electric)
        {
            Vector2[] adjecents = new Vector2[] { tile + new Vector2(0,1),tile + new Vector2(1,0),tile + new Vector2(-1,0),tile + new Vector2(0,-1)};

            foreach(var a in adjecents)
            {
                if (group.pipes.Contains(a) || !pipeDict.ContainsKey(a))
                    continue;
                if (PipesAreJoined(tile, a, electric))
                {
                    group.pipes.Add(a);
                    group.power += PipePower(a, electric);
                    pipeDict.Remove(a);
                    Monitor.Log($"Adding pipe to group; {group.pipes.Count} pipes in group; total power: {group.power}");
                    AddTilesToGroup(a, ref group, pipeDict, electric);
                }
            }
        }

        private float PipePower(Vector2 key, bool electric)
        {
            if (!Game1.getFarm().Objects.ContainsKey(key))
                return 0;
            var obj = Game1.getFarm().Objects[key];
            if (electric && obj.bigCraftable.Value && obj.ParentSheetIndex == 13)
            {
                obj.modData["aedenthorn.UtilityGrid/type"] = "electric";
                obj.modData["aedenthorn.UtilityGrid/power"] = "1";
            }
            else if (!obj.modData.ContainsKey("aedenthorn.UtilityGrid/type") || (obj.modData["aedenthorn.UtilityGrid/type"] == "electric") != electric)
                return 0;
                
            return float.Parse(obj.modData["aedenthorn.UtilityGrid/power"], CultureInfo.InvariantCulture);
        }
    }
}