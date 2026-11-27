
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Monsters;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;

namespace PetHats
{
    public partial class ModEntry
    {

        private void ReloadData()
        {
            hatDict.Clear();
            catOffsetDict = GetFrameOffsets(true);
            dogOffsetDict = GetFrameOffsets(false);
            hatOffsetDict = GetHatOffsets();

        }

        private static Hat GetHat(string str)
        {
            Dictionary<int, string> dictionary = Game1.content.Load<Dictionary<int, string>>("Data\\hats");

            foreach (var kvp in dictionary)
            {
                if (kvp.Value.Equals(str))
                {
                    var hat = new Hat(kvp.Key);
                    hatDict[str] = hat;
                    return hat;
                }
            }
            str = str.Split('/')[0];
            foreach (var kvp in dictionary)
            {
                if (kvp.Value.StartsWith(str + "/"))
                {
                    var hat = new Hat(kvp.Key);
                    hatDict[str] = hat;
                    return hat;
                }
            }
            return null;
        }
        private static string GetHatString(Hat instance)
        {
            return Game1.content.Load<Dictionary<int, string>>("Data\\hats").TryGetValue(instance.which.Value, out var str) ? str : instance.Name;
        }

        private static void TryReturnHat(Pet instance, Farmer who, string str)
        {
            var hat = GetHat(str);
            if (hat != null)
            {
                if (!who.addItemToInventoryBool(hat))
                {
                    who.currentLocation.debris.Add(new Debris(hat, who.Position));
                }
            }
        }

        private static bool GetFrameOffsetsBool(Pet __instance, out int x, out int y, out int direction)
        {
            x = 0;
            y = 0;
            direction = 0;
            var dict = __instance is Cat ? catOffsetDict : dogOffsetDict;
            if(dict.TryGetValue(__instance.Sprite.CurrentFrame, out var data))
            {
                if (data.disable)
                    return false;
                if (__instance.flip)
                {
                    x = data.flippedX; 
                    y = data.flippedY;
                    direction = data.direction;
                    if(direction == 1)
                        direction = 3;
                    else if(direction == 3)
                        direction = 1;
                }
                else
                {
                    x = data.X; 
                    y = data.Y;
                    direction = data.direction;
                }
            }
            return true;
        }
        private static Dictionary<int, FrameOffsetData> GetFrameOffsets(bool cat)
        {
            return SHelper.GameContent.Load<Dictionary<int, FrameOffsetData>>(cat ? catPath : dogPath);
            Dictionary<int, FrameOffsetData> dataDict = new();
            if (cat)
            {
                dataDict.Add(0, new FrameOffsetData()
                {
                    flippedX = 24,
                    flippedY = -38,
                    X = 28,
                    Y = -38
                });
                dataDict[2] = dataDict[0];

                dataDict.Add(1, new FrameOffsetData()
                {
                    flippedX = 24,
                    flippedY = -34,
                    X = 28,
                    Y = -34
                });
                dataDict[3] = dataDict[1];

                dataDict.Add(4, new FrameOffsetData()
                {
                    flippedX = 0,
                    flippedY = -40,
                    X = 52,
                    Y = -40
                });
                dataDict[6] = dataDict[4];

                dataDict.Add(5, new FrameOffsetData()
                {
                    flippedX = 0,
                    flippedY = -40,
                    X = 52,
                    Y = -40
                });
                dataDict[7] = dataDict[5];

                dataDict.Add(8, new FrameOffsetData()
                {
                    flippedX = 28,
                    flippedY = -64,
                    X = 28,
                    Y = -64
                });
                dataDict[10] = dataDict[8];

                dataDict.Add(9, new FrameOffsetData()
                {
                    flippedX = 28,
                    flippedY = -60,
                    X = 28,
                    Y = -60
                });
                dataDict[11] = dataDict[9];

                dataDict.Add(12, new FrameOffsetData()
                {
                    flippedX = 52,
                    flippedY = -44,
                    X = 4,
                    Y = -44
                });
                dataDict[14] = dataDict[12];

                dataDict.Add(13, new FrameOffsetData()
                {
                    flippedX = 52,
                    flippedY = -40,
                    X = 4,
                    Y = -40
                });
                dataDict[15] = dataDict[13];
                
                dataDict.Add(16, new FrameOffsetData()
                {
                    flippedX = 24,
                    flippedY = -44,
                    X = 28,
                    Y = -44
                });

                dataDict.Add(21, new FrameOffsetData()
                {
                    flippedX = 24,
                    flippedY = -48,
                    X = 28,
                    Y = -48
                });
                dataDict[23] = dataDict[21];
                
                dataDict.Add(17, new FrameOffsetData()
                {
                    flippedX = 24,
                    flippedY = -52,
                    X = 28,
                    Y = -52
                });
                dataDict[20] = dataDict[17];
                dataDict[22] = dataDict[17];


                dataDict.Add(18, new FrameOffsetData()
                {
                    flippedX = 24,
                    flippedY = -56,
                    X = 28,
                    Y = -56
                });
                dataDict[19] = dataDict[18];

                dataDict.Add(25, new FrameOffsetData()
                {
                    flippedX = 0,
                    flippedY = -32,
                    X = 52,
                    Y = -32
                });
                                
                dataDict.Add(26, new FrameOffsetData()
                {
                    flippedX = 0,
                    flippedY = -28,
                    X = 52,
                    Y = -28
                });
                dataDict[27] = dataDict[26];
                                
                dataDict.Add(28, new FrameOffsetData()
                {
                    flippedX = 24,
                    flippedY = -28,
                    X = 28,
                    Y = -28
                });
                dataDict[29] = dataDict[28];
                
                dataDict.Add(30, new FrameOffsetData()
                {
                    flippedX = 8,
                    flippedY = -36,
                    X = 48,
                    Y = -36
                });
                dataDict[31] = dataDict[30];
            }
            return dataDict;
        }

