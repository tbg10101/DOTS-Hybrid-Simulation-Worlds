using Unity.Entities;

namespace Software10101.DOTS.Systems.EntityCommandBufferSystems {
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    [UpdateAfter(typeof(ManagedMonoBehaviourUpdateSystem))]
    public sealed partial class PostManagedMonoBehaviourUpdateEntityCommandBufferSystem : EntityCommandBufferSystem { }
}
