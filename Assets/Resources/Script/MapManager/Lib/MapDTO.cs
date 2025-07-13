using System;
using System.Collections.Generic;
using UnityEngine;

namespace Map 
{
    /// Data Transfer Object (DTO) for serializing and deserializing map data.
    /// Contains a list of node DTOs, the path through the map, the boss node name, 
    /// and the configuration name.
    /// Provides conversion between the Map and MapDTO representations.
    [Serializable]
    public class MapDTO 
    {
        /// List of node data transfer objects representing the map's nodes
        public List<NodeDTO> nodes = new List<NodeDTO>();
        /// List of coordinates representing the path through the map
        public List<Vector2Int> path = new List<Vector2Int>();
        /// Name of the boss node in the map
        public string bossNodeName;
        /// Name of the map configuration
        public string configName;

        public MapDTO() {}

        /// Constructs a MapDTO from a Map instance
        public MapDTO(Map map) 
        {
            bossNodeName = map.bossNodeName;
            configName = map.configName;
            path = new List<Vector2Int>(map.path);
            if (map.nodes != null) {
                foreach (var node in map.nodes) {
                    nodes.Add(new NodeDTO(node));
                }
            }
        }

        /// Converts this MapDTO back into a Map object
        public Map ToMap() 
        {
            var nodeList = new List<Node>();
            if (nodes != null) {
                foreach (var nodeDto in nodes) {
                    nodeList.Add(nodeDto.ToNode());
                }
            }
            return new Map(configName, bossNodeName, nodeList, path);
        }
    }
}