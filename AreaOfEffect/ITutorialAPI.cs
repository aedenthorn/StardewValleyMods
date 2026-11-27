using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace AreaOfEffect
{
    public interface ITutorialAPI
    {
        public void AddCategory(string key, string value);
        public void AddTutorial(string key, object indata);
        public void AddTutorialFrame(string key, string texture, int frames, int frameRate, Rectangle? startRect, string text);
        public void AddTutorialTrigger(string key, string tutorial, string category, List<string> categories);
    }
}