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

        protected virtual void OnDestroy() {
            _world?.Dispose();
        }

        public T GetExistingSystem<T>() where T : ComponentSystemBase {
            return _world.GetExistingSystem<T>();
        }

        internal T GetOrCreateSystem<T>(Type parent) where T : ComponentSystemBase {
            return _world.GetOrCreateSystem<T>(parent);
        }

        public T GetOrCreateSystem<T>(ComponentSystemGroup group) where T : ComponentSystemBase {
            return _world.GetOrCreateSystem<T>(group);
        }

        public ComponentSystemBase GetOrCreateSystem(ComponentSystemGroup group, Type systemType) {
            return _world.GetOrCreateSystem(group, systemType);
        }

        public T AddSystem<T>(ComponentSystemGroup group, T system) where T : ComponentSystemBase {
            return _world.AddSystem(group, system);
        }

        public ComponentSystemBase AddSystem(ComponentSystemGroup group, ComponentSystemBase system) {
            return _world.AddSystem(group, system);
        }

        internal T AddSystem<T>(Type parent, T system) where T : ComponentSystemBase {
            return _world.AddSystem(parent, system);
        }

        internal ComponentSystemBase AddSystem(Type parent, ComponentSystemBase system) {
            return _world.AddSystem(parent, system);
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

        internal T GetOrCreateSystem<T>(Type parent) where T : ComponentSystemBase {
            bool hasSystem = _world.GetExistingSystem<T>() != null;

            T system = _world.GetOrCreateSystem<T>();

            if (!hasSystem) {
                _topLevelSystems[system] = parent;
                PlayerLoopUtil.AddSubSystem(parent, system);
            }

            return system;
        }

        public T GetOrCreateSystem<T>(ComponentSystemGroup group) where T : ComponentSystemBase {
            T system = _world.GetOrCreateSystem<T>();

            AddSystemToGroup(group, system);

            return system;
        }

        public ComponentSystemBase GetOrCreateSystem(ComponentSystemGroup group, Type systemType) {
            ComponentSystemBase system = _world.GetOrCreateSystem(systemType);

            AddSystemToGroup(group, system);

            return system;
        }

        public T AddSystem<T>(ComponentSystemGroup group, T system) where T : ComponentSystemBase {
            return (T)AddSystem(group, (ComponentSystemBase)system);
        }

        public ComponentSystemBase AddSystem(ComponentSystemGroup group, ComponentSystemBase system) {
            _world.AddSystem(system);

            AddSystemToGroup(group, system);

            return system;
        }

        internal T AddSystem<T>(Type parent, T system) where T : ComponentSystemBase {
            return (T)AddSystem(parent, (ComponentSystemBase)system);
        }

        internal ComponentSystemBase AddSystem(Type parent, ComponentSystemBase system) {
            _world.AddSystem(system);

            _topLevelSystems[system] = parent;
            PlayerLoopUtil.AddSubSystem(parent, system);

            return system;
        }

        public void Dispose() {
            foreach (KeyValuePair<ComponentSystemBase,Type> systemEntry in _topLevelSystems) {
                PlayerLoopUtil.RemoveSubSystem(systemEntry.Value, systemEntry.Key);
            }

            _world.Dispose();
        }

        private void AddSystemToGroup(ComponentSystemGroup group, ComponentSystemBase system) {
            switch (group) {
                case IList<ComponentSystemBase> lcsg:
                    lcsg.Add(system);
                    return;
                default:
                    group.AddSystemToUpdateList(system);
                    return;
            }
        }
    }
}
