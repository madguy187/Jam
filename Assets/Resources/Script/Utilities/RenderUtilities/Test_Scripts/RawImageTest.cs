using UnityEngine;
using UnityEngine.UI;

public class RawImageTest : MonoBehaviour
{
    public RawImage rawImage;       // Assign this in Inspector
    public GameObject unitPrefab;   // Assign your unit prefab here

    private RenderTexture unitTexture;
    private GameObject previewCamera;

    void Start()
    {
        // Instantiate unit temporarily
        GameObject unitInstance = Instantiate(unitPrefab);
        unitInstance.SetActive(true);

        // Render unit to texture
        (unitTexture, previewCamera) = RenderUtilities.RenderUnitToTexture(unitInstance);

        // Assign texture to RawImage
        rawImage.texture = unitTexture;

        // Cleanup: destroy preview camera and unit instance after use
        // Destroy(previewCamera);
        Destroy(unitInstance);
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
