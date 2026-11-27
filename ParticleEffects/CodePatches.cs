using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using System.Collections.Generic;
using Object = StardewValley.Object;

namespace ParticleEffects
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry
    {
        public static void Farmer_draw_postfix(Farmer __instance, SpriteBatch b)
        {
            if (!Config.EnableMod)
                return;
            if (farmerDict.ContainsKey(__instance.UniqueMultiplayerID))
            {
                foreach(var effect in farmerDict[__instance.UniqueMultiplayerID])
                {
                    if (effectDict.ContainsKey(effect))
                    {
                        ShowFarmerParticleEffect(b, __instance, effect, effectDict[effect]);
                    }
                }
            }
            foreach(var kvp in effectDict)
            {
                switch (kvp.Value.type.ToLower())
                {
                    case "hat":
                        if (__instance.hat.Value != null && __instance.hat.Value.Name == kvp.Value.name)
                            ShowFarmerParticleEffect(b, __instance, kvp.Key, kvp.Value);
                        break;
                    case "shirt":
                        if (__instance.shirt.Value.ToString() == kvp.Value.name)
                            ShowFarmerParticleEffect(b, __instance, kvp.Key, kvp.Value);
                        break;
                    case "pants":
                        if (__instance.pants.Value.ToString() == kvp.Value.name)
                            ShowFarmerParticleEffect(b, __instance, kvp.Key, kvp.Value);
                        break;
                    case "boots":
                        if (__instance.boots.Value != null && __instance.boots.Value.Name == kvp.Value.name)
                            ShowFarmerParticleEffect(b, __instance, kvp.Key, kvp.Value);
                        break;
                    case "tool":
                        if (__instance.CurrentItem is Tool && __instance.CurrentItem.Name == kvp.Value.name)
                            ShowFarmerParticleEffect(b, __instance, kvp.Key, kvp.Value);
                        break;
                    case "ring":
                        if (__instance.leftRing.Value != null && __instance.leftRing.Value.Name == kvp.Value.name)
                            ShowFarmerParticleEffect(b, __instance, kvp.Key, kvp.Value);
                        else if (__instance.rightRing.Value != null && __instance.rightRing.Value.Name == kvp.Value.name)
                            ShowFarmerParticleEffect(b, __instance, kvp.Key, kvp.Value);
                        break;
                }
            }
        }
        public static void Object_draw_postfix(Object __instance, SpriteBatch spriteBatch, int x, int y)
        {
            if (!Config.EnableMod)
                return;
            foreach (var kvp in effectDict)
            {
                if(kvp.Value.type.ToLower() == "object" && kvp.Value.name == __instance.Name)
                {
                    ShowObjectParticleEffect(spriteBatch, __instance, x, y, kvp.Key, kvp.Value);
                }
            }
        }
        public static void NPC_draw_postfix(NPC __instance, SpriteBatch b)
        {
            if (!Config.EnableMod)
                return;
            if (NPCDict.ContainsKey(__instance.Name))
            {
                foreach (var effect in NPCDict[__instance.Name])
                {
                    if (effectDict.ContainsKey(effect))
                    {
                        ShowNPCParticleEffect(b, __instance, effect, effectDict[effect]);
                    }
                }
            }
            foreach (var kvp in effectDict)
            {
                if (kvp.Value.type.ToLower() == "npc" && kvp.Value.name == __instance.Name)
                {
                    ShowNPCParticleEffect(b, __instance, kvp.Key, kvp.Value);
                }
            }
        }
    }
}