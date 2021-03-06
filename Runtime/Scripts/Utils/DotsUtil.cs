using Software10101.DOTS.MonoBehaviours;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Software10101.DOTS.Utils {
    public static class DotsUtil {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void Reset() {
            World.DisposeAllWorlds();

            WordStorage.Clear();

            PlayerLoopUtil.ResetPlayerLoop();

            ReferenceTypeUtil.Clear();

            ManagedMonoBehaviour.DestroyAll();

            TimeUtil.TimeOffset = 0;
        }
    }
}
