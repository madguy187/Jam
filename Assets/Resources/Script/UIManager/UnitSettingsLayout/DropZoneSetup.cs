using UnityEngine;
using UnityEngine.UI;

public static class DropZoneSetup
{
    public static void AddDropZone(GameObject target, DropZone.AllowedItemType allowedType, Transform contentParent = null)
    {
        if (target == null) return;

        // Add DropZone component if not already present
        DropZone dropZone = target.GetComponent<DropZone>();
        if (dropZone == null)
            dropZone = target.AddComponent<DropZone>();

        // Set allowed item type
        dropZone.allowedType = allowedType;

        // Set content parent (default to target transform if null)
        dropZone.contentParent = contentParent != null ? contentParent : target.transform;

        // Add an Image component if none exists (needed for pointer events)
        Image img = target.GetComponent<Image>();
        if (img == null)
        {
            img = target.AddComponent<Image>();
            img.color = new Color(1, 1, 1, 0); // Transparent image to catch raycasts
        }
    }
}