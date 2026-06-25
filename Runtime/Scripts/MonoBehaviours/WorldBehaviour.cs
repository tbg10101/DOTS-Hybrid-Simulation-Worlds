using System;
using System.Collections.Generic;
using System.Linq;
using Software10101.DOTS.Archetypes;
using Software10101.DOTS.Data;
using Software10101.DOTS.Systems;
using Software10101.DOTS.Systems.EntityCommandBufferSystems;
using Software10101.DOTS.Systems.Groups;
using Unity.Entities;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;
using InitializationSystemGroup = Unity.Entities.InitializationSystemGroup;
using PresentationSystemGroup = Unity.Entities.PresentationSystemGroup;
using SimulationSystemGroup = Unity.Entities.SimulationSystemGroup;

namespace Software10101.DOTS.MonoBehaviours {
    [DisallowMultipleComponent]
    public sealed class WorldBehaviour : MonoBehaviour {
        private World _world;
        public World World => _world;
        public EntityManager EntityManager => _world.EntityManager;

        [SerializeField]
        private WorldFlags _flags = WorldFlags.Game;

        [Tooltip("Also know as the Initialization group.")]
        [SerializeField]
        private SystemGroupGraphAsset _startOfFrameGraph = null;

        [SerializeField]
        private SystemGroupGraphAsset _simResetGraph = null;

        [SerializeField]
        private SystemGroupGraphAsset _mainSimGraph = null;

        [SerializeField]
        private SystemGroupGraphAsset _presentationPreUpdateGraph = null;

        [SerializeField]
        private SystemGroupGraphAsset _presentationPostUpdateGraph = null;

        [SerializeField]
        private SystemGroupGraphAsset _endOfFrameGraph = null;

        // Legacy embedded group data, retained so the migration utility (Migrate System Group Graphs) can convert
        // pre-6.5 scenes into SystemGroupGraphAsset references. These fields are no longer read at runtime and will be
        // removed in a future version once existing content has been migrated.
        [SerializeField, HideInInspector]
        private GraphSystemGroupData _startOfFrameGroup = GraphSystemGroupData.CreateEmpty();

        [SerializeField, HideInInspector]
        private GraphSystemGroupData _simResetGroup = GraphSystemGroupData.CreateEmpty();

        [SerializeField, HideInInspector]
        private GraphSystemGroupData _mainSimGroup = GraphSystemGroupData.CreateEmpty();

        [SerializeField, HideInInspector]
        private GraphSystemGroupData _presentationPreUpdateGroup = GraphSystemGroupData.CreateEmpty();

        [SerializeField, HideInInspector]
        private GraphSystemGroupData _presentationPostUpdateGroup = GraphSystemGroupData.CreateEmpty();

        [SerializeField, HideInInspector]
        private GraphSystemGroupData _endOfFrameGroup = GraphSystemGroupData.CreateEmpty();

        [SerializeField]
        private List<ArchetypeProducer> _archetypeProducers = null;
        private readonly List<EntityArchetype> _archetypes = new();
        private readonly Dictionary<ArchetypeProducer, int> _archetypeProducerIndices = new();

