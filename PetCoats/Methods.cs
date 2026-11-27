using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Characters;
using StardewValley.GameData.Pets;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PetCoats
{
    public partial class ModEntry
    {
        public static Texture2D GetTextureSwap(Texture2D texture, Dictionary<Color, Color> swap, Rectangle sourceRect)
        {
            var newTexture = new Texture2D(texture.GraphicsDevice, texture.Width, texture.Height);
            var data = new Color[texture.Width * texture.Height];
            texture.GetData(data);
            for (int i = 0; i < data.Length; i++)
            {
                int x = i % texture.Width;
                int y = i / texture.Width;
                if (!sourceRect.Contains(x, y))
                    continue;
                if(swap.TryGetValue(data[i], out var c))
                {
                    data[i] = c;
                }
            }
            newTexture.SetData(data);
            return newTexture;
        }
        private static Dictionary<string, PetCoatData> GetDataDict()
        {
            if (dataDict != null)
                return dataDict;
            if (Game1.MasterPlayer == null)
                return new Dictionary<string, PetCoatData>();
            dataDict = SHelper.GameContent.Load<Dictionary<string, PetCoatData>>(dictPath);
            foreach (var key in dataDict.Keys)
            {
                if(dataDict[key].Tint != null)
                {
                    dataDict[key].Tint = new Color(dataDict[key].Tint.Value.R, dataDict[key].Tint.Value.G, dataDict[key].Tint.Value.B);
                }
                if(dataDict[key].IconTexturePath != null)
                {
                    dataDict[key].IconTexture = SHelper.GameContent.Load<Texture2D>(dataDict[key].IconTexturePath);
                }
                else if(dataDict[key].RealSwap != null)
                {
                    dataDict[key].IconTexture = GetCoatTexture(dataDict[key].PetType, dataDict[key].PetBreed, true, dataDict[key].RealSwap);
                }
                if (dataDict[key].TexturePath != null)
                {
                    dataDict[key].Texture = SHelper.GameContent.Load<Texture2D>(dataDict[key].TexturePath);
                }
                else if (dataDict[key].RealSwap != null)
                {
                    dataDict[key].Texture = GetCoatTexture(dataDict[key].PetType, dataDict[key].PetBreed, false, dataDict[key].RealSwap);
                }
            }
            return dataDict;
        }

        public static Texture2D GetCoatTexture(string petType, string petBreed, bool icon, Dictionary<Color, Color> swap = null)
        {
            if (!Pet.TryGetData(petType, out var petData))
                return null;
            Texture2D texture = null;
            Rectangle rectangle = new Rectangle();
            foreach (PetBreed breed in petData.Breeds)
            {
                if (breed.Id == petBreed)
                {
                    texture = Game1.content.Load<Texture2D>(icon ? breed.IconTexture : breed.Texture);
                    if (icon)
                    {
                        rectangle = breed.IconSourceRect;
                    }
                    else
                    {
                        rectangle = new Rectangle(0, 0, texture.Width, texture.Height);
                    }
                    break;
                }
            }
            if (texture != null && swap != null)
            {
                texture = GetTextureSwap(texture, swap, rectangle);
            }
            return texture;
        }


        private static string GetPetData(Pet __instance, out PetCoatData data)
        {
            data = null;
            if (!__instance.modData.TryGetValue(modKey, out var coat))
            {
                coat = MasterCoat;
            }
            if (string.IsNullOrEmpty(coat))
                return null;
            var dict = GetDataDict();
            if (dict?.TryGetValue(coat, out data) != true)
            {
                return null;
            }
            if (data.PetType != __instance.petType.Value || data.PetBreed != __instance.whichBreed.Value || data.Texture == null)
            {
                data = null;
                return null;
            }
            return coat;
        }


        private static string ChangePetCoat(int change, string whichPetType, string whichPetBreed, string inputCoat)
        {
            string outputCoat = inputCoat;
            var dict = GetDataDict();
            if (!dict.Any())
                return outputCoat;

            List<string> coats = new List<string>();
            foreach (var kvp in dict)
            {
                if (kvp.Value.PetType == whichPetType && kvp.Value.PetBreed == whichPetBreed)
                {
                    coats.Add(kvp.Key);
                }
            }
            if (!coats.Any())
                return outputCoat;
            coats.Sort();
            if (inputCoat == null)
            {
                if (change > 0)
                    outputCoat = coats[0];
                else
                    outputCoat = coats[coats.Count - 1];
            }
            else
            {
                int which = change > 0 ? 0 : coats.Count - 1;
                for (int i = 0; i < coats.Count; i++)
                {
                    if (coats[i] == inputCoat)
                    {
                        which = i + change;
                        if (which < 0 || which >= coats.Count)
                        {
                            return null;
                        }
                    }
                }
                outputCoat = coats[which];
            }
            return outputCoat;
        }


        private static Color TintPet(Color white, Pet pet)
        {
            if (!Config.EnableMod)
                return white;
            GetPetData(pet, out var data);
            if (data?.Tint == null)
                return white;
            return data.Tint.Value;
        }
    }
}