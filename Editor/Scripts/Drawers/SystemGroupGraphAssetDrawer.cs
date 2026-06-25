using Software10101.DOTS.Editor.GraphEditor;
using Software10101.DOTS.MonoBehaviours;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Software10101.DOTS.Editor.Drawers {
    /// <summary>
    /// Draws a <see cref="SystemGroupGraphAsset"/> reference field as an object picker plus an "Edit Graph" button that
    /// opens the sibling <c>.sysgroup</c> authoring graph (offering to create one if the asset was migrated without it).
    /// </summary>
    [CustomPropertyDrawer(typeof(SystemGroupGraphAsset))]
    public class SystemGroupGraphAssetDrawer : PropertyDrawer {
        public override VisualElement CreatePropertyGUI(SerializedProperty property) {
            VisualElement container = new() {
                style = {
                    flexDirection = FlexDirection.Row,
                    alignItems = Align.Center
                }
            };

            ObjectField objectField = new(property.displayName) {
                objectType = typeof(SystemGroupGraphAsset),
                style = {
                    flexGrow = 1,
                    flexShrink = 1,
                    marginRight = 2
                }
            };
            // Align the label to the inspector's standard label column so the field + button match other rows.
            objectField.AddToClassList(BaseField<Object>.alignedFieldUssClassName);
            objectField.BindProperty(property);
            container.Add(objectField);

            Button editButton = new(() => OpenGraph(property)) {
                text = "Edit Graph",
                style = { flexShrink = 0 }
            };
            container.Add(editButton);

            return container;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            EditorGUI.BeginProperty(position, label, property);

            float buttonWidth = 80f;
            Rect fieldRect = new(position.x, position.y, position.width - buttonWidth - 2f, position.height);
            Rect buttonRect = new(position.xMax - buttonWidth, position.y, buttonWidth, position.height);

            EditorGUI.PropertyField(fieldRect, property, label);

            if (GUI.Button(buttonRect, "Edit Graph")) {
                OpenGraph(property);
            }

            EditorGUI.EndProperty();
        }

        private static void OpenGraph(SerializedProperty property) {
            if (property.objectReferenceValue is not SystemGroupGraphAsset asset) {
                EditorUtility.DisplayDialog(
                    "No graph assigned",
                    "Assign a System Group Graph asset first, or create one via Assets > Create > DOTS > System Group Graph.",
                    "OK");
                return;
            }

            SystemGroupGraphAssetActions.OpenOrOfferToCreate(asset);
        }
    }
}
