using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Software10101.DOTS.Editor.GraphEditor {
    // TODO add node creation from context menu
    // TODO add system reference selection context menu item
    public class GraphSystemGroupGraphView : GraphView {
        public bool Dirty = false;

        public GraphSystemGroupGraphView() {
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            graphViewChanged += OnGraphChanged;
        }

        private GraphViewChange OnGraphChanged(GraphViewChange change) {
            Dirty = true;
            return change;
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter) {
            // used to find circular dependencies
            Dictionary<Port, List<Node>> oppositeDirectionPortLinks = new();

            switch (startPort.direction) {
                case Direction.Input:
                    edges.ForEach(edge => {
                        if (edge.output == null) {
                            return;
                        }

                        if (!oppositeDirectionPortLinks.TryGetValue(edge.output, out List<Node> inputPorts)) {
                            inputPorts = new List<Node>();
                            oppositeDirectionPortLinks[edge.output] = inputPorts;
                        }

                        inputPorts.Add(edge.input.node);
                    });
                    break;
                case Direction.Output:
                    edges.ForEach(edge => {
                        if (edge.input == null) {
                            return;
                        }

                        if (!oppositeDirectionPortLinks.TryGetValue(edge.input, out List<Node> outputPorts)) {
                            outputPorts = new List<Node>();
                            oppositeDirectionPortLinks[edge.input] = outputPorts;
                        }

                        outputPorts.Add(edge.output.node);
                    });
                    break;
            }

            return ports
                .Where(port => startPort.node != port.node) // disallow linking to self
                .Where(port => startPort.direction != port.direction) // disallow input-input and output-output
                .Where(port => startPort.portType == port.portType) // ensure port type matching
                .Where(port => {
                    // disallow circular dependencies

                    Node targetNode = port.node;

                    Queue<Node> fringe = new();
                    fringe.Enqueue(startPort.node);

                    while (fringe.Count > 0) {
                        Node currentNode = fringe.Dequeue();

                        VisualElement oppositePortContainer = startPort.direction == Direction.Input
                            ? currentNode.outputContainer
                            : currentNode.inputContainer;

                        foreach (Port oppositePort in oppositePortContainer.Children().OfType<Port>()) {
                            if (oppositeDirectionPortLinks.TryGetValue(oppositePort, out List<Node> otherNodes)) {
                                foreach (Node otherNode in otherNodes) {
                                    if (otherNode == targetNode) {
                                        return false; // circular dependency
                                    }

                                    fringe.Enqueue(otherNode);
                                }
                            }
                        }
                    }

                    return true;
                })
                .ToList();
        }

        public SystemNode AddSystemNode(int? instanceId, Vector2? position = null) {
            SystemNode systemNode = new()  {
                title = instanceId.HasValue ? EditorUtility.InstanceIDToObject(instanceId.Value).name : "End of Group",
                InstanceId = instanceId
            };

            Port inputPort = systemNode.InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(object));
            inputPort.portName = "Depends On";
            systemNode.inputContainer.Add(inputPort);
            systemNode.DependenciesInput = inputPort;

            if (instanceId.HasValue) {
                Port outputPort = systemNode.InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi,
                    typeof(object));
                outputPort.portName = "";
                systemNode.outputContainer.Add(outputPort);
                systemNode.SelfOutput = outputPort;
            }

            systemNode.RefreshExpandedState();
            systemNode.RefreshPorts();

            Vector2 viewportRect = viewTransform.position;
            Vector2 nodePosition = position ?? (-viewportRect + 0.5f * contentRect.size);
            systemNode.SetPosition(new Rect(nodePosition.x, nodePosition.y, 250, 100));

            AddElement(systemNode);

            ClearSelection();
            AddToSelection(systemNode);

            return systemNode;
        }

        public void AddSystemDependency(SystemNode dependent, SystemNode dependency) {
            Edge edge = dependent.DependenciesInput.ConnectTo(dependency.SelfOutput);
            AddElement(edge);
        }
    }
}
