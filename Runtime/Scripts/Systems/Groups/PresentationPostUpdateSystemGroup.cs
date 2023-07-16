using Software10101.DOTS.Systems.EntityCommandBufferSystems;
using Software10101.DOTS.Systems.Groups.Abstract;
using Unity.Entities;

namespace Software10101.DOTS.Systems.Groups {
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    [UpdateAfter(typeof(PostManagedMonoBehaviourUpdateEntityCommandBufferSystem))]
    internal sealed partial class PresentationPostUpdateSystemGroup : GraphSystemGroup { }
}
