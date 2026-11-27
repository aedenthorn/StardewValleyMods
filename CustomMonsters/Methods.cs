using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Monsters;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CustomMonsters
{
	public partial class ModEntry : Mod
    {
        private static bool TryGetData(Monster m, out MonsterData data)
        {
            if (!Config.ModEnabled || !m.modData.TryGetValue(monsterKey, out var id) || !Monsters.TryGetValue(id, out data))
            {
                data = null;
                return false;
            }
            if(data.MonsterId == null)
            {
                data.MonsterId = id;
            }
            return true;
        }
        public static string ChangeSpawnSound(string value, Monster m)
        {
            return (!TryGetData(m, out var data) || data.SpawnSound == null) ? value : data.SpawnSound;
        }
        public static string ChangeDamageSound(string value, Monster m)
        {
            return (!TryGetData(m, out var data) || data.DamageSound == null) ? value : (value == "rockGolemHit" && m is Bat ? data.DeathSound ?? value : data.DamageSound);
        }
        public static string ChangeDamageSound2(string value, Monster m)
        {
            return (!TryGetData(m, out var data) || data.DamageSound2 == null) ? value : data.DamageSound2;
        }
        public static string ChangeDeathSound(string value, Monster m)
        {
            return (!TryGetData(m, out var data) || data.DeathSound == null) ? value : data.DeathSound;
        }
        public static string ChangeDeathSound2(string value, Monster m)
        {
            return (!TryGetData(m, out var data) || data.DeathSound2 == null) ? value : data.DeathSound2;
        }
        public static string ChangeContactSound(string value, Monster m)
        {
            return (!TryGetData(m, out var data) || data.ContactSound == null) ? value : data.ContactSound;
        }
        public static string ChangeMoveSound(string value, Monster m)
        {
            return (!TryGetData(m, out var data) || data.MoveSound == null) ? value : data.MoveSound;
        }
        public static string ChangeCrumbleSound(string value, Monster m)
        {
            return (!TryGetData(m, out var data) || data.CrumbleSound == null) ? value : data.CrumbleSound;
        }
        public static string ChangeUncrumbleSound(string value, Monster m)
        {
            return (!TryGetData(m, out var data) || data.UncrumbleSound == null) ? value : data.UncrumbleSound;
        }
        public static string ChangeHitSound(string value, Monster m)
        {
            return (!TryGetData(m, out var data) || data.ShellSound == null) ? value : data.ShellSound;
        }
        public static string ChangeBreakSound(string value, Monster m)
        {
            return (!TryGetData(m, out var data) || data.BreakSound == null) ? value : data.BreakSound;
        }
        public static string ChangeContactDebuff(string value, Monster m)
        {
            return (!TryGetData(m, out var data) || data.ContactDebuff == null) ? value : data.ContactDebuff;
        }
        public static int ChangeReviveTimer(int value, Monster m)
        {
            return (!TryGetData(m, out var data) || data.ReviveTimer == null) ? value : data.ReviveTimer.Value;
        }
        public static float ChangeChildhoodLength(float value, Monster m)
        {
            return (!TryGetData(m, out var data) || data.ChildhoodLength == null) ? value : data.ChildhoodLength.Value;
        }
        public static string ChangeSpritePath(string value, Monster m)
        {
            return (!TryGetData(m, out var data) || data.Sprite == null) ? value : data.Sprite;
        }
        public static string ChangeMoveSound2(string value, Monster m)
        {
            return (!TryGetData(m, out var data) || data.MoveSound2 == null) ? value : data.MoveSound2;
        }
        public static string ChangeProjectileSound(string value, Monster m)
        {
            return (!TryGetData(m, out var data) || data.ProjectileSound == null) ? value : data.ProjectileSound;
        }
        public static string ChangeProjectileSound2(string value, Monster m)
        {
            return (!TryGetData(m, out var data) || data.ProjectileSound2 == null) ? value : data.ProjectileSound2;
        }
        public static string ChangeProjectileDebuff(string value, Monster m)
        {
            return (!TryGetData(m, out var data) || data.ProjectileDebuff == null) ? value : data.ProjectileDebuff;
        }
        public static int ChangeProjectileDamage(int value, Monster m)
        {
            return (!TryGetData(m, out var data) || data.ProjectileDamage == null) ? value : data.ProjectileDamage.Value;
        }
        public static int ChangeProjectileCount(int value, Monster m)
        {
            return (!TryGetData(m, out var data) || data.ProjectileCount == null) ? value : data.ProjectileCount.Value;
        }

        public static float ChangeProjectileTimer(float value, Monster m)
        {
            return (!TryGetData(m, out var data) || data.ProjectileTimer == null) ? value : data.ProjectileTimer.Value;
        }
        public static int ChangeProjectileDamage2(int value, DinoMonster.BreathProjectile bp)
        {
            return (!Config.ModEnabled || bp is not CustomBreathProjectile cbp|| cbp.data.ProjectileDamage == null) ? value : cbp.data.ProjectileDamage.Value;
        }
        public static int ChangeProjectileIndex(int value, Monster m)
        {
            return (!TryGetData(m, out var data) || data.ProjectileIndex == null) ? value : data.ProjectileIndex.Value;
        }
        public static string ChangeArmorSound(string value, Monster m)
        {
            return (!TryGetData(m, out var data) || data.ArmorSound == null) ? value : data.ArmorSound;
        }
        public static Color ChangeSprinkleColor(Color value, Monster m)
        {
            return (!TryGetData(m, out var data) || data.SprinkleColor == null) ? value : MakeColor(data.SprinkleColor);
        }
        public static int ChangeLightType(int value, Monster m)
        {
            return (!TryGetData(m, out var data) || data.LightType == -1) ? value : data.LightType;
        }
        public static Color MakeColor(MonsterData data)
        {
            try
            {
                if(data.Colors != null)
                {
                    int maxWeight = data.Colors.Sum(c => c.Chance);
                    int cumWeight = 0;
                    int chance = Game1.random.Next(maxWeight);
                    foreach(var c in data.Colors)
                    {
                        cumWeight += c.Chance;
                        if(chance < cumWeight) 
                        {
                            if(c.Color != null && c.Color.StartsWith("#") && c.Color.Length == 7)
                            {
                                return MakeColor(c.Color);
                            }
                            return new Color((byte)Game1.random.Next(c.R.Min, c.R.Max + 1),(byte)Game1.random.Next(c.G.Min, c.G.Max + 1),(byte)Game1.random.Next(c.B.Min, c.B.Max + 1));
                        }
                    }
                }
                else if(data.Color.StartsWith("#") && data.Color.Length == 7)
                {
                    return MakeColor(data.Color);
                }
            }
            catch
            {
                return Color.White;
            }
            return Color.White;
        }

        private static Color MakeColor(string color)
        {
            return new Color(
                int.Parse(color.Substring(1, 2), System.Globalization.NumberStyles.HexNumber),
                int.Parse(color.Substring(3, 2), System.Globalization.NumberStyles.HexNumber),
                int.Parse(color.Substring(5, 2), System.Globalization.NumberStyles.HexNumber)
            );
        }

        public static Monster GetSpawnMonster(Monster old, string newId, List<DungeonReplaceData> list, int level, int difficulty, Vector2 position)
        {
            if (old == null)
                return null;
            var spawnData = list.FirstOrDefault(m => m.MinLevel <= level && m.MaxLevel >= level && m.Types?.Contains(old.GetType().ToString()) != false && (m.MinDifficulty < 0 || difficulty >= m.MinDifficulty) && (m.MaxDifficulty < 0 || difficulty <= m.MaxDifficulty));
            if (spawnData != default && Game1.random.NextDouble() < spawnData.Chance / 100.0)
            {
                var m = CreateMonster(newId, position);
                return m;
            }
            return null;
        }
        public static void TrySpawnMonsters(string key, MonsterSpawnData spawn, int spawns)
        {
            for(int i = 0; i < spawns; i++)
            {

                var loc = Game1.getLocationFromName(spawn.Location);
                if (loc == null)
                {
                    return;
                }
                int present = loc.characters.Where(c => c is Monster m && m.modData.TryGetValue(monsterKey, out var type) && type == key).Count();
                if (spawn.MaxAmount >= 0 && present >= spawn.MaxAmount)
                {
                    return;
                }
                int toSpawn = Game1.random.Next(spawn.MinSpawn, spawn.MaxSpawn + 1);
                if (spawn.MaxAmount >= 0 && toSpawn > spawn.MaxAmount - present)
                {
                    toSpawn = spawn.MaxAmount - present;
                }
                List<Vector2> spawned = new();
                for (int j = 0; j < toSpawn; j++)
                {
                    Vector2 pos = new Vector2(Game1.random.Next(spawn.MinTile.X, spawn.MaxTile.X + 1), Game1.random.Next(spawn.MinTile.Y, spawn.MaxTile.Y + 1)) * 64;
                    if (spawned.Contains(pos))
                    {
                        continue;
                    }
                    spawned.Add(pos);
                    var m = CreateMonster(key, pos);
                    if (m != null)
                    {
                        loc.characters.Add(m);
                    }
                }
            }
        }

    }
}
