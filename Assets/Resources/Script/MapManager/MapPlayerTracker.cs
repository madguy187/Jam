using System;
using System.Linq;
using UnityEngine;

namespace Map {
    public class MapPlayerTracker : MonoBehaviour {
        public bool lockAfterSelecting = false;
        public float enterNodeDelay = 1f;
        public MapManager mapManager;
        public MapView view;

        public static MapPlayerTracker Instance;

        public bool Locked { get; set; }

        private void Awake() {
            Instance = this;
        }

        public void SelectNode(MapNode mapNode) {
            if (Locked) return;

            // Debug.Log("Selected node: " + mapNode.Node.point);

            if (mapManager.CurrentMap.path.Count == 0) {
                // player has not selected the node yet, he can select any of the nodes with y = 0
                if (mapNode.Node.point.y == 0)
                    SendPlayerToNode(mapNode);
                else
                    Debug.Log("Selected node cannot be accessed");
            } else {
                Vector2Int currentPoint = mapManager.CurrentMap.path[mapManager.CurrentMap.path.Count - 1];
                Node currentNode = mapManager.CurrentMap.GetNode(currentPoint);

                if (currentNode != null && currentNode.outgoing.Any(point => point.Equals(mapNode.Node.point)))
                    SendPlayerToNode(mapNode);
                else
                    Debug.Log("Selected node cannot be accessed");
            }
        }

        private void SendPlayerToNode(MapNode mapNode) {
            Locked = lockAfterSelecting;
            mapManager.CurrentMap.path.Add(mapNode.Node.point);
            mapManager.SaveMap();
            view.SetAttainableNodes();
            view.SetLineColors();
            mapNode.ShowSwirlAnimation();

            // Use coroutine for delay instead of DOTween
            StartCoroutine(EnterNodeAfterDelay(mapNode, enterNodeDelay));
        }

        private System.Collections.IEnumerator EnterNodeAfterDelay(MapNode mapNode, float delay) {
            yield return new WaitForSeconds(delay);
            EnterNode(mapNode);
        }

        private static void EnterNode(MapNode mapNode) {
            // we have access to blueprint name here as well
            Debug.Log("Entering node: " + mapNode.Node.blueprintName + " of type: " + mapNode.Node.nodeType);
            // load appropriate scene with context based on nodeType:
            // or show appropriate GUI over the map: 
            // if you choose to show GUI in some of these cases, do not forget to set "Locked" in MapPlayerTracker back to false
            switch (mapNode.Node.nodeType) {
                case NodeType.Enemy:
                    break;
                case NodeType.Encounter:
                    break;
                case NodeType.Shop:
                    break;
                case NodeType.MiniBoss:
                    break;
                case NodeType.MajorBoss:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}