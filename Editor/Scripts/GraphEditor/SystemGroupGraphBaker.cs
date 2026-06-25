using System.Collections.Generic;
using System.Linq;
using Software10101.DOTS.MonoBehaviours;
using Unity.GraphToolkit.Editor;
using UnityEditor;
using UnityEngine;

namespace Software10101.DOTS.Editor.GraphEditor {
    /// <summary>
    /// Traverses a <see cref="SystemGroupGraph"/> and bakes it into the runtime <see cref="SystemGroupGraphAsset"/>
    /// that <see cref="WorldBehaviour"/> consumes. Also surfaces authoring problems (missing references, duplicate
    /// systems) as graph errors/warnings so they appear as badges on the offending nodes.
    /// </summary>
    internal static class SystemGroupGraphBaker {
        public static void Bake(SystemGroupGraph graph, SystemGroupGraphAsset asset, GraphLogger logger) {
            List<GraphSystemGroupData.SystemNodeData> nodeData = new();

            // Guard against the same system appearing twice: the runtime execution-order builder keys nodes by their
            // SystemTypeReference, so duplicates would conflict. We keep the first and flag the rest.
            HashSet<SystemTypeReference> seenSystems = new();

            // GraphToolkit cannot hide the End of Group node from the Add Node menu (a node that is addable is always
            // listed). Instead we allow only one: the first is baked as the group's sink and any extras get an in-graph
            // warning marker and are ignored here (and defensively at runtime).
            bool rootBaked = false;

            foreach (INode node in graph.GetNodes()) {
                switch (node) {
                    case EndOfGroupNode endOfGroupNode:
                        if (rootBaked) {
                            logger?.LogWarning("Extra End of Group node is ignored; only one per group is used. Delete the duplicate.", node);
                            continue;
                        }

                        rootBaked = true;
                        nodeData.Add(new GraphSystemGroupData.SystemNodeData(
                            null,
                            node.Position,
                            GetDependencies(endOfGroupNode, logger)));
                        break;
                    case SystemReferenceNode systemReferenceNode:
                        SystemTypeReference systemReference = systemReferenceNode.GetSystemReference();

                        if (!systemReference) {
                            logger?.LogError("System node has no System Reference assigned.", node);
                            continue;
                        }

                        if (!seenSystems.Add(systemReference)) {
                            logger?.LogError($"System '{systemReference.name}' is used by more than one node in this group.", node);
                            continue;
                        }

                        nodeData.Add(new GraphSystemGroupData.SystemNodeData(
                            systemReference,
                            node.Position,
                            GetDependencies(systemReferenceNode, logger)));
                        break;
                }
            }

            if (!rootBaked) {
                logger?.LogWarning("System group graph has no End of Group node; nothing will run.");
            }

            GraphSystemGroupData data = new() {
                Nodes = nodeData.ToArray()
            };

            // Update the in-memory asset and mark it dirty only. Persisting (AssetDatabase.SaveAssetIfDirty) must not
            // happen here: Bake runs inside OnGraphChanged, and saving from there triggers asset postprocessing that
            // re-enters GraphToolkit's graph-processing observer and throws. The caller persists at a safe time.
            asset.SetData(data);
            EditorUtility.SetDirty(asset);
        }

        /// <summary>
        /// Resolves the systems wired into a node's "Depends On" input. Each connected port belongs to the output of a
        /// dependency node; only <see cref="SystemReferenceNode"/>s contribute a system reference.
        /// </summary>
        private static SystemTypeReference[] GetDependencies(Node node, GraphLogger logger) {
            IPort inputPort = node.GetInputPortByName(SystemReferenceNode.DependsOnPortName);

            if (inputPort == null) {
                return System.Array.Empty<SystemTypeReference>();
            }

            List<IPort> connectedPorts = new();
            inputPort.GetConnectedPorts(connectedPorts);

            List<SystemTypeReference> dependencies = new();

            foreach (IPort connectedPort in connectedPorts) {
                if (connectedPort.GetNode() is SystemReferenceNode dependencyNode) {
                    SystemTypeReference dependency = dependencyNode.GetSystemReference();

                    if (dependency) {
                        dependencies.Add(dependency);
                    }
                }
            }

            return dependencies.ToArray();
        }
    }
}
