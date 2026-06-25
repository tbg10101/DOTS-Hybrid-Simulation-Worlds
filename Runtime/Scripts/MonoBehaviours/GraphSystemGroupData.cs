using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Software10101.DOTS.MonoBehaviours {
    [Serializable]
    public struct GraphSystemGroupData : IEquatable<GraphSystemGroupData> {
        public SystemNodeData[] Nodes;

        public static GraphSystemGroupData CreateEmpty() {
            GraphSystemGroupData newData = new() {
                Nodes = new []{ new SystemNodeData() }
            };

            return newData;
        }

        public IEnumerable<SystemTypeReference> GetExecutionOrder() {
            SystemNodeData root = default;
            bool rootFound = false;
            Dictionary<SystemTypeReference, SystemNodeData> nodesForSystemReferences = new();

            foreach (SystemNodeData node in Nodes) {
                if (ReferenceEquals(node.SystemReference, null)) {
                    // The first End of Group (root) node defines the group; ignore any extras.
                    if (!rootFound) {
                        root = node;
                        rootFound = true;
                    }

                    continue;
                }

                if (!node.SystemReference) {
                    Debug.LogWarning("System graph has missing system references!");
                    continue;
                }

                nodesForSystemReferences[node.SystemReference] = node;
            }

            List<SystemTypeReference> results = new();
            Queue<SystemTypeReference> fringe = new(rootFound && root.Dependencies != null
                ? root.Dependencies
                : Enumerable.Empty<SystemTypeReference>());

            while (fringe.Count > 0) {
                SystemTypeReference systemReference = fringe.Dequeue();

                if (!systemReference) {
                    continue;
                }

                int existingIndex = results.IndexOf(systemReference);

                if (existingIndex < 0) {
                    results.Insert(0, systemReference);
                } else if (existingIndex > 0) {
                    results.RemoveAt(existingIndex);
                    results.Insert(0, systemReference);
                }

                if (!nodesForSystemReferences.TryGetValue(systemReference, out SystemNodeData node)) {
                    continue;
                }

                if (node.Dependencies != null) {
                    foreach (SystemTypeReference systemTypeReference in node.Dependencies) {
                        fringe.Enqueue(systemTypeReference);
                    }
                }
            }

            return results;
        }

        [Serializable]
        public struct SystemNodeData : IEquatable<SystemNodeData> {
            public SystemTypeReference SystemReference;
            public Vector2 NodePosition;
            public SystemTypeReference[] Dependencies;

            public SystemNodeData(
                SystemTypeReference systemReference,
                Vector2 nodePosition,
                SystemTypeReference[] dependencies
            ) {
                SystemReference = systemReference;
                NodePosition = nodePosition;
                Dependencies = dependencies;
            }

            public bool Equals(SystemNodeData other) {
                return Equals(SystemReference, other.SystemReference) &&
                       NodePosition.Equals(other.NodePosition) &&
                       Dependencies.SequenceEqual(other.Dependencies);
            }

            public override bool Equals(object obj) {
                return obj is SystemNodeData other && Equals(other);
            }

            public override int GetHashCode() {
                unchecked {
                    int hashCode = (SystemReference != null ? SystemReference.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ NodePosition.GetHashCode();
                    hashCode = (hashCode * 397) ^ (Dependencies != null ? Dependencies.GetHashCode() : 0);
                    return hashCode;
                }
            }

            public static bool operator ==(SystemNodeData left, SystemNodeData right) {
                return left.Equals(right);
            }

            public static bool operator !=(SystemNodeData left, SystemNodeData right) {
                return !left.Equals(right);
            }
        }

        public bool Equals(GraphSystemGroupData other) {
            return Nodes.SequenceEqual(other.Nodes);
        }

        public override bool Equals(object obj) {
            return obj is GraphSystemGroupData other && Equals(other);
        }

        public override int GetHashCode() {
            return (Nodes != null ? Nodes.GetHashCode() : 0);
        }

        public static bool operator ==(GraphSystemGroupData left, GraphSystemGroupData right) {
            return left.Equals(right);
        }

        public static bool operator !=(GraphSystemGroupData left, GraphSystemGroupData right) {
            return !left.Equals(right);
        }
    }
}
