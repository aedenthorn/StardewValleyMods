namespace SixtyNine
{
    public interface IJsonAssetsApi
    {
        int GetClothingId(string name);
        void LoadAssets(string path);
    }
}
