using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Characters;
using StardewValley.GameData.Pets;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using static StardewValley.Minigames.BoatJourney;

namespace PetCoats
{
    public partial class ModEntry
    {
        public static PetCoatCache cachedPetCoatIcon;

        [HarmonyPatch(typeof(Game1), "UpdateControlInput")]
        public class Game1_UpdateControlInput_Patch
        {
            public static void Prefix()
            {
                if (!Config.EnableMod || !Context.IsPlayerFree || Game1.currentLocation is null)
                    return;

                MouseState currentMouseState = Game1.input.GetMouseState();
                if (currentMouseState.ScrollWheelValue == Game1.oldMouseState.ScrollWheelValue)
                    return;
                int diff = Math.Sign(currentMouseState.ScrollWheelValue - Game1.oldMouseState.ScrollWheelValue);
                
                var dict = GetDataDict();

                var mp = Game1.getMousePosition() + new Point(Game1.viewport.Location.X, Game1.viewport.Location.Y);

                foreach (var c in Game1.currentLocation.characters)
                {
                    var bb = c.GetBoundingBox();
                    bb.Offset(0, -32);
                    bb.Inflate(0, 32);
                    if (c is not Pet pet || !bb.Contains(mp))
                        continue;

                    if (SHelper.Input.IsDown(Config.BreedKey))
                    {
                        if (!Pet.TryGetData(Game1.MasterPlayer.whichPetType, out var petData))
                            return;
                        for (int i = 0; i < petData.Breeds.Count; i++)
                        {
                            if (pet.whichBreed.Value == petData.Breeds[i].Id)
                            {
                                int which = i + diff;
                                if (which < 0)
                                    which = petData.Breeds.Count - 1;
                                else if (which >= petData.Breeds.Count)
                                {
                                    which = 0;
                                }
                                pet.whichBreed.Value = petData.Breeds[which].Id;
                                pet.reloadBreedSprite();
                                Game1.oldMouseState = Game1.input.GetMouseState();
                                Game1.playSound("grassyStep");
                                return;
                            }
                        }

                    }
                    else if (SHelper.Input.IsDown(Config.TypeKey))
                    {
                        var keys = Game1.petData.Keys.ToArray();

                        for (int i = 0; i < keys.Length; i++)
                        {
                            var key = keys[i];
                            if (pet.petType.Value == key)
                            {
                                int which = i + diff;
                                if (which < 0)
                                    which = keys.Length - 1;
                                else if (which >= keys.Length)
                                {
                                    which = 0;
                                }
                                pet.petType.Value = keys[which];
                                pet.whichBreed.Value = Game1.petData[keys[which]].Breeds[0].Id;
                                pet.reloadBreedSprite();
                                Game1.oldMouseState = Game1.input.GetMouseState();
                                Game1.playSound("yoba");
                                return;
                            }
                        }

                    }
                    else if (SHelper.Input.IsDown(Config.CoatKey))
                    {
                        string coat = GetPetData(pet, out var data);
                        coat = ChangePetCoat(diff, pet.petType.Value, pet.whichBreed.Value, coat);

                        pet.modData[modKey] = coat == null ? "" : coat;
                        pet.reloadBreedSprite();
                        Game1.oldMouseState = Game1.input.GetMouseState();
                        Game1.playSound("grassyStep");
                        return;
                    }
                }
            }
        }

        [HarmonyPatch(typeof(CharacterCustomization), "receiveLeftClick")]
        public class CharacterCustomization_receiveLeftClick_Patch
        {
            public static bool Prefix(CharacterCustomization __instance, int x, int y, bool playSound)
            {
                if (!Config.EnableMod || __instance.petPortraitBox is null || !__instance.petPortraitBox.Value.Contains(x, y))
                    return true;
                cachedPetCoatIcon = null;
                MasterCoat = ChangePetCoat(1, Game1.MasterPlayer.whichPetType, Game1.MasterPlayer.whichPetBreed, MasterCoat);
                if(playSound)
                    Game1.playSound("grassyStep");
                return false;
            }
        }

        [HarmonyPatch(typeof(CharacterCustomization), "performHoverAction")]
        public class CharacterCustomization_performHoverAction_Patch
        {
            public static bool Prefix(CharacterCustomization __instance, int x, int y, ref string ___hoverText, ref string ___hoverTitle)
            {
                if (!Config.EnableMod || __instance.petPortraitBox is null || !__instance.petPortraitBox.Value.Contains(x, y))
                    return true;
                var dict = GetDataDict();
                if (dict?.Any() != true)
                    return true;
                int count = 0;

                string title = null;
                foreach(var kvp in dict)
                {
                    if (kvp.Value.PetType == Game1.player.whichPetType && kvp.Value.PetBreed == Game1.player.whichPetBreed)
                    {
                        count++;
                        if(MasterCoat == kvp.Key && kvp.Value.DisplayName != null) 
                        {
                            title = kvp.Value.DisplayName != null ? kvp.Value.DisplayName : kvp.Key;
                        }
                    }
                }
                if (count > 0)
                {
                    ___hoverTitle = title == null ? Game1.player.whichPetType + " " + Game1.player.whichPetBreed : title;
                    ___hoverText = string.Format(SHelper.Translation.Get(count == 1 ? "1-custom" : "x-custom"), count);
                }
                return false;
            }
        }

