using Microsoft.Xna.Framework;
using StardewValley;
using System;
using System.Collections.Generic;

namespace AprilFools
{
    public class SpeakingAnimalData
    {
        public float textAboveHeadAlpha;
        public string textAboveHead;
        public float textAboveHeadPreTimer;
        public int textAboveHeadTimer;
        public int textAboveHeadStyle;
        public int textAboveHeadColor;
        public float bTextAboveHeadAlpha;
        public string bTextAboveHead;
        public float bTextAboveHeadPreTimer;
        public int bTextAboveHeadTimer;
        public int bTextAboveHeadStyle;
        public int bTextAboveHeadColor;
        public long aID;
        public long bID;

        public static string[][] animalSpeech = new string[][]
        {
            new string[]
            {
                ModEntry.SHelper.Translation.Get("animal-speech-1-1"),
                ModEntry.SHelper.Translation.Get("animal-speech-1-2")
            },
            new string[]
            {
                ModEntry.SHelper.Translation.Get("animal-speech-2-1"),
                ModEntry.SHelper.Translation.Get("animal-speech-2-2")
            },
            new string[]
            {
                ModEntry.SHelper.Translation.Get("animal-speech-3-1"),
                ModEntry.SHelper.Translation.Get("animal-speech-3-2")
            },
            new string[]
            {
                ModEntry.SHelper.Translation.Get("animal-speech-4-1"),
                ModEntry.SHelper.Translation.Get("animal-speech-4-2")
            },
            new string[]
            {
                ModEntry.SHelper.Translation.Get("animal-speech-5-1"),
                ModEntry.SHelper.Translation.Get("animal-speech-5-2")
            },
            new string[]
            {
                ModEntry.SHelper.Translation.Get("animal-speech-6-1"),
                ModEntry.SHelper.Translation.Get("animal-speech-6-2")
            },
            new string[]
            {
                ModEntry.SHelper.Translation.Get("animal-speech-7-1"),
                ModEntry.SHelper.Translation.Get("animal-speech-7-2")
            },
            new string[]
            {
                ModEntry.SHelper.Translation.Get("animal-speech-8-1"),
                ModEntry.SHelper.Translation.Get("animal-speech-8-2")
            },
            new string[]
            {
                ModEntry.SHelper.Translation.Get("animal-speech-9-1"),
                ModEntry.SHelper.Translation.Get("animal-speech-9-2")
            },
            new string[]
            {
                ModEntry.SHelper.Translation.Get("animal-speech-10-1"),
                ModEntry.SHelper.Translation.Get("animal-speech-10-2")
            }
        };

        public SpeakingAnimalData(FarmAnimal a, FarmAnimal b)
        {
            aID = a.myID.Value;
            textAboveHeadAlpha = 0f;
            textAboveHeadPreTimer = 0;
            textAboveHeadTimer = 3000;
            textAboveHeadStyle = -1;
            textAboveHeadColor = -1;

            bID = b.myID.Value;
            bTextAboveHeadAlpha = 0f;
            bTextAboveHeadPreTimer = 2000 + Game1.random.Next(500);
            bTextAboveHeadTimer = 3000;
            bTextAboveHeadStyle = -1;
            bTextAboveHeadColor = -1;
            var strings = animalSpeech[Game1.random.Next(animalSpeech.Length)];
            textAboveHead = strings[0];
            bTextAboveHead = strings[1];
        }
    }
}