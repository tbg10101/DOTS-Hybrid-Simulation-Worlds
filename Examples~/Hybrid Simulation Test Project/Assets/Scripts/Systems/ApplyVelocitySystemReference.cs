using Software10101.DOTS.Example.Data;
using Software10101.DOTS.MonoBehaviours;
using Software10101.DOTS.Utils;
using Unity.Burst;
using Unity.Entities;
using UnityEngine;

namespace Software10101.DOTS.Example.Systems {
    [CreateAssetMenu(menuName = "Systems/" + nameof(ApplyVelocitySystem))]
    public class ApplyVelocitySystemReference : ISystemTypeReference<ApplyVelocitySystem> {
        [Min(0.0f)]
        [SerializeField]
        private float _multiplier = 1.0f;
        internal float Multiplier => _multiplier;

        public override void SetConfig(EntityManager entityManager, SystemHandle systemHandle) {
            Debug.Log($"Velocity multiplier: {_multiplier}");

            entityManager.AddComponentData(systemHandle, new ApplyVelocitySystemConfigData {
                Multiplier = _multiplier,
            });
        }
    }

    [BurstCompile]
    public partial struct ApplyVelocitySystem : ISystem {
        [BurstCompile]
        public void OnUpdate(ref SystemState state) {
            ApplyVelocitySystemConfigData config =
                state.EntityManager.GetComponentData<ApplyVelocitySystemConfigData>(state.SystemHandle);

            new ApplyVelocityJob {
                DeltaTime = TimeUtil.FixedDeltaTime,
                Multiplier = config.Multiplier
            }.ScheduleParallel();
        }
    }

    [BurstCompile]
    public partial struct ApplyVelocityJob : IJobEntity {
        public float DeltaTime;
        public float Multiplier;

        [BurstCompile]
        private void Execute(ref PositionComponentData component, in VelocityComponentData rate) {
            component.PreviousValue = component.NextValue;
            component.NextValue += DeltaTime * rate.Value * Multiplier;
        }
    }

    public struct ApplyVelocitySystemConfigData : IComponentData {
        public float Multiplier;
    }
}
