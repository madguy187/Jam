using System.Collections.Generic;
using UnityEngine;

namespace Map {
    [CreateAssetMenu]
    public class MapConfig : ScriptableObject {
        public List<NodeBlueprint> nodeBlueprints;
        [Tooltip("Nodes that will be used on layers with Randomize Nodes > 0")]
        public List<NodeType> randomNodes = new List<NodeType>
            {NodeType.Enemy, NodeType.MiniBoss, NodeType.Encounter, NodeType.Shop, NodeType.MajorBoss};
        public int GridWidth => Mathf.Max(numOfPreBossNodes.max, numOfStartingNodes.max);

        [Header("Pre Boss Nodes")]
        public IntMinMax numOfPreBossNodes;
        [Header("Starting Nodes")]
        public IntMinMax numOfStartingNodes;

        [Tooltip("Increase this number to generate more paths")]
        public int extraPaths;
        public List<MapLayer> layers;
    }
}