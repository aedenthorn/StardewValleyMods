using Microsoft.Xna.Framework;
using Netcode;
using StardewValley;
using StardewValley.Monsters;
using StardewValley.Network;
using StardewValley.Tools;
using System;
using System.Collections.Generic;

namespace Swim
{
    public class SeaCrab : RockCrab
    {
        public List<string> crabTextures = new List<string>()
        {
            "HermitCrab",
            "ChestCrab",
        };
        private NetBool shellGone = new NetBool();
        private NetInt shellHealth = new NetInt(5);

        public SeaCrab() : base()
        {
        }

        public SeaCrab(Vector2 position) : base(position)
        {
            Sprite.LoadTexture("aedenthorn.Swim/Fishies/" + crabTextures[Game1.random.Next(100) < ModEntry.config.PercentChanceCrabIsMimic ? 1 : 0]);
            moveTowardPlayerThreshold.Value = 1;
            damageToFarmer.Value = 0;
        }
        protected override void initNetFields()
        {
            base.initNetFields();
            NetFields.AddField(
                shellGone
            );
            NetFields.AddField(
                shellHealth
            );
            position.Field.AxisAlignedMovement = true;
        }

        public override int takeDamage(int damage, int xTrajectory, int yTrajectory, bool isBomb, double addedPrecision, Farmer who)
        {
            return 0;
        }
        public override bool hitWithTool(Tool t)
        {
            if (t is Pickaxe && t.getLastFarmerToUse() != null && shellHealth.Value > 0)
            {
                currentLocation.playSound("hammer");
                NetInt netInt = shellHealth;
                int value = netInt.Value;
                netInt.Value = value - 1;
                shake(500);
                setTrajectory(Utility.getAwayFromPlayerTrajectory(GetBoundingBox(), t.getLastFarmerToUse()));
                if (shellHealth.Value <= 0)
                {
                    shellGone.Value = true;
                    moveTowardPlayer(-1);
                    currentLocation.playSound("stoneCrack");
                    Game1.createRadialDebris(currentLocation, 14, (int)Tile.X, (int)Tile.Y, Game1.random.Next(2, 7), false, -1, false);
                    Game1.createRadialDebris(currentLocation, 14, (int)Tile.X, (int)Tile.Y, Game1.random.Next(2, 7), false, -1, false);
                }
                return true;
            }
            return base.hitWithTool(t);
        }
        public override void behaviorAtGameTick(GameTime time)
        {
            if (shellGone)
            {
                Sprite.CurrentFrame = 16 + Sprite.currentFrame % 4;
            }
            if (withinPlayerThreshold())
            {
                if (Math.Abs(Player.GetBoundingBox().Center.Y - GetBoundingBox().Center.Y) < Math.Abs(Player.GetBoundingBox().Center.X - GetBoundingBox().Center.X))
                {
                    if (Player.GetBoundingBox().Center.X - GetBoundingBox().Center.X > 0 && TilePoint.X > 0)
                    {
                        SetMovingLeft(true);
                    }
                    else if (Player.GetBoundingBox().Center.X - GetBoundingBox().Center.X < 0 && TilePoint.X < currentLocation.map.Layers[0].TileWidth)
                    {
                        SetMovingRight(true);
                    }
                }
                else if (Player.GetBoundingBox().Center.Y - GetBoundingBox().Center.Y > 0 && TilePoint.Y > 0)
                {
                    SetMovingUp(true);
                }
                else if (Player.GetBoundingBox().Center.Y - GetBoundingBox().Center.Y < 0 && TilePoint.Y < currentLocation.map.Layers[0].TileHeight)
                {
                    SetMovingDown(true);
                }
                MovePosition(time, Game1.viewport, currentLocation);
            }
            else
            {
                Halt();
            }
        }

        protected override void updateMonsterSlaveAnimation(GameTime time)
        {
            if (isMoving())
            {
                if (base.FacingDirection == 0)
                {
                    Sprite.AnimateUp(time, 0, "");
                }
                else if (base.FacingDirection == 3)
                {
                    Sprite.AnimateLeft(time, 0, "");
                }
                else if (base.FacingDirection == 1)
                {
                    Sprite.AnimateRight(time, 0, "");
                }
                else if (base.FacingDirection == 2)
                {
                    Sprite.AnimateDown(time, 0, "");
                }
            }
            else
            {
                Sprite.StopAnimation();
            }
            if (isMoving() && Sprite.currentFrame % 4 == 0)
            {
                Sprite.currentFrame++;
                Sprite.UpdateSourceRect();
            }
            if (shellGone.Value)
            {
                updateGlow();
                if (invincibleCountdown > 0)
                {
                    glowingColor = Color.Cyan;
                    invincibleCountdown -= time.ElapsedGameTime.Milliseconds;
                    if (invincibleCountdown <= 0)
                    {
                        stopGlowing();
                    }
                }
                Sprite.currentFrame = 16 + Sprite.currentFrame % 4;
            }
        }
    }
}
