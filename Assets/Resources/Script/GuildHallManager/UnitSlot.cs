using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

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

        [SerializeField, Min(0.1f)] private float animationInterval = 2f; 

        private Coroutine animationRoutine;

        private const string UI_LAYER_NAME = "UI";
        private int uiLayer;
        private static readonly Dictionary<RuntimeAnimatorController, List<AnimationClip>> ValidClipCache = new();

        public GameObject CurrentPrefab => currentPrefab;

        private static List<GameObject> recruitPool;

        private void Awake()
        {
            uiLayer = LayerMask.NameToLayer(UI_LAYER_NAME);
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
            previewCamera.cullingMask = 1 << uiLayer;
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
            // Stop animation first
            if (animationRoutine != null)
            {
                StopCoroutine(animationRoutine);
                animationRoutine = null;
            }

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

            // Stop any previous animation loop
            if (animationRoutine != null)
            {
                StopCoroutine(animationRoutine);
                animationRoutine = null;
            }

            Vector3 spawnPosition = previewCamera.transform.position + Vector3.forward + (Vector3)renderOffset;
            currentPreview = Instantiate(currentPrefab, spawnPosition, Quaternion.identity);
            currentPreview.transform.localScale = Vector3.one;

            SetLayerRecursive(currentPreview, uiLayer);
            HideHudWidgets(currentPreview);

            // Play a random animation clip if an Animator is present
            PlayRandomAnimation(currentPreview);

            // Start looping random animations
            animationRoutine = StartCoroutine(AnimationLoop(currentPreview));
        }

        private void PlayRandomAnimation(GameObject target)
        {
            if (target == null) return;

            // Try to fetch an Animator on the root first, then search children
            Animator anim = target.GetComponent<Animator>();
            if (anim == null)
            {
                anim = target.GetComponentInChildren<Animator>();
            }

            if (anim == null || anim.runtimeAnimatorController == null)
            {
                return;
            }

            var validClips = GetValidClips(anim.runtimeAnimatorController);
            if (validClips.Count == 0) return;

            AnimationClip randomClip = validClips[Random.Range(0, validClips.Count)];
            if (randomClip != null)
            {
                anim.Play(randomClip.name, 0, 0f);
            }
        }

        private IEnumerator AnimationLoop(GameObject target)
        {
            if (target == null) yield break;

            Animator anim = target.GetComponent<Animator>();
            if (anim == null) anim = target.GetComponentInChildren<Animator>();
            if (anim == null || anim.runtimeAnimatorController == null) yield break;

            var validClips = GetValidClips(anim.runtimeAnimatorController);
            if (validClips.Count == 0) yield break;

            // Added null checks
            while (target != null && target == currentPreview && anim != null) 
            {
                // Extra safety check
                if (anim == null || !anim.gameObject.activeInHierarchy)
                {
                    yield break;
                }

                AnimationClip clip = validClips[Random.Range(0, validClips.Count)];
                if (clip != null)
                {
                    try
                    {
                        anim.Play(clip.name, 0, 0f);
                    }
                    catch (MissingReferenceException)
                    {
                        // Exit if animator was destroyed
                        yield break; 
                    }
                }
                yield return new WaitForSeconds(animationInterval);
            }
        }

        private void OnDisable()
        {
            if (animationRoutine != null)
            {
                StopCoroutine(animationRoutine);
                animationRoutine = null;
            }

            DestroyPreview();
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

        private static List<AnimationClip> GetValidClips(RuntimeAnimatorController controller)
        {
            if (controller == null) return new List<AnimationClip>();

            if (ValidClipCache.TryGetValue(controller, out var cached))
            {
                return cached;
            }

            var clips = controller.animationClips ?? new AnimationClip[0];
            var valid = clips.Where(c => c != null && !c.name.ToLower().Contains("death")).ToList();
            ValidClipCache[controller] = valid;
            return valid;
        }
    }
}