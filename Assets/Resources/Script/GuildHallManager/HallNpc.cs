using UnityEngine;

namespace StoryManager
{
    public class HallNpc : MonoBehaviour
    {
        public bool faceRight;
        [Range(0.5f,3f)] public float zoom = 1.25f;

        private void Start()
        {
            GameObject prefab = PickPrefab();
            if (prefab == null) return;
            GameObject inst = Instantiate(prefab);
            inst.SetActive(true);
            HideHudWidgets(inst);
            var (tex, cam) = RenderUtilities.RenderUnitToTexture(inst, zoom);
            Sprite sprite = RenderUtilities.ConvertRenderTextureToSprite(tex);
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr == null) sr = gameObject.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.flipX = !faceRight; 
            Destroy(tex);
            Destroy(inst);
        }

        private GameObject PickPrefab()
        {
            var pool = ResourceManager.instance.GetAllUnitPrefabs();
            if (pool.Count==0) return null;
            return pool[Random.Range(0,pool.Count)];
        }

        private static void HideHudWidgets(GameObject root)
        {
            if (root == null) return;

            Transform hb = root.transform.Find("UIHealthBar(Clone)");
            if (hb) hb.gameObject.SetActive(false);
            
            Transform eg = root.transform.Find("UIEffectGrid(Clone)");
            if (eg) eg.gameObject.SetActive(false);
        }
    }
} 