using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Map 
{
    /// Handles the visual representation and interaction logic for the node-based map.
    /// Responsible for instantiating nodes and lines, setting up map orientation,
    /// coloring, and background, and updating node and line states based on player progress.
    public class MapView : MonoBehaviour {
        /// Defines the orientation in which the map is displayed and scrolled
        public enum MapOrientation {
            BottomToTop,
            TopToBottom,
            RightToLeft,
            LeftToRight
        }

        /// Reference to the MapManager controlling the current map logic
        public MapManager mapManager;
        /// The orientation of the map (affects scrolling and layout)
        public MapOrientation orientation;

        [Tooltip(
            "List of all the MapConfig scriptable objects from the Assets folder that might be used to construct maps.")]
        public List<MapConfig> allMapConfigs;

        /// Prefab used to instantiate map node
        public GameObject nodePrefab;

        [Tooltip("Offset of the start/end nodes of the map from the edges of the screen")]
        public float orientationOffset;

        [Header("Background Settings")]
        [Tooltip("If the background sprite is null, background will not be shown")]
        public Sprite background;
        public Color32 backgroundColor = Color.white;
        public float xSize;
        public float yOffset;

        [Header("Line Settings")]
        public GameObject linePrefab;
        [Tooltip("Line point count should be > 2 to get smooth color gradients")]
        [Range(3, 10)]
        public int linePointsCount = 10;
        [Tooltip("Distance from the node till the line starting point")]
        public float offsetFromNodes = 0.5f;

        [Header("Colors")]
        [Tooltip("Node Visited or Attainable color")]
        public Color32 visitedColor = Color.white;
        [Tooltip("Locked node color")]
        public Color32 lockedColor = Color.gray;
        [Tooltip("Visited or available path color")]
        public Color32 lineVisitedColor = Color.white;
        [Tooltip("Unavailable path color")]
        public Color32 lineLockedColor = Color.gray;

        protected GameObject firstParent;
        protected GameObject mapParent;
        private List<List<Vector2Int>> paths;
        private Camera cam;
        private bool isInteractionLocked = false;

        /// All instantiated MapNode components for the current map
        public readonly List<MapNode> MapNodes = new List<MapNode>();
        /// All line connections between nodes for the current map
        protected readonly List<LineConnection> lineConnections = new List<LineConnection>();

        /// Singleton instance for global access
        public static MapView Instance;

        /// The currently displayed map
        public Map Map { get; protected set; }

        private void Awake()
        {
            /// Sets the singleton instance and initializes the camera reference
            if (Instance != null) {
                Destroy(Instance);
            }
            Instance = this;
            cam = Camera.main;
        }

        /// Destroys the current map's parent objects and clears all node and line references
        protected virtual void ClearMap()
        {
            if (firstParent != null) { Destroy(firstParent); }

            MapNodes.Clear();
            lineConnections.Clear();
        }

        /// Displays the given map by instantiating nodes, lines, background, and setting up 
        /// orientation and states
        public virtual void ShowMap(Map m)
        {
            if (m == null) {
                Global.DEBUG_PRINT("[MapView::ShowMap] Map was null in MapView.ShowMap()");
                return;
            }

            Map = m;

            ClearMap();
            CreateMapParent();
            CreateNodes(m.nodes);
            DrawLines();
            SetOrientation();
            ResetNodesRotation();
            SetAttainableNodes();
            SetLineColors();
            CreateMapBackground(m);
        }

        /// Instantiates and positions the background sprite for the map, if set
        protected virtual void CreateMapBackground(Map m)
        {
            if (background == null) { return; }

            GameObject backgroundObject = new GameObject("Background");
            backgroundObject.transform.SetParent(mapParent.transform);
            MapNode bossNode = MapNodes.FirstOrDefault(node => node.Node.nodeType == NodeType.MajorBoss);
            float span = m.DistanceBetweenFirstAndLastLayers();
            backgroundObject.transform.localPosition = new Vector3(bossNode.transform.localPosition.x, span / 2f, 0f);
            backgroundObject.transform.localRotation = Quaternion.identity;
            SpriteRenderer sr = backgroundObject.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 0;
            sr.color = backgroundColor;
            sr.drawMode = SpriteDrawMode.Sliced;
            sr.sprite = background;
            sr.size = new Vector2(xSize, span + yOffset * 2f);
        }

        /// Creates the parent GameObjects for the map and sets up scrolling and collision
        protected virtual void CreateMapParent()
        {
            firstParent = new GameObject("OuterMapParent");
            mapParent = new GameObject("MapParentWithAScroll");
            mapParent.transform.SetParent(firstParent.transform);
            ScrollNonUI scrollNonUi = mapParent.AddComponent<ScrollNonUI>();
            scrollNonUi.freezeX = orientation == MapOrientation.BottomToTop || orientation == MapOrientation.TopToBottom;
            scrollNonUi.freezeY = orientation == MapOrientation.LeftToRight || orientation == MapOrientation.RightToLeft;
            BoxCollider boxCollider = mapParent.AddComponent<BoxCollider>();
            boxCollider.size = new Vector3(100, 100, 1);
        }

        /// Instantiates all nodes for the map and adds them to MapNodes
        protected void CreateNodes(IEnumerable<Node> nodes)
        {
            foreach (Node node in nodes) {
                MapNode mapNode = CreateMapNode(node);
                MapNodes.Add(mapNode);
            }
        }

        /// Instantiates a single MapNode and sets it up with its blueprint and position
        protected virtual MapNode CreateMapNode(Node node)
        {
            GameObject mapNodeObject = Instantiate(nodePrefab, mapParent.transform);
            MapNode mapNode = mapNodeObject.GetComponent<MapNode>();
            NodeBlueprint blueprint = GetBlueprint(node.blueprintName);
            mapNode.SetUp(node, blueprint);
            mapNode.transform.localPosition = node.position;
            return mapNode;
        }

        public void LockMapInteractions(bool lockInteractions)
        {
            mapParent.GetComponent<ScrollNonUI>().enabled = !lockInteractions;
            mapParent.GetComponent<BoxCollider>().enabled = !lockInteractions;

            foreach (MapNode node in MapNodes) {
                node.LockInteractions(lockInteractions);
            }
            isInteractionLocked = lockInteractions;
        }

        /// Updates the state of all nodes to reflect which are locked, attainable, or visited based on the current path
        public void SetAttainableNodes()
        {
            // first set all the nodes as unattainable/locked:
            foreach (MapNode node in MapNodes)
                node.SetState(NodeStates.Locked);

            if (mapManager.CurrentMap.path.Count == 0) {
                // we have not started traveling on this map yet, set entire first layer as attainable:
                foreach (MapNode node in MapNodes.Where(n => n.Node.point.y == 0))
                    node.SetState(NodeStates.Attainable);
            } else {
                // we have already started moving on this map, first highlight the path as visited:
                foreach (Vector2Int point in mapManager.CurrentMap.path) {
                    MapNode mapNode = GetNode(point);
                    if (mapNode != null)
                        mapNode.SetState(NodeStates.Visited);
                }

                Vector2Int currentPoint = mapManager.CurrentMap.path[mapManager.CurrentMap.path.Count - 1];
                Node currentNode = mapManager.CurrentMap.GetNode(currentPoint);

                // set all the nodes that we can travel to as attainable:
                foreach (Vector2Int point in currentNode.outgoing) {
                    MapNode mapNode = GetNode(point);
                    if (mapNode != null)
                        mapNode.SetState(NodeStates.Attainable);
                }
            }
        }

        /// Updates the color of all lines to reflect which are available, visited, or locked
        public virtual void SetLineColors()
        {
            Global.DEBUG_PRINT("[MapView::SetLineColors] Setting line colors for map: " + mapManager.CurrentMap.configName);
            foreach (LineConnection connection in lineConnections)
                connection.SetColor(lineLockedColor);

            // set all lines that are a part of the path to visited color:
            // if we have not started moving on the map yet, leave everything as is:
            if (mapManager.CurrentMap.path.Count == 0)
                return;

            // in any case, we mark outgoing connections from the final node with visible/attainable color:
            Vector2Int currentPoint = mapManager.CurrentMap.path[mapManager.CurrentMap.path.Count - 1];
            Node currentNode = mapManager.CurrentMap.GetNode(currentPoint);

            foreach (Vector2Int point in currentNode.outgoing) {
                LineConnection lineConnection = lineConnections.FirstOrDefault(conn => conn.from.Node == currentNode &&
                                                                            conn.to.Node.point.Equals(point));
                lineConnection?.SetColor(lineVisitedColor);
            }

            if (mapManager.CurrentMap.path.Count <= 1) return;

            for (int i = 0; i < mapManager.CurrentMap.path.Count - 1; i++) {
                Vector2Int current = mapManager.CurrentMap.path[i];
                Vector2Int next = mapManager.CurrentMap.path[i + 1];
                LineConnection lineConnection = lineConnections.FirstOrDefault(conn => conn.@from.Node.point.Equals(current) &&
                                                                            conn.to.Node.point.Equals(next));
                lineConnection?.SetColor(lineVisitedColor);
            }
        }

        /// Sets the orientation and scrolling constraints of the map based on the selected orientation
        protected virtual void SetOrientation()
        {
            ScrollNonUI scrollNonUi = mapParent.GetComponent<ScrollNonUI>();
            float span = mapManager.CurrentMap.DistanceBetweenFirstAndLastLayers();
            MapNode bossNode = MapNodes.FirstOrDefault(node => node.Node.nodeType == NodeType.MajorBoss);
            Global.DEBUG_PRINT("[MapView::SetOrientation] Map span in set orientation: " + span + " camera aspect: " + cam.aspect);

            // setting first parent to be right in front of the camera first:
            firstParent.transform.position = new Vector3(cam.transform.position.x, cam.transform.position.y, 0f);
            float offset = orientationOffset;
            switch (orientation) {
                case MapOrientation.BottomToTop:
                    if (scrollNonUi != null) {
                        scrollNonUi.yConstraints.max = 0;
                        scrollNonUi.yConstraints.min = -(span + 2f * offset);
                    }
                    firstParent.transform.localPosition += new Vector3(0, offset, 0);
                    break;
                case MapOrientation.TopToBottom:
                    mapParent.transform.eulerAngles = new Vector3(0, 0, 180);
                    if (scrollNonUi != null) {
                        scrollNonUi.yConstraints.min = 0;
                        scrollNonUi.yConstraints.max = span + 2f * offset;
                    }
                    // factor in map span:
                    firstParent.transform.localPosition += new Vector3(0, -offset, 0);
                    break;
                case MapOrientation.RightToLeft:
                    offset *= cam.aspect;
                    mapParent.transform.eulerAngles = new Vector3(0, 0, 90);
                    // factor in map span:
                    firstParent.transform.localPosition -= new Vector3(offset, bossNode.transform.position.y, 0);
                    if (scrollNonUi != null) {
                        scrollNonUi.xConstraints.max = span + 2f * offset;
                        scrollNonUi.xConstraints.min = 0;
                    }
                    break;
                case MapOrientation.LeftToRight:
                    offset *= cam.aspect;
                    mapParent.transform.eulerAngles = new Vector3(0, 0, -90);
                    firstParent.transform.localPosition += new Vector3(offset, -bossNode.transform.position.y, 0);
                    if (scrollNonUi != null) {
                        scrollNonUi.xConstraints.max = 0;
                        scrollNonUi.xConstraints.min = -(span + 2f * offset);
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// Instantiates and connects lines between all nodes based on their outgoing connections
        private void DrawLines()
        {
            foreach (MapNode node in MapNodes) {
                foreach (Vector2Int connection in node.Node.outgoing)
                    AddLineConnection(node, GetNode(connection));
            }
        }

        /// Resets the rotation of all nodes to ensure they are upright after orientation changes
        private void ResetNodesRotation()
        {
            foreach (MapNode node in MapNodes) {
                node.transform.rotation = Quaternion.identity;
            }
        }

        /// Instantiates a line between two nodes and configures its appearance and connection data
        protected virtual void AddLineConnection(MapNode from, MapNode to)
        {
            if (linePrefab == null) { return; }

            GameObject lineObject = Instantiate(linePrefab, mapParent.transform);
            LineRenderer lineRenderer = lineObject.GetComponent<LineRenderer>();
            lineRenderer.sortingOrder = 10;
            lineRenderer.startWidth = 0.03f;
            lineRenderer.endWidth = 0.03f;
            Vector3 fromPoint = from.transform.position +
                                (to.transform.position - from.transform.position).normalized * offsetFromNodes;

            Vector3 toPoint = to.transform.position +
                              (from.transform.position - to.transform.position).normalized * offsetFromNodes;

            // drawing lines in local space:
            lineObject.transform.position = fromPoint;
            lineRenderer.useWorldSpace = false;

            // line renderer with 2 points only does not handle transparency properly:
            lineRenderer.positionCount = linePointsCount;
            for (int i = 0; i < linePointsCount; i++) {
                lineRenderer.SetPosition(i,
                    Vector3.Lerp(Vector3.zero, toPoint - fromPoint, (float)i / (linePointsCount - 1)));
            }

            DottedLineRenderer dottedLine = lineObject.GetComponent<DottedLineRenderer>();
            if (dottedLine != null) { dottedLine.ScaleMaterial(); }

            lineConnections.Add(new LineConnection(lineRenderer, from, to));
        }

        /// Finds the MapNode corresponding to a given grid point
        protected MapNode GetNode(Vector2Int p)
        {
            return MapNodes.FirstOrDefault(n => n.Node.point.Equals(p));
        }

        /// Finds a MapConfig by its name from the list of all configs
        protected MapConfig GetConfig(string configName)
        {
            return allMapConfigs.FirstOrDefault(c => c.name == configName);
        }

        /// Finds a NodeBlueprint by node type from the current map's config
        protected NodeBlueprint GetBlueprint(NodeType type)
        {
            MapConfig config = GetConfig(mapManager.CurrentMap.configName);
            return config.nodeBlueprints.FirstOrDefault(n => n.nodeType == type);
        }

        /// Finds a NodeBlueprint by blueprint name from the current map's config
        protected NodeBlueprint GetBlueprint(string blueprintName)
        {
            MapConfig config = GetConfig(mapManager.CurrentMap.configName);
            return config.nodeBlueprints.FirstOrDefault(n => n.name == blueprintName);
        }
        
        public bool AreInteractionsLocked() => isInteractionLocked;
    }
}
