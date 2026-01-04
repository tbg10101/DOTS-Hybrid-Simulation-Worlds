using Software10101.DOTS.Example.Data;
using Software10101.DOTS.MonoBehaviours;
using Software10101.DOTS.Utils;
using Unity.Burst;
using Unity.Entities;
using UnityEngine;

namespace Software10101.DOTS.Example.Systems {
    [CreateAssetMenu(menuName = "Systems/Dummy/" + nameof(DummySystemA))]
    public class DummySystemAReference : SystemTypeReference<DummySystemA> { }

    public partial class DummySystemA : ReferenceCreatedSystemBase<DummySystemAReference> {
        protected override void OnUpdate() {
            new DummyJobA {
                DeltaTime = TimeUtil.FixedDeltaTime
            }.ScheduleParallel();
        }
    }

    [BurstCompile]
    public partial struct DummyJobA : IJobEntity {
        public float DeltaTime;
        private void Execute(ref PositionComponentData component) {
            component.NextValue += DeltaTime * 0.0f;
        }
    }
}
