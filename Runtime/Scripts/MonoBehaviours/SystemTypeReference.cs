using System;
using Unity.Entities;
using UnityEngine;

namespace Software10101.DOTS.MonoBehaviours {
    public abstract class SystemTypeReference : ScriptableObject {
        public abstract Type SystemType {
            get;
        }
    }

    public abstract class SystemTypeReference<T> : SystemTypeReference where T : ReferenceCreatedSystemBase {
        public override Type SystemType => typeof(T);
    }

    // ReSharper disable once PartialTypeWithSinglePart
    public abstract partial class ReferenceCreatedSystemBase : SystemBase {
        internal abstract void SetCreator(SystemTypeReference creator);
    }

    // ReSharper disable once PartialTypeWithSinglePart
    public abstract partial class ReferenceCreatedSystemBase<T> : ReferenceCreatedSystemBase where T : SystemTypeReference {
        public T Creator {
            internal set;
            get;
        }

        internal sealed override void SetCreator(SystemTypeReference creator) {
            Creator = (T) creator;
        }
    }
}
