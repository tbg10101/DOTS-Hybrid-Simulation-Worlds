using Software10101.DOTS.Data;
using Software10101.DOTS.MonoBehaviours;
using Unity.Entities;

namespace Software10101.DOTS.Systems {
    internal sealed class PresentationDestroySystem : SystemBase {
        protected override void OnUpdate() {
            Entities
                .WithoutBurst()
                .WithStructuralChanges()
                .WithAll<DestroyFlagComponentData, GameObjectFlagComponentData>()
                .ForEach((Entity entity) => {
                    // TODO use a pool instead
                    EntityMonoBehaviour.Get(entity).Destroy();

                    EntityManager.RemoveComponent<GameObjectFlagComponentData>(entity);
                })
                .Run(); // must be on the main thread
        }
    }
}
