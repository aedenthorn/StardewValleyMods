using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ZombieOutbreak
{
    internal class Utils
    {
        private static IModHelper helper;
        private static IMonitor monitor;
        private static ModConfig config;

        public static void Initialize(IModHelper _helper, IMonitor _monitor, ModConfig _config)
        {
            helper = _helper;
            monitor = _monitor;
            config = _config;
        }
        internal static void MakeZombieSpeak(ref string dialogue, bool dontAngry = false)
        {
            monitor.Log($"input {dialogue}");

            string[] strs = dialogue.Split('#');
            for(int i = 0; i < strs.Length; i++)
            {
                string str = strs[i];
                if (str.StartsWith("$") || str.StartsWith("%") || str.StartsWith("[") || str.Length == 0)
                    continue;
                if (i > 0 && strs[i - 1].StartsWith("$r ") && !ModEntry.playerZombies.ContainsKey(Game1.player.uniqueMultiplayerID))
                    continue;

                Regex x1 = new Regex(@"\$[hsuln]");
                Regex x2 = new Regex(@"\$neutral");
                Regex x3 = new Regex(@"\$[0-9]+");

                Regex r1 = new Regex(@"[AEIOU]");
                Regex r1a = new Regex(@"[aeiou]");
                Regex r2 = new Regex(@"[RSTLNDCM]");
                Regex r2a = new Regex(@"[rstlndcm]");
                Regex r3 = new Regex(@"[GH]");
                Regex r3a = new Regex(@"[gh]");
                Regex r4 = new Regex(@"[BFJKPQVWXYZ]");
                Regex r4a = new Regex(@"[bfjkpqvwxyz]");

                str = x1.Replace(str, "");
                str = x2.Replace(str, "");
                str = x3.Replace(str, "");

                str = r1.Replace(str, "A");
                str = r2.Replace(str, "R");
                str = r3.Replace(str, "GH");
                str = r4.Replace(str, "A");

                str = r1a.Replace(str, "a");
                str = r2a.Replace(str, "r");
                str = r3a.Replace(str, "gh");
                str = r4a.Replace(str, "a");

                if(!dontAngry && (i == 0 || !strs[i - 1].StartsWith("$r ")))
                    str += "$a";
                strs[i] = str;
            }

            dialogue = string.Join("#", strs);

            monitor.Log($"zombified: {dialogue}");

        }

        internal static void InfectNPC(string arg1, string[] arg2)
        {
            if (arg2.Any() && ModEntry.villagerNames.Contains(arg2[0]))
                AddZombie(arg2[0]);
        }
        internal static void InfectPlayer(string arg1, string[] arg2)
        {
            AddZombiePlayer(Game1.player.uniqueMultiplayerID);
        }

        internal static void CheckForInfection()
        {
            if (ModEntry.villagerNames == null)
                return;

            //monitor.Log($"checking for infections. zombies: {ModEntry.zombieTextures.Count} player zombies: {ModEntry.playerZombies.Count}");

            if (ModEntry.playerZombies.ContainsKey(Game1.player.uniqueMultiplayerID))
            {
                Game1.buffsDisplay.otherBuffs.RemoveAll(b => b.source == "zombification");
                Buff buff = new Buff(helper.Translation.Get("zombification"), 1000, "zombification", 13);
                Game1.buffsDisplay.addOtherBuff(buff);
                buff = new Buff(helper.Translation.Get("zombification"), 1000, "zombification", 14);
                Game1.buffsDisplay.addOtherBuff(buff);
                buff = new Buff(helper.Translation.Get("zombification"), 1000, "zombification", 18);
                Game1.buffsDisplay.addOtherBuff(buff);
            }

            foreach (string name in ModEntry.villagerNames)
            {
                if (ModEntry.zombieTextures.ContainsKey(name))
                {
                    NPC npc = Game1.getCharacterFromName(name);
                    if (npc == null)
                        continue;
                    foreach (NPC onpc in npc.currentLocation.characters.Where(
                            n => ModEntry.villagerNames.Contains(n.Name) 
                            && !ModEntry.zombieTextures.ContainsKey(n.Name)
                            && !ModEntry.curedNPCs.Contains(n.Name)
                            && Vector2.Distance(n.position, npc.position) < config.InfectionDistance 
                            && Game1.random.NextDouble() < config.InfectionChancePerSecond
                        )
                    )
                    {
                        monitor.Log($"{name} turned {onpc.Name} into a zombie! (distance {Vector2.Distance(onpc.position, npc.position)})");
                        AddZombie(onpc.Name);
                    }
                    foreach (Farmer f in Game1.getAllFarmers().Where(f => 
                            !ModEntry.playerZombies.ContainsKey(f.uniqueMultiplayerID) 
                            && f.currentLocation == npc.currentLocation 
                            && !ModEntry.curedFarmers.Contains(f.uniqueMultiplayerID)
                            && Vector2.Distance(f.position, npc.position) < config.InfectionDistance
                            && Game1.random.NextDouble() < config.InfectionChancePerSecond
                        )
                    )
                    {
                        monitor.Log($"{npc.name} turned farmer {f.Name} into a zombie! (distance {Vector2.Distance(npc.position, npc.position)})");
                        AddZombiePlayer(f.uniqueMultiplayerID);
                    }
                }
                else if (ModEntry.playerZombies.Count > 0)
                {
                    NPC npc = Game1.getCharacterFromName(name);
                    if (npc == null)
                        continue;
                    foreach (Farmer f in Game1.getAllFarmers().Where(
                            f => ModEntry.playerZombies.ContainsKey(f.uniqueMultiplayerID) 
                            && f.currentLocation == npc.currentLocation
                            && Vector2.Distance(f.position, npc.position) < config.InfectionDistance 
                            && Game1.random.NextDouble() < config.InfectionChancePerSecond
                        )
                    )
                    {
                        monitor.Log($"farmer {f.name} turned {npc.Name} into a zombie! (distance {Vector2.Distance(f.position, npc.position)})");
                        AddZombie(npc.Name);
                    }
                }
            }
            if (ModEntry.playerZombies.Count > 0)
            {
                foreach (Farmer f1 in Game1.getAllFarmers().Where(f => ModEntry.playerZombies.ContainsKey(f.uniqueMultiplayerID)))
                {
                    foreach (Farmer f2 in Game1.getAllFarmers().Where(f =>
                            !ModEntry.playerZombies.ContainsKey(f.uniqueMultiplayerID)
                            && f.currentLocation == f1.currentLocation
                            && !ModEntry.curedFarmers.Contains(f.uniqueMultiplayerID)
                            && Vector2.Distance(f.position, f1.position) < config.InfectionDistance
                            && Game1.random.NextDouble() < config.InfectionChancePerSecond
                        )
                    )
                    {
                        monitor.Log($"farmer {f1.name} turned {f2.Name} into a zombie! (distance {Vector2.Distance(f1.position, f2.position)})");
                        AddZombie(f2.Name);
                    }
                }
            }
        }

        internal static void MakeRandomZombie()
        {
            monitor.Log($"Adding random zombie");
            string name = ModEntry.villagerNames.ElementAt(Game1.random.Next(ModEntry.villagerNames.Count()));
            AddZombie(name);
        }

        public static void AddZombie(string name)
        {
            if (ModEntry.curedNPCs.Contains(name))
            {
                monitor.Log($"{name} is immune to zombification today");
                return;
            }

            List<string> zombies = helper.Data.ReadSaveData<List<string>>("zombies") ?? new List<string>();
            if (zombies.Contains(name))
                return;
            zombies.Add(name);
            helper.Data.WriteSaveData("zombies", zombies);
            MakeZombieTexture(name);
            monitor.Log($"{name} turned into a zombie!");
        }

        public static void RemoveZombie(string name)
        {
            List<string> zombies = helper.Data.ReadSaveData<List<string>>("zombies") ?? new List<string>();
            zombies.Remove(name);
            helper.Data.WriteSaveData("zombies", zombies);
            ModEntry.zombieTextures.Remove(name);
            ModEntry.zombiePortraits.Remove(name);
            ModEntry.curedNPCs.Add(name);
            NPC npc = Game1.getCharacterFromName(name);
            helper.Reflection.GetField<Texture2D>(npc.sprite.Value, "spriteTexture").SetValue(helper.Content.Load<Texture2D>($"Characters/{name}", ContentSource.GameContent));
            helper.Content.InvalidateCache($"Portraits/{name}");
            monitor.Log($"{name} was cured of zombification!");
        }
        public static void AddZombiePlayer(long id)
        {
            if (ModEntry.curedFarmers.Contains(id))
            {
                monitor.Log($"{id} is immune to zombification today");
                return;
            }

            List<long> zombiePlayers = helper.Data.ReadSaveData<List<long>>("zombiePlayers") ?? new List<long>();
            if (zombiePlayers.Contains(id))
                return;
            zombiePlayers.Add(id);
            helper.Data.WriteSaveData("zombiePlayers", zombiePlayers);
            MakeZombiePlayer(id);
            monitor.Log($"player {id} turned into a zombie!");
        }

        public static void RemoveZombiePlayer(long id)
        {
            List<long> zombiePlayers = helper.Data.ReadSaveData<List<long>>("zombiePlayers") ?? new List<long>();
            zombiePlayers.Remove(id);
            helper.Data.WriteSaveData("zombiePlayers", zombiePlayers);
            ModEntry.playerZombies.Remove(id);
            ModEntry.curedFarmers.Add(id);
            Farmer f = Game1.getFarmer(id);
            helper.Content.InvalidateCache(asset => asset.DataType == typeof(Texture2D) && asset.AssetName.Contains("Farmer"));
            monitor.Log($"player {f.name} was cured of zombification!");
        }


        internal static void MakeZombieTexture(string name)
        {
            //monitor.Log($"editing color data");

            NPC npc = Game1.getCharacterFromName(name);
            if (npc == null)
            {
                monitor.Log($"Error getting character from name {name}", LogLevel.Error);
                return;
            }
            Texture2D texture = npc.Sprite.Texture;
            Texture2D texture2 = npc.Portrait;

            Color[] data = new Color[texture.Width * texture.Height];
            Color[] data2 = new Color[texture2.Width * texture2.Height];
            texture.GetData(data);
            texture2.GetData(data2);
            float green = Math.Min(1, Math.Max(0, config.GreenAmount));
            for (int i = 0; i < data.Length || i < data2.Length; i++)
            {
                if (i < data.Length && data[i] != Color.Transparent)
                {
                    data[i].R -= (byte)Math.Round(data[i].R * green / 2);
                    data[i].B -= (byte)Math.Round(data[i].B * green);
                    //data[i].G += (byte)Math.Round((255 - data[i].G) * green / 2f);
                }
                if (i < data2.Length && data2[i] != Color.Transparent)
                {
                    data2[i].R -= (byte)Math.Round(data2[i].R * green / 2);
                    data2[i].B -= (byte)Math.Round(data2[i].B * green);
                    //data2[i].G += (byte)Math.Round((255 - data2[i].G) * green / 2f);
                }
            }
            texture.SetData(data);
            texture2.SetData(data2);
            ModEntry.zombieTextures[name] = texture;
            ModEntry.zombiePortraits[name] = texture2;
        }
        internal static void MakeZombiePlayer(long z)
        {
            Farmer f = Game1.getFarmer(z);
            if (f == null)
            {
                monitor.Log($"Error getting famer from id {z}", LogLevel.Error);
                return;
            }
            Texture2D textureRef = helper.Reflection.GetField<Texture2D>(f.FarmerRenderer, "baseTexture").GetValue();
            if (textureRef == null)
            {
                monitor.Log($"farmer {f.Name} has no texture", LogLevel.Error);
                return;
            }

            Texture2D texture = new Texture2D(Game1.graphics.GraphicsDevice, textureRef.Width,textureRef.Height);
            Color[] data = new Color[textureRef.Width * textureRef.Height];
            textureRef.GetData(data);

            ModEntry.playerSprites[z] = texture;

            float green = Math.Min(1, Math.Max(0, config.GreenAmount));
            for (int i = 0; i < data.Length; i++)
            {
                if (i < data.Length && data[i] != Color.Transparent)
                {
                    data[i].R -= (byte)Math.Round(data[i].R * green / 2);
                    data[i].B -= (byte)Math.Round(data[i].B * green);
                    //data[i].G += (byte)Math.Round((255 - data[i].G) * green / 2f);
                }
            }
            texture.SetData(data);
            ModEntry.playerZombies[z] = texture;

        }

    }
}