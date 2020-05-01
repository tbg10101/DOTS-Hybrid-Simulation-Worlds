using Software10101.DOTS.Data;
using Software10101.DOTS.MonoBehaviours;
using Software10101.DOTS.Systems.EntityCommandBufferSystems;
using Unity.Entities;
using UnityEngine;

namespace Software10101.DOTS.Systems {
    [DisableAutoCreation]
    internal sealed class PrefabSpawnSystem : SystemBase {
        private readonly Bootstrapper _bootstrapper;

        public PrefabSpawnSystem(Bootstrapper bootstrapper) {
            _bootstrapper = bootstrapper;
        }

        protected override void OnUpdate() {
            EntityCommandBuffer ecb = World.GetExistingSystem<PreSimulationEntityCommandBufferSystem>().CreateCommandBuffer();

            Entities
                .WithoutBurst()
                .ForEach((Entity entity, in InitComponentData component) => {
                    // TODO use a pool instead
                    EntityMonoBehaviour instance = Object.Instantiate(_bootstrapper.GetPrefab(component.PrefabIndex));

                    instance.Entity = entity;
                    instance.Bootstrapper = _bootstrapper;

#if UNITY_EDITOR && ENTITY_NAME_SYNC
                    EntityManager.SetName(entity, instance.name);
#endif

                    instance.PostInstantiate();

                    ecb.RemoveComponent<InitComponentData>(entity);
                    ecb.AddComponent(entity, new GameObjectFlagComponentData());
                })
                .Run(); // must be on the main thread
        }
    }
}
