using Harmony;
using Microsoft.Xna.Framework;
using Netcode;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace MagnetMod
{
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod
    {

        private ModConfig Config;
		public float magnetRangeMult;
		private int magnetSpeedMult;
		private bool noLootBounce;
		private bool noLootWave;

		/*********
        ** Public methods
        *********/
		/// <summary>The mod entry point, called after the mod is first loaded.</summary>
		/// <param name="helper">Provides simplified APIs for writing mods.</param>
		public override void Entry(IModHelper helper)
        {
            helper.Events.GameLoop.UpdateTicking += this.UpdateTicking;
            this.Config = this.Helper.ReadConfig<ModConfig>();
			this.magnetRangeMult = Config.MagnetRangeMult;
			this.magnetSpeedMult = Config.MagnetSpeedMult;
			this.noLootBounce = Config.NoLootBounce;
			this.noLootWave = Config.NoLootWave;

			ObjectPatches.magnetRangeMult = this.magnetRangeMult;
			var harmony = HarmonyInstance.Create(this.ModManifest.UniqueID);

			harmony.Patch(
			   original: AccessTools.Method(typeof(Debris), "playerInRange"),
			   prefix: new HarmonyMethod(typeof(ObjectPatches), nameof(ObjectPatches.playerInRange_Prefix))
			);
		}

		private void UpdateTicking(object sender, UpdateTickingEventArgs e)
        {
            if (!Context.IsWorldReady)
                return;
			GameLocation location = Game1.getPlayerOrEventFarmer().currentLocation;
			Netcode.NetCollection<Debris> debris = location.debris;
			GameTime time = new GameTime();
			float rangeMult = this.magnetRangeMult;
			bool infRange = (rangeMult < 0 ? true : false);
			int speedMult = this.magnetSpeedMult; 
			bool noBounce = this.noLootBounce;
			bool noWave = this.noLootWave;

			for (int j = 0; j < debris.Count; j++)
            {
                Debris d = debris[j];
				NetObjectShrinkList<Chunk> chunks = (NetObjectShrinkList<Chunk>) typeof(Debris).GetField("chunks", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(d);

				if (chunks.Count == 0)
				{
					continue;
				}
				d.timeSinceDoneBouncing += (float)time.ElapsedGameTime.Milliseconds;
				if (d.timeSinceDoneBouncing >= (d.floppingFish ? 2500f : ((d.debrisType == Debris.DebrisType.SPRITECHUNKS || d.debrisType == Debris.DebrisType.NUMBERS) ? 1800f : (noBounce ? 0f : 600f))))
				{
					if (d.debrisType == Debris.DebrisType.LETTERS || d.debrisType == Debris.DebrisType.NUMBERS || d.debrisType == Debris.DebrisType.SQUARES || d.debrisType == Debris.DebrisType.SPRITECHUNKS || (d.debrisType == Debris.DebrisType.CHUNKS && chunks[0].debrisType - chunks[0].debrisType % 2 != 8))
					{
						continue;
					}
					if (d.debrisType == Debris.DebrisType.ARCHAEOLOGY || d.debrisType == Debris.DebrisType.OBJECT || d.debrisType == Debris.DebrisType.RESOURCE || d.debrisType == Debris.DebrisType.CHUNKS)
					{
						d.chunksMoveTowardPlayer = true;
					}
					d.timeSinceDoneBouncing = 0f;
				}
				if (location.farmers.Count == 0)
				{
					continue;
				}
				Vector2 total = default(Vector2);
				foreach (Chunk chunk in chunks)
				{
					total += chunk.position.Value;
				}
				Vector2 position = total / (float)chunks.Count;
				if (d.player.Value != null && (d.player.Value.currentLocation != location || !infRange && !(Math.Abs(position.X + 32f - (float)d.player.Value.getStandingX()) <= (float)d.player.Value.MagneticRadius * rangeMult && Math.Abs(position.Y + 32f - (float)d.player.Value.getStandingY()) <= (float)d.player.Value.MagneticRadius * rangeMult)))
				{
					d.player.Value = null;
				}
				Farmer farmer = d.player.Value;
				if (farmer == null && (Game1.IsMasterGame || location.isTemp()))
				{
					float bestDistance = float.MaxValue;
					Farmer bestFarmer = null;
					foreach (Farmer f in location.farmers)
					{
						bool pir = infRange || (Math.Abs(position.X + 32f - (float)f.getStandingX()) <= (float)f.MagneticRadius * rangeMult && Math.Abs(position.Y + 32f - (float)f.getStandingY()) <= (float)f.MagneticRadius * rangeMult);
						if ((f.UniqueMultiplayerID != d.DroppedByPlayerID || bestFarmer == null) && pir)
						{
							float distance = (f.Position - position).LengthSquared();
							if (distance < bestDistance || (bestFarmer != null && bestFarmer.UniqueMultiplayerID == d.DroppedByPlayerID))
							{
								bestFarmer = f;
								bestDistance = distance;
							}
						}
					}
					farmer = bestFarmer;
				}

				bool anyCouldMove = false;
				for (int i = chunks.Count - 1; i >= 0; i--)
				{
					Chunk chunk = chunks[i];
					chunk.position.UpdateExtrapolation(chunk.getSpeed());
					if (chunk.alpha > 0.1f && (d.debrisType == Debris.DebrisType.SPRITECHUNKS || d.debrisType == Debris.DebrisType.NUMBERS) && d.timeSinceDoneBouncing > 600f)
					{
						chunk.alpha = (1800f - d.timeSinceDoneBouncing) / 1000f;
					}
					if (chunk.position.X < -128f || chunk.position.Y < -64f || chunk.position.X >= (float)(location.map.DisplayWidth + 64) || chunk.position.Y >= (float)(location.map.DisplayHeight + 64))
					{
						chunks.RemoveAt(i);
					}
					else
					{
						bool canMoveTowardPlayer = farmer != null;
						if (canMoveTowardPlayer)
						{
							Debris.DebrisType value = d.debrisType.Value;
							if (value - Debris.DebrisType.ARCHAEOLOGY > 1)
							{
								canMoveTowardPlayer = (value != Debris.DebrisType.RESOURCE || farmer.couldInventoryAcceptThisObject(chunk.debrisType - chunk.debrisType % 2, 1, 0));
							}
							else if (d.item != null)
							{
								canMoveTowardPlayer = farmer.couldInventoryAcceptThisItem(d.item);
							}
							else
							{
								if (chunk.debrisType < 0)
								{
									canMoveTowardPlayer = farmer.couldInventoryAcceptThisItem(new StardewValley.Object(Vector2.Zero, chunk.debrisType * -1, false));
								}
								else
								{
									canMoveTowardPlayer = farmer.couldInventoryAcceptThisObject(chunk.debrisType, 1, d.itemQuality);
								}
								if (chunk.debrisType == 102 && farmer.hasMenuOpen)
								{
									canMoveTowardPlayer = false;
								}
							}
							anyCouldMove = (anyCouldMove || canMoveTowardPlayer);
							if (canMoveTowardPlayer)
							{
								d.player.Value = farmer;
							}
						}
						if ((d.chunksMoveTowardPlayer || d.isFishable) && canMoveTowardPlayer)
						{
							if (d.player.Value.IsLocalPlayer)
							{
								if(speedMult < 0)
								{
									chunk.position.X = d.player.Value.Position.X;
									chunk.position.Y = d.player.Value.Position.Y;
								} 
								else
								{
									for (int l = 1; l < speedMult; l++)
									{
										if (noWave)
										{
											if (chunk.position.X < d.player.Value.Position.X - 12f)
											{
												chunk.xVelocity.Value = 8f;
											}
											else if (chunk.position.X > d.player.Value.Position.X + 12f)
											{
												chunk.xVelocity.Value = -8f;
											}
											if (chunk.position.Y < d.player.Value.Position.Y - 12f)
											{
												chunk.yVelocity.Value = -8f;
											}
											else if (chunk.position.Y > d.player.Value.Position.Y + 12f)
											{
												chunk.yVelocity.Value = 8f;
											}
										}
										else {
											if (chunk.position.X < d.player.Value.Position.X - 12f)
											{
												chunk.xVelocity.Value = Math.Min(chunk.xVelocity + 0.8f, 8f);
											}
											else if (chunk.position.X > d.player.Value.Position.X + 12f)
											{
												chunk.xVelocity.Value = Math.Max(chunk.xVelocity - 0.8f, -8f);
											}
											if (chunk.position.Y + 32f < (float)(d.player.Value.getStandingY() - 12))
											{
												chunk.yVelocity.Value = Math.Max(chunk.yVelocity - 0.8f, -8f);
											}
											else if (chunk.position.Y + 32f > (float)(d.player.Value.getStandingY() + 12))
											{
												chunk.yVelocity.Value = Math.Min(chunk.yVelocity + 0.8f, 8f);
											}
										}
										chunk.position.X += chunk.xVelocity;
										chunk.position.Y -= chunk.yVelocity;
									}
								}
							}
						}
					}
				}
				typeof(Debris).GetField("chunks", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(d, chunks);
			}
            
        }
    }
}