using Software10101.DOTS.Example.Data;
using Software10101.DOTS.MonoBehaviours;
using Software10101.DOTS.Utils;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Software10101.DOTS.Example.Systems {
    [CreateAssetMenu(menuName = "Systems/" + nameof(ISystemExampleSystemReference))]
    // ReSharper disable once InconsistentNaming
    public class ISystemExampleSystemReference : ISystemTypeReference<ISystemExample> {
        [SerializeField]
        private Vector3 _target = Vector3.zero;

        [SerializeField]
        private bool _generateTarget = true;

        public override void SetConfig(EntityManager entityManager, SystemHandle systemHandle) {
            entityManager.AddComponentData(systemHandle, new ISystemExampleConfigData {
                Target = _target,
                GenerateTarget = _generateTarget,
            });
        }
    }

    // ReSharper disable once InconsistentNaming
    public struct ISystemExampleConfigData : IComponentData {
        public Vector3 Target;
        public bool GenerateTarget;
    }

    // ReSharper disable once InconsistentNaming
    public partial struct ISystemExample : ISystem {
        public void OnUpdate(ref SystemState state) {
            ISystemExampleConfigData config = state.EntityManager.GetComponentData<ISystemExampleConfigData>(state.SystemHandle);

            float3 target = config.GenerateTarget
                ? 10.0f * new float3(math.sin(TimeUtil.NextFixedTime), 0.0f, math.cos(TimeUtil.NextFixedTime))
                : config.Target;

            new ExampleJob {
                Target = target,
            }.ScheduleParallel();
        }
    }

    [BurstCompile]
    public partial struct ExampleJob : IJobEntity {
        private static readonly float3 Up = new(0.0f, 1.0f, 0.0f);

        public float3 Target;

        public void Execute(ref RotationComponentData rotationData, in PositionComponentData positionData) {
            rotationData.PreviousValue = rotationData.NextValue;

            quaternion newRotation = quaternion.LookRotation(positionData.NextValue - Target, Up);

            if (float.IsNaN(newRotation.value.w)) {
                newRotation = quaternion.identity;
            }

            rotationData.NextValue = newRotation;
        }
    }
}
