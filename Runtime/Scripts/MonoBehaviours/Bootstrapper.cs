using System.Collections.Generic;
using Software10101.DOTS.Archetypes;
using Software10101.DOTS.Data;
using Software10101.DOTS.Systems;
using Software10101.DOTS.Systems.EntityCommandBufferSystems;
using Software10101.DOTS.Systems.Groups;
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

        [Tooltip("Systems that execute before each simulation tick. (executed 0-n times per frame)")]
        [SerializeField]
        private SystemTypeReference[] _simulationResetSystems = new SystemTypeReference[0];

        [Tooltip("Systems that execute during each simulation tick. (executed 0-n times per frame)")]
        [FormerlySerializedAs("_simulationSystems")]
        [SerializeField]
        private SystemTypeReference[] _mainSimulationSystems = new SystemTypeReference[0];

        [Tooltip("Systems that execute before the ManagedMonoBehaviours. (executed once per frame)")]
        [FormerlySerializedAs("_presentationSystems")]
        [SerializeField]
        private SystemTypeReference[] _presentationPreUpdateSystems = new SystemTypeReference[0];

        [Tooltip("Systems that execute after the ManagedMonoBehaviours. (executed once per frame)")]
        [SerializeField]
        private SystemTypeReference[] _presentationPostUpdateSystems = new SystemTypeReference[0];

        [Tooltip("Systems that execute at the end of each frame. Useful for systems that serialize the world. (executed once per frame)")]
        [SerializeField]
        private SystemTypeReference[] _endOfFrameSystems = new SystemTypeReference[0];

        protected virtual void Start() {
            // set up simulation systems
            SimulationSystemGroup simGroup = AddSystem(typeof(FixedUpdate), new SimulationSystemGroup());
            {
                AddSystem(simGroup, new SimulationDestroySystem());

                SimulationResetSystemGroup simResetGroup = AddSystem(simGroup, new SimulationResetSystemGroup());
                {
                    foreach (SystemTypeReference systemTypeReference in _simulationResetSystems) {
                        GetOrCreateSystem(simResetGroup, systemTypeReference.SystemType);
                    }
                }

                AddSystem(simGroup, new PreSimulationEntityCommandBufferSystem());

                SimulationMainSystemGroup simMainGroup = AddSystem(simGroup, new SimulationMainSystemGroup());
                {
                    foreach (SystemTypeReference systemTypeReference in _mainSimulationSystems) {
                        GetOrCreateSystem(simMainGroup, systemTypeReference.SystemType);
                    }
                }

                AddSystem(simGroup, new PostSimulationEntityCommandBufferSystem());
            }

            // UI interactions happen before the presentation group

            // set up presentation systems
            PresentationSystemGroup presGroup = AddSystem(typeof(Update), new PresentationSystemGroup());
            {
                AddSystem(presGroup, new PrePresentationEntityCommandBufferSystem());

                PresentationPreUpdateSystemGroup presPreUpdateGroup = AddSystem(presGroup, new PresentationPreUpdateSystemGroup());
                {
                    foreach (SystemTypeReference systemTypeReference in _presentationPreUpdateSystems) {
                        GetOrCreateSystem(presPreUpdateGroup, systemTypeReference.SystemType);
                    }
                }

                AddSystem(presGroup, new PreManagedMonoBehaviourUpdateEntityCommandBufferSystem());
                AddSystem(presGroup, new ManagedMonoBehaviourUpdateSystem());
                AddSystem(presGroup, new PostManagedMonoBehaviourUpdateEntityCommandBufferSystem());

                PresentationPostUpdateSystemGroup presPostUpdateGroup = AddSystem(presGroup, new PresentationPostUpdateSystemGroup());
                {
                    foreach (SystemTypeReference systemTypeReference in _presentationPostUpdateSystems) {
                        GetOrCreateSystem(presPostUpdateGroup, systemTypeReference.SystemType);
                    }
                }

                AddSystem(presGroup, new PostPresentationEntityCommandBufferSystem());

                AddSystem(presGroup, new PresentationDestroySystem());
                AddSystem(presGroup, new PrefabSpawnSystem(this));

                AddSystem(presGroup, new EndOfFrameEntityCommandBufferSystem());

                EndOfFrameSystemGroup endOfFrameGroup = AddSystem(presGroup, new EndOfFrameSystemGroup());
                {
                    foreach (SystemTypeReference systemTypeReference in _endOfFrameSystems) {
                        GetOrCreateSystem(endOfFrameGroup, systemTypeReference.SystemType);
                    }
                }
            }

            // set up archetypes
            ArchetypeProducer[] initialArchetypeProducers = _archetypeProducers.ToArray();
            _archetypeProducers.Clear();

            foreach (ArchetypeProducer archetypeProducer in initialArchetypeProducers) {
                AddArchetypeProducer(archetypeProducer);
            }
        }

        public int AddArchetypeProducer(ArchetypeProducer archetypeProducer) {
            if (_archetypeProducerIndices.TryGetValue(archetypeProducer, out int index)) {
                return index;
            }

            index = _archetypes.Count;
            _archetypes.Add(archetypeProducer.Produce(EntityManager));
            _archetypeProducers.Add(archetypeProducer);
            _archetypeProducerIndices[archetypeProducer] = index;
            return index;
        }

        internal EntityMonoBehaviour GetPrefab(int prefabIndex) {
            return _archetypeProducers[prefabIndex].Prefab;
        }

        public (Entity, EntityCommandBuffer) Create(
            ArchetypeProducer archetypeProducer,
            CreationBufferToken creationBufferToken = null) {

            return Create(_archetypeProducerIndices[archetypeProducer], creationBufferToken);
        }

        public (Entity, EntityCommandBuffer) Create(int prefabIndex, CreationBufferToken creationBufferToken = null) {
            EntityCommandBuffer ecb = creationBufferToken?.EntityCommandBuffer
                                      ?? GetExistingSystem<PostManagedMonoBehaviourUpdateEntityCommandBufferSystem>()
                                          .CreateCommandBuffer();

            Entity entity = ecb.CreateEntity(_archetypes[prefabIndex]);
            ecb.AddComponent(entity, new SpawnPrefabComponentData {
                PrefabIndex = prefabIndex
            });

            return (entity, ecb);
        }

        public void Destroy(Entity entity) {
            EntityCommandBuffer ecb =
                GetExistingSystem<PostManagedMonoBehaviourUpdateEntityCommandBufferSystem>().CreateCommandBuffer();
            ecb.AddComponent(entity, new DestroyFlagComponentData());
        }

        public CreationBufferToken GetCreationBufferToken() {
            return new CreationBufferToken {
                EntityCommandBuffer =
                    GetExistingSystem<PostManagedMonoBehaviourUpdateEntityCommandBufferSystem>().CreateCommandBuffer()
            };
        }

        public class CreationBufferToken {
            internal EntityCommandBuffer EntityCommandBuffer;

            internal CreationBufferToken() { }
        }
    }
}
