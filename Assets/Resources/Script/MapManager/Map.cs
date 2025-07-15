using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Map 
{
    /// Represents a map structure composed of nodes and paths, for node progression.
    /// Provides additional serialization/deserialization to and from JSON or DTO
    [Serializable]
    public class Map 
    {
        /// List of all nodes present in the map
        public List<Node> nodes;
        /// List of points representing the main path through the map
        public List<Vector2Int> path;
        /// The name of the boss node in the map
        public string bossNodeName;
        /// The name of the map configuration
        public string configName;

        public Map() {}

        public Map(string configName, string bossNodeName, List<Node> nodes, List<Vector2Int> path) 
        {
            this.configName = configName;
            this.bossNodeName = bossNodeName;
            this.nodes = nodes;
            this.path = path;
        }

        /// Returns the node marked as the major boss node, or null if not found
        public Node GetBossNode() 
        {
            return nodes.FirstOrDefault(n => n.nodeType == NodeType.MajorBoss);
        }

        /// Calculates the vertical distance between the first layer node (y == 0) and the boss node.
        /// Returns 0 if either node is not found
        public float DistanceBetweenFirstAndLastLayers() 
        {
            Node bossNode = GetBossNode();
            Node firstLayerNode = nodes.FirstOrDefault(n => n.point.y == 0);

            if (bossNode == null || firstLayerNode == null)
                return 0f;

            return bossNode.position.y - firstLayerNode.position.y;
        }

        /// Finds and returns the node at the specified grid point, or null if not found
        public Node GetNode(Vector2Int point) 
        {
            return nodes.FirstOrDefault(n => n.point.Equals(point));
        }

        // --- Serialization using DTO for Unity's JsonUtility ---

        /// Serializes this map to a formatted JSON string using a DTO
        public string ToJson() 
        {
            MapDTO dto = new MapDTO(this);
            return JsonUtility.ToJson(dto, true);
        }

        /// Deserializes a JSON string into a Map object using a DTO
        public static Map FromJson(string json) 
        {
            MapDTO dto = JsonUtility.FromJson<MapDTO>(json);
            return dto.ToMap();
        }

        /// Converts this map to a data transfer object (DTO) for serialization
        public MapDTO ToDTO() 
        {
            return new MapDTO(this);
        }

        /// Creates a Map object from a data transfer object (DTO)
        public static Map FromDTO(MapDTO dto) 
        {
            return dto.ToMap();
        }
    }
}