using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Object = StardewValley.Object;

namespace UtilityGrid
{
    public partial class ModEntry
    {

        public static void DrawTile(SpriteBatch b, Vector2 tile, GridPipe which, Color color)
        {
            float layerDepth = (tile.Y * (16 * Game1.pixelZoom) + 16 * Game1.pixelZoom) / 10000f;

            b.Draw(pipeTexture, Game1.GlobalToLocal(Game1.viewport, tile * 64), new Rectangle(which.rotation * 64, which.index * 64, 64, 64), color, 0, Vector2.Zero, 1, SpriteEffects.None, layerDepth);
        }
        private void DrawAmount(SpriteBatch b, KeyValuePair<Vector2, UtilityObject> kvp, float netPower, Color color)
        {
            float objPower;
            bool enough = netPower >= 0;
            if (CurrentGrid == GridType.electric)
            {
                objPower = kvp.Value.electric;
            }
            else
            {
                objPower = kvp.Value.water;
                if (!IsObjectPowered(Game1.player.currentLocation.Name, kvp.Key, kvp.Value))
                {
                    enough = false;
                }
            }

            if (objPower == 0)
                return;

            if (enough && !IsObjectWorking(Game1.getLocationFromName(Game1.player.currentLocation.Name), kvp.Value))
                enough = false;

            string str = "" + Math.Round(objPower);
            Vector2 pos = Game1.GlobalToLocal(Game1.viewport, kvp.Key * 64);
            b.DrawString(Game1.dialogueFont, str, pos + new Vector2(-2, 2), Config.ShadowColor, 0, Vector2.Zero, 1f, SpriteEffects.None, 0.999999f);
            b.DrawString(Game1.dialogueFont, str, pos + new Vector2(2, -2), Config.ShadowColor, 0, Vector2.Zero, 1f, SpriteEffects.None, 0.999999f);
            b.DrawString(Game1.dialogueFont, str, pos + new Vector2(-2, -2), Config.ShadowColor, 0, Vector2.Zero, 1f, SpriteEffects.None, 0.999999f);
            b.DrawString(Game1.dialogueFont, str, pos + new Vector2(2, 2), Config.ShadowColor, 0, Vector2.Zero, 1f, SpriteEffects.None, 0.999999f);
            b.DrawString(Game1.dialogueFont, str, pos, enough ? color : Config.InsufficientColor, 0, Vector2.Zero, 1f, SpriteEffects.None, 0.9999999f);
        }
        private void DrawCharge(SpriteBatch b, KeyValuePair<Vector2, UtilityObject> kvp, Color color)
        {
            float charge = 0;
            int capacity;
            if (CurrentGrid == GridType.electric)
            {
                if (kvp.Value.electricChargeCapacity <= 0)
                    return;
                capacity = kvp.Value.electricChargeCapacity;
                if (kvp.Value.worldObj.modData.ContainsKey("aedenthorn.UtilityGrid/electricCharge"))
                    float.TryParse(kvp.Value.worldObj.modData["aedenthorn.UtilityGrid/electricCharge"], NumberStyles.Float, CultureInfo.InvariantCulture, out charge);
            }
            else
            {
                if (kvp.Value.waterChargeCapacity <= 0)
                    return;
                if (kvp.Value.worldObj.modData.ContainsKey("aedenthorn.UtilityGrid/waterCharge"))
                    float.TryParse(kvp.Value.worldObj.modData["aedenthorn.UtilityGrid/waterCharge"], NumberStyles.Float, CultureInfo.InvariantCulture, out charge);

                capacity = kvp.Value.waterChargeCapacity;
            }


            string str = Math.Round(charge, 2) + "\n" + capacity;
            Vector2 pos = Game1.GlobalToLocal(Game1.viewport, kvp.Key * 64) + new Vector2(16, -16);
            b.DrawString(Game1.dialogueFont, str, pos + new Vector2(-1, 1), Config.ShadowColor, 0, Vector2.Zero, 0.5f, SpriteEffects.None, 0.999999f);
            b.DrawString(Game1.dialogueFont, str, pos + new Vector2(1, -1), Config.ShadowColor, 0, Vector2.Zero, 0.5f, SpriteEffects.None, 0.999999f);
            b.DrawString(Game1.dialogueFont, str, pos + new Vector2(-1, -1), Config.ShadowColor, 0, Vector2.Zero, 0.5f, SpriteEffects.None, 0.999999f);
            b.DrawString(Game1.dialogueFont, str, pos + new Vector2(1, 1), Config.ShadowColor, 0, Vector2.Zero, 0.5f, SpriteEffects.None, 0.999999f);
            b.DrawString(Game1.dialogueFont, str, pos, color, 0, Vector2.Zero, 0.5f, SpriteEffects.None, 0.9999999f);
        }
        public bool PayForPipe(bool destroying)
        {
            int lessGold = 0;
            Dictionary<int, int> lessItems = new Dictionary<int, int>();
            if (destroying)
            {
                lessGold = Config.PipeDestroyGold;
                if (Config.PipeDestroyItems?.Length > 0)
                {
                    var itemStrings = Config.PipeDestroyItems.Split(',');
                    foreach (var item in itemStrings)
                    {
                        var parts = item.Split(':');
                        if (int.TryParse(parts[0], out int index) && int.TryParse(parts[1], out int amount))
                        {
                            lessItems[index] = amount;
                        }
                    }
                }
            }
            if (Config.PipeCostGold > 0)
            {
                if(Game1.player.Money + lessGold < Config.PipeCostGold)
                {
                    Game1.showRedMessage(Helper.Translation.Get("not-enough-money"));
                    return false;
                }
            }
            List<int[]> items = new List<int[]>();
            if (Config.PipeCostItems?.Length > 0)
            {
                var itemStrings = Config.PipeCostItems.Split(',');
                foreach(var item in itemStrings)
                {
                    var parts = item.Split(':');
                    if(int.TryParse(parts[0], out int index) && int.TryParse(parts[1], out int amount))
                    {
                        if (Game1.player.getItemCount(index) + (lessItems.ContainsKey(index) ? lessItems[index] : 0) < amount)
                        {
                            Game1.showRedMessage(Helper.Translation.Get("not-enough-materials"));
                            return false;
                        }
                        items.Add(new int[] { index, amount });
                    }
                }
            }

            if (destroying)
                PipeDestroyed();
            Game1.player.Money -= Config.PipeCostGold;
            foreach (var item in items)
            {
                Game1.player.removeItemsFromInventory(item[0], item[1]);
            }
            return true;
        }
        public void PipeDestroyed()
        {
            if (Config.PipeDestroyGold > 0)
            {
                Game1.player.Money += Config.PipeDestroyGold;
            }
            if (Config.PipeDestroyItems?.Length > 0)
            {
                var itemStrings = Config.PipeDestroyItems.Split(',');
                foreach (var item in itemStrings)
                {
                    var parts = item.Split(':');
                    if (int.TryParse(parts[0], out int index) && int.TryParse(parts[1], out int amount))
                    {
                        Object obj = new Object(index, amount);
                        if (obj == null)
                            continue;
                        if (Utility.canItemBeAddedToThisInventoryList(obj, Game1.player.Items))
                            Game1.player.addItemToInventory(obj);
                        else
                            Game1.createObjectDebris(index, Game1.player.getTileX(), Game1.player.getTileY(), Game1.player.currentLocation);
                    }
                }
            }
        }
        public static bool PipesAreJoined(string location, Vector2 tile, Vector2 tile2, GridType gridType)
        {
            if (!utilitySystemDict.ContainsKey(location))
            {
                SMonitor.Log($"{location} has no utility grid");
                return false;
            }

            Dictionary<Vector2, GridPipe> pipeDict;
            if (gridType == GridType.electric)
            {
                pipeDict = utilitySystemDict[location].electricPipes;
            }
            else
            {
                pipeDict = utilitySystemDict[location].waterPipes;
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
        public static void RemakeAllGroups(string location)
        {
            if (!utilitySystemDict.ContainsKey(location))
            {
                utilitySystemDict[location] = new UtilitySystem();
            }
            RemakeGroups(location, GridType.water);
            RemakeGroups(location, GridType.electric);
        }
        public static void RemakeGroups(string location, GridType gridType)
        {
            if (!utilitySystemDict.ContainsKey(location))
            {
                utilitySystemDict[location] = new UtilitySystem();
            }

            Dictionary<Vector2, GridPipe> pipeDict;
            List<PipeGroup> groupList;
            if (gridType == GridType.electric)
            {
                pipeDict = new Dictionary<Vector2, GridPipe>(utilitySystemDict[location].electricPipes);
                groupList = utilitySystemDict[location].electricGroups;
            }
            else
            {
                pipeDict = new Dictionary<Vector2, GridPipe>(utilitySystemDict[location].waterPipes);
                groupList = utilitySystemDict[location].waterGroups;
            }
            groupList.Clear();

            while(pipeDict.Count > 0)
            {
                var tile = pipeDict.Keys.ToArray()[0];
                var group = new PipeGroup { pipes = new List<Vector2>() { tile } };
                //SMonitor.Log($"Creating new group; power: {group.input}");
                pipeDict.Remove(tile);
                AddTilesToGroup(location, tile, ref group, pipeDict, gridType);
                groupList.Add(group);
            }

            AddObjectsToGrid(location, gridType);

            EventHandler<KeyValuePair<GameLocation, int>> handler = refreshEventHandler;
            if (handler != null)
            {
                KeyValuePair<GameLocation, int> e = new KeyValuePair<GameLocation, int>(Game1.getLocationFromName(location), (int)gridType);
                handler(context, e);
            }
        }

        private static void AddObjectsToGrid(string location, GridType gridType)
        {
            if (!utilitySystemDict.ContainsKey(location))
            {
                return;
            }
            List<PipeGroup> groupList;
            Dictionary<Vector2, UtilityObject> objectDict;
            if (gridType == GridType.electric)
            {
                groupList = utilitySystemDict[location].electricGroups;
                objectDict = utilitySystemDict[location].electricUnconnectedObjects;
            }
            else
            {
                groupList = utilitySystemDict[location].waterGroups;
                objectDict = utilitySystemDict[location].waterUnconnectedObjects;
            }
            objectDict.Clear();
            GameLocation gl = Game1.getLocationFromName(location);
            if (gl == null)
                return;
            foreach (var kvp in gl.Objects.Pairs)
            {
                var obj = GetUtilityObjectAtTile(gl, kvp.Key);
                if (obj == null)
                    continue;
                if (gridType == GridType.water && obj.water == 0 && obj.waterChargeCapacity <= 0)
                    continue;
                if (gridType == GridType.electric && obj.electric == 0 && obj.electricChargeCapacity <= 0)
                    continue;
                bool found = false;
                foreach (var group in groupList)
                {
                    if (group.pipes.Contains(kvp.Key))
                    {
                        group.objects[kvp.Key] = obj;
                        found = true;
                        break;
                    }
                }
                if (!found)
                    objectDict[kvp.Key] = obj;
            }
        }
        public static void AddTilesToGroup(string location, Vector2 tile, ref PipeGroup group, Dictionary<Vector2, GridPipe> pipeDict, GridType gridType)
        {
            Vector2[] adjecents = new Vector2[] { tile + new Vector2(0,1),tile + new Vector2(1,0),tile + new Vector2(-1,0),tile + new Vector2(0,-1)};

            foreach(var a in adjecents)
            {
                if (group.pipes.Contains(a) || !pipeDict.ContainsKey(a))
                    continue;
                if (PipesAreJoined(location, tile, a, gridType))
                {
                    group.pipes.Add(a);
                    pipeDict.Remove(a);
                    //SMonitor.Log($"Adding pipe to group; {group.pipes.Count} pipes in group; total power: {group.input}");
                    AddTilesToGroup(location, a, ref group, pipeDict, gridType);
                }
            }
        }
        public Vector2 GetGroupPower(string location, PipeGroup group, GridType gridType)
        {
            if (gridType == GridType.water)
                return GetGroupWaterPower(location, group);

            return GetGroupElectricPower(location, group);
        }
        public static float GetTileNetElectricPower(string location, Vector2 tile)
        {
            Vector2 power = GetTileElectricPower(location, tile);
            return power.X + power.Y;
        }
        public static float GetTileNetWaterPower(string location, Vector2 tile)
        {
            Vector2 power = GetTileWaterPower(location, tile);
            return power.X + power.Y;
        }
        public static Vector2 GetTileElectricPower(string location, Vector2 tile)
        {
            Vector2 power = Vector2.Zero;
            foreach (var group in utilitySystemDict[location].electricGroups)
            {
                if (group.objects.ContainsKey(tile))
                {

                    return GetGroupElectricPower(location, group);
                }
            }
            return power;
        }
        public static Vector2 GetTileWaterPower(string location, Vector2 tile)
        {
            Vector2 power = Vector2.Zero;
            foreach (var group in utilitySystemDict[location].waterGroups)
            {
                if (group.objects.ContainsKey(tile))
                {
                    return GetGroupWaterPower(location, group);
                }
            }
            return power;
        }
        public static Vector2 GetGroupElectricPower(string location, PipeGroup group, bool tick = false)
        {
            Vector2 power = Vector2.Zero;
            foreach (var obj in group.objects.Values)
            {
                power += GetPowerVector(location, obj, obj.electric);
            }
            foreach (var func in powerFuctionList)
            {
                power += func(location, (int)GridType.electric, group.pipes);
            }
            Dictionary<Vector2, float> objectsToDischarge = new Dictionary<Vector2, float>();
            foreach (var obj in group.objects.Values)
            {
                var netPower = power.X + power.Y;
                if (netPower == 0)
                    break;

                if (obj.electricChargeCapacity == 0)
                    continue;
                float charge = 0;
                if (obj.worldObj.modData.ContainsKey("aedenthorn.UtilityGrid/electricCharge"))
                    float.TryParse(obj.worldObj.modData["aedenthorn.UtilityGrid/electricCharge"], NumberStyles.Float, CultureInfo.InvariantCulture, out charge);
                if (netPower > 0)
                {
                    float add = Math.Min(obj.electricChargeCapacity - charge, Math.Min(obj.electricChargeRate, netPower));
                    if (tick)
                        obj.worldObj.modData["aedenthorn.UtilityGrid/electricCharge"] = Math.Min(obj.electricChargeCapacity, charge + add).ToString();
                    power.X -= add;
                }
                else
                {
                    float add = Math.Min(charge, obj.electricDischargeRate <= 0 ? -netPower : Math.Min(-netPower, obj.electricDischargeRate));
                    if (tick)
                    {
                        objectsToDischarge[obj.worldObj.TileLocation] = Math.Max(0, charge - add);
                    }

                    power.X += add;
                }
            }
            if (objectsToDischarge.Count > 0 && power.X + power.Y >= 0)
            {
                GameLocation gl = Game1.getLocationFromName(location);
                if(gl == null)
                {
                    SMonitor.Log($"Invalid game location {location}", StardewModdingAPI.LogLevel.Error);
                    return power;
                }

                foreach (var kvp in objectsToDischarge)
                {
                    if (gl.objects.ContainsKey(kvp.Key))
                    {
                        gl.objects[kvp.Key].modData["aedenthorn.UtilityGrid/electricCharge"] = kvp.Value.ToString();
                    }
                }
            }

            return power;
        }
        public static Vector2 GetGroupWaterPower(string location, PipeGroup group, bool tick = false)
        {
            Vector2 power = Vector2.Zero;
            foreach (var kvp in group.objects)
            {
                if (kvp.Value.electric < 0)
                {
                    Vector2 ePower = GetTileElectricPower(location, kvp.Key);
                    if (ePower.X == 0 || ePower.X + ePower.Y < 0) // unpowered
                        continue;
                }
                var obj = kvp.Value;
                power += GetPowerVector(location, obj, obj.water);
            }
            foreach (var func in powerFuctionList)
            {
                power += func(location, (int)GridType.water, group.pipes);
            }
            Dictionary<Vector2, float> objectsToDischarge = new Dictionary<Vector2, float>();
            foreach (var obj in group.objects.Values)
            {
                var netPower = power.X + power.Y;
                if (netPower == 0)
                    break;

                if (obj.waterChargeCapacity == 0)
                    continue;

                float charge = 0;
                if (obj.worldObj.modData.ContainsKey("aedenthorn.UtilityGrid/waterCharge"))
                    float.TryParse(obj.worldObj.modData["aedenthorn.UtilityGrid/waterCharge"], NumberStyles.Float, CultureInfo.InvariantCulture, out charge);

                if (tick && obj.fillWaterFromRain)
                {
                    charge = Math.Min(charge + obj.waterChargeCapacity, obj.waterChargeCapacity);
                }

                if (netPower > 0)
                {
                    float add = Math.Min(obj.waterChargeCapacity - charge, Math.Min(obj.waterChargeRate, netPower));
                    if (tick)
                        obj.worldObj.modData["aedenthorn.UtilityGrid/waterCharge"] = Math.Min(obj.waterChargeCapacity, charge + add).ToString();
                    power.X -= add;
                }
                else
                {
                    float add = Math.Min(charge, obj.waterDischargeRate <= 0 ? -netPower : Math.Min(-netPower, obj.waterDischargeRate));
                    if (tick)
                    {
                        objectsToDischarge[obj.worldObj.TileLocation] = Math.Max(0, charge - add);
                    }
                        
                    power.X += add;
                }
            }
            if (objectsToDischarge.Count > 0 && power.X + power.Y >= 0)
            {
                GameLocation gl = Game1.getLocationFromName(location);
                if (gl == null)
                {
                    SMonitor.Log($"Invalid game location {location}", StardewModdingAPI.LogLevel.Error);
                    return power;
                }

                foreach (var kvp in objectsToDischarge)
                {
                    if (gl.objects.ContainsKey(kvp.Key))
                    {
                        gl.objects[kvp.Key].modData["aedenthorn.UtilityGrid/waterCharge"] = kvp.Value.ToString();
                    }
                }
            }
            return power;
        }
        public static Vector2 GetPowerVector(string location, UtilityObject obj, float amount)
        {
            var power = Vector2.Zero;
            if (amount == 0 || !IsObjectWorking(Game1.getLocationFromName(location), obj))
                return power;
            if (amount > 0)
                power.X += amount;
            else if (amount < 0)
                power.Y += amount;
            return power;
        }

        public static bool IsObjectWorking(GameLocation location, UtilityObject obj)
        {
            return (!obj.mustBeOn || obj.worldObj.IsOn) &&
            (!obj.onlyDay || Game1.timeOfDay < 1800) &&
            (!obj.onlyNight || Game1.timeOfDay >= 1800) &&
            (!obj.worldObj.IsSprinkler() || Game1.timeOfDay == 600) &&
            (!obj.onlyMorning || Game1.timeOfDay == 600) &&
            (!obj.mustBeFull || obj.worldObj.heldObject.Value != null) &&
            (!obj.mustBeWorking || obj.worldObj.MinutesUntilReady > 0) &&
            (obj.mustContain == null || obj.mustContain.Length == 0 || obj.worldObj.heldObject.Value?.Name == obj.mustContain) &&
            (!obj.mustHaveSun || ((location.IsOutdoors || location.IsGreenhouse) && !Game1.netWorldState.Value.GetWeatherForLocation(location.GetLocationContext()).isRaining.Value)) &&
            (!obj.mustHaveRain || (location.IsOutdoors && Game1.netWorldState.Value.GetWeatherForLocation(location.GetLocationContext()).isRaining.Value)) &&
            (!obj.mustHaveLightning || (location.IsOutdoors && !Game1.netWorldState.Value.GetWeatherForLocation(location.GetLocationContext()).isLightning.Value));
        }

        public static bool IsObjectPowered(string location, Vector2 tile, UtilityObject obj)
        {
            Vector2 waterPower = GetTileWaterPower(location, tile);
            Vector2 electricPower = GetTileElectricPower(location, tile);
            if(obj.water < 0 && (waterPower == Vector2.Zero || waterPower.X + waterPower.Y < 0))
            {
                    return false;
            }
            if (obj.electric < 0 && (electricPower == Vector2.Zero || electricPower.X + electricPower.Y < 0))
            {
                return false;
            }
            return true;
        }

        public static bool ObjectNeedsPower(UtilityObject obj)
        {
            return obj.water < 0 || obj.electric < 0;
        }

        public static UtilityObject GetUtilityObjectAtTile(GameLocation location, Vector2 tile)
        {
            if (location == null || !location.Objects.ContainsKey(tile))
                return null;
            var obj = location.Objects[tile];
            if (!utilityObjectDict.ContainsKey(obj.Name) && (obj.modData.ContainsKey("aedenthorn.UtilityGrid/" + GridType.water) || obj.modData.ContainsKey("aedenthorn.UtilityGrid/" + GridType.electric) || obj.modData.ContainsKey("aedenthorn.UtilityGrid/electricChargeCapacity") || obj.modData.ContainsKey("aedenthorn.UtilityGrid/waterChargeCapacity")))
            {
                utilityObjectDict[obj.Name] = new UtilityObject();
                if (obj.modData.ContainsKey("aedenthorn.UtilityGrid/" + GridType.water))
                {
                    utilityObjectDict[obj.Name].water = float.Parse(obj.modData["aedenthorn.UtilityGrid/" + GridType.water], CultureInfo.InvariantCulture);
                }
                if (obj.modData.ContainsKey("aedenthorn.UtilityGrid/" + GridType.electric))
                {
                    utilityObjectDict[obj.Name].electric = float.Parse(obj.modData["aedenthorn.UtilityGrid/" + GridType.electric], CultureInfo.InvariantCulture);
                }
                if (obj.modData.ContainsKey("aedenthorn.UtilityGrid/mustBeOn"))
                {
                    utilityObjectDict[obj.Name].mustBeOn = bool.Parse(obj.modData["aedenthorn.UtilityGrid/mustBeOn"]);
                }
                if (obj.modData.ContainsKey("aedenthorn.UtilityGrid/mustBeWorking"))
                {
                    utilityObjectDict[obj.Name].mustBeWorking = bool.Parse(obj.modData["aedenthorn.UtilityGrid/mustBeWorking"]);
                }
                if (obj.modData.ContainsKey("aedenthorn.UtilityGrid/mustBeFull"))
                {
                    utilityObjectDict[obj.Name].mustBeFull = bool.Parse(obj.modData["aedenthorn.UtilityGrid/mustBeFull"]);
                }
                if (obj.modData.ContainsKey("aedenthorn.UtilityGrid/onlyInWater"))
                {
                    utilityObjectDict[obj.Name].onlyInWater = bool.Parse(obj.modData["aedenthorn.UtilityGrid/onlyInWater"]);
                }
                if (obj.modData.ContainsKey("aedenthorn.UtilityGrid/mustContain"))
                {
                    utilityObjectDict[obj.Name].mustContain = obj.modData["aedenthorn.UtilityGrid/mustContain"];
                }
                if (obj.modData.ContainsKey("aedenthorn.UtilityGrid/electricChargeCapacity"))
                {
                    utilityObjectDict[obj.Name].electricChargeCapacity = int.Parse(obj.modData["aedenthorn.UtilityGrid/electricChargeCapacity"]);
                }
                if (obj.modData.ContainsKey("aedenthorn.UtilityGrid/waterChargeCapacity"))
                {
                    utilityObjectDict[obj.Name].waterChargeCapacity = int.Parse(obj.modData["aedenthorn.UtilityGrid/waterChargeCapacity"]);
                }
                if (obj.modData.ContainsKey("aedenthorn.UtilityGrid/electricChargeRate"))
                {
                    utilityObjectDict[obj.Name].electricChargeRate = int.Parse(obj.modData["aedenthorn.UtilityGrid/electricChargeRate"]);
                }
                if (obj.modData.ContainsKey("aedenthorn.UtilityGrid/waterChargeRate"))
                {
                    utilityObjectDict[obj.Name].waterChargeRate = int.Parse(obj.modData["aedenthorn.UtilityGrid/waterChargeRate"]);
                }
                if (obj.modData.ContainsKey("aedenthorn.UtilityGrid/electricDischargeRate"))
                {
                    utilityObjectDict[obj.Name].electricDischargeRate = int.Parse(obj.modData["aedenthorn.UtilityGrid/electricDischargeRate"]);
                }
                if (obj.modData.ContainsKey("aedenthorn.UtilityGrid/waterDischargeRate"))
                {
                    utilityObjectDict[obj.Name].waterDischargeRate = int.Parse(obj.modData["aedenthorn.UtilityGrid/waterDischargeRate"]);
                }
                if (obj.modData.ContainsKey("aedenthorn.UtilityGrid/fillWaterFromRain"))
                {
                    utilityObjectDict[obj.Name].fillWaterFromRain = bool.Parse(obj.modData["aedenthorn.UtilityGrid/fillWaterFromRain"]);
                }
            }
            if (utilityObjectDict.ContainsKey(obj.Name))
            {
                UtilityObject outObj = utilityObjectDict[obj.Name];
                outObj.worldObj = obj;
                return outObj;
            }
            return null;
        }
    }
}