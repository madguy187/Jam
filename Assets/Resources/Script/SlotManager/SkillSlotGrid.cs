using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public static class RenderTextureExtensions
{
    public static Texture2D ToTexture2D(this RenderTexture rTex)
    {
        Texture2D tex = new Texture2D(rTex.width, rTex.height, TextureFormat.RGBA32, false);
        var old_rt = RenderTexture.active;
        RenderTexture.active = rTex;

        tex.ReadPixels(new Rect(0, 0, rTex.width, rTex.height), 0, 0);
        tex.Apply();

        RenderTexture.active = old_rt;
        return tex;
    }
}

public class SkillSlotGrid : MonoBehaviour
{
    [Header("Grid Settings")]
    [SerializeField] private RectTransform slotColumnContainer;
    [SerializeField] private float slotHeight = 40f;
    [SerializeField] private float spacing = 5f;
    [SerializeField] private int visibleSlots = 3;

    [Header("Roll Settings")]
    [SerializeField] private float rollSpeed = 500f;
    [SerializeField] private float snapDuration = 0.5f;

    [Header("Debug Visualization")]
    [SerializeField] private bool showGizmos = true;
    [SerializeField] private Color viewportColor = new Color(0, 1, 0, 0.2f);
    [SerializeField] private Color slotColor = new Color(1, 0, 0, 0.2f);
    [SerializeField] private Color pathColor = new Color(0, 0, 1, 0.5f);

    private bool isRolling = false;
    private List<UnitObject> cachedUnits = new List<UnitObject>();
    private List<Image> slotImages = new List<Image>();
    private float startY;
    private float targetY;
    private List<RenderTexture> renderTextures = new List<RenderTexture>();
    private List<GameObject> tempCameras = new List<GameObject>();

    private void Start()
    {
        Debug.Log("[SkillSlotGrid] Starting initialization");
        
        if (slotColumnContainer == null)
        {
            Debug.LogError("[SkillSlotGrid] SlotColumnContainer reference is missing!");
            return;
        }

        // Cache all slot images
        Image[] images = slotColumnContainer.GetComponentsInChildren<Image>();
        if (images.Length == 0)
        {
            Debug.LogError("[SkillSlotGrid] No Image components found in children!");
            return;
        }
        
        slotImages.AddRange(images);
        Debug.Log($"[SkillSlotGrid] Found {slotImages.Count} slot images");

        // Set initial position
        startY = slotColumnContainer.anchoredPosition.y;
        Debug.Log($"[SkillSlotGrid] Initial Y position: {startY}");
    }

    private void OnDestroy()
    {
        // Cleanup render textures and cameras
        CleanupRenderResources();
    }

