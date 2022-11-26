using System;
using Software10101.DOTS.Example.Data;
using Software10101.DOTS.MonoBehaviours;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace Software10101.DOTS.Example.Systems {
    [CreateAssetMenu(menuName = "Systems/" + nameof(ShuffleEntitiesExampleSystem))]
    public class ShuffleEntitiesExampleReference : SystemTypeReference<ShuffleEntitiesExampleSystem> { }

    // ReSharper disable once PartialTypeWithSinglePart // systems need to be partial after Entities 0.50
    // ReSharper disable once RedundantExtendsListEntry
    public partial class ShuffleEntitiesExampleSystem : ReferenceCreatedSystemBase<ShuffleEntitiesExampleReference> {
        protected override void OnUpdate() {
            Random r = new Random(Convert.ToUInt32(new System.Random().Next()));

            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            // get entities with position
            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            NativeList<Entity> entitiesWithPosition = new NativeList<Entity>(Allocator.TempJob);

            Entities
                .ForEach((Entity e, ref PositionComponentData component) => {
                    entitiesWithPosition.Add(e);
                })
                .Schedule();

            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            // shuffle
            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            Job
                .WithCode(() => {
                    // https://en.wikipedia.org/wiki/Fisher%E2%80%93Yates_shuffle#Modern_method
                    for (int i = entitiesWithPosition.Length - 1; i >= 1; i--) {
                        int j = r.NextInt(0, i + 1); // max is not inclusive
                        (entitiesWithPosition[i], entitiesWithPosition[j]) =
                            (entitiesWithPosition[j], entitiesWithPosition[i]);
                    }
                })
                .Schedule();

            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            // do something with the shuffled list and dispose of it
            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            Job
                .WithCode(() => {
                    if (entitiesWithPosition.IsEmpty) {
                        return;
                    }

                    int index = entitiesWithPosition[0].Index;
                    Debug.Log($"Initial shuffled entity index: {index}");
                })
                .WithDisposeOnCompletion(entitiesWithPosition)
                .Schedule();
        }
    }
}