        private void Awake() {
            _world = new World(name, _flags);

            // set up initialization systems
            InitializationSystemGroup startOfFrameGroup =
                AddSystemToCurrentPlayerLoop(new InitializationSystemGroup(), typeof(Initialization));
            foreach (SystemTypeReference systemTypeReference in GetExecutionOrder(_startOfFrameGraph)) {
                CreateSystemIntoGroup(systemTypeReference.SystemType, startOfFrameGroup, systemTypeReference);
            }
            AddSystemToGroup(new PostStartOfFrameEntityCommandBufferSystem(), startOfFrameGroup);

            // set up simulation systems
            SimulationSystemGroup simGroup = AddSystemToCurrentPlayerLoop(new SimulationSystemGroup(), typeof(FixedUpdate));
            AddSystemToGroup<SimulationDestroySystem>(simGroup);
            SimulationResetSystemGroup simResetGroup = AddSystemToGroup(new SimulationResetSystemGroup(), simGroup);
            foreach (SystemTypeReference systemTypeReference in GetExecutionOrder(_simResetGraph)) {
                CreateSystemIntoGroup(systemTypeReference.SystemType, simResetGroup, systemTypeReference);
            }
            AddSystemToGroup(new PreSimulationEntityCommandBufferSystem(), simGroup);
            SimulationMainSystemGroup mainSimGroup = AddSystemToGroup(new SimulationMainSystemGroup(), simGroup);
            foreach (SystemTypeReference systemTypeReference in GetExecutionOrder(_mainSimGraph)) {
                CreateSystemIntoGroup(systemTypeReference.SystemType, mainSimGroup, systemTypeReference);
            }
            AddSystemToGroup(new PostSimulationEntityCommandBufferSystem(), simGroup);

            // UI interactions happen before the presentation group

            // set up presentation systems
            PresentationSystemGroup presentationGroup =
                AddSystemToCurrentPlayerLoop(new PresentationSystemGroup(), typeof(Update));
            AddSystemToGroup(new PrePresentationEntityCommandBufferSystem(), presentationGroup);
            PresentationPreUpdateSystemGroup preUpdateGroup =
                AddSystemToGroup(new PresentationPreUpdateSystemGroup(), presentationGroup);
            foreach (SystemTypeReference systemTypeReference in GetExecutionOrder(_presentationPreUpdateGraph)) {
                CreateSystemIntoGroup(systemTypeReference.SystemType, preUpdateGroup, systemTypeReference);
            }
            AddSystemToGroup(new PreManagedMonoBehaviourUpdateEntityCommandBufferSystem(), presentationGroup);
            AddSystemToGroup<ManagedMonoBehaviourUpdateSystem>(presentationGroup);
            AddSystemToGroup(new PostManagedMonoBehaviourUpdateEntityCommandBufferSystem(), presentationGroup);
            PresentationPostUpdateSystemGroup postUpdateGroup =
                AddSystemToGroup(new PresentationPostUpdateSystemGroup(), presentationGroup);
            foreach (SystemTypeReference systemTypeReference in GetExecutionOrder(_presentationPostUpdateGraph)) {
                CreateSystemIntoGroup(systemTypeReference.SystemType, postUpdateGroup, systemTypeReference);
            }
            AddSystemToGroup(new PostPresentationEntityCommandBufferSystem(), presentationGroup);
            AddSystemToGroup(new PresentationDestroySystem(), presentationGroup);
            AddSystemToGroup(new PrefabSpawnSystem(this), presentationGroup);
            AddSystemToGroup(new EndOfFrameEntityCommandBufferSystem(), presentationGroup);
            EndOfFrameSystemGroup endOfFrameGroup = AddSystemToGroup(new EndOfFrameSystemGroup(), presentationGroup);
            foreach (SystemTypeReference systemTypeReference in GetExecutionOrder(_endOfFrameGraph)) {
                CreateSystemIntoGroup(systemTypeReference.SystemType, endOfFrameGroup, systemTypeReference);
            }

            ArchetypeProducer[] initialArchetypeProducers = _archetypeProducers.ToArray();
            _archetypeProducers.Clear();

            foreach (ArchetypeProducer archetypeProducer in initialArchetypeProducers) {
                AddArchetypeProducer(archetypeProducer);
            }
        }

        private void Reset() {
            _startOfFrameGraph = null;
            _simResetGraph = null;
            _mainSimGraph = null;
            _presentationPreUpdateGraph = null;
            _presentationPostUpdateGraph = null;
            _endOfFrameGraph = null;
        }

        private static IEnumerable<SystemTypeReference> GetExecutionOrder(SystemGroupGraphAsset graph) {
            return graph ? graph.GetExecutionOrder() : Enumerable.Empty<SystemTypeReference>();
        }

        private void OnDestroy() {
            _world.Dispose();
        }

        private T AddSystemToCurrentPlayerLoop<T>(T system, Type playerLoopSystemType) where T : ComponentSystemBase {
            PlayerLoopSystem playerLoop = PlayerLoop.GetCurrentPlayerLoop();

            system = _world.AddSystemManaged(system);
            ScriptBehaviourUpdateOrder.AppendSystemToPlayerLoop(system, ref playerLoop, playerLoopSystemType);

            PlayerLoop.SetPlayerLoop(playerLoop);

            return system;
        }

