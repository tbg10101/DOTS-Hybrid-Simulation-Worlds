using Software10101.DOTS.Example.Data;
using Software10101.DOTS.MonoBehaviours;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace Software10101.DOTS.Example.Systems {
    [CreateAssetMenu(menuName = "Systems/" + nameof(ShuffleEntitiesExampleReference))]
    public class ShuffleEntitiesExampleReference : ISystemTypeReference<ShuffleEntitiesExampleSystem> { }

    [BurstCompile]
    public partial struct ShuffleEntitiesExampleSystem : ISystem {
        private EntityQuery _query;

        [BurstCompile]
        public void OnCreate(ref SystemState state) {
            _query = state.GetEntityQuery(ComponentType.ReadOnly<PositionComponentData>());
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state) {
            NativeList<Entity> entitiesWithPosition = _query.ToEntityListAsync(
                state.WorldUpdateAllocator,
                state.Dependency,
                out JobHandle gatherHandle
            );

            state.Dependency = new ShuffleJob {
                EntitiesWithPosition = entitiesWithPosition,
                Random = new Random((uint)Time.frameCount),
            }.Schedule(gatherHandle);

            state.Dependency = new LogJob {
                EntitiesWithPosition = entitiesWithPosition,
            }.Schedule(state.Dependency);
        }
    }

    [BurstCompile]
    public struct ShuffleJob : IJob {
        public NativeList<Entity> EntitiesWithPosition;
        public Random Random;

        public void Execute() {
            // https://en.wikipedia.org/wiki/Fisher%E2%80%93Yates_shuffle#Modern_method
            for (int i = EntitiesWithPosition.Length - 1; i >= 1; i--) {
                int j = Random.NextInt(0, i + 1); // max is not inclusive
                (EntitiesWithPosition[i], EntitiesWithPosition[j]) = (EntitiesWithPosition[j], EntitiesWithPosition[i]);
            }
        }
    }

    public struct LogJob : IJob {
        [ReadOnly]
        public NativeList<Entity> EntitiesWithPosition;

        public void Execute() {
            if (EntitiesWithPosition.IsEmpty) {
                return;
            }

            int index = EntitiesWithPosition[0].Index;
            Debug.Log($"Initial shuffled entity index: {index}");
        }
    }
}
