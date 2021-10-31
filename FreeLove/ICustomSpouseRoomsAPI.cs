using Microsoft.Xna.Framework;
using StardewValley;

namespace FreeLove
{
    public interface ICustomSpouseRoomsAPI
    {
        Point GetSpouseRoomPoint(NPC spouse);
        int GetSpouseRoomOffset(NPC spouse);
    }
}