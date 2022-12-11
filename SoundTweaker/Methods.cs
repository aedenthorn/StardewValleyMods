using Netcode;
using Newtonsoft.Json;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Objects;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Object = StardewValley.Object;

namespace SoundTweaker
{
    public partial class ModEntry
    {
        internal struct RpcCurve
        {
            public float Evaluate(float position)
            {
                RpcPoint first = this.Points[0];
                if (position <= first.Position)
                {
                    return first.Value;
                }
                RpcPoint second = this.Points[this.Points.Length - 1];
                if (position >= second.Position)
                {
                    return second.Value;
                }
                for (int i = 1; i < this.Points.Length; i++)
                {
                    second = this.Points[i];
                    if (second.Position >= position)
                    {
                        break;
                    }
                    first = second;
                }
                RpcPointType type = first.Type;
                float t = (position - first.Position) / (second.Position - first.Position);
                return first.Value + (second.Value - first.Value) * t;
            }

            public uint FileOffset;

            public int Variable;

            public bool IsGlobal;
            public RpcParameter Parameter;

            public RpcPoint[] Points;
        }
        internal struct RpcPoint
        {
            public RpcPointType Type;

            public float Position;

            public float Value;
        }
        internal enum RpcPointType
        {
            Linear,
            Fast,
            Slow,
            SinCos
        }
        internal enum RpcParameter
        {
            Volume,
            Pitch,
            ReverbSend,
            FilterFrequency,
            FilterQFactor,
            NumParameters
        }
        internal struct DspParameter
        {
            public DspParameter(BinaryReader reader)
            {
                reader.ReadByte();
                this.Value = reader.ReadSingle();
                this.MinValue = reader.ReadSingle();
                this.MaxValue = reader.ReadSingle();
                reader.ReadUInt16();
            }

            public void SetValue(float value)
            {
                if (value < this.MinValue)
                {
                    this.Value = this.MinValue;
                    return;
                }
                if (value > this.MaxValue)
                {
                    this.Value = this.MaxValue;
                    return;
                }
                this.Value = value;
            }

            public override string ToString()
            {
                return string.Concat(new string[]
                {
                "Value:",
                this.Value.ToString(),
                " MinValue:",
                this.MinValue.ToString(),
                " MaxValue:",
                this.MaxValue.ToString()
                });
            }

            public float Value;

            public readonly float MinValue;

            public readonly float MaxValue;
        }
    }
}