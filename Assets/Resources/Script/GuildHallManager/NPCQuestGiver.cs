using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace StoryManager
{
    [RequireComponent(typeof(Collider2D))]
    public class NPCQuestGiver : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {

        public static NPCQuestGiver instance { get; private set; }

        [Header("General")]
        [SerializeField] private string tooltipText = "Click to recruit your adventurers!";
        [SerializeField] private GameObject recruitPanel;

        [Header("Config")]
        [SerializeField] private RawImage previewImage;           
        [SerializeField] private GameObject unitPrefab;          
        [SerializeField, Min(0.01f)] private float cameraSize = 0.4f;  
        [SerializeField] private Vector2 renderOffset = new Vector2(0f, -0.25f);
        [SerializeField] private bool faceRight = true;
        [SerializeField] private Vector2 tooltipOffset = new Vector2(150f, 0f);

        [Header("Entry Animation")]
        [SerializeField] private Vector3 entryOffset = new Vector3(-3f, 0f, 0f); 
        [SerializeField] private float entryDuration = 1.5f;

        [Header("Dialogue")]
        [SerializeField] private DialogueManager dialogueManager;
        [TextArea(2,4)]
        [SerializeField] private string[] dialogueLines;

        private bool isInteractable = false;

        private RenderTexture previewTexture;
        private Camera previewCamera;
        private GameObject previewUnit;
        private const string UI_LAYER_NAME = "UI";
        private int uiLayer; 
        private Coroutine entryRoutine;

        private void Awake()
        {
            instance = this;
            uiLayer = LayerMask.NameToLayer(UI_LAYER_NAME);
        }

        private void Start()
        {
            if (recruitPanel != null)
            {
                recruitPanel.SetActive(false);
            }

            if (previewImage == null)
            {
                Debug.LogError("NPCQuestGiver: Preview RawImage reference is missing.");
                return;
            }

            SelectUnitPrefab();

            if (unitPrefab == null)
            {
                return;
            }

            CreatePreviewCamera();
            SpawnPreviewUnit();
            entryRoutine = StartCoroutine(EntrySequence());
        }

        private void SelectUnitPrefab()
        {
            if (unitPrefab != null)
            {
                return;
            }

            if (ResourceManager.instance == null)
            {
                return;
            }

            var pool = ResourceManager.instance.GetAllUnitPrefabs();
            if (pool.Count > 0)
            {
                int randomIndex = Random.Range(0, pool.Count);
                unitPrefab = pool[randomIndex];
            }
        }

        private void CreatePreviewCamera()
        {
            Rect rect = previewImage.rectTransform.rect;
            int width = Mathf.CeilToInt(rect.width);
            int height = Mathf.CeilToInt(rect.height);

            previewTexture = new RenderTexture(width, height, 16)
            {
                antiAliasing = 1
            };
            previewImage.texture = previewTexture;

            GameObject cameraObject = new GameObject("NPCPreviewCamera");
            cameraObject.transform.SetParent(transform, false);
            cameraObject.transform.localPosition = new Vector3(0f, 0f, -1f);

            previewCamera = cameraObject.AddComponent<Camera>();
            previewCamera.clearFlags = CameraClearFlags.SolidColor;
            previewCamera.backgroundColor = Color.clear;
            previewCamera.orthographic = true;
            previewCamera.orthographicSize = cameraSize;
            previewCamera.cullingMask = 1 << uiLayer;
            previewCamera.targetTexture = previewTexture;
        }

        private void SpawnPreviewUnit()
        {
            Vector3 spawnPosition = previewCamera.transform.position + Vector3.forward + (Vector3)renderOffset;
            previewUnit = Instantiate(unitPrefab, spawnPosition, Quaternion.identity);

            Vector3 scale = Vector3.one;
            if (faceRight)
            {
                scale.x *= -1f;  
            }
            previewUnit.transform.localScale = scale;

            SetLayerRecursive(previewUnit, uiLayer);
            HideHudWidgets(previewUnit);
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

        private static void SetLayerRecursive(GameObject node, int layer)
        {
            node.layer = layer;
            foreach (Transform child in node.transform)
            {
                SetLayerRecursive(child.gameObject, layer);
            }
        }

        public void OnPointerClick(PointerEventData _)
        {
            if (!isInteractable) return;
            if (recruitPanel != null)
            {
                recruitPanel.SetActive(true);
            }
        }

        public void OnPointerEnter(PointerEventData _)
        {
            if (!isInteractable) return;
            if (recruitPanel != null && recruitPanel.activeInHierarchy)
            {
                return; 
            }

            if (TooltipSystem.instance != null)
            {
                Vector3 tooltipPosition = Input.mousePosition + (Vector3)tooltipOffset;
                TooltipSystem.Show(tooltipText, tooltipPosition);
            }
        }

        public void OnPointerExit(PointerEventData _)
        {
            if (TooltipSystem.instance != null)
            {
                TooltipSystem.Hide();
            }
        }

        private IEnumerator EntrySequence()
        {
            // move NPC from start offset to original position
            Vector3 targetPos = transform.position;
            transform.position = targetPos + entryOffset;

            float elapsed = 0f;
            while (elapsed < entryDuration)
            {
                transform.position = Vector3.Lerp(targetPos + entryOffset, targetPos, elapsed / entryDuration);
                elapsed += Time.deltaTime;
                yield return null;
            }
            transform.position = targetPos;

            // run dialogue
            if (dialogueManager != null && dialogueLines != null && dialogueLines.Length > 0)
            {
                dialogueManager.StartDialogue(dialogueLines, () => { isInteractable = true; });
            }
            else
            {
                isInteractable = true;
            }
        }

        private void OnDestroy()
        {
            if (entryRoutine != null)
            {
                StopCoroutine(entryRoutine);
                entryRoutine = null;
            }

            if (previewTexture != null)
            {
                previewTexture.Release();
                Destroy(previewTexture);
            }

            if (previewCamera != null)
            {
                Destroy(previewCamera.gameObject);
            }

            if (previewUnit != null)
            {
                Destroy(previewUnit);
            }
        }

        private void OnDisable()
        {
            TooltipSystem.Hide();

            if (entryRoutine != null)
            {
                StopCoroutine(entryRoutine);
                entryRoutine = null;
            }
        }
    }
}