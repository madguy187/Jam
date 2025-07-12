using System;
using System.Collections.Generic;
using UnityEngine;

namespace Map {
    [Serializable]
    public class NodeDTO {
        public Vector2Int point;
        public List<Vector2Int> incoming = new List<Vector2Int>();
        public List<Vector2Int> outgoing = new List<Vector2Int>();
        public string nodeType; // string for enum
        public string blueprintName;
        public Vector2 position;

        public NodeDTO() { }

        public NodeDTO(Node node) {
            point = node.point;
            incoming = new List<Vector2Int>(node.incoming);
            outgoing = new List<Vector2Int>(node.outgoing);
            nodeType = node.nodeType.ToString();
            blueprintName = node.blueprintName;
            position = node.position;
        }

        public Node ToNode() {
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