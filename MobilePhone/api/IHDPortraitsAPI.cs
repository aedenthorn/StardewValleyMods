using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace MobilePhone.Api
{
    public interface IHDPortraitsAPI
    {
        /// <summary>
        /// Draw NPC Portrait over region
        /// </summary>
        /// <param name="b">The spritebatch to draw with</param>
        /// <param name="npc">NPC</param>
        /// <param name="index">Portrait index</param>
        /// <param name="region">The region of the screen to draw to</param>
        /// <param name="color">Tint</param>
        /// <param name="reset">Whether or not to reset animations this tick</param>
        public void DrawPortrait(SpriteBatch b, NPC npc, int index, Rectangle region, Color? color = null, bool reset = false);
        /// <summary>
        /// Draw NPC Portrait at position with default size
        /// </summary>
        /// <param name="b">The spritebatch to draw with</param>
        /// <param name="npc">NPC</param>
        /// <param name="index">Portrait index</param>
        /// <param name="position">Position to draw at</param>
        /// <param name="color">Tint</param>
        /// <param name="reset">Whether or not to reset animations this tick</param>
        public void DrawPortrait(SpriteBatch b, NPC npc, int index, Point position, Color? color = null, bool reset = false);
        /// <summary>
        /// Retrieves the texture and texture region to use for a portrait
        /// </summary>
        /// <param name="npc">NPC</param>
        /// <param name="index">Portrait index</param>
        /// <param name="elapsed">Time since last call (for animation)</param>
        /// <param name="reset">Whether or not to reset animations this tick</param>
        /// <returns>The source region & the texture to use</returns>
        public (Rectangle, Texture2D) GetTextureAndRegion(NPC npc, int index, int elapsed = -1, bool reset = false);
        /// <summary>
        /// Forces HD Portraits to reload its metadata.
        /// </summary>
        public void ReloadData();
    }
}
