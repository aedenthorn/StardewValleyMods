using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Objects;
using System;

namespace PettingAnimation
{
    public partial class ModEntry
    {
        [HarmonyPatch(typeof(FarmerRenderer), nameof(FarmerRenderer.draw), new Type[] { typeof(SpriteBatch), typeof(FarmerSprite.AnimationFrame), typeof(int), typeof(Rectangle), typeof(Vector2), typeof(Vector2), typeof(float), typeof(int), typeof(Color), typeof(float), typeof(float), typeof(Farmer) })]
        public static class FarmerRenderer_draw_Patch
        {
            public static void Prefix(FarmerRenderer __instance, ref Vector2 position, ref float layerDepth, Farmer who)
            {
                if (!Config.ModEnabled || offset.Value == Vector2.Zero)
                    return;
                if(who.Sprite.CurrentAnimation is null)
                {
                    ticks.Value = 0;
                    layer.Value = -1;
                    offset.Value = Vector2.Zero;
                    return;
                }
                if (ticks.Value < Config.MovementTicks)
                    ticks.Value++;
                position += Vector2.Lerp(Vector2.Zero, offset.Value, ticks.Value / (float)Config.MovementTicks);
                if(layer.Value > 0)
                    layerDepth = layer.Value;
            }
        }
        [HarmonyPatch(typeof(Character), nameof(Character.GetShadowOffset))]
        public static class Character_GetShadowOffset_Patch
        {
            public static bool Prefix(Character __instance, ref Vector2 __result)
            {
                if (!Config.ModEnabled || offset.Value == Vector2.Zero)
                    return true;
                __result = Vector2.Lerp(Vector2.Zero, offset.Value, ticks.Value / 15f);
                return false;
            }
        }
        [HarmonyPatch(typeof(Pet), nameof(Pet.checkAction))]
        public static class Pet_checkAction_Patch
        {
            public static void Prefix(Pet __instance, Farmer who, ref bool __state)
            {
                if (Config.ModEnabled && Config.AlwaysAllowPet && who.CurrentItem is not Hat && who.CurrentItem?.QualifiedItemId != "(O)ButterflyPowder")
                    __state = true;
            }
            public static void Postfix(Pet __instance, Farmer who, ref bool __result, bool __state)
            {
                if (!Config.ModEnabled)
                    return;
                if(__result || __state)
                {
                    PetPet(__instance, who);
                }
            }
        }
        [HarmonyPatch(typeof(FarmAnimal), nameof(FarmAnimal.pet))]
        public static class FarmAnimal_pet_Patch
        {
            public static void Prefix(FarmAnimal __instance, Farmer who, bool is_auto_pet, ref bool __state)
            {
                if (Config.ModEnabled && !is_auto_pet && who.CurrentItem?.QualifiedItemId != "(O)GoldenAnimalCracker" && (Config.AlwaysAllowPet || !__instance.wasPet.Value))
                    __state = true;
            }
            public static void Postfix(FarmAnimal __instance, Farmer who, bool is_auto_pet, ref bool __state)
            {
                if(__state)
                    PetPet(__instance, who);
            }
        }
    }
}