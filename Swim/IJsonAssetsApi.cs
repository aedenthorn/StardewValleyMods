namespace Swim
{
    public interface IJsonAssetsApi
    {
        string GetClothingId(string name);
        string GetHatId(string name);
        void LoadAssets(string path);
    }
}
