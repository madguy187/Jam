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
    private List<RenderTexture> deckTextures = new List<RenderTexture>();
    private List<GameObject> deckCameras = new List<GameObject>(); // Store camera objects

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

        if (deckTextures != null) {
            foreach (var tex in deckTextures) {
                if (tex != null) {
                    tex.Release();
                    Destroy(tex);
                }
            }
            deckTextures.Clear();
        }

        if (deckCameras != null) {
            foreach (var camObj in deckCameras) {
                if (camObj != null)
                    Destroy(camObj);
            }
            deckCameras.Clear();
        }

        if (units != null) {
            foreach (UnitObject unit in units) {
                if (unit == null) { continue; }

                GameObject box = Instantiate(deckBoxPrefab, deckPanel);
                if (box == null) { continue; }

                RawImage rawImg = box.GetComponent<RawImage>();
                if (rawImg != null) {
                    RenderTexture tex;
                    GameObject camObj;
                    (tex, camObj) = RenderUnitToTexture(unit);
                    if (tex != null) {
                        rawImg.texture = tex;
                        deckTextures.Add(tex);
                    }
                    if (camObj != null) {
                        deckCameras.Add(camObj);
                    }
                }
                deckBoxes.Add(box);
            }
        }
    }

    public static Sprite RenderUnitToSprite(UnitObject unit) 
    {
        var (rt, cam) = RenderUnitToTexture(unit);
        Texture2D tex = rt.ToTexture2D();
        Sprite sprite = Sprite.Create(tex, new Rect(0, 0, rt.width, rt.height), new Vector2(0.5f, 0.5f));

        // Clean up render texture
        rt.Release();
        Destroy(rt);
        Destroy(cam);

        return sprite;
    }

    // Return both RenderTexture and camera GameObject
    public static (RenderTexture, GameObject) RenderUnitToTexture(UnitObject unit) 
    {
        // Create a preview layer (e.g., Layer 31 in Unity)
        int previewLayer = 31; // Make sure this layer exists in your project

        var unitRoot = unit.transform.Find("UnitRoot");
        var unitRootGameObject = unitRoot != null ? unitRoot.gameObject : unit.gameObject;

        // Store original layer
        int originalLayer = unitRootGameObject.layer;

        // Set unit and all children to preview layer
        SetLayerRecursively(unitRootGameObject, previewLayer);

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
        SetLayerRecursively(unitRootGameObject, originalLayer);

        cam.enabled = false;
        camObj.SetActive(false);

        return (rt, camObj);
    }

    // Helper to set layer recursively
    static void SetLayerRecursively(GameObject obj, int newLayer)
    {
        obj.layer = newLayer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, newLayer);
        }
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