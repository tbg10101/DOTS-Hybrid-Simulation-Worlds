using Software10101.DOTS.Editor.GraphEditor;
using Software10101.DOTS.MonoBehaviours;
using UnityEditor;
using UnityEngine.UIElements;

namespace Software10101.DOTS.Editor.Drawers {
    [CustomPropertyDrawer(typeof(WorldBehaviour.GraphSystemGroupData))]
    public class GraphSystemGroupDataDrawer : PropertyDrawer {
        public override VisualElement CreatePropertyGUI(SerializedProperty property) {
            // Create property container element.
            VisualElement container = new();

            // Create property fields.
            Button editButton = new(() => {
                GraphSystemGroupEditorWindow window = EditorWindow.GetWindow<GraphSystemGroupEditorWindow>();

                WorldBehaviour wb = (WorldBehaviour)property.serializedObject.targetObject;
                window.Initialize(wb, property.name);
            }) {
                text = $"Edit {property.displayName}"
            };

            // Add fields to the container.
            container.Add(editButton);

            return container;
        }
    }
}