        private static bool GetHatOffsetBool(Pet __instance, Hat hat, out Vector2 hatOffset)
        {
            hatOffset = Vector2.Zero;
            if(hatOffsetDict.TryGetValue(hat.which.Value, out var data))
            {
                if (__instance.flip)
                {
                    hatOffset = new Vector2(data.flippedX, data.flippedY);
                }
                else
                {
                    hatOffset = new Vector2(data.flippedX, data.flippedY);
                }
                FrameOffsetData fd = null;
                switch(__instance.FacingDirection)
                {
                    case 0:
                        fd = data.facingUp;
                        break;
                    case 1:
                        fd = data.facingRight;
                        break;
                    case 2:
                        fd = data.facingDown;
                        break;
                    case 3:
                        fd = data.facingLeft;
                        break;
                }
                if(fd is not null)
                {
                    if (fd.disable)
                        return false;
                    if(__instance.flip)
                    {
                        hatOffset.X += fd.flippedX;
                        hatOffset.Y += fd.flippedY;
                    }
                    else
                    {
                        hatOffset.X += fd.X;
                        hatOffset.Y += fd.Y;
                    }
                }
            }
            hatOffset *= 4f;
            return true;
        }
        private static Dictionary<int, HatOffsetData> GetHatOffsets()
        {
            return SHelper.GameContent.Load<Dictionary<int, HatOffsetData>>(hatPath);

            Dictionary<int, HatOffsetData> dataDict = new();
            dataDict.Add(6, new HatOffsetData()
            {
                Y = 2,
                flippedY = 2,
                facingDown = new FrameOffsetData()
                {
                    flippedY = -1
                }
            });
            dataDict.Add(11, new HatOffsetData()
            {
                facingLeft = new FrameOffsetData()
                {
                    X = -2,
                    flippedX = 2
                },
                facingRight = new FrameOffsetData()
                {
                    X = -2,
                    flippedX = 2
                }
            });
            dataDict.Add(10, new HatOffsetData()
            {
                Y = 3,
                facingUp = new FrameOffsetData() { disable = true }
            });
            dataDict.Add(14, new HatOffsetData()
            {
                facingUp = new FrameOffsetData() { disable = true }
            });
            dataDict.Add(26, new HatOffsetData()
            {

                facingLeft = new FrameOffsetData()
                {
                    X = -1,
                    flippedX = 1
                },
                facingRight = new FrameOffsetData()
                {
                    X = -1,
                    flippedX = 1
                }
            });
            dataDict.Add(31, new HatOffsetData()
            {
                Y = 1,
            });
            dataDict.Add(32, new HatOffsetData()
            {

                facingUp = new FrameOffsetData()
                {
                    Y = 1,
                    flippedY = 1
                },
                facingDown = new FrameOffsetData()
                {
                    Y = 1,
                    flippedY = 1
                }
            });
            dataDict.Add(56, new HatOffsetData()
            {
                facingUp = new FrameOffsetData() { disable = true }
            });
            dataDict.Add(61, new HatOffsetData()
            {

                facingUp = new FrameOffsetData()
                {
                    Y = -1,
                    flippedY = -1
                },
                facingDown = new FrameOffsetData()
                {
                    Y = -1,
                    flippedY = -1
                }
            });
            return dataDict;
        }

    }
}