using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Characters;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Menus;
using System;

namespace PettingAnimation
{
    public partial class ModEntry
    {
        public static void PetPet(Character target, Farmer who)
        {
            if(target is null || who is null || target.currentLocation != who.currentLocation || Vector2.Distance(target.Position, who.Position) > 128f)
                return;
            ticks.Value = 0;
            layer.Value = who.FacingDirection == 2 ? -1 : target.StandingPixel.Y / 10000f + 0.0002f;
            int amount = target.GetType().Name switch
            {
               nameof(Pet) => 32,
               nameof(FarmAnimal) => 32,
               _ => 32
            };
            Vector2 distance = (target.GetBoundingBox().Center - who.GetBoundingBox().Center).ToVector2();
            offset.Value = who.FacingDirection switch
            {
                0 => distance - new Vector2(0, -amount),
                1 => distance - new Vector2(amount, 0),
                2 => distance - new Vector2(0, amount),
                3 => distance - new Vector2(-amount, 0),
                _ => distance - new Vector2(0, 0),
            };
            int which = who.FacingDirection switch
            {
                0 => FarmerSprite.swordswipeUp,
                1 => FarmerSprite.swordswipeRight,
                2 => FarmerSprite.swordswipeDown,
                _ => FarmerSprite.swordswipeLeft,
            };
            who.animateOnce(which);
            int speed = Config.FrameMilliseconds;
            who.FarmerSprite.timer = speed;
            for (int i = 0; i < who.FarmerSprite.currentAnimation.Count; i++)
            {
                var f = who.FarmerSprite.currentAnimation[i];
                who.FarmerSprite.currentAnimation[i] = new FarmerSprite.AnimationFrame(f.frame, speed, f.positionOffset)
                {
                    flip = f.flip,
                    armOffset = f.armOffset,
                    xOffset = f.xOffset,
                    frameEndBehavior = i == who.FarmerSprite.currentAnimation.Count - 1 ? new AnimatedSprite.endOfAnimationBehavior((Farmer f) =>
                    {
                        offset.Value = Vector2.Zero;
                        layer.Value = -1;
                        ticks.Value = 0;
                    }) : null
                };
            }
        }

    }
}