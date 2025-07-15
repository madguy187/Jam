using System;
using System.Collections.Generic;
using UnityEngine;

namespace Map 
{
    /// Data Transfer Object (DTO) for serializing and deserializing node data in a map.
    /// Contains information about the node's position, connections, type, blueprint, 
    /// and display position.
    /// Provides conversion between the Node and NodeDTO representations.
    [Serializable]
    public class NodeDTO 
    {
        /// The grid position of the node
        public Vector2Int point;
        /// List of grid positions representing incoming connections to this node
        public List<Vector2Int> incoming = new List<Vector2Int>();
        /// List of grid positions representing outgoing connections from this node
        public List<Vector2Int> outgoing = new List<Vector2Int>();
        /// The type of the node, stored as a string (typically from an enum)
        public string nodeType;
        /// The name of the blueprint associated with this node
        public string blueprintName;
        /// The display position of the node in world or UI space
        public Vector2 position;

        public NodeDTO() {}

        /// Constructs a NodeDTO from a Node instance
        public NodeDTO(Node node) 
        {
            point = node.point;
            incoming = new List<Vector2Int>(node.incoming);
            outgoing = new List<Vector2Int>(node.outgoing);
            nodeType = node.nodeType.ToString();
            blueprintName = node.blueprintName;
            position = node.position;
        }

        /// Converts this NodeDTO back into a Node object
        public Node ToNode() 
        {
            return new Node(
                Enum.TryParse(nodeType, out NodeType parsedType) ? parsedType : NodeType.Undefined,
                blueprintName,
                point
            ) {
                position = this.position,
                incoming = new List<Vector2Int>(this.incoming),
                outgoing = new List<Vector2Int>(this.outgoing)
            };
        }
    }
}