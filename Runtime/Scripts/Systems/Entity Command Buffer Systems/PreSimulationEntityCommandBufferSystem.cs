using Software10101.DOTS.Data;
using Software10101.DOTS.Systems.EntityCommandBufferSystems;
using Software10101.DOTS.Systems.Groups;
using Unity.Entities;

[assembly: RegisterGenericComponentType(typeof(EntityCommandBufferSystemSingleton<PreSimulationEntityCommandBufferSystem>))]

namespace Software10101.DOTS.Systems.EntityCommandBufferSystems {
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(SimulationResetSystemGroup))]
    public sealed partial class PreSimulationEntityCommandBufferSystem : EntityCommandBufferSystem {
        protected override void OnCreate()
        {
            base.OnCreate();
            this.RegisterSingleton<EntityCommandBufferSystemSingleton<PreSimulationEntityCommandBufferSystem>>(
                ref PendingBuffers,
                World.Unmanaged
            );
        }
    }
}
