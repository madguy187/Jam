using System;
using System.Collections.Generic;
using UnityEngine;

namespace Map {
    [Serializable]
    public class Node {
        public Vector2Int point;
        public List<Vector2Int> incoming = new List<Vector2Int>();
        public List<Vector2Int> outgoing = new List<Vector2Int>();
        public NodeType nodeType;
        public string blueprintName;
        public Vector2 position;

        public Node() { }

        public Node(NodeType nodeType, string blueprintName, Vector2Int point) {
            this.nodeType = nodeType;
            this.blueprintName = blueprintName;
            this.point = point;
        }

        public void AddIncoming(Vector2Int p) {
            if (incoming.Contains(p))
                return;
            incoming.Add(p);
        }

        public void AddOutgoing(Vector2Int p) {
            if (outgoing.Contains(p))
                return;
            outgoing.Add(p);
        }

        public void RemoveIncoming(Vector2Int p) {
            incoming.RemoveAll(element => element.Equals(p));
        }

        public void RemoveOutgoing(Vector2Int p) {
            outgoing.RemoveAll(element => element.Equals(p));
        }

        public bool HasNoConnections() {
            return incoming.Count == 0 && outgoing.Count == 0;
        }

        // Convert to DTO for JSON serialization (enum as string)
        public NodeDTO ToDTO() => new NodeDTO(this);

        // Create Node from DTO
        public static Node FromDTO(NodeDTO dto) => dto.ToNode();
    }
}