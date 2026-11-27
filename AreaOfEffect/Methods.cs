using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.GameData;
using StardewValley.Internal;
using StardewValley.Mods;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using Object = StardewValley.Object;

namespace AreaOfEffect
{
    public partial class ModEntry
    {
        public static void CastSpell(Tool tool, int level, Vector2 tile)
        {
            if (!Config.ModEnabled || !TryGetTool(tool, out var tdata))
            {
                return;
            }
            if (tool.lastUser is null)
                tool.lastUser = Game1.player;
            if (!TryGetEffect(tool, out var data))
            {
                return;
            }
            CastSpell(tool, tool.lastUser, tdata, data, level, tile);
        }
        public static void CastSpell(Tool tool, Farmer f, SpellToolData tdata, SpellData data, int level, Vector2 tile)
        {
            level = Math.Min(level, data.SpellLevels.Count - 1);
            var cdata = data.SpellLevels[level];
            if (tdata.MaxCharges > 0)
            {
                var charges = GetCurrentCharges(tool, tdata.MaxCharges);
                if (charges <= 0)
                {
                    Game1.showRedMessage(string.Format(SHelper.Translation.Get("x-no-charges-y"), tool.DisplayName, ItemRegistry.GetDataOrErrorItem(tdata.RechargeItem).DisplayName));
                    return;
                }
                SetCurrentCharges(tool, charges - cdata.Charges);
            }
            if (Config.ForceRecast)
            {
                tool.modData.Remove(effectKey);
            }

            if(cdata.Projectiles?.Any() == true)
            {
                foreach (var item in cdata.Projectiles) 
                {
                    FireProjectile(f.currentLocation, f, item, tile, cdata);
                }
            }
            else
            {
                DoCastSpell(f.currentLocation, f, tile, cdata);
            }
            if (!string.IsNullOrEmpty(cdata.CastSound))
            {
                f.currentLocation.playSound(cdata.CastSound, f.Tile);
            }

        }

        public static List<Vector2> GetTiles(Vector2 origin, Vector2 target, SpellCastData data)
        {
            if(data.AreaType == AOEType.Line)
            {
                HashSet<Vector2> set = new();
                var length = (target - origin).Length();
                for (int i = 0; i < length; i++)
                {
                    Vector2 pos = Vector2.Lerp(origin, target, i);
                    set.AddRange(GetRoundedTiles(pos));
                }
                return set.ToList();
            }
            List<Vector2> list = new();
            Vector2 start = target + new Vector2(-1, -1) * data.Radius;
            Vector2 position = target;
            if(data.Radius == 0)
            {
                list.Add(target);
                return list;
            }
            var diameter = (data.Radius) * 2;
            for (int x = 0; x < diameter; x++)
            {
                for (int y = 0; y < diameter; y++)
                {
                    Vector2 tile = start + new Vector2(x, y);
                    var distance = Convert.ToInt32(Math.Round(Vector2.Distance(position, tile)));
                    if (data.AreaType == AOEType.Square || distance <= data.Radius)
                        list.Add(tile);
                }
            }
            return list;
        }

        private static IEnumerable<Vector2> GetRoundedTiles(Vector2 pos)
        {
            return new List<Vector2>()
            {
                new Vector2((float)Math.Ceiling(pos.X), (float)Math.Ceiling(pos.Y)),
                new Vector2((float)Math.Ceiling(pos.X), (float)Math.Floor(pos.Y)),
                new Vector2((float)Math.Floor(pos.X), (float)Math.Ceiling(pos.Y)),
                new Vector2((float)Math.Floor(pos.X), (float)Math.Floor(pos.Y))
            };
        }

        public static Vector2 GetTargetTile(Farmer f, SpellData t, int maxDistance)
        {
            var target = Game1.currentCursorTile;
            int d = maxDistance;
            float m = Vector2.Distance(target, f.Tile);
            if(m > d)
            {
                target = Vector2.Lerp(f.Tile, target, d / m);
                target = new Vector2(Convert.ToInt32(Math.Round(target.X)), Convert.ToInt32(Math.Round(target.Y)));
            }
            return target;
        }

