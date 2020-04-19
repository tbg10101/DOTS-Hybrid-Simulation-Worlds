using Software10101.DOTS.Example.Data;
using Software10101.DOTS.MonoBehaviours;
using Software10101.DOTS.Utils;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Software10101.DOTS.Example.Systems {
    [CreateAssetMenu(menuName = "Systems/" + nameof(PositionPresentationSystem))]
    public class PositionPresentationSystemReference : SystemTypeReference<PositionPresentationSystem> { }

    public class PositionPresentationSystem : SystemBase {
        protected override void OnUpdate() {
            float presentationFraction = TimeUtil.PresentationTimeFraction; // this is done just once instead of once per instance

            Entities
                .ForEach((ref PositionComponentData component) => {
                    component.PresentationValue = math.lerp(component.PreviousValue, component.NextValue, presentationFraction);
                })
                .ScheduleParallel();
        }
    }
}
