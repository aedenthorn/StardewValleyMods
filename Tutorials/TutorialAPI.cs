using Microsoft.Xna.Framework;
using System.Collections;
using System.Collections.Generic;

namespace Tutorials
{
    public interface ITutorialAPI
    {
        public void AddCategory(string key, string value);
        public void AddTutorial(string key, object indata);
        public void AddTutorialFrame(string key, string texture, int frames, int frameRate, Rectangle? startRect, string text);
        public void AddTutorialTrigger(string key, string tutorial, string category, List<string> categories);
    }
    public class TutorialAPI : ITutorialAPI
    {
        public void AddCategory(string key, string value)
        {
            ModEntry.CatDict[key] = value;
        }

        public void AddTutorialFrame(string key, string texture, int frames, int frameRate, Rectangle? startRect, string text)
        {
            if(ModEntry.TutorialDict.TryGetValue(key, out var data))
            {
                data.Frames.Add(new TutorialFrame()
                {
                    Texture = texture,
                    Frames = frames,
                    FrameRate = frameRate,
                    StartRect = startRect,
                    Text = text
                });
            }
        }
        public void AddTutorialTrigger(string key, string tutorial, string category, List<string> categories)
        {
            ModEntry.TriggerDict.Add(key, new()
            {
                Tutorial = tutorial,
                Category = category,
                Categories = categories
            });
        }

        public void AddTutorial(string key, object indata)
        {
            var data = new TutorialData();
            foreach (var p in indata.GetType().GetProperties())
            {
                if (p.Name == "Frames")
                {
                    var inframes = (IEnumerable<object>)p.GetValue(indata);
                    foreach(var inframe in inframes)
                    {
                        var frame = new TutorialFrame();
                        foreach (var p2 in inframe.GetType().GetProperties())
                        {
                            foreach (var p3 in frame.GetType().GetProperties())
                            {
                                if (p2.Name == p3.Name)
                                {
                                    p3.SetValue(frame, p2.GetValue(inframe));
                                }
                            }
                        }
                        data.Frames.Add(frame);
                    }
                    continue;
                }
                foreach (var p2 in data.GetType().GetProperties())
                {
                    if(p2.Name == p.Name)
                    {
                        p2.SetValue(data, p.GetValue(indata));
                        break;
                    }
                }
            }
            ModEntry.TutorialDict[key] = data;
        }
    }
}