        public static int GetCurrentCharges(Tool tool, int max)
        {
            if (!tool.modData.TryGetValue(chargesKey, out var str))
            {
                SetCurrentCharges(tool, max);
                return max;
            }
            return int.Parse(str);
        }
        public static void SetCurrentCharges(Tool tool, int value)
        {
            tool.modData[chargesKey] = value.ToString();
        }

        public static void DestroyAt(GameLocation l, Vector2 tile, object o)
        {
            if (o is Object)
            {
                l.Objects.Remove(tile);
            }
            else if (o is Crop c && c.Dirt is not null)
            {
                c.Dirt.crop = null;
            }
            else if (o is TerrainFeature tf && tf.Location is not null)
            {
                if (tf is Tree t)
                {
                    if (t.growthStage.Value >= 5)
                    {
                        t.health.Value = 0;
                        AccessTools.Method(typeof(Tree), "performTreeFall").Invoke(t, new object[] { null, 1, tile });
                    }
                    else
                    {
                        l.terrainFeatures.Remove(tile);
                    }
                }
                else if(tf is Grass g)
                {
                    g.reduceBy(4, true);
                }
            }
        }

        public static bool TryGetTool(Tool __instance, out SpellToolData data)
        {
            var key = __instance.ItemId;
            if(!ToolDict.TryGetValue(__instance.ItemId, out data))
            {
                data = null;
                return false;
            }
            return true;
        }
        public static bool TryGetEffect(Tool __instance, out SpellData data)
        {
            var key = __instance.ItemId;
            if (__instance.modData.TryGetValue(effectKey, out key))
            {
            }
            else if(TryGetTool(__instance, out var tdata) && tdata.Spells?.Count == 1)
            {
                key = tdata.Spells[0];
            }
            else
            {
                data = null;
                return false;
            }
            if (!SpellDict.TryGetValue(key, out data))
            {
                data = null;
                return false;
            }
            return true;
        }

        public static void SetEffect(Tool tool, string type)
        {
            Game1.playSound(Config.SetEffectSound);
            tool.modData[effectKey] = type;
        }

        public static void PerformTool(GameLocation l, Vector2 tile, Farmer who, string id, bool asFarmer)
        {
            if (!asFarmer)
                who = new Farmer();
            var tool = ItemRegistry.Create<Tool>(id, 1, 0, false);
            if (tool is null)
            {
                SMonitor.Log($"Missing tool for id {id}", LogLevel.Warn);
                return;
            }
            tool.DoFunction(l, Convert.ToInt32(tile.X) * 64, Convert.ToInt32(tile.Y) * 64, 1, who);
        }

