
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using StardewValley;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Object = StardewValley.Object;

namespace ImmersiveSprinklersAndScarecrows
{
    public partial class ModEntry
    {
        public static void SendMessage(string location, string which)
        {
            MyMessage message = new MyMessage(location, which);
            SHelper.Multiplayer.SendMessage(message, "UpdateImmersiveObjects", modIDs: new[] { context.ModManifest.UniqueID });
        }
        public static Object GetSprinklerCached(GameLocation l, int x, int y)
        {
            if (!sprinklerDict.TryGetValue(l, out var dict))
            {
                dict = new Dictionary<Vector2, Object>();
                sprinklerDict[l] = dict;
            }
            if(!dict.TryGetValue(new Vector2(x, y), out var obj))
            {
                obj = GetSprinkler(l, x, y);
                dict[new Vector2(x, y)] = obj;
            }
            return obj;
        }

        public static Object GetScarecrowCached(GameLocation l, int x, int y)
        {
            if (!scarecrowDict.TryGetValue(l, out var dict))
            {
                dict = new Dictionary<Vector2, Object>();
                scarecrowDict[l] = dict;
            }
            if (!dict.TryGetValue(new Vector2(x, y), out var obj))
            {
                obj = GetScarecrow(l, x, y);
                dict[new Vector2(x, y)] = obj;
            }
            return obj;
        }
        public static Object GetSprinkler(GameLocation l, int x, int y)
        {
            if (!TryGetData(l, x, y, out var data) || !data.Sprinkler)
                return null;
            Object obj = null;
            if (data.BigCraftable)
            {
                if (Game1.bigCraftableData.ContainsKey(data.ItemId))
                {
                    obj = new Object(Vector2.Zero, data.ItemId);
                }
            }
            else if (Game1.objectData.ContainsKey(data.ItemId))
            {
                obj = new Object(data.ItemId, 1);
            }
            if (obj != null)
            {
                obj.TileLocation = new Vector2(x, y);
                
                if (data.Enricher)
                {
                    obj.heldObject.Value = new Object("913", 1);
                    Chest chest = new Chest
                    {
                        SpecialChestType = Chest.SpecialChestTypes.Enricher
                    };
                    if (data.Fertilizer != null)
                    {
                        chest.addItem(new Object(data.Fertilizer, data.FertilizerStack));
                    }
                    obj.heldObject.Value.heldObject.Value = chest;
                    if (data.Nozzle)
                    {
                        obj.modData[nozzleKey] = "true";
                    }
                }
                else if (data.Nozzle)
                {
                    obj.heldObject.Value = new Object("915", 1);
                }
                foreach (var m in data.modData)
                {
                    obj.modData[m.Key] = m.Value;
                }
                if(!sprinklerDict.TryGetValue(l, out var dict))
                {
                    dict = new Dictionary<Vector2, Object>();
                    sprinklerDict[l] = dict;
                }
                dict[new Vector2(x,y)] = obj;
            }
            return obj;
        }
        public static Object GetScarecrow(GameLocation l, int x, int y)
        {
            if (!TryGetData(l, x, y, out var data) || data.Sprinkler)
                return null;
            Object obj = null;
            if (data.BigCraftable)
            {
                if (Game1.bigCraftableData.ContainsKey(data.ItemId))
                {
                    obj = new Object(Vector2.Zero, data.ItemId);
                }
            }
            else if (Game1.objectData.ContainsKey(data.ItemId))
            {
                obj = new Object(data.ItemId, 1);
            }
            if (obj == null)
                return null;
            obj.TileLocation = new Vector2(x, y);
            if (data.Scared > 0)
            {
                obj.SpecialVariable = data.Scared;
            }
            if (data.Hat is not null)
            {
                obj.preservedParentSheetIndex.Value = data.Hat;
            }
            foreach (var m in data.modData)
            {
                obj.modData[m.Key] = m.Value;
            }
            if (!scarecrowDict.TryGetValue(l, out var dict))
            {
                dict = new Dictionary<Vector2, Object>();
                scarecrowDict[l] = dict;
            }
            dict[new Vector2(x, y)] = obj; 
            return obj;
        }

