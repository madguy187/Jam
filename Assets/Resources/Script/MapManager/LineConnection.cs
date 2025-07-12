using UnityEngine;

namespace Map 
{
    /// Represents a visual connection between two map nodes using a LineRenderer.
    /// Stores references to the LineRenderer component and the source and destination MapNodes.
    /// Provides a method to set the color of the line by updating its color gradient
    [System.Serializable]
    public class LineConnection 
    {
        /// The LineRenderer component used to draw the connection
        public LineRenderer lineRenderer;
        public MapNode from;
        public MapNode to;

        /// Initializes a new instance of the LineConnection class with the specified LineRenderer and nodes
        public LineConnection(LineRenderer lr, MapNode from, MapNode to) 
        {
            this.lineRenderer = lr;
            this.from = from;
            this.to = to;
        }

        /// Sets the color of the line by updating all color keys in the LineRenderer's gradien
        public void SetColor(Color color) 
        {
            if (lineRenderer != null) {
                Gradient gradient = lineRenderer.colorGradient;
                GradientColorKey[] colorKeys = gradient.colorKeys;
                for (int j = 0; j < colorKeys.Length; j++) {
                    colorKeys[j].color = color;
                }

                gradient.colorKeys = colorKeys;
                lineRenderer.colorGradient = gradient;
            }
        }
    }
}