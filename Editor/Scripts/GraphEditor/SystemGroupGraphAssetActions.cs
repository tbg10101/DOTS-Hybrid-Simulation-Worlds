using System.IO;
using Software10101.DOTS.MonoBehaviours;
using UnityEditor;
using UnityEditor.Callbacks;
using Object = UnityEngine.Object;

namespace Software10101.DOTS.Editor.GraphEditor {
    /// <summary>
    /// Shared actions for opening a <see cref="SystemGroupGraphAsset"/>'s authoring graph, including the recovery flow
    /// for assets that have baked runtime data but no visual graph (typically migrated from pre-6.5 embedded data).
    /// </summary>
    internal static class SystemGroupGraphAssetActions {
        public static string GetAuthoringGraphPath(SystemGroupGraphAsset asset) {
            string assetPath = AssetDatabase.GetAssetPath(asset);
            return string.IsNullOrEmpty(assetPath)
                ? null
                : Path.ChangeExtension(assetPath, "." + SystemGroupGraph.AssetExtension);
        }

        public static bool HasAuthoringGraph(SystemGroupGraphAsset asset) {
            string graphPath = GetAuthoringGraphPath(asset);
            return !string.IsNullOrEmpty(graphPath) && AssetDatabase.LoadAssetAtPath<Object>(graphPath);
        }

        /// <summary>
        /// Opens the asset's authoring graph if one exists; otherwise warns the user that the group has no visual graph
        /// (it was created or migrated without one) and offers to create a fresh one to rebuild it in.
        /// </summary>
        public static void OpenOrOfferToCreate(SystemGroupGraphAsset asset) {
            if (!asset) {
                return;
            }

            string graphPath = GetAuthoringGraphPath(asset);
            if (string.IsNullOrEmpty(graphPath)) {
                return;
            }

            Object graphObject = AssetDatabase.LoadAssetAtPath<Object>(graphPath);
            if (graphObject) {
                AssetDatabase.OpenAsset(graphObject);
                return;
            }

            bool create = EditorUtility.DisplayDialog(
                "No authoring graph",
                $"'{asset.name}' has baked runtime data but no visual authoring graph — it was created or migrated " +
                "from embedded data without one. The baked execution order still drives the simulation at runtime.\n\n" +
                "Create a new, empty graph to rebuild it in? The baked data is preserved until you start editing the " +
                "new graph, at which point the graph's contents replace it, so you will need to re-add the system nodes.",
                "Create Graph",
                "Cancel");

            if (create) {
                CreateAndOpenAuthoringGraph(asset);
            }
        }

        /// <summary>
        /// Creates an empty authoring graph next to the asset and opens it. Deliberately does not bake, so the asset's
        /// existing (e.g. migrated) runtime data survives until the user actually edits the new graph.
        /// </summary>
        public static void CreateAndOpenAuthoringGraph(SystemGroupGraphAsset asset) {
            string graphPath = GetAuthoringGraphPath(asset);
            if (string.IsNullOrEmpty(graphPath) || AssetDatabase.LoadAssetAtPath<Object>(graphPath)) {
                return;
            }

            SystemGroupGraph.CreateSeededGraph(graphPath);

            Object graphObject = AssetDatabase.LoadAssetAtPath<Object>(graphPath);
            Selection.activeObject = graphObject;

            if (graphObject) {
                AssetDatabase.OpenAsset(graphObject);
            }
        }

        // Double-clicking a SystemGroupGraphAsset opens its authoring graph (or offers to create one), rather than just
        // selecting the runtime asset.
        [OnOpenAsset]
        private static bool OnOpenAsset(UnityEngine.EntityId entityId, int line) {
            if (EditorUtility.EntityIdToObject(entityId) is SystemGroupGraphAsset asset) {
                OpenOrOfferToCreate(asset);
                return true;
            }

            return false;
        }
    }
}
