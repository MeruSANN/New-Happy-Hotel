using System;

namespace HappyHotel.Core.Registry
{
    // 所有类型ID的抽象基类
    public abstract class TypeId : IEquatable<TypeId>
    {
        protected TypeId()
        {
        }

        protected TypeId(string id)
        {
            this.Id = id;
        }

        public string Id { get; private set; }

        public bool Equals(TypeId other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return Id.Equals(other.Id, StringComparison.Ordinal);
        }

        public static T Create<T>(string typeID) where T : TypeId, new()
        {
            var typeId = new T
            {
                Id = typeID
            };
            return typeId;
        }

        public override bool Equals(object obj)
        {
            if (obj is TypeId other) return Id == other.Id;
            return false;
        }

        public override int GetHashCode()
        {
            return Id?.GetHashCode() ?? 0;
        }

        public override string ToString()
        {
            return Id;
        }
    }
}