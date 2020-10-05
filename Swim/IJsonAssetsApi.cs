namespace Swim
{
    public interface IJsonAssetsApi
    {
        int GetClothingId(string name);
        int GetHatId(string name);
        void LoadAssets(string path);
    }
}
