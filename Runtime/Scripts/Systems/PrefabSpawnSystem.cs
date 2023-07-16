using Software10101.DOTS.Data;
using Software10101.DOTS.MonoBehaviours;
using Software10101.DOTS.Systems.EntityCommandBufferSystems;
using Unity.Entities;
using UnityEngine;

namespace Software10101.DOTS.Systems {
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    [UpdateAfter(typeof(PresentationDestroySystem))]
    [DisableAutoCreation]
    internal partial class PrefabSpawnSystem : SystemBase {
        private readonly WorldBehaviour _worldBehaviour;

        public PrefabSpawnSystem(WorldBehaviour worldBehaviour) {
            _worldBehaviour = worldBehaviour;
        }

        protected override void OnUpdate() {
             EntityCommandBuffer ecb = World
                 .GetExistingSystemManaged<EndOfFrameEntityCommandBufferSystem>()
                 .CreateCommandBuffer();

             Entities
                 .WithoutBurst()
                 .ForEach((Entity entity, in SpawnPrefabComponentData initData) => {
                    EntityMonoBehaviour instance = Object.Instantiate(_worldBehaviour.GetPrefab(initData.PrefabIndex));

                    instance.Entity = entity;
                    instance.WorldBehaviour = _worldBehaviour;

#if UNITY_EDITOR && ENTITY_NAME_SYNC
                    EntityManager.SetName(entity, instance.name);
#endif

                    instance.OnPostInstantiate();

                    // doing these in an ECB makes it a ton faster
                    ecb.RemoveComponent<SpawnPrefabComponentData>(entity);
                    ecb.AddComponent(entity, new GameObjectFlagComponentData());
                 })
                 .Run(); // must be on the main thread
        }
    }
}
