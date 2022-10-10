using System;
using UnityEngine;

namespace rwby
{
    [System.Serializable]
    public struct ModObjectSetContentReference : IEquatable<ModObjectSetContentReference>
    {
        [SerializeField] public ContentGUID modGUID;
        [SerializeField] public ContentGUID contentGUID;

        public ModObjectSetContentReference(ContentGUID modGUID, ContentGUID contentGUID)
        {
            this.modGUID = modGUID;
            this.contentGUID = contentGUID;
        }

        public ModObjectSetContentReference(byte[] modGUID, byte[] contentGUID)
        {
            this.modGUID = new ContentGUID(modGUID);
            this.contentGUID = new ContentGUID(contentGUID);
        }

        public ModObjectSetContentReference(string modGUID, string contentGUID)
        {
            this.modGUID = ContentGUID.StringToContentGUID(modGUID);
            this.contentGUID = ContentGUID.StringToContentGUID(contentGUID);
        }
        
        public override string ToString()
        {
            return $"{modGUID.ToString()}:?:{contentGUID}";
        }

        public bool Equals(ModObjectSetContentReference other)
        {
            return modGUID.Equals(other.modGUID) && contentGUID.Equals(other.contentGUID);
        }

        public override bool Equals(object obj)
        {
            return obj is ModObjectSetContentReference other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(modGUID, contentGUID);
        }
        
        public static bool operator ==(ModObjectSetContentReference x, ModObjectSetContentReference y)
        {
            return x.Equals(y);
        }

        public static bool operator !=(ModObjectSetContentReference x, ModObjectSetContentReference y)
        {
            return !(x == y);
        }
        
        //public static implicit operator NetworkModObjectSetContentReference(ModObjectGUIDReference nmo) =>
        //    new NetworkModObjectGUIDReference(nmo.modGUID, nmo.contentType, nmo.contentGUID);
    }
}