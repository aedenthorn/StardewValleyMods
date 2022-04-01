using Microsoft.Xna.Framework;
using StardewValley;
using System;
using System.Collections.Generic;

namespace NPCConversations
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