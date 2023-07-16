using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NPCClothing
{
    public partial class ModEntry
    {
        public static string[] ages = new string[] {
            "adult",
            "teen",
            "child"
        };
        public static string[] genders = new string[] {
            "male",
            "female",
            "undefined"
        };

        private static void CheckClothes(IAssetData obj)
        {
            if (!Config.ModEnabled)
                return;
            var split = obj.NameWithoutLocale.Name.Split('/');
            NPC npc = Game1.getCharacterFromName(split[1]);
            if (npc is null)
                return;
            Dictionary<string, List<ClothingData>> slotData = new Dictionary<string, List<ClothingData>>();
            List<ClothingData> toApply = new List<ClothingData>();
            foreach (var kvp in clothingDict)
            {
                if (TryToWear(npc, kvp.Value, split[0] == "Characters"))
                {
                    if (kvp.Value.clothingSlot is null)
                    {
                        toApply.Add(kvp.Value);
                    }
                    else
                    {
                        if (forceWear is not null && kvp.Value.id == forceWear.id)
                        {
                            toApply.Add(kvp.Value);
                        }
                        else if (forceWear is null || forceWear.clothingSlot is null || forceWear.clothingSlot != kvp.Value.clothingSlot)
                        {
                            if (!slotData.ContainsKey(kvp.Value.clothingSlot))
                                slotData[kvp.Value.clothingSlot] = new List<ClothingData>();
                            slotData[kvp.Value.clothingSlot].Add(kvp.Value);
                        }
                    }
                }
            }
            foreach(var list in slotData.Values)
            {
                SMonitor.Log($"Applying random slot clothes out of {list.Count}");

                toApply.Add(list[Game1.random.Next(list.Count)]);
            }
            toApply.Sort(delegate (ClothingData a, ClothingData b) {
                return a.zIndex.CompareTo(b.zIndex);
            });
            foreach(ClothingData data in toApply)
            {
                SMonitor.Log($"Applying clothes {data.id} ({data.clothingSlot}) to {obj.NameWithoutLocale.Name}");
                Color[] colors = ApplyClothes(npc, obj.AsImage().Data, data, split[0] == "Characters");
                obj.AsImage().Data.SetData(colors);
            }
        }
        private static bool TryToWear(NPC npc, ClothingData data, bool sprite)
        {
            if (npc == null)
                return false;
            if (!ClothesFit(npc, data, sprite))
                return false;
            if (data.giftName is not null && data.giftName.Length > 0 && (!npc.modData.TryGetValue(giftKey, out string gifts) || !gifts.Split(',').Contains(data.giftName)))
                return false;
            if (data.percentChance <= Game1.random.Next(100))
                return false;
            return true;
        }
        private static bool ClothesFit(NPC npc, ClothingData data, bool sprite)
        {
            if (npc == null)
                return false;
            if (sprite && string.IsNullOrEmpty(data.spriteTexturePath))
                return false;
            if (!sprite && string.IsNullOrEmpty(data.portraitTexturePath))
                return false;
            if (data.namesAllow is not null && data.namesAllow.Count > 0 && !data.namesAllow.Contains(npc.Name))
                return false;
            if (data.namesForbid is not null && data.namesForbid.Count > 0 && data.namesForbid.Contains(npc.Name))
                return false;
            if (data.gendersAllow is not null && data.gendersAllow.Count > 0 && !data.gendersAllow.Contains(genderList[npc.Gender]))
                return false;
            if (data.agesAllow is not null && data.agesAllow.Count > 0 && !data.agesAllow.Contains(ageList[npc.Age]))
                return false;
            return true;
        }
        private static Color[] ApplyClothes(NPC npc, Texture2D charTexture, ClothingData data, bool sprite)
        {
            string texturePath = sprite ? data.spriteTexturePath : data.portraitTexturePath;
            Texture2D clothesTexture = SHelper.GameContent.Load<Texture2D>(texturePath);
            Color[] clothesColors = new Color[clothesTexture.Width * clothesTexture.Height];
            Color[] charColors = new Color[charTexture.Width * charTexture.Height];
            clothesTexture.GetData(clothesColors);
            charTexture.GetData(charColors);
            skinDict.TryGetValue(npc.Name, out List<Color> skins);
            Point offset = new Point(0, 0);
            var offsets = sprite ? data.spriteOffsets : data.portraitOffsets;
            if (offsets is not null)
            {
                foreach(var d in offsets)
                {
                    if (d.names?.Contains(npc.Name) == true || d.ages?.Contains(ages[npc.Age]) == true || d.genders?.Contains(genders[npc.Gender]) == true)
                    {
                        offset = d.offset;
                        break;
                    }
                }
            }
            int length = (clothesColors.Length <= charColors.Length ? clothesColors.Length : charColors.Length);
            int width = (clothesColors.Length <= charColors.Length ? clothesTexture.Width : charTexture.Width);
            for (int i = 0; i < length; i++)
            {
                int idx = i + offset.Y * width + offset.X;
                if (idx < 0 || idx >= length)
                    continue;
                if (clothesColors[i] != Color.Transparent)
                {
                    if (skins is not null && data.skinColors is not null)
                    {
                        if (data.skinColors.Contains(clothesColors[i]))
                        {
                            int skinidx = data.skinColors.IndexOf(clothesColors[i]);
                            if (skins.Count > skinidx)
                                charColors[idx] = skins[skinidx];
                            continue;
                        }
                    }
                    if (clothesColors[i].A < 255)
                    {
                        if (clothesColors[i].A < 15)
                            charColors[idx] = Color.Transparent;
                        else
                            charColors[idx] = Color.Lerp(charColors[idx], new Color(clothesColors[i].R, clothesColors[i].G, clothesColors[i].B), clothesColors[i].A / 255f);
                    }
                    else
                        charColors[idx] = clothesColors[i];
                }
            }
            return charColors;
        }


        private void MakeHatData()
        {
            Dictionary<int, string> hatDict = Helper.GameContent.Load<Dictionary<int, string>>("Data/hats");
            Dictionary<string, ClothingData> hatData = new Dictionary<string, ClothingData>();
            List<Dictionary<string, string>> cpData = new List<Dictionary<string, string>>();
            Texture2D spriteSheetTexture = Helper.GameContent.Load<Texture2D>("Characters/Farmer/hats");
            Color[] sheetData = new Color[spriteSheetTexture.Width * spriteSheetTexture.Height];
            spriteSheetTexture.GetData(sheetData);
            List<int> forbiddenTall = new List<int>() {
                39,40,42,57,58,59,61
            };
            List<int> forbiddenMed = new List<int>() {
                40,42
            };
            List<int> forbiddenAll = new List<int>() {
                73,74
            };
            List<OffsetData> custom = new();
            var tmp = Path.Combine(Helper.DirectoryPath, "tmp", "custom.txt");
            if (File.Exists(tmp))
            {
                foreach (var s in File.ReadAllLines(tmp))
                {

                }
            }
            var cpJson = new CPJSON();
            for (int i = 0; i < 94; i++)
            {
                if (forbiddenAll.Contains(i))
                    continue;

                bool forbidExtraTall = false;
                bool forbidTall = forbiddenTall.Contains(i);
                bool forbidMed = forbiddenMed.Contains(i);
                string name = hatDict[i].Split('/')[0];
                if (true)
                {
                    Texture2D outTexture = new Texture2D(Game1.graphics.GraphicsDevice, 64, 128);
                    Color[] oneData = new Color[64 * 128];
                    int xi = i % 12;
                    int yi = i / 12;

                    for (int j = 0; j < 4; j++)
                    {
                        int jj = j;
                        if (j == 2)
                            jj = 3;
                        else if (j == 3)
                            jj = 2;
                        for (int k = 0; k < 4; k++)
                        {
                            for (int y = 0; y < 20; y++)
                            {
                                for (int x = 0; x < 16; x++)
                                {
                                    int idx = (y + yi * 80 + 20 * jj) * 240 + x + 2 + xi * 20;
                                    int cidx = (y + k % 2 + 32 * j) * 64 + x + k * 16;

                                    if (y < 2)
                                    {
                                        bool trans = sheetData[idx] == Color.Transparent;
                                        if (!trans)
                                            forbidExtraTall = true;
                                        if (y < 1)
                                        {
                                            if (!trans)
                                                forbidTall = true;
                                        }
                                    }

                                    try
                                    {
                                        oneData[cidx] = sheetData[idx];
                                    }
                                    catch
                                    {
                                        Monitor.Log($"data {cidx} ({oneData.Length}), texdata {idx} ({sheetData.Length})");
                                    }
                                }
                            }
                        }
                    }
                    outTexture.SetData(oneData);
                    FileStream file = File.OpenWrite($"tmp/hat_{i}.png");
                    outTexture.SaveAsPng(file, outTexture.Width, outTexture.Height);
                    file.Close();
                }

                ClothingData clothingData = new ClothingData()
                {
                    id = $"hat_{i}",
                    clothingSlot = "hat",
                    giftName = name,
                    giftReaction = "like",
                    namesAllow = new List<string>(),
                    spriteTexturePath = $"aedenthorn.NCFHatsCP/hat_{i}_sprite", 
                    spriteOffsets = new List<OffsetData>()
                };
                if (!forbidMed)
                {
                    if (!forbidTall)
                    {
                        if (!forbidExtraTall)
                        {
                            clothingData.namesAllow.Add("Demetrius");
                            clothingData.spriteOffsets.Add(new OffsetData()
                            {
                                offset = new Point(0, -3),
                                names = new List<string>() { "Demetrius" }
                            });
                        }
                        clothingData.namesAllow.AddRange(new List<string>() { "Alex", "Clint", "Dick", "Emily", "Gus", "Harvey", "Lewis", "Pierre", "Sam", "Sebastian", "Shane" });
                        clothingData.spriteOffsets.Add(new OffsetData()
                        {
                            offset = new Point(0, -2),
                            names = new List<string>() { "Alex", "Clint", "Dick", "Emily", "Gus", "Harvey", "Lewis", "Pierre", "Sam", "Sebastian", "Shane" }
                        });
                    }
                    clothingData.namesAllow.AddRange(new List<string>() { "Caroline", "Haley", "Jodi", "Leah", "Linus", "Marnie", "Maru", "Pam", "Penny", "Robin", "Sandy" });
                    clothingData.spriteOffsets.Add(new OffsetData()
                    {
                        offset = new Point(0, -1),
                        names = new List<string>() { "Caroline", "Haley", "Jodi", "Leah", "Linus", "Marnie", "Maru", "Pam", "Penny", "Robin", "Sandy" }
                    });
                }

                clothingData.namesAllow.AddRange(new List<string>() { "Abigail", "George", "Evelyn", "Jas", "Vincent" });
                clothingData.spriteOffsets.Add(new OffsetData()
                {
                    offset = new Point(0, 1),
                    names = new List<string>() { "Evelyn" }
                });
                clothingData.spriteOffsets.Add(new OffsetData()
                {
                    offset = new Point(0, 4),
                    names = new List<string>() { "Jas" }
                });
                clothingData.spriteOffsets.Add(new OffsetData()
                {
                    offset = new Point(0, 5),
                    names = new List<string>() { "Vincent" }
                });

                hatData.Add($"hat_{i}", clothingData);
                cpJson.Changes.Add(new Dictionary<string, object>()
                {
                    { "Action", "Load"},
                    { "Target", $"aedenthorn.NCFHatsCP/hat_{i}_sprite"},
                    { "FromFile", $"assets/hat_{i}.png"},
                });
            }
            cpJson.Changes.Add(new Dictionary<string, object>()
            {
                { "Action", "EditData" },
                { "Target", "aedenthorn.NPCClothing/dictionary" },
                { "Entries", hatData },
            });
            string json = JsonConvert.SerializeObject(cpJson, Formatting.Indented);
            File.WriteAllText("tmp/content.json", json);
        }
    }
}