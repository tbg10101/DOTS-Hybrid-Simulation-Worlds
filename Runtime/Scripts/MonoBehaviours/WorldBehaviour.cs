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

        [SerializeField]
        private GraphSystemGroupData _simResetGroup = GraphSystemGroupData.CreateEmpty();

        [SerializeField]
        private GraphSystemGroupData _mainSimGroup = GraphSystemGroupData.CreateEmpty();

        [SerializeField]
        private GraphSystemGroupData _presentationPreUpdateGroup = GraphSystemGroupData.CreateEmpty();

        [SerializeField]
        private GraphSystemGroupData _presentationPostUpdateGroup = GraphSystemGroupData.CreateEmpty();

        [SerializeField]
        private GraphSystemGroupData _endOfFrameGroup = GraphSystemGroupData.CreateEmpty();

        [SerializeField]
        private List<ArchetypeProducer> _archetypeProducers = null;
        private readonly List<EntityArchetype> _archetypes = new();
        private readonly Dictionary<ArchetypeProducer, int> _archetypeProducerIndices = new();

        private void Awake() {
            _world = new World(name, _flags);

            // set up initialization group
            // ReSharper disable once UnusedVariable // not used by the bootstrapper but is one of Unity's root system groups
            InitializationSystemGroup initGroup = AddSystemToCurrentPlayerLoop(new InitializationSystemGroup(), typeof(Initialization));

            // set up simulation systems
            SimulationSystemGroup simGroup = AddSystemToCurrentPlayerLoop(new SimulationSystemGroup(), typeof(FixedUpdate));
            AddSystemToGroup(new SimulationDestroySystem(), simGroup);
            SimulationResetSystemGroup simResetGroup = AddSystemToGroup(new SimulationResetSystemGroup(), simGroup);
            foreach (SystemTypeReference systemTypeReference in _simResetGroup.GetExecutionOrder()) {
                CreateSystemIntoGroup(systemTypeReference.SystemType, simResetGroup);
            }
            AddSystemToGroup(new PreSimulationEntityCommandBufferSystem(), simGroup);
            SimulationMainSystemGroup mainSimGroup = AddSystemToGroup(new SimulationMainSystemGroup(), simGroup);
            foreach (SystemTypeReference systemTypeReference in _mainSimGroup.GetExecutionOrder()) {
                CreateSystemIntoGroup(systemTypeReference.SystemType, mainSimGroup);
            }
            AddSystemToGroup(new PostSimulationEntityCommandBufferSystem(), simGroup);

            // UI interactions happen before the presentation group

            // set up presentation systems
            PresentationSystemGroup presentationGroup = AddSystemToCurrentPlayerLoop(new PresentationSystemGroup(), typeof(Update));
            AddSystemToGroup(new PrePresentationEntityCommandBufferSystem(), presentationGroup);
            PresentationPreUpdateSystemGroup preUpdateGroup =
                AddSystemToGroup(new PresentationPreUpdateSystemGroup(), presentationGroup);
            foreach (SystemTypeReference systemTypeReference in _presentationPreUpdateGroup.GetExecutionOrder()) {
                CreateSystemIntoGroup(systemTypeReference.SystemType, preUpdateGroup);
            }
            AddSystemToGroup(new PreManagedMonoBehaviourUpdateEntityCommandBufferSystem(), presentationGroup);
            AddSystemToGroup(new ManagedMonoBehaviourUpdateSystem(), presentationGroup);
            AddSystemToGroup(new PostManagedMonoBehaviourUpdateEntityCommandBufferSystem(), presentationGroup);
            PresentationPostUpdateSystemGroup postUpdateGroup =
                AddSystemToGroup(new PresentationPostUpdateSystemGroup(), presentationGroup);
            foreach (SystemTypeReference systemTypeReference in _presentationPostUpdateGroup.GetExecutionOrder()) {
                CreateSystemIntoGroup(systemTypeReference.SystemType, postUpdateGroup);
            }
            AddSystemToGroup(new PostPresentationEntityCommandBufferSystem(), presentationGroup);
            AddSystemToGroup(new PresentationDestroySystem(), presentationGroup);
            AddSystemToGroup(new PrefabSpawnSystem(this), presentationGroup);
            AddSystemToGroup(new EndOfFrameEntityCommandBufferSystem(), presentationGroup);
            EndOfFrameSystemGroup endOfFrameGroup = AddSystemToGroup(new EndOfFrameSystemGroup(), presentationGroup);
            foreach (SystemTypeReference systemTypeReference in _endOfFrameGroup.GetExecutionOrder()) {
                CreateSystemIntoGroup(systemTypeReference.SystemType, endOfFrameGroup);
            }

            ArchetypeProducer[] initialArchetypeProducers = _archetypeProducers.ToArray();
            _archetypeProducers.Clear();

            foreach (ArchetypeProducer archetypeProducer in initialArchetypeProducers) {
                AddArchetypeProducer(archetypeProducer);
            }
        }

        private void Reset() {
            _simResetGroup = GraphSystemGroupData.CreateEmpty();
            _mainSimGroup = GraphSystemGroupData.CreateEmpty();
            _presentationPreUpdateGroup = GraphSystemGroupData.CreateEmpty();
            _presentationPostUpdateGroup = GraphSystemGroupData.CreateEmpty();
            _endOfFrameGroup = GraphSystemGroupData.CreateEmpty();
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

        private void CreateSystemIntoGroup(Type systemType, ComponentSystemGroup group) {
            ReferenceCreatedSystemBase system = _world.CreateSystemManaged(systemType) as ReferenceCreatedSystemBase;
            group.AddSystemToUpdateList(system);
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

            Dictionary<GraphSystemGroupData, string> groups = new() {
                { _simResetGroup, nameof(_simResetGroup) },
                { _mainSimGroup, nameof(_mainSimGroup) },
                { _presentationPreUpdateGroup, nameof(_presentationPreUpdateGroup) },
                { _presentationPostUpdateGroup, nameof(_presentationPostUpdateGroup) },
                { _endOfFrameGroup, nameof(_endOfFrameGroup) },
            };

            foreach ((GraphSystemGroupData graphSystemGroupData, string groupName) in groups) {
                foreach (GraphSystemGroupData.SystemNodeData systemNodeData in graphSystemGroupData.Nodes) {
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

        [Serializable]
        public struct GraphSystemGroupData {
            public SystemNodeData[] Nodes;

            public static GraphSystemGroupData CreateEmpty() {
                GraphSystemGroupData newData = new() {
                    Nodes = new []{ new SystemNodeData() }
                };

                return newData;
            }

            public IEnumerable<SystemTypeReference> GetExecutionOrder() {
                SystemNodeData root = default;
                Dictionary<SystemTypeReference, SystemNodeData> nodesForSystemReferences = Nodes
                    .Where(node => {
                        if (ReferenceEquals(node.SystemReference, null)) {
                            root = node;
                            return false;
                        }

                        bool nodeContentsValid = node.SystemReference;
                        if (!nodeContentsValid) {
                            Debug.LogWarning("System graph has missing system references!");
                        }

                        return nodeContentsValid;
                    })
                    .ToDictionary(node => node.SystemReference, node => node);

                List<SystemTypeReference> results = new();
                Queue<SystemTypeReference> fringe = new(root.Dependencies);

                while (fringe.Count > 0) {
                    SystemTypeReference systemReference = fringe.Dequeue();

                    if (!systemReference) {
                        continue;
                    }

                    int existingIndex = results.IndexOf(systemReference);

                    if (existingIndex < 0) {
                        results.Insert(0, systemReference);
                    } else if (existingIndex > 0) {
                        results.RemoveAt(existingIndex);
                        results.Insert(0, systemReference);
                    }

                    SystemNodeData node = nodesForSystemReferences[systemReference];

                    foreach (SystemTypeReference systemTypeReference in node.Dependencies) {
                        fringe.Enqueue(systemTypeReference);
                    }
                }

                return results;
            }

            [Serializable]
            public struct SystemNodeData {
                public SystemTypeReference SystemReference;
                public Vector2 NodePosition;
                public SystemTypeReference[] Dependencies;

                public SystemNodeData(
                    SystemTypeReference systemReference,
                    Vector2 nodePosition,
                    SystemTypeReference[] dependencies
                ) {
                    SystemReference = systemReference;
                    NodePosition = nodePosition;
                    Dependencies = dependencies;
                }
            }
        }
    }
}
