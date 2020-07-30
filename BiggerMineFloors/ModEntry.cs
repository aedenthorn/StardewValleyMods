using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.IO;

namespace YAJM
{
    public class ModEntry : Mod 
	{
		public static ModEntry context;

		public static ModConfig Config;
        private int jumps;
        private Vector2 startPos;
        private bool jumping;
        private float lastYJumpVelocity;
        private float origDrawLayer;
        private float velX;
        private float velY;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
		{
			context = this;
			Config = Helper.ReadConfig<ModConfig>();
			if (!Config.EnableMod)
				return;

            Helper.Events.Input.ButtonPressed += Input_ButtonPressed;

        }

        private void Input_ButtonPressed(object sender, StardewModdingAPI.Events.ButtonPressedEventArgs e)
        {
            if(e.Button == Config.JumpButton && Game1.player.yJumpVelocity == 0)
            {
				if(Config.PlayJumpSound)
					Game1.playSound("dwop");
				startPos = Game1.player.position;
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
				for (int i = 0;  i < maxJumpDistance; i++)
                {
					Rectangle box = Game1.player.GetBoundingBox();
					box.X += ox * 64 * i;
					box.Y += oy * 64 * i;
					collisions.Add(l.isCollidingPosition(box, Game1.viewport, true, 0, false, Game1.player));
                }

				if (!collisions[0] && !collisions[1])
				{
					Game1.player.synchronizedJump(8f);
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
						Helper.Events.GameLoop.UpdateTicked += GameLoop_UpdateTicked;
						Game1.player.synchronizedJump((float)Math.Sqrt(i * 16));
						return;
					}
				}
				Game1.player.synchronizedJump(8f);
			}
		}

		private void GameLoop_UpdateTicked(object sender, StardewModdingAPI.Events.UpdateTickedEventArgs e)
        {
			if (Game1.player.yJumpVelocity == 0f && lastYJumpVelocity < 0f)
			{
				Game1.player.canMove = true;
				Helper.Events.GameLoop.UpdateTicked -= GameLoop_UpdateTicked;
				return;
			}
			Game1.player.position.X += velX;
			Game1.player.position.Y += velY;
			lastYJumpVelocity = Game1.player.yJumpVelocity;
		}

    }
}
