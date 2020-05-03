using System.Collections.Generic;
using Software10101.DOTS.Archetypes;
using Software10101.DOTS.Data;
using Software10101.DOTS.Systems;
using Software10101.DOTS.Systems.EntityCommandBufferSystems;
using Unity.Entities;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.Serialization;
using PresentationSystemGroup = Software10101.DOTS.Systems.Groups.PresentationSystemGroup;
using SimulationSystemGroup = Software10101.DOTS.Systems.Groups.SimulationSystemGroup;

namespace Software10101.DOTS.MonoBehaviours {
    /// <summary>
    /// The purpose of this class is to enable DOTS configuration from the inspector.
    ///
    /// Each instance of this class contains it's own world.
    ///
    /// This system handles simulation and presentation a little differently than the default Unity world:
    ///     * Simulation systems are run during Unity's FixedUpdate. Presentation still runs during Update so there are utilities
    ///             and examples that demonstrate how to render an interpolated simulation state.
    ///     * The top level system groups are not sorted using UpdateAfter/UpdateBefore annotations. Instead they execute in the
    ///             order configured in the inspector. Other groups can be sorted using those annotations if you want.
    ///
    /// Systems are set up to do presentation using GameObjects. Your custom Components should inherit from EntityMonoBehaviour
    /// instead of MonoBehaviour.
    ///
    /// Prefabs and EntityArchetypes are managed in this class as well. Utility methods for creation and destruction are provided
    /// to ease the management of the simulation and presentation sides of an object.
    ///
    /// This class can be extended. Place your custom world bootstrapping code into a Start() override AFTER base.Start() is
    /// called. (see example)
    /// </summary>
    public class Bootstrapper : WorldBehaviour {
        [FormerlySerializedAs("_prefabs")]
        [SerializeField]
        private List<ArchetypeProducer> _archetypeProducers = null;
        private readonly List<EntityArchetype> _archetypes = new List<EntityArchetype>();
        private readonly Dictionary<ArchetypeProducer, int> _archetypeProducerIndices = new Dictionary<ArchetypeProducer, int>();

        [SerializeField]
        private SystemTypeReference[] _simulationSystems = new SystemTypeReference[0];

        [SerializeField]
        private SystemTypeReference[] _presentationSystems = new SystemTypeReference[0];

        protected virtual void Start() {
            // set up systems
            SimulationSystemGroup simGroup = GetOrCreateSystem<SimulationSystemGroup>(typeof(FixedUpdate));
            GetOrCreateSystem<SimulationDestroySystem>(simGroup);
            AddSystem(simGroup, new PrefabSpawnSystem(this));
            GetOrCreateSystem<PreSimulationEntityCommandBufferSystem>(simGroup);
            foreach (SystemTypeReference systemTypeReference in _simulationSystems) {
                GetOrCreateSystem(simGroup, systemTypeReference.SystemType);
            }
            GetOrCreateSystem<PostSimulationEntityCommandBufferSystem>(simGroup);

            PresentationSystemGroup presGroup = GetOrCreateSystem<PresentationSystemGroup>(typeof(Update));
            foreach (SystemTypeReference systemTypeReference in _presentationSystems) {
                GetOrCreateSystem(presGroup, systemTypeReference.SystemType);
            }
            GetOrCreateSystem<PreUpdatePresentationEntityCommandBufferSystem>(presGroup);
            GetOrCreateSystem<ManagedMonoBehaviourUpdateSystem>(presGroup);
            GetOrCreateSystem<PostUpdatePresentationEntityCommandBufferSystem>(presGroup);
            GetOrCreateSystem<PresentationDestroySystem>(presGroup);

            // set up archetypes
            foreach (ArchetypeProducer archetypeProducer in _archetypeProducers) {
                AddArchetypeProducer(archetypeProducer);
            }
        }

        public int AddArchetypeProducer(ArchetypeProducer archetypeProducer) {
            if (_archetypeProducerIndices.TryGetValue(archetypeProducer, out int index)) {
                return index;
            }

            index = _archetypes.Count;
            _archetypes.Add(archetypeProducer.Produce(EntityManager));
            _archetypeProducerIndices[archetypeProducer] = index;
            return index;
        }

        internal EntityMonoBehaviour GetPrefab(int prefabIndex) {
            return _archetypeProducers[prefabIndex].Prefab;
        }

        public (Entity, EntityCommandBuffer) Create(ArchetypeProducer archetypeProducer) {
            return Create(_archetypeProducerIndices[archetypeProducer]);
        }

        public (Entity, EntityCommandBuffer) Create(int prefabIndex) {
            EntityCommandBuffer entityCommandBuffer =
                GetExistingSystem<PostUpdatePresentationEntityCommandBufferSystem>().CreateCommandBuffer();

            Entity entity = entityCommandBuffer.CreateEntity(_archetypes[prefabIndex]);
            entityCommandBuffer.AddComponent(entity, new InitComponentData {
                PrefabIndex = prefabIndex
            });

            return (entity, entityCommandBuffer);
        }

        public void Destroy(Entity entity) {
            EntityCommandBuffer entityCommandBuffer =
                GetExistingSystem<PostUpdatePresentationEntityCommandBufferSystem>().CreateCommandBuffer();

            entityCommandBuffer.AddComponent(entity, new DestroyFlagComponentData());
        }
    }
}