        public static void FireProjectile(GameLocation l, Farmer who, SpellProjectileData pdata, Vector2 tile, SpellCastData data)
        {
            Vector2 shotOrigin = who.getStandingPosition() - new Vector2(32f, 32f);

            var v = tile * 64 - shotOrigin;
            var angle = -Math.Atan2(v.X, -v.Y) + Math.PI / 2;
            

            float angleOffsetMultiplier = 1f;
            var offset = (pdata.MinAngleOffset + (float)Game1.random.NextDouble() * (pdata.MaxAngleOffset - pdata.MinAngleOffset)) * angleOffsetMultiplier * Math.PI / 180;

            angle += offset;

            string shotItemId = null;
            if (pdata.Item != null)
            {
                ISpawnItemData item = pdata.Item;
                GameLocation currentLocation = who.currentLocation;
                Random random = null;
                DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(30, 2);
                defaultInterpolatedStringHandler.AppendLiteral("AreaOfEffect '");
                defaultInterpolatedStringHandler.AppendFormatted(who.CurrentTool?.ItemId);
                defaultInterpolatedStringHandler.AppendLiteral("' > projectile data '");
                defaultInterpolatedStringHandler.AppendFormatted(pdata.Id);
                defaultInterpolatedStringHandler.AppendLiteral("'");
                Item item2 = ItemQueryResolver.TryResolveRandomItem(item, new ItemQueryContext(currentLocation, who, random, defaultInterpolatedStringHandler.ToStringAndClear()), false, null, null, null, null);
                shotItemId = ((item2 != null) ? item2.QualifiedItemId : null);
                if (shotItemId == null)
                {
                    return;
                }
            }
            int damage = pdata.Damage;
            int spriteIndex = pdata.SpriteIndex;
            int bounces = pdata.Bounces;
            int tailLength = pdata.TailLength;
            float num = (float)pdata.RotationVelocity * 0.017453292f;
            float num2 = (float)pdata.Velocity * (float)Math.Cos((double)angle);
            float num3 = (float)pdata.Velocity * (float)(-(float)Math.Sin((double)angle));
            Vector2 vector = shotOrigin;
            string fireSound = pdata.FireSound;
            SpellProjectile projectile = new SpellProjectile(damage, spriteIndex, bounces, tailLength, num, num2, num3, vector, pdata.CollisionSound, pdata.BounceSound, fireSound, pdata.Explodes, true, who.currentLocation, who, new(delegate (GameLocation l, int x, int y, Character who)
            {
                var t = new Vector2(x / 64, y / 64);
                if(!string.IsNullOrEmpty(data.TriggerSound))
                    l.playSound(data.TriggerSound, t);
                DoCastSpell(l, who as Farmer, t, data);

            }), shotItemId, pdata.Texture, pdata.SourceRect);
            projectile.ignoreTravelGracePeriod.Value = true;
            projectile.ignoreMeleeAttacks.Value = true;
            projectile.maxTravelDistance.Value = Convert.ToInt32((v.Length()));
            projectile.height.Value = 32f;
            who.currentLocation.projectiles.Add(projectile);
            if(data.AreaType == AOEType.Line)
            {
                ProjectileDict[projectile] = new()
                {
                    firer = who,
                    location = l,
                    target = tile,
                    affectedTiles = new(),
                    spell = data
                };
            }
        }

        public static void SetModData(ModDataDictionary dict, SpellEffect effect)
        {
            dict[effect.Name] = effect.Value.ToString();
        }

