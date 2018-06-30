using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace UChainDB.Example.BlockChain.Chain
{
    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + ", nq}")]
    public abstract class HashBase : IHashObject
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private UInt256 hash;
        private bool dirtyHash = true;

        public UInt256 Hash
        {
            get
            {
                if (this.dirtyHash)
                {
                    this.hash = new Hash(this.HashContent);
                    this.dirtyHash = false;
                }

                return this.hash;
            }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        protected internal abstract string HashContent { get; }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        protected virtual string DebuggerDisplay => $"{this.Hash.ToShort()}";

        public static bool operator ==(HashBase a, HashBase b)
        {
            if (ReferenceEquals(a, null))
            {
                return ReferenceEquals(b, null);
            }

            return a.Equals(b);
        }

        public static bool operator !=(HashBase a, HashBase b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            var item = obj as HashBase;

            if (item == null)
            {
                return false;
            }

            return this.Hash.Equals(item.Hash);
        }

        public override int GetHashCode()
        {
            return this.Hash.GetHashCode();
        }

        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            this.dirtyHash = true;
        }

        protected void SetPropertyField<T>(ref T field, T newValue, [CallerMemberName]string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(field, newValue)) return;
            field = newValue;
            this.OnPropertyChanged(propertyName);
        }
    }
}