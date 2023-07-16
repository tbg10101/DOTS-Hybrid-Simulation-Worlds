using Unity.Entities;

namespace Software10101.DOTS.Systems.EntityCommandBufferSystems {
    [UpdateInGroup(typeof(PresentationSystemGroup), OrderFirst = true)]
    public sealed partial class PrePresentationEntityCommandBufferSystem : EntityCommandBufferSystem { }
}
