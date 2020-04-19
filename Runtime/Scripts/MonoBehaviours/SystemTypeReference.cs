using System;
using Unity.Entities;
using UnityEngine;

namespace Software10101.DOTS.MonoBehaviours {
    public abstract class SystemTypeReference : ScriptableObject {
        public virtual Type SystemType => null;
    }

    public abstract class SystemTypeReference<T> : SystemTypeReference where T : SystemBase {
        public override Type SystemType => typeof(T);
    }
}
