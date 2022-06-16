using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using UnityEngine;

namespace Software10101.DOTS.Systems.Groups {
    /// <summary>
    /// Component system group that acts like a List. Not compatible with unmanaged systems.
    /// </summary>
    [Obsolete]
    public abstract class ListComponentSystemGroup :
        ComponentSystemGroup,
        IList<ComponentSystemBase>,
        IReadOnlyList<ComponentSystemBase>,
        IList {

        private List<ComponentSystemBase> _underlyingSystemsToUpdate;
        private static readonly FieldInfo UnderlyingSystemsToUpdateField = typeof(ComponentSystemGroup).GetField(
            "m_systemsToUpdate",
            BindingFlags.Instance | BindingFlags.NonPublic);

        private List<ComponentSystemBase> _underlyingSystemsToRemove;
        private static readonly FieldInfo UnderlyingSystemsToRemoveField = typeof(ComponentSystemGroup).GetField(
            "m_systemsToRemove",
            BindingFlags.Instance | BindingFlags.NonPublic);

        private static readonly FieldInfo MasterUpdateListField = typeof(ComponentSystemGroup).GetField(
            "m_MasterUpdateList",
            BindingFlags.Instance | BindingFlags.NonPublic);

        private static readonly Type UpdateIndexType = typeof(ComponentSystemGroup).Assembly.GetType("Unity.Entities.UpdateIndex");
        private static readonly Type UnsafeListOfUpdateIndexType = typeof(UnsafeList<>).MakeGenericType(UpdateIndexType);

        private static readonly MethodInfo UnsafeListOfUpdateIndexClearMethod = UnsafeListOfUpdateIndexType
            .GetMethod("Clear", BindingFlags.Instance | BindingFlags.Public);
        private static readonly MethodInfo UnsafeListOfUpdateIndexSetCapacityMethod = UnsafeListOfUpdateIndexType
            .GetMethod("SetCapacity", BindingFlags.Instance | BindingFlags.Public);
        private static readonly MethodInfo UnsafeListOfUpdateIndexAddMethod = UnsafeListOfUpdateIndexType
            .GetMethod("Add", BindingFlags.Instance | BindingFlags.Public);

        private static readonly ConstructorInfo UpdateIndexConstructor = UpdateIndexType.GetConstructor(new[] {typeof(int), typeof(bool)});

        private readonly List<ComponentSystemBase> _systems = new List<ComponentSystemBase>();
        private readonly List<ComponentSystemBase> _mutableSystemsList = new List<ComponentSystemBase>();
        private bool _systemsListDirtyFlag = false;

        public int Count => _mutableSystemsList.Count;
        public bool IsSynchronized => ((ICollection)_mutableSystemsList).IsSynchronized;
        public object SyncRoot => ((ICollection)_mutableSystemsList).SyncRoot;
        public bool IsFixedSize => ((IList)_mutableSystemsList).IsFixedSize;
        public bool IsReadOnly => ((ICollection<ComponentSystemBase>)_mutableSystemsList).IsReadOnly;

        public override IReadOnlyList<ComponentSystemBase> Systems => _systems;

        protected override void OnCreate() {
            base.OnCreate();

            _underlyingSystemsToUpdate = (List<ComponentSystemBase>)UnderlyingSystemsToUpdateField.GetValue(this);
            _underlyingSystemsToRemove = (List<ComponentSystemBase>)UnderlyingSystemsToRemoveField.GetValue(this);
        }

        protected override void OnUpdate() {
            if (_underlyingSystemsToUpdate.Count > 0) {
                _mutableSystemsList.AddRange(_underlyingSystemsToUpdate);
                _underlyingSystemsToUpdate.Clear();
                _systemsListDirtyFlag = true;
            }

            if (_underlyingSystemsToRemove.Count > 0) {
                _underlyingSystemsToRemove.ForEach(system => _mutableSystemsList.Remove(system));
                _underlyingSystemsToRemove.Clear();
                _systemsListDirtyFlag = true;
            }

            if (_systemsListDirtyFlag) {
                _systemsListDirtyFlag = false;
                _systems.Clear();
                _systems.AddRange(_mutableSystemsList);

                // this icky stuff is because the entity debugger only really wants to work with the default group class
                #region EntityDebuggerWorkarounds
                    object o = MasterUpdateListField.GetValue(this);
                    UnsafeListOfUpdateIndexClearMethod.Invoke(o, null);
                    UnsafeListOfUpdateIndexSetCapacityMethod.Invoke(o, new object[] {_systems.Count});

                    for (int i = 0; i < _systems.Count; i++) {
                        object obj = UpdateIndexConstructor.Invoke(new object[] {i, true});
                        UnsafeListOfUpdateIndexAddMethod.Invoke(o, new [] {obj});
                    }

                    MasterUpdateListField.SetValue(this, o);
                #endregion
            }

            if (RateManager == null) {
                UpdateAllSystems();
            } else {
                while (RateManager.ShouldGroupUpdate(this)) {
                    UpdateAllSystems();
                }
            }
        }

        private void UpdateAllSystems() {
            int count = _systems.Count;
            int index;

            for (index = 0; index < count; index++) {
                try {
                    _systems[index].Update();
                } catch (Exception e) {
                    Debug.LogException(e);
                }

                if (World.QuitUpdate) {
                    break;
                }
            }
        }

        IEnumerator<ComponentSystemBase> IEnumerable<ComponentSystemBase>.GetEnumerator() {
            return ((IEnumerable<ComponentSystemBase>)_mutableSystemsList).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return ((IEnumerable)_mutableSystemsList).GetEnumerator();
        }

        public List<ComponentSystemBase>.Enumerator GetEnumerator() {
            return _mutableSystemsList.GetEnumerator();
        }

        int IList.Add(object value) {
            int result = ((IList)_mutableSystemsList).Add(value);
            _systemsListDirtyFlag = true;
            return result;
        }

        public void Add(ComponentSystemBase item) {
            _mutableSystemsList.Add(item);
            _systemsListDirtyFlag = true;
        }

        public void Clear() {
            _mutableSystemsList.Clear();
            _systemsListDirtyFlag = true;
        }

        bool IList.Contains(object value) {
            return ((IList)_mutableSystemsList).Contains(value);
        }

        public bool Contains(ComponentSystemBase item) {
            return _mutableSystemsList.Contains(item);
        }

        void ICollection.CopyTo(Array array, int index) {
            ((ICollection)_mutableSystemsList).CopyTo(array, index);
        }

        public void CopyTo(ComponentSystemBase[] array, int arrayIndex) {
            _mutableSystemsList.CopyTo(array, arrayIndex);
        }

        void IList.Remove(object value) {
            ((IList)_mutableSystemsList).Remove(value);
            _systemsListDirtyFlag = true;
        }

        public bool Remove(ComponentSystemBase item) {
            bool result = _mutableSystemsList.Remove(item);
            _systemsListDirtyFlag = true;
            return result;
        }

        int IList.IndexOf(object value) {
            return ((IList)_mutableSystemsList).IndexOf(value);
        }

        public int IndexOf(ComponentSystemBase item) {
            return _mutableSystemsList.IndexOf(item);
        }

        public int IndexOf(ComponentSystemBase item, int index) {
            return _mutableSystemsList.IndexOf(item, index);
        }

        public int IndexOf(ComponentSystemBase item, int index, int count) {
            return _mutableSystemsList.IndexOf(item, index, count);
        }

        void IList.Insert(int index, object value) {
            ((IList)_mutableSystemsList).Insert(index, value);
            _systemsListDirtyFlag = true;
        }

        public void Insert(int index, ComponentSystemBase item) {
            _mutableSystemsList.Insert(index, item);
            _systemsListDirtyFlag = true;
        }

        public void RemoveAt(int index) {
            _mutableSystemsList.RemoveAt(index);
            _systemsListDirtyFlag = true;
        }

        object IList.this[int index] {
            get => _mutableSystemsList[index];
            set {
                ((IList)_mutableSystemsList)[index] = value;
                _systemsListDirtyFlag = true;
            }
        }

        public ComponentSystemBase this[int index] {
            get => _mutableSystemsList[index];
            set {
                _mutableSystemsList[index] = value;
                _systemsListDirtyFlag = true;
            }
        }

        public ReadOnlyCollection<ComponentSystemBase> AsReadOnly() {
            return _mutableSystemsList.AsReadOnly();
        }

        public int BinarySearch(int index, int count, ComponentSystemBase item, IComparer<ComponentSystemBase> comparer) {
            return _mutableSystemsList.BinarySearch(index, count, item, comparer);
        }

        public int BinarySearch(ComponentSystemBase item) {
            return _mutableSystemsList.BinarySearch(item);
        }

        public int BinarySearch(ComponentSystemBase item, IComparer<ComponentSystemBase> comparer) {
            return _mutableSystemsList.BinarySearch(item, comparer);
        }

        public bool Exists(Predicate<ComponentSystemBase> match) {
            return _mutableSystemsList.Exists(match);
        }

        public ComponentSystemBase Find(Predicate<ComponentSystemBase> match) {
            return _mutableSystemsList.Find(match);
        }

        public List<ComponentSystemBase> FindAll(Predicate<ComponentSystemBase> match) {
            return _mutableSystemsList.FindAll(match);
        }

        public int FindIndex(Predicate<ComponentSystemBase> match) {
            return _mutableSystemsList.FindIndex(match);
        }

        public int FindIndex(int startIndex, Predicate<ComponentSystemBase> match) {
            return _mutableSystemsList.FindIndex(match);
        }

        public int FindIndex(int startIndex, int count, Predicate<ComponentSystemBase> match) {
            return _mutableSystemsList.FindIndex(startIndex, count, match);
        }

        public ComponentSystemBase FindLast(Predicate<ComponentSystemBase> match) {
            return _mutableSystemsList.FindLast(match);
        }

        public int FindLastIndex(Predicate<ComponentSystemBase> match) {
            return _mutableSystemsList.FindLastIndex(match);
        }

        public int FindLastIndex(int startIndex, Predicate<ComponentSystemBase> match) {
            return _mutableSystemsList.FindLastIndex(startIndex, match);
        }

        public int FindLastIndex(int startIndex, int count, Predicate<ComponentSystemBase> match) {
            return _mutableSystemsList.FindLastIndex(startIndex, count, match);
        }

        public void ForEach(Action<ComponentSystemBase> action) {
            _mutableSystemsList.ForEach(action);
        }

        public List<ComponentSystemBase> GetRange(int index, int count) {
            return _mutableSystemsList.GetRange(index, count);
        }

        public void InsertRange(int index, IEnumerable<ComponentSystemBase> collection) {
            _mutableSystemsList.InsertRange(index, collection);
            _systemsListDirtyFlag = true;
        }

        public int LastIndexOf(ComponentSystemBase item) {
            return _mutableSystemsList.LastIndexOf(item);
        }

        public int LastIndexOf(ComponentSystemBase item, int index) {
            return _mutableSystemsList.LastIndexOf(item, index);
        }

        public int LastIndexOf(ComponentSystemBase item, int index, int count) {
            return _mutableSystemsList.LastIndexOf(item, index, count);
        }

        public int RemoveAll(Predicate<ComponentSystemBase> match) {
            int result = _mutableSystemsList.RemoveAll(match);
            _systemsListDirtyFlag = true;
            return result;
        }

        public void RemoveRange(int index, int count) {
            _mutableSystemsList.RemoveRange(index, count);
            _systemsListDirtyFlag = true;
        }

        public void Reverse() {
            _mutableSystemsList.Reverse();
            _systemsListDirtyFlag = true;
        }

        public void Reverse(int index, int count) {
            _mutableSystemsList.Reverse(index, count);
            _systemsListDirtyFlag = true;
        }

        public void Sort() {
            _mutableSystemsList.Sort();
            _systemsListDirtyFlag = true;
        }

        public void Sort(IComparer<ComponentSystemBase> comparer) {
            _mutableSystemsList.Sort(comparer);
            _systemsListDirtyFlag = true;
        }

        public void Sort(int index, int count, IComparer<ComponentSystemBase> comparer) {
            _mutableSystemsList.Sort(index, count, comparer);
            _systemsListDirtyFlag = true;
        }

        public void Sort(Comparison<ComponentSystemBase> comparison) {
            _mutableSystemsList.Sort(comparison);
            _systemsListDirtyFlag = true;
        }

        public ComponentSystemBase[] ToArray() {
            return _mutableSystemsList.ToArray();
        }

        public void TrimExcess() {
            _mutableSystemsList.TrimExcess();
        }

        public bool TrueForAll(Predicate<ComponentSystemBase> match) {
            return _mutableSystemsList.TrueForAll(match);
        }
    }
}
