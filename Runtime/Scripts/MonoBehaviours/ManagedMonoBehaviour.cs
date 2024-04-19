using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

namespace Software10101.DOTS.MonoBehaviours {
    public abstract class ManagedMonoBehaviour : MonoBehaviour {
        private static readonly List<ManagedMonoBehaviour> Instances = new List<ManagedMonoBehaviour>();

        private bool _destroyed = false;

        protected virtual void OnEnable() {
            if (_destroyed) {
                return;
            }

            Instances.Add(this);
        }

        protected virtual void OnDisable() {
            if (_destroyed) {
                return;
            }

            Instances.Remove(this);
        }

        protected abstract void OnUpdate();

        internal static void DoUpdate() {
            Profiler.BeginSample("ManagedMonoBehaviour.OnUpdate()");

            int i;
            int count = Instances.Count;

            for (i = 0; i < count; i++) {
                ManagedMonoBehaviour instance = Instances[i];

                if (instance._destroyed) {
                    Debug.LogWarning($"{instance.name} was destroyed but is still on this list of objects to update!");
                    continue;
                }

                // Profiler.BeginSample($"ManagedMonoBehaviour.OnUpdate() - {instance.name}");
                instance.OnUpdate();
                // Profiler.EndSample();
            }

            Profiler.EndSample();
        }

        internal virtual void Destroy() {
            _destroyed = true;
            Instances.Remove(this);
            Destroy(gameObject);
        }

        internal static void DestroyAll() {
            while (Instances.Count > 0) {
                Instances[Instances.Count - 1].Destroy();
            }
        }
    }
}
