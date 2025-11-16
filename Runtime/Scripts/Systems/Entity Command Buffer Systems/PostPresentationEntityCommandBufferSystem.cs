using Software10101.DOTS.Data;
using Software10101.DOTS.Systems.EntityCommandBufferSystems;
using Software10101.DOTS.Systems.Groups;
using Unity.Entities;

[assembly: RegisterGenericComponentType(typeof(EntityCommandBufferSystemSingleton<PostPresentationEntityCommandBufferSystem>))]

namespace Software10101.DOTS.Systems.EntityCommandBufferSystems {
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    [UpdateAfter(typeof(PresentationPostUpdateSystemGroup))]
    public sealed partial class PostPresentationEntityCommandBufferSystem : EntityCommandBufferSystem {
        protected override void OnCreate()
        {
            base.OnCreate();
            this.RegisterSingleton<EntityCommandBufferSystemSingleton<PostPresentationEntityCommandBufferSystem>>(
                ref PendingBuffers,
                World.Unmanaged
            );
        }
    }
}
