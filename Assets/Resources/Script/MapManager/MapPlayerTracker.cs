using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Map 
{
    /// Tracks and manages player interactions with the map nodes.
    /// Handles node selection, path progression, node accessibility, and triggers node entry logic.
    /// Ensures only valid nodes can be selected based on the current path and node connections.
    /// Supports locking after selection and delayed node entry for animation or transition effects
    public class MapPlayerTracker : MonoBehaviour 
    {
        /// If true, the tracker will lock after a node is selected, preventing further selections 
        /// until unlocked
        public bool lockAfterSelecting = false;
        /// Delay in seconds before entering a node after selection
        public float enterNodeDelay = 1f;
        /// Reference to the MapManager controlling the current map
        public MapManager mapManager;
        /// Reference to the MapView for updating node visuals and lines
        public MapView view;

        /// Singleton instance for global acces
        public static MapPlayerTracker Instance;

        /// Indicates whether node selection is currently locked
        public bool Locked { get; set; }

        private void Awake() 
        {
            /// Sets the singleton instance on Awake
            if (Instance != null) {
                Destroy(Instance);
            }
            Instance = this;
            Locked = false; // Reset lock on scene load
        }

        /// Attempts to select a node. Only allows selection if the node is accessible from the current path.
        /// If the path is empty, only nodes in the first layer (y == 0) can be selected.
        /// Otherwise, only nodes connected to the current node can be selected.
        public void SelectNode(MapNode mapNode) 
        {
            if (Locked) { return; }

            if (mapManager.CurrentMap.path.Count == 0) {
                if (mapNode.Node.point.y == 0) {
                    if (mapNode.Node.nodeType == NodeType.Enemy ||
                        mapNode.Node.nodeType == NodeType.MajorBoss ||
                        mapNode.Node.nodeType == NodeType.MiniBoss)
                    {
                        Deck cPlayerDeck = DeckManager.instance.GetDeckByType(eDeckType.PLAYER);
                        List<UnitObject> listAlive = DeckHelperFunc.GetAllAliveUnit(cPlayerDeck);
                        if (listAlive.Count <= 0) {
                            UIPopUpManager.instance.CreatePopUp("Deck is empty");
                            return;
                        }
                    }
                    
                    SendPlayerToNode(mapNode);
                } else {
                    Global.DEBUG_PRINT("[MapPlayerTracker::SelectNode] Selected node cannot be accessed");
                }
            } else {
                Vector2Int currentPoint = mapManager.CurrentMap.path[mapManager.CurrentMap.path.Count - 1];
                Node currentNode = mapManager.CurrentMap.GetNode(currentPoint);

                if (mapNode.Node.nodeType == NodeType.Enemy ||
                    mapNode.Node.nodeType == NodeType.MajorBoss ||
                    mapNode.Node.nodeType == NodeType.MiniBoss)
                {
                    Deck cPlayerDeck = DeckManager.instance.GetDeckByType(eDeckType.PLAYER);
                        List<UnitObject> listAlive = DeckHelperFunc.GetAllAliveUnit(cPlayerDeck);
                        if (listAlive.Count <= 0) {
                            UIPopUpManager.instance.CreatePopUp("Deck is empty");
                            return;
                        }
                }

                if (currentNode != null && currentNode.outgoing.Any(point => point.Equals(mapNode.Node.point))) {
                    SendPlayerToNode(mapNode);
                } else {
                    Global.DEBUG_PRINT("[MapPlayerTracker::SelectNode] Selected node cannot be accessed");
                }
            }
        }

        /// Handles the logic for moving the player to the selected node, updating 
        /// the path, saving the map, updating visuals,
        /// and starting the delayed node entry coroutine
        private void SendPlayerToNode(MapNode mapNode) 
        {
            Locked = lockAfterSelecting;
            mapManager.CurrentMap.path.Add(mapNode.Node.point);
            mapManager.SaveMap();
            view.SetAttainableNodes();
            view.SetLineColors();
            mapNode.ShowSwirlAnimation();

            // Use coroutine for delay
            StartCoroutine(EnterNodeAfterDelay(mapNode, enterNodeDelay));
        }

        /// Coroutine that waits for a specified delay before entering the node
        private System.Collections.IEnumerator EnterNodeAfterDelay(MapNode mapNode, float delay) 
        {
            yield return new WaitForSeconds(delay);
            EnterNode(mapNode);
        }

        /// Handles the logic for entering a node, such as loading a new scene or triggering events based on node type
        private void EnterNode(MapNode mapNode) 
        {
            // We have access to blueprint name here as well.
            Global.DEBUG_PRINT("[MapPlayerTracker::EnterNode] Entering node: " + mapNode.Node.blueprintName + " of type: " + mapNode.Node.nodeType);
            // Load appropriate scene with context based on nodeType:
            // If choose to show GUI in some of these cases, do not forget to set "Locked" in MapPlayerTracker back to false
            switch (mapNode.Node.nodeType) {
                case NodeType.Enemy:
                    mapManager.SaveMap();
                    EnemyManager.instance.nodeType = mapNode.Node.nodeType;
                    EnemyManager.instance.PopulateMobBasedOnNodeType();
                    SceneManager.LoadScene("Game");
                    break;
                case NodeType.Encounter:
                    mapManager.SaveMap();
                    SceneManager.LoadScene("Game_StoryTeller");
                    break;
                case NodeType.Necromancer:
                    mapManager.SaveMap();
                    SceneManager.LoadScene("Game_Necromancer");
                    break;
                case NodeType.Shop:
                    mapManager.SaveMap();
                    SceneManager.LoadScene("Game_Shop");
                    break;
                case NodeType.Blacksmith:
                    mapManager.SaveMap();
                    SceneManager.LoadScene("Game_Blacksmith");
                    break;
                case NodeType.MiniBoss:
                    mapManager.SaveMap();
                    EnemyManager.instance.nodeType = mapNode.Node.nodeType;
                    EnemyManager.instance.PopulateMobBasedOnNodeType();
                    SceneManager.LoadScene("Game");
                    break;
                case NodeType.MajorBoss:
                    mapManager.SaveMap();
                    EnemyManager.instance.nodeType = mapNode.Node.nodeType;
                    EnemyManager.instance.PopulateMobBasedOnNodeType();
                    SceneManager.LoadScene("Game");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            Locked = false;
        }

        public void CleanUpMap()
        {
            mapManager.CleanUpMap();
        }
    }
}