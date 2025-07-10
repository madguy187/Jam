using System;
using System.Collections.Generic;
using UnityEngine;

namespace Map {
    [Serializable]
    public class MapDTO {
        public List<NodeDTO> nodes = new List<NodeDTO>();
        public List<Vector2Int> path = new List<Vector2Int>();
        public string bossNodeName;
        public string configName;

        public MapDTO() { }

        public MapDTO(Map map) {
            bossNodeName = map.bossNodeName;
            configName = map.configName;
            path = new List<Vector2Int>(map.path);
            if (map.nodes != null) {
                foreach (var node in map.nodes) {
                    nodes.Add(new NodeDTO(node));
                }
            }
        }

        public Map ToMap() {
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