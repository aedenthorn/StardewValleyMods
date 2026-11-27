using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using SObject = StardewValley.Object;

namespace SimpleCooking
{
    public interface IAutomateAPI
    {
        /// <summary>Add an automation factory.</summary>
        /// <param name="factory">An automation factory which construct machines, containers, and connectors.</param>
        void AddFactory(IAutomationFactory factory);

        /// <summary>Get the status of machines in a tile area. This is a specialized API for Data Layers and similar mods.</summary>
        /// <param name="location">The location for which to display data.</param>
        /// <param name="tileArea">The tile area for which to display data.</param>
        IDictionary<Vector2, int> GetMachineStates(GameLocation location, Rectangle tileArea);
    }
    public interface IAutomationFactory
    {
        /*********
        ** Accessors
        *********/
        /// <summary>Get a machine, container, or connector instance for a given object.</summary>
        /// <param name="obj">The in-game object.</param>
        /// <param name="location">The location to check.</param>
        /// <param name="tile">The tile position to check.</param>
        /// <returns>Returns an instance or <c>null</c>.</returns>
        IAutomatable? GetFor(SObject obj, GameLocation location, in Vector2 tile);

        /// <summary>Get a machine, container, or connector instance for a given terrain feature.</summary>
        /// <param name="feature">The terrain feature.</param>
        /// <param name="location">The location to check.</param>
        /// <param name="tile">The tile position to check.</param>
        /// <returns>Returns an instance or <c>null</c>.</returns>
        IAutomatable? GetFor(TerrainFeature feature, GameLocation location, in Vector2 tile);

        /// <summary>Get a machine, container, or connector instance for a given building.</summary>
        /// <param name="building">The building.</param>
        /// <param name="location">The location to check.</param>
        /// <param name="tile">The tile position to check.</param>
        /// <returns>Returns an instance or <c>null</c>.</returns>
        IAutomatable? GetFor(Building building, GameLocation location, in Vector2 tile);

        /// <summary>Get a machine, container, or connector instance for a given tile position.</summary>
        /// <param name="location">The location to check.</param>
        /// <param name="tile">The tile position to check.</param>
        /// <returns>Returns an instance or <c>null</c>.</returns>
        IAutomatable? GetForTile(GameLocation location, in Vector2 tile);
    }

    /// <summary>An automatable entity, which can implement a more specific type like <see cref="IMachine"/> or <see cref="IContainer"/>. If it doesn't implement a more specific type, it's treated as a connector with no additional logic.</summary>
    public interface IAutomatable
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The location which contains the machine.</summary>
        GameLocation Location { get; }

