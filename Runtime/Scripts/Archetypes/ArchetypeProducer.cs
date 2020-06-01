using Software10101.DOTS.MonoBehaviours;
using Unity.Entities;
using UnityEngine;

namespace Software10101.DOTS.Archetypes {
    public abstract class ArchetypeProducer : ScriptableObject {
        [SerializeField]
        private EntityMonoBehaviour _prefab = null;
        public EntityMonoBehaviour Prefab => _prefab;

        public abstract EntityArchetype Produce(EntityManager entityManager);
    }
}
