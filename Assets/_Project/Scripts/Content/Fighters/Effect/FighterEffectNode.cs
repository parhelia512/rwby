using System;
using Fusion;
using UnityEngine;

namespace rwby
{
    [System.Serializable]
    public struct FighterEffectNode : INetworkStruct, IEquatable<rwby.FighterEffectNode>
    {
        public int bank;
        public int effect;
        public int frame;
        public int parent;
        public Vector3 pos;
        public Vector3 rot;
        public Vector3 scale;

        public bool Equals(FighterEffectNode other)
        {
            return bank == other.bank 
                   && effect == other.effect 
                   && frame == other.frame 
                   && parent == other.parent
                   && pos.Equals(other.pos)
                   && rot.Equals(other.rot)
                   && scale.Equals(other.scale);
        }

        public override bool Equals(object obj)
        {
            return obj is FighterEffectNode other && Equals(other);
        }
        
        public static bool operator ==(rwby.FighterEffectNode a, rwby.FighterEffectNode b)
        {
            return a.bank == b.bank 
                   && a.effect == b.effect
                   && a.frame == b.frame
                   && a.parent == b.parent
                   && a.pos == b.pos
                   && a.rot == b.rot
                   && a.scale == b.scale;
        }

        public static bool operator !=(rwby.FighterEffectNode a, rwby.FighterEffectNode b)
        {
            return !(a == b);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(bank, effect, frame, parent, pos, rot, scale);
        }
    }
}