        public static Point GetMouseCornerTile()
        {
            var tile = Game1.currentCursorTile.ToPoint();
            var x = Game1.getMouseX() + Game1.viewport.X;
            var y = Game1.getMouseY() + Game1.viewport.Y;
            if (x % 64 < 32)
            {
                if (y % 64 < 32)
                {
                    return tile + new Point(-1, -1);
                }
                else
                {
                    return tile + new Point(-1, 0);
                }
            }
            else
            {
                if (y % 64 < 32)
                {
                    return tile + new Point(0, -1);
                }
                else
                {
                    return tile + new Point(0, 0);
                }
            }
        }
        public static Object GetSprinklerAtMouse()
        {
            if (Game1.currentLocation == null)
                return null;

            Point tile = GetMouseCornerTile();

            return GetSprinklerCached(Game1.currentLocation, tile.X, tile.Y);
        }
        public static Object GetScarecrowAtMouse()
        {
            if (Game1.currentLocation == null)
                return null;

            Point tile = GetMouseCornerTile();

            return GetScarecrowCached(Game1.currentLocation, tile.X, tile.Y);
        }
        public static void PlaceObject(Object one, GameLocation location, int tileX, int tileY, bool sprinkler)
        {
            ImmersiveData data = GetImmersiveData(one, sprinkler);
            SetData(location, tileX, tileY, data);
            var dict = sprinkler ? sprinklerDict : scarecrowDict;
            if (!dict.TryGetValue(location, out var dict2))
            {
                dict2 = new Dictionary<Vector2, Object>();
                dict[location] = dict2;
            }
            dict2[new Vector2(tileX, tileY)] = one;
            SendMessage(location.NameOrUniqueName, sprinkler ? "sprinklers" : "scarecrows");
        }
        public static bool ReturnOrDropSprinkler(GameLocation l, int x, int y, Farmer who, bool drop)
        {
            var pos = new Vector2(x, y) * 64;
            Object sprinkler = GetSprinklerCached(l, x, y);
            if (sprinkler == null)
                return false;
            if (drop)
            {
                l.debris.Add(new Debris(sprinkler, pos));
                if (sprinkler.heldObject.Value != null)
                {
                    if (sprinkler.heldObject.Value.heldObject.Value is Chest chest && chest.Items.FirstOrDefault() is Item obj)
                    {
                        l.debris.Add(new Debris(obj, pos));
                    }
                    l.debris.Add(new Debris(sprinkler.heldObject.Value, pos));
                }
            }
            else
            {
                if (sprinkler.heldObject.Value != null)
                {
                    if (sprinkler.heldObject.Value.heldObject.Value is Chest chest && chest.Items.FirstOrDefault() is Item obj)
                    {
                        TryReturnObject(obj, who);
                    }
                    TryReturnObject(sprinkler.heldObject.Value, who);
                    if (sprinkler.modData.ContainsKey(nozzleKey))
                    {
                        TryReturnObject(ItemRegistry.Create("(O)915"), who);
                        sprinkler.modData.Remove(nozzleKey);
                    }
                }
                TryReturnObject(sprinkler, who);
            }
            SetData(l, x, y, null);
            if (sprinklerDict.TryGetValue(l, out var dict))
            {
                dict.Remove(new Vector2(x, y));
            }
            SMonitor.Log($"{(drop ? "Dropped" : "Returned")} {sprinkler?.Name}");
            SendMessage(l.NameOrUniqueName, "sprinklers");
            return true;
        }

        public static bool ReturnOrDropScarecrow(GameLocation l, int x, int y, Farmer who, bool drop)
        {

            var pos = new Vector2(x, y) * 64;
            Object scarecrow = GetScarecrowCached(l, x, y);
            if (scarecrow == null)
                return false;
            if (drop)
            {
                l.debris.Add(new Debris(scarecrow, pos));
                if (scarecrow.preservedParentSheetIndex.Value != null)
                {
                    l.debris.Add(new Debris(new Hat(scarecrow.preservedParentSheetIndex.Value), pos));
                }
            }
            else
            {
                TryReturnObject(scarecrow, who);
                if (scarecrow.preservedParentSheetIndex.Value != null)
                {
                    TryReturnObject(new Hat(scarecrow.preservedParentSheetIndex.Value), who);
                }
            }
            SetData(l, x, y, null);
            if (scarecrowDict.TryGetValue(l, out var dict))
            {
                dict.Remove(new Vector2(x, y));
            }
            SMonitor.Log($"{(drop ? "Dropped" : "Returned")} {scarecrow?.Name}");
            SendMessage(l.NameOrUniqueName, "scarecrows");
            return true;
        }

