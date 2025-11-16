using Software10101.DOTS.Data;
using Software10101.DOTS.Systems.EntityCommandBufferSystems;
using Unity.Entities;

[assembly: RegisterGenericComponentType(typeof(EntityCommandBufferSystemSingleton<PostStartOfFrameEntityCommandBufferSystem>))]

namespace Software10101.DOTS.Systems.EntityCommandBufferSystems {
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public sealed partial class PostStartOfFrameEntityCommandBufferSystem : EntityCommandBufferSystem {
        protected override void OnCreate()
        {
            base.OnCreate();
            this.RegisterSingleton<EntityCommandBufferSystemSingleton<PostStartOfFrameEntityCommandBufferSystem>>(
                ref PendingBuffers,
                World.Unmanaged
            );
        }
    }
}
