using System.Linq;
using UnityEngine;

namespace Map {
    public class MapManager : MonoBehaviour {
        public MapConfig config;
        public MapView view;

        public Map CurrentMap { get; private set; }

        private void Start() {
            // PlayerPrefs.DeleteKey("Map");
            // PlayerPrefs.Save();
            // if (PlayerPrefs.HasKey("Map")) {
            //     string mapJson = PlayerPrefs.GetString("Map");
            //     // Deserialize using DTO
            //     MapDTO mapDTO = JsonUtility.FromJson<MapDTO>(mapJson);
            //     Map map = Map.FromDTO(mapDTO);
            // 
            //     if (map.path.Any(p => p.Equals(map.GetBossNode().point))) {
            //         // player has already reached the boss, generate a new map
            //         GenerateNewMap();
            //     } else {
            //         CurrentMap = map;
            //         // player has not reached the boss yet, load the current map
            //         view.ShowMap(map);
            //     }
            // } else {
            //     GenerateNewMap();
            // }
            GenerateNewMap();
        }

        public void GenerateNewMap() {
            Map map = MapGenerator.GetMap(config);
            CurrentMap = map;
            Debug.Log(map.ToJson());
            view.ShowMap(map);
        }

        public void SaveMap() {
            if (CurrentMap == null) return;

            // Convert to DTO and serialize
            MapDTO mapDTO = CurrentMap.ToDTO();
            string json = JsonUtility.ToJson(mapDTO, true);
            PlayerPrefs.SetString("Map", json);
            PlayerPrefs.Save();
        }

        private void OnApplicationQuit() {
            // SaveMap();
        }
    }
}