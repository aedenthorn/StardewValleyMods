using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Swim
{
    public interface IJsonAssetsApi
    {
        int GetClothingId(string name);
        int GetHatId(string name);
        void LoadAssets(string path);
    }
}
