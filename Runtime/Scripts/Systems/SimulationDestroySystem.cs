using Software10101.DOTS.Data;
using Unity.Entities;

namespace Software10101.DOTS.Systems {
    [UpdateInGroup(typeof(SimulationSystemGroup), OrderFirst = true)]
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
