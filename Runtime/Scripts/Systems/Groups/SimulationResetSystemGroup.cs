using Software10101.DOTS.Systems.Groups.Abstract;
using Unity.Entities;

namespace Software10101.DOTS.Systems.Groups {
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(SimulationDestroySystem))]
    internal sealed partial class SimulationResetSystemGroup : GraphSystemGroup { }
}
