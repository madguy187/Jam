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
        [SerializeField] public GameObject recruitPanel;

        [Header("Config")]        
        [SerializeField] private Vector2 renderOffset = new Vector2(0f, -0.25f);
        [SerializeField] private Vector2 tooltipOffset = new Vector2(150f, 0f);

        [Header("Entry Animation")]
        [SerializeField] private Vector3 entryOffset = new Vector3(-3f, 0f, 0f); 
        [SerializeField] private float entryDuration = 1.5f;

        [Header("Dialogue")]
        [SerializeField] public DialogueManager dialogueManager;
        [TextArea(2,4)]
        [SerializeField] private string[] dialogueLines;

        private bool isInteractable = false;
        private bool hasOpenedRecruit = false;

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

            entryRoutine = StartCoroutine(EntrySequence());
            return;
        }

        private static void HideHudWidgets(GameObject root)
        {
            Transform healthBar = root.transform.Find("UIHealthBar(Clone)");
            if (healthBar != null)
            {
                healthBar.gameObject.SetActive(false);
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
            if (!isInteractable || hasOpenedRecruit) return;

            if (recruitPanel != null)
            {
                recruitPanel.SetActive(true);
                DestroyBackgroundNpcs();
            }

            // disable future clicks
            hasOpenedRecruit = true; 
        }

        public void OnPointerEnter(PointerEventData _)
        {
            if (!isInteractable || hasOpenedRecruit) return;
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

        private void DestroyBackgroundNpcs()
        {
            var npcs = FindObjectsOfType<MonoBehaviour>();
            foreach (var m in npcs)
            {
                if (m == null) continue;
                if (m.GetType().Name == "HallNpc")
                {
                    Destroy(m.gameObject);
                }
            }
        }
    }
}