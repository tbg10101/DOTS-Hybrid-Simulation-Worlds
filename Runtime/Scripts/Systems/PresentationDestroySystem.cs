using Software10101.DOTS.Data;
using Software10101.DOTS.MonoBehaviours;
using Software10101.DOTS.Systems.EntityCommandBufferSystems;
using Unity.Entities;

namespace Software10101.DOTS.Systems {
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    [UpdateAfter(typeof(PostPresentationEntityCommandBufferSystem))]
    internal partial class PresentationDestroySystem : SystemBase {
        protected override void OnUpdate() {
            Entities
                .WithoutBurst()
                .WithStructuralChanges()
                .WithAll<DestroyFlagComponentData, GameObjectFlagComponentData>()
                .ForEach((Entity entity) => {
                    EntityMonoBehaviour.Get(entity).Destroy();
                    EntityManager.RemoveComponent<GameObjectFlagComponentData>(entity);
                })
                .Run(); // must be on the main thread
        }
    }
}
