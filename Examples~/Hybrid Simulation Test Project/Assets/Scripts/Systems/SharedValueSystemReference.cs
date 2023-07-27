using Software10101.DOTS.Example.Data;
using Software10101.DOTS.MonoBehaviours;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Software10101.DOTS.Example.Systems {
    [CreateAssetMenu(menuName = "Systems/" + nameof(SharedValueSystem))]
    public class SharedValueSystemReference : SystemTypeReference<SharedValueSystem> { }

    public partial class SharedValueSystem : ReferenceCreatedSystemBase<SharedValueSystemReference> {
        protected override void OnUpdate() {
            NativeReference<int> nativeReference = new(0, Allocator.TempJob);

            // do something to populate the value
            Entities
                .ForEach((in PositionComponentData _) => {
                    nativeReference.Value++;
                })
                .Schedule();

            // read the value in another job - the dependency is already managed by the SystemBase' implicit JobHandle
            Job
                .WithReadOnly(nativeReference)
                .WithCode(() => {
                    Debug.Log($"Position Count: {nativeReference.Value}");
                })
                .WithDisposeOnCompletion(nativeReference)
                .Schedule();
        }
    }
}
