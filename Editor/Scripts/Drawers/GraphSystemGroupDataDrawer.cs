using Software10101.DOTS.Editor.GraphEditor;
using Software10101.DOTS.MonoBehaviours;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Software10101.DOTS.Editor.Drawers {
    [CustomPropertyDrawer(typeof(WorldBehaviour.GraphSystemGroupData))]
    public class GraphSystemGroupDataDrawer : PropertyDrawer {
        public override VisualElement CreatePropertyGUI(SerializedProperty property) {
            VisualElement container = new();

            Button editButton = new(() => {
                OpenGraph(property);
            }) {
                text = $"Edit {property.displayName}"
            };

            container.Add(editButton);

            return container;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            EditorGUI.BeginProperty(position, label, property);

            if (GUILayout.Button($"Edit {property.displayName}")) {
                OpenGraph(property);
            }

            EditorGUI.EndProperty();
        }

        private static void OpenGraph(SerializedProperty property) {
            GraphSystemGroupEditorWindow window = EditorWindow.GetWindow<GraphSystemGroupEditorWindow>();

            WorldBehaviour wb = (WorldBehaviour)property.serializedObject.targetObject;
            window.Initialize(wb, property.name);
        }
    }
}
