using System.Text;
using Software10101.DOTS.MonoBehaviours;
using UnityEditor;
using UnityEngine;

namespace Software10101.DOTS.Editor.GraphEditor {
    /// <summary>
    /// Inspector for <see cref="SystemGroupGraphAsset"/>. When the asset has a sibling authoring graph it offers to
    /// open it; when it does not (e.g. it was migrated from pre-6.5 embedded data) it warns the user and offers to
    /// create one, listing the systems that need to be re-added by hand.
    /// </summary>
    [CustomEditor(typeof(SystemGroupGraphAsset))]
    internal class SystemGroupGraphAssetEditor : UnityEditor.Editor {
        public override void OnInspectorGUI() {
            SystemGroupGraphAsset asset = (SystemGroupGraphAsset)target;

            if (SystemGroupGraphAssetActions.HasAuthoringGraph(asset)) {
                if (GUILayout.Button("Open Authoring Graph")) {
                    SystemGroupGraphAssetActions.OpenOrOfferToCreate(asset);
                }
            } else {
                EditorGUILayout.HelpBox(
                    "This system group has baked runtime data but no visual authoring graph — it was created or " +
                    "migrated from embedded data without one. The baked execution order below still drives the " +
                    "simulation at runtime.\n\n" +
                    "To edit it visually you must create a new authoring graph and re-add the system nodes listed " +
                    "below. Editing the new graph replaces this baked data.",
                    MessageType.Warning);

                if (GUILayout.Button("Create Authoring Graph…")) {
                    SystemGroupGraphAssetActions.OpenOrOfferToCreate(asset);
                }

                DrawBakedSystems(asset);
            }

            EditorGUILayout.Space();

            // The baked data is generated output; show it read-only so it is not hand-edited out of sync with a graph.
            using (new EditorGUI.DisabledScope(true)) {
                DrawDefaultInspector();
            }
        }

        private static void DrawBakedSystems(SystemGroupGraphAsset asset) {
            StringBuilder builder = new();

            foreach (GraphSystemGroupData.SystemNodeData node in asset.Nodes) {
                if (node.SystemReference) {
                    builder.AppendLine($"• {node.SystemReference.name}");
                }
            }

            EditorGUILayout.LabelField("Systems to recreate", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(builder.Length > 0 ? builder.ToString().TrimEnd() : "(none)", MessageType.None);
        }
    }
}
