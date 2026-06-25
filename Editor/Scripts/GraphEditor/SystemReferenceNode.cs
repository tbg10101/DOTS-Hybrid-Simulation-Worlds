using Software10101.DOTS.MonoBehaviours;
using Unity.GraphToolkit.Editor;

namespace Software10101.DOTS.Editor.GraphEditor {
    /// <summary>
    /// A node wrapping a single <see cref="SystemTypeReference"/>. Its "Depends On" input wires to the output of every
    /// system that must run before it; its output feeds the systems (and the End of Group node) that depend on it.
    /// </summary>
    [System.Serializable]
    [Node("System")]
    [UseWithGraph(typeof(SystemGroupGraph))]
    internal class SystemReferenceNode : Node {
        public const string SystemOptionName = "System";
        public const string DependsOnPortName = "Depends On";
        public const string OutputPortName = "Dependents";

        protected override void OnDefineOptions(IOptionDefinitionContext context) {
            context.AddOption(SystemOptionName, typeof(SystemTypeReference)).Build();
        }

        protected override void OnDefinePorts(IPortDefinitionContext context) {
            // Reflect the assigned system in the node header; the option field is defined before ports, so its
            // deserialized value is already readable here and re-applied whenever the node is redefined.
            SystemTypeReference systemReference = GetSystemReference();
            Title = systemReference ? systemReference.name : "(Unassigned System)";

            context.AddInputPort<SystemTypeReference>(DependsOnPortName)
                .WithConnectorUI(PortConnectorUI.Arrowhead)
                .Build();
            context.AddOutputPort<SystemTypeReference>(OutputPortName)
                .Build();
        }

        public SystemTypeReference GetSystemReference() {
            INodeOption option = GetNodeOptionByName(SystemOptionName);
            return option != null && option.TryGetValue(out SystemTypeReference systemReference)
                ? systemReference
                : null;
        }
    }
}
