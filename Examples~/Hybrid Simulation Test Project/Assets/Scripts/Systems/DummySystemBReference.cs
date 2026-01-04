using Software10101.DOTS.Example.Data;
using Software10101.DOTS.MonoBehaviours;
using Software10101.DOTS.Utils;
using Unity.Burst;
using Unity.Entities;
using UnityEngine;

namespace Software10101.DOTS.Example.Systems {
    [CreateAssetMenu(menuName = "Systems/Dummy/" + nameof(DummySystemB))]
    public class DummySystemBReference : SystemTypeReference<DummySystemB> { }

    public partial class DummySystemB : ReferenceCreatedSystemBase<DummySystemBReference> {
        protected override void OnUpdate() {
            new DummyJobB {
                DeltaTime = TimeUtil.FixedDeltaTime
            }.ScheduleParallel();
        }
    }

    [BurstCompile]
    public partial struct DummyJobB : IJobEntity {
        public float DeltaTime;
        private void Execute(ref PositionComponentData component) {
            component.NextValue += DeltaTime * 0.0f;
        }
    }
}
