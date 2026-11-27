using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Characters;
using StardewValley.Menus;
using StardewValley.Objects;
using StardewValley.Tools;
using System.Reflection;
using System.Reflection.Emit;
using static StardewValley.Minigames.CraneGame;

namespace CustomMounts
{
    public partial class ModEntry
    {
        public static void Character_faceDirection_Prefix(Character __instance, int direction)
        {
            return;
            if (!Config.ModEnabled || __instance is not Horse horse || horse.rider == null || !__instance.modData.TryGetValue(modKey, out var key) || !MountDict.TryGetValue(key, out var data) || __instance.FacingDirection % 2 == direction % 2)
                return;
            Rectangle bounds =horse.GetBoundingBox();
            Vector2 diff = Vector2.Zero;
            switch (direction)
            {
                case 0:
                case 2:
                    bounds.Width = data.Size.Y;
                    bounds.Height = data.Size.X;
                    diff = new Vector2(data.Size.Y / 2 - data.Size.X / 2, data.Size.X / 2 - data.Size.Y / 2);
                    break;
                case 1:
                case 3:
                    bounds.Width = data.Size.X;
                    bounds.Height = data.Size.Y;
                    diff = new Vector2(data.Size.X / 2 - data.Size.Y / 2, data.Size.Y / 2 - data.Size.X / 2);
                    break;
            }
            var newPos = horse.rider.Position - diff;
            var sx = Math.Sign(diff.X);
            var sy = Math.Sign(diff.Y);
            while (Vector2.Distance(newPos, horse.rider.Position) > 2)
            {
                Rectangle nextPosition = new Rectangle(bounds.X - sx, bounds.Y - sy, bounds.Width, bounds.Height);
                if (__instance.currentLocation.isCollidingPosition(nextPosition, Game1.viewport, false, -1, false, horse))
                    break;
                horse.rider.Position -= new Vector2(sx, sy);
            }
        }
        public static bool NPC_behaviorOnFarmerLocationEntry_Prefix(NPC __instance)
        {
            if (!Config.ModEnabled || __instance.Sprite == null || !__instance.modData.TryGetValue(modKey, out var key))
                return true;
            __instance.Sprite.currentFrame = 0;
            return false;
        }
        public static void Building_OnUpgraded_Postfix(Building __instance)
        {
            if (!Config.ModEnabled || __instance is not Stable stable)
                return;
            Horse horse = stable.getStableHorse();
            if (horse is null)
                return;
            MakeCustomMount(horse);
        }
        public static bool Stable_GetDefaultHorseTile_Prefix(Stable __instance, ref Point __result)
        {
            if (!Config.ModEnabled)
                return true;
            var bd = __instance.GetData();
            if (bd is null)
                return true;
            foreach (var kvp in MountDict)
            {
                if (bd.Name == kvp.Value.Stable || __instance.buildingType.Value == kvp.Value.Stable)
                {
                    __result = new Point(__instance.tileX.Value + kvp.Value.SpawnOffset.X, __instance.tileY.Value + kvp.Value.SpawnOffset.Y);
                    return false;
                }
            }
            return true;
        }

        public static void Horse_Postfix(Horse __instance)
        {
            if (!Config.ModEnabled)
                return;
            MakeCustomMount(__instance);
        }

