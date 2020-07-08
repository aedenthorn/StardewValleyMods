using System;
using System.Collections.Generic;
using System.Reflection;
using Familiars;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Network;
using StardewValley.Projectiles;

namespace StardewValley.Monsters
{
	public class DinoFamiliar : DinoMonster
	{

		public Monster currentTarget = null;
		public bool followingPlayer = true;
		public Farmer owner;
		private bool chargingMonster;
		private Color color;

		public DinoFamiliar()
		{
		}

		public DinoFamiliar(Vector2 position, Farmer owner) : base(position)
		{
			this.owner = owner;
			this.Sprite.SpriteWidth = 32;
			this.Sprite.SpriteHeight = 32;
			this.Sprite.UpdateSourceRect();
			this.timeUntilNextAttack = 2000;
			this.nextChangeDirectionTime = Game1.random.Next(1000, 3000);
			this.nextWanderTime = Game1.random.Next(1000, 2000);
			moveTowardPlayerThreshold.Value = 20;
			damageToFarmer.Value = 0;
			farmerPassesThrough = true;
			this.reloadSprite();

		}

		protected override void initNetFields()
		{
			base.NetFields.AddFields(new INetSerializable[]
			{
				this.attackState,
				this.firing
			});
			base.initNetFields();
		}

		public override void reloadSprite()
		{
			if (this.Sprite == null)
			{
				this.Sprite = new AnimatedSprite(ModEntry.Config.DinoTexture);
			}
			else
			{
				this.Sprite.textureName.Value = ModEntry.Config.DinoTexture;
			}
			if (!ModEntry.Config.DefaultDinoColor)
			{
				typeof(AnimatedSprite).GetField("spriteTexture", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(Sprite, Utils.ColorFamiliar(Sprite.Texture, ModEntry.Config.DinoMainColor, ModEntry.Config.DinoRedColor, ModEntry.Config.DinoGreenColor, ModEntry.Config.DinoBlueColor));
			}
			this.Sprite.SpriteWidth = 32;
			this.Sprite.SpriteHeight = 32;
			this.Sprite.UpdateSourceRect();
			base.HideShadow = true;
		}

		public override Rectangle GetBoundingBox()
		{
			return new Rectangle((int)base.Position.X + 8, (int)base.Position.Y, this.Sprite.SpriteWidth * 4 * 3 / 4, 64);
		}

		public override List<Item> getExtraDropItems()
		{
			List<Item> extra_items = new List<Item>();
			if (Game1.random.NextDouble() < 0.10000000149011612)
			{
				extra_items.Add(new Object(107, 1, false, -1, 0));
			}
			else
			{
				extra_items.Add(Utility.GetRandom<Item>(new List<Item>
				{
					new Object(580, 1, false, -1, 0),
					new Object(583, 1, false, -1, 0),
					new Object(584, 1, false, -1, 0)
				}, null));
			}
			return extra_items;
		}

		protected override void sharedDeathAnimation()
		{
			base.currentLocation.playSound("skeletonDie", NetAudio.SoundContext.Default);
			base.currentLocation.playSound("grunt", NetAudio.SoundContext.Default);
			for (int i = 0; i < 16; i++)
			{
				Game1.createRadialDebris(base.currentLocation, this.Sprite.textureName, new Rectangle(64, 128, 16, 16), 16, (int)Utility.Lerp((float)this.GetBoundingBox().Left, (float)this.GetBoundingBox().Right, (float)Game1.random.NextDouble()), (int)Utility.Lerp((float)this.GetBoundingBox().Bottom, (float)this.GetBoundingBox().Top, (float)Game1.random.NextDouble()), 1, (int)base.getTileLocation().Y, Color.White, 4f);
			}
		}

		protected override void localDeathAnimation()
		{
			Utility.makeTemporarySpriteJuicier(new TemporaryAnimatedSprite(44, base.Position, Color.HotPink, 10, false, 100f, 0, -1, -1f, -1, 0)
			{
				holdLastFrame = true,
				alphaFade = 0.01f,
				interval = 70f
			}, base.currentLocation, 8, 96, 64);
		}

		public override void behaviorAtGameTick(GameTime time)
		{
			invincibleCountdown = 1000;

			chargingMonster = false;
			foreach (NPC npc in currentLocation.characters)
			{
				if (ModEntry.familiarTypes.Contains(npc.GetType()))
					continue;

				if (npc is Monster && Utils.withinMonsterThreshold(this, (Monster)npc, 2))
				{
					chargingMonster = true;
					if (currentTarget == null || Vector2.Distance(npc.position, position) < Vector2.Distance(currentTarget.position, position))
					{
						currentTarget = (Monster)npc;
					}
				}
			}

			if (this.attackState.Value == 1)
			{
				base.IsWalkingTowardPlayer = false;
				this.Halt();
			}
			else if (this.withinPlayerThreshold())
			{
				base.IsWalkingTowardPlayer = true;
			}
			else
			{
				base.IsWalkingTowardPlayer = false;
				this.nextChangeDirectionTime -= time.ElapsedGameTime.Milliseconds;
				this.nextWanderTime -= time.ElapsedGameTime.Milliseconds;
				if (this.nextChangeDirectionTime < 0)
				{
					this.nextChangeDirectionTime = Game1.random.Next(500, 1000);
					this.facingDirection.Value = (this.facingDirection.Value + (Game1.random.Next(0, 3) - 1) + 4) % 4;
				}
				if (this.nextWanderTime < 0)
				{
					if (this.wanderState)
					{
						this.nextWanderTime = Game1.random.Next(1000, 2000);
					}
					else
					{
						this.nextWanderTime = Game1.random.Next(1000, 3000);
					}
					this.wanderState = !this.wanderState;
				}
				if (this.wanderState)
				{
					this.moveLeft = (this.moveUp = (this.moveRight = (this.moveDown = false)));
					base.tryToMoveInDirection(this.facingDirection.Value, false, base.DamageToFarmer, this.isGlider);
				}
			}
			this.timeUntilNextAttack -= time.ElapsedGameTime.Milliseconds;
			if (this.attackState.Value == 0 && chargingMonster)
			{
				this.firing.Set(false);
				if (this.timeUntilNextAttack < 0)
				{
					this.timeUntilNextAttack = 0;
					this.attackState.Set(1);
					this.nextFireTime = 500;
					this.totalFireTime = 3000;
					base.currentLocation.playSound("croak", NetAudio.SoundContext.Default);
					return;
				}
			}
			else if (this.totalFireTime > 0)
			{
				if (!this.firing)
				{
					if (currentTarget != null)
					{
						base.faceGeneralDirection(currentTarget.Position, 0, false);
					}
				}
				this.totalFireTime -= time.ElapsedGameTime.Milliseconds;
				if (this.nextFireTime > 0)
				{
					this.nextFireTime -= time.ElapsedGameTime.Milliseconds;
					if (this.nextFireTime <= 0)
					{
						if (!this.firing.Value)
						{
							this.firing.Set(true);
							base.currentLocation.playSound("furnace", NetAudio.SoundContext.Default);
						}
						float fire_angle = 0f;
						Vector2 shot_origin = new Vector2((float)this.GetBoundingBox().Center.X - 32f, (float)this.GetBoundingBox().Center.Y - 32f);
						switch (this.facingDirection.Value)
						{
							case 0:
								this.yVelocity = -1f;
								shot_origin.Y -= 64f;
								fire_angle = 90f;
								break;
							case 1:
								this.xVelocity = -1f;
								shot_origin.X += 64f;
								fire_angle = 0f;
								break;
							case 2:
								this.yVelocity = 1f;
								fire_angle = 270f;
								break;
							case 3:
								this.xVelocity = 1f;
								shot_origin.X -= 64f;
								fire_angle = 180f;
								break;
						}
						fire_angle += (float)Math.Sin((double)((float)this.totalFireTime / 1000f * 180f) * 3.1415926535897931 / 180.0) * 25f;
						Vector2 shot_velocity = new Vector2((float)Math.Cos((double)fire_angle * 3.1415926535897931 / 180.0), -(float)Math.Sin((double)fire_angle * 3.1415926535897931 / 180.0));
						shot_velocity *= 10f;
						BasicProjectile projectile = new BasicProjectile(ModEntry.Config.DinoDamage, 10, 0, 1, 0.196349546f, shot_velocity.X, shot_velocity.Y, shot_origin, "", "", false, true, base.currentLocation, this, false, null);
						projectile.ignoreTravelGracePeriod.Value = true;
						projectile.maxTravelDistance.Value = 256;
						base.currentLocation.projectiles.Add(projectile);
						this.nextFireTime = 50;
					}
				}
				if (this.totalFireTime <= 0)
				{
					this.totalFireTime = 0;
					this.nextFireTime = 0;
					this.attackState.Set(0);
					this.timeUntilNextAttack = Game1.random.Next(1000, 2000);
				}
			}
		}

		protected override void updateAnimation(GameTime time)
		{
			int direction_offset = 0;
			if (base.FacingDirection == 2)
			{
				direction_offset = 0;
			}
			else if (base.FacingDirection == 1)
			{
				direction_offset = 4;
			}
			else if (base.FacingDirection == 0)
			{
				direction_offset = 8;
			}
			else if (base.FacingDirection == 3)
			{
				direction_offset = 12;
			}
			if (this.attackState.Value != 1)
			{
				if (this.isMoving() || this.wanderState)
				{
					if (base.FacingDirection == 0)
					{
						this.Sprite.AnimateUp(time, 0, "");
						return;
					}
					if (base.FacingDirection == 3)
					{
						this.Sprite.AnimateLeft(time, 0, "");
						return;
					}
					if (base.FacingDirection == 1)
					{
						this.Sprite.AnimateRight(time, 0, "");
						return;
					}
					if (base.FacingDirection == 2)
					{
						this.Sprite.AnimateDown(time, 0, "");
						return;
					}
				}
				else
				{
					if (base.FacingDirection == 0)
					{
						this.Sprite.AnimateUp(time, 0, "");
					}
					else if (base.FacingDirection == 3)
					{
						this.Sprite.AnimateLeft(time, 0, "");
					}
					else if (base.FacingDirection == 1)
					{
						this.Sprite.AnimateRight(time, 0, "");
					}
					else if (base.FacingDirection == 2)
					{
						this.Sprite.AnimateDown(time, 0, "");
					}
					this.Sprite.StopAnimation();
				}
				return;
			}
			if (this.firing.Value)
			{
				this.Sprite.CurrentFrame = 16 + direction_offset;
				return;
			}
			this.Sprite.CurrentFrame = 17 + direction_offset;
		}

		protected override void updateMonsterSlaveAnimation(GameTime time)
		{
			int direction_offset = 0;
			if (base.FacingDirection == 2)
			{
				direction_offset = 0;
			}
			else if (base.FacingDirection == 1)
			{
				direction_offset = 4;
			}
			else if (base.FacingDirection == 0)
			{
				direction_offset = 8;
			}
			else if (base.FacingDirection == 3)
			{
				direction_offset = 12;
			}
			if (this.attackState.Value != 1)
			{
				if (this.isMoving())
				{
					if (base.FacingDirection == 0)
					{
						this.Sprite.AnimateUp(time, 0, "");
						return;
					}
					if (base.FacingDirection == 3)
					{
						this.Sprite.AnimateLeft(time, 0, "");
						return;
					}
					if (base.FacingDirection == 1)
					{
						this.Sprite.AnimateRight(time, 0, "");
						return;
					}
					if (base.FacingDirection == 2)
					{
						this.Sprite.AnimateDown(time, 0, "");
						return;
					}
				}
				else
				{
					this.Sprite.StopAnimation();
				}
				return;
			}
			if (this.firing.Value)
			{
				this.Sprite.CurrentFrame = 16 + direction_offset;
				return;
			}
			this.Sprite.CurrentFrame = 17 + direction_offset;
		}
		public override int takeDamage(int damage, int xTrajectory, int yTrajectory, bool isBomb, double addedPrecision, Farmer who)
		{
			if (who != null)
			{
				return 0;
			}
			return base.takeDamage(damage, xTrajectory, yTrajectory, isBomb, addedPrecision, who);
		}
		public override bool withinPlayerThreshold()
		{
			if (base.currentLocation != null && !base.currentLocation.farmers.Any())
			{
				return false;
			}
			Vector2 tileLocationOfPlayer = owner.getTileLocation();
			Vector2 tileLocationOfMonster = base.getTileLocation();
			return Math.Abs(tileLocationOfMonster.X - tileLocationOfPlayer.X) <= (float)moveTowardPlayerThreshold && Math.Abs(tileLocationOfMonster.Y - tileLocationOfPlayer.Y) <= (float)moveTowardPlayerThreshold;
		}

	}
}
