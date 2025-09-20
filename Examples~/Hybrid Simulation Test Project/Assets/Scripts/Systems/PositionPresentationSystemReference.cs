using Software10101.DOTS.Example.Data;
using Software10101.DOTS.MonoBehaviours;
using Software10101.DOTS.Systems;
using Software10101.DOTS.Utils;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Software10101.DOTS.Example.Systems {
    [CreateAssetMenu(menuName = "Systems/" + nameof(PositionPresentationSystem))]
    public class PositionPresentationSystemReference : SystemTypeReference<PositionPresentationSystem> { }

    public partial class PositionPresentationSystem : ReferenceCreatedSystemBase<PositionPresentationSystemReference> {
        protected override void OnUpdate() {
            float presentationFraction = TimeUtil.PresentationTimeFraction; // this is done just once instead of once per instance

            Entities
                .ForEach((ref PositionComponentData positionData, ref RotationComponentData rotationData) => {
                    positionData.PresentationValue = math.lerp(positionData.PreviousValue, positionData.NextValue, presentationFraction);
                    rotationData.PresentationValue = math.slerp(rotationData.PreviousValue, rotationData.NextValue, presentationFraction);
                })
                .ScheduleParallel();

            Dependency.CompleteBeforeManagedMonoBehaviourUpdates();
        }
    }
}
