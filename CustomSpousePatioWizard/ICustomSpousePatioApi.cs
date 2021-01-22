using System.Collections.Generic;

namespace CustomSpousePatioWizard
{
    public interface ICustomSpousePatioApi
    {
        Dictionary<string, object> GetCurrentSpouseAreas();
        Dictionary<string, int[]> GetDefaultSpouseOffsets();
        void RemoveAllSpouseAreas();
        void ReloadSpouseAreaData();
        void AddTileSheets();
        void ShowSpouseAreas();
        void ReloadPatios();
        bool RemoveSpousePatio(string spouse);
        void AddSpousePatioHere(string spouse_tilesOf);
    }
}