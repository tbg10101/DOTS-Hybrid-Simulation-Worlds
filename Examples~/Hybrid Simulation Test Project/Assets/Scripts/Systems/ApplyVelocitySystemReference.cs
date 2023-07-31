using Software10101.DOTS.Example.Data;
using Software10101.DOTS.MonoBehaviours;
using Software10101.DOTS.Utils;
using Unity.Entities;
using UnityEngine;

namespace Software10101.DOTS.Example.Systems {
    [CreateAssetMenu(menuName = "Systems/" + nameof(ApplyVelocitySystem))]
    public class ApplyVelocitySystemReference : SystemTypeReference<ApplyVelocitySystem> {
        [SerializeField]
        private float _multiplier = 1.0f;
        internal float Multiplier => _multiplier;
    }

    public partial class ApplyVelocitySystem : ReferenceCreatedSystemBase<ApplyVelocitySystemReference> {
        protected override void OnCreatorSet() {
            base.OnCreatorSet();

            Debug.Log($"Velocity multiplier: {Creator.Multiplier}");
        }

        protected override void OnUpdate() {
            float dt = TimeUtil.FixedDeltaTime;
            float multiplier = Creator.Multiplier;

            Entities
                .ForEach((ref PositionComponentData component, in VelocityComponentData rate) => {
                    component.PreviousValue = component.NextValue;
                    component.NextValue += dt * rate.Value * multiplier;
                })
                .ScheduleParallel();
        }
    }
}
