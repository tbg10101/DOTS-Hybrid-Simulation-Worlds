using Software10101.DOTS.Data;
using Unity.Entities;

namespace Software10101.DOTS.Systems {
    // ReSharper disable once PartialTypeWithSinglePart // systems need to be partial after Entities 0.50
    internal partial class SimulationDestroySystem : SystemBase {
        protected override void OnUpdate() {
            Entities
                .WithAll<DestroyFlagComponentData>()
                .WithNone<GameObjectFlagComponentData>()
                .WithStructuralChanges()
                .ForEach((Entity entity) => {
                    EntityManager.DestroyEntity(entity);
                })
                .Run();
        }
    }
}
