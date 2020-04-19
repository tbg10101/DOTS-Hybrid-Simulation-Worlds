using Unity.Entities;
using UnityEngine;

namespace Software10101.DOTS.Archetypes {
    public abstract class ArchetypeProducer : ScriptableObject {
        public abstract EntityArchetype Produce(EntityManager entityManager);
    }
}
