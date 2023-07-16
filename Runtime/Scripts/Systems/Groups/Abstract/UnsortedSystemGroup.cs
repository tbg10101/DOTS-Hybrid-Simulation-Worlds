using Unity.Entities;

namespace Software10101.DOTS.Systems.Groups.Abstract {
    public abstract partial class UnsortedSystemGroup : ComponentSystemGroup {
        protected override void OnCreate() {
            base.OnCreate();
            EnableSystemSorting = false;
        }
    }
}
