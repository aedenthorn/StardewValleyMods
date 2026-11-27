using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.GameData.Characters;
using StardewValley.Locations;
using StardewValley.Objects;
using StardewValley.TokenizableStrings;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PeopleSeeds
{
    public partial class ModEntry
    {
        // Token: 0x06000037 RID: 55 RVA: 0x00006180 File Offset: 0x00004380
        public PeopleFurniture(int personId, Vector2 tp)
            : base(0, tp)
        {
            this.personId = personId;
            this.tileLocation.Value = tp;
            this.isOn.Value = false;
            base.ParentSheetIndex = 0;
            base.name = People.GetPerson(personId);
            base.DisplayName = base.Name;
            this.furniture_type.Value = 0;
            this.defaultSourceRect.Value = new Rectangle(0, 0, 16, 32);
            this.sourceRect.Value = new Rectangle(0, 0, 16, 32);
            this.drawHeldObjectLow.Value = false;
            this.defaultBoundingBox.Value = new Rectangle((int)this.tileLocation.X * 64, (int)this.tileLocation.Y * 64, 64, 64);
            this.boundingBox.Value = new Rectangle((int)this.tileLocation.X * 64, (int)this.tileLocation.Y * 64, 64, 64);
            base.updateDrawPosition();
            this.rotations.Value = 0;
            this.bigCraftable.Value = true;
            base.Category = -17;
            this.edibility.Value = 729;
            this.price.Value = 490;
        }

        // Token: 0x06000038 RID: 56 RVA: 0x000062CF File Offset: 0x000044CF
        public override int maximumStackSize()
        {
            return 99;
        }

        // Token: 0x06000039 RID: 57 RVA: 0x000062D3 File Offset: 0x000044D3
        public override bool canBeGivenAsGift()
        {
            return true;
        }

        // Token: 0x0600003A RID: 58 RVA: 0x000062D8 File Offset: 0x000044D8
        public virtual bool canBePlacedHere(GameLocation l, Vector2 tile)
        {
            if (!(l is DecoratableLocation))
            {
                return false;
            }
            if (Utility.distance(tile.X, Game1.player.getTileLocation().X, tile.Y, Game1.player.getTileLocation().Y) > 3f)
            {
                return false;
            }
            for (int i = 0; i < this.boundingBox.Width / 64; i++)
            {
                int j = 0;
                while (j < this.boundingBox.Height / 64)
                {
                    Vector2 vector = tile * 64f + new Vector2((float)i, (float)j) * 64f;
                    vector.X += 32f;
                    vector.Y += 32f;
                    foreach (Furniture furniture in (l as DecoratableLocation).furniture)
                    {
                        if (furniture.furniture_type.Value == 11 && furniture.getBoundingBox(furniture.TileLocation).Contains((int)vector.X, (int)vector.Y) && furniture.heldObject.Value == null && base.getTilesWide() == 1)
                        {
                            return true;
                        }
                        if ((furniture.furniture_type.Value != 12 || this.furniture_type.Value == 12) && furniture.getBoundingBox(furniture.TileLocation).Contains((int)vector.X, (int)vector.Y))
                        {
                            return false;
                        }
                    }
                    Vector2 vector2 = tile + new Vector2((float)i, (float)j);
                    if (l.Objects.ContainsKey(vector2))
                    {
                        return false;
                    }
                    j++;
                    continue;
                }
            }
            return base.canBePlacedHere(l, tile);
        }

        // Token: 0x0600003B RID: 59 RVA: 0x000064C4 File Offset: 0x000046C4
        public PeopleFurniture CloneOne()
        {
            return new PeopleFurniture(this.personId, Vector2.Zero)
            {
                Quality = base.Quality,
                gquality = this.gquality,
                Stack = 1
            };
        }

        // Token: 0x0600003C RID: 60 RVA: 0x000064F8 File Offset: 0x000046F8
        public override int addToStack(int amount)
        {
            this.stack.Value += amount;
            if (this.stack.Value > 99)
            {
                int num = this.stack.Value - 99;
                this.stack.Value = 99;
                return num;
            }
            return 0;
        }

        // Token: 0x0600003D RID: 61 RVA: 0x00006544 File Offset: 0x00004744
        public override Color getCategoryColor()
        {
            return new Color(255, 80, 80);
        }

        // Token: 0x0600003E RID: 62 RVA: 0x00006554 File Offset: 0x00004754
        public override string getCategoryName()
        {
            return "People";
        }

        // Token: 0x0600003F RID: 63 RVA: 0x0000655B File Offset: 0x0000475B
        public override string getDescription()
        {
            return "It's " + base.name + ", but they are still, as if frozen.";
        }

        // Token: 0x06000040 RID: 64 RVA: 0x00006574 File Offset: 0x00004774
        private float GetScaleSize()
        {
            int num = this.sourceRect.Width / 16;
            int num2 = this.sourceRect.Height / 16;
            if (num >= 5)
            {
                return 0.75f;
            }
            if (num2 >= 3)
            {
                return 1f;
            }
            if (num <= 2)
            {
                return 2f;
            }
            if (num <= 4)
            {
                return 1f;
            }
            return 0.1f;
        }

        // Token: 0x06000041 RID: 65 RVA: 0x000062D3 File Offset: 0x000044D3
        public override bool canBeShipped()
        {
            return true;
        }

        // Token: 0x06000042 RID: 66 RVA: 0x000065CC File Offset: 0x000047CC
        public override void drawWhenHeld(SpriteBatch spriteBatch, Vector2 objectPosition, Farmer f)
        {
            if (Game1.player.itemToEat == this)
            {
                spriteBatch.Draw(People.CloneTemplates[People.GetPerson(this.personId)].Sprite.Texture, objectPosition, new Rectangle?(this.defaultSourceRect), Color.White, 0f, new Vector2(0f, 3f), 4f, SpriteEffects.None, Math.Max(0f, (float)(f.getStandingY() + 2) / 10000f));
            }
        }

        // Token: 0x06000043 RID: 67 RVA: 0x00006654 File Offset: 0x00004854
        public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, bool drawStackNumber, Color color, bool drawShadow)
        {
            spriteBatch.Draw(People.CloneTemplates[People.GetPerson(this.personId)].Sprite.Texture, location + new Vector2(32f, 32f), new Rectangle?(this.defaultSourceRect), color * transparency, 0f, new Vector2((float)(this.defaultSourceRect.Width / 2), (float)(this.defaultSourceRect.Height / 2)), 1f * this.GetScaleSize() * scaleSize, SpriteEffects.None, layerDepth);
            if (drawStackNumber && (double)scaleSize > 0.3 && this.Stack > 1)
            {
                Utility.drawTinyDigits(this.stack, spriteBatch, location + new Vector2((float)(64 - Utility.getWidthOfTinyDigitString(this.stack, 3f * scaleSize)) + 3f * scaleSize, 64f - 18f * scaleSize + 2f), 3f * scaleSize, 1f, color);
            }
            if (drawStackNumber && this.quality.Value > 0)
            {
                float num = ((this.quality.Value < 4) ? 0f : (((float)Math.Cos((double)Game1.currentGameTime.TotalGameTime.Milliseconds * 3.141592653589793 / 512.0) + 1f) * 0.05f));
                spriteBatch.Draw(Game1.mouseCursors, location + new Vector2(12f, 52f + num), new Rectangle?((this.quality.Value < 4) ? new Rectangle(338 + (this.quality.Value - 1) * 8, 400, 8, 8) : new Rectangle(346, 392, 8, 8)), color * transparency, 0f, new Vector2(4f, 4f), 3f * scaleSize * (1f + num), SpriteEffects.None, layerDepth);
            }
        }

        // Token: 0x06000044 RID: 68 RVA: 0x00006864 File Offset: 0x00004A64
        public override void draw(SpriteBatch spriteBatch, int x, int y, float alpha = 1f)
        {
            if (x == -1)
            {
                spriteBatch.Draw(People.CloneTemplates[People.GetPerson(this.personId)].Sprite.Texture, Game1.GlobalToLocal(Game1.viewport, this.drawPosition), new Rectangle?(this.sourceRect), Color.White * alpha, 0f, Vector2.Zero, 4f, this.flipped.Value ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (float)(this.boundingBox.Value.Bottom - 8) / 10000f);
            }
            else
            {
                spriteBatch.Draw(People.CloneTemplates[People.GetPerson(this.personId)].Sprite.Texture, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(x * 64), (float)(y * 64 - (this.sourceRect.Height * 4 - this.boundingBox.Height)))), new Rectangle?(this.sourceRect), Color.White * alpha, 0f, Vector2.Zero, 4f, this.flipped.Value ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (float)(this.boundingBox.Value.Bottom - 8) / 10000f);
            }
            if (this.heldObject.Value != null)
            {
                spriteBatch.Draw(Game1.objectSpriteSheet, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(this.boundingBox.Center.X - 32), (float)(this.boundingBox.Center.Y - (this.drawHeldObjectLow.Value ? 17 : 70)))), new Rectangle?(GameLocation.getSourceRectForObject(this.heldObject.Value.ParentSheetIndex)), Color.White * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.FlipVertically, (float)(this.boundingBox.Bottom + 1) / 10000f);
            }
        }

        // Token: 0x0400000B RID: 11
        internal int personId = -1;

        // Token: 0x0400000C RID: 12
        internal int gquality = 1;

        // Token: 0x0400000D RID: 13
        internal bool living;
    }
}
