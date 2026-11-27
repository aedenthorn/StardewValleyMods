using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Locations;

namespace HedgeMaze
{
    internal class DwarfNPC : NPC
    {
        public DwarfNPC(AnimatedSprite sprite, Vector2 position, string defaultMap, int facingDirection, string name, bool datable, Texture2D portrait) : base(sprite, position, defaultMap, facingDirection, name, datable, portrait)
        {
        }

        public override bool checkAction(Farmer who, GameLocation l)
        {
            if (who.canUnderstandDwarves)
            {
                Utility.TryOpenShopMenu("Dwarf", Name, true);
                return true;
            }
            return base.checkAction(who, l);
        }
        public override bool CanSocialize => false;
    }
}