using Software10101.DOTS.Example.Data;
using Software10101.DOTS.MonoBehaviours;
using Unity.Entities;
using UnityEngine;

namespace Software10101.DOTS.Example.MonoBehaviours {
    public class TestEntityBehaviour : EntityMonoBehaviour {
        private static uint _idCounter = 0u;

        public bool DestroyMe = false;

        protected override void Awake() {
            base.Awake();

            name = $"Test {_idCounter}";
            _idCounter++;
        }

        protected override void OnUpdate() {
            base.OnUpdate();

            // Profiler.BeginSample("Apply Position");
            transform.localPosition = EntityManager.GetComponentData<PositionComponentData>(Entity).PresentationValue;
            // Profiler.EndSample();

            if (DestroyMe) {
                if (Entity == Entity.Null) {
                    Debug.LogWarning($"{name} has a null entity when trying to be destroyed!");
                }

                WorldBehaviour.Destroy(Entity);
            }
        }
    }
}
