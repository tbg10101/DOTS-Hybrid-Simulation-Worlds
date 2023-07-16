using UnityEditor.Experimental.GraphView;

namespace Software10101.DOTS.Editor.GraphEditor {
    public class SystemNode : Node {
        public Port SelfOutput;
        public Port DependenciesInput;
        public int? InstanceId;
    }
}
