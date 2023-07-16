using Software10101.DOTS.Systems.Groups;
using Unity.Entities;

namespace Software10101.DOTS.Systems.EntityCommandBufferSystems {
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    [UpdateAfter(typeof(PresentationPreUpdateSystemGroup))]
    public sealed partial class PreManagedMonoBehaviourUpdateEntityCommandBufferSystem : EntityCommandBufferSystem { }
}