        private T AddSystemToGroup<T>(T system, ComponentSystemGroup parent) where T : ComponentSystemBase {
            system = _world.AddSystemManaged(system);
            parent.AddSystemToUpdateList(system);

            return system;
        }

        private SystemHandle AddSystemToGroup<T>(ComponentSystemGroup parent) where T : ISystem {
            SystemHandle sh = _world.CreateSystem(typeof(T));
            parent.AddSystemToUpdateList(sh);

            return sh;
        }

        private void CreateSystemIntoGroup(Type systemType, ComponentSystemGroup group, SystemTypeReference systemTypeReference) {
            if (systemType.IsSubclassOf(typeof(ReferenceCreatedSystemBase))) {
                ReferenceCreatedSystemBase system = _world.CreateSystemManaged(systemType) as ReferenceCreatedSystemBase;
                system.SetCreator(systemTypeReference);
                group.AddSystemToUpdateList(system);
            } else if (systemType.GetInterfaces().Contains(typeof(ISystem))) {
                ISystemTypeReference iSystemTypeReference = (ISystemTypeReference)systemTypeReference;
                SystemHandle sh = _world.CreateSystem(systemType);

                iSystemTypeReference.SetConfig(EntityManager, sh);

                group.AddSystemToUpdateList(sh);
            } else {
                throw new ArgumentException(
                    $"System type must be subclass of ReferenceCreatedSystemBase or ISystem: {systemType.FullName}");
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

        public EntityMonoBehaviour GetPrefab(int prefabIndex) {
            return _archetypeProducers[prefabIndex].Prefab;
        }

        public (Entity, EntityCommandBuffer) Create(
            ArchetypeProducer archetypeProducer,
            CreationBufferToken creationBufferToken = null
        ) {
            return Create(_archetypeProducerIndices[archetypeProducer], creationBufferToken);
        }

        public (Entity, EntityCommandBuffer) Create(int prefabIndex, CreationBufferToken creationBufferToken = null) {
            EntityCommandBuffer ecb = creationBufferToken?.EntityCommandBuffer ??
                                      _world.GetExistingSystemManaged<PostManagedMonoBehaviourUpdateEntityCommandBufferSystem>()
                                          .CreateCommandBuffer();

            Entity entity = ecb.CreateEntity(_archetypes[prefabIndex]);
            ecb.AddComponent(entity, new SpawnPrefabComponentData {
                PrefabIndex = prefabIndex
            });

            return (entity, ecb);
        }

        public IReadOnlyDictionary<SystemTypeReference, string> GetAllConfiguredSystemReferences() {
            Dictionary<SystemTypeReference, string> result = new();

            (SystemGroupGraphAsset graph, string groupName)[] groups = {
                (_startOfFrameGraph, nameof(_startOfFrameGraph)),
                (_simResetGraph, nameof(_simResetGraph)),
                (_mainSimGraph, nameof(_mainSimGraph)),
                (_presentationPreUpdateGraph, nameof(_presentationPreUpdateGraph)),
                (_presentationPostUpdateGraph, nameof(_presentationPostUpdateGraph)),
                (_endOfFrameGraph, nameof(_endOfFrameGraph)),
            };

            foreach ((SystemGroupGraphAsset graph, string groupName) in groups) {
                if (!graph) {
                    continue;
                }

                foreach (GraphSystemGroupData.SystemNodeData systemNodeData in graph.Nodes) {
                    if (systemNodeData.SystemReference) {
                        result.Add(systemNodeData.SystemReference, groupName);
                    }
                }
            }

            return result;
        }

        public void Destroy(Entity entity) {
            EntityCommandBuffer ecb = _world
                .GetExistingSystemManaged<PostManagedMonoBehaviourUpdateEntityCommandBufferSystem>()
                .CreateCommandBuffer();

            ecb.AddComponent(entity, new DestroyFlagComponentData());
        }

        public CreationBufferToken GetCreationBufferToken() {
            return new CreationBufferToken {
                EntityCommandBuffer = _world
                    .GetExistingSystemManaged<PostManagedMonoBehaviourUpdateEntityCommandBufferSystem>()
                    .CreateCommandBuffer()
            };
        }

        public class CreationBufferToken {
            internal EntityCommandBuffer EntityCommandBuffer;

            internal CreationBufferToken() { }
        }
    }
}
