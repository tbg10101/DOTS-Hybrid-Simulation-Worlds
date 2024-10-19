using Software10101.DOTS.Systems.Groups;
using Unity.Entities;

namespace Software10101.DOTS.Systems.EntityCommandBufferSystems {
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(SimulationMainSystemGroup))]
    public sealed partial class PostStartOfFrameEntityCommandBufferSystem : EntityCommandBufferSystem { }
}