        public static void TrySetCustomVariable(object obj, SpellEffect effect)
        {
            var prop = AccessTools.Property(obj.GetType(), effect.Name);
            if(prop != null)
            {
                try
                {
                    if (prop.PropertyType == typeof(int))
                    {
                        var nf = Convert.ToInt32(prop.GetValue(obj));
                        nf = Convert.ToInt32(GetNumber(nf, effect));
                        prop.SetValue(obj, nf);
                    }
                    else if (prop.PropertyType == typeof(long))
                    {
                        var nf = (long)prop.GetValue(obj);
                        nf = GetNumber(nf, effect);
                        prop.SetValue(obj, nf);
                    }
                    else if (prop.PropertyType == typeof(ulong))
                    {
                        var nf = (ulong)prop.GetValue(obj);
                        nf = (ulong)GetNumber((long)nf, effect);

                        prop.SetValue(obj, nf);
                    }
                    else if (prop.PropertyType == typeof(byte))
                    {
                        var nf = Convert.ToByte(prop.GetValue(obj));
                        nf = Convert.ToByte(GetNumber(nf, effect));
                        prop.SetValue(obj, nf);
                    }
                    else if (prop.PropertyType == typeof(float))
                    {
                        var nf = (float)prop.GetValue(obj);
                        nf = (float)GetDecimal(nf, effect);
                        prop.SetValue(obj, nf);
                    }
                    else if (prop.PropertyType == typeof(double))
                    {
                        var nf = (double)prop.GetValue(obj);
                        nf = GetDecimal(nf, effect);
                        prop.SetValue(obj, nf);
                    }
                    else if (prop.PropertyType == typeof(bool))
                    {
                        var nf = (bool)prop.GetValue(obj);
                        nf = GetBool(nf, effect);
                        prop.SetValue(obj, nf);
                    }
                    else if (prop.PropertyType == typeof(Point))
                    {
                        var nf = (Point)prop.GetValue(obj);
                        nf = GetPoint(nf, effect);
                        prop.SetValue(obj, nf);
                    }
                    else if (prop.PropertyType == typeof(Vector2))
                    {
                        var nf = (Vector2)prop.GetValue(obj);
                        nf = GetVector(nf, effect);
                        prop.SetValue(obj, nf);
                    }
                    else if (prop.PropertyType == typeof(Rectangle))
                    {
                        string[] split = ((string)effect.Value).Split(',');
                        var nf = (Rectangle)prop.GetValue(obj);
                        nf = GetRectangle(nf, effect);
                        prop.SetValue(obj, nf);
                    }
                    else if (prop.PropertyType == typeof(Color))
                    {
                        var nf = (Color)prop.GetValue(obj);
                        nf = GetColor(nf, effect);
                        prop.SetValue(obj, nf);
                    }
                    else if (prop.PropertyType == typeof(string))
                    {
                        var nf = (string)prop.GetValue(obj);
                        nf = GetString(nf, effect);
                        prop.SetValue(obj, nf);
                    }
                }
                catch { }
                return;
            }
            var field = AccessTools.Field(obj.GetType(), effect.Name);
            if(field != null)
            {
                try
                {
                    if(field.FieldType == typeof(NetInt))
                    {
                        var nf = (NetInt)field.GetValue(obj);
                        nf.Value = Convert.ToInt32(GetNumber(nf.Value, effect));
                    }
                    else if(field.FieldType == typeof(int))
                    {
                        var nf = Convert.ToInt32(field.GetValue(obj));
                        nf = Convert.ToInt32(GetNumber(nf, effect));
                        field.SetValue(obj, nf);
                    }
                    else if(field.FieldType == typeof(NetLong))
                    {
                        var nf = (NetLong)field.GetValue(obj);
                        nf.Value = GetNumber(nf.Value, effect);

                    }
                    else if(field.FieldType == typeof(long))
                    {
                        var nf = (long)field.GetValue(obj);
                        nf = GetNumber(nf, effect);
                        field.SetValue(obj, nf);
                    }
                    else if(field.FieldType == typeof(ulong))
                    {
                        var nf = (ulong)field.GetValue(obj);
                        nf = (ulong)GetNumber((long)nf, effect);

                        field.SetValue(obj, nf);
                    }
                    else if(field.FieldType == typeof(NetByte))
                    {
                        var nf = (NetByte)field.GetValue(obj);
                        nf.Value = (byte)GetNumber(nf.Value, effect);

                    }
                    else if(field.FieldType == typeof(byte))
                    {
                        var nf = (byte)field.GetValue(obj);
                        nf = (byte)GetNumber(nf, effect);
                        field.SetValue(obj, nf);
                    }
                    else if(field.FieldType == typeof(NetFloat))
                    {
                        var nf = (NetFloat)field.GetValue(obj);
                        nf.Value = (float)GetDecimal(nf.Value, effect);
                    }
                    else if(field.FieldType == typeof(float))
                    {
                        var nf = (float)field.GetValue(obj);
                        nf = (float)GetDecimal(nf, effect);
                        field.SetValue(obj, nf);
                    }
                    else if(field.FieldType == typeof(NetDouble))
                    {
                        var nf = (NetDouble)field.GetValue(obj);
                        nf.Value = GetDecimal(nf.Value, effect);
                    }
                    else if(field.FieldType == typeof(double))
                    {
                        var nf = (double)field.GetValue(obj);
                        nf = GetDecimal(nf, effect);
                        field.SetValue(obj, nf);
                    }
                    else if(field.FieldType == typeof(NetBool))
                    {
                        var nf = (NetBool)field.GetValue(obj);
                        nf.Value = GetBool(nf.Value, effect);
                    }
                    else if(field.FieldType == typeof(bool))
                    {
                        var nf = (bool)field.GetValue(obj);
                        nf = GetBool(nf, effect);
                        field.SetValue(obj, nf);
                    }
                    else if (field.FieldType == typeof(NetPoint))
                    {
                        var nf = (NetPoint)field.GetValue(obj);
                        nf.Value = GetPoint(nf.Value, effect);
                    }
                    else if (field.FieldType == typeof(Point))
                    {
                        var nf = (Point)field.GetValue(obj);
                        nf = GetPoint(nf, effect);
                        field.SetValue(obj, nf);
                    }
                    else if (field.FieldType == typeof(NetVector2))
                    {
                        var nf = (NetVector2)field.GetValue(obj);
                        nf.Value = GetVector(nf.Value, effect);

                    }
                    else if (field.FieldType == typeof(Vector2))
                    {
                        var nf = (Vector2)field.GetValue(obj);
                        nf = GetVector(nf, effect);
                        field.SetValue(obj, nf);
                    }
                    else if (field.FieldType == typeof(NetRectangle))
                    {
                        var nf = (NetRectangle)field.GetValue(obj);
                        nf.Value = GetRectangle(nf.Value, effect);
                    }
                    else if (field.FieldType == typeof(Rectangle))
                    {
                        string[] split = ((string)effect.Value).Split(',');
                        var nf = (Rectangle)field.GetValue(obj);
                        nf = GetRectangle(nf, effect);
                        field.SetValue(obj, nf);
                    }
                    else if (field.FieldType == typeof(NetColor))
                    {
                        var nf = (NetColor)field.GetValue(obj);
                        nf.Value = GetColor(nf.Value, effect);
                    }
                    else if (field.FieldType == typeof(Color))
                    {
                        var nf = (Color)field.GetValue(obj);
                        nf = GetColor(nf, effect);
                        field.SetValue(obj, nf);
                    }
                    else if (field.FieldType == typeof(NetString))
                    {
                        var nf = (NetString)field.GetValue(obj);
                        nf.Value = GetString(nf.Value, effect);
                    }
                    else if (field.FieldType == typeof(string))
                    {
                        var nf = (string)field.GetValue(obj);
                        nf = GetString(nf, effect);
                        field.SetValue(obj, nf);
                    }
                }
                catch { }
                return;
            }
        }

