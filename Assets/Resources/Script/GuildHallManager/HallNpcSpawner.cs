using System.Collections.Generic;
using UnityEngine;

namespace StoryManager
{
    public class HallNpcSpawner : MonoBehaviour
    {
        [Header("Spawn Settings")]
        [SerializeField] private int pairCount = 3;
        [SerializeField] private int rowCount = 2;
        [SerializeField] private float rowSpacing = 1.2f;
        [SerializeField] private float rowHeightFactor = 1.6f;
        [SerializeField] private float minX = -6f;
        [SerializeField] private float maxX = 6f;
        [SerializeField] private float spacing = 1.0f;
        [SerializeField] private bool alignToPlayerDeckY = true;
        [SerializeField] private float fixedY = -3.8f;

        private void Start()
        {
            SpawnNpcPairs();
        }

        private void SpawnNpcPairs()
        {
            if (pairCount <= 0 || rowCount <= 0) return;
            if (ResourceManager.instance == null) return;

            float baseY = fixedY;
            if (alignToPlayerDeckY && DeckManager.instance != null)
            {
                var posArr = DeckManager.instance.GetAllPositionByType(eDeckType.PLAYER);
                if (posArr.Length > 0)
                    baseY = posArr[0].transform.position.y;
            }

            for (int row = 0; row < rowCount; row++)
            {
                float yPos = baseY + rowSpacing * rowHeightFactor * row;
                float step = (maxX - minX) / Mathf.Max(1, pairCount); 

                for (int i = 0; i < pairCount; i++)
                {
                    float centreX = minX + step * (i + 0.5f) + Random.Range(-step * 0.3f, step * 0.3f);
                    float halfGap = spacing * 0.55f; 

                    // Left NPC faces right
                    Vector3 leftPos = new Vector3(centreX - halfGap, yPos, 0f);
                    SpawnSingleNpc(leftPos, faceRight: true);

                    // Right NPC aces left
                    Vector3 rightPos = new Vector3(centreX + halfGap, yPos, 0f);
                    SpawnSingleNpc(rightPos, faceRight: false);
                }
            }
        }

        private void SpawnSingleNpc(Vector3 worldPos, bool faceRight)
        {
            string unitName = ResourceManager.instance.Debug_RandUnit();
            if (string.IsNullOrEmpty(unitName)) return;

            GameObject prefab = ResourceManager.instance.GetUnit(unitName);
            if (prefab == null) return;

            UnitObject unit = ResourceManager.instance.CreateUnit(prefab, isEnemy: false);
            if (unit == null) return;

            unit.transform.position = worldPos;
            Vector3 scale = unit.transform.localScale;
            scale.x = Mathf.Abs(scale.x) * (faceRight ? -1f : 1f);
            unit.transform.localScale = scale;
            unit.gameObject.AddComponent<HallNpc>();
        }
    }
}