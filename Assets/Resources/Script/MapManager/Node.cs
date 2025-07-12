using System;
using System.Collections.Generic;
using UnityEngine;

namespace Map 
{
    /// Represents a single node in the map graph, including its position, type, blueprint, and connections to other nodes.
    /// Provides methods for managing incoming and outgoing connections, and for serialization to and from DTOs
    [Serializable]
    public class Node 
    {
        /// The grid position of this node
        public Vector2Int point;
        /// List of points representing nodes that connect to this node
        public List<Vector2Int> incoming = new List<Vector2Int>();
        /// List of points representing nodes that this node connects to
        public List<Vector2Int> outgoing = new List<Vector2Int>();
        /// The type of this node (e.g., enemy, shop, boss)
        public NodeType nodeType;
        /// The name of the blueprint used for this node's visual representatio
        public string blueprintName;
        /// The world or UI position of this node
        public Vector2 position;

        public Node() { }

        public Node(NodeType nodeType, string blueprintName, Vector2Int point) {
            this.nodeType = nodeType;
            this.blueprintName = blueprintName;
            this.point = point;
        }

        /// Adds a point to the list of incoming connections if it is not already present
        public void AddIncoming(Vector2Int p) {
            if (incoming.Contains(p))
                return;
            incoming.Add(p);
        }

        /// Adds a point to the list of outgoing connections if it is not already present
        public void AddOutgoing(Vector2Int p) {
            if (outgoing.Contains(p))
                return;
            outgoing.Add(p);
        }

        /// Removes all incoming connections matching the specified point
        public void RemoveIncoming(Vector2Int p) {
            incoming.RemoveAll(element => element.Equals(p));
        }

        /// Removes all outgoing connections matching the specified point
        public void RemoveOutgoing(Vector2Int p) {
            outgoing.RemoveAll(element => element.Equals(p));
        }

        /// Returns true if the node has no incoming or outgoing connections
        public bool HasNoConnections() {
            return incoming.Count == 0 && outgoing.Count == 0;
        }

        /// Converts this node to a data transfer object (DTO) for serializatio
        public NodeDTO ToDTO() => new NodeDTO(this);

        /// Creates a Node instance from a data transfer object (DTO)
        public static Node FromDTO(NodeDTO dto) => dto.ToNode();
    }
}