using UnityEngine;
using System.Collections.Generic;

public class MapNode {
    public NodeType Type;
    public Vector2 Position;
    public List<MapNode> ConnectedNodes = new List<MapNode>();
    public RectTransform UINode;

    public MapNode(NodeType type, Vector2 position) {
        Type = type;
        Position = position;
    }
}

