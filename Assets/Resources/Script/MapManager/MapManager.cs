using System.Linq;
using UnityEngine;

namespace Map 
{
    /// Manages the lifecycle and persistence of the map in the game
    public class MapManager : MonoBehaviour 
    {
        /// The configuration used for map generation
        public MapConfig config;
        /// The view component responsible for displaying the map
        public MapView view;

        /// The currently active map instance
        public Map CurrentMap { get; private set; }

        /// Initializes the map manager and generates a new map at start
        private void Start() 
        {
            if (PlayerPrefs.HasKey("Map")) {
                string mapJson = PlayerPrefs.GetString("Map");
                // Deserialize using DTO
                MapDTO mapDTO = JsonUtility.FromJson<MapDTO>(mapJson);
                Map map = Map.FromDTO(mapDTO);
            
                if (map.path.Any(p => p.Equals(map.GetBossNode().point))) {
                    // player has already reached the boss, generate a new map
                    GenerateNewMap();
                } else {
                    CurrentMap = map;
                    // player has not reached the boss yet, load the current map
                    view.ShowMap(map);
                }
            } else {
                GenerateNewMap();
            }
        }

        /// Generates a new map using the current configuration and displays it
        public void GenerateNewMap() 
        {
            Map map = MapGenerator.GetMap(config);
            CurrentMap = map;
            Debug.Log(map.ToJson());
            view.ShowMap(map);
        }

        public void LoadNextConfig(string newConfigName) 
        {
            MapConfig newConfig = view.allMapConfigs.FirstOrDefault(c => c.name == newConfigName);
            if (newConfig != null) {
                config = newConfig;
                GenerateNewMap();
            } else {
                Global.DEBUG_PRINT($"[MapManager::LoadNextConfig] with name '{newConfigName}' not found in MapConfigs. Add it to allMapConfigs in MapView script");
            }
        }

        /// Saves the current map to PlayerPrefs as a JSON string
        public void SaveMap() 
        {
            if (CurrentMap == null) return;

            // Convert to DTO and serialize
            MapDTO mapDTO = CurrentMap.ToDTO();
            string json = JsonUtility.ToJson(mapDTO, true);
            PlayerPrefs.SetString("Map", json);
            var scrollPosition = view.GetScrollPosition();
            PlayerPrefs.SetFloat("ScrollPositionX", scrollPosition.x);
            PlayerPrefs.SetFloat("ScrollPositionY", scrollPosition.y);
            PlayerPrefs.SetFloat("ScrollPositionZ", scrollPosition.z);
            Global.DEBUG_PRINT("[MapManager::SaveMap] Map saved with scroll position: " + scrollPosition);
            PlayerPrefs.Save();
        }

        private void OnApplicationQuit() 
        {
            PlayerPrefs.DeleteKey("Map");
        }
    }
}