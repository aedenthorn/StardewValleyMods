using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Locations;
using StardewValley.Monsters;
using StardewValley.Network;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace PacifistValley
{
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod
    {
        private static ModEntry context;
        private static ModConfig Config;
        private static IMonitor SMonitor;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            context = this;
            Config = this.Helper.ReadConfig<ModConfig>();
            SMonitor = Monitor;

            Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            
            if (!Config.EnableMod)
                return;

            Helper.Events.GameLoop.DayEnding += GameLoop_DayEnding;
            helper.Events.Content.AssetRequested += Content_AssetRequested;

            var harmony = new Harmony(ModManifest.UniqueID);

            harmony.Patch(
               original: AccessTools.Method(typeof(Farmer), nameof(Farmer.takeDamage)),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.takeDamage_prefix))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(MeleeWeapon), nameof(MeleeWeapon.setFarmerAnimating)),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.setFarmerAnimating_prefix))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(MeleeWeapon), "beginSpecialMove"),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.beginSpecialMove_prefix))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(GameLocation), "updateCharacters"),
               transpiler: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.updateCharacters_Transpiler))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(GameLocation), "damageMonster", new Type[] { typeof(Microsoft.Xna.Framework.Rectangle), typeof(int), typeof(int), typeof(bool), typeof(float), typeof(int), typeof(float), typeof(float), typeof(bool), typeof(Farmer) }),
               transpiler: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.damageMonster_Transpiler)),
               postfix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.damageMonster_Postfix))
               //prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.damageMonster_prefix))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(GameLocation), "drawCharacters"),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.drawCharacters_prefix))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(MineShaft), nameof(MineShaft.monsterDrop)),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.monsterDrop_prefix))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(Grub), nameof(Grub.behaviorAtGameTick)),
               postfix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Grub_behaviorAtGameTick_postfix))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(DinoMonster), nameof(DinoMonster.behaviorAtGameTick)),
               postfix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.DinoMonster_behaviorAtGameTick_postfix))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(SquidKid), nameof(SquidKid.behaviorAtGameTick)),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.SquidKid_behaviorAtGameTick_prefix))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(ShadowShaman), nameof(ShadowShaman.behaviorAtGameTick)),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.ShadowShaman_behaviorAtGameTick_prefix))
            );

            if (!Config.LovedMonstersStillSwarm || Config.MonstersIgnorePlayer)
            {
                harmony.Patch(
                   original: AccessTools.Method(typeof(Skeleton), nameof(Skeleton.behaviorAtGameTick)),
                   prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Skeleton_behaviorAtGameTick_prefix))
                );
                harmony.Patch(
                   original: AccessTools.Method(typeof(Monster), nameof(Monster.updateMovement)),
                   prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Monster_updateMovement_prefix))
                );
                harmony.Patch(
                   original: AccessTools.Method(typeof(Serpent), "updateAnimation"),
                   prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Serpent_updateAnimation_prefix))
                );
                harmony.Patch(
                   original: AccessTools.Method(typeof(Bat), "behaviorAtGameTick"),
                   postfix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Bat_behaviorAtGameTick_Postfix))
                );
                harmony.Patch(
                   original: AccessTools.Method(typeof(Fly), "updateAnimation"),
                   prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Fly_updateAnimation_prefix))
                );
                harmony.Patch(
                   original: AccessTools.Method(typeof(GreenSlime), nameof(GreenSlime.behaviorAtGameTick)),
                   prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.GreenSlime_behaviorAtGameTick_prefix))
                );
                harmony.Patch(
                   original: AccessTools.Method(typeof(DustSpirit), nameof(DustSpirit.behaviorAtGameTick)),
                   postfix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.DustSpirit_behaviorAtGameTick_Postfix))
                );
            }
            else
            {
                harmony.Patch(
                   original: AccessTools.Method(typeof(Skeleton), nameof(Skeleton.behaviorAtGameTick)),
                   postfix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Skeleton_behaviorAtGameTick_postfix))
                );
            }

            harmony.Patch(
               original: AccessTools.Method(typeof(FarmHouse), nameof(FarmHouse.UpdateWhenCurrentLocation)),
               transpiler: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.FarmHouse_UpdateWhenCurrentLocation_Transpiler))
            );
        }

        private void Content_AssetRequested(object sender, StardewModdingAPI.Events.AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo("TileSheets/weapons"))
            {
                e.LoadFromModFile<Texture2D>("assets/totally_not_weapons.png", StardewModdingAPI.Events.AssetLoadPriority.Exclusive);
            }
            else if (e.NameWithoutLocale.IsEquivalentTo("Data/weapons"))
            {
                e.LoadFromModFile<Dictionary<int, string>>("assets/totally_not_weapons.json", StardewModdingAPI.Events.AssetLoadPriority.Exclusive);
            }
            else if (e.NameWithoutLocale.IsEquivalentTo("Data/mail"))
            {
                e.Edit(delegate (IAssetData data) {
                    var editor = data.AsDictionary<string, string>();
                    editor.Data["guildQuest"] = Helper.Translation.Get("guildQuest");
                    editor.Data["Kent"] = editor.Data["Kent"].Replace("286 1 287 1 288 1 787 1", "787 1");
                });
            }
            else if (e.NameWithoutLocale.IsEquivalentTo("Data/Quests"))
            {
                e.Edit(delegate (IAssetData data) {
                    var editor = data.AsDictionary<int, string>();
                    editor.Data[15] = Helper.Translation.Get("Quests-15");
                });

            }
            else if (e.NameWithoutLocale.IsEquivalentTo("String/Locations"))
            {
                e.Edit(delegate (IAssetData data) {
                    var editor = data.AsDictionary<string, string>();
                    editor.Data["AdventureGuild_KillList_Header"] = Helper.Translation.Get("AdventureGuild_KillList_Header");
                });
            }
            else if (e.NameWithoutLocale.IsEquivalentTo("Strings/UI"))
            {
                e.Edit(delegate (IAssetData data) {
                    var editor = data.AsDictionary<string, string>();
                editor.Data["ItemHover_Damage"] = Helper.Translation.Get("ItemHover_Damage");
                editor.Data["ItemHover_Buff3"] = Helper.Translation.Get("ItemHover_Buff3");
                editor.Data["ItemHover_Buff11"] = Helper.Translation.Get("ItemHover_Buff11");
                editor.Data["Character_combat"] = Helper.Translation.Get("Character_combat");
                editor.Data["LevelUp_ProfessionName_Fighter"] = Helper.Translation.Get("LevelUp_ProfessionName_Fighter");
                editor.Data["LevelUp_ProfessionName_Brute"] = Helper.Translation.Get("LevelUp_ProfessionName_Brute");
                editor.Data["LevelUp_ProfessionName_Scout"] = Helper.Translation.Get("LevelUp_ProfessionName_Scout");
                editor.Data["LevelUp_ProfessionName_Desperado"] = Helper.Translation.Get("LevelUp_ProfessionName_Desperado");
                editor.Data["LevelUp_ProfessionDescription_Fighter"] = Helper.Translation.Get("LevelUp_ProfessionDescription_Fighter");
                editor.Data["LevelUp_ProfessionDescription_Brute"] = Helper.Translation.Get("LevelUp_ProfessionDescription_Brute");
                editor.Data["LevelUp_ProfessionDescription_Scout"] = Helper.Translation.Get("LevelUp_ProfessionDescription_Scout");
                editor.Data["LevelUp_ProfessionDescription_Desperado"] = Helper.Translation.Get("LevelUp_ProfessionDescription_Desperado");
                editor.Data["Chat_GalaxySword"] = Helper.Translation.Get("Chat_GalaxySword");
                editor.Data["Chat_MonsterSlayer0"] = Helper.Translation.Get("Chat_MonsterSlayer0");
                editor.Data["Chat_MonsterSlayer1"] = Helper.Translation.Get("Chat_MonsterSlayer1");
                editor.Data["Chat_MonsterSlayer2"] = Helper.Translation.Get("Chat_MonsterSlayer2");
                editor.Data["Chat_MonsterSlayer3"] = Helper.Translation.Get("Chat_MonsterSlayer3");
                });
            }
            else if (e.NameWithoutLocale.IsEquivalentTo("Data/ObjectInformation"))
            {
                e.Edit(delegate (IAssetData data) {
                    var editor = data.AsDictionary<int, string>();
                    editor.Data[527] = Helper.Translation.Get("ObjectInformation-527");
                    editor.Data[531] = Helper.Translation.Get("ObjectInformation-531");
                    editor.Data[532] = Helper.Translation.Get("ObjectInformation-532");
                    editor.Data[533] = Helper.Translation.Get("ObjectInformation-533");
                    editor.Data[534] = Helper.Translation.Get("ObjectInformation-534");
                    editor.Data[521] = Helper.Translation.Get("ObjectInformation-521");
                    editor.Data[522] = Helper.Translation.Get("ObjectInformation-522");
                    editor.Data[523] = Helper.Translation.Get("ObjectInformation-523");
                });
            }
            else if (e.NameWithoutLocale.IsEquivalentTo("Strings/StringsFromCSFiles"))
            {
                e.Edit(delegate (IAssetData data) {
                    var editor = data.AsDictionary<string, string>();
                editor.Data["SkillsPage.cs.11608"] = Helper.Translation.Get("SkillsPage.cs.11608");
                editor.Data["Buff.cs.463"] = Helper.Translation.Get("Buff.cs.463");
                editor.Data["Buff.cs.469"] = Helper.Translation.Get("Buff.cs.469");
                editor.Data["Buff.cs.504"] = Helper.Translation.Get("Buff.cs.504");
                editor.Data["Farmer.cs.1996"] = Helper.Translation.Get("Farmer.cs.1996");
                editor.Data["Event.cs.1205"] = Helper.Translation.Get("Event.cs.1205");
                editor.Data["Event.cs.1209"] = Helper.Translation.Get("Event.cs.1209");
                editor.Data["Utility.cs.5567"] = Helper.Translation.Get("Utility.cs.5567");
                editor.Data["Utility.cs.5583"] = Helper.Translation.Get("Utility.cs.5583");
                editor.Data["MeleeWeapon.cs.14122"] = Helper.Translation.Get("MeleeWeapon.cs.14122");
                editor.Data["Sword.cs.14290"] = Helper.Translation.Get("Sword.cs.14290");
                editor.Data["Sword.cs.14291"] = Helper.Translation.Get("Sword.cs.14291");
                editor.Data["Sword.cs.14292"] = Helper.Translation.Get("Sword.cs.14292");
                editor.Data["Sword.cs.14293"] = Helper.Translation.Get("Sword.cs.14293");
                editor.Data["Sword.cs.14294"] = Helper.Translation.Get("Sword.cs.14294");
                editor.Data["Sword.cs.14295"] = Helper.Translation.Get("Sword.cs.14295");
                editor.Data["Sword.cs.14296"] = Helper.Translation.Get("Sword.cs.14296");
                editor.Data["Sword.cs.14297"] = Helper.Translation.Get("Sword.cs.14297");
                editor.Data["Tool.cs.14306"] = Helper.Translation.Get("Tool.cs.14306");
                editor.Data["Tool.cs.14304"] = Helper.Translation.Get("Tool.cs.14304");
                editor.Data["Tool.cs.14305"] = Helper.Translation.Get("Tool.cs.14305");
                editor.Data["ShopMenu.cs.11518"] = Helper.Translation.Get("ShopMenu.cs.11518");
                editor.Data["ShopMenu.cs.11520"] = Helper.Translation.Get("ShopMenu.cs.11520");
                editor.Data["SlayMonsterQuest.cs.13696"] = Helper.Translation.Get("SlayMonsterQuest.cs.13696");
                editor.Data["SlayMonsterQuest.cs.13723"] = Helper.Translation.Get("SlayMonsterQuest.cs.13723");
                editor.Data["SlayMonsterQuest.cs.13747"] = Helper.Translation.Get("SlayMonsterQuest.cs.13747");
                editor.Data["SlayMonsterQuest.cs.13750"] = Helper.Translation.Get("SlayMonsterQuest.cs.13750");
                editor.Data["SlayMonsterQuest.cs.13752"] = Helper.Translation.Get("SlayMonsterQuest.cs.13752");
                editor.Data["SlayMonsterQuest.cs.13756"] = Helper.Translation.Get("SlayMonsterQuest.cs.13756");
                editor.Data["SlayMonsterQuest.cs.13764"] = Helper.Translation.Get("SlayMonsterQuest.cs.13764");
                editor.Data["SlayMonsterQuest.cs.13770"] = Helper.Translation.Get("SlayMonsterQuest.cs.13770");
                editor.Data["Stats.cs.5129"] = Helper.Translation.Get("Stats.cs.5129");
                });
            }
            else if (e.NameWithoutLocale.IsEquivalentTo("Data/TV/TipChannel"))
            {
                e.Edit(delegate (IAssetData data) {
                    var editor = data.AsDictionary<string, string>();
                    editor.Data["137"] = Helper.Translation.Get("TipChannel-137");
                });
            }

            if (e.NameWithoutLocale.IsEquivalentTo("LooseSprites/Cursors"))
            {
                Texture2D customTexture = Helper.ModContent.Load<Texture2D>("assets/heart.png");
                e.Edit(delegate (IAssetData data) {
                    var editor = data.AsImage();
                    editor.PatchImage(customTexture, targetArea: new Rectangle(120, 428, 10, 10));
                });
            }
            else if (e.NameWithoutLocale.IsEquivalentTo("Characters/Monsters/Dust Spirit")) // 1t
            {
                Texture2D customTexture = Helper.ModContent.Load<Texture2D>("assets/heart_small_4.png");
                e.Edit(delegate (IAssetData data) {
                    var editor = data.AsImage();
                    editor.PatchImage(customTexture, targetArea: new Rectangle(0, 24, 16, 16));
                });
            }
            else if (e.NameWithoutLocale.IsEquivalentTo("Characters/Monsters/Armored Bug") // 4
                     || e.NameWithoutLocale.IsEquivalentTo("Characters/Monsters/Bug") //4
                     || e.NameWithoutLocale.IsEquivalentTo("Characters/Monsters/Metal Head") // 4
                     || e.NameWithoutLocale.IsEquivalentTo("Characters/Monsters/Haunted Skull") // 4
            )
            {
                Texture2D customTexture = Helper.ModContent.Load<Texture2D>("assets/heart_small_4.png");
                e.Edit(delegate (IAssetData data) {
                    var editor = data.AsImage();
                    editor.PatchImage(customTexture, targetArea: new Rectangle(0, 64, 16, 16));
                });
            }
            else if (e.NameWithoutLocale.IsEquivalentTo("Characters/Monsters/Duggy")) //3t
            {
                Texture2D customTexture = Helper.ModContent.Load<Texture2D>("assets/heart_small_4.png");
                e.Edit(delegate (IAssetData data) {
                    var editor = data.AsImage();
                    editor.PatchImage(customTexture, targetArea: new Rectangle(0, 72, 16, 16));
                });
            }
            else if (e.NameWithoutLocale.IsEquivalentTo("Characters/Monsters/Bat") //4t
                || e.NameWithoutLocale.IsEquivalentTo("Characters/Monsters/Frost Bat") // 4t
                || e.NameWithoutLocale.IsEquivalentTo("Characters/Monsters/Lava Bat") // 4t
                || e.NameWithoutLocale.IsEquivalentTo("Characters/Monsters/Iridium Bat") // 4t
                )
            {
                Texture2D customTexture = Helper.ModContent.Load<Texture2D>("assets/heart_small_4.png");
                e.Edit(delegate (IAssetData data) {
                    var editor = data.AsImage();
                    editor.PatchImage(customTexture, targetArea: new Rectangle(0, 96, 16, 16));
                });
            }
            else if (e.NameWithoutLocale.IsEquivalentTo("Characters/Monsters/Fly") // 5t
                || e.NameWithoutLocale.IsEquivalentTo("Characters/Monsters/Green Slime") // 5t
                || e.NameWithoutLocale.IsEquivalentTo("Characters/Monsters/Grub") // 5t
                || e.NameWithoutLocale.IsEquivalentTo("Characters/Monsters/Iridium Crab") // 5t
                || e.NameWithoutLocale.IsEquivalentTo("Characters/Monsters/Lava Crab") // 5t
                || e.NameWithoutLocale.IsEquivalentTo("Characters/Monsters/Rock Crab") // 5t
                )
            {
                Texture2D customTexture = Helper.ModContent.Load<Texture2D>("assets/heart_small_4.png");
                e.Edit(delegate (IAssetData data) {
                    var editor = data.AsImage();
                    editor.PatchImage(customTexture, targetArea: new Rectangle(0, 120, 16, 16));
                });
            }
            else if (e.NameWithoutLocale.IsEquivalentTo("Characters/Monsters/Pepper Rex"))// unique 32x32
            {
                Texture2D customTexture = Helper.ModContent.Load<Texture2D>("assets/heart_2.png");
                e.Edit(delegate (IAssetData data) {
                    var editor = data.AsImage();
                    editor.PatchImage(customTexture, targetArea: new Rectangle(64, 128, 32, 32));
                });
            }
            else if (e.NameWithoutLocale.IsEquivalentTo("Characters/Monsters/Skeleton") // 4x
                || e.NameWithoutLocale.IsEquivalentTo("Characters/Monsters/Skeleton Mage") // 4x
                )
            {
                Texture2D customTexture = Helper.ModContent.Load<Texture2D>("assets/heart_small_4.png");
                e.Edit(delegate (IAssetData data) {
                    var editor = data.AsImage();
                    editor.PatchImage(customTexture, targetArea: new Rectangle(0, 128, 16, 16));
                });
            }
            else if (e.NameWithoutLocale.IsEquivalentTo("Characters/Monsters/Stone Golem") // 6t
                || e.NameWithoutLocale.IsEquivalentTo("Characters/Monsters/Wilderness Golem") // 6t)
                )
            {
                Texture2D customTexture = Helper.ModContent.Load<Texture2D>("assets/heart_small_4.png");
                e.Edit(delegate (IAssetData data) {
                    var editor = data.AsImage();
                    editor.PatchImage(customTexture, targetArea: new Rectangle(0, 144, 16, 16));
                });
            }
        }

        private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {

            // get Generic Mod Config Menu's API (if it's installed)
            var configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu is not null)
            {

                // register mod
                configMenu.Register(
                    mod: ModManifest,
                    reset: () => Config = new ModConfig(),
                    save: () => Helper.WriteConfig(Config)
                );
                configMenu.AddBoolOption(
                    mod: ModManifest,
                    name: () => Helper.Translation.Get("mod-enabled"),
                    getValue: () => Config.EnableMod,
                    setValue: value => Config.EnableMod = value
                );
                configMenu.AddBoolOption(
                    mod: ModManifest,
                    name: () => Helper.Translation.Get("prevent-unloved-damage"),
                    getValue: () => Config.PreventUnlovedMonsterDamage,
                    setValue: value => Config.PreventUnlovedMonsterDamage = value
                );
                configMenu.AddBoolOption(
                    mod: ModManifest,
                    name: () => Helper.Translation.Get("show-emote"),
                    getValue: () => Config.ShowMonsterHeartEmote,
                    setValue: value => Config.ShowMonsterHeartEmote = value
                );
                configMenu.AddBoolOption(
                    mod: ModManifest,
                    name: () => Helper.Translation.Get("loved-swarm"),
                    getValue: () => Config.LovedMonstersStillSwarm,
                    setValue: value => Config.LovedMonstersStillSwarm = value
                );
                configMenu.AddBoolOption(
                    mod: ModManifest,
                    name: () => Helper.Translation.Get("monsters-ignore-player"),
                    getValue: () => Config.MonstersIgnorePlayer,
                    setValue: value => Config.MonstersIgnorePlayer = value
                );
                configMenu.AddNumberOption(
                    mod: ModManifest,
                    name: () => Helper.Translation.Get("ms-per-love"),
                    getValue: () => Config.MillisecondsPerLove,
                    setValue: value => Config.MillisecondsPerLove = value
                );
                configMenu.AddNumberOption(
                    mod: ModManifest,
                    name: () => Helper.Translation.Get("device-speed-factor"),
                    getValue: () => Config.DeviceSpeedFactor,
                    setValue: value => Config.DeviceSpeedFactor = value
                );
                configMenu.AddNumberOption(
                    mod: ModManifest,
                    name: () => Helper.Translation.Get("area-of-effect-mod"),
                    getValue: () => Config.AreaOfKissEffectModifier,
                    setValue: value => Config.AreaOfKissEffectModifier = value
                );
            }
        }

        private void GameLoop_DayEnding(object sender, StardewModdingAPI.Events.DayEndingEventArgs e)
        {
            foreach(GameLocation l in Game1.locations)
            {
                for (int i = l.characters.Count - 1; i >= 0; i--)
                {
                    NPC npc = l.characters[i];
                    if (npc is Monster && (npc as Monster).Health <= 0)
                        l.characters.RemoveAt(i);
                }
            }
        }

        private static void DustSpirit_behaviorAtGameTick_Postfix(DustSpirit __instance, ref bool ___runningAwayFromFarmer, ref bool ___chargingFarmer)
        {
            if (__instance.Health <= 0 ||Config.MonstersIgnorePlayer)
            {
                ___runningAwayFromFarmer = false;
                ___chargingFarmer = false;
                __instance.controller = null;
            }
        }

        private static void GreenSlime_behaviorAtGameTick_prefix(GreenSlime __instance, GameTime time, ref int ___readyToJump)
        {
            if (__instance.Health <= 0 || Config.MonstersIgnorePlayer)
            {
                ___readyToJump = -1;
            }
        }
        private static void Fly_updateAnimation_prefix(Fly __instance, GameTime time, ref int ___invincibleCountdown)
        {
            if (__instance.Health <= 0 || Config.MonstersIgnorePlayer)
            {
                ___invincibleCountdown = 1;
            }
        }

        private static void Bat_behaviorAtGameTick_Postfix(Bat __instance)
        {
            try
            {
                if (__instance.Health <= 0 || Config.MonstersIgnorePlayer)
                {
                    __instance.xVelocity = 0f;
                    __instance.yVelocity = 0f;
                }
            }
            catch(Exception ex)
            {
                SMonitor.Log($"Failed in {nameof(Bat_behaviorAtGameTick_Postfix)}:\n{ex}", LogLevel.Error);
            }
        }
        private static bool Serpent_updateAnimation_prefix(Serpent __instance, GameTime time)
        {
            if (__instance.Health <= 0 || Config.MonstersIgnorePlayer)
            {
                var ftn = typeof(Monster).GetMethod("updateAnimation", BindingFlags.NonPublic | BindingFlags.Instance).MethodHandle.GetFunctionPointer();
                var action = (Action<GameTime>)Activator.CreateInstance(typeof(Action<GameTime>), __instance, ftn);
                action(time);

                __instance.Sprite.Animate(time, 0, 9, 40f);

                typeof(Monster).GetMethod("resetAnimationSpeed", BindingFlags.NonPublic | BindingFlags.Instance).Invoke((Monster)__instance, new object[] { });
                return false;
            }
            return true;
        }
        private static bool Monster_updateMovement_prefix(Monster __instance, GameTime time)
        {
            if ((__instance.Health <= 0 && __instance.IsWalkingTowardPlayer) || Config.MonstersIgnorePlayer)
            {
                __instance.defaultMovementBehavior(time);
                return false;
            }
            return true;
        }
        private static void ShadowShaman_behaviorAtGameTick_prefix(SquidKid __instance, ref NetBool ___casting)
        {
            if (__instance.Health <= 0 || Config.MonstersIgnorePlayer)
            {
                ___casting.Value = false;
            }
        }

        private static void SquidKid_behaviorAtGameTick_prefix(ref GameTime time, SquidKid __instance, ref float ___lastFireball)
        {
            if (__instance.Health <= 0 || Config.MonstersIgnorePlayer)
            {
                ___lastFireball = Math.Max(1f, ___lastFireball);
                time = new GameTime(TimeSpan.Zero, TimeSpan.Zero);
                if (!Config.LovedMonstersStillSwarm)
                {
                    __instance.moveTowardPlayerThreshold.Value = -1;
                }
            }
        }

        private static void DinoMonster_behaviorAtGameTick_postfix(DinoMonster __instance, ref int ___nextFireTime)
        {
            if (__instance.Health <= 0 || Config.MonstersIgnorePlayer)
            {
                ___nextFireTime = 0;
            }
        }

        private static bool Skeleton_behaviorAtGameTick_prefix(Skeleton __instance, GameTime time)
        {
            if (__instance.Health <= 0 || Config.MonstersIgnorePlayer)
            {
                var ftn = typeof(Monster).GetMethod("behaviorAtGameTick", BindingFlags.Public | BindingFlags.Instance).MethodHandle.GetFunctionPointer();
                var action = (Action<GameTime>)Activator.CreateInstance(typeof(Action<GameTime>), __instance, ftn);
                action(time);
                return false;
            }
            return true;
        }

        private static void Skeleton_behaviorAtGameTick_postfix(Skeleton __instance, ref NetBool ___throwing)
        {
            if (__instance.Health <= 0 || Config.MonstersIgnorePlayer)
            {
                __instance.Sprite.StopAnimation();
                ___throwing.Value = false;
            }
        }

        private static void Grub_behaviorAtGameTick_postfix(Grub __instance, GameTime time, ref int ___metamorphCounter, ref NetBool ___pupating)
        {
            if (___pupating && ___metamorphCounter <= time.ElapsedGameTime.Milliseconds)
            {
                    __instance.Health = -500;
                    __instance.currentLocation.characters.Add(new Fly(__instance.Position, __instance.hard.Value)
                    {
                        currentLocation = __instance.currentLocation
                    });
                    __instance.currentLocation.characters.Remove(__instance);
                    ___metamorphCounter = 200000;
                
            }
        }

        private static void monsterDrop_prefix(MineShaft __instance, NetVector2 ___netTileBeneathLadder)
        {
            if (__instance.mustKillAllMonstersToAdvance())
            {
                int monsters = 0;
                foreach(Character c in __instance.characters)
                {
                    if(c is Monster && (c as Monster).Health > 0)
                    {
                        monsters++;
                    }
                }
                if(monsters <= 1)
                {
                    Vector2 p = new Vector2((float)((int)___netTileBeneathLadder.X), (float)((int)___netTileBeneathLadder.Y));
                    __instance.createLadderAt(p, "newArtifact");
                    if (Game1.player.currentLocation == __instance)
                    {
                        Game1.showGlobalMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:MineShaft.cs.9484"));
                    }
                }
            }
        }

        private static bool beginSpecialMove_prefix(MeleeWeapon __instance, Farmer who)
        {
            __instance.leftClick(who);
            return false;
        }

        private static bool drawCharacters_prefix(GameLocation __instance, SpriteBatch b)
        {
            if (__instance.shouldHideCharacters())
            {
                return false;
            }
            if (!Game1.eventUp)
            {
                for (int i = 0; i < __instance.characters.Count; i++)
                {
                    if (__instance.characters[i] != null)
                    {
                        NPC npc = __instance.characters[i];
                        npc.draw(b);
                        if (npc is Monster && npc.IsEmoting)
                        {
                            Vector2 emotePosition = npc.getLocalPosition(Game1.viewport);
                            emotePosition.Y -= (float)(32 + npc.Sprite.SpriteHeight * 4);
                            b.Draw(Game1.emoteSpriteSheet, emotePosition, new Rectangle?(new Rectangle(npc.CurrentEmoteIndex * 16 % Game1.emoteSpriteSheet.Width, npc.CurrentEmoteIndex * 16 / Game1.emoteSpriteSheet.Width * 16, 16, 16)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)npc.getStandingY() / 10000f);
                        }
                    }
                }
            }
            return false;
        }
        public static IEnumerable<CodeInstruction> damageMonster_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            SMonitor.Log($"Transpiling GameLocation.damageMonster!");

            var codes = new List<CodeInstruction>(instructions);
            var newCodes = new List<CodeInstruction>();
            bool startLooking = false;
            bool startSkipping = false;
            bool stopLooking = false;
            for (int i = 0; i < codes.Count; i++)
            {
                if (startLooking && !stopLooking)
                {
                    if (startSkipping && codes[i].opcode == OpCodes.Pop)
                    {
                        SMonitor.Log($"popped!");
                        newCodes.Add(codes[i]);
                        stopLooking = true;
                    }
                    else if (!startSkipping && codes[i].opcode == OpCodes.Ldarg_0)
                    {
                        SMonitor.Log($"start skipping!");
                        startSkipping = true;
                    }
                    if (startSkipping)
                    {
                        codes[i].opcode = OpCodes.Nop;
                        codes[i].operand = null;
                        SMonitor.Log($"nullifying {codes[i].opcode}");
                    }
                }
                else if (!stopLooking && codes[i].opcode == OpCodes.Ldstr && (codes[i].operand as string) == "hardModeMonstersKilled")
                {
                    SMonitor.Log($"got hardModeMonstersKilled!");
                    startLooking = true;
                }
                newCodes.Add(codes[i]);
            }

            return newCodes.AsEnumerable();
        }
        private static void damageMonster_Postfix(GameLocation __instance)
        {
            for (int i = __instance.characters.Count - 1; i >= 0; i--)
            {
                Monster monster;
                if ((monster = (__instance.characters[i] as Monster)) != null && monster.Health <= 0)
                    monster.farmerPassesThrough = true;
            }
        }

        public static IEnumerable<CodeInstruction> updateCharacters_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            SMonitor.Log($"Transpiling GameLocation.spawnObjects");

            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; i++)
            {
                if (i > 3 && codes[i].opcode == OpCodes.Callvirt && (MethodInfo)codes[i].operand == AccessTools.Method(typeof(NetCollection<NPC>), nameof(NetCollection<NPC>.RemoveAt)))
                {
                    SMonitor.Log("Overriding remove dead monster");
                    codes[i - 2].opcode = OpCodes.Ldloc_2;
                    codes[i - 2].operand = null;
                    codes[i].opcode = OpCodes.Call;
                    codes[i].operand = AccessTools.Method(typeof(ModEntry), nameof(ModEntry.EmoteMonster));
                    break;
                }
            }
            return codes.AsEnumerable();
        }

        private static void EmoteMonster(GameLocation location, NPC monster, int i)
        {
            if (Config.EnableMod && Config.ShowMonsterHeartEmote)
                monster.doEmote(20, true);
        }

        private static bool setFarmerAnimating_prefix(MeleeWeapon __instance, Farmer who, ref bool ___hasBegunWeaponEndPause, ref bool ___anotherClick, Farmer ___lastUser)
        {
            if (__instance.isScythe(-1))
            {
                return true;
            }
            Vector2 actionTile = who.GetToolLocation(true);
            Vector2 tileLocation = Vector2.Zero;
            Vector2 tileLocation2 = Vector2.Zero;
            Rectangle areaOfEffect = __instance.getAreaOfEffect((int)actionTile.X, (int)actionTile.Y, who.FacingDirection, ref tileLocation, ref tileLocation2, who.GetBoundingBox(), 666);
            areaOfEffect.Inflate(Config.AreaOfKissEffectModifier, Config.AreaOfKissEffectModifier);
            who.currentLocation.damageMonster(areaOfEffect, (int)((float)__instance.minDamage.Value * (1f + who.attackIncreaseModifier)), (int)((float)__instance.maxDamage.Value * (1f + who.attackIncreaseModifier)), false, __instance.knockback.Value * (1f + who.knockbackModifier), (int)((float)__instance.addedPrecision.Value * (1f + who.weaponPrecisionModifier)), __instance.critChance.Value * (1f + who.critChanceModifier), __instance.critMultiplier.Value * (1f + who.critPowerModifier), __instance.type.Value != 1 || !__instance.isOnSpecial, ___lastUser);

            GameLocation location = who.currentLocation;

            foreach (Vector2 v in Utility.removeDuplicates(Utility.getListOfTileLocationsForBordersOfNonTileRectangle(areaOfEffect)))
            {
                if (location.terrainFeatures.ContainsKey(v) && location.terrainFeatures[v].performToolAction(__instance, 0, v, location))
                {
                    location.terrainFeatures.Remove(v);
                }
                if (location.objects.ContainsKey(v) && location.objects[v].performToolAction(__instance, location))
                {
                    location.objects.Remove(v);
                }
                if (location.performToolAction(__instance, (int)v.X, (int)v.Y))
                {
                    break;
                }
            }

            ___anotherClick = false;
            location.playSound("dwop", NetAudio.SoundContext.Default);

            int speed = (int)Math.Round((Config.MillisecondsPerLove - __instance.speed.Value * Config.DeviceSpeedFactor*10)/10f)*10;

            who.faceDirection(who.facingDirection);
            who.FarmerSprite.PauseForSingleAnimation = false;
            who.FarmerSprite.animateOnce(new List<FarmerSprite.AnimationFrame>
            {
                new FarmerSprite.AnimationFrame(101, Utility.Clamp(speed,100,10000), 0, false, who.FacingDirection == 3, null, false, 0),
                new FarmerSprite.AnimationFrame(6, 1, false, who.FacingDirection == 3, new AnimatedSprite.endOfAnimationBehavior(Farmer.completelyStopAnimating), false)
            }.ToArray(), null);

            who.doEmote(20, true);
            return false;
        }
        private static bool takeDamage_prefix(Farmer __instance, Monster damager)
        {
            if(damager != null && damager.Health <= 0)
            {
                __instance.doEmote(20, true);
            }
            else if (!Config.PreventUnlovedMonsterDamage)
            {
                return true;
            }
            //__instance.temporarilyInvincible = true;
            //__instance.currentLocation.playSound("dwop", NetAudio.SoundContext.Default);
            return false;
        }
        public static IEnumerable<CodeInstruction> FarmHouse_UpdateWhenCurrentLocation_Transpiler(IEnumerable<CodeInstruction> instructions)
        {

            SMonitor.Log($"Transpiling FarmHouse.UpdateWhenCurrentLocation");

            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Callvirt && codes[i].operand is MethodInfo && (MethodInfo)codes[i].operand == AccessTools.Method(typeof(Farmer), nameof(Farmer.isMarried)))
                {
                    SMonitor.Log($"Adding prevent method");

                    codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.PreventMonsterAttack))));
                    break;
                }
            }

            return codes.AsEnumerable();
        }

        private static bool PreventMonsterAttack(bool married)
        {
            return false;
        }
    }
}