using System.Collections.Generic;
using UnityEngine;

namespace Software10101.DOTS.MonoBehaviours {
    /// <summary>
    /// Runtime-readable, baked representation of a single system group's execution graph.
    /// <para>
    /// This asset is the stable contract consumed by <see cref="WorldBehaviour"/> at runtime and in player builds. The
    /// authoring experience (the GraphToolkit node editor) lives entirely in the editor assembly and bakes its result
    /// into the <see cref="Data"/> stored here, so the runtime never depends on any editor-only graph API.
    /// </para>
    /// </summary>
    public sealed class SystemGroupGraphAsset : ScriptableObject {
        [SerializeField]
        private GraphSystemGroupData _data = GraphSystemGroupData.CreateEmpty();

        public GraphSystemGroupData Data => _data;

        public IEnumerable<GraphSystemGroupData.SystemNodeData> Nodes => _data.Nodes;

        public IEnumerable<SystemTypeReference> GetExecutionOrder() {
            return _data.GetExecutionOrder();
        }

#if UNITY_EDITOR
        /// <summary>
        /// Editor-only entry point used by the GraphToolkit bake bridge to write the authored graph into this asset.
        /// </summary>
        public void SetData(GraphSystemGroupData data) {
            _data = data;
        }
#endif
    }
}