        private static string GetString(string i, SpellEffect effect)
        {
            return effect.ChangeType switch
            {
                FieldChangeType.Add => i + (string)effect.Value,
                FieldChangeType.Prefix => (string)effect.Value + i,
                FieldChangeType.Subtract => i.Replace((string)effect.Value, ""),
                FieldChangeType.Replace => i.Replace(((IEnumerable<string>)effect.Value).First(), ((IEnumerable<string>)effect.Value).Last()),
                FieldChangeType.Set => (string)effect.Value,
                _ => i
            };
        }

        private static Color GetColor(Color i, SpellEffect effect)
        {
            return effect.ChangeType switch
            {
                FieldChangeType.Multiply => i * (float)effect.Value,
                FieldChangeType.Set => Utility.StringToColor((string)effect.Value) ?? i,
                _ => i
            };
        }

        private static Rectangle GetRectangle(Rectangle i, SpellEffect effect)
        {
            string[] split = ((string)effect.Value).Split(',');
            return effect.ChangeType switch
            {
                FieldChangeType.Add => new Rectangle(i.X + int.Parse(split[0]), i.Y + int.Parse(split[1]), i.Width + int.Parse(split[2]), i.Height + int.Parse(split[3])),
                FieldChangeType.Subtract => new Rectangle(i.X - int.Parse(split[0]), i.Y - int.Parse(split[1]), i.Width - int.Parse(split[2]), i.Height - int.Parse(split[3])),
                FieldChangeType.Multiply => new Rectangle(Convert.ToInt32(Math.Round(i.X * float.Parse(split[1], NumberStyles.Any, CultureInfo.InvariantCulture))), Convert.ToInt32(Math.Round(i.Y * float.Parse(split[1], NumberStyles.Any, CultureInfo.InvariantCulture))), Convert.ToInt32(Math.Round(i.Width * float.Parse(split[2], NumberStyles.Any, CultureInfo.InvariantCulture))), Convert.ToInt32(Math.Round(i.Height * float.Parse(split[3], NumberStyles.Any, CultureInfo.InvariantCulture)))),
                FieldChangeType.Divide => new Rectangle(Convert.ToInt32(Math.Round(i.X / float.Parse(split[1], NumberStyles.Any, CultureInfo.InvariantCulture))), Convert.ToInt32(Math.Round(i.Y / float.Parse(split[1], NumberStyles.Any, CultureInfo.InvariantCulture))), Convert.ToInt32(Math.Round(i.Width / float.Parse(split[2], NumberStyles.Any, CultureInfo.InvariantCulture))), Convert.ToInt32(Math.Round(i.Height / float.Parse(split[3], NumberStyles.Any, CultureInfo.InvariantCulture)))),
                FieldChangeType.Set => new Rectangle(int.Parse(split[0]), int.Parse(split[1]), int.Parse(split[2]), int.Parse(split[3])),
                _ => i
            };
        }

