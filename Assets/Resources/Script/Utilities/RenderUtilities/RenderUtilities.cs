using UnityEngine;

public static class RenderUtilities
{
    /// Renders the given unit GameObject to a RenderTexture and returns both the texture and the camera GameObject.
    public static (RenderTexture, GameObject) RenderUnitToTexture(GameObject unit, float scaleMultiplier = 1f)
    {
        int previewLayer = 31;
        int originalLayer = unit.layer;
    
        _SetLayerRecursively(unit, previewLayer);
    
        GameObject camObj = new GameObject("UnitPreviewCamera");
        Camera cam = camObj.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.Color;
        cam.backgroundColor = Color.clear;
        cam.orthographic = true;
        cam.transform.rotation = Quaternion.identity;
    
        // Calculate bounds of the unit including all renderers
        Bounds bounds = new Bounds(unit.transform.position, Vector3.zero);
        Renderer[] renderers = unit.GetComponentsInChildren<Renderer>();
        if (renderers.Length > 0)
        {
            bounds = renderers[0].bounds;
            foreach (Renderer r in renderers)
            {
                bounds.Encapsulate(r.bounds);
            }
        }
    
        // Position camera centered on the bounds, offset on Z so camera looks at the unit
        cam.transform.position = new Vector3(bounds.center.x, bounds.center.y, bounds.center.z - 10);
    
        // Set orthographic size to cover largest extent plus some padding
        float maxExtent = Mathf.Max(bounds.extents.x, bounds.extents.y);
        cam.orthographicSize = (maxExtent + 0.5f) * (1f / scaleMultiplier);
    
        // Set culling mask to only preview layer
        cam.cullingMask = (1 << previewLayer);
    
        RenderTexture rt = new RenderTexture(256, 256, 16, RenderTextureFormat.ARGB32);
        rt.Create();
    
        cam.targetTexture = rt;
        cam.Render();
    
        // Restore original layer on unit and children AFTER rendering
        _SetLayerRecursively(unit, originalLayer);
    
        cam.targetTexture = null;
        cam.enabled = false;
        camObj.SetActive(false);
    
        return (rt, camObj);
    }

    /// Converts a RenderTexture to a Sprite.
    public static Sprite ConvertRenderTextureToSprite(RenderTexture rt)
    {
        RenderTexture currentRT = RenderTexture.active;
        RenderTexture.active = rt;

        Texture2D tex = new Texture2D(rt.width, rt.height, TextureFormat.ARGB32, false);
        tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        tex.Apply();

        RenderTexture.active = currentRT;

        return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
    }

    /// Recursively sets the layer of a GameObject and all children.
    public static void _SetLayerRecursively(GameObject obj, int newLayer)
    {
        obj.layer = newLayer;
        foreach (Transform child in obj.transform)
        {
            _SetLayerRecursively(child.gameObject, newLayer);
        }
    }
}
