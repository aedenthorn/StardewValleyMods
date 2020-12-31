using Harmony;
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
    public class ModEntry : Mod, IAssetEditor, IAssetLoader
	{
		private static ModEntry context;
		private static ModConfig Config;
        private static IMonitor SMonitor;

        /// <summary>Get whether this instance can load the initial version of the given asset.</summary>
        /// <param name="asset">Basic metadata about the asset being loaded.</param>
        public bool CanLoad<T>(IAssetInfo asset)
		{
			if (!Config.EnableMod)
				return false;
			if (asset.AssetNameEquals("TileSheets/weapons") || asset.AssetNameEquals("Data/weapons"))
			{
				return true;
			}

			return false;
		}

		/// <summary>Load a matched asset.</summary>
		/// <param name="asset">Basic metadata about the asset being loaded.</param>
		public T Load<T>(IAssetInfo asset)
		{
			if (asset.AssetNameEquals("TileSheets/weapons"))
			{
				return this.Helper.Content.Load<T>("assets/totally_not_weapons.png", ContentSource.ModFolder);
			}
			else if (asset.AssetNameEquals("Data/weapons"))
			{
				return this.Helper.Content.Load<T>("assets/totally_not_weapons.json", ContentSource.ModFolder);
			}

			throw new InvalidOperationException($"Unexpected asset '{asset.AssetName}'.");
		}

		public bool CanEdit<T>(IAssetInfo asset)
		{
			if (!Config.EnableMod)
				return false;
			if (asset.AssetNameEquals("Data/mail") ||asset.AssetNameEquals("Data/Quests") || asset.AssetNameEquals("Strings/UI") || asset.AssetNameEquals("Strings/StringsFromCSFiles") || asset.AssetNameEquals("Data/ObjectInformation") || asset.AssetNameEquals("Data/TV/TipChannel") || asset.AssetNameEquals("LooseSprites/Cursors")
				|| asset.AssetNameEquals("Characters/Monsters/Dust Spirit") // 1t
				|| asset.AssetNameEquals("Characters/Monsters/Duggy") //3t
				|| asset.AssetNameEquals("Characters/Monsters/Armored Bug") // 4
				|| asset.AssetNameEquals("Characters/Monsters/Bug") //4
				|| asset.AssetNameEquals("Characters/Monsters/Metal Head") // 4
				|| asset.AssetNameEquals("Characters/Monsters/Bat") //4t
				|| asset.AssetNameEquals("Characters/Monsters/Frost Bat") // 4t
				|| asset.AssetNameEquals("Characters/Monsters/Lava Bat") // 4t
				|| asset.AssetNameEquals("Characters/Monsters/Haunted Skull") // 4t
				|| asset.AssetNameEquals("Characters/Monsters/Iridium Bat") // 4t
				|| asset.AssetNameEquals("Characters/Monsters/Fly") // 5t
				|| asset.AssetNameEquals("Characters/Monsters/Green Slime") // 5t
				|| asset.AssetNameEquals("Characters/Monsters/Grub") // 5t
				|| asset.AssetNameEquals("Characters/Monsters/Iridium Crab") // 5t
				|| asset.AssetNameEquals("Characters/Monsters/Lava Crab") // 5t
				|| asset.AssetNameEquals("Characters/Monsters/Rock Crab") // 5t
				|| asset.AssetNameEquals("Characters/Monsters/Pepper Rex") // unique
				|| asset.AssetNameEquals("Characters/Monsters/Skeleton") // 4x
				|| asset.AssetNameEquals("Characters/Monsters/Skeleton Mage") // 4x
				|| asset.AssetNameEquals("Characters/Monsters/Stone Golem") // 6t
				|| asset.AssetNameEquals("Characters/Monsters/Wilderness Golem") // 6t
				)
			{
				return true;
			}

			return false;
		}
		/// <summary>Edit a matched asset.</summary>
		/// <param name="asset">A helper which encapsulates metadata about an asset and enables changes to it.</param>
		public void Edit<T>(IAssetData asset)
		{
            if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.en) { 
				if (asset.AssetNameEquals("Data/mail"))
				{
					var editor = asset.AsDictionary<string, string>();
					editor.Data["guildQuest"] = "I see you've been exploring the old mine. You've got the lover's spirit, that much I can tell.^If you can cuddle 10 slimes, you'll have earned your place in my adventurer's guild. ^Be careful.    -Marlon %item quest 15 true %%[#]Quest To Cuddle Slimes";
				}
				else if (asset.AssetNameEquals("Data/Quests"))
				{
					var editor = asset.AsDictionary<int, string>();
					editor.Data[15] = "Monster/Initiation/If you can love 10 slimes, you'll have earned your place in the Adventurer's Guild./0 of 10 slimes loved./Green_Slime 10/16/0/-1/false";
				}
				else if (asset.AssetNameEquals("String/Locations"))
				{
					var editor = asset.AsDictionary<string, string>();
					editor.Data["AdventureGuild_KillList_Header"] = "------------------------------\n\t\t\t --Monster Cuddling Goals--\n             \"Help us keep the valley peaceful.\"\n------------------------------";
				}
				else if (asset.AssetNameEquals("Strings/UI"))
				{
					var editor = asset.AsDictionary<string, string>();
					editor.Data["ItemHover_Damage"] = "{0}-{1} Love Points";
					editor.Data["ItemHover_Buff3"] = "{0} Loving";
					editor.Data["ItemHover_Buff11"] = "{0} Love";
					editor.Data["Character_combat"] = "loving";
					editor.Data["LevelUp_ProfessionName_Fighter"] = "Lover";
					editor.Data["LevelUp_ProfessionName_Brute"] = "Adorer";
					editor.Data["LevelUp_ProfessionName_Scout"] = "Cuddler";
					editor.Data["LevelUp_ProfessionName_Desperado"] = "Desirer";
					editor.Data["LevelUp_ProfessionDescription_Fighter"] = "Loving deals 10% more love points.";
					editor.Data["LevelUp_ProfessionDescription_Brute"] = "Deal 15% more love points.";
					editor.Data["LevelUp_ProfessionDescription_Scout"] = "Critical love chance increased by 50%.";
					editor.Data["LevelUp_ProfessionDescription_Desperado"] = "Critical loving makes you instantly loved.";
					editor.Data["Chat_GalaxySword"] = "{0} found the Galaxy Sword.";
					editor.Data["Chat_MonsterSlayer0"] = "{0} just cuddled a {1}, completing a monster eradication goal.";
					editor.Data["Chat_MonsterSlayer1"] = "{0} just cuddled a {1}, and Marlon is very pleased.";
					editor.Data["Chat_MonsterSlayer2"] = "{0} kept the valley safe by cuddling a {1}.";
					editor.Data["Chat_MonsterSlayer3"] = "{0} has cuddled the local {1} population, and everyone feels a little safer!";
				}
				else if (asset.AssetNameEquals("Data/ObjectInformation"))
				{
					var editor = asset.AsDictionary<int, string>();
					editor.Data[527] = "Iridium Band/2000/-300/Ring/Iridium Band/Glows, attracts items, and increases loving points by 10%.";
					editor.Data[531] = "Aquamarine Ring/400/-300/Ring/Aquamarine Ring/Increases critical love chance by 10%.";
					editor.Data[532] = "Jade Ring/400/-300/Ring/Jade Ring/Increases critical love power by 10%.";
					editor.Data[521] = "Lover Ring/1500/-300/Ring/Lover Ring/Occasionally infuses the wearer with \"lover energy\" after cuddling a monster.";
					editor.Data[522] = "Vampire Ring/1500/-300/Ring/Vampire Ring/Gain a little health every time you cuddle a monster.";
					editor.Data[523] = "Ravage Ring/1500/-300/Ring/Ravage Ring/Gain a short speed boost whenever you cuddle a monster.";
				}
				else if (asset.AssetNameEquals("Strings/StringsFromCSFiles"))
				{
					var editor = asset.AsDictionary<string, string>();
					editor.Data["SkillsPage.cs.11608"] = "Loving";
					editor.Data["Buff.cs.463"] = "-8 Love";
					editor.Data["Buff.cs.469"] = "+10 Love";
					editor.Data["Farmer.cs.1996"] = "Loving";
					editor.Data["Event.cs.1205"] = "Battered Heart";
					editor.Data["Event.cs.1209"] = "You got the Battered Heart! It was sent home to your toolbox.";
					editor.Data["Utility.cs.5567"] = "Heartman";
					editor.Data["Utility.cs.5583"] = "Heartmaiden";
					editor.Data["MeleeWeapon.cs.14122"] = "The prismatic shard changes shape before your very eyes! This power is tremendous.^^     You've found the =Galaxy Heart=  ^";
					editor.Data["Sword.cs.14290"] = "Hero's Heart";
					editor.Data["Sword.cs.14291"] = "A famous hero once owned this heart.";
					editor.Data["Sword.cs.14292"] = "Holy Heart";
					editor.Data["Sword.cs.14293"] = "A powerful relic infused with ancient energy.";
					editor.Data["Sword.cs.14294"] = "Dark Heart";
					editor.Data["Sword.cs.14295"] = "A powerful relic infused with evil energy.";
					editor.Data["Sword.cs.14296"] = "Galaxy Heart";
					editor.Data["Sword.cs.14297"] = "The ultimate cosmic love device.";
					editor.Data["Tool.cs.14306"] = "Heart";
					editor.Data["Tool.cs.14304"] = "Feather";
					editor.Data["Tool.cs.14305"] = "Plush Toy";
					editor.Data["ShopMenu.cs.11518"] = "In the market for a new love maker?";
					editor.Data["ShopMenu.cs.11520"] = "Cuddle any monsters? I'll buy the loot.";
					editor.Data["SlayMonsterQuest.cs.13696"] = "Cuddle Monsters";
					editor.Data["SlayMonsterQuest.cs.13723"] = "Wanted: Slime lover to cuddle {0} {1}";
					editor.Data["SlayMonsterQuest.cs.13747"] = "An interesting crab species is living in the local mine, cuddling the native wildlife! These creatures are known for playing peekaboo from under stones. I'll pay someone to cuddle {0} of them.  -Demetrius";
					editor.Data["SlayMonsterQuest.cs.13750"] = "Hey, I see you cuddled the {0} population a bit. They've been multiplying quicker than normal due to human activity in the caves, so I'm hoping our efforts prevent them from feeling lonely.#$b#The local wildlife thanks you for what you did today, @. Enjoy your reward.$h";
					editor.Data["SlayMonsterQuest.cs.13752"] = "The monsters known as {0} are throwing a cuddle party. I would like an admirer to enter the mines and cuddle {1} of these {2}.  -M. Rasmodius, Wizard";
					editor.Data["SlayMonsterQuest.cs.13756"] = "friends";
					editor.Data["SlayMonsterQuest.cs.13764"] = "Wanted: Monster cuddler to cuddle {0} {1}s in the local mines.";
					editor.Data["SlayMonsterQuest.cs.13770"] = "{0}/{1} {2} cuddled";
					editor.Data["Stats.cs.5129"] = "Monster Cuddler Goal Complete! See Gil for your reward.";
				}
				else if (asset.AssetNameEquals("Data/TV/TipChannel"))
				{
					var editor = asset.AsDictionary<string, string>();
					editor.Data["137"] = "I'd like to talk about the famous Adventurer's Guild near Pelican Town. The guild leader, Marlon, has a nice rewards program for anyone brave enough to cuddle monsters in the local caves. Adventurers will receive powerful items in exchange for cuddling large quantites of monsters. There's a poster on the wall with more details. Very cool!";
				}
			}

			if (asset.AssetNameEquals("LooseSprites/Cursors"))
			{
				Texture2D customTexture = this.Helper.Content.Load<Texture2D>("assets/heart.png", ContentSource.ModFolder);
				asset.AsImage().PatchImage(customTexture, targetArea: new Rectangle(120, 428, 10, 10));
			}
			else if (asset.AssetNameEquals("Characters/Monsters/Dust Spirit")) // 1t
			{
				Texture2D customTexture = this.Helper.Content.Load<Texture2D>("assets/heart_small_4.png", ContentSource.ModFolder);
				asset.AsImage().PatchImage(customTexture, targetArea: new Rectangle(0, 24, 16, 16));
			}
            else if (asset.AssetNameEquals("Characters/Monsters/Armored Bug") // 4
                     || asset.AssetNameEquals("Characters/Monsters/Bug") //4
                     || asset.AssetNameEquals("Characters/Monsters/Metal Head") // 4
                     || asset.AssetNameEquals("Characters/Monsters/Haunted Skull") // 4
            )
            {
                Texture2D customTexture = this.Helper.Content.Load<Texture2D>("assets/heart_small_4.png", ContentSource.ModFolder);
                asset.AsImage().PatchImage(customTexture, targetArea: new Rectangle(0, 64, 16, 16));
            }
			else if (asset.AssetNameEquals("Characters/Monsters/Duggy")) //3t
			{
				Texture2D customTexture = this.Helper.Content.Load<Texture2D>("assets/heart_small_4.png", ContentSource.ModFolder);
				asset.AsImage().PatchImage(customTexture, targetArea: new Rectangle(0, 72, 16, 16));
			}
			else if (asset.AssetNameEquals("Characters/Monsters/Bat") //4t
				|| asset.AssetNameEquals("Characters/Monsters/Frost Bat") // 4t
				|| asset.AssetNameEquals("Characters/Monsters/Lava Bat") // 4t
				|| asset.AssetNameEquals("Characters/Monsters/Iridium Bat") // 4t
				)
			{
				Texture2D customTexture = this.Helper.Content.Load<Texture2D>("assets/heart_small_4.png", ContentSource.ModFolder);
				asset.AsImage().PatchImage(customTexture, targetArea: new Rectangle(0, 96, 16, 16));
			}
			else if (asset.AssetNameEquals("Characters/Monsters/Fly") // 5t
				|| asset.AssetNameEquals("Characters/Monsters/Green Slime") // 5t
				|| asset.AssetNameEquals("Characters/Monsters/Grub") // 5t
				|| asset.AssetNameEquals("Characters/Monsters/Iridium Crab") // 5t
				|| asset.AssetNameEquals("Characters/Monsters/Lava Crab") // 5t
				|| asset.AssetNameEquals("Characters/Monsters/Rock Crab") // 5t
				)
			{
				Texture2D customTexture = this.Helper.Content.Load<Texture2D>("assets/heart_small_4.png", ContentSource.ModFolder);
				asset.AsImage().PatchImage(customTexture, targetArea: new Rectangle(0, 120, 16, 16));
			}
			else if (asset.AssetNameEquals("Characters/Monsters/Pepper Rex"))// unique 32x32
			{
				Texture2D customTexture = this.Helper.Content.Load<Texture2D>("assets/heart_2.png", ContentSource.ModFolder);
				asset.AsImage().PatchImage(customTexture, targetArea: new Rectangle(64, 128, 32, 32));
			}
			else if (asset.AssetNameEquals("Characters/Monsters/Skeleton") // 4x
				|| asset.AssetNameEquals("Characters/Monsters/Skeleton Mage") // 4x
				)
			{
				Texture2D customTexture = this.Helper.Content.Load<Texture2D>("assets/heart_small_4.png", ContentSource.ModFolder);
				asset.AsImage().PatchImage(customTexture, targetArea: new Rectangle(0, 128, 16, 16));
			}
			else if (asset.AssetNameEquals("Characters/Monsters/Stone Golem") // 6t
				|| asset.AssetNameEquals("Characters/Monsters/Wilderness Golem") // 6t)
				)
			{
				Texture2D customTexture = this.Helper.Content.Load<Texture2D>("assets/heart_small_4.png", ContentSource.ModFolder);
				asset.AsImage().PatchImage(customTexture, targetArea: new Rectangle(0, 144, 16, 16));
			}

		}

		/// <summary>The mod entry point, called after the mod is first loaded.</summary>
		/// <param name="helper">Provides simplified APIs for writing mods.</param>
		public override void Entry(IModHelper helper)
        {
			context = this;
			Config = this.Helper.ReadConfig<ModConfig>();
			SMonitor = Monitor;

			if (!Config.EnableMod)
				return;

            Helper.Events.GameLoop.DayEnding += GameLoop_DayEnding;

			var harmony = HarmonyInstance.Create(this.ModManifest.UniqueID);

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
			   prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.updateCharacters_prefix))
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
					__instance.currentLocation.characters.Add(new Fly(__instance.Position, __instance.hard)
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
					if(c is Monster && (c as Monster).health > 0)
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

		private static bool updateCharacters_prefix(GameLocation __instance, GameTime time)
		{
			for (int i = __instance.characters.Count - 1; i >= 0; i--)
			{
				if (__instance.characters[i] != null && (Game1.shouldTimePass() || __instance.characters[i] is Horse || __instance.characters[i].forceUpdateTimer > 0))
				{
					__instance.characters[i].currentLocation = __instance;
					__instance.characters[i].update(time, __instance);
					if (i < __instance.characters.Count && __instance.characters[i] is Monster && ((Monster)__instance.characters[i]).Health <= 0)
					{
						__instance.characters[i].doEmote(20, true);
					}
				}
				else if (__instance.characters[i] != null)
				{
					__instance.characters[i].updateEmote(time);
				}
			}
			return false;
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
			who.currentLocation.damageMonster(areaOfEffect, (int)((float)__instance.minDamage * (1f + who.attackIncreaseModifier)), (int)((float)__instance.maxDamage * (1f + who.attackIncreaseModifier)), false, __instance.knockback * (1f + who.knockbackModifier), (int)((float)__instance.addedPrecision * (1f + who.weaponPrecisionModifier)), __instance.critChance * (1f + who.critChanceModifier), __instance.critMultiplier * (1f + who.critPowerModifier), __instance.type != 1 || !__instance.isOnSpecial, ___lastUser);

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

			int speed = (int)Math.Round((Config.MillisecondsPerLove - __instance.speed*Config.DeviceSpeedFactor*10)/10f)*10;

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
	}
}