        private static Vector2 GetVector(Vector2 i, SpellEffect effect)
        {
            string[] split = ((string)effect.Value).Split(',');
            var v = new Vector2(float.Parse(split[0], NumberStyles.Any, CultureInfo.InvariantCulture), float.Parse(split[1], NumberStyles.Any, CultureInfo.InvariantCulture));
            return effect.ChangeType switch
            {
                FieldChangeType.Add => i + v,
                FieldChangeType.Subtract => i - v,
                FieldChangeType.Multiply => new Vector2(i.X * v.X, i.Y * v.Y),
                FieldChangeType.Divide => new Vector2(i.X / v.X, i.Y / v.Y),
                FieldChangeType.Set => v,
                _ => i
            };
        }
        private static Point GetPoint(Point i, SpellEffect effect)
        {
            string[] split = ((string)effect.Value).Split(',');
            return effect.ChangeType switch
            {
                FieldChangeType.Add => i + new Point(int.Parse(split[0]), int.Parse(split[1])),
                FieldChangeType.Subtract => i - new Point(int.Parse(split[0]), int.Parse(split[1])),
                FieldChangeType.Multiply => new Point(Convert.ToInt32(Math.Round(i.X * float.Parse(split[1], NumberStyles.Any, CultureInfo.InvariantCulture))), Convert.ToInt32(Math.Round(i.Y * float.Parse(split[1], NumberStyles.Any, CultureInfo.InvariantCulture)))),
                FieldChangeType.Divide => new Point(Convert.ToInt32(Math.Round(i.X / float.Parse(split[1], NumberStyles.Any, CultureInfo.InvariantCulture))), Convert.ToInt32(Math.Round(i.Y / float.Parse(split[1], NumberStyles.Any, CultureInfo.InvariantCulture)))),
                FieldChangeType.Set => new Point(int.Parse(split[0]), int.Parse(split[1])),
                _ => i
            };
        }

