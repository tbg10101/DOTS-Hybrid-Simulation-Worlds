using Software10101.DOTS.Data;
using Software10101.DOTS.MonoBehaviours;
using Software10101.DOTS.Systems.EntityCommandBufferSystems;
using Unity.Collections;
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
             EntityCommandBuffer ecb = SystemAPI
                 .GetSingleton<EntityCommandBufferSystemSingleton<EndOfFrameEntityCommandBufferSystem>>()
                 .CreateCommandBuffer(World.Unmanaged);

             using NativeArray<Entity> entities = GetEntityQuery(typeof(SpawnPrefabComponentData))
                 .ToEntityArray(Allocator.Temp);

             foreach (Entity entity in entities) {
                SpawnPrefabComponentData initData = EntityManager.GetComponentData<SpawnPrefabComponentData>(entity);
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
             }
        }
    }
}
