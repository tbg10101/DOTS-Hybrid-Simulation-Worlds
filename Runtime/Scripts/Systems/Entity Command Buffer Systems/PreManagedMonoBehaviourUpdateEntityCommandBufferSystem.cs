using Software10101.DOTS.Data;
using Software10101.DOTS.Systems.EntityCommandBufferSystems;
using Software10101.DOTS.Systems.Groups;
using Unity.Entities;

[assembly: RegisterGenericComponentType(typeof(EntityCommandBufferSystemSingleton<PreManagedMonoBehaviourUpdateEntityCommandBufferSystem>))]

namespace Software10101.DOTS.Systems.EntityCommandBufferSystems {
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    [UpdateAfter(typeof(PresentationPreUpdateSystemGroup))]
    public sealed partial class PreManagedMonoBehaviourUpdateEntityCommandBufferSystem : EntityCommandBufferSystem {
        protected override void OnCreate()
        {
            base.OnCreate();
            this.RegisterSingleton<EntityCommandBufferSystemSingleton<PreManagedMonoBehaviourUpdateEntityCommandBufferSystem>>(
                ref PendingBuffers,
                World.Unmanaged
            );
        }
    }
}
