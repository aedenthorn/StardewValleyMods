using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using System;

namespace MapTeleport
{
    public interface IMobilePhoneApi
    {   
        bool AddApp(string id, string name, Action action, Texture2D icon);

        Vector2 GetScreenPosition();
        Vector2 GetScreenSize();
        Vector2 GetScreenSize(bool rotated);
        Rectangle GetPhoneRectangle();
        Rectangle GetScreenRectangle();
        bool GetPhoneRotated();
        void SetPhoneRotated(bool value);
        bool GetPhoneOpened();
        void SetPhoneOpened(bool value);
        bool GetAppRunning();
        void SetAppRunning(bool value);
        string GetRunningApp();
        void SetRunningApp(string value);

        void PlayRingTone();
        void PlayNotificationTone();
        NPC GetCallingNPC();
        bool IsCallingNPC();
    }
}