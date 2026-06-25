using System.Collections.Generic;
using Software10101.DOTS.MonoBehaviours;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Software10101.DOTS.Editor.Migration {
    /// <summary>
    /// One-time converter from the pre-6.5 embedded <see cref="GraphSystemGroupData"/> fields on
    /// <see cref="WorldBehaviour"/> into the new per-group <see cref="SystemGroupGraphAsset"/> references.
    /// <para>
    /// For each populated legacy group whose new reference is still empty, this creates a runtime asset holding the
    /// exact same data (so execution order is preserved bit-for-bit) and assigns it. Re-authoring a migrated group
    /// visually is done by creating a System Group Graph (Assets &gt; Create &gt; DOTS &gt; System Group Graph).
    /// </para>
    /// </summary>
    internal static class SystemGroupGraphMigration {
        private const string OutputRoot = "Assets/SystemGroups";

        // (legacy embedded field, new asset-reference field, friendly asset suffix)
        private static readonly (string Legacy, string New, string Suffix)[] Groups = {
            ("_startOfFrameGroup", "_startOfFrameGraph", "Start Of Frame"),
            ("_simResetGroup", "_simResetGraph", "Sim Reset"),
            ("_mainSimGroup", "_mainSimGraph", "Main Sim"),
            ("_presentationPreUpdateGroup", "_presentationPreUpdateGraph", "Presentation Pre Update"),
            ("_presentationPostUpdateGroup", "_presentationPostUpdateGraph", "Presentation Post Update"),
            ("_endOfFrameGroup", "_endOfFrameGraph", "End Of Frame"),
        };

        [MenuItem("Tools/DOTS/Migrate System Group Graphs In Open Scenes")]
        private static void Migrate() {
            int created = MigrateOpenScenes(out int worldCount);

            if (worldCount == 0) {
                EditorUtility.DisplayDialog("Migrate System Group Graphs", "No WorldBehaviour found in the open scenes.", "OK");
                return;
            }

            EditorUtility.DisplayDialog(
                "Migrate System Group Graphs",
                $"Migrated {created} group(s) across {worldCount} WorldBehaviour(s).",
                "OK");
        }

        /// <summary>
        /// Migrates every WorldBehaviour in the open scenes, creating and assigning runtime assets for any populated
        /// legacy group whose new reference is still empty. Returns the number of groups migrated; <paramref
        /// name="worldCount"/> receives the number of WorldBehaviours inspected.
        /// </summary>
        internal static int MigrateOpenScenes(out int worldCount) {
            WorldBehaviour[] worldBehaviours =
                Object.FindObjectsByType<WorldBehaviour>(FindObjectsInactive.Include);

            worldCount = worldBehaviours.Length;
            int created = 0;

            foreach (WorldBehaviour worldBehaviour in worldBehaviours) {
                created += MigrateWorldBehaviour(worldBehaviour);
            }

            if (created > 0) {
                AssetDatabase.SaveAssets();
            }

            return created;
        }

        private static int MigrateWorldBehaviour(WorldBehaviour worldBehaviour) {
            SerializedObject serializedObject = new(worldBehaviour);
            int created = 0;
            bool changed = false;

            foreach ((string legacyName, string newName, string suffix) in Groups) {
                SerializedProperty legacyProperty = serializedObject.FindProperty(legacyName);
                SerializedProperty newProperty = serializedObject.FindProperty(newName);

                if (legacyProperty == null || newProperty == null || newProperty.objectReferenceValue) {
                    continue;
                }

                GraphSystemGroupData legacyData = (GraphSystemGroupData)legacyProperty.boxedValue;

                if (!HasContent(legacyData)) {
                    continue;
                }

                SystemGroupGraphAsset asset = CreateAsset(worldBehaviour.name, suffix, legacyData);
                newProperty.objectReferenceValue = asset;
                created++;
                changed = true;
            }

            if (changed) {
                serializedObject.ApplyModifiedProperties();
                EditorSceneManager.MarkSceneDirty(GetScene(worldBehaviour));
            }

            return created;
        }

        private static bool HasContent(GraphSystemGroupData data) {
            if (data.Nodes == null) {
                return false;
            }

            foreach (GraphSystemGroupData.SystemNodeData node in data.Nodes) {
                if (node.SystemReference) {
                    return true;
                }
            }

            return false;
        }

        private static SystemGroupGraphAsset CreateAsset(string worldName, string suffix, GraphSystemGroupData data) {
            EnsureFolder(worldName);

            SystemGroupGraphAsset asset = ScriptableObject.CreateInstance<SystemGroupGraphAsset>();
            asset.SetData(data);

            string path = AssetDatabase.GenerateUniqueAssetPath($"{OutputRoot}/{worldName}/{worldName} - {suffix}.asset");
            AssetDatabase.CreateAsset(asset, path);

            return asset;
        }

        private static void EnsureFolder(string worldName) {
            if (!AssetDatabase.IsValidFolder(OutputRoot)) {
                AssetDatabase.CreateFolder("Assets", "SystemGroups");
            }

            string worldFolder = $"{OutputRoot}/{worldName}";
            if (!AssetDatabase.IsValidFolder(worldFolder)) {
                AssetDatabase.CreateFolder(OutputRoot, worldName);
            }
        }

        private static Scene GetScene(WorldBehaviour worldBehaviour) {
            return worldBehaviour.gameObject.scene;
        }
    }
}
