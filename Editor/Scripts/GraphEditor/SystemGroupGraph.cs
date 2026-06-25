using System.IO;
using Software10101.DOTS.MonoBehaviours;
using Unity.GraphToolkit.Editor;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Software10101.DOTS.Editor.GraphEditor {
    /// <summary>
    /// GraphToolkit authoring graph for a single system group. The graph is the editing surface only; on every change
    /// it bakes the authored nodes/wires into the linked runtime <see cref="SystemGroupGraphAsset"/> (the sibling
    /// <c>.asset</c> file next to this graph), which is what <see cref="WorldBehaviour"/> references and reads at
    /// runtime.
    /// </summary>
    [System.Serializable]
    [Graph(AssetExtension)]
    internal class SystemGroupGraph : Graph {
        public const string AssetExtension = "sysgroup";

        [MenuItem("Assets/Create/DOTS/System Group Graph")]
        private static void CreateAsset() {
            string folder = GetSelectedFolder();
            string graphPath = AssetDatabase.GenerateUniqueAssetPath($"{folder}/New System Group Graph.{AssetExtension}");

            SystemGroupGraph graph = CreateSeededGraph(graphPath);

            // Fresh graph: bake the (empty) sibling runtime asset immediately so OnGraphChanged only ever updates it.
            SystemGroupGraphAsset asset = graph.GetOrCreateBakedAsset();
            if (asset) {
                SystemGroupGraphBaker.Bake(graph, asset, null);
                AssetDatabase.SaveAssetIfDirty(asset);
            }

            Object graphObject = AssetDatabase.LoadAssetAtPath<Object>(graphPath);
            Selection.activeObject = graphObject;
            EditorGUIUtility.PingObject(graphObject);
        }

        /// <summary>
        /// Creates a graph asset at <paramref name="graphPath"/> seeded with the single End of Group sink. The graph
        /// cannot be modified inside OnEnable, so creation time is the moment a new graph gets its default content.
        /// This does NOT bake the sibling runtime asset, so it is safe to call against an existing (e.g. migrated)
        /// <see cref="SystemGroupGraphAsset"/> whose baked data should survive until the new graph is actually edited.
        /// </summary>
        internal static SystemGroupGraph CreateSeededGraph(string graphPath) {
            SystemGroupGraph graph = GraphDatabase.CreateGraph<SystemGroupGraph>(graphPath);
            graph.AddNode(new EndOfGroupNode());
            GraphDatabase.SaveGraph(graph);
            return graph;
        }

        public override void OnGraphChanged(GraphLogger logger) {
            string bakedPath = GetBakedAssetPath();

            if (string.IsNullOrEmpty(bakedPath)) {
                return;
            }

            SystemGroupGraphAsset asset = AssetDatabase.LoadAssetAtPath<SystemGroupGraphAsset>(bakedPath);

            if (asset) {
                // Update the in-memory asset and surface validation as node badges in-cycle, then persist on the next
                // editor tick. Writing to the AssetDatabase here (during graph processing) re-enters GraphToolkit's
                // observer and throws.
                SystemGroupGraphBaker.Bake(this, asset, logger);
                ScheduleSave(bakedPath);
            } else {
                // No sibling runtime asset yet (rare): create, bake and save off the graph-processing observer.
                EditorApplication.delayCall += () => {
                    SystemGroupGraphAsset created = GetOrCreateBakedAsset();

                    if (created) {
                        SystemGroupGraphBaker.Bake(this, created, null);
                        AssetDatabase.SaveAssetIfDirty(created);
                    }
                };
            }
        }

        private static void ScheduleSave(string bakedPath) {
            EditorApplication.delayCall += () => {
                SystemGroupGraphAsset asset = AssetDatabase.LoadAssetAtPath<SystemGroupGraphAsset>(bakedPath);

                if (asset) {
                    AssetDatabase.SaveAssetIfDirty(asset);
                }
            };
        }

        /// <summary>
        /// Resolves the runtime <see cref="SystemGroupGraphAsset"/> that mirrors this graph: a <c>.asset</c> file with
        /// the same name, in the same folder. Created on demand so a freshly authored graph always has a bake target.
        /// Must only be called outside OnGraphChanged, since it may write to the AssetDatabase.
        /// </summary>
        internal SystemGroupGraphAsset GetOrCreateBakedAsset() {
            string bakedPath = GetBakedAssetPath();

            if (string.IsNullOrEmpty(bakedPath)) {
                return null;
            }

            SystemGroupGraphAsset asset = AssetDatabase.LoadAssetAtPath<SystemGroupGraphAsset>(bakedPath);

            if (!asset) {
                asset = ScriptableObject.CreateInstance<SystemGroupGraphAsset>();
                AssetDatabase.CreateAsset(asset, bakedPath);
            }

            return asset;
        }

        private string GetBakedAssetPath() {
            string graphPath = GraphDatabase.GetGraphAssetPath(this);
            return string.IsNullOrEmpty(graphPath) ? null : Path.ChangeExtension(graphPath, ".asset");
        }

        private static string GetSelectedFolder() {
            foreach (Object obj in Selection.GetFiltered<Object>(SelectionMode.Assets)) {
                string path = AssetDatabase.GetAssetPath(obj);

                if (AssetDatabase.IsValidFolder(path)) {
                    return path;
                }

                if (!string.IsNullOrEmpty(path)) {
                    return Path.GetDirectoryName(path);
                }
            }

            return "Assets";
        }
    }
}
