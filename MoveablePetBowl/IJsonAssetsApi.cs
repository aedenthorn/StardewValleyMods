namespace MoveablePetBowl
{
    public interface IJsonAssetsApi
    {
        void LoadAssets(string path);
        int GetBigCraftableId(string name);
    }
}