using Unity.Entities;

namespace Software10101.DOTS.Systems.EntityCommandBufferSystems {
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public sealed partial class PostStartOfFrameEntityCommandBufferSystem : EntityCommandBufferSystem { }
}
