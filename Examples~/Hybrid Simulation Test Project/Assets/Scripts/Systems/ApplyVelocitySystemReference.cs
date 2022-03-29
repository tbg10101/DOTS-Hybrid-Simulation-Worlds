using Software10101.DOTS.Example.Data;
using Software10101.DOTS.MonoBehaviours;
using Software10101.DOTS.Utils;
using Unity.Entities;
using UnityEngine;

namespace Software10101.DOTS.Example.Systems {
    [CreateAssetMenu(menuName = "Systems/" + nameof(ApplyVelocitySystem))]
    public class ApplyVelocitySystemReference : SystemTypeReference<ApplyVelocitySystem> { }

    // ReSharper disable once PartialTypeWithSinglePart // systems need to be partial after Entities 0.50
    public partial class ApplyVelocitySystem : SystemBase {
        protected override void OnUpdate() {
            float dt = TimeUtil.FixedDeltaTime;

            Entities
                .ForEach((ref PositionComponentData component, in VelocityComponentData rate) => {
                    component.PreviousValue = component.NextValue;
                    component.NextValue += dt * rate.Value;
                })
                .ScheduleParallel();
        }
    }
}
