using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SocialNetwork
{
    public class SocialPost
    {
        public NPC npc;
        public Texture2D portrait;
        public string text;
        public Rectangle sourceRect;
        public string[] postItems;
        public Texture2D picture;
        public string[] reactions;
        public string[] comments;
        public List<SocialPostReaction> postComments = new List<SocialPostReaction>();
        public List<SocialPostReaction> postReactions = new List<SocialPostReaction>();
        public int postHeight;
        public List<string> lines = new List<string>();

        public SocialPost(NPC npc, Texture2D portrait, Rectangle sourceRect, string post)
        {
            this.npc = npc;
            this.portrait = portrait;
            this.sourceRect = sourceRect;
            postItems = post.Split('^');
            GetPostDetails();
        }

        public void GetPostHeight()
        {
            postHeight = 0;
            postHeight += (Utils.GetTextLines(text).Count + 1) * 20;
            postHeight += (reactions != null ? 20 : 0);
            postHeight += (picture != null ? Utils.GetPictureHeight(picture) : 0);
            if(postComments != null && postComments.Count > 0)
            {
                foreach(SocialPostReaction c in postComments)
                {
                    postHeight += (c.lines.Count + 1)* 20;
                }
            }
        }

        private void GetPostDetails()
        {
            if(postItems.Length > 0)
            {
                foreach(string item in postItems)
                {
                    string key = item.Split(':')[0];
                    string val = string.Join("",item.Split(':').Skip(1));
                    if (key == null || val == null)
                        continue;
                    switch (key.ToLower())
                    {
                        case "text":
                            text = val;
                            lines = new List<string>(Utils.GetTextLines(text));
                            break;
                        case "picture":
                            picture = Utils.GetPicture(val);
                            break;
                        case "reactions":
                            reactions = val.Split(',');
                            GetReactions();
                            break;
                        case "comments":
                            comments = val.Split(',');
                            GetComments();
                            break;
                    }
                }

            }
        }

        public void GetComments()
        {
            foreach(string c in comments)
            {
                postComments.Add(new SocialPostReaction(this, c, true));
            }
        }
        public void GetReactions()
        {
            foreach(string like in reactions)
            {
                postReactions.Add(new SocialPostReaction(this, like, false));
            }
        }

        internal void Refresh()
        {
            lines = new List<string>(Utils.GetTextLines(text));
            if (postComments != null && postComments.Count > 0)
            {
                foreach (SocialPostReaction c in postComments)
                {
                    c.Refresh();
                }
            }
            GetPostHeight();

        }
    }
}