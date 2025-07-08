using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class MapGenerator : MonoBehaviour {
    [Header("UI Prefabs and Containers")]
    public GameObject nodePrefab; // Assign NodeUI prefab here
    public RectTransform mapContainer; // Assign the RectTransform of the Scroll View content
    public GameObject linePrefab; // Assign UILine prefab here

    [Header("Map Settings")]
    public int layers = 5;
    public int nodesPerLayer = 3;
    public float horizontalSpacing = 200f;
    public float verticalSpacing = 200f;

    private List<List<MapNode>> mapLayers = new List<List<MapNode>>();

    void Start() {
        GenerateMap();
    }

    void GenerateMap() {
        // Step 1: Create all nodes first
        Vector2 offset = CalculateMapCenterOffset();

        for (int i = 0; i < layers; i++) {
            List<MapNode> layer = new List<MapNode>();

            for (int j = 0; j < nodesPerLayer; j++) {
                NodeType type = (i == layers - 1)
                    ? NodeType.Boss
                    : (i == 0 ? NodeType.Encounter : GetRandomNodeType());

                Vector2 position = new Vector2(
                    j * horizontalSpacing,
                    -i * verticalSpacing
                );

                position += offset;

                MapNode node = new MapNode(type, position);
                layer.Add(node);
            }

            mapLayers.Add(layer);
        }

        // Step 2: Instantiate all node UI elements
        foreach (var layer in mapLayers) {
            foreach (var node in layer) {
                GameObject uiNode = Instantiate(nodePrefab, mapContainer);
                RectTransform rect = uiNode.GetComponent<RectTransform>();
                rect.anchoredPosition = node.Position;
                uiNode.GetComponent<NodeUI>().Init(node.Type);

                node.UINode = rect;
            }
        }

        // Step 3: Connect nodes and draw lines
        for (int i = 1; i < mapLayers.Count; i++) {
            ConnectLayers(mapLayers[i - 1], mapLayers[i]);
            DrawLinesBetweenLayer(mapLayers[i - 1], mapLayers[i]);
        }

        ResizeMapContainer();
    }

    NodeType GetRandomNodeType() {
        int roll = Random.Range(0, 100);
        if (roll < 50) return NodeType.Fight;
        if (roll < 80) return NodeType.Encounter;
        return NodeType.Shop;
    }

    void ConnectLayers(List<MapNode> fromLayer, List<MapNode> toLayer) {
        foreach (var from in fromLayer) {
            int connections = Random.Range(1, 3);
            for (int i = 0; i < connections; i++) {
                MapNode to = toLayer[Random.Range(0, toLayer.Count)];
                if (!from.ConnectedNodes.Contains(to))
                    from.ConnectedNodes.Add(to);
            }
        }
    }

    void DrawLinesBetweenLayer(List<MapNode> fromLayer, List<MapNode> toLayer) {
        foreach (var from in fromLayer) {
            foreach (var to in from.ConnectedNodes) {
                if (from.UINode == null || to.UINode == null)
                    continue;

                GameObject line = Instantiate(linePrefab, mapContainer);
                RectTransform rect = line.GetComponent<RectTransform>();

                Vector2 start = from.UINode.anchoredPosition;
                Vector2 end = to.UINode.anchoredPosition;
                Vector2 direction = end - start;

                float distance = direction.magnitude;
                rect.sizeDelta = new Vector2(distance, 3f);
                rect.anchoredPosition = start + direction / 2;
                rect.rotation = Quaternion.FromToRotation(Vector3.right, direction);
            }
        }
    }

    Vector2 CalculateMapCenterOffset() {
        float mapWidth = (nodesPerLayer - 1) * horizontalSpacing;
        float mapHeight = (layers - 1) * verticalSpacing;

        // Center offset to shift nodes so map is centered at (0,0)
        return new Vector2(-mapWidth / 2f, mapHeight / 2f);
    }

    void ResizeMapContainer() {
        float mapWidth = (nodesPerLayer - 1) * horizontalSpacing + 400; // padding
        float mapHeight = (layers - 1) * verticalSpacing + 400;

        mapContainer.sizeDelta = new Vector2(mapWidth, mapHeight);
    }
}
