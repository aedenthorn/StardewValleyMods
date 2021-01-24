using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace CustomSpousePatioWizard
{
    public interface ICustomSpousePatioApi
    {
        Dictionary<string, object> GetCurrentSpouseAreas();
        Dictionary<string, Point> GetDefaultSpouseOffsets();
        void RemoveAllSpouseAreas();
        void ReloadSpouseAreaData(); 
        void AddTileSheets();
        void ShowSpouseAreas();
        void ReloadPatios();
        bool RemoveSpousePatio(string spouse);
        void AddSpousePatioHere(string spouse_tilesOf, Point cursorLoc);
        bool MoveSpousePatio(string whichAnswer, Point cursorLoc);
    }
}