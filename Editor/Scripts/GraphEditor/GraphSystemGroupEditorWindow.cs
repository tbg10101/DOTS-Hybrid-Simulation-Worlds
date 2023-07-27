using System;
using System.Collections.Generic;
using System.Linq;
using Software10101.DOTS.MonoBehaviours;
using Unity.Collections;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Software10101.DOTS.Editor.GraphEditor {
    public class GraphSystemGroupEditorWindow : EditorWindow {
        private Object _serializedObjectPrev;
        private bool _worldBehaviourPrevExist;
        private string _propertyNamePrev;

        private Object _serializedObject;
        private string _propertyName;

        private Toolbar _toolbar;
        private Label _loadedLabel;

        private GraphSystemGroupGraphView _graphView;

        private readonly List<string> _addNodeChoices = new();

        private bool _populateNodeCreationChoices = false;

        private void CreateGUI() {
            titleContent = new GUIContent("Graph System Group");

            _graphView = new GraphSystemGroupGraphView {
                name = "System Group Graph View"
            };
            _graphView.StretchToParentSize();
            _graphView.graphViewChanged += change => {
                if (change.elementsToRemove != null) {
                    _populateNodeCreationChoices = true;
                }

                return change;
            };
            rootVisualElement.Add(_graphView);

            Node selectGraphNode = new() {
                title = "Open a graph system group to use the editor."
            };

            selectGraphNode.SetPosition(new Rect(0, 20, 150, 100));
            selectGraphNode.RefreshExpandedState();
            selectGraphNode.RefreshPorts();

            _graphView.AddElement(selectGraphNode);
            _graphView.UpdateViewTransform(Vector3.zero, Vector3.one);
            _graphView.SetEnabled(false);

            _toolbar = new();

            PopulateNodeCreationChoices();
            DropdownField nodeCreateDropDown = new("Create Node", _addNodeChoices, -1);
            nodeCreateDropDown.RegisterValueChangedCallback(evt => {
                if (string.IsNullOrEmpty(evt.newValue)) {
                    return;
                }

                SystemTypeReference systemReference = AssetDatabase.LoadAssetAtPath<SystemTypeReference>(evt.newValue);
                _graphView.AddSystemNode(systemReference.GetInstanceID());
                nodeCreateDropDown.index = -1;
                PopulateNodeCreationChoices();
            });
            _toolbar.Add(nodeCreateDropDown);

            _loadedLabel = new Label("");
            _toolbar.Add(_loadedLabel);

            Button saveButton = new(Save) {
                text = "Save"
            };
            _toolbar.Add(saveButton);

            Button revertButton = new(Revert) {
                text = "Revert"
            };
            _toolbar.Add(revertButton);

            rootVisualElement.Add(_toolbar);
            _toolbar.SetEnabled(false);
        }

        private void OnFocus() {
            PopulateNodeCreationChoices();
        }

        private void PopulateNodeCreationChoices() {
            if (_graphView == null) {
                return;
            }

            _addNodeChoices.Clear();

            if (!_serializedObject) {
                return;
            }

            WorldBehaviour editingWorldBehaviour = (WorldBehaviour)_serializedObject;

            IEnumerable<int> existingInstanceIdsInGraph = _graphView.nodes
                .OfType<SystemNode>()
                .Where(node => node.InstanceId.HasValue)
                .Select(node => node.InstanceId.Value);

            IEnumerable<int> existingInstanceIdsInWorld = editingWorldBehaviour.GetAllConfiguredSystemReferences()
                .Where(entry => entry.Value != _propertyName)
                .Select(entry => entry.Key.GetInstanceID());

            int[] existingInstanceIds = existingInstanceIdsInGraph
                .Union(existingInstanceIdsInWorld)
                .ToArray();

            NativeArray<int> existingInstanceIdsNative = new(existingInstanceIds.Length, Allocator.Persistent);
            for (int i = 0; i < existingInstanceIds.Length; i++) {
                int existingInstanceId = existingInstanceIds[i];
                existingInstanceIdsNative[i] = existingInstanceId;
            }

            NativeArray<GUID> existingNodeGuidsNative = new(existingInstanceIds.Length, Allocator.Persistent);

            AssetDatabase.InstanceIDsToGUIDs(existingInstanceIdsNative, existingNodeGuidsNative);

            existingInstanceIdsNative.Dispose();

            HashSet<string> existingNodeGuids = new();
            for (int i = 0; i < existingNodeGuidsNative.Length; i++) {
                GUID existingGuid = existingNodeGuidsNative[i];
                existingNodeGuids.Add(existingGuid.ToString());
            }

            _addNodeChoices.AddRange(
                AssetDatabase.FindAssets($"t:{nameof(SystemTypeReference)}")
                    .Where(guid => !existingNodeGuids.Contains(guid))
                    .Select(AssetDatabase.GUIDToAssetPath)
                    .Select(path => $"{path}")
            );

            existingNodeGuidsNative.Dispose();
        }

        private void OnGUI() {
            if (_graphView == null) {
                return;
            }

            if (_populateNodeCreationChoices) {
                _populateNodeCreationChoices = false;
                PopulateNodeCreationChoices();
            }

            if (EditorPrefs.HasKey("graph_editing_world_instance")) {
                int instanceId = EditorPrefs.GetInt("graph_editing_world_instance");
                _serializedObject = (WorldBehaviour)EditorUtility.InstanceIDToObject(instanceId);

                if (_serializedObject) {
                    if (EditorPrefs.HasKey("graph_editing_save_callback")) {
                        _propertyName = EditorPrefs.GetString("graph_editing_save_callback");
                    }
                }
            }

            if (_serializedObjectPrev != _serializedObject || _propertyNamePrev != _propertyName || _worldBehaviourPrevExist != _serializedObject) {
                if (!_serializedObject) {
                    _graphView.DeleteElements(_graphView.graphElements);

                    _toolbar.SetEnabled(false);

                    Node selectGraphNode = new() {
                        title = "Open a graph system group to use the editor."
                    };

                    selectGraphNode.SetPosition(new Rect(0, 20, 150, 100));
                    selectGraphNode.RefreshExpandedState();
                    selectGraphNode.RefreshPorts();

                    _graphView.AddElement(selectGraphNode);
                    _graphView.UpdateViewTransform(Vector3.zero, Vector3.one);
                    _graphView.SetEnabled(false);
                } else {
                    SaveChangesDialog();

                    _graphView.DeleteElements(_graphView.graphElements);

                    _toolbar.SetEnabled(true);
                    _graphView.SetEnabled(true);

                    Load();
                }
            } else if (_serializedObject && _graphView.enabledSelf == false) {
                SaveChangesDialog();

                _graphView.DeleteElements(_graphView.graphElements);

                _toolbar.SetEnabled(true);
                _graphView.SetEnabled(true);

                Load();
            }

            if (_serializedObject) {
                string dirtyFlag = _graphView.Dirty ? " *" : "";
                _loadedLabel.text = $"{_serializedObject.name} - {_propertyName}{dirtyFlag}";
            } else {
                _loadedLabel.text = "";
            }

            _worldBehaviourPrevExist = _serializedObject;
            _serializedObjectPrev = _serializedObject;
            _propertyNamePrev = _propertyName;
        }

        public void Initialize(Object serializedObject, string propertyName) {
            _serializedObject = serializedObject;
            _propertyName = propertyName;

            EditorPrefs.SetInt("graph_editing_world_instance", serializedObject.GetInstanceID());
            EditorPrefs.SetString("graph_editing_save_callback", propertyName);
        }

        private void OnDisable() {
            SaveChangesDialog();

            rootVisualElement.Clear();
            _serializedObject = null;
            _propertyName = null;
        }

        private void SaveChangesDialog() {
            if (_graphView.Dirty && EditorUtility.DisplayDialog("Save changes?",
                    "The graph contains unsaved changes. Would you like to save them?",
                    "Save",
                    "Discard")) {

                Save();
            }
        }

        private void Save() {
            SerializedObject so = new(_serializedObject);

            WorldBehaviour.GraphSystemGroupData graphData = WorldBehaviour.GraphSystemGroupData.CreateEmpty();

            HashSet<int> rootDependencies = new();
            Dictionary<int, HashSet<int>> dependencies = new();

            _graphView.edges.ForEach(edge => {
                int? dependent = ((SystemNode)edge.input.node).InstanceId;
                // ReSharper disable once PossibleInvalidOperationException // not possible - root is never a dependency
                int dependency = ((SystemNode)edge.output.node).InstanceId.Value;

                HashSet<int> existingDependencies;

                if (!dependent.HasValue) {
                    existingDependencies = rootDependencies;
                } else if (!dependencies.TryGetValue(dependent.Value, out existingDependencies)) {
                    existingDependencies = new HashSet<int>();
                    dependencies[dependent.Value] = existingDependencies;
                }

                existingDependencies.Add(dependency);
            });

            graphData.Nodes = _graphView.nodes
                .OfType<SystemNode>()
                .Select(systemNode => new WorldBehaviour.GraphSystemGroupData.SystemNodeData(
                    systemNode.InstanceId.HasValue
                        ? (SystemTypeReference)EditorUtility.InstanceIDToObject(systemNode.InstanceId.Value)
                        : null,
                    systemNode.GetPosition().position,
                    systemNode.InstanceId.HasValue
                        ? dependencies.TryGetValue(systemNode.InstanceId.Value, out HashSet<int> nodeDependencies)
                            ? nodeDependencies?
                                .Select(instanceId => (SystemTypeReference)EditorUtility.InstanceIDToObject(instanceId))
                                .ToArray() ?? Array.Empty<SystemTypeReference>()
                            : Array.Empty<SystemTypeReference>()
                        : rootDependencies
                            .Select(instanceId => (SystemTypeReference)EditorUtility.InstanceIDToObject(instanceId))
                            .ToArray()))
                .ToArray();

            so.FindProperty(_propertyName).boxedValue = graphData;
            so.ApplyModifiedProperties();

            _graphView.Dirty = false;

            PopulateNodeCreationChoices();
        }

        private void Load() {
            SerializedObject so = new(_serializedObject);
            object graphDataRaw = so.FindProperty(_propertyName).boxedValue;
            WorldBehaviour.GraphSystemGroupData graphData = (WorldBehaviour.GraphSystemGroupData)graphDataRaw;

            SystemNode rootNode = null;
            Dictionary<int, SystemNode> nodesByInstanceId = new();

            foreach (WorldBehaviour.GraphSystemGroupData.SystemNodeData systemNodeData in graphData.Nodes) {
                if (systemNodeData.SystemReference) {
                    int instanceId = systemNodeData.SystemReference.GetInstanceID();

                    SystemNode node = _graphView.AddSystemNode(instanceId, systemNodeData.NodePosition);
                    nodesByInstanceId[instanceId] = node;
                } else {
                    rootNode = _graphView.AddSystemNode(null, systemNodeData.NodePosition);
                }
            }

            foreach (WorldBehaviour.GraphSystemGroupData.SystemNodeData systemNodeData in graphData.Nodes) {
                if (systemNodeData.SystemReference) {
                    SystemNode dependent = nodesByInstanceId[systemNodeData.SystemReference.GetInstanceID()];

                    foreach (SystemTypeReference systemTypeReference in systemNodeData.Dependencies) {
                        _graphView.AddSystemDependency(dependent, nodesByInstanceId[systemTypeReference.GetInstanceID()]);
                    }
                } else {
                    foreach (SystemTypeReference systemTypeReference in systemNodeData.Dependencies) {
                        _graphView.AddSystemDependency(rootNode, nodesByInstanceId[systemTypeReference.GetInstanceID()]);
                    }
                }
            }

            _graphView.Dirty = false;
        }

        private void Revert() {
            if (_graphView.Dirty && EditorUtility.DisplayDialog("Revert changes?",
                    "The graph contains unsaved changes. Are you sure you want to revert them?",
                    "Cancel",
                    "Revert")) {

                return;
            }

            _graphView.DeleteElements(_graphView.graphElements);

            _toolbar.SetEnabled(true);
            _graphView.SetEnabled(true);

            Load();
        }
    }
}
