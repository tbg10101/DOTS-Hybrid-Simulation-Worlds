using Software10101.DOTS.Example.Data;
using Software10101.DOTS.MonoBehaviours;
using Unity.Entities;
using Unity.Mathematics;

namespace Software10101.DOTS.Example.MonoBehaviours {
    public class GameBootstrapper : Bootstrapper {
        public ushort EntitiesToGenerate = 1;
        public float SpeedMultiplier = 1.0f;

        protected override void Start() {
            base.Start();

            Random r = new Random();
            r.InitState();

            for (ushort i = 0; i < EntitiesToGenerate; i++) {
                (Entity entity, EntityCommandBuffer ecb) = Create(0);
                ecb.SetComponent(entity, new VelocityComponentData {
                     Value = SpeedMultiplier * math.normalize(2.0f * (r.NextFloat3() - 0.5f)) * r.NextFloat()
                });
            }
        }
    }
}
