using Software10101.DOTS.Example.Data;
using Software10101.DOTS.MonoBehaviours;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace Software10101.DOTS.Example.MonoBehaviours {
    public class GameBootstrapper : MonoBehaviour {
        [SerializeField]
        private WorldBehaviour _world;

        public ushort EntitiesToGenerate = 1;
        public float SpeedMultiplier = 1.0f;

        private void Start() {
            Random r = new();
            r.InitState();

            for (ushort i = 0; i < EntitiesToGenerate; i++) {
                (Entity entity, EntityCommandBuffer ecb) = _world.Create(0);
                ecb.SetComponent(entity, new VelocityComponentData {
                    Value = SpeedMultiplier * math.normalize(2.0f * (r.NextFloat3() - 0.5f)) * r.NextFloat(),
                });
                ecb.SetComponent(entity, new RotationComponentData {
                    PreviousValue = quaternion.identity,
                    PresentationValue = quaternion.identity,
                    NextValue = quaternion.identity,
                });
            }
        }
    }
}