        [HarmonyPatch(typeof(CharacterCustomization), "selectionClick")]
        public class CharacterCustomization_selectionClick_Patch
        {
            public static bool Prefix(CharacterCustomization __instance, string name, int change)
            {
                if (!Config.EnableMod || name != "Pet" || !SHelper.Input.IsDown(Config.ModKey))
                    return true;
                cachedPetCoatIcon = null;
                MasterCoat = ChangePetCoat(change, Game1.MasterPlayer.whichPetType, Game1.MasterPlayer.whichPetBreed, MasterCoat);
                Game1.playSound("grassyStep");
                return false;
            }
        }
        [HarmonyPatch(typeof(Pet), nameof(Pet.getPetTextureName))]
        public class Pet_getPetTextureName_Patch
        {
            public static bool Prefix(Pet __instance, ref string __result)
            {
                if (!Config.EnableMod)
                    return true;
                string coat = GetPetData(__instance, out var data);
                if(data == null)
                    return true;
                __result = texturesPrefix + coat;
                return false;
            }

        }
        
        [HarmonyPatch(typeof(Pet), nameof(Pet.draw), new Type[] { typeof(SpriteBatch) })]
        public class Pet_draw_Patch
        {

            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling Pet.draw");
                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (i < codes.Count - 3 && codes[i].opcode == OpCodes.Call && codes[i].operand is MethodInfo info && info == AccessTools.PropertyGetter(typeof(Color), nameof(Color.White)) && codes[i + 1].opcode == OpCodes.Ldarg_0)
                    {
                        SMonitor.Log($"adding method to tint pet");
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(TintPet))));
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Ldarg_0));
                        break;
                    }
                }

                return codes.AsEnumerable();
            }
        }

        [HarmonyPatch(typeof(SpriteBatch), nameof(SpriteBatch.Draw), new Type[] {typeof(Texture2D),typeof(Rectangle),typeof(Rectangle?),typeof(Color),typeof(float),typeof(Vector2),typeof(SpriteEffects),typeof(float) })]
        public class SpriteBatch_Draw_Patch
        {
            public static void Prefix(ref Texture2D texture, ref Color color)
            {
                if (!Config.EnableMod || texture == null || Game1.MasterPlayer?.whichPetType == null)
                    return;
                if(cachedPetCoatIcon == null)
                {
                    cachedPetCoatIcon = new PetCoatCache();
                    PetData petData;
                    if (!Pet.TryGetData(Game1.MasterPlayer.whichPetType, out petData))
                        return;
                    foreach (PetBreed breed in petData.Breeds)
                    {
                        if (breed.Id == Game1.MasterPlayer.whichPetBreed)
                        {
                            cachedPetCoatIcon.defaultTexture = Game1.content.Load<Texture2D>(breed.IconTexture);
                            break;
                        }
                    }
                    if (cachedPetCoatIcon.defaultTexture == null)
                    {
                        cachedPetCoatIcon.noTexture = true;
                        return;
                    }
                }
                else if (cachedPetCoatIcon.noTexture)
                    return;

                if (texture == cachedPetCoatIcon.defaultTexture)
                {
                    if (cachedPetCoatIcon.tint != null)
                    {
                        color = cachedPetCoatIcon.tint.Value;
                    }
                    if (cachedPetCoatIcon.texture != null)
                    {
                        texture = cachedPetCoatIcon.texture;
                        //var path = Path.Combine(SHelper.DirectoryPath, "test.png");
                        //if (!File.Exists(path))
                        //{
                        //    using (var stream = File.Create(path))
                        //    {
                        //        cachedPetCoatIcon.texture.SaveAsPng(stream, cachedPetCoatIcon.texture.Width, cachedPetCoatIcon.texture.Height);
                        //    }
                        //}
                    }
                    else
                    {
                        if (MasterCoat == null)
                        {
                            cachedPetCoatIcon.noTexture = true;
                            return;
                        }
                        var dict = GetDataDict();
                        if (!dict.TryGetValue(MasterCoat, out var coatData))
                        {
                            cachedPetCoatIcon.noTexture = true;
                            return;
                        }
                        if (coatData.IconTexture != null)
                        {
                            cachedPetCoatIcon.texture = coatData.IconTexture;
                            texture = coatData.IconTexture;
                        }
                        else
                        {
                            cachedPetCoatIcon.texture = texture;
                        }
                        cachedPetCoatIcon.tint = coatData.Tint;
                    }
                }
            }
        }
    }
}