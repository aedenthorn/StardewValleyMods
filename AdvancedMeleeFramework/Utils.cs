using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Enchantments;
using StardewValley.GameData.Weapons;
using StardewValley.Monsters;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AdvancedMeleeFramework
{
    internal static class Utils
    {
        private static ModEntry ctx;

        public const int CombatSkill = 4;

        internal static void Initialize(ModEntry context) => ctx = context;

        public static void AddEnchantments(MeleeWeapon weapon)
        {
            AdvancedMeleeWeapon amw = GetAdvancedMeleeWeapon(weapon);
            if (amw is null || amw.enchantments.Count <= 0)
                return;
            weapon.enchantments.Clear();

            foreach (var enchtantData in amw.enchantments)
            {
                var enchtant = GetEnchantmentFromDataType(enchtantData);
                if (enchtant is null)
                {
                    ctx.Monitor.Log($"No suitable enchtantment found for type {enchtantData.type}", LogLevel.Error);
                    ctx.Monitor.Log($"Currently registered advanced enchantments are: {string.Join(", ", ctx.AdvancedEnchantmentCallbacks.Keys)}");
                    continue;
                }
                weapon.enchantments.Add(enchtant);
            }
        }

        public static AdvancedMeleeWeapon? GetAdvancedMeleeWeapon(MeleeWeapon mw, Farmer? who = null)
        {
            AdvancedMeleeWeapon? amw = null;
            int skillLevel = -1;
            if (ctx.AdvancedMeleeWeapons.ContainsKey(mw.ItemId))
            {
                foreach (var weapon in ctx.AdvancedMeleeWeapons[mw.ItemId])
                {
                    if (who is null || (weapon.skillLevel <= who.getEffectiveSkillLevel(CombatSkill) && weapon.skillLevel > skillLevel))
                    {
                        skillLevel = weapon.skillLevel;
                        amw = weapon;
                    }
                }

                if (amw is not null)
                    return amw;
            }
            if (ctx.AdvancedMeleeWeaponsByType.ContainsKey(mw.type.Value) && who is not null)
            {
                skillLevel = -1;
                foreach (var weapon in ctx.AdvancedMeleeWeaponsByType[mw.type.Value])
                {
                    if (weapon.skillLevel <= who.getEffectiveSkillLevel(CombatSkill) && weapon.skillLevel > skillLevel)
                    {
                        skillLevel = weapon.skillLevel;
                        amw = weapon;
                    }
                }
            }
            return amw;
        }

        public static BaseWeaponEnchantment? GetEnchantmentFromDataType(AdvancedEnchantmentData data)
        {
            BaseWeaponEnchantment? enchant = data.type.ToLower() switch
            {
                "vampiric" => new VampiricEnchantment(),
                "haymaker" => new HaymakerEnchantment(),
                "bugkiller" => new BugKillerEnchantment(),
                "crusaded" => new CrusaderEnchantment(),
                "magic" => new MagicEnchantment(),
                "jade" => new JadeEnchantment(),
                "aquamarine" => new AquamarineEnchantment(),
                "topaz" => new TopazEnchantment(),
                "amethyst" => new AmethystEnchantment(),
                "ruby" => new RubyEnchantment(),
                "emerald" => new EmeraldEnchantment(),
                _ => null
            };
            if (enchant is not null)
                return enchant;
            if (!ctx.AdvancedEnchantmentCallbacks.ContainsKey(data.type))
                return null;
            enchant = new();
            ctx.Helper.Reflection.GetField<string>(enchant, "_displayName").SetValue(data.name);
            return enchant;
        }

        public static void LoadAdvancedMeleeWeapons()
        {
            ctx.AdvancedMeleeWeapons.Clear();
            ctx.AdvancedMeleeWeaponsByType[1].Clear();
            ctx.AdvancedMeleeWeaponsByType[2].Clear();
            ctx.AdvancedMeleeWeaponsByType[3].Clear();

            var weaponData = ctx.Helper.GameContent.Load<Dictionary<string, WeaponData>>("Data\\Weapons");

            foreach (var contentPack in ctx.Helper.ContentPacks.GetOwned())
            {
                ctx.Monitor.Log($"Reading content pack: {contentPack.Manifest.Name} {contentPack.Manifest.Version}", LogLevel.Info);
                ctx.Monitor.Log($"[{contentPack.Manifest.Name}] build for version {contentPack.Manifest.ContentPackFor.MinimumVersion}");
                try
                {
                    List<AdvancedMeleeWeapon> weapons = ReadContentPack(contentPack);
                    if (weapons is null || weapons.Count <= 0)
                        continue;
                    Dictionary<string, object> config = ReadContentPackConfig(contentPack) ?? [];

                    var AdvancedMeleeWeaponType = typeof(AdvancedMeleeWeapon);
                    var MeleeActionFrameTyep = typeof(MeleeActionFrame);
                    var AdvancedWeaponProjectileType = typeof(AdvancedWeaponProjectile);

                    foreach (var weapon in weapons)
                    {
                        WriteConfigFor(weapon, weapon.config, config);
                        foreach (var frame in weapon.frames)
                        {
                            WriteConfigFor(frame, frame.config, config);
                            foreach (var projectile in frame.projectiles)
                                WriteConfigFor(projectile, projectile.config, config);
                            if (frame.special is not null)
                                WriteConfigFor(frame.special, frame.special.config, config);
                        }
                        foreach (var enchantment in weapon.enchantments)
                        {
                            WriteConfigFor(enchantment, enchantment.config, config);
                            ctx.AdvancedEnchantments[enchantment.name] = enchantment;
                        }
                        if (config.Count > 0)
                            contentPack.WriteJsonFile("config.json", config);

                        if (weapon.type < 1 || weapon.type > 3)
                        {
                            ctx.Monitor.Log($"Found unrecognized weapon type ({weapon.type}), trying to read from id ({weapon.id})");
                            string id;
                            try
                            {
                                id = weaponData.First(x => x.Key == weapon.id).Key;
                                ctx.Monitor.Log($"Found weapon by item id");
                            }
                            catch
                            {
                                try
                                {
                                    id = weaponData.First(x => x.Value.Name == weapon.id).Key;
                                    ctx.Monitor.Log($"Found weapon by name");
                                }
                                catch
                                {
                                    id = ctx.JsonAssetsApi?.GetWeaponId(weapon.id);
                                    if (string.IsNullOrWhiteSpace(id))
                                    {
                                        ctx.Monitor.Log($"Could not read weapon id {weapon.id}", LogLevel.Error);
                                        continue;
                                    }
                                    ctx.Monitor.VerboseLog($"Found weapon in json assets (given id: {weapon.id} -> JA id: {id})");
                                }
                            }
                            if (!ctx.AdvancedMeleeWeapons.ContainsKey(id))
                                ctx.AdvancedMeleeWeapons.Add(id, []);
                            ctx.AdvancedMeleeWeapons[id].Add(weapon);
                            continue;
                        }
                        ctx.AdvancedMeleeWeaponsByType[weapon.type].Add(weapon);
                    }
                }
                catch (Exception ex)
                {
                    ctx.Monitor.Log($"Could not read content.json file for content pack {contentPack.Manifest.Name}", LogLevel.Error);
                    ctx.Monitor.Log($"{ex.GetType().Name} - {ex.Message}\n{ex.StackTrace}");
                }
            }

            ctx.Monitor.VerboseLog($"Total advanced melee weapons: {ctx.AdvancedMeleeWeapons.Count}");
            ctx.Monitor.VerboseLog($"Total advanced melee dagger attacks: {ctx.AdvancedMeleeWeaponsByType[1].Count}");
            ctx.Monitor.VerboseLog($"Total advanced melee club attacks: {ctx.AdvancedMeleeWeaponsByType[2].Count}");
            ctx.Monitor.VerboseLog($"Total advanced melee sword attacks: {ctx.AdvancedMeleeWeaponsByType[3].Count}");
        }

        private static List<AdvancedMeleeWeapon>? ReadContentPack(IContentPack contentPack)
        {
            try
            {
                var weapons = contentPack.ReadJsonFile<AdvancedMeleeWeaponData>("content.json")?.weapons ?? contentPack.ReadJsonFile<List<AdvancedMeleeWeapon>>("content.json");
                return weapons;
            }
            catch (Exception ex2)
            {
                ctx.Monitor.Log($"Could not read content.json file for content pack", LogLevel.Error);
                ctx.Monitor.Log($"{ex2.GetType().Name} - {ex2.Message}\n{ex2.StackTrace}");
                return null;
            }
        }

        private static Dictionary<string, object>? ReadContentPackConfig(IContentPack contentPack)
        {
            try
            {
                var config = contentPack.ReadJsonFile<Dictionary<string, object>>("config.json");
                return config;
            }
            catch
            {
                try
                {
                    var legacyConfig = contentPack.ReadJsonFile<WeaponPackConfigData>("config.json");
                    if (legacyConfig is null)
                    {
                        ctx.Monitor.Log($"Could not read config.json file for content pack", LogLevel.Error);
                        return null;
                    }
                    contentPack.WriteJsonFile("config.json", legacyConfig.variables);
                    ctx.Monitor.Log($"Found legacy config format for content pack, the config has automatically migrated to the new format", LogLevel.Warn);
                    return legacyConfig.variables;
                }
                catch (Exception ex2)
                {
                    ctx.Monitor.Log($"Could not read config.json file for content pack", LogLevel.Error);
                    ctx.Monitor.Log($"{ex2.GetType().Name} - {ex2.Message}\n{ex2.StackTrace}");
                    return null;
                }
            }
        }

        private static void WriteConfigFor<T>(T value, Dictionary<string, string> configuredFields, Dictionary<string, object> config)
        {
            try
            {
                var type = typeof(T);
                foreach (var field in configuredFields)
                {
                    if (type.GetField(field.Key) is not { } fi)
                    {
                        if (type.GetField("parameters") is { } paramF && paramF.GetValue(value) is Dictionary<string, string> param)
                        {
                            WriteConfigForParameter(param, field.Key, type, config);
                            continue;
                        }
                        ctx.Monitor.Log($"Field {field.Key} could not be found for type {type.Name}", LogLevel.Error);
                        continue;
                    }

                    if (config.ContainsKey(field.Key))
                    {
                        if (config[field.Key] is long)
                            fi.SetValue(value, Convert.ToInt32(config[field.Key].ToString()));
                        else
                            fi.SetValue(value, config[field.Key]);
                        continue;
                    }
                    config.Add(field.Key, fi.GetValue(value));
                }
            }
            catch (Exception ex)
            {
                ctx.Monitor.Log($"An error occured when assigning configurable fields", LogLevel.Error);
                ctx.Monitor.Log($"{ex.GetType().Name} - {ex.Message}\n{ex.StackTrace}");
            }
        }

        private static void WriteConfigForParameter(Dictionary<string, string> parameters, string key, Type type, Dictionary<string, object> config)
        {
            try
            {
                if (!parameters.ContainsKey(key))
                {
                    ctx.Monitor.Log($"Field {key} could not be found for parameters of type {type.Name}", LogLevel.Error);
                    return;
                }

                if (config.ContainsKey(key))
                {
                    if (config[key] is long)
                        parameters[key] = Convert.ToInt32(config[key].ToString()).ToString();
                    else
                        parameters[key] = config[key].ToString();
                    return;
                }
                config.Add(key, parameters[key]);
            }
            catch (Exception ex)
            {
                ctx.Monitor.Log($"An error occured when assigning configurable fields for parameters", LogLevel.Error);
                ctx.Monitor.Log($"{ex.GetType().Name} - {ex.Message}\n{ex.StackTrace}");
            }
        }

        public static Vector2 TranslateVector(Vector2 vector, int facingDirection)
        {
            return facingDirection switch
            {
                0 => new(-vector.X, -vector.Y),
                1 => new(vector.Y, -vector.X),
                2 => vector,
                3 => new(-vector.Y, vector.X),
            };
        }
    }
}
