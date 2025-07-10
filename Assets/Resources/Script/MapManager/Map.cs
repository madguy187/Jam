using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Map {
    [Serializable]
    public class Map {
        public List<Node> nodes;
        public List<Vector2Int> path;
        public string bossNodeName;
        public string configName; // similar to the act name in Slay the Spire

        public Map() { }

        public Map(string configName, string bossNodeName, List<Node> nodes, List<Vector2Int> path) {
            this.configName = configName;
            this.bossNodeName = bossNodeName;
            this.nodes = nodes;
            this.path = path;
        }

        public Node GetBossNode() {
            return nodes.FirstOrDefault(n => n.nodeType == NodeType.MajorBoss);
        }

        public float DistanceBetweenFirstAndLastLayers() {
            Node bossNode = GetBossNode();
            Node firstLayerNode = nodes.FirstOrDefault(n => n.point.y == 0);

            if (bossNode == null || firstLayerNode == null)
                return 0f;

            return bossNode.position.y - firstLayerNode.position.y;
        }

        public Node GetNode(Vector2Int point) {
            return nodes.FirstOrDefault(n => n.point.Equals(point));
        }

        // --- Serialization using DTO for Unity's JsonUtility ---
        public string ToJson() {
            MapDTO dto = new MapDTO(this);
            return JsonUtility.ToJson(dto, true);
        }

        public static Map FromJson(string json) {
            MapDTO dto = JsonUtility.FromJson<MapDTO>(json);
            return dto.ToMap();
        }

        public MapDTO ToDTO() {
            return new MapDTO(this);
        }

        public static Map FromDTO(MapDTO dto) {
            return dto.ToMap();
        }
    }
}