using System;
using System.Globalization;
using System.Reflection;
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
    public class WorldBehaviour : MonoBehaviour {
        [SerializeField]
        private WorldFlags _flags = WorldFlags.Live | WorldFlags.Game;

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

        private World _world;
        public World World => _world;
        public EntityManager EntityManager => _world.EntityManager;

        private void Awake() {
            _world = new World(name, _flags);

            // set up initialization group
            // ReSharper disable once UnusedVariable // not used by the bootstrapper but is one of Unity's root system groups
            InitializationSystemGroup initGroup = AddSystemToCurrentPlayerLoop(new InitializationSystemGroup(), typeof(Initialization));

            // set up simulation systems
            SimulationSystemGroup simGroup = AddSystemToCurrentPlayerLoop(new SimulationSystemGroup(), typeof(FixedUpdate));
            AddSystemToGroup(new SimulationDestroySystem(), simGroup);
            AddSystemToGroup(new SimulationResetSystemGroup(), simGroup);
            // TODO populate
            AddSystemToGroup(new PreSimulationEntityCommandBufferSystem(), simGroup);
            AddSystemToGroup(new SimulationMainSystemGroup(), simGroup);
            // TODO populate
            AddSystemToGroup(new PostSimulationEntityCommandBufferSystem(), simGroup);

            // UI interactions happen before the presentation group

            // set up presentation systems
            PresentationSystemGroup presentationGroup = AddSystemToCurrentPlayerLoop(new PresentationSystemGroup(), typeof(Update));
            AddSystemToGroup(new PrePresentationEntityCommandBufferSystem(), presentationGroup);
            AddSystemToGroup(new PresentationPreUpdateSystemGroup(), presentationGroup);
            // TODO populate
            AddSystemToGroup(new PreManagedMonoBehaviourUpdateEntityCommandBufferSystem(), presentationGroup);
            AddSystemToGroup(new ManagedMonoBehaviourUpdateSystem(), presentationGroup);
            AddSystemToGroup(new PostManagedMonoBehaviourUpdateEntityCommandBufferSystem(), presentationGroup);
            AddSystemToGroup(new PresentationPostUpdateSystemGroup(), presentationGroup);
            // TODO populate
            AddSystemToGroup(new PostPresentationEntityCommandBufferSystem(), presentationGroup);
            AddSystemToGroup(new PresentationDestroySystem(), presentationGroup);
            AddSystemToGroup(new PrefabSpawnSystem(this), presentationGroup);
            AddSystemToGroup(new EndOfFrameEntityCommandBufferSystem(), presentationGroup);
            AddSystemToGroup(new EndOfFrameSystemGroup(), presentationGroup);
            // TODO populate

            // TODO archetype producers
        }

        private void Reset() {
            _simResetGroup = GraphSystemGroupData.CreateEmpty();
            _mainSimGroup = GraphSystemGroupData.CreateEmpty();
            _presentationPreUpdateGroup = GraphSystemGroupData.CreateEmpty();
            _presentationPostUpdateGroup = GraphSystemGroupData.CreateEmpty();
            _endOfFrameGroup = GraphSystemGroupData.CreateEmpty();
        }

        public T AddSystemToCurrentPlayerLoop<T>(T system, Type playerLoopSystemType) where T : ComponentSystemBase {
            PlayerLoopSystem playerLoop = PlayerLoop.GetCurrentPlayerLoop();

            system = _world.AddSystemManaged(system);
            ScriptBehaviourUpdateOrder.AppendSystemToPlayerLoop(system, ref playerLoop, playerLoopSystemType);

            PlayerLoop.SetPlayerLoop(playerLoop);

            return system;
        }

        public T AddSystemToGroup<T>(T system, ComponentSystemGroup parent) where T : ComponentSystemBase {
            system = _world.AddSystemManaged(system);
            parent.AddSystemToUpdateList(system);

            return system;
        }

        /// <summary>
        /// This reflection helper needed because the setter on <see cref="ComponentSystemGroup.EnableSystemSorting"/> is
        /// protected.
        /// </summary>
        private static void SetSystemSortingEnabled(ComponentSystemGroup group, bool enabled) {
            const string propertyName = "EnableSystemSorting";

            PropertyInfo enableSystemSortingPropertyInfo = group.GetType()
                .GetProperty(propertyName)!
                .DeclaringType!
                .GetProperty(propertyName);

            enableSystemSortingPropertyInfo!.SetValue(
                group,
                enabled,
                BindingFlags.Instance | BindingFlags.NonPublic,
                null,
                null,
                CultureInfo.CurrentCulture);
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

        public EntityMonoBehaviour GetPrefab(int initDataPrefabIndex) {
            throw new NotImplementedException();
        }
    }
}
