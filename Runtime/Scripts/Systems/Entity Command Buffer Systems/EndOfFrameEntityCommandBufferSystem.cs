using Unity.Entities;

namespace Software10101.DOTS.Systems.EntityCommandBufferSystems {
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    [UpdateAfter(typeof(PrefabSpawnSystem))]
    internal sealed partial class EndOfFrameEntityCommandBufferSystem : EntityCommandBufferSystem { }
}
