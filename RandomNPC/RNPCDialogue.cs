using System;
using System.Collections;

namespace RandomNPC
{
    internal class RNPCDialogue
    {
        private string gender;
        private string situation;
        private string mood;
        private string friendship;
        private string personality;
        private string age;
        private string manners;
        private string anxiety;
        private string optimism;

        public RNPCDialogue(string dialogueString)
        {
            string[] dialogueArray = dialogueString.Split('/');
            this.age = dialogueArray[0];
            this.manners = dialogueArray[1];
            this.anxiety = dialogueArray[2];
            this.optimism = dialogueArray[3];
            this.gender = dialogueArray[4];
            this.mood = dialogueArray[5];
            this.friendship = dialogueArray[5];
        }

    }
}