        public static void TryReturnObject(Item obj, Farmer who)
        {
            if (obj is null)
                return;
            SMonitor.Log($"Trying to return {obj.Name}");
            if (!who.addItemToInventoryBool(obj))
            {
                who.currentLocation.debris.Add(new Debris(obj, who.Position));
            }
        }

        public static int GetSprinklerRadius(Object obj)
        {
            if (!Config.SprinklerRadii.TryGetValue(obj.ItemId, out int radius) && !Config.SprinklerRadii.TryGetValue(obj.Name, out radius))
                return obj.GetModifiedRadiusForSprinkler();
            if (obj.modData.ContainsKey(nozzleKey) || (obj.heldObject.Value != null && Utility.IsNormalObjectAtParentSheetIndex(obj.heldObject.Value, "915")))
            {
                radius++;
            }
            return radius;
        }

        public static List<Vector2> GetSprinklerTiles(Vector2 tileLocation, int radius)
        {

            Vector2 start = tileLocation + new Vector2(-1, -1) * radius;
            List<Vector2> list = new();
            var diameter = (radius + 1) * 2;
            for (int x = 0; x < diameter; x++)
            {
                for (int y = 0; y < diameter; y++)
                {
                    list.Add(start + new Vector2(x, y));
                }
            }
            return list;

        }
        public static List<Vector2> GetScarecrowTiles(Vector2 tileLocation, int radius)
        {
            Vector2 start = tileLocation + new Vector2(-1, -1) * (radius - 2);
            Vector2 position = tileLocation + new Vector2(0.5f, 0.5f);
            List<Vector2> list = new();
            var diameter = (radius - 1) * 2;
            for (int x = 0; x < diameter; x++)
            {
                for (int y = 0; y < diameter; y++)
                {
                    Vector2 tile = start + new Vector2(x, y);
                    var distance = (int)Math.Ceiling(Vector2.Distance(position, tile));
                    if (distance <= radius)
                        list.Add(tile);
                }
            }
            return list;

        }
        public static bool IsScarecrowInRange(bool scarecrow, Farm f, Vector2 v)
        {
            if (!Config.EnableMod || scarecrow)
                return scarecrow;
            foreach (var obj in GetScarecrows(f))
            {
                if (obj is not null)
                {
                    var tiles = GetScarecrowTiles(obj.TileLocation, obj.GetRadiusForScarecrow());
                    if (tiles.Contains(v))
                    {
                        obj.SpecialVariable++;
                        SMonitor.Log($"Scarecrow at {obj.TileLocation} has scared {obj.SpecialVariable} crows");
                        return true;
                    }
                }
            }
            foreach (var obj in GetSprinklers(f))
            {
                if (obj is not null && obj.IsScarecrow())
                {
                    var tiles = GetScarecrowTiles(obj.TileLocation, obj.GetRadiusForScarecrow());
                    if (tiles.Contains(v))
                    {
                        obj.SpecialVariable++;
                        return true;
                    }
                }
            }
            return false;
        }


