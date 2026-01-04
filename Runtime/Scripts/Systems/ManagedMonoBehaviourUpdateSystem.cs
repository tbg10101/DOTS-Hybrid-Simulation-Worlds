using Software10101.DOTS.MonoBehaviours;
using Software10101.DOTS.Systems.EntityCommandBufferSystems;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace Software10101.DOTS.Systems {
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    [UpdateAfter(typeof(PreManagedMonoBehaviourUpdateEntityCommandBufferSystem))]
    internal partial struct ManagedMonoBehaviourUpdateSystem : ISystem {
        private Entity _managedMonoBehaviourUpdateSingletonEntity;

        public void OnCreate(ref SystemState state) {
            _managedMonoBehaviourUpdateSingletonEntity = state.EntityManager
                .CreateSingletonBuffer<ManagedMonoBehaviourUpdateData>("Dependencies To Complete Before MonoBehaviors Update");
        }

        public void OnUpdate(ref SystemState state) {
            DynamicBuffer<ManagedMonoBehaviourUpdateData> buffer =
                state.EntityManager.GetBuffer<ManagedMonoBehaviourUpdateData>(_managedMonoBehaviourUpdateSingletonEntity);

            NativeList<JobHandle> dependenciesList = new(buffer.Length, Allocator.Temp);

            for (int i = 0; i < buffer.Length; i++) {
                dependenciesList.Add(buffer[i].Dependency);
            }

            NativeArray<JobHandle> jobsArray = dependenciesList.ToArray(Allocator.Temp);

            dependenciesList.Dispose();

            buffer.Clear();

            JobHandle.CompleteAll(jobsArray);

            jobsArray.Dispose();

            ManagedMonoBehaviour.DoUpdate();
        }

        public void OnDestroy(ref SystemState state) {
            state.EntityManager.DestroyEntity(_managedMonoBehaviourUpdateSingletonEntity);
            _managedMonoBehaviourUpdateSingletonEntity = Entity.Null;
        }
    }

    public static class JobHandleExtensions {
        /// <summary>
        /// Schedules this job to be completed just before the ManagedMonoBehaviours are updated.
        /// </summary>
        public static void CompleteBeforeManagedMonoBehaviourUpdates(this JobHandle job, ref SystemState state) {
            DynamicBuffer<ManagedMonoBehaviourUpdateData> buffer = state
                .GetEntityQuery(ComponentType.ReadWrite<ManagedMonoBehaviourUpdateData>())
                .GetSingletonBuffer<ManagedMonoBehaviourUpdateData>();

            buffer.Add(new ManagedMonoBehaviourUpdateData { Dependency = job});
        }
    }

    [InternalBufferCapacity(1)]
    public struct ManagedMonoBehaviourUpdateData : IBufferElementData {
        public JobHandle Dependency;
    }
}
