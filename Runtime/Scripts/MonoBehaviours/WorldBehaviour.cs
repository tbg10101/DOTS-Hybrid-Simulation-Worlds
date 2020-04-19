using System;
using System.Collections.Generic;
using Software10101.DOTS.Utils;
using Unity.Entities;
using UnityEngine;

namespace Software10101.DOTS.MonoBehaviours {
    public class WorldBehaviour : MonoBehaviour {
        private WorldWrapper _world = null;

        public EntityManager EntityManager => _world.EntityManager;

        protected virtual void Awake() {
            _world = new WorldWrapper(name);
        }

        private void OnDestroy() {
            _world?.Dispose();
        }

        public T GetExistingSystem<T>() where T : ComponentSystemBase {
            return _world.GetExistingSystem<T>();
        }

        public T GetOrCreateSystem<T>(Type parent) where T : ComponentSystemBase {
            return _world.GetOrCreateSystem<T>(parent);
        }

        public T GetOrCreateSystem<T>(ComponentSystemBase group) where T : ComponentSystemBase {
            return _world.GetOrCreateSystem<T>(group);
        }

        public ComponentSystemBase GetOrCreateSystem(ComponentSystemBase group, Type systemType) {
            return _world.GetOrCreateSystem(group, systemType);
        }

        public ComponentSystemBase AddSystem(ComponentSystemBase group, ComponentSystemBase system) {
            return _world.AddSystem(group, system);
        }
    }

    public class WorldWrapper : IDisposable {
        private readonly World _world;

        private readonly Dictionary<ComponentSystemBase, Type> _topLevelSystems = new Dictionary<ComponentSystemBase, Type>();

        public EntityManager EntityManager => _world.EntityManager;

        public WorldWrapper(string name) {
            _world = new World(name);
        }

        public T GetExistingSystem<T>() where T : ComponentSystemBase {
            return _world.GetExistingSystem<T>();
        }

        public T GetOrCreateSystem<T>(Type parent) where T : ComponentSystemBase {
            bool hasSystem = _world.GetExistingSystem<T>() != null;

            T system = _world.GetOrCreateSystem<T>();

            if (!hasSystem) {
                _topLevelSystems[system] = parent;
                PlayerLoopUtil.AddSubSystem(parent, system);
            }

            return system;
        }

        public T GetOrCreateSystem<T>(ComponentSystemBase group) where T : ComponentSystemBase {
            T system = _world.GetOrCreateSystem<T>();

            AddSystemToGroup(group, system);

            return system;
        }

        public ComponentSystemBase GetOrCreateSystem(ComponentSystemBase group, Type systemType) {
            ComponentSystemBase system = _world.GetOrCreateSystem(systemType);

            AddSystemToGroup(group, system);

            return system;
        }

        public ComponentSystemBase AddSystem(ComponentSystemBase group, ComponentSystemBase system) {
            _world.AddSystem(system);

            AddSystemToGroup(group, system);

            return system;
        }

        public void Dispose() {
            foreach (KeyValuePair<ComponentSystemBase,Type> systemEntry in _topLevelSystems) {
                PlayerLoopUtil.RemoveSubSystem(systemEntry.Value, systemEntry.Key);
            }

            _world.Dispose();
        }

        private void AddSystemToGroup(ComponentSystemBase group, ComponentSystemBase system) {
            switch (group) {
                case ComponentSystemGroup csg:
                    csg.AddSystemToUpdateList(system);
                    break;
                case IList<ComponentSystemBase> lcsg:
                    lcsg.Add(system);
                    break;
                default:
                    throw new Exception("Group must be compatible.");
            }
        }
    }
}
