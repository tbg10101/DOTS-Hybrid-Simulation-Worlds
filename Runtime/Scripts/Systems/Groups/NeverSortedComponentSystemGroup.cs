using System.Reflection;
using Unity.Entities;

namespace Software10101.DOTS.Systems.Groups {
    /// <summary>
    /// Groups MUST inherit from ComponentSystemGroup in order for the Entity debugger to show the contained systems.
    /// </summary>
    public abstract class NeverSortedComponentSystemGroup : ComponentSystemGroup {
        private static readonly FieldInfo DirtyFlag = typeof(ComponentSystemGroup).GetField(
            "m_systemSortDirty",
            BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        /// This is the same as ComponentSystemGroup except it never sorts m_systemsToUpdate.
        /// </summary>
        public override void SortSystemUpdateList() {
            bool dirty = (bool)DirtyFlag.GetValue(this);

            if (!dirty) {
                return;
            }

            DirtyFlag.SetValue(this, false);

            if (m_systemsToRemove.Count > 0) {
                foreach (var sys in m_systemsToRemove) {
                    m_systemsToUpdate.Remove(sys);
                }

                m_systemsToRemove.Clear();
            }

            foreach (var sys in m_systemsToUpdate) {
                if (TypeManager.IsSystemAGroup(sys.GetType())) {
                    ((ComponentSystemGroup)sys).SortSystemUpdateList();
                }
            }

            // ComponentSystemGroup would do the sort here
        }
    }
}
