using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Software10101.DOTS.Editor.GraphEditor {
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
            return ports
                .Where(port => startPort.node != port.node)
                .Where(port => startPort.direction != port.direction)
                .Where(port => startPort.portType == port.portType)
                .ToList();

            // TODO prevent circular dependencies
        }

        public SystemNode AddSystemNode(int? instanceId, Vector2? position = null) {
            SystemNode systemNode = new()  {
                title = instanceId.HasValue ? EditorUtility.InstanceIDToObject(instanceId.Value).name : "Root",
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
            Edge edge = new() {
                input = dependent.DependenciesInput,
                output = dependency.SelfOutput
            };

            AddElement(edge);
        }
    }
}
