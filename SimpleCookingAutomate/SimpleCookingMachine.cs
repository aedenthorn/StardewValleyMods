using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Pathoschild.Stardew.Automate;
using StardewValley;
using SObject = StardewValley.Object;

namespace SimpleCookingAutomate
{
    public class SimpleCookingAutomateMachine : IMachine
    {
        /*********
        ** Fields
        *********/
        /// <summary>The underlying entity.</summary>
        private readonly SObject Entity;


        /*********
        ** Accessors
        *********/
        /// <summary>The location which contains the machine.</summary>
        public GameLocation Location { get; }

        /// <summary>The tile area covered by the machine.</summary>
        public Rectangle TileArea { get; }

        /// <summary>A unique ID for the machine type.</summary>
        /// <remarks>This value should be identical for two machines if they have the exact same behavior and input logic. For example, if one machine in a group can't process input due to missing items, Automate will skip any other empty machines of that type in the same group since it assumes they need the same inputs.</remarks>
        public string MachineTypeID { get; } = "aedenthorn.SimpleCookingAutomate/Cooker";


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="entity">The underlying entity.</param>
        /// <param name="location">The location which contains the machine.</param>
        /// <param name="tile">The tile covered by the machine.</param>
        public SimpleCookingAutomateMachine(SObject entity, GameLocation location, in Vector2 tile)
        {
            this.Entity = entity;
            this.Location = location;
            this.TileArea = new Rectangle((int)tile.X, (int)tile.Y, 1, 1);
        }

        /// <summary>Get the machine's processing state.</summary>
        public MachineState GetState()
        {
            if (!ModEntry.scapi.TryGetCookingDataForCooker(Entity, out var data))
                return MachineState.Empty;

            return data.Progress >= 1
                ? MachineState.Done
                : MachineState.Processing;
        }

        /// <summary>Get the output item.</summary>
        public ITrackedStack GetOutput()
        {
            if (!ModEntry.scapi.TryGetCookingDataForCooker(Entity, out var data) || data.Progress < 1)
                return null;
            return new TrackedItem(data.GetProduct(), () =>
            {
                ModEntry.scapi.SetCookingDataForCooker(Entity, null);
            });
        }

        /// <summary>Provide input to the machine.</summary>
        /// <param name="input">The available items.</param>
        /// <returns>Returns whether the machine started processing an item.</returns>
        public bool SetInput(IStorage input)
        {
            foreach(var item in input.GetItems())
            {
                if (item.Sample is not SObject obj)
                    continue;
                if (ModEntry.scapi.TryGetCookingDataForCookable(obj, out var data))
                {
                    if(input.TryConsume(stack =>
                    {
                        return stack.Sample.QualifiedItemId == item.Sample.QualifiedItemId;
                    }, 1))
                    {
                        Entity.Location.playSound(data.PlacedSound, Entity.TileLocation);
                        ModEntry.scapi.SetCookingDataForCooker(Entity, data);
                        return true;
                    }
                }
            }

            return false;
        }
    }
}