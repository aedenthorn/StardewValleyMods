using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.GameData.Buildings;
using System.Collections.Generic;

namespace MapTokens
{
    internal class MailboxTiles : PropertyTile
    {
        private int which;
        private string separator;

        public MailboxTiles(int _which = 0, string _s = " ") : base(_which, _s)
        {
        }

        /*********
        ** Fields
        *********/
        private List<string> positions = new List<string>();


        /// <summary>Whether the token may return multiple values for the given input.</summary>
        /// <param name="input">The input arguments, if applicable.</param>
        public override bool CanHaveMultipleValues(string input = null)
        {
            return true;
        }

        /****
        ** State
        ****/
        /// <summary>Update the values when the context changes.</summary>
        /// <returns>Returns whether the value changed, which may trigger patch updates.</returns>
        public override bool UpdateContext()
        {
            var newList = new List<string>();
            var farm = (Game1.getLocationFromName("Farm") as Farm);
            if (farm == null)
                return false;

            bool changed = false;
            foreach (var b in farm.buildings)
            {
                if (b.buildingType.Value != "FarmHouse" && b.buildingType.Value != "Cabin")
                    continue;
                BuildingData buildingData = b.GetData();
                foreach (BuildingActionTile action in buildingData.ActionTiles)
                {
                    if (action.Action == "Mailbox")
                    {
                        newList.Add(GetString(new Point(b.tileX.Value + action.Tile.X, b.tileY.Value + action.Tile.Y)));
                        break;
                    }
                }
            }
            var main = GetString(farm.GetMainMailboxPosition());
            if (!newList.Contains(main))
                newList.Insert(0, main);
            if (newList.Count != positions.Count)
            {
                changed = true;
            }
            else
            {
                for (int i = 0; i < newList.Count; i++)
                {
                    if (newList[i] != positions[i])
                    {
                        changed = true;
                        break;
                    }
                }
            }
            positions = newList;
            return changed;
        }

        /// <summary>Get whether the token is available for use.</summary>
        public override bool IsReady()
        {
            return true;
        }

        /// <summary>Get the current values.</summary>
        /// <param name="input">The input arguments, if applicable.</param>
        public override IEnumerable<string> GetValues(string input)
        {
            return positions;
        }
    }
}