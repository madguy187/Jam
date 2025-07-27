using UnityEngine;
using UnityEngine.UI;

public class ImageTest : MonoBehaviour
{
    public Image image;       // Assign this in Inspector
    public GameObject unitPrefab;   // Assign your unit prefab here

    private RenderTexture unitTexture;
    private GameObject previewCamera;

    void Start()
    {
        // Instantiate unit temporarily
        GameObject unitInstance = Instantiate(unitPrefab);
        unitInstance.SetActive(true);

        (var rt, var camObj) = RenderUtilities.RenderUnitToTexture(unitInstance);
        Sprite unitSprite = RenderUtilities.ConvertRenderTextureToSprite(rt);

        // Now assign it to an Image or SpriteRenderer
        image.sprite = unitSprite;

        // Cleanup: destroy preview camera and unit instance after use
        // Destroy(previewCamera);
        // Destroy(unitInstance);
    }

    private void OnDestroy()
    {
        if (unitTexture != null)
        {
            unitTexture.Release();
            Destroy(unitTexture);
        }
    }
}
