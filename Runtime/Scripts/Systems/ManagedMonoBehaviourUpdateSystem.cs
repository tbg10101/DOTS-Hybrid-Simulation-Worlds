using System.Collections.Generic;
using Software10101.DOTS.MonoBehaviours;
using Software10101.DOTS.Systems.EntityCommandBufferSystems;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace Software10101.DOTS.Systems {
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    [UpdateAfter(typeof(PreManagedMonoBehaviourUpdateEntityCommandBufferSystem))]
    internal partial class ManagedMonoBehaviourUpdateSystem : SystemBase {
        protected override void OnUpdate() {
            JobHandleExtensions.CompleteJobList();
            ManagedMonoBehaviour.DoUpdate();
        }
    }

    public static class JobHandleExtensions {
        private static readonly List<JobHandle> InternalJobList = new List<JobHandle>();

        /// <summary>
        /// Schedules this job to be completed just before the ManagedMonoBehaviours are updated.
        /// </summary>
        /// <param name="job">The job handle for the job to be completed.</param>
        public static void CompleteBeforeManagedMonoBehaviourUpdates(this JobHandle job) {
            InternalJobList.Add(job);
        }

        internal static void CompleteJobList() {
            int length = InternalJobList.Count;

            NativeArray<JobHandle> jobsArray = new NativeArray<JobHandle>(length, Allocator.Temp);

            for (int i = 0; i < length; i++) {
                jobsArray[i] = InternalJobList[i];
            }

            InternalJobList.Clear();

            JobHandle.CompleteAll(jobsArray);

            jobsArray.Dispose();
        }
    }
}
