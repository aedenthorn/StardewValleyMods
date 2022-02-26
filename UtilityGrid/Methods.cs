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
        private void DrawAmount(SpriteBatch b, KeyValuePair<Vector2, UtilityObjectInstance> kvp, float netPower, Color color)
        {
            float objPower;
            if (CurrentGrid == GridType.electric)
            {
                objPower = kvp.Value.Template.electric;
            }
            else
            {
                objPower = kvp.Value.Template.water;
            }
            if (objPower == 0)
                return;
            bool enough = IsObjectPowered(Game1.player.currentLocation.NameOrUniqueName, kvp.Key, kvp.Value.Template);

            var name = Game1.getLocationFromName(Game1.player.currentLocation.NameOrUniqueName);
            color = IsObjectWorking(name, kvp.Value) && (objPower > 0 || !kvp.Value.Template.mustNeedOther || IsObjectNeeded(name, kvp.Value, CurrentGrid)) ? (enough ? color : Config.InsufficientColor) : Config.IdleColor;

            string str = "" + Math.Round(objPower);
            Vector2 pos = Game1.GlobalToLocal(Game1.viewport, kvp.Key * 64);
            b.DrawString(Game1.dialogueFont, str, pos + new Vector2(-2, 2), Config.ShadowColor, 0, Vector2.Zero, 1f, SpriteEffects.None, 0.999999f);
            b.DrawString(Game1.dialogueFont, str, pos + new Vector2(2, -2), Config.ShadowColor, 0, Vector2.Zero, 1f, SpriteEffects.None, 0.999999f);
            b.DrawString(Game1.dialogueFont, str, pos + new Vector2(-2, -2), Config.ShadowColor, 0, Vector2.Zero, 1f, SpriteEffects.None, 0.999999f);
            b.DrawString(Game1.dialogueFont, str, pos + new Vector2(2, 2), Config.ShadowColor, 0, Vector2.Zero, 1f, SpriteEffects.None, 0.999999f);
            b.DrawString(Game1.dialogueFont, str, pos, color, 0, Vector2.Zero, 1f, SpriteEffects.None, 0.9999999f);
        }
        private void DrawCharge(SpriteBatch b, KeyValuePair<Vector2, UtilityObjectInstance> kvp, Color color)
        {
            float charge = 0;
            int capacity;
            string chargeKey;
            if(CurrentGrid == GridType.electric)
            {
                capacity = kvp.Value.Template.electricChargeCapacity;
                chargeKey = "aedenthorn.UtilityGrid/electricCharge";
            }
            else
            {
                capacity = kvp.Value.Template.waterChargeCapacity;
                chargeKey = "aedenthorn.UtilityGrid/waterCharge";
            }
            if (capacity <= 0)
                return;

            if (kvp.Value.WorldObject.modData.TryGetValue(chargeKey, out string chargeString))
                float.TryParse(chargeString, NumberStyles.Float, CultureInfo.InvariantCulture, out charge);

            string str = Math.Round(charge, 1) + "\n" + capacity;
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

            Dictionary<Vector2, GridPipe> pipeDict = utilitySystemDict[location][gridType].pipes;

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
                utilitySystemDict[location] = new Dictionary<GridType, UtilitySystem>();
                utilitySystemDict[location][GridType.water] = new UtilitySystem();
                utilitySystemDict[location][GridType.electric] = new UtilitySystem();
            }
            RemakeGroups(location, GridType.water);
            RemakeGroups(location, GridType.electric);
        }
        public static void RemakeGroups(string location, GridType gridType)
        {
            if (!utilitySystemDict.ContainsKey(location))
            {
                utilitySystemDict[location] = new Dictionary<GridType, UtilitySystem>();
            }

            Dictionary<Vector2, GridPipe> pipeDict = new Dictionary<Vector2, GridPipe>(utilitySystemDict[location][gridType].pipes);
            List<PipeGroup> groupList = utilitySystemDict[location][gridType].groups;
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
                GameLocation gl = Game1.getLocationFromName(location);
                if (gl == null)
                {
                    gl = Game1.getLocationFromName(location, true);
                    if (gl == null)
                        return;
                }
                KeyValuePair<GameLocation, int> e = new KeyValuePair<GameLocation, int>(gl, (int)gridType);
                handler(context, e);
            }
        }

        private static void AddObjectsToGrid(string location, GridType gridType)
        {
            if (!utilitySystemDict.ContainsKey(location))
            {
                return;
            }
            List<PipeGroup> groupList = utilitySystemDict[location][gridType].groups;
            Dictionary<Vector2, UtilityObjectInstance> objectDict = utilitySystemDict[location][gridType].objects;
            objectDict.Clear();
            GameLocation gl = Game1.getLocationFromName(location);
            if (gl == null)
            {
                gl = Game1.getLocationFromName(location, true);
                if (gl == null)
                    return;
            }
            foreach (var kvp in gl.Objects.Pairs)
            {
                var obj = GetUtilityObjectAtTile(gl, kvp.Key);
                if (obj == null)
                    continue;
                if (gridType == GridType.water && obj.Template.water == 0 && obj.Template.waterChargeCapacity <= 0)
                    continue;
                if (gridType == GridType.electric && obj.Template.electric == 0 && obj.Template.electricChargeCapacity <= 0)
                    continue;
                foreach (var group in groupList)
                {
                    if (group.pipes.Contains(kvp.Key))
                    {
                        obj.Group = group;
                        break;
                    }
                }
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
            if (!utilitySystemDict[location][GridType.electric].objects.TryGetValue(tile, out UtilityObjectInstance obj) || obj.Group == null)
                return Vector2.Zero;
            var power = GetGroupPower(location, obj.Group, GridType.electric);
            power.X += GetGroupStoragePower(location, obj.Group, GridType.electric).X;
            return power;
        }
        public static Vector2 GetTileWaterPower(string location, Vector2 tile)
        {
            if (!utilitySystemDict[location][GridType.water].objects.TryGetValue(tile, out UtilityObjectInstance obj) || obj.Group == null)
                return Vector2.Zero;
            var power = GetGroupPower(location, obj.Group, GridType.water);
            power.X += GetGroupStoragePower(location, obj.Group, GridType.water).X;
            return power;
        }
        public static Vector2 GetGroupPower(string location, PipeGroup group, GridType type)
        {
            Vector2 power = Vector2.Zero;
            foreach (var tile in group.pipes)
            {
                if (!utilitySystemDict[location][type].objects.TryGetValue(tile, out UtilityObjectInstance obj))
                    continue;
                var objT = obj.Template;
                if (type == GridType.water && objT.electric < 0)
                {
                    Vector2 ePower = GetTileElectricPower(location, tile);
                    if (ePower.X == 0 || ePower.X + ePower.Y < 0) // unpowered
                        continue;
                }
                power += GetPowerVector(location, obj, type == GridType.water ? objT.water : objT.electric);
            }
            foreach (var func in powerFuctionList)
            {
                power += func(location, (int)type, group.pipes);
            }

            foreach (var v in utilitySystemDict[location][type].objects.Keys.ToArray())
            {
                if (group.pipes.Contains(v))
                    utilitySystemDict[location][type].objects[v].CurrentPowerVector = power;
            }
            return power;
        }
        public static Vector2 GetGroupStoragePower(string location, PipeGroup group, GridType type)
        {
            var chargeKey = type == GridType.water ? "aedenthorn.UtilityGrid/waterCharge" : "aedenthorn.UtilityGrid/electricCharge";
            Vector2 power = Vector2.Zero;
            foreach (var v in utilitySystemDict[location][type].objects.Keys.ToArray())
            {
                if (!group.pipes.Contains(v))
                    continue;
                var obj = utilitySystemDict[location][type].objects[v];
                var objW = obj.WorldObject;
                var objT = obj.Template;
                var capacity = type == GridType.water ? objT.waterChargeCapacity : objT.electricChargeCapacity;
                if (capacity == 0)
                    continue;

                float charge = 0;
                if (objW.modData.TryGetValue(chargeKey, out string chargeString))
                    float.TryParse(chargeString, NumberStyles.Float, CultureInfo.InvariantCulture, out charge);

                power.X += Math.Min(charge, type == GridType.water ? objT.waterDischargeRate : objT.electricDischargeRate);
                power.Y -= Math.Min(capacity - charge, type == GridType.water ? objT.waterChargeRate : objT.electricChargeRate);
            }
            return power; // X is providable, Y is receivable
        }
        public static void ChangeStorageObjects(string location, PipeGroup group, GridType type, float hours)
        {
            var netPower = group.powerVector.X + group.powerVector.Y;

            if (group.storageVector.X + netPower < 0) // not enough stored power
                return;

            GameLocation gl = Game1.getLocationFromName(location, false);
            if (gl == null)
            {
                gl = Game1.getLocationFromName(location, true);
                if (gl == null)
                {
                    SMonitor.Log($"Invalid game location {location}", StardewModdingAPI.LogLevel.Error);
                    return;
                }
            }
            var chargeKey = type == GridType.water ? "aedenthorn.UtilityGrid/waterCharge" : "aedenthorn.UtilityGrid/electricCharge";
            var changeObjects = new Dictionary<Vector2, float>();
            foreach (var v in utilitySystemDict[location][type].objects.Keys.ToArray())
            {
                if (!group.pipes.Contains(v))
                    continue;
                var obj = utilitySystemDict[location][type].objects[v];
                var objW = obj.WorldObject;
                var objT = obj.Template;
                var capacity = type == GridType.water ? objT.waterChargeCapacity : objT.electricChargeCapacity;
                if (capacity == 0)
                    continue;
                float charge = 0;
                if (obj.WorldObject.modData.TryGetValue(chargeKey, out string chargeString))
                    float.TryParse(chargeString, NumberStyles.Float, CultureInfo.InvariantCulture, out charge);
                if (type == GridType.water && hours > 0 && objT.fillWaterFromRain && gl.IsOutdoors && Game1.netWorldState.Value.GetWeatherForLocation(gl.GetLocationContext()).isRaining.Value)
                {
                    charge = Math.Min(charge + objT.waterChargeRate * hours, objT.waterChargeCapacity);
                    objW.modData[chargeKey] = charge + "";
                }
                if(netPower != 0)
                {
                    if(netPower > 0)
                        changeObjects.Add(v, Math.Min(capacity - charge, type == GridType.water ? objT.waterChargeRate : objT.electricChargeRate));
                    else
                        changeObjects.Add(v, Math.Min(charge, type == GridType.water ? objT.waterDischargeRate : objT.electricDischargeRate));
                }
            }

            // change charges

            while (changeObjects != null && changeObjects.Count > 0 && netPower != 0)
            {
                var eachPower = netPower / changeObjects.Count;
                foreach (var v in changeObjects.Keys.ToArray())
                {
                    var obj = utilitySystemDict[location][type].objects[v].WorldObject;
                    var objT = utilitySystemDict[location][type].objects[v].Template;
                    float currentCharge = 0;
                    float diff = changeObjects[v];
                    if (obj.modData.TryGetValue(chargeKey, out string chargeString))
                    {
                        float.TryParse(chargeString, NumberStyles.Float, CultureInfo.InvariantCulture, out currentCharge);
                    }
                    if (eachPower > 0)
                    {
                        var capacity = type == GridType.water ? objT.waterChargeCapacity : objT.electricChargeCapacity;
                        var add = Math.Min(capacity - currentCharge, Math.Min(diff, eachPower));
                        if (hours > 0)
                        {
                            SMonitor.Log($"adding {add * hours} {type} energy to {obj.Name} at {v}");
                            obj.modData[chargeKey] = Math.Min(capacity, currentCharge + add * hours) + "";
                        }
                        changeObjects[v] -= add;
                        if (add != eachPower)
                            changeObjects.Remove(v);
                    }
                    else
                    {
                        float subtract = Math.Min(currentCharge, Math.Min(diff, -eachPower));
                        if (hours > 0)
                        {
                            SMonitor.Log($"subtracting {subtract * hours} {type} energy from {obj.Name} at {v}");
                            obj.modData[chargeKey] = Math.Max(0, currentCharge - subtract * hours) + "";
                        }
                        changeObjects[v] -= subtract;
                        if (subtract != -eachPower)
                            changeObjects.Remove(v);
                    }
                }
            }
        }

        public static Vector2 GetPowerVector(string location, UtilityObjectInstance obj, float amount)
        {
            var power = Vector2.Zero;
            GameLocation gl = Game1.getLocationFromName(location);
            if(gl == null)
            {
                gl = Game1.getLocationFromName(location, true);
                if (gl == null)
                {
                    return power;
                }
            }
            if (amount == 0 || !IsObjectWorking(gl, obj))
                return power;
            if (amount > 0)
                power.X += amount;
            else if (amount < 0)
                power.Y += amount;
            return power;
        }

        public static bool IsObjectWorking(GameLocation location, UtilityObjectInstance obj)
        {
            return (!obj.Template.mustBeOn || obj.WorldObject.IsOn) &&
            (!obj.Template.onlyDay || Game1.timeOfDay < 1800) &&
            (!obj.Template.onlyNight || Game1.timeOfDay >= 1800) &&
            (!obj.WorldObject.IsSprinkler() || Game1.timeOfDay == 600) &&
            (!obj.Template.onlyMorning || Game1.timeOfDay == 600) &&
            (!obj.Template.mustBeFull || obj.WorldObject.heldObject.Value != null) &&
            (!obj.Template.mustBeWorking || obj.WorldObject.MinutesUntilReady > 0) &&
            (obj.Template.mustContain == null || obj.Template.mustContain.Length == 0 || obj.WorldObject.heldObject.Value?.Name == obj.Template.mustContain) &&
            (!obj.Template.mustHaveSun || ((location.IsOutdoors || location.IsGreenhouse) && !Game1.netWorldState.Value.GetWeatherForLocation(location.GetLocationContext()).isRaining.Value)) &&
            (!obj.Template.mustHaveRain || (location.IsOutdoors && Game1.netWorldState.Value.GetWeatherForLocation(location.GetLocationContext()).isRaining.Value)) &&
            (!obj.Template.mustHaveLightning || (location.IsOutdoors && !Game1.netWorldState.Value.GetWeatherForLocation(location.GetLocationContext()).isLightning.Value));
        }

        private static bool IsObjectNeeded(GameLocation location, UtilityObjectInstance obj, GridType checkType)
        {
            var type = checkType == GridType.water ? GridType.electric : GridType.water;
            foreach(var group in utilitySystemDict[location.NameOrUniqueName][type].groups)
            {
                if (group.pipes.Contains(obj.WorldObject.TileLocation))
                {
                    return group.powerVector.Y < 0 || group.storageVector.Y < 0;
                }
            }
            return false;
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

        public static UtilityObjectInstance GetUtilityObjectAtTile(GameLocation location, Vector2 tile)
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
                UtilityObjectInstance outObj = new UtilityObjectInstance(utilityObjectDict[obj.Name], obj);
                return outObj;
            }
            return null;
        }
    }
}