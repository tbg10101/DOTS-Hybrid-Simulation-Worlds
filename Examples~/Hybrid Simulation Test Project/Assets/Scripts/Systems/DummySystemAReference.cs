using Software10101.DOTS.Example.Data;
using Software10101.DOTS.MonoBehaviours;
using Software10101.DOTS.Utils;
using Unity.Entities;
using UnityEngine;

namespace Software10101.DOTS.Example.Systems {
    [CreateAssetMenu(menuName = "Systems/Dummy/" + nameof(DummySystemA))]
    public class DummySystemAReference : SystemTypeReference<DummySystemA> { }

    public partial class DummySystemA : ReferenceCreatedSystemBase<DummySystemAReference> {
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
