using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using System.Collections.Generic;

namespace PlayingCards
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry
    {
        private static bool Object_draw_prefix_1(Object __instance, SpriteBatch spriteBatch, int x, int y, float alpha)
        {
            if (!Config.EnableMod || !__instance.modData.TryGetValue(deckKey, out string deckString))
                return true;

            var topCard = new PlayingCard(deckString.Split(',')[0]);
            Vector2 scaleFactor = __instance.getScale();
            scaleFactor *= 4f;
            Vector2 position = Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(x * 64), (float)(y * 64 - 64)));
            Rectangle destination = new Rectangle((int)(position.X - scaleFactor.X / 2f) + ((__instance.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (int)(position.Y - scaleFactor.Y / 2f) + ((__instance.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (int)(64f + scaleFactor.X), (int)(128f + scaleFactor.Y / 2f));
            float draw_layer = System.Math.Max(0f, (float)((y + 1) * 64 - 24) / 10000f) + (float)x * 1E-05f;
            if (topCard.facing == "up")
            {
                spriteBatch.Draw(cardTexture, destination, new Rectangle(topCard.index * 32, topCard.suit * 64, 32, 64), Color.White * alpha, 0f, Vector2.Zero, SpriteEffects.None, draw_layer);
            }
            else
            {
                spriteBatch.Draw(backTexture, destination, new Rectangle(0, 0, 32, 64), Color.White * alpha, 0f, Vector2.Zero, SpriteEffects.None, draw_layer);
            }
            return false;
        }
        private static bool Object_draw_prefix_2(Object __instance, SpriteBatch spriteBatch, int xNonTile, int yNonTile, float layerDepth, float alpha)
        {
            if (!Config.EnableMod || !__instance.modData.TryGetValue(deckKey, out string deckString))
                return true;

            var topCard = new PlayingCard(deckString.Split(',')[0]);

            Vector2 scaleFactor = __instance.getScale();
            scaleFactor *= 4f;
            Vector2 position = Game1.GlobalToLocal(Game1.viewport, new Vector2((float)xNonTile, (float)yNonTile));
            Rectangle destination = new Rectangle((int)(position.X - scaleFactor.X / 2f) + ((__instance.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (int)(position.Y - scaleFactor.Y / 2f) + ((__instance.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (int)(64f + scaleFactor.X), (int)(128f + scaleFactor.Y / 2f));
            if(topCard.facing == "up")
            {
                spriteBatch.Draw(cardTexture, destination, new Rectangle(topCard.index * 32, topCard.suit * 64, 32, 64), Color.White * alpha, 0f, Vector2.Zero, SpriteEffects.None, layerDepth);
            }
            else
            {
                spriteBatch.Draw(backTexture, destination, new Rectangle(0, 0, 32, 64), Color.White * alpha, 0f, Vector2.Zero, SpriteEffects.None, layerDepth);
            }
            return false;
        }
        private static bool Object_placementAction_prefix(Object __instance, GameLocation location, int x, int y, Farmer who)
        {
            string tileDeckString = null;
            int X = x / 64;
            int Y = y / 64;
            Vector2 tile = new Vector2(X, Y);
            if (!Config.EnableMod || location == null || who == null || (!SHelper.Input.IsDown(Config.DealDownModButton) && !SHelper.Input.IsDown(Config.DealUpModButton)) || !__instance.modData.TryGetValue(deckKey, out string deckString) || (location.objects.TryGetValue(tile, out Object tileObject) && !tileObject.modData.TryGetValue(deckKey, out tileDeckString)))
                return true;

            CardDeck deck = new CardDeck(deckString);
            PlayingCard card = deck.cards[0];
            card.facing = SHelper.Input.IsDown(Config.DealDownModButton) ? "down" : "up";

            if (tileDeckString != null)
            {
                CardDeck tileDeck = new CardDeck(tileDeckString);
                SMonitor.Log($"Placing {card.index}:{card.suit} in existing deck of {tileDeck.cards.Count}");
                tileDeck.cards.Insert(0, card);
                location.objects[tile].modData[deckKey] = tileDeck.GetDeckString();
            }
            else
            {
                SMonitor.Log($"Creating new deck at {X},{Y}");
                Object newDeck = new Object(tile, 0);
                newDeck.modData[deckKey] = card.GetString();
                location.objects.Add(tile, newDeck);
            }
            deck.cards.RemoveAt(0);
            if(deck.cards.Count == 0)
            {
                SMonitor.Log("removing empty deck in hand");
                who.reduceActiveItemByOne();
            }
            else
            {
                SMonitor.Log($"deck in hand has {deck.cards.Count} cards left");
                __instance.modData[deckKey] = deck.GetDeckString();
            }
            return false;
        }
    }
}