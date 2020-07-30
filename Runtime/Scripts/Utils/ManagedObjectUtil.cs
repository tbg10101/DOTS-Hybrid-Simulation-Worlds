using System.Collections.Generic;

namespace Software10101.DOTS.Utils {
    /// <summary>
    /// Used to attach managed objects to entity components. This keeps the data blittable but this utility cannot be accessed
    /// job-ified code.
    ///
    /// This should only be used for slow-moving data that cannot be stored in ECS, like references to GameObjects.
    /// </summary>
    public static class ReferenceTypeUtil {
        private static readonly List<object> ManagedObjects = new List<object>();
        private static readonly List<int> RemovedIndices = new List<int>();

        public static int Add(object o) {
            int index;

            int removedIndicesCount = RemovedIndices.Count;

            if (RemovedIndices.Count > 0) {
                int indexOfIndex = removedIndicesCount - 1;
                index = RemovedIndices[indexOfIndex];
                RemovedIndices.RemoveAt(indexOfIndex);
                ManagedObjects[index] = o;
            } else {
                index = ManagedObjects.Count;
                ManagedObjects.Add(o);
            }

            return index;
        }

        public static T Get<T>(int index) {
            return (T)ManagedObjects[index];
        }

        public static object Get(int index) {
            return ManagedObjects[index];
        }

        public static void Remove(int index) {
            ManagedObjects[index] = null;
            RemovedIndices.Add(index);
        }

        public static void Clear() {
            ManagedObjects.Clear();
            RemovedIndices.Clear();
        }
    }
}
