using UnityEngine;

namespace Map 
{
    /// Adjusts the material tiling of a LineRenderer to create a dotted or dashed line effect that scales with the line's length
    public class DottedLineRenderer : MonoBehaviour 
    {
        /// If true, the material scaling will be updated every frame in Update().
        /// If false, scaling is only performed once in Start().
        public bool scaleInUpdate = false;
        /// Initializes the component, applies initial material scaling, and enables Update() if needed
        private LineRenderer lineRenderer;
        private Renderer myRenderer;

        /// Initializes the component, applies initial material scaling, and enables Update() if needed
        private void Start() 
        {
            ScaleMaterial();
            enabled = scaleInUpdate;
        }

        /// Scales the main texture of the LineRenderer's material so that the texture repeats
        /// proportionally to the distance between the first and last points of the line
        public void ScaleMaterial() 
        {
            lineRenderer = GetComponent<LineRenderer>();
            myRenderer = GetComponent<Renderer>();
            myRenderer.material.mainTextureScale =
                new Vector2(Vector2.Distance(lineRenderer.GetPosition(0), 
                            lineRenderer.GetPosition(lineRenderer.positionCount - 1)) / lineRenderer.widthMultiplier,
                            1
                );
        }

        /// If enabled, continuously updates the material scaling to match any changes in the line's length
        private void Update() 
        {
            myRenderer.material.mainTextureScale =
                new Vector2(Vector2.Distance(lineRenderer.GetPosition(0), 
                            lineRenderer.GetPosition(lineRenderer.positionCount - 1)) / lineRenderer.widthMultiplier,
                            1
                );
        }
    }
}