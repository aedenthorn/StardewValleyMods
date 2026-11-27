using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Locations;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using System;
using System.Globalization;
using System.Linq;
using Object = StardewValley.Object;

namespace LoadMenuTweaks
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod, IAssetEditor
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;
        public static ModEntry context;

        public static string mapAssetKey;
        public static Texture2D palmTreeTexture;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();

            context = this;

            SMonitor = Monitor;
            SHelper = helper;

            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            helper.Events.GameLoop.Saving += GameLoop_Saving;
            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();
        }

        private void GameLoop_Saving(object sender, SavingEventArgs e)
        {
			Rectangle sourceRect = new Rectangle(0, 0, 16, 32);
			Farmer who = Game1.player;
			AccessTools.Method(typeof(FarmerRenderer), "executeRecolorActions").Invoke(who.FarmerRenderer, new object[] { who });
			AccessTools.FieldRefAccess<FarmerRenderer, Vector2>(who.FarmerRenderer, "rotationAdjustment") = Vector2.Zero;
			AccessTools.FieldRefAccess<FarmerRenderer, Vector2>(who.FarmerRenderer, "positionOffset") = Vector2.Zero;

			Texture2D texture = new Texture2D(Game1.graphics.GraphicsDevice, 16, 32);

			var baseTexture = AccessTools.FieldRefAccess<FarmerRenderer, Texture2D>(who.FarmerRenderer, "baseTexture");
			Color[] data = new Color[16 * 32];
			baseTexture.GetData(0, sourceRect, data, 0, 16 * 32);

			Rectangle pants_rect = sourceRect;
			pants_rect.X += who.FarmerRenderer.ClampPants(who.GetPantsIndex()) % 10 * 192;
			pants_rect.Y += who.FarmerRenderer.ClampPants(who.GetPantsIndex()) / 10 * 688;
			if (!who.IsMale)
			{
				pants_rect.X += 96;
			}

			Color[] pantsData = new Color[16 * 32];
			FarmerRenderer.pantsTexture.GetData(0, pants_rect, pantsData, 0, pantsData.Length);
			data = CombineTextures(data, pantsData);

            Rectangle eyeRect = new Rectangle(5, 16, 6, 2);
            Color[] eyeData = new Color[6 * 2];
            baseTexture.GetData(0, eyeRect, pantsData, 0, eyeData.Length);
            for(int y = 16; y < 18; y++)
            {
                for (int x = 5; x < 11; x++)
                {
                    int i = y * 16 + x;
                    data[i] = new Color((data[i].R * data[i].A / 255 + eyeData[i].R * eyeData[i].A / 255) / 2, (data[i].G * data[i].A / 255 + eyeData[i].G * eyeData[i].A / 255) / 2, (data[i].B * data[i].A / 255 + eyeData[i].B * eyeData[i].A / 255) / 2);
                }
            }

            b.Draw(this.baseTexture, position + origin + this.positionOffset + new Vector2((float)x_adjustment, (float)(FarmerRenderer.featureYOffsetPerFrame[currentFrame] * 4 + ((who.IsMale && who.FacingDirection != 2) ? 36 : 40))), new Rectangle?(new Rectangle(5, 16, (facingDirection == 2) ? 6 : 2, 2)), overrideColor, 0f, origin, 4f * scale, SpriteEffects.None, layerDepth + 5E-08f);
            b.Draw(this.baseTexture, position + origin + this.positionOffset + new Vector2((float)x_adjustment, (float)(FarmerRenderer.featureYOffsetPerFrame[currentFrame] * 4 + ((who.FacingDirection == 1 || who.FacingDirection == 3) ? 40 : 44))), new Rectangle?(new Rectangle(264 + ((facingDirection == 3) ? 4 : 0), 2 + (who.currentEyes - 1) * 2, (facingDirection == 2) ? 6 : 2, 2)), overrideColor, 0f, origin, 4f * scale, SpriteEffects.None, layerDepth + 1.2E-07f);


            this.drawHairAndAccesories(b, facingDirection, who, position, origin, scale, currentFrame, rotation, overrideColor, layerDepth);
			float arm_layer_offset = 4.9E-05f;
			if (facingDirection == 0)
			{
				arm_layer_offset = -1E-07f;
			}
            sourceRect.Offset(96, 0);

            b.Draw(this.baseTexture, position + origin + this.positionOffset + who.armOffset, new Rectangle?(sourceRect), overrideColor, rotation, origin, 4f * scale, animationFrame.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth + arm_layer_offset);
		}

        private Color[] CombineTextures(Color[] data, Color[] add)
        {
			for (int i = 0; i < 16 * 32; i++)
			{
				data[i] = new Color((data[i].R * data[i].A / 255 + add[i].R * add[i].A / 255) / 2, (data[i].G * data[i].A / 255 + add[i].G * add[i].A / 255) / 2, (data[i].B * data[i].A / 255 + add[i].B * add[i].A / 255) / 2);
			}
			return data;
		}

		private void GameLoop_GameLaunched(object sender, GameLaunchedEventArgs e)
        {

            // get Generic Mod Config Menu's API (if it's installed)
            var configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu is null)
                return;

            // register mod
            configMenu.Register(
                mod: ModManifest,
                reset: () => Config = new ModConfig(),
                save: () => Helper.WriteConfig(Config)
            );

            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Mod Enabled",
                getValue: () => Config.EnableMod,
                setValue: value => Config.EnableMod = value
            );
        }

    }
}