    private void CleanupRenderResources()
    {
        foreach (var rt in renderTextures)
        {
            if (rt != null)
            {
                rt.Release();
                Destroy(rt);
            }
        }
        renderTextures.Clear();

        foreach (var cam in tempCameras)
        {
            if (cam != null)
            {
                Destroy(cam);
            }
        }
        tempCameras.Clear();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            Debug.Log("[SkillSlotGrid] Key 9 pressed");
            StartRoll();
        }
    }

    private bool CachePlayerDeckUnits()
    {
        cachedUnits.Clear();
        
        if (DeckManager.instance == null)
        {
            Debug.LogError("[SkillSlotGrid] DeckManager instance is null!");
            return false;
        }

        Deck playerDeck = DeckManager.instance.GetDeckByType(eDeckType.PLAYER);
        if (playerDeck == null)
        {
            Debug.LogError("[SkillSlotGrid] Could not get player deck!");
            return false;
        }

        // Get all units from player deck
        for (int i = 0; i < playerDeck.GetDeckMaxSize(); i++)
        {
            UnitObject unit = playerDeck.GetUnitObject(i);
            if (unit != null && !unit.IsDead())
            {
                cachedUnits.Add(unit);
                Debug.Log($"[SkillSlotGrid] Added unit to cache: {unit.unitSO.unitName}");
            }
        }

        Debug.Log($"[SkillSlotGrid] Cached {cachedUnits.Count} player units");
        return cachedUnits.Count > 0;
    }

    private void RefreshSlotIcons()
    {
        if (cachedUnits.Count == 0)
        {
            Debug.LogWarning("[SkillSlotGrid] No units available to display!");
            return;
        }

        // Cleanup previous render resources
        CleanupRenderResources();

        Debug.Log("[SkillSlotGrid] Refreshing slot icons");
        foreach (Image img in slotImages)
        {
            UnitObject randomUnit = cachedUnits[Random.Range(0, cachedUnits.Count)];
            var (rt, cam) = RenderUnitToTexture(randomUnit);
            
            // Create sprite from render texture
            Sprite sprite = Sprite.Create(rt.ToTexture2D(), new Rect(0, 0, rt.width, rt.height), new Vector2(0.5f, 0.5f));
            img.sprite = sprite;
            img.color = Color.white; // Make sure image is visible

            // Store resources for cleanup
            renderTextures.Add(rt);
            tempCameras.Add(cam);
        }
    }

    private void StartRoll()
    {
        if (isRolling)
        {
            Debug.LogWarning("[SkillSlotGrid] Already rolling!");
            return;
        }

        // Cache units when starting roll
        if (!CachePlayerDeckUnits())
        {
            Debug.LogWarning("[SkillSlotGrid] Cannot start roll: No units available");
            return;
        }

        Debug.Log("[SkillSlotGrid] Starting roll animation");
        isRolling = true;
        
        // Calculate how far to roll (3 full slot heights)
        float rollDistance = (slotHeight + spacing) * 3;
        targetY = startY - rollDistance;

        StartCoroutine(RollAnimation());
    }

    private IEnumerator RollAnimation()
    {
        Debug.Log("[SkillSlotGrid] Roll animation started");
        float elapsedTime = 0f;
        float rollTime = Mathf.Abs(targetY - startY) / rollSpeed;
        Vector2 startPos = slotColumnContainer.anchoredPosition;
        Vector2 targetPos = new Vector2(startPos.x, targetY);

        while (elapsedTime < rollTime)
        {
            float t = elapsedTime / rollTime;
            slotColumnContainer.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        Debug.Log("[SkillSlotGrid] Roll animation completed");
        // Reset position and refresh icons
        slotColumnContainer.anchoredPosition = new Vector2(startPos.x, startY);
        RefreshSlotIcons();
        
        isRolling = false;
    }

    // Helper to set layer recursively
    private void SetLayerRecursively(GameObject obj, int newLayer)
    {
        obj.layer = newLayer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }

    // Return both RenderTexture and camera GameObject
    private (RenderTexture, GameObject) RenderUnitToTexture(UnitObject unit)
    {
        // Create a preview layer
        int previewLayer = 31;

        // Store original layer
        int originalLayer = unit.gameObject.layer;

        // Set unit and all children to preview layer
        SetLayerRecursively(unit.gameObject, previewLayer);

        GameObject camObj = new GameObject("UnitPreviewCamera");
        Camera cam = camObj.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.Color;
        cam.backgroundColor = Color.clear;
        cam.orthographic = true;

        RenderTexture rt = new RenderTexture(256, 256, 16);
        cam.targetTexture = rt;

        cam.transform.position = unit.transform.position + new Vector3(0, 0, -10);
        cam.orthographicSize = 1.5f;

        // Only render the preview layer
        cam.cullingMask = 1 << previewLayer;

        cam.Render();

        // Restore original layer
        SetLayerRecursively(unit.gameObject, originalLayer);

        cam.enabled = false;
        camObj.SetActive(false);

        return (rt, camObj);
    }

    private void OnDrawGizmos()
    {
        if (!showGizmos || slotColumnContainer == null) return;

        // Cache world positions
        Vector3[] corners = new Vector3[4];
        slotColumnContainer.GetWorldCorners(corners);
        float containerWidth = corners[2].x - corners[0].x;

        // Draw viewport area (area where slots are visible)
        float viewportHeight = (slotHeight + spacing) * visibleSlots - spacing;
        Vector3 viewportCenter = transform.position;
        Gizmos.color = viewportColor;
        Gizmos.DrawCube(viewportCenter, new Vector3(containerWidth, viewportHeight, 1));

        // Draw all slot positions
        Gizmos.color = slotColor;
        for (int i = 0; i < slotImages.Count; i++)
        {
            if (slotImages[i] != null)
            {
                Vector3 slotPos = slotImages[i].transform.position;
                Gizmos.DrawCube(slotPos, new Vector3(slotHeight, slotHeight, 1));
            }
        }

        // Draw roll path when rolling
        if (isRolling)
        {
            Gizmos.color = pathColor;
            Vector3 start = new Vector3(viewportCenter.x, startY, viewportCenter.z);
            Vector3 target = new Vector3(viewportCenter.x, targetY, viewportCenter.z);
            Gizmos.DrawLine(start, target);
            
            // Draw arrow at target
            float arrowSize = 10f;
            Vector3 right = new Vector3(arrowSize, 0, 0);
            Vector3 up = new Vector3(0, arrowSize, 0);
            Gizmos.DrawLine(target, target + right + up);
            Gizmos.DrawLine(target, target - right + up);
        }
    }

    // Optional: Add runtime debug visualization
    private void OnGUI()
    {
        if (!showGizmos) return;

        GUILayout.BeginArea(new Rect(10, 10, 200, 100));
        GUILayout.Label($"Rolling: {isRolling}");
        GUILayout.Label($"Start Y: {startY:F1}");
        GUILayout.Label($"Target Y: {targetY:F1}");
        GUILayout.Label($"Current Y: {slotColumnContainer.anchoredPosition.y:F1}");
        GUILayout.EndArea();
    }
} 