using Software10101.DOTS.Archetypes;
using Software10101.DOTS.Example.Data;
using Unity.Entities;
using UnityEngine;

namespace Software10101.DOTS.Example.Archetypes {
    [CreateAssetMenu(menuName = "Archetypes/" + nameof(TestArchetypeProducer))]
    public class TestArchetypeProducer : ArchetypeProducer {
        public override EntityArchetype Produce(EntityManager entityManager) {
            return entityManager.CreateArchetype(
                new ComponentType(typeof(PositionComponentData)),
                new ComponentType(typeof(RotationComponentData)),
                new ComponentType(typeof(VelocityComponentData), ComponentType.AccessMode.ReadOnly));
        }
    }
}
