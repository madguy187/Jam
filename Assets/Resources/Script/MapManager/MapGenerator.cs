using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Map 
{
    /// Static class responsible for procedural map generation, including node placement, 
    /// path creation, and connection management.
    public static class MapGenerator 
    {
        private static MapConfig config;
        private static List<float> layerDistances;
        private static readonly List<List<Node>> nodes = new List<List<Node>>();

        /// Generates a new map based on the provided configuration.
        public static Map GetMap(MapConfig conf)
        {
            if (conf == null) {
                Global.DEBUG_PRINT("[MapGenerator::GetMap] Config was null in MapGenerator.Generate()");
                return null;
            }

            config = conf;
            nodes.Clear();

            GenerateLayerDistances();

            for (int i = 0; i < conf.layers.Count; i++) {
                PlaceLayer(i);
            }

            var paths = GeneratePaths();

            RandomizeNodePositions();
            SetUpConnections(paths);
            RemoveCrossConnections();

            // Create List<Node> with Connections
            var nodesList = nodes.SelectMany(n => n)
                                 .Where(n => n.incoming.Count > 0 || n.outgoing.Count > 0)
                                 .ToList();

            // Create random stringname for the boss level of this Map
            var bossNodeName = config.nodeBlueprints
                                     .Where(b => b.nodeType == NodeType.MajorBoss)
                                     .ToList()
                                     .Random()
                                     .name;

            return new Map(conf.name, bossNodeName, nodesList, new List<Vector2Int>());
        }

        /// Calculates and stores the distances between each map layer.
        private static void GenerateLayerDistances() 
        {
            layerDistances = config.layers
                .Select(layer => layer.distanceFromPreviousLayer.GetValue())
                .ToList();
        }

        /// Gets the total distance from the start to the specified layer index
        private static float GetDistanceToLayer(int layerIndex) 
        {
            if (layerIndex < 0 || layerIndex > layerDistances.Count) {
                return 0f;
            }
            return layerDistances.Take(layerIndex + 1).Sum();
        }

        /// Places all nodes for a specific layer, assigning types and positions
        private static void PlaceLayer(int layerIndex) 
        {
            MapLayer layer = config.layers[layerIndex];
            var nodesOnThisLayer = new List<Node>();
            // Calculate offfset for the layer to center all nodes
            float offset = layer.nodesApartDistance * config.GridWidth / 2f;
            var supportedRandomNodeTypes =
                config.randomNodes
                    .Where(t => config.nodeBlueprints
                    .Any(b => b.nodeType == t))
                    .ToList();

            for (int i = 0; i < config.GridWidth; i++) {
                NodeType nodeType = Random.Range(0f, 1f) < layer.randomizeNodes && supportedRandomNodeTypes.Count > 0
                    ? supportedRandomNodeTypes.Random()
                    : layer.nodeType;
                
                string blueprintName = config.nodeBlueprints
                    .Where(b => b.nodeType == nodeType)
                    .ToList()
                    .Random()
                    .name;

                // Create a new node with the given type and position
                Node node = new Node(nodeType, blueprintName, new Vector2Int(i, layerIndex)) {
                    position = new Vector2(-offset + i * layer.nodesApartDistance, GetDistanceToLayer(layerIndex))
                };
                nodesOnThisLayer.Add(node);
            }

            nodes.Add(nodesOnThisLayer);
        }

        /// Randomizes the positions of nodes within each layer for a more organic layout
        private static void RandomizeNodePositions() {
            for (int index = 0; index < nodes.Count; index++) {
                List<Node> list = nodes[index];
                MapLayer layer = config.layers[index];
                float distToNextLayer = (index + 1 >= layerDistances.Count) ? 0f : layerDistances[index + 1];
                float distToPreviousLayer = layerDistances[index];

                foreach (Node node in list) {
                    float xRnd = Random.Range(-0.5f, 0.5f);
                    float yRnd = Random.Range(-0.5f, 0.5f);

                    float x = xRnd * layer.nodesApartDistance;
                    float y = yRnd < 0 ? distToPreviousLayer * yRnd : distToNextLayer * yRnd;

                    node.position += new Vector2(x, y) * layer.randomizePosition;
                }
            }
        }

        /// Sets up connections between nodes based on the generated paths
        private static void SetUpConnections(List<List<Vector2Int>> paths) {
            foreach (List<Vector2Int> path in paths) {
                for (int i = 0; i < path.Count - 1; ++i) {
                    Node node = GetNode(path[i]);
                    Node nextNode = GetNode(path[i + 1]);
                    node.AddOutgoing(nextNode.point);
                    nextNode.AddIncoming(node.point);
                }
            }
        }

        /// Removes cross connections between nodes to prevent overlapping paths and 
        /// ensure a cleaner map structure
        private static void RemoveCrossConnections() {
            for (int i = 0; i < config.GridWidth - 1; ++i) {
                for (int j = 0; j < config.layers.Count - 1; ++j) {
                    // Get the node at (i, j)
                    var node = GetNode(new Vector2Int(i, j));
                    if (node == null || node.HasNoConnections()) { continue; }

                    // Check if the node has outgoing connections to the right and top
                    var right = GetNode(new Vector2Int(i + 1, j));
                    if (right == null || right.HasNoConnections()) { continue; }

                    // Get the top and top-right nodes
                    var top = GetNode(new Vector2Int(i, j + 1));
                    if (top == null || top.HasNoConnections()) { continue; }

                    // Get the top-right node
                    var topRight = GetNode(new Vector2Int(i + 1, j + 1));
                    if (topRight == null || topRight.HasNoConnections()) { continue; }

                    // Check if the node has outgoing connections to the top-right and right nodes
                    if (!node.outgoing.Any(element => element.Equals(topRight.point))) { continue; }
                    if (!right.outgoing.Any(element => element.Equals(top.point))) { continue; }

                    // If we managed to get here, we have a cross connection to resolve
                    // Add direct connections
                    node.AddOutgoing(top.point);
                    top.AddIncoming(node.point);

                    right.AddOutgoing(topRight.point);
                    topRight.AddIncoming(right.point);

                    // Randomly remove one of the cross connections
                    float rnd = Random.Range(0f, 1f);
                    if (rnd < 0.2f) {
                        node.RemoveOutgoing(topRight.point);
                        topRight.RemoveIncoming(node.point);
                        right.RemoveOutgoing(top.point);
                        top.RemoveIncoming(right.point);
                    } else if (rnd < 0.6f) {
                        node.RemoveOutgoing(topRight.point);
                        topRight.RemoveIncoming(node.point);
                    } else {
                        right.RemoveOutgoing(top.point);
                        top.RemoveIncoming(right.point);
                    }
                }
            }
        }

        /// Retrieves the node at the specified grid position
        private static Node GetNode(Vector2Int p) {
            if (p.y >= nodes.Count || p.x >= nodes[p.y].Count) {
                return null;
            }
            return nodes[p.y][p.x];
        }

        /// Determines the final (boss) node's position in the map
        private static Vector2Int GetFinalNode() {
            int y = config.layers.Count - 1;
            if (config.GridWidth % 2 == 1) {
                return new Vector2Int(config.GridWidth / 2, y);
            }
            return Random.Range(0, 2) == 0
                ? new Vector2Int(config.GridWidth / 2, y)
                : new Vector2Int(config.GridWidth / 2 - 1, y);
        }

        /// Generates all the main paths through the map, from starting 
        /// nodes to the pre-boss and final nodes
        private static List<List<Vector2Int>> GeneratePaths() {
            Vector2Int finalNode = GetFinalNode();
            var paths = new List<List<Vector2Int>>();
            int numOfStartingNodes = config.numOfStartingNodes.GetValue();
            int numOfPreBossNodes = config.numOfPreBossNodes.GetValue();

            List<int> candidateXs = Enumerable.Range(0, config.GridWidth).ToList();
            candidateXs.Shuffle();
            IEnumerable<int> startingXs = candidateXs.Take(numOfStartingNodes);
            List<Vector2Int> startingPoints = startingXs.Select(x => new Vector2Int(x, 0)).ToList();
            candidateXs.Shuffle();
            
            IEnumerable<int> preBossXs = candidateXs.Take(numOfPreBossNodes);
            List<Vector2Int> preBossPoints = (from x in preBossXs select new Vector2Int(x, finalNode.y - 1)).ToList();

            int numOfPaths = Mathf.Max(numOfStartingNodes, numOfPreBossNodes) + Mathf.Max(0, config.extraPaths);
            for (int i = 0; i < numOfPaths; ++i) {
                Vector2Int startNode = startingPoints[i % numOfStartingNodes];
                Vector2Int endNode = preBossPoints[i % numOfPreBossNodes];
                List<Vector2Int> path = Path(startNode, endNode);
                path.Add(finalNode);
                paths.Add(path);
            }

            return paths;
        }

        /// Generates a random path from a starting node to a target node, moving 
        /// bottom-up through the layers
        private static List<Vector2Int> Path(Vector2Int fromPoint, Vector2Int toPoint) {
            int toRow = toPoint.y;
            int toCol = toPoint.x;
            int lastNodeCol = fromPoint.x;

            var path = new List<Vector2Int> { fromPoint };
            var candidateCols = new List<int>();

            for (int row = 1; row < toRow; ++row) {
                candidateCols.Clear();

                int verticalDistance = toRow - row;

                int forwardCol = lastNodeCol;
                if (Mathf.Abs(toCol - forwardCol) <= verticalDistance)
                    candidateCols.Add(lastNodeCol);

                int leftCol = lastNodeCol - 1;
                if (leftCol >= 0 && Mathf.Abs(toCol - leftCol) <= verticalDistance)
                    candidateCols.Add(leftCol);

                int rightCol = lastNodeCol + 1;
                if (rightCol < config.GridWidth && Mathf.Abs(toCol - rightCol) <= verticalDistance)
                    candidateCols.Add(rightCol);

                int randomCandidateIndex = Random.Range(0, candidateCols.Count);
                int candidateCol = candidateCols[randomCandidateIndex];
                var nextPoint = new Vector2Int(candidateCol, row);

                path.Add(nextPoint);
                lastNodeCol = candidateCol;
            }

            path.Add(toPoint);
            return path;
        }
    }
}