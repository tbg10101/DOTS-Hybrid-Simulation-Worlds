using System;
using System.Reflection;
using Unity.Entities;
using UnityEngine.LowLevel;

namespace Software10101.DOTS.Utils {
    public static class PlayerLoopUtil {
        private static readonly Type DummyWrapperType = typeof(ScriptBehaviourUpdateOrder).Assembly
            .GetType("Unity.Entities.ScriptBehaviourUpdateOrder+DummyDelegateWrapper");

        private static readonly ConstructorInfo DummyWrapperConstructor = DummyWrapperType
            .GetConstructors(BindingFlags.Instance | BindingFlags.Public)[0];

        private static readonly FieldInfo DummyWrapperSystem = DummyWrapperType
            .GetField("m_System", BindingFlags.Instance | BindingFlags.NonPublic);

        private static readonly MethodInfo TriggerUpdateMethod = DummyWrapperType
            .GetMethod("TriggerUpdate", BindingFlags.Public | BindingFlags.Instance);

        /// <summary>
        /// Adds a system to the player loop in the same way that <see cref="Unity.Entities.ScriptBehaviourUpdateOrder"/> does.
        ///
        /// One difference is that this does not check to see if the system is already added. It is up to the users of this
        /// method to ensure that they do not add systems multiple times.
        /// </summary>
        /// <param name="parent">
        /// Must be one of the top level types already in the player loop such as
        /// <see cref="UnityEngine.PlayerLoop.FixedUpdate"/> or <see cref="UnityEngine.PlayerLoop.Update"/>.
        /// </param>
        /// <param name="system">
        /// The system to be added to the player loop.
        /// </param>
        /// <exception cref="Exception">
        /// An exception is thrown if the reflection components could not be initialized or the parent type was not in the player
        /// loop.
        /// </exception>
        public static void AddSubSystem(Type parent, ComponentSystemBase system) {
            ValidateReflectionComponents();

            PlayerLoopSystem currentPlayerLoop = PlayerLoop.GetCurrentPlayerLoop();

            for (int i = 0; i < currentPlayerLoop.subSystemList.Length; i++) {
                if (currentPlayerLoop.subSystemList[i].type == parent) {
                    PlayerLoopSystem[] oldSubSystems = currentPlayerLoop.subSystemList[i].subSystemList;

                    PlayerLoopSystem[] newSubSystems = new PlayerLoopSystem[oldSubSystems.Length + 1];
                    Array.Copy(oldSubSystems, newSubSystems, oldSubSystems.Length);

                    newSubSystems[oldSubSystems.Length].type = system.GetType();
                    newSubSystems[oldSubSystems.Length].updateDelegate = CreateDummyWrapper(system);

                    currentPlayerLoop.subSystemList[i].subSystemList = newSubSystems;

                    PlayerLoop.SetPlayerLoop(currentPlayerLoop);

                    return;
                }
            }

            throw new Exception($"Could not add {system.GetType().Name} to player loop. Are you sure {parent.Name} is in the player loop?");
        }

        /// <summary>
        /// Removes the first instance of the system from the specified parent.
        /// </summary>
        /// <param name="parent">
        /// Must be one of the top level types already in the player loop such as
        /// <see cref="UnityEngine.PlayerLoop.FixedUpdate"/> or <see cref="UnityEngine.PlayerLoop.Update"/>.
        /// </param>
        /// <param name="system">
        /// The system to be removed from the player loop.
        /// </param>
        public static void RemoveSubSystem(Type parent, ComponentSystemBase system) {
            ValidateReflectionComponents();

            PlayerLoopSystem currentPlayerLoop = PlayerLoop.GetCurrentPlayerLoop();

            for (int i = 0; i < currentPlayerLoop.subSystemList.Length; i++) {
                if (currentPlayerLoop.subSystemList[i].type == parent) {
                    PlayerLoopSystem[] oldSubSystems = currentPlayerLoop.subSystemList[i].subSystemList;
                    PlayerLoopSystem[] newSubSystems = new PlayerLoopSystem[oldSubSystems.Length - 1];

                    int index;
                    for (index = 0; index < oldSubSystems.Length; index++) {
                        if (oldSubSystems[index].updateDelegate == null) {
                            continue;
                        }

                        object target = oldSubSystems[index].updateDelegate.Target;
                        if (target.GetType() == DummyWrapperType && DummyWrapperSystem.GetValue(target) == system) {
                            break;
                        }
                    }

                    if (index < oldSubSystems.Length) {
                        Array.Copy(
                            oldSubSystems,
                            newSubSystems,
                            index);

                        Array.Copy(
                            oldSubSystems,
                            index + 1,
                            newSubSystems,
                            index,
                            oldSubSystems.Length - index - 1);
                    }

                    currentPlayerLoop.subSystemList[i].subSystemList = newSubSystems;

                    PlayerLoop.SetPlayerLoop(currentPlayerLoop);

                    return;
                }
            }
        }

        /// <summary>
        /// Resets the player loop to the default.
        /// </summary>
        public static void ResetPlayerLoop() {
            PlayerLoop.SetPlayerLoop(PlayerLoop.GetDefaultPlayerLoop());
        }

        private static PlayerLoopSystem.UpdateFunction CreateDummyWrapper(ComponentSystemBase system) {
            object dummyWrapper = DummyWrapperConstructor.Invoke(new object[] { system });

            Delegate del = TriggerUpdateMethod.CreateDelegate(typeof(PlayerLoopSystem.UpdateFunction), dummyWrapper);

            return (PlayerLoopSystem.UpdateFunction)del;
        }

        private static void ValidateReflectionComponents() {
            if (DummyWrapperType == null) {
                throw new Exception("Could not find DummyDelegateWrapper!");
            }

            if (DummyWrapperConstructor == null) {
                throw new Exception("Could not find DummyDelegateWrapper constructor!");
            }

            if (TriggerUpdateMethod == null) {
                throw new Exception("Could not find TriggerUpdate!");
            }

            if (DummyWrapperSystem == null) {
                throw new Exception("Could not find m_System!");
            }
        }
    }
}
