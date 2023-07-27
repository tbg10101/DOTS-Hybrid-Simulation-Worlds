using Software10101.DOTS.Example.Data;
using Software10101.DOTS.MonoBehaviours;
using Software10101.DOTS.Utils;
using Unity.Entities;
using UnityEngine;

namespace Software10101.DOTS.Example.Systems {
    [CreateAssetMenu(menuName = "Systems/Dummy/" + nameof(DummySystemB))]
    public class DummySystemBReference : SystemTypeReference<DummySystemB> { }

    public partial class DummySystemB : ReferenceCreatedSystemBase<DummySystemBReference> {
        protected override void OnUpdate() {
            float dt = TimeUtil.FixedDeltaTime;

            Entities
                .ForEach((ref PositionComponentData component) => {
                    component.NextValue += dt * 0.0f;
                })
                .ScheduleParallel();
        }
    }
}