        private static long GetNumber(long i, SpellEffect effect)
        {
            return effect.ChangeType switch
            {
                FieldChangeType.Add => i + (long)effect.Value,
                FieldChangeType.Subtract => i - (long)effect.Value,
                FieldChangeType.Multiply => (long)Math.Round(i * (float)effect.Value),
                FieldChangeType.Divide => (long)Math.Round(i / (float)effect.Value),
                FieldChangeType.Set => (long)effect.Value,
                _ => i
            };
        }
        private static double GetDecimal(double i, SpellEffect effect)
        {
            return effect.ChangeType switch
            {
                FieldChangeType.Add => i + (float)effect.Value,
                FieldChangeType.Subtract => i - (float)effect.Value,
                FieldChangeType.Multiply => i * (float)effect.Value,
                FieldChangeType.Divide => i / (float)effect.Value,
                FieldChangeType.Set => (float)effect.Value,
                _ => i
            };
        }
        private static bool GetBool(bool i, SpellEffect effect)
        {
            return effect.ChangeType switch
            {
                FieldChangeType.Set => (bool)effect.Value,
                FieldChangeType.Toggle => !i,
                _ => i
            };
        }
        private void CreateTutorials()
        {
            var iTutorialAPI = SHelper.ModRegistry.GetApi<ITutorialAPI>("aedenthorn.Tutorials");

            int frameHeight = 720;
            int seqLength = frameHeight / 3;
            int seqSide = seqLength / 2; // (int)Math.Round(seqLength / Math.Sqrt(2));
            int frameRate = 60;
            string catId = "aedenthorn.AreaOfEffect/aoe-cat";
            string catName = SHelper.Translation.Get("aoe-cat");
            string spellPrefix = SHelper.Translation.Get("aoe-spell-prefix");
            foreach (var kvp in SpellDict)
            {
                if (kvp.Value.AddTutorial == true)
                {

                    List<Vector2> offsets = new()
                    {
                        Vector2.Zero
                    };

                    int left = 0;
                    int top = 0;
                    int right = 0;
                    int bottom = 0;
                    int xpos = 0; 
                    int ypos = 0;
                    foreach (var s in kvp.Value.Sequence)
                    {
                        var l = s switch
                        {
                            SpellDirection.Left => seqLength,
                            SpellDirection.UpLeft => seqSide,
                            SpellDirection.DownLeft => seqSide,
                            _ => 0
                        };
                        var r = s switch
                        {
                            SpellDirection.Right => seqLength,
                            SpellDirection.UpRight => seqSide,
                            SpellDirection.DownRight => seqSide,
                            _ => 0
                        };
                        var u = s switch
                        {
                            SpellDirection.Up => seqLength,
                            SpellDirection.UpRight => seqSide,
                            SpellDirection.UpLeft => seqSide,
                            _ => 0
                        };
                        var d = s switch
                        {
                            SpellDirection.Down => seqLength,
                            SpellDirection.DownRight => seqSide,
                            SpellDirection.DownLeft => seqSide,
                            _ => 0
                        };
                        xpos += r;
                        xpos -= l;
                        ypos += d;
                        ypos -= u;
                        if(xpos < left)
                        {
                            left = xpos;
                        }
                        if(xpos > right)
                        {
                            right = xpos;
                        }
                        if(ypos < top)
                        {
                            top = ypos;
                        }
                        if(ypos > bottom)
                        {
                            bottom = ypos;
                        }
                        offsets.Add(new(xpos, ypos));
                    }
                    var width = right - left;
                    var height = bottom - top;
                    Vector2 origin = new(-left, -top);

                    var frameWidth = frameHeight * 16 / 9;
                    var tw = frameWidth * kvp.Value.Sequence.Count;
                    var th = frameHeight;

                    Rectangle bounds = new(frameWidth / 2 - width / 2, frameHeight / 2 - height / 2, width, height);

                    if(!TextureDict.TryGetValue(texturePrefix + kvp.Key, out var tex))
                    {
                        tex = new Texture2D(Game1.graphics.GraphicsDevice, tw, th);
                        TextureDict[texturePrefix + kvp.Key] = tex;
                    }
                    RenderTarget2D renderTarget = new RenderTarget2D(Game1.graphics.GraphicsDevice, tw, th);
                    renderTarget.SetData(new Color[tw * th]);
                    Rectangle destinationRectangle = new Rectangle(0, 0, tw, th);

                    Game1.graphics.GraphicsDevice.SetRenderTarget(renderTarget);

                    var renderBatch = new SpriteBatch(Game1.graphics.GraphicsDevice);

                    renderBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, null);
                    int distance = 5;
                    int amount = seqLength / distance; 
                    for(int k = 0; k < kvp.Value.Sequence.Count; k++)
                    {
                        //renderBatch.Draw(Game1.staminaRect, new Rectangle(bounds.X + frameWidth * k, bounds.Y, bounds.Width, bounds.Height), null, Color.White, 0f, Vector2.Zero, SpriteEffects.None, 1f);
                        var drawPos = new Vector2(frameWidth * k + bounds.X, bounds.Y) + origin;
                        for(int i = 0; i <= k; i++)
                        {
                            for (int j = 0; j < amount; j++)
                            {
                                renderBatch.Draw(Game1.mouseCursors, drawPos + Vector2.Lerp(offsets[i], offsets[i + 1], j / (float)amount) - new Vector2(15, 15), new Rectangle(88, 1779, 30, 30), GetDirectionColor(Convert.ToInt32(kvp.Value.Sequence[i])), 0, Vector2.Zero, 1f, SpriteEffects.None, 1f);
                            }
                        }
                        renderBatch.Draw(Game1.mouseCursors, drawPos + offsets[k + 1], new Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 0, 16, 16)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
                    }
                    renderBatch.End();

                    Game1.graphics.GraphicsDevice.SetRenderTarget(null);
                    Color[] data = new Color[renderTarget.Width * renderTarget.Height];
                    renderTarget.GetData(data);
                    tex.SetData(data);
                    //Stream fstream = File.Create(Path.Combine(SHelper.DirectoryPath, kvp.Key+".png"));
                    //tex.SaveAsPng(fstream, tex.Width, tex.Height);
                    //continue;
                    var tdata = new TutorialData()
                    {
                        Title = spellPrefix + kvp.Value.DisplayName,
                        Category = catId,
                        Frames = new()
                        {
                            new TutorialFrame()
                            {
                                StartRect = new Rectangle(0, 0, frameWidth, frameHeight),
                                Frames = kvp.Value.Sequence.Count,
                                FrameRate = frameRate,
                                Texture = texturePrefix + kvp.Key,
                                Text = string.Format(SHelper.Translation.Get("to-cast-x-y"), kvp.Value.DisplayName, GetSequenceString(kvp.Value.Sequence))
                            }
                        }
                    };
                    iTutorialAPI.AddTutorial("aedenthorn.AreaOfEffect/spells/" + kvp.Key, tdata);
                }
            }
            var tutorial = new TutorialData()
            {
                Title = SHelper.Translation.Get("aoe-tutorial"),
                Category = catId,
                Frames = new()
                {
                    new TutorialFrame()
                    {
                        Texture = internalPrefix + "1",
                        Text = SHelper.Translation.Get("tutorial-1"),

                    },
                    new TutorialFrame()
                    {
                        Texture = internalPrefix + "2",
                        Text = SHelper.Translation.Get("tutorial-2"),

                    },
                    new TutorialFrame()
                    {
                        Texture = internalPrefix + "fireball",
                        StartRect = new Rectangle(0, 0, 1970, 1109),
                        Text = SHelper.Translation.Get("tutorial-3"),
                        Frames = 4,
                        FrameRate = 60
                    },
                    new TutorialFrame()
                    {
                        Texture = internalPrefix + "3",
                        StartRect = new Rectangle(0, 0, 1499, 843),
                        Text = SHelper.Translation.Get("tutorial-4"),
                        Frames = 2,
                        FrameRate = 60
                    }
                }
            };
            iTutorialAPI.AddCategory(catId, catName);
            iTutorialAPI.AddTutorial("aedenthorn.AreaOfEffect/basics", tutorial);
            iTutorialAPI.AddTutorialTrigger("(T)aedenthorn.AreaOfEffect/wand2", "aedenthorn.AreaOfEffect/basics", null, new() { catId });

        }
        public static Color GetDirectionColor(int i)
        {
            return i switch
            {
                0 => Color.Red,
                1 => Color.Orange,
                2 => Color.Yellow,
                3 => Color.Lime,
                4 => Color.Cyan,
                5 => new Color(0, 100, 255),
                6 => new Color(152, 96, 255),
                7 => new Color(255, 100, 255),
                _ => Color.White,
            };
        }
        public static string GetSequenceString(List<SpellDirection> directions)
        {
            return string.Join(", ", directions.Select(d => SHelper.Translation.Get(d.ToString())));
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
    }
}