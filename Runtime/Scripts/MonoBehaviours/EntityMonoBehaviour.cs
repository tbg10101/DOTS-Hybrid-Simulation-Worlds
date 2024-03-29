using System;
using System.Collections.Generic;
using Unity.Entities;

#if UNITY_EDITOR && ENTITY_NAME_SYNC
using UnityEngine.Profiling;
#endif

namespace Software10101.DOTS.MonoBehaviours {
    public abstract class EntityMonoBehaviour : ManagedMonoBehaviour {
        private static readonly Dictionary<Entity, EntityMonoBehaviour> Instances = new();

        private Entity _entity = Entity.Null;
        public Entity Entity {
            get => _entity;
            internal set {
                Instances.Remove(_entity);

                if (value != Entity.Null) {
                    Instances[value] = this;
                }

                _entity = value;
            }
        }

        private WorldBehaviour _worldBehaviour = null;
        public WorldBehaviour WorldBehaviour {
            protected get => _worldBehaviour;
            set {
                if (_worldBehaviour != null) {
                    throw new Exception("Cannot set bootstrapper multiple times.");
                }

                _worldBehaviour = value;
            }
        }

        protected EntityManager EntityManager => _worldBehaviour.EntityManager;

#if UNITY_EDITOR && ENTITY_NAME_SYNC
        private string _oldName = null;
#endif

        protected virtual void Awake() {
#if UNITY_EDITOR && ENTITY_NAME_SYNC
            _oldName = name;
#endif
        }

        protected virtual void OnDestroy() {
            Instances.Remove(_entity);
        }

        protected override void OnUpdate() {
#if UNITY_EDITOR && ENTITY_NAME_SYNC
            Profiler.BeginSample("NameChange");
            if (name != _oldName) {
                EntityManager.SetName(_entity, name);
            }
            Profiler.EndSample();
#endif
        }

        public static EntityMonoBehaviour Get(Entity entity) {
            return Instances[entity];
        }

        internal override void Destroy() {
            Entity = Entity.Null;
            _worldBehaviour = null;

            base.Destroy();
        }

        /// <summary>
        /// Called after the PrefabSpawnSystem instantiates the prefab.
        /// </summary>
        public virtual void OnPostInstantiate() { }
    }
}
