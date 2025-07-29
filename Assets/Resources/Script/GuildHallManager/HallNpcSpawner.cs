using UnityEngine;

namespace StoryManager
{

    public class HallNpcSpawner : MonoBehaviour
    {
        [Header("Spawn Area (world units)")]
        [SerializeField] private float minX = -6f;
        [SerializeField] private float maxX = 6f;
        [SerializeField] private float y = 1f;
        [SerializeField] private float minY = 1f;
        [SerializeField] private float maxY = 2.5f;

        [Header("Spawn Settings")]
        [SerializeField, Range(1, 10)] private int pairCount = 3;
        [SerializeField] private float pairSpacing = 0.8f; 
        [SerializeField] private float pairPadding = 1f;   

        private void Start()
        {
            float segmentWidth = (maxX - minX) / pairCount;
            for (int i = 0; i < pairCount; i++)
            {
                float segStart = minX + i * segmentWidth + pairPadding;
                float segEnd = minX + (i + 1) * segmentWidth - pairPadding - pairSpacing;
                if (segEnd <= segStart) continue;

                float centerX = Random.Range(segStart, segEnd);
                float yPos = Random.Range(minY, maxY);

                Vector3 leftPos = new Vector3(centerX - pairSpacing * 0.5f, yPos, 0f);
                Vector3 rightPos = new Vector3(centerX + pairSpacing * 0.5f, yPos, 0f);

                Spawn(leftPos, faceRight: true);  
                Spawn(rightPos, faceRight: false); 
            }
        }

        private void Spawn(Vector3 position, bool faceRight)
        {
            GameObject go = new GameObject("HallNPC");
            go.transform.SetParent(transform);
            go.transform.position = position;
            var spawn = go.AddComponent<HallNpc>();
            spawn.faceRight = faceRight;
        }
    }
} 