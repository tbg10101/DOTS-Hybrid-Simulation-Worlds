using Software10101.DOTS.MonoBehaviours;
using Unity.Entities;

namespace Software10101.DOTS.Systems {
    internal sealed class ManagedMonoBehaviourUpdateSystem : SystemBase {
        protected override void OnUpdate() {
            Dependency.Complete();
            ManagedMonoBehaviour.DoUpdate();
        }
    }
}
