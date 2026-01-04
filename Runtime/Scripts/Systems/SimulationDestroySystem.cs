using Software10101.DOTS.Data;
using Software10101.DOTS.Systems.EntityCommandBufferSystems;
using Unity.Burst;
using Unity.Entities;

namespace Software10101.DOTS.Systems {
    [UpdateInGroup(typeof(SimulationSystemGroup), OrderFirst = true)]
    [BurstCompile]
    internal partial struct SimulationDestroySystem : ISystem {
        [BurstCompile]
        public void OnCreate(ref SystemState state) {
            state.RequireForUpdate<EntityCommandBufferSystemSingleton<PostSimulationEntityCommandBufferSystem>>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state) {
            EntityCommandBuffer.ParallelWriter ecb = SystemAPI
                .GetSingleton<EntityCommandBufferSystemSingleton<PostSimulationEntityCommandBufferSystem>>()
                .CreateCommandBuffer(state.WorldUnmanaged)
                .AsParallelWriter();

            new SimulationDestroyJob {
                Ecb = ecb,
            }.ScheduleParallel();
        }
    }

    [BurstCompile]
    public partial struct SimulationDestroyJob : IJobEntity {
        public EntityCommandBuffer.ParallelWriter Ecb;

        [BurstCompile]
        private void Execute([ChunkIndexInQuery] int sortKey, Entity entity, in DestroyFlagComponentData destroyFlag) {
            Ecb.DestroyEntity(sortKey, entity);
        }
    }
}
