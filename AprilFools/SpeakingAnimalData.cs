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
                "To err is human...",
                "To forgive bovine."
            },
            new string[]
            {
                "If it talks like a duck...",
                "then you're drunk. Ducks don't talk."
            },
            new string[]
            {
                "If it talks like a duck...",
                "Then you're drunk. Ducks don't talk."
            },
            new string[]
            {
                "Why do cows have hooves and not feet?",
                "Because they lactose."
            },
            new string[]
            {
                "Hear about the happy-go-lucky farmer?",
                "They lived life by the seeds of their plants."
            },
            new string[]
            {
                "Hear about prize-winning scarecrow?",
                "It was out standing in its field."
            },
            new string[]
            {
                "They say making hay is difficult.",
                "Seems rather cut and dried to me."
            },
            new string[]
            {
                "They say making hay is difficult.",
                "Seems rather cut and dried to me."
            },
            new string[]
            {
                "It's pasture bedtime.",
                "Sheep happens."
            },
            new string[]
            {
                "Bees scare me.",
                "Yeah, the whole alphabet scares me."
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