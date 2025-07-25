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
    [SerializeField] private float rollSpeed = 300f;
    [SerializeField] private float rollDuration = 2f;
    [SerializeField] private AnimationCurve speedCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Debug Visualization")]
    [SerializeField] private bool showGizmos = true;
    [SerializeField] private Color viewportColor = new Color(0, 1, 0, 0.2f);
    [SerializeField] private Color slotColor = new Color(1, 0, 0, 0.2f);
    [SerializeField] private Color pathColor = new Color(0, 0, 1, 0.5f);

    [Header("Render Settings")]
    [SerializeField] private float renderYOffset = -0.15f; 

    [Header("Tuning")]
    [SerializeField] private float snapYOffset = 8f; 

    private bool unitsCached = false;

    private bool isRolling = false;
    private List<UnitObject> cachedUnits = new List<UnitObject>();
    private List<Image> slotImages = new List<Image>();
    private float currentRollPosition = 0f;
    private float slotSpacing;
    private float viewportHeight;
    private float totalColumnHeight;
    private float bottomThreshold;
    private VerticalLayoutGroup layoutGroup;
    private bool slotsInitialized = false;

    // Map each Image slot to its current unit archetype for result extraction
    private readonly Dictionary<Image, eUnitArchetype> slotArchetypes = new Dictionary<Image, eUnitArchetype>();

    public bool IsRolling()
    {
        return isRolling;
    }

    public float GetRollSpeed()
    {
        return rollSpeed;
    }

    public void SetRollSpeed(float value)
    {
        rollSpeed = Mathf.Max(0f, value);
        return;
    }

    public float GetRollDuration()
    {
        return rollDuration;
    }

    public void SetRollDuration(float value)
    {
        rollDuration = Mathf.Max(0f, value);
        return;
    }


    public void Spin()
    {
        if (isRolling || !unitsCached) return;
        StartRoll();
    }

    private void Start()
    {
        Debug.Log("[SkillSlotGrid] Starting initialization");
        
        if (slotColumnContainer == null)
        {
            Debug.LogError("[SkillSlotGrid] SlotColumnContainer reference is missing!");
            return;
        }

        // Disable VerticalLayoutGroup if present so we can control positions manually
        layoutGroup = slotColumnContainer.GetComponent<VerticalLayoutGroup>();
        if (layoutGroup != null)
        {
            layoutGroup.enabled = false;
        }

        // Cache all slot images (children of container)
        Image[] images = slotColumnContainer.GetComponentsInChildren<Image>();
        if (images.Length == 0)
        {
            Debug.LogError("[SkillSlotGrid] No Image components found in children!");
            return;
        }
        slotImages.AddRange(images);
        Debug.Log($"[SkillSlotGrid] Found {slotImages.Count} slot images");

        // distance from centre of one slot to the next
        slotSpacing      = slotHeight + spacing;        
        // height of the visible 3-slot window           
        viewportHeight   = slotSpacing * visibleSlots - spacing;   
        // distance to move a slot from bottom back to top
        totalColumnHeight = slotSpacing * slotImages.Count;        
        // y where we recycle
        bottomThreshold   = -(viewportHeight / 2f) - (slotSpacing * 0.5f); 
    }

    private void OnDestroy()
    {

    }

    private void Update()
    {
        // Try to cache units automatically once at start
        if (!unitsCached)
        {
            if (CachePlayerDeckUnits())
            {
                unitsCached = true;
                PositionSlotsInitial();
            }
        }
    }

    private bool CachePlayerDeckUnits()
    {
        cachedUnits.Clear();
        
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

        // Only randomise icons once up-front; do not touch visible slots again during spin
        foreach (Image img in slotImages)
        {
            RandomizeIcon(img);
        }
    }

    private void RandomizeIcon(Image img)
    {
        if (cachedUnits.Count == 0) return;

        UnitObject randomUnit = cachedUnits[Random.Range(0, cachedUnits.Count)];

        // Track archetype for result extraction later
        if (slotArchetypes.ContainsKey(img))
        {
            slotArchetypes[img] = randomUnit.unitSO.eUnitArchetype;
        }
        else
        {
            slotArchetypes.Add(img, randomUnit.unitSO.eUnitArchetype);
        }

        var (rt, cam) = RenderUnitToTexture(randomUnit);

        // Create sprite from render texture
        Sprite sprite = Sprite.Create(rt.ToTexture2D(), new Rect(0, 0, rt.width, rt.height), new Vector2(0.5f, 0.5f));
        img.sprite = sprite;
        img.color = Color.white;

        rt.Release();
        Destroy(rt);
        Destroy(cam);
    }

    public List<eUnitArchetype> GetVisibleArchetypes()
    {
        List<(Image img, float y)> candidates = new List<(Image, float)>();
        foreach (Image img in slotImages)
        {
            float y = img.rectTransform.anchoredPosition.y;
            if (Mathf.Abs(y) <= slotSpacing + 0.01f) 
            {
                candidates.Add((img, y));
            }
        }

        // Sort descending Y 
        candidates.Sort((a, b) => b.y.CompareTo(a.y));

        List<eUnitArchetype> result = new List<eUnitArchetype>();
        for (int i = 0; i < Mathf.Min(3, candidates.Count); i++)
        {
            Image img = candidates[i].img;
            if (slotArchetypes.TryGetValue(img, out eUnitArchetype arch))
            {
                result.Add(arch);
            }
            else
            {
                result.Add(eUnitArchetype.NONE);
            }
        }
        return result;
    }

    private void PositionSlotsInitial()
    {
        float startY = slotSpacing;         
        for (int i = 0; i < slotImages.Count; i++)
        {
            RectTransform rt = slotImages[i].rectTransform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot     = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(slotHeight, slotHeight);
            rt.anchoredPosition = new Vector2(0f, startY - (i * slotSpacing));
        }
        slotsInitialized = true;
    }

    private void StartRoll()
    {
        if (isRolling) return;
        if (!unitsCached) return; 
        isRolling = true;
        StartCoroutine(RollAnimation());
    }

    private IEnumerator RollAnimation()
    {
        float elapsed = 0f;
        while (elapsed < rollDuration)
        {
            float speedFactor = speedCurve.Evaluate(elapsed / rollDuration); 
            float deltaY = rollSpeed * speedFactor * Time.deltaTime;

            // Move each slot downwards
            foreach (Image img in slotImages)
            {
                RectTransform rt = img.rectTransform;
                Vector2 pos = rt.anchoredPosition;
                pos.y -= deltaY;

                // If slot has moved below recycle threshold, wrap it to the top and randomize
                if (pos.y < bottomThreshold)
                {
                    pos.y += totalColumnHeight;
                    rt.anchoredPosition = pos;
                    // ONLY this slot gets a new sprite
                    RandomizeIcon(img); 
                }
                else
                {
                    rt.anchoredPosition = pos;
                }
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Final alignment
        // Find the slot whose centre is closest to the viewport centre  ...
        Image nearest = null;
        float minAbsY = float.MaxValue;
        foreach (Image img in slotImages)
        {
            float y = img.rectTransform.anchoredPosition.y;
            float abs = Mathf.Abs(y);
            if (abs < minAbsY)
            {
                minAbsY = abs;
                nearest = img;
            }
        }
        if (nearest != null)
        {
            // how far we are from perfect centre
            float offset = nearest.rectTransform.anchoredPosition.y;
            // Shift every slot by -offset so that the nearest slot is perfectly centred
            foreach (Image img in slotImages)
            {
                RectTransform rt = img.rectTransform;
                Vector2 p = rt.anchoredPosition;
                p.y -= offset;
                // magic number to offset
                p.y -= snapYOffset; 
                rt.anchoredPosition = p;
            }
        }

        isRolling = false;
    }

    private void SetLayerRecursively(GameObject obj, int newLayer)
    {
        obj.layer = newLayer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }

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

        cam.transform.position = unit.transform.position + new Vector3(0, renderYOffset, -10);
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
    
} 