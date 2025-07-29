using System.Collections.Generic;
// using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

public class DeckDisplayManager : MonoBehaviour
{
    public static DeckDisplayManager instance;
    public GameObject deckBoxPrefab;
    public Transform deckPanel;

    private List<UnitObject> lastUnits = new List<UnitObject>();
    private List<GameObject> deckBoxes = new List<GameObject>();

    void Awake() 
    {
        if (instance != null) {
            Destroy(instance);
        }
        instance = this;
    }

    void Update()
    {
        var currentUnits = new List<UnitObject>();
        foreach (UnitObject unit in DeckManager.instance.GetDeckByType(eDeckType.PLAYER))
        {
            if (unit != null) {
                currentUnits.Add(unit);
            }
        }

        // Only update if the deck has changed
        if (!AreUnitListsEqual(lastUnits, currentUnits)) {
            RefreshDeckDisplay(currentUnits);
            lastUnits = new List<UnitObject>(currentUnits);
        }
    }

    bool AreUnitListsEqual(List<UnitObject> a, List<UnitObject> b)
    {
        if (a.Count != b.Count) { return false; }
        for (int i = 0; i < a.Count; i++) {
            if (a[i] != b[i]) { return false; }
        }
        return true;
    }

    void RefreshDeckDisplay(List<UnitObject> units)
    {
        // Clean up old boxes and textures
        if (deckBoxes != null) {
            foreach (var box in deckBoxes) {
                if (box != null) { Destroy(box); }
            }
            deckBoxes.Clear();
        }

        if (units != null) {
            foreach (UnitObject unit in units) {
                if (unit == null) { continue; }

                GameObject box = Instantiate(deckBoxPrefab, deckPanel);
                if (box == null) { continue; }

                Image rawImg = box.GetComponent<Image>();
                if (rawImg != null) {
                    rawImg.sprite = RenderUnitToSprite(unit);
                }
                deckBoxes.Add(box);
            }
        }
    }

    public static Sprite RenderUnitToSprite(UnitObject unit) 
    {
        RenderTexture tex;
        GameObject camObj;
        var unitRoot = unit.transform.Find("UnitRoot");
        if (unitRoot == null) {
            Global.DEBUG_PRINT("[DeckDisplayManager::GetUnitSprite] UnitRoot not found on unitPrefab!");
            GameObject.DestroyImmediate(unit);
            return null; // Or handle error properly
        }
        (tex, camObj) = RenderUtilities.RenderUnitToTexture(unitRoot.gameObject, 1.0f);
        Sprite sprite = RenderUtilities.ConvertRenderTextureToSprite(tex);
        Destroy(camObj);
        Destroy(tex);
        return sprite;
    }

    // Uncomment if you want to use individual sprite logic (e.g. Head sprite)
    //Sprite GetHeadSprite(UnitObject unit)
    //{
    //    // Regex pattern for "Head" (case-insensitive)
    //    Regex headRegex = new Regex("head", RegexOptions.IgnoreCase);
    //
    //    // Search all children for a name matching "Head" and has SpriteRenderer
    //    foreach (Transform child in unit.GetComponentsInChildren<Transform>(true))
    //    {
    //        if (headRegex.IsMatch(child.gameObject.name))
    //        {
    //            SpriteRenderer sr = child.GetComponent<SpriteRenderer>();
    //            if (sr != null && sr.sprite != null)
    //            {
    //                return sr.sprite;
    //            }
    //        }
    //    }
    //
    //    // If not found, return null
    //    return null;
    //}
    //void RefreshDeckDisplay(List<UnitObject> units)
    //{
    //    // Clear old boxes
    //    foreach (var box in deckBoxes)
    //    {
    //        Destroy(box);
    //    }
    //    deckBoxes.Clear();
    //    // Add boxes for current units
    //    foreach (UnitObject unit in units)
    //    {
    //        GameObject box = Instantiate(deckBoxPrefab, deckPanel);
    //        Sprite headSprite = GetHeadSprite(unit);
    //        box.GetComponent<Image>().sprite = headSprite;
    //        deckBoxes.Add(box);
    //    }
    //}
}