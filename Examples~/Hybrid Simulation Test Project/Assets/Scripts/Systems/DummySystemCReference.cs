using Software10101.DOTS.Example.Data;
using Software10101.DOTS.MonoBehaviours;
using Software10101.DOTS.Utils;
using Unity.Burst;
using Unity.Entities;
using UnityEngine;

namespace Software10101.DOTS.Example.Systems {
    [CreateAssetMenu(menuName = "Systems/Dummy/" + nameof(DummySystemC))]
    public class DummySystemCReference : SystemTypeReference<DummySystemC> { }

    public partial class DummySystemC : ReferenceCreatedSystemBase<DummySystemCReference> {
        protected override void OnUpdate() {
            new DummyJobC {
                DeltaTime = TimeUtil.FixedDeltaTime
            }.ScheduleParallel();
        }
    }

    [BurstCompile]
    public partial struct DummyJobC : IJobEntity {
        public float DeltaTime;
        private void Execute(ref PositionComponentData component) {
            component.NextValue += DeltaTime * 0.0f;
        }
    }
}
