using System;
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
        /// Creates the query used to reach the managed-MonoBehaviour-update singleton buffer. Call this once
        /// from a system's <c>OnCreate</c> and cache the result, then pass it to
        /// <see cref="CompleteBeforeManagedMonoBehaviourUpdates(JobHandle,EntityQuery)"/> each update.
        /// Building the query up front (instead of during <c>OnUpdate</c>) avoids Entities' "creates a query
        /// during OnUpdate" performance warning.
        /// </summary>
        public static EntityQuery GetManagedMonoBehaviourUpdateQuery(ref SystemState state) {
            return state.GetEntityQuery(ComponentType.ReadWrite<ManagedMonoBehaviourUpdateData>());
        }

        /// <summary>
        /// Schedules this job to be completed just before the ManagedMonoBehaviours are updated.
        /// </summary>
        /// <param name="job">The job to complete before the managed MonoBehaviour update.</param>
        /// <param name="managedMonoBehaviourUpdateQuery">
        /// The query returned by <see cref="GetManagedMonoBehaviourUpdateQuery"/>, created and cached in the
        /// calling system's <c>OnCreate</c>.
        /// </param>
        public static void CompleteBeforeManagedMonoBehaviourUpdates(
            this JobHandle job,
            EntityQuery managedMonoBehaviourUpdateQuery
        ) {
            DynamicBuffer<ManagedMonoBehaviourUpdateData> buffer =
                managedMonoBehaviourUpdateQuery.GetSingletonBuffer<ManagedMonoBehaviourUpdateData>();

            buffer.Add(new ManagedMonoBehaviourUpdateData { Dependency = job });
        }

        /// <summary>
        /// Schedules this job to be completed just before the ManagedMonoBehaviours are updated.
        /// </summary>
        [Obsolete(
            "This overload builds an EntityQuery during OnUpdate, which triggers Entities' " +
            "'creates a query during OnUpdate' performance warning. Cache the query once in OnCreate via " +
            "GetManagedMonoBehaviourUpdateQuery and call the (JobHandle, EntityQuery) overload instead.",
            false)]
        public static void CompleteBeforeManagedMonoBehaviourUpdates(this JobHandle job, ref SystemState state) {
            job.CompleteBeforeManagedMonoBehaviourUpdates(GetManagedMonoBehaviourUpdateQuery(ref state));
        }
    }

    [InternalBufferCapacity(1)]
    public struct ManagedMonoBehaviourUpdateData : IBufferElementData {
        public JobHandle Dependency;
    }
}
