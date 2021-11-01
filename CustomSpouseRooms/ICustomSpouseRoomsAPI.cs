using Microsoft.Xna.Framework;
using StardewValley;

namespace CustomSpouseRooms
{
    public interface ICustomSpouseRoomsAPI
    {
        public Point GetSpouseTileOffset(NPC spouse);
        public Point GetSpouseTile(NPC spouse);

        public Point GetSpouseRoomCornerTile(NPC spouse);
    }
}