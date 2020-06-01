using Software10101.DOTS.Example.Data;
using Software10101.DOTS.MonoBehaviours;
using Unity.Entities;
using UnityEngine;

namespace Software10101.DOTS.Example.MonoBehaviours {
    public class TestEntityBehaviour : EntityMonoBehaviour {
        public bool DestroyMe = false;

        protected override void OnUpdate() {
            base.OnUpdate();

            transform.localPosition = EntityManager.GetComponentData<PositionComponentData>(Entity).PresentationValue;

            if (DestroyMe) {
                if (Entity == Entity.Null) {
                    Debug.LogWarning($"{name} has a null entity when trying to be destroyed!");
                }

                Bootstrapper.Destroy(Entity);
            }
        }
    }
}
