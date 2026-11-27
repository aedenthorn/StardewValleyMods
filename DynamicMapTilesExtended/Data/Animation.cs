using Microsoft.Xna.Framework;
using StardewValley;

namespace DMT.Data
{
    public class Animation
    {
        public string name;
        /// <summary>
        /// A unique name for this animation
        /// </summary>
        public string group;
        /// <summary>
        /// A unique name for this animation
        /// </summary>
        public string Name
        {
            get => name; 
            set => name = value;
        }
        /// <summary>
        /// A group name for this animation
        /// </summary>
        public string Group
        {
            get => group; 
            set => group = value;
        }
        public const int idBase = -87965;
        public int id = -87965;
        /// <summary>
        /// The internal game id for this animation
        /// </summary>
        public int Id
        {
            get => id; 
            set => id = value;
        }
        public int textureRowIndex = -1;
        /// <summary>
        /// The index of the row this animation starts from in the texture
        /// </summary>
        public int TextureRowIndex
        {
            get => textureRowIndex;
            set => textureRowIndex = value;
        }
        public Rectangle sourceRect;
        /// <summary>
        /// The initial source rectangle of the animation in the texture
        /// </summary>
        public Rectangle SourceRect
        {
            get => sourceRect;
            set => sourceRect = value;
        }
        public Vector2 position;
        /// <summary>
        /// The position in the map at which the animation should start
        /// </summary>
        public Vector2 Position
        {
            get => position;
            set => position = value;
        }
        public Vector2 motion = Vector2.Zero;
        /// <summary>
        /// The way in which the animation should move around the map
        /// </summary>
        public Vector2 Motion
        {
            get => motion;
            set => motion = value;
        }
        public Vector2 acceleration = Vector2.Zero;
        /// <summary>
        /// The speed at which the animation moves around the map
        /// </summary>
        public Vector2 Acceleration
        {
            get => acceleration;
            set => acceleration = value;
        }
        public Color color = Color.White;
        /// <summary>
        /// The color of the animated texture
        /// </summary>
        public Color Color
        {
            get => color;
            set => color = value;
        }
        public bool flicker = false;
        /// <summary>
        /// Whether or not the texture of the animation should flicker
        /// </summary>
        public bool Flicker
        {
            get => flicker;
            set => flicker = value;
        }
        public bool flipped = false;
        /// <summary>
        /// Whether or not the texture of the animation should be flipped
        /// </summary>
        public bool Flipped
        {
            get => flipped;
            set => flipped = value;
        }
        public bool local = false;
        /// <summary>
        /// Whether or not to use the local position instead of the global position (e.g. use tile position or pixel position)
        /// </summary>
        public bool Local
        {
            get => local;
            set => local = value;
        }
        public float interval = 100;
        /// <summary>
        /// The interval between animation frames in milliseconds
        /// </summary>
        public float Interval
        {
            get => interval;
            set => interval = value;
        }
        public float layerDepth = -1;
        /// <summary>
        /// Controls when the texture should be drawn over or under other textures (higher: draw over, lower: draw under)
        /// </summary>
        public float LayerDepth
        {
            get => layerDepth;
            set => layerDepth = value;
        }
        public float alphaFade = 0f;
        /// <summary>
        /// The strenth at which the texture should fade out
        /// </summary>
        public float AlphaFade
        {
            get => alphaFade;
            set => alphaFade = value;
        }
        public float scale;
        /// <summary>
        /// The scale of the texture
        /// </summary>
        public float Scale
        {
            get => scale;
            set => scale = value;
        }
        public float scaleChange;
        /// <summary>
        /// By how much should the scale change when the animation updates
        /// </summary>
        public float ScaleChange
        {
            get => scaleChange;
            set => scaleChange = value;
        }
        public float rotation;
        /// <summary>
        /// The rotation of the texture
        /// </summary>
        public float Rotation
        {
            get => rotation;
            set => rotation = value;
        }
        public float rotationChange;
        /// <summary>
        /// By how much should the rotation change when the animation updates
        /// </summary>
        public float RotationChange
        {
            get => rotationChange;
            set => rotationChange = value;
        }
        public int loops = 0;
        /// <summary>
        /// The amount of times the animation should loop
        /// </summary>
        public int Loops
        {
            get => loops;
            set => loops = value;
        }
        public int length = 8;
        /// <summary>
        /// The amount of frames this animation consists of
        /// </summary>
        public int Length
        {
            get => length;
            set => length = value;
        }
        public int delay = 0;
        /// <summary>
        /// The delay before the animation should start
        /// </summary>
        public int Delay
        {
            get => delay;
            set => delay = value;
        }
        public int sourceRectWidth = -1;
        /// <summary>
        /// The width of the source rectangle of the texture
        /// </summary>
        public int SourceRectWidth
        {
            get => sourceRectWidth;
            set => sourceRectWidth = value;
        }
        public int sourceRectHeight = -1;
        /// <summary>
        /// The height of the source rectangle of the texture
        /// </summary>
        public int SourceRectHeight
        {
            get => sourceRectHeight; 
            set => sourceRectHeight = value;
        }
        public string texture;
        /// <summary>
        /// The name of the texture to use (Default "TileSheets\\animations")
        /// </summary>
        public string Texture
        {
            get => texture;
            set => texture = value;
        }

        public TemporaryAnimatedSprite? ToSAnim()
        {
            TemporaryAnimatedSprite? sprite = null;
            string[] split2 = [];
            if (!string.IsNullOrWhiteSpace(Texture))
            {
                try
                {
                    if(Id == idBase)
                    {
                        Id -= ++animationCounter;
                    }
                    sprite = new(Texture, SourceRect, Interval, Length, Loops, Position, Flicker, Flipped, LayerDepth, AlphaFade, Color, Scale, ScaleChange, Rotation, RotationChange, Local)
                    {
                        motion = Motion,
                        acceleration = Acceleration,
                        delayBeforeAnimationStart = Delay,
                        id = Id,
                    };
                    return sprite;
                }
                catch(Exception ex) 
                {
                    context.Monitor.Log("Could not create animation from custom texture", LogLevel.Error);
                    context.Monitor.Log($"[{ex.GetType().Name}] {ex.Message}\n{ex.StackTrace}", LogLevel.Error);
                    return null;
                }
            }
            try
            {
                sprite = new(TextureRowIndex, Position, Color, Length, Flipped, Interval, Loops, SourceRectWidth, LayerDepth, SourceRectHeight, Delay)
                {
                    id = Id
                };
            }
            catch (Exception ex)
            {
                context.Monitor.Log("Could not create animation from default texture", LogLevel.Error);
                context.Monitor.Log($"[{ex.GetType().Name}] {ex.Message}\n{ex.StackTrace}", LogLevel.Error);
                return null;
            }
            return sprite;
        }
    }
}
