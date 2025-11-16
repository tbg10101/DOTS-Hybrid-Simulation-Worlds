using Software10101.DOTS.Data;
using Software10101.DOTS.Systems.EntityCommandBufferSystems;
using Unity.Entities;

[assembly: RegisterGenericComponentType(typeof(EntityCommandBufferSystemSingleton<PrePresentationEntityCommandBufferSystem>))]

namespace Software10101.DOTS.Systems.EntityCommandBufferSystems {
    [UpdateInGroup(typeof(PresentationSystemGroup), OrderFirst = true)]
    public sealed partial class PrePresentationEntityCommandBufferSystem : EntityCommandBufferSystem {
        protected override void OnCreate()
        {
            base.OnCreate();
            this.RegisterSingleton<EntityCommandBufferSystemSingleton<PrePresentationEntityCommandBufferSystem>>(
                ref PendingBuffers,
                World.Unmanaged
            );
        }
    }
}
