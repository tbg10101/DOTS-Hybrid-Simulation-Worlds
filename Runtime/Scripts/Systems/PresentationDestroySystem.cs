using Software10101.DOTS.Data;
using Software10101.DOTS.MonoBehaviours;
using Software10101.DOTS.Systems.EntityCommandBufferSystems;
using Unity.Collections;
using Unity.Entities;

namespace Software10101.DOTS.Systems {
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    [UpdateAfter(typeof(PostPresentationEntityCommandBufferSystem))]
    internal partial class PresentationDestroySystem : SystemBase {
        protected override void OnUpdate() {
            EntityQuery query = GetEntityQuery(typeof(DestroyFlagComponentData), typeof(GameObjectFlagComponentData));
            using NativeArray<Entity> entities = query.ToEntityArray(Allocator.Temp);

            foreach (Entity entity in entities) {
                EntityMonoBehaviour.Get(entity).Destroy();
                EntityManager.RemoveComponent<GameObjectFlagComponentData>(entity);
            }
        }
    }
}
