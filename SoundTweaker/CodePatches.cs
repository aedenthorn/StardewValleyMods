using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using StardewValley;

namespace SoundTweaker
{
    public partial class ModEntry
    {

        [HarmonyPatch(typeof(AudioEmitter))]
        [HarmonyPatch(MethodType.Constructor)]
        public class AudioEmitter_Patch
        {
            public static void Postfix(AudioEmitter __instance)
            {
                if (!Config.ModEnabled)
                    return;
            }
        }
        [HarmonyPatch(typeof(AudioEmitter), nameof(AudioEmitter.Position))]
        [HarmonyPatch(MethodType.Getter)]
        public class AudioEmitter_Position_Patch
        {
            public static void Postfix(ref Vector3 __result)
            {
                if (!Config.ModEnabled)
                    return;
                __result = new Vector3(Game1.random.Next(0, 2) - 1, Game1.random.Next(0, 2) - 1, Game1.random.Next(0, 2) - 1);
            }
        }
        [HarmonyPatch(typeof(Cue), nameof(Cue.Apply3D))]
        public class Cue_Apply3D_Patch
        {
            public static void Postfix(AudioListener listener, AudioEmitter emitter)
            {
                if (!Config.ModEnabled)
                    return;
                var x = 1;
            }
        }
        [HarmonyPatch(typeof(Cue), "PlaySoundInstance")]
        public class Cue_PlaySoundInstance_Patch
        {
            public static void Prefix(SoundEffectInstance sound_instance, int variant_index)
            {
                if (!Config.ModEnabled)
                    return;
                var x = 1;
            }
        }
        [HarmonyPatch(typeof(Cue), "UpdateRpcCurves")]
        public class Cue_UpdateRpcCurves_Patch
        {
            public static void Postfix(Cue __instance, XactSoundBankSound ____currentXactSound)
            {
                if (!Config.ModEnabled || !__instance.Name.ToLower().Contains("step"))
                    return;
                if(____currentXactSound?.rpcCurves?.Length > 0)
                {
                    var x = 1;
                }

                //AccessTools.FieldRefAccess<Cue, float>(__instance, "_rpcVolume") = 10f;
            }
        }
    }
}