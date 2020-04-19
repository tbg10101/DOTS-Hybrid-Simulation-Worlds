using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Software10101.DOTS.Utils {
    public static class DotsUtil {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void Reset() {
            World.DisposeAllWorlds();

            WordStorage.Instance.Dispose();
            WordStorage.Instance = null;

            ScriptBehaviourUpdateOrder.UpdatePlayerLoop(null);
        }
    }
}