        public static bool Horse_ChooseAppearance_Prefix(Horse __instance)
        {
            if (!Config.ModEnabled || !__instance.modData.TryGetValue(modKey, out var key))
                return true;
            if (__instance.Sprite != null && __instance.spriteOverridden)
                return true;
            if (!MountDict.TryGetValue(key, out var data))
                return true;
            SetSprite(__instance, data);
            return false;
        }
        public static void Horse_dayUpdate_Postfix(Horse __instance)
        {
            if (!Config.ModEnabled)
                return;
            MakeCustomMount(__instance);
        }
        public static void Horse_GetBoundingBox_Postfix(Horse __instance, bool ___squeezingThroughGate, ref Rectangle __result)
        {
            if (!Config.ModEnabled)
                return;
            int diff = __instance.Sprite.SpriteWidth * 3 - 96;
             __result.Width -= diff;

        }
        public static void Horse_SyncPositionToRider_Postfix(Horse __instance)
        {
            if (!Config.ModEnabled || __instance.rider is null || __instance.dismounting.Value || !__instance.modData.TryGetValue(modKey, out var key) || !MountDict.TryGetValue(key, out var data))
                return;
            var offset = new Vector2((32 - data.FrameWidth) * 2, data.FrameWidth - 32);
            var newPos = __instance.Position + offset;
            if (newPos.X < 0)
                return;

            __instance.Position = newPos;
        }
        public static IEnumerable<CodeInstruction> Horse_PerformDefaultHorseFootstep_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            SMonitor.Log($"Transpiling Horse.PerformDefaultHorseFootstep");

            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Ldstr)
                {
                    string which = (string)codes[i].operand;
                    switch (which)
                    {
                        case "thudStep":
                        case "woodyStep":
                        case "stoneStep":
                            SMonitor.Log($"Adding method to change {which}");
                            codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.SetStepSound))));
                            codes.Insert(i + 1, new CodeInstruction(OpCodes.Ldarg_0));
                            i += 2;
                            break;
                        default:
                            break;
                    }
                }
            }

            return codes.AsEnumerable();
        }
        public static void Horse_draw_Prefix(Horse __instance, int ___munchingCarrotTimer, ref int __state)
        {
            //___munchingCarrotTimer = 10;

            if (!Config.ModEnabled || !CheckModData(__instance, out var data) || ___munchingCarrotTimer <= 0|| (__instance.Sprite.SpriteWidth == 32 && __instance.Sprite.SpriteHeight == 32))
                return;
            
            __state = ___munchingCarrotTimer;
            ___munchingCarrotTimer = 0;
        }
        public static void Horse_draw_Postfix(Horse __instance, SpriteBatch b, ref int ___munchingCarrotTimer, int __state)
        {
            if (!Config.ModEnabled || !CheckModData(__instance, out var data))
                return;
            /*
            if (++toggle < 60)
                return;
            toggle %= 120;
            */
            var posy = __instance.Position.Y;
            if(Game1.currentLocation.Name == "StardewOpenWorld")
            {
                
                posy %= 6400 + (Game1.viewport.Y / 6400 < (int)posy / (6400) ? 6400 : 0);
            }
                
            if (__state > 0)
            {

                float xScale = __instance.Sprite.SpriteWidth / 32f;
                float yScale = __instance.Sprite.SpriteHeight / 32f;
                int yDiff = (__instance.Sprite.SpriteHeight - 32) / 2;
                switch (__instance.FacingDirection)
                {
                    case 1:
                        b.Draw(__instance.Sprite.Texture, __instance.getLocalPosition(Game1.viewport) + new Vector2(80f * xScale, -56f * yScale - yDiff), new Rectangle?(new Rectangle((int)Math.Round(179 * xScale) + (int)(Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 600.0) / 300 * (int)Math.Round(16 * xScale), (int)Math.Round(97 * yScale), (int)Math.Round(16 * xScale), (int)Math.Round(14 * yScale))), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (posy + 64f) / 10000f + 1E-07f);
                        return;
                    case 2:
                        b.Draw(__instance.Sprite.Texture, __instance.getLocalPosition(Game1.viewport) + new Vector2(24f * xScale, -24f * yScale - yDiff), new Rectangle?(new Rectangle((int)Math.Round(170 * xScale) + (int)(Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 600.0) / 300 * (int)Math.Round(16 * xScale), (int)Math.Round(112 * yScale), (int)Math.Round(16 * xScale), (int)Math.Round(16 * yScale))), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (posy + 64f) / 10000f + 1E-07f);
                        return;
                    case 3:
                        b.Draw(__instance.Sprite.Texture, __instance.getLocalPosition(Game1.viewport) + new Vector2(-16f * xScale, -56f * yScale - yDiff), new Rectangle?(new Rectangle((int)Math.Round(179 * xScale) + (int)(Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 600.0) / 300 * (int)Math.Round(16 * xScale), (int)Math.Round(97 * yScale), (int)Math.Round(16 * xScale), (int)Math.Round(14 * yScale))), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.FlipHorizontally, (posy + 64f) / 10000f + 1E-07f);
                        break;
                    default:
                        return;
                }
                ___munchingCarrotTimer = __state;
            }

        }
        public static IEnumerable<CodeInstruction> Horse_draw_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            SMonitor.Log($"Transpiling Horse.draw");

            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; i++)
            {
                if(codes[i].opcode == OpCodes.Callvirt && (MethodInfo) codes[i].operand == AccessTools.Method(typeof(Hat), nameof(Hat.draw)))
                {
                    SMonitor.Log($"Overriding hat draw method");
                    codes[i].opcode = OpCodes.Call;
                    codes[i].operand = AccessTools.Method(typeof(ModEntry), nameof(ModEntry.DrawHat));
                    codes.Insert(i, new CodeInstruction(OpCodes.Ldarg_0));
                    i++;
                }
                else if(codes[i].opcode == OpCodes.Callvirt && (MethodInfo) codes[i].operand == AccessTools.Method(typeof(SpriteBatch), nameof(SpriteBatch.Draw), new Type[] { typeof(Texture2D), typeof(Vector2), typeof(Rectangle?), typeof(Color), typeof(float), typeof(Vector2), typeof(float), typeof(SpriteEffects), typeof(float) }))
                {
                    SMonitor.Log($"Overriding Spritebatch.Draw method");
                    codes[i].opcode = OpCodes.Call;
                    codes[i].operand = AccessTools.Method(typeof(ModEntry), nameof(ModEntry.DrawHorse));
                    codes.Insert(i, new CodeInstruction(OpCodes.Ldarg_0));
                    i++;
                }
            }

            return codes.AsEnumerable();
        }
        public static IEnumerable<CodeInstruction> NPC_draw_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            SMonitor.Log($"Transpiling NPC.draw");

            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; i++)
            {
                if(codes[i].opcode == OpCodes.Call && codes[i].operand is MethodInfo && (MethodInfo) codes[i].operand == AccessTools.Method(typeof(Character), nameof(Character.getLocalPosition)))
                {
                    SMonitor.Log($"Overriding draw position");
                    codes.Insert(i + 1, new(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.GetDrawPosition))));
                    codes.Insert(i + 1, new(OpCodes.Ldarg_0));
                    i+=2;
                }
            }

            return codes.AsEnumerable();
        }
        public static bool Horse_checkAction_Prefix(Horse __instance, Farmer who)
        {
            if (!Config.ModEnabled)
                return true;

            bool modded = CheckModData(__instance, out MountData data);

            if(Config.RenameModKey != SButton.None && SHelper.Input.IsDown(Config.RenameModKey))
            {
                Game1.activeClickableMenu = new NamingMenu(new NamingMenu.doneNamingBehavior(__instance.nameHorse), modded ? string.Format(SHelper.Translation.Get("NameYourX"), data.Name) : Game1.content.LoadString("Strings\\Characters:NameYourHorse"), string.IsNullOrEmpty(__instance.displayName) ? Game1.content.LoadString("Strings\\Characters:DefaultHorseName") : __instance.displayName);
                return false;
            }

            if (!modded)
                return true;

            if (!data.AllowHats && who != null && who.canMove && __instance.rider == null && who.mount == null && !who.FarmerSprite.PauseForSingleAnimation && __instance.currentLocation == who.currentLocation && __instance.getOwner() == Game1.player && who.Items.Count > who.CurrentToolIndex && who.Items[who.CurrentToolIndex] is Hat)
            {
                SMonitor.Log($"Preventing hat wearing");
                return false;
            }
            return true;
        }
        public static IEnumerable<CodeInstruction> Horse_checkAction_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            SMonitor.Log($"Transpiling Horse.checkAction");

            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; i++)
            {
                if(i < codes.Count - 3 && codes[i].opcode == OpCodes.Ldc_I4_0 && codes[i + 1].opcode == OpCodes.Conv_I8 && codes[i + 2].opcode == OpCodes.Callvirt)
                {
                    SMonitor.Log($"Preventing ownership erasure");
                    codes[i + 2].opcode = OpCodes.Call;
                    codes[i + 2].operand = AccessTools.Method(typeof(ModEntry), nameof(ModEntry.PreventOwnershipErasure));
                    break;
                }
            }

            return codes.AsEnumerable();
        }
        public static void Horse_update_Prefix(Horse __instance, GameLocation location)
        {
            if (Config.ModEnabled && __instance.modData.TryGetValue(animKey, out var anim))
            {
                if (__instance.Sprite?.CurrentAnimation != null && __instance.rider is null)
                {
                    var data = MountDict[__instance.modData[modKey]];
                    var animData = data.CustomAnimations[anim];
                    if(animData.FacingDirection != __instance.FacingDirection || animData.Frames.Length <= __instance.Sprite.currentAnimationIndex)
                    {
                        __instance.modData.Remove(animKey);
                    }
                    else if (animData.Frames[__instance.Sprite.currentAnimationIndex].Sound != null)
                    {
                        location.playSound(animData.Frames[__instance.Sprite.currentAnimationIndex].Sound, __instance.Tile);
                    }
                }
                else
                {
                    __instance.modData.Remove(animKey);
                }
            }
        }
        public static IEnumerable<CodeInstruction> Horse_update_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            SMonitor.Log($"Transpiling Horse.update");

            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; i++)
            {
                if(i > 7 && codes[i - 8].opcode == OpCodes.Ldc_I4_2 && codes[i].opcode == OpCodes.Ldc_R8 && (double)codes[i].operand == 0.002)
                {
                    SMonitor.Log($"Overriding random animation chance");
                    codes.Insert(i + 1, new(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.OverrideRandomAnimationChance))));
                    codes.Insert(i + 1, new(OpCodes.Ldarg_0));
                    codes[i - 8].opcode = OpCodes.Ldc_I4_8;
                    break;
                }
            }

            return codes.AsEnumerable();
        }
        public static IEnumerable<CodeInstruction> Horse_checkAction2_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            SMonitor.Log($"Transpiling Horse.checkAction2");

            var codes = new List<CodeInstruction>(instructions);
            int which = -1;
            for (int i = 0; i < codes.Count; i++)
            {
                if (which < 0 && codes[i].opcode == OpCodes.Ldfld && ((FieldInfo)codes[i].operand).Name.EndsWith("__this"))
                    which = i;
                if (codes[i].opcode == OpCodes.Ldstr)
                {
                    var str = (string)codes[i].operand;
                    switch (str)
                    {
                        case "(O)Carrot":
                            SMonitor.Log($"Overriding carrot item");
                            codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.SetCarrotItem))));
                            codes.Insert(i + 1, codes[which].Clone());
                            codes.Insert(i + 1, new CodeInstruction(OpCodes.Ldarg_0));
                            i += 2;
                            break;
                        case "eat":
                            SMonitor.Log($"Overriding eat sound");
                            codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.SetEatSound))));
                            codes.Insert(i + 1, codes[which].Clone());
                            codes.Insert(i + 1, new CodeInstruction(OpCodes.Ldarg_0));
                            i += 2;
                            break;
                        case "Strings\\Characters:NameYourHorse":
                            SMonitor.Log($"Overriding nameyourhorse string");
                            codes.Insert(i + 2, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.SetNameYourHorse))));
                            codes.Insert(i + 2, codes[which].Clone());
                            codes.Insert(i + 2, new CodeInstruction(OpCodes.Ldarg_0));
                            i += 3;
                            break;
                        case "Strings\\Characters:DefaultHorseName":
                            SMonitor.Log($"Overriding defaulthorsename string");
                            codes.Insert(i + 2, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.SetDefaultHorseName))));
                            codes.Insert(i + 2, codes[which].Clone());
                            codes.Insert(i + 2, new CodeInstruction(OpCodes.Ldarg_0));
                            i += 2;
                            break;
                    }
                }
            }

            return codes.AsEnumerable();
        }
        
        public static IEnumerable<CodeInstruction> Object_performUseAction_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            SMonitor.Log($"Transpiling Object.performUseAction");

            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; i++)
            {
                if(codes[i].opcode == OpCodes.Ldstr && (string)codes[i].operand == "(O)911")
                {
                    SMonitor.Log($"Overriding flute item");
                    codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.SetFluteItem))));
                    codes.Insert(i + 1, new CodeInstruction(OpCodes.Ldarg_0));
                    i += 2;
                }
                else if(codes[i].opcode == OpCodes.Ldstr && (string)codes[i].operand == "horse_flute")
                {
                    SMonitor.Log($"Overriding flute sound");
                    codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.SetFluteSound))));
                    codes.Insert(i + 1, new CodeInstruction(OpCodes.Ldarg_0));
                    i += 2;
                }
            }
            return codes.AsEnumerable();
        }
        public static void Utility_GetHorseWarpRestrictionsForFarmer_Postfix(Farmer who, ref Utility.HorseWarpRestrictions __result)
        {
            if (!Config.ModEnabled)
                return;
            if (__result.HasFlag(Utility.HorseWarpRestrictions.NoOwnedHorse))
            {
                bool found = false;
                Utility.ForEachBuilding<Stable>(delegate (Stable stable)
                {
                    Horse curHorse = stable.getStableHorse();
                    var owner = curHorse?.getOwner();
                    var ownerId = curHorse?.ownerId;
                    if (owner == Game1.player)
                    {
                        if (curHorse.modData.TryGetValue(modKey, out var key) && MountDict.TryGetValue(key, out var data))
                        {
                            if (who.ActiveObject.QualifiedItemId == data.FluteItem)
                            {
                                found = true;
                                return false;
                            }
                        }
                    }
                    return true;
                }, true);
                if (found) 
                {
                    __result &= ~Utility.HorseWarpRestrictions.NoOwnedHorse;
                }
            }
        }
        public static void Farmer_Update_Prefix(Farmer __instance, ref bool __state)
        {
            if (!Config.ModEnabled || __instance.yJumpOffset >= 0)
                return;
            __state = true;
        }
        public static void Farmer_Update_Postfix(Farmer __instance, bool __state)
        {
            if (!Config.ModEnabled || !__state || __instance.yJumpOffset < 0)
                return;
            AccessTools.Method(typeof(Character), "ClearCachedPosition").Invoke(__instance, null);
        }
        public static IEnumerable<CodeInstruction> Farmer_updateMovementAnimation_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            SMonitor.Log($"Transpiling Farmer.updateMovementAnimation");

            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Ldc_I4_2 && codes[i + 1].opcode == OpCodes.Call)
                {
                    SMonitor.Log($"Adding method to change mount speed");
                    codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.SetSpeed))));
                    codes.Insert(i + 1, new CodeInstruction(OpCodes.Ldarg_0));
                    break;
                }
            }

            return codes.AsEnumerable();
        }
        public static IEnumerable<CodeInstruction> Farmer_getMovementSpeed_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            SMonitor.Log($"Transpiling Farmer.getMovementSpeed");

            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; i++)
            {
                if (i > 3 && codes[i].opcode == OpCodes.Ldc_R4 && codes[i - 4].opcode == OpCodes.Ldfld && ((FieldInfo)codes[i - 4].operand).Name == "ateCarrotToday")
                {
                    SMonitor.Log($"Adding method to change carrot speed bonus");
                    codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.SetSpeedBonus))));
                    codes.Insert(i + 1, new CodeInstruction(OpCodes.Ldarg_0));
                    break;
                }
            }

            return codes.AsEnumerable();
        }
        public static void Farmer_set_mount_Postfix(Farmer __instance)
        {
            if (!Config.ModEnabled)
                return;
        }
        public static IEnumerable<CodeInstruction> FarmerTeam_OnRequestHorseWarp_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            SMonitor.Log($"Transpiling FarmerTeam_OnRequestHorseWarp");

            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Ldloc_0 && codes[i + 4].opcode == OpCodes.Call && ((MethodInfo)codes[i + 4].operand).Name.Contains("ForEachBuilding"))
                {
                    SMonitor.Log($"Reworking horse finding code");
                    codes.RemoveRange(i, 5);
                    codes[i - 2] = new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.GetHorseForFlute)));
                    codes.Insert(i  - 2, new CodeInstruction(OpCodes.Ldarg_1));
                    break;
                }
            }

            return codes.AsEnumerable();
        }
        public static IEnumerable<CodeInstruction> Game1_UpdateHorseOwnership_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            SMonitor.Log($"Transpiling Game1_UpdateHorseOwnership");

            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; i++)
            {
                if (i < codes.Count - 3 && codes[i].opcode == OpCodes.Ldc_I4_0 && codes[i + 1].opcode == OpCodes.Conv_I8 && codes[i + 2].opcode == OpCodes.Callvirt)
                {
                    SMonitor.Log($"Preventing ownership erasure");
                    codes[i + 2].opcode = OpCodes.Call;
                    codes[i + 2].operand = AccessTools.Method(typeof(ModEntry), nameof(ModEntry.PreventOwnershipErasure));
                    break;
                }
            }

            return codes.AsEnumerable();
        }
        public static IEnumerable<CodeInstruction> Stable_updateHorseOwnership_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            SMonitor.Log($"Transpiling Stable.UpdateHorseOwnership");

            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; i++)
            {
                if (i < codes.Count - 2 && codes[i].opcode == OpCodes.Callvirt && (MethodInfo)codes[i].operand == AccessTools.Method(typeof(Horse), nameof(Horse.getOwner)) && codes[i+1].opcode == OpCodes.Brfalse_S)
                {
                    SMonitor.Log($"Preventing name erasure");
                    codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.PreventNameErasure))));
                    break;
                }
            }

            return codes.AsEnumerable();
        }

        public static void AnimalPage_CreateSpriteComponent_Postfix(AnimalPage.AnimalEntry entry, int index, ref ClickableTextureComponent __result)
        {
            if (!Config.ModEnabled || entry.Animal is not Horse horse || !CheckModData(horse, out var data))
                return;
            float scale = data.FrameHeight / 32f;
            __result.scale /= scale;
            __result.sourceRect = new Rectangle(0, (int)Math.Round(data.FrameHeight * 2 - 26 * scale), data.FrameWidth, (int)Math.Round(24 * scale));
        }
    }
}
