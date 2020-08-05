using Harmony;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Characters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace YAJM
{
	public class ModEntry : Mod, IAssetEditor
	{
		public static ModEntry context;

		public static ModConfig Config;
		private float lastYJumpVelocity;
		private float velX;
		private float velY;
		private static Texture2D horseShadow;
        private static Texture2D horse;
        private static bool playerJumping;
        private bool playerJumpingOver;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
		{
			context = this;
			Config = Helper.ReadConfig<ModConfig>();
			if (!Config.EnableMod)
				return;

			Helper.Events.Input.ButtonPressed += Input_ButtonPressed;

			HarmonyInstance harmony = HarmonyInstance.Create(Helper.ModRegistry.ModID);

			harmony.Patch(
			   original: AccessTools.Method(typeof(NPC), nameof(NPC.draw), new Type[] { typeof(SpriteBatch), typeof(float) }),
			   prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.NPC_draw_prefix))
			);
			harmony.Patch(
			   original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.isCollidingPosition), new Type[] { typeof(Rectangle), typeof(xTile.Dimensions.Rectangle), typeof(bool), typeof(int), typeof(bool), typeof(Character), typeof(bool), typeof(bool), typeof(bool) }),
			   prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.GameLocation_isCollidingPosition_prefix))
			);

			horseShadow = Helper.Content.Load<Texture2D>(Path.Combine("assets", "horse_shadow.png"));
			horse = Helper.Content.Load<Texture2D>(Path.Combine("assets", "horse.png"));
			
		}

        private static bool GameLocation_isCollidingPosition_prefix(bool isFarmer, ref bool __result)
        {
			if (isFarmer && playerJumping)
			{
				__result = false;
				return false;
			}
			return true;
		}

		private static void NPC_draw_prefix(ref NPC __instance, SpriteBatch b, float alpha)
		{
			if (__instance is Horse)
			{
				b.Draw(horseShadow, __instance.getLocalPosition(Game1.viewport) + new Vector2((float)(__instance.Sprite.SpriteWidth * 4 / 2), (float)(__instance.GetBoundingBox().Height / 2)), new Rectangle?(__instance.Sprite.SourceRect), Color.White * alpha, __instance.rotation, new Vector2((float)(__instance.Sprite.SpriteWidth / 2), (float)__instance.Sprite.SpriteHeight * 3f / 4f), Math.Max(0.2f, __instance.scale) * 4f, (__instance.flip || (__instance.Sprite.CurrentAnimation != null && __instance.Sprite.CurrentAnimation[__instance.Sprite.currentAnimationIndex].flip)) ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0);
				if ((__instance as Horse).rider != null && playerJumping)
					__instance.Position += new Vector2(0, (__instance as Horse).rider.yJumpOffset * 2);
			}
		}

		private void Input_ButtonPressed(object sender, StardewModdingAPI.Events.ButtonPressedEventArgs e)
		{
			if (e.Button == Config.JumpButton && Context.IsPlayerFree && Game1.player.yJumpVelocity == 0)
			{
				if (Config.PlayJumpSound)
					Game1.playSound("dwop");
				velX = 0;
				velY = 0;
				int ox = 0;
				int oy = 0;
				switch (Game1.player.facingDirection.Value)
				{
					case 0:
						oy = -1;
						break;
					case 1:
						ox = 1;
						break;
					case 2:
						oy = 1;
						break;
					case 3:
						ox = -1;
						break;
				}

				int maxJumpDistance = Math.Max(2, Config.MaxJumpDistance);
				GameLocation l = Game1.player.currentLocation;
				List<bool> collisions = new List<bool>();
				for (int i = 0; i < maxJumpDistance; i++)
				{
					Rectangle box = Game1.player.GetBoundingBox();
                    if (Game1.player.isRidingHorse())
                    {
						box.X += ox * 32;
						box.Y += oy * 32;
					}
					box.X += ox * 64 * i;
					box.Y += oy * 64 * i;
					collisions.Add(l.isCollidingPosition(box, Game1.viewport, true, 0, false, Game1.player));
				}

				if (!collisions[0] && !collisions[1])
				{
					PlayerJump(8f);
					return;
				}

				for (int i = 1; i < collisions.Count; i++)
				{
					if (!collisions[i])
					{
						velX = ox * (float)Math.Sqrt(i * 16);
						velY = oy * (float)Math.Sqrt(i * 16);
						lastYJumpVelocity = 0;
						Game1.player.canMove = false;
						playerJumpingOver = true;
						PlayerJump((float)Math.Sqrt(i * 16));
						return;
					}
				}
				Game1.player.synchronizedJump(8f);
			}
		}

		private void PlayerJump(float v)
		{
			playerJumping = true;
			Game1.player.synchronizedJump(v);
			Helper.Events.GameLoop.UpdateTicked += GameLoop_UpdateTicked;
		}

		private void GameLoop_UpdateTicked(object sender, StardewModdingAPI.Events.UpdateTickedEventArgs e)
		{
			if (Game1.player.yJumpVelocity == 0f && lastYJumpVelocity < 0f)
			{
				playerJumping = false;
				playerJumpingOver = false;
				Game1.player.canMove = true;
				Helper.Events.GameLoop.UpdateTicked -= GameLoop_UpdateTicked;
				return;
			}
			Game1.player.position.X += velX;
			Game1.player.position.Y += velY;
			lastYJumpVelocity = Game1.player.yJumpVelocity;
		}


		public bool CanEdit<T>(IAssetInfo asset)
		{
			if (!Config.EnableMod)
				return false;
			if (asset.AssetNameEquals("Animals/Horse"))
			{
				return true;
			}

			return false;
		}
		/// <summary>Edit a matched asset.</summary>
		/// <param name="asset">A helper which encapsulates metadata about an asset and enables changes to it.</param>
		public void Edit<T>(IAssetData asset)
		{
			//Texture2D customTexture = this.Helper.Content.Load<Texture2D>(Path.Combine("assets","horse.png");
			if(!Config.CustomHorseTexture)
				asset.AsImage().ReplaceWith(horse);
		}
	}
}