        public static void ActivateSprinkler(GameLocation environment, Vector2 tileLocation, Object obj, bool delay)
        {
            if (Game1.player.team.SpecialOrderRuleActive("NO_SPRINKLER", null))
            {
                return;
            }
            int radius = GetSprinklerRadius(obj);
            if (radius < 0)
                return;

            obj.Location = environment;

            foreach (Vector2 tile in GetSprinklerTiles(tileLocation, radius))
            {
                obj.ApplySprinkler(tile);
                if (environment.objects.TryGetValue(tile, out var o) && o is IndoorPot)
                {
                    (o as IndoorPot).hoeDirt.Value.state.Value = 1;
                    o.showNextIndex.Value = true;
                }
            }
            ApplySprinklerAnimation(tileLocation, radius, environment, delay ? Game1.random.Next(1000) : 0);
        }
        public static void ApplySprinklerAnimation(Vector2 tileLocation, int radius, GameLocation location, int delay)
        {
            int id = (int)(tileLocation.X * 4000f + tileLocation.Y * 10);
            if (radius < 0 || location.getTemporarySpriteByID(id) is not null)
            {
                return;
            }
            var position = tileLocation * 64 + new Vector2(32, 16);
            float layerDepth = 1;
            if (radius == 0)
            {
                float rotation = 60 * MathHelper.Pi / 180;
                int a = 24;
                int b = 40;
                location.temporarySprites.Add(new TemporaryAnimatedSprite(29, position + new Vector2(a, -b), Color.White * 0.5f, 4, false, 60f, 100, -1, layerDepth, -1, 0)
                {
                    rotation = rotation,
                    delayBeforeAnimationStart = delay,
                    id = id
                });
                location.temporarySprites.Add(new TemporaryAnimatedSprite(29, position + new Vector2(b, a), Color.White * 0.5f, 4, false, 60f, 100, -1, layerDepth, -1, 0)
                {
                    rotation = 1.57079637f + rotation,
                    delayBeforeAnimationStart = delay,
                    id = id
                });
                location.temporarySprites.Add(new TemporaryAnimatedSprite(29, position + new Vector2(-a, b), Color.White * 0.5f, 4, false, 60f, 100, -1, layerDepth, -1, 0)
                {
                    rotation = 3.14159274f + rotation,
                    delayBeforeAnimationStart = delay,
                    id = id
                });
                location.temporarySprites.Add(new TemporaryAnimatedSprite(29, position + new Vector2(-b, -a), Color.White * 0.5f, 4, false, 60f, 100, -1, layerDepth, -1, 0)
                {
                    rotation = 4.712389f + rotation,
                    delayBeforeAnimationStart = delay,
                    id = id
                });
                return;
            }
            if (radius == 1)
            {
                location.temporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Rectangle(0, 1984, 192, 192), 60f, 3, 100, position + new Vector2(-96f, -96f), false, false)
                {
                    color = Color.White * 0.4f,
                    delayBeforeAnimationStart = delay,
                    id = id,
                    layerDepth = layerDepth,
                    scale = 1.3f
                });
                return;
            }
            float scale = radius / 1.6f;
            location.temporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Rectangle(0, 2176, 320, 320), 60f, 4, 100, position + new Vector2(32f, 32f) + new Vector2(-160f, -160f) * scale, false, false)
            {
                color = Color.White * 0.4f,
                delayBeforeAnimationStart = delay,
                id = id,
                layerDepth = layerDepth,
                scale = scale
            });
        }
        public static IEnumerable<Object> GetSprinklers(GameLocation l)
        {
            return l.modData.FieldDict.Where(p => p.Key.StartsWith(dataKey + ",")).Select(p => GetSprinklerCached(l, int.Parse(p.Key.Split(',')[1]), int.Parse(p.Key.Split(',')[2]))).Where(o => o is not null);
        }
        public static IEnumerable<Object> GetScarecrows(GameLocation l)
        {
            return l.modData.FieldDict.Where(p => p.Key.StartsWith(dataKey + ",")).Select(p => GetScarecrowCached(l, int.Parse(p.Key.Split(',')[1]), int.Parse(p.Key.Split(',')[2]))).Where(o => o is not null);
        }
        public static IEnumerable<Object> GetAsScarecrows(GameLocation l)
        {
            return GetScarecrows(l).Concat(GetSprinklers(l).Where(s => s.IsScarecrow()));
        }

        public static void SetAltTextureForObject(Object obj)
        {
            if (atApi is null)
                return;
            try
            {
                AccessTools.Method(atApi.GetType(), "SetTextureForObject").Invoke(atApi, new object[] { obj });
            }
            catch
            {
            }
        }

        public static Texture2D GetAltTextureForObject(Object obj, out Rectangle sourceRect)
        {
            sourceRect = new Rectangle();
            var inputParams = new object[] { obj, sourceRect };
            Texture2D result = (Texture2D)AccessTools.Method(atApi.GetType(), "GetTextureForObject").Invoke(atApi, inputParams);
            sourceRect = (Rectangle)inputParams[1];
            if (false && Config.Debug)
            {
                SMonitor.Log($"Alt Texture: Location: {obj.Location.Name}, {obj.TileLocation}, result:\n\n\tTexture: {result.Name}\n\tData{string.Join('|', obj.modData.Pairs.Select(p => p.Key + ":"+p.Value))}");
            }
            return result;
        }

        public static bool TryGetData(GameLocation l, int x, int y, out ImmersiveData value)
        {
            value = null;
            if (l.modData.TryGetValue($"{dataKey},{x},{y}", out var str) && !string.IsNullOrEmpty(str))
            {
                if (false && Config.Debug)
                {
                    SMonitor.Log($"Location: {l.Name}, {x},{y}:\n\n\t{str}");
                }
                value = JsonConvert.DeserializeObject<ImmersiveData>(str) ?? new ImmersiveData();
                return true;
            }
            return false;
        }
        public static bool HasData(GameLocation l, int x, int y)
        {
            return l.modData.ContainsKey($"{dataKey},{x},{y}");
        }
        public static void SetData(GameLocation l, int x, int y, ImmersiveData value)
        {
            if(value == null)
            {
                l.modData.Remove($"{dataKey},{x},{y}");
            }
            else
            {
                l.modData[$"{dataKey},{x},{y}"] = JsonConvert.SerializeObject(value);
            }
        }

        public static void OpenPaintMenu(Tool tool, Object obj)
        {
            atPosition = obj.TileLocation;
            Game1.currentLocation.Objects[new Vector2(-ridiculous, -ridiculous)] = obj;
            tool.beginUsing(Game1.currentLocation, -ridiculous * 64, -ridiculous * 64, Game1.player);
        }

        public static ImmersiveData GetImmersiveData(Object obj, bool sprinkler)
        {

            ImmersiveData data = new ImmersiveData()
            {
                Sprinkler = sprinkler,
                ItemId = obj.ItemId,
                BigCraftable = obj.bigCraftable.Value,

            };
            if (sprinkler)
            {
                if (obj.heldObject.Value is Object held)
                {
                    if (held.ItemId == "913")
                    {
                        data.Enricher = true;
                        if (held.heldObject.Value is Chest chest && chest.Items.Count > 0 && chest.Items[0] is Item item)
                        {
                            data.Fertilizer = item.ItemId;
                            data.FertilizerStack = item.Stack;
                        }
                    }
                    else if (held.ItemId == "915")
                    {
                        data.Nozzle = true;
                    }
                }
            }
            else
            {
                data.Scared = obj.SpecialVariable;
                data.Hat = obj.preservedParentSheetIndex.Value;
            }

            if (atApi is not null)
            {
                SetAltTextureForObject(obj);
            }
            foreach (var kvp in obj.modData.Pairs)
            {
                data.modData.Add(kvp.Key, kvp.Value);
            }
            return data;
        }

        public static void StoreObj(Object obj)
        {
            if (obj.Location is null)
                return;
            var data = GetImmersiveData(obj, obj.IsSprinkler());
            obj.Location.modData[$"{dataKey},{obj.TileLocation.X},{obj.TileLocation.Y}"] = JsonConvert.SerializeObject(data);
        }
        public static bool CheckForHoeDirt(GameLocation l, Vector2 tile)
        {
            if (!Config.RequireHoeDirt)
                return true;
            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    if (!l.terrainFeatures.TryGetValue(tile + new Vector2(i, j), out var tf) || tf is not HoeDirt)
                    {
                        return false;
                    }
                }
            }
            return true;
        }
        public static void ReloadSprinklers(GameLocation location)
        {
            if (sprinklerDict.TryGetValue(location, out var dict))
            {
                dict.Clear();
            }
            else
            {
                dict = new();
                sprinklerDict[location] = dict;
            }
            var sp = GetSprinklers(location);
            var count = sp.Count();

        }
        public static void ReloadScarecrows(GameLocation location)
        {
            if (scarecrowDict.TryGetValue(location, out var dict))
            {
                dict.Clear();
            }
            else
            {
                dict = new();
                scarecrowDict[location] = dict;
            }
            var sp = GetScarecrows(location);
            var count = sp.Count();
        }
    }
}