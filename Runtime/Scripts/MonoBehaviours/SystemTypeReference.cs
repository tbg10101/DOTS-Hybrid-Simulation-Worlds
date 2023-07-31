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
