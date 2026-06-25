using Software10101.DOTS.MonoBehaviours;
using Unity.GraphToolkit.Editor;

namespace Software10101.DOTS.Editor.GraphEditor {
    /// <summary>
    /// The single sink node of a system group graph (the old "End of Group" root). Whatever connects into its
    /// "Depends On" input is treated as the set of systems the group's execution order is computed back from.
    /// </summary>
    [System.Serializable]
    [Node("End of Group")]
    [UseWithGraph(typeof(SystemGroupGraph))]
    internal class EndOfGroupNode : Node {
        public const string DependsOnPortName = "Depends On";

        protected override void OnDefinePorts(IPortDefinitionContext context) {
            context.AddInputPort<SystemTypeReference>(DependsOnPortName)
                .WithConnectorUI(PortConnectorUI.Arrowhead)
                .Build();
        }
    }
}
