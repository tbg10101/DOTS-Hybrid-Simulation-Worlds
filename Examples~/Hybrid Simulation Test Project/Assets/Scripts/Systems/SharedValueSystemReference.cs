using Software10101.DOTS.Example.Data;
using Software10101.DOTS.MonoBehaviours;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace Software10101.DOTS.Example.Systems {
    [CreateAssetMenu(menuName = "Systems/" + nameof(SharedValueSystem))]
    public class SharedValueSystemReference : ISystemTypeReference<SharedValueSystem> { }

    public partial struct SharedValueSystem : ISystem {
        public void OnUpdate(ref SystemState state) {
            NativeReference<int> nativeReference = new(0, Allocator.TempJob);

            // do something to populate the value
            state.Dependency = new CountPositionsJob {
                NativeReference = nativeReference
            }.Schedule(state.Dependency);

            // read the value in another job - the dependency is already managed by the SystemBase's implicit JobHandle
            state.Dependency = new LogCountJob {
                NativeReference = nativeReference
            }.Schedule(state.Dependency);

            nativeReference.Dispose(state.Dependency);
        }

        [BurstCompile]
        private partial struct CountPositionsJob : IJobEntity {
            public NativeReference<int> NativeReference;

            private void Execute(in PositionComponentData _) {
                NativeReference.Value++;
            }
        }

        [BurstCompile]
        private struct LogCountJob : IJob {
            [ReadOnly] public NativeReference<int> NativeReference;

            public void Execute() {
                Debug.Log($"Position Count: {NativeReference.Value}");
            }
        }
    }
}
