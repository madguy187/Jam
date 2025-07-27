using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace StoryManager
{
    public class UnitSlot : MonoBehaviour
    {
        [Header("UI Refs")]
        [SerializeField] private Button rerollButton;
        [SerializeField] private TMP_Text rerollText;
        [SerializeField] private RectTransform previewAnchor;
        [SerializeField] private RawImage previewImage;

        [Header("Config")]
        [SerializeField] private int maxRerolls = 3;
        [SerializeField] private float cameraSize = 0.6f;
        [SerializeField] private Vector2 renderOffset = new Vector2(0f, -0.3f);

        private int rerollsLeft;
        private GameObject currentPrefab;
        private GameObject currentPreview;
        private RenderTexture previewTexture;
        private Camera previewCamera;

        public GameObject CurrentPrefab => currentPrefab;

        private static List<GameObject> recruitPool;

        private void Awake()
        {
            CreatePreviewRenderTargets();
        }

        private void OnDestroy()
        {
            if (previewTexture != null)
            {
                previewTexture.Release();
                Destroy(previewTexture);
            }

            if (previewCamera != null)
            {
                Destroy(previewCamera.gameObject);
            }
        }

        private void CreatePreviewRenderTargets()
        {
            if (previewImage == null)
            {
                return;
            }

            Rect rect = previewImage.rectTransform.rect;
            previewTexture = new RenderTexture((int)rect.width, (int)rect.height, 16)
            {
                antiAliasing = 1
            };
            previewImage.texture = previewTexture;

            GameObject cameraObject = new GameObject($"PreviewCamera_{name}");
            cameraObject.transform.SetParent(transform);
            cameraObject.transform.localPosition = new Vector3(0f, 0f, -1f);

            previewCamera = cameraObject.AddComponent<Camera>();
            previewCamera.clearFlags = CameraClearFlags.SolidColor;
            previewCamera.backgroundColor = Color.clear;
            previewCamera.orthographic = true;
            previewCamera.orthographicSize = cameraSize;
            previewCamera.targetTexture = previewTexture;
            previewCamera.cullingMask = 1 << LayerMask.NameToLayer("UI");
        }

        private void UpdateCameraSettings()
        {
            if (previewCamera != null)
            {
                previewCamera.orthographicSize = cameraSize;
            }
        }

        public static void InitialisePool(List<GameObject> prefabs)
        {
            recruitPool = prefabs;
        }

        public void SetAnchor(Transform anchor)
        {
            if (anchor != null)
            {
                previewAnchor = anchor as RectTransform;
            }
        }

        public void Init()
        {
            rerollsLeft = maxRerolls;

            if (rerollButton != null)
            {
                rerollButton.onClick.RemoveAllListeners();
                rerollButton.onClick.AddListener(HandleReroll);
            }

            HandleReroll(); 
        }

        public void DestroyPreview()
        {
            if (currentPreview != null)
            {
                Destroy(currentPreview);
                currentPreview = null;
            }
        }

        public void RefreshPreviewPosition()
        {
            if (currentPreview == null || previewCamera == null)
            {
                return;
            }

            Vector3 basePosition = previewCamera.transform.position + Vector3.forward + (Vector3)renderOffset;
            currentPreview.transform.position = basePosition;
        }

        private void HandleReroll()
        {
            if (rerollsLeft <= 0)
            {
                return;
            }

            if (recruitPool == null || recruitPool.Count == 0)
            {
                return;
            }

            int randomIndex = Random.Range(0, recruitPool.Count);
            currentPrefab = recruitPool[randomIndex];

            SpawnPreview();
            rerollsLeft--;
            UpdateRerollUI();
        }

        private void SpawnPreview()
        {
            if (previewCamera == null)
            {
                return;
            }

            DestroyPreview();

            Vector3 spawnPosition = previewCamera.transform.position + Vector3.forward + (Vector3)renderOffset;
            currentPreview = Instantiate(currentPrefab, spawnPosition, Quaternion.identity);
            currentPreview.transform.localScale = Vector3.one;

            SetLayerRecursive(currentPreview, LayerMask.NameToLayer("UI"));
            HideHudWidgets(currentPreview);
        }

        private static void SetLayerRecursive(GameObject node, int layer)
        {
            node.layer = layer;
            foreach (Transform child in node.transform)
            {
                SetLayerRecursive(child.gameObject, layer);
            }
        }

        private static void HideHudWidgets(GameObject root)
        {
            Transform healthBar = root.transform.Find("UIHealthBar(Clone)");
            if (healthBar != null)
            {
                healthBar.gameObject.SetActive(false);
            }

            Transform effectGrid = root.transform.Find("UIEffectGrid(Clone)");
            if (effectGrid != null)
            {
                effectGrid.gameObject.SetActive(false);
            }
        }

        private void UpdateRerollUI()
        {
            if (rerollText != null)
            {
                int shownValue = Mathf.Max(rerollsLeft, 0);
                rerollText.text = $"Rerolls left: {shownValue}";
            }

            if (rerollButton != null)
            {
                rerollButton.interactable = rerollsLeft > 0;
            }
        }
    }
}