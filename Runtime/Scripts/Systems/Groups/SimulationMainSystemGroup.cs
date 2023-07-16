using Software10101.DOTS.Systems.EntityCommandBufferSystems;
using Software10101.DOTS.Systems.Groups.Abstract;
using Unity.Entities;

namespace Software10101.DOTS.Systems.Groups {
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(PreSimulationEntityCommandBufferSystem))]
    internal sealed partial class SimulationMainSystemGroup : GraphSystemGroup { }
}
