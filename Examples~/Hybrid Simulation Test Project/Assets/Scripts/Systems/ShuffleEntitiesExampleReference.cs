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

    public class ShuffleEntitiesExampleSystem : SystemBase {
        protected override void OnUpdate() {
            // encapsulate the random
            NativeSingleton<Random> random = new NativeSingleton<Random>(
                new Random(Convert.ToUInt32(new System.Random().Next())),
                Allocator.TempJob);

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
                    Random r = random.GetValue();

                    // https://en.wikipedia.org/wiki/Fisher%E2%80%93Yates_shuffle#Modern_method
                    for (int i = entitiesWithPosition.Length - 1; i >= 1; i--) {
                        int j = r.NextInt(0, i + 1); // max is not inclusive
                        (entitiesWithPosition[i], entitiesWithPosition[j]) =
                            (entitiesWithPosition[j], entitiesWithPosition[i]);
                    }

                    random.SetValue(r);
                })
                .WithDisposeOnCompletion(random)
                .Schedule();
            
            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            // do something with the shuffled list and dispose of it
            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            Job
                .WithCode(() => {
                    int length = entitiesWithPosition.Length;
                    
                    for (int i = 0; i < length; i++) {
                        Entity e = entitiesWithPosition[i];
                    }
                })
                .WithDisposeOnCompletion(entitiesWithPosition)
                .Schedule();
        }
    }
}
