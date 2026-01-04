using Software10101.DOTS.Example.Data;
using Software10101.DOTS.MonoBehaviours;
using Software10101.DOTS.Systems;
using Software10101.DOTS.Utils;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Software10101.DOTS.Example.Systems {
    [CreateAssetMenu(menuName = "Systems/" + nameof(PositionPresentationSystem))]
    public class PositionPresentationSystemReference : ISystemTypeReference<PositionPresentationSystem> { }

    [BurstCompile]
    public partial struct PositionPresentationSystem : ISystem {
        [BurstCompile]
        public void OnUpdate(ref SystemState state) {
            new PositionPresentationJob {
                PresentationFraction = TimeUtil.PresentationTimeFraction
            }.ScheduleParallel();

            state.Dependency.CompleteBeforeManagedMonoBehaviourUpdates(ref state);
        }
    }

    [BurstCompile]
    public partial struct PositionPresentationJob : IJobEntity {
        public float PresentationFraction;

        [BurstCompile]
        private void Execute(ref PositionComponentData positionData, ref RotationComponentData rotationData) {
            positionData.PresentationValue = math.lerp(positionData.PreviousValue, positionData.NextValue, PresentationFraction);
            rotationData.PresentationValue = math.slerp(rotationData.PreviousValue, rotationData.NextValue, PresentationFraction);
        }
    }
}
