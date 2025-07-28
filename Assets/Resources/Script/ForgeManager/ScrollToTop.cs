using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ScrollToTop : MonoBehaviour
{
    [SerializeField] private ScrollRect scrollRect;

    void Start()
    {
        StartCoroutine(ScrollToTopNextFrame());
    }

    private IEnumerator ScrollToTopNextFrame()
    {
        // Wait for end of frame so Unity can finish layouting
        yield return new WaitForEndOfFrame();

        Canvas.ForceUpdateCanvases(); // Ensure layout updated
        scrollRect.verticalNormalizedPosition = 1f;
    }
}