        /// <summary>The tile area covered by the machine.</summary>
        Rectangle TileArea { get; }
    }


    /// <summary>A machine that accepts input and provides output.</summary>
    public interface IMachine : IAutomatable
    {
        /*********
        ** Accessors
        *********/
        /// <summary>A unique ID for the machine type.</summary>
        /// <remarks>This value should be identical for two machines if they have the exact same behavior and input logic. For example, if one machine in a group can't process input due to missing items, Automate will skip any other empty machines of that type in the same group since it assumes they need the same inputs.</remarks>
        string MachineTypeID { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Get the machine's processing state.</summary>
        MachineState GetState();

        /// <summary>Get the output item.</summary>
        ITrackedStack? GetOutput();

        /// <summary>Provide input to the machine.</summary>
        /// <param name="input">The available items.</param>
        /// <returns>Returns whether the machine started processing an item.</returns>
        bool SetInput(IStorage input);
    }

    /// <summary>An item stack in an input pipe which can be reduced or taken.</summary>
    public interface ITrackedStack
    {
        /*********
        ** Accessors
        *********/
        /// <summary>A sample item for comparison.</summary>
        /// <remarks>This should be equivalent to the underlying item (except in stack size), but *not* a reference to it.</remarks>
        Item Sample { get; }

        /// <summary>The identifier for the type definition which contains the item, matching one of the <see cref="ItemRegistry"/> <c>type_</c> constants.</summary>
        string Type { get; }

        /// <summary>The number of items in the stack.</summary>
        int Count { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Remove the specified number of this item from the stack.</summary>
        /// <param name="count">The number to consume.</param>
        void Reduce(int count);

        /// <summary>Remove the specified number of this item from the stack and return a new stack matching the count.</summary>
        /// <param name="count">The number to get.</param>
        Item? Take(int count);
    }
    /// <summary>An ingredient stack (or stacks) which can be consumed by a machine.</summary>
    public interface IConsumable
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The items available to consumable.</summary>
        ITrackedStack Consumables { get; }

        /// <summary>A sample item for comparison.</summary>
        /// <remarks>This should not be a reference to the original stack.</remarks>
        Item Sample { get; }

        /// <summary>The number of items needed for the recipe.</summary>
        int CountNeeded { get; }

        /// <summary>Whether the consumables needed for this requirement are ready.</summary>
        bool IsMet { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Remove the needed number of this item from the stack.</summary>
        void Reduce();

        /// <summary>Remove the needed number of this item from the stack and return a new stack matching the count.</summary>
        Item? Take();
    }/// <summary>Describes a generic recipe based on item input and output.</summary>
    public interface IRecipe
    {
        /*********
        ** Accessors
        *********/
        /// <summary>Matches items that can be used as input.</summary>
        Func<Item, bool> Input { get; }

        /// <summary>The number of inputs needed.</summary>
        int InputCount { get; }

        /// <summary>The output to generate (given an input).</summary>
        Func<Item, Item> Output { get; }

        /// <summary>The time needed to prepare an output (given an input).</summary>
        Func<Item, int> Minutes { get; }


        /*********
        ** Methods
        *********/
        /// <summary>Get whether the recipe can accept a given item as input (regardless of stack size).</summary>
        /// <param name="stack">The item to check.</param>
        bool AcceptsInput(ITrackedStack stack);
    }
    /// <summary>Manages access to items in the underlying containers.</summary>
    public interface IStorage
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The storage containers that accept input, in priority order.</summary>
        IContainer[] InputContainers { get; }

        /// <summary>The storage containers that provide items, in priority order.</summary>
        IContainer[] OutputContainers { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Get whether any of the <see cref="InputContainers"/> or <see cref="OutputContainers"/> are locked.</summary>
        bool HasLockedContainers();

        /// <summary>Get all items from the given pipes.</summary>
        IEnumerable<ITrackedStack> GetItems();

        /****
        ** TryGetIngredient
        ****/
        /// <summary>Get an ingredient needed for a recipe.</summary>
        /// <param name="predicate">Returns whether an item should be matched.</param>
        /// <param name="count">The number of items to find.</param>
        /// <param name="consumable">The matching consumables.</param>
        /// <returns>Returns whether the requirement is met.</returns>
        bool TryGetIngredient(Func<ITrackedStack, bool> predicate, int count, [NotNullWhen(true)] out IConsumable? consumable);

        /// <summary>Get an ingredient needed for a recipe.</summary>
        /// <param name="recipes">The items to match.</param>
        /// <param name="consumable">The matching consumables.</param>
        /// <param name="recipe">The matched requisition.</param>
        /// <returns>Returns whether the requirement is met.</returns>
        bool TryGetIngredient(IRecipe[] recipes, [NotNullWhen(true)] out IConsumable? consumable, [NotNullWhen(true)] out IRecipe? recipe);

        /****
        ** TryConsume
        ****/
        /// <summary>Consume an ingredient needed for a recipe.</summary>
        /// <param name="predicate">Returns whether an item should be matched.</param>
        /// <param name="count">The number of items to find.</param>
        /// <returns>Returns whether the item was consumed.</returns>
        bool TryConsume(Func<ITrackedStack, bool> predicate, int count);

        /****
        ** TryPush
        ****/
        /// <summary>Add the given item stack to the output pipe if there's space.</summary>
        /// <param name="item">The item stack to push.</param>
        /// <returns>Returns whether at least some of the item stack was received.</returns>
        bool TryPush(ITrackedStack? item);
    }
    /// <summary>A machine processing state.</summary>
    public enum MachineState
    {
        /// <summary>The machine is not currently enabled (e.g. out of season or needs to be started manually).</summary>
        Disabled,

        /// <summary>The machine has no input.</summary>
        Empty,

        /// <summary>The machine is processing an input.</summary>
        Processing,

        /// <summary>The machine finished processing an input and has an output item ready.</summary>
        Done
    }
}