using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

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

    [Header("Render Settings")]
    [SerializeField] private float renderYOffset = -0.15f; 

    [Header("Tuning")]
    [SerializeField] private float snapYOffset = 8f; 

    private bool isRolling = false;
    private List<UnitObject> cachedUnits = new List<UnitObject>();
    private List<Image> slotImages = new List<Image>();
    private float slotSpacing;
    private float viewportHeight;
    private float totalColumnHeight;
    private float bottomThreshold;
    private VerticalLayoutGroup layoutGroup;
    private bool slotsInitialized = false;

    // Map each Image slot to its current unit archetype 
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
        if (isRolling) return;

        // Re-cache deck based on current turn each spin
        CacheUnitsForCurrentTurn();

        if (cachedUnits.Count == 0)
        {
            Debug.LogWarning("[SkillSlotGrid] Cannot spin – no cached units");
            return;
        }

        // If slots were never initialised (first run), set positions
        if (!slotsInitialized)
        {
            PositionSlotsInitial();
        }

        StartRoll();
    }

    private void Start()
    {
        Global.DEBUG_PRINT("[SkillSlotGrid] Starting initialization");
        
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
        Global.DEBUG_PRINT($"[SkillSlotGrid] Found {slotImages.Count} slot images");

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

    private void Update() { }

    private bool CacheUnitsForCurrentTurn()
    {
        cachedUnits.Clear();

        // Always cache from player deck – enemy no longer spins
        eDeckType deckType = eDeckType.PLAYER;

        Global.DEBUG_PRINT($"[SkillSlotGrid] Caching units for {deckType} turn");

        Deck deck = DeckManager.instance.GetDeckByType(deckType);
        if (deck == null)
        {
            Debug.LogError("[SkillSlotGrid] Could not get deck for " + deckType);
            return false;
        }

        for (int i = 0; i < deck.GetDeckMaxSize(); i++)
        {
            UnitObject unit = deck.GetUnitObject(i);
            if (unit != null && !unit.IsDead())
            {
                cachedUnits.Add(unit);
                // Global.DEBUG_PRINT($"[SkillSlotGrid] Added unit to cache: {unit.unitSO.unitName}");
            }
        }

        // Global.DEBUG_PRINT($"[SkillSlotGrid] Cached {cachedUnits.Count} units for {deckType}");
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

        // Pick a symbol according to probability
        SymbolType symbol = SymbolGenerator.instance.GenerateRandomSymbol();
        eUnitArchetype archetype = SymbolGenerator.GetArchetypeForSymbol(symbol);

        // Store archetype for later extraction
        if (slotArchetypes.ContainsKey(img))
        {
            slotArchetypes[img] = archetype;
        }
        else
        {
            slotArchetypes.Add(img, archetype);
        }

        // Clean up old sprite and texture if they exist
        if (img.sprite != null)
        {
            if (img.sprite.texture != null)
            {
                Destroy(img.sprite.texture);
            }
            Destroy(img.sprite);
        }

        if (symbol == SymbolType.EMPTY)
        {
            img.sprite = null;
            img.color = Color.clear;
            return;
        }

        // Choose a cached unit matching this archetype if possible
        List<UnitObject> candidates = cachedUnits.FindAll(u => u.unitSO.eUnitArchetype == archetype);
        UnitObject randomUnit = candidates.Count > 0 ? candidates[Random.Range(0,candidates.Count)] : cachedUnits[Random.Range(0,cachedUnits.Count)];

        var (rt, cam) = RenderUnitToTexture(randomUnit);
        if (rt != null)
        {
            // Create new sprite from the render texture
            Texture2D tex = rt.ToTexture2D();
            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, rt.width, rt.height), new Vector2(0.5f, 0.5f));
            img.sprite = sprite;
            img.color = Color.white;

            // Clean up render texture
            rt.Release();
            Destroy(rt);
        }
        else
        {
            img.sprite = null;
            img.color = Color.clear;
        }

        if (cam != null)
        {
            Destroy(cam.gameObject);
        }
    }

    public List<eUnitArchetype> GetVisibleArchetypes()
    {
        // Grab the three slots whose centres are closest to viewport centre (y≈0)
        List<(Image img, float absY, float y)> candidates = new List<(Image, float, float)>();
        foreach (Image img in slotImages)
        {
            float y = img.rectTransform.anchoredPosition.y;
            candidates.Add((img, Mathf.Abs(y), y));
        }

        // Sort by absolute distance then derive order by actual y for top→bottom
        candidates.Sort((a, b) => a.absY.CompareTo(b.absY));
        var nearestThree = candidates.GetRange(0, Mathf.Min(3, candidates.Count));

        // Now sort those three by real y descending (top first)
        nearestThree.Sort((a, b) => b.y.CompareTo(a.y));

        List<eUnitArchetype> result = new List<eUnitArchetype>();
        foreach (var tuple in nearestThree)
        {
            Image img = tuple.img;
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
        if (isRolling) 
        {
            return;
        }
        
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
        // Get SPUM_Prefabs component
        SPUM_Prefabs spumPrefab = unit.GetComponent<SPUM_Prefabs>();
        if (spumPrefab == null)
        {
            Debug.LogError("[SkillSlotGrid] Unit does not have SPUM_Prefabs component!");
            return (null, null);
        }

        // Locate HeadSet transform 
        Transform headSet = null;
        foreach (Transform t in unit.GetComponentsInChildren<Transform>(true))
        {
            if (t.name.Equals("HeadSet", System.StringComparison.OrdinalIgnoreCase))
            {
                headSet = t;
                break;
            }
        }

        if (headSet == null)
        {
            Debug.LogWarning("[SkillSlotGrid] HeadSet not found on unit " + unit.name );
        }

        // Store original layers for restoration
        Dictionary<GameObject,int> originalLayers = new Dictionary<GameObject,int>();

        if (headSet != null)
        {
            foreach (Transform child in headSet.GetComponentsInChildren<Transform>(true))
            {
                originalLayers[child.gameObject] = child.gameObject.layer;
                child.gameObject.layer = 31; // Preview layer
            }
        }
        else
        {
            // fallback: whole unit
            SetLayerRecursively(unit.gameObject, 31);
            originalLayers[unit.gameObject] = unit.gameObject.layer;
        }

        // Create camera and render texture
        GameObject camObj = new GameObject("UnitPreviewCamera");
        Camera cam = camObj.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.Color;
        cam.backgroundColor = Color.clear;
        cam.orthographic = true;

        RenderTexture rt = new RenderTexture(128, 128, 16);
        cam.targetTexture = rt;
        cam.orthographicSize = 1.0f;

        Vector3 focusPos = (headSet != null ? headSet.position : unit.transform.position);
        cam.transform.position = focusPos + new Vector3(0, renderYOffset, -10);

        // Only render the preview layer
        cam.cullingMask = 1 << 31;

        // Render
        cam.Render();

        // Restore original layers
        foreach (var pair in originalLayers)
        {
            if (pair.Key != null)
            {
                pair.Key.layer = pair.Value;
            }
        }

        cam.enabled = false;
        camObj.SetActive(false);

        return (rt, camObj);
    }
} 