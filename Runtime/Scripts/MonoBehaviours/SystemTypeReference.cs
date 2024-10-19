using System;
using Unity.Entities;
using UnityEngine;

namespace Software10101.DOTS.MonoBehaviours {
    public abstract class SystemTypeReference : ScriptableObject, IEquatable<SystemTypeReference> {
        public abstract Type SystemType {
            get;
        }

        public bool Equals(SystemTypeReference other) {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return base.Equals(other) && SystemType == other.SystemType;
        }

        public override bool Equals(object obj) {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((SystemTypeReference)obj);
        }

        public override int GetHashCode() {
            unchecked {
                return (base.GetHashCode() * 397) ^ (SystemType != null ? SystemType.GetHashCode() : 0);
            }
        }

        public static bool operator ==(SystemTypeReference left, SystemTypeReference right) {
            return Equals(left, right);
        }

        public static bool operator !=(SystemTypeReference left, SystemTypeReference right) {
            return !Equals(left, right);
        }
    }

    public abstract class SystemTypeReference<T> : SystemTypeReference where T : ReferenceCreatedSystemBase {
        public override Type SystemType => typeof(T);
    }

    public abstract partial class ReferenceCreatedSystemBase : SystemBase {
        internal abstract void SetCreator(SystemTypeReference creator);

        /// <summary>
        /// Invoked after the Creator field is populated. This will happen after OnCreate.
        /// </summary>
        protected virtual void OnCreatorSet() {
            // do nothing by default
        }
    }

    public abstract partial class ReferenceCreatedSystemBase<T> : ReferenceCreatedSystemBase where T : SystemTypeReference {
        protected T Creator {
            private set;
            get;
        }

        internal sealed override void SetCreator(SystemTypeReference creator) {
            Creator = (T) creator;
            OnCreatorSet();
        }
    }
}
