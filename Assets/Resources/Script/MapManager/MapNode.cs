using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Map 
{
    /// Represents the possible states of a map node
    public enum NodeStates 
    {
        Locked,
        Visited,
        Attainable
    }
}

namespace Map 
{
    /// Visual and interactive representation of a node in the map.
    /// Handles state changes, user interactions (hover, click), and visual 
    /// feedback such as scaling, color transitions, and animations.
    public class MapNode : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler {
        /// The SpriteRenderer for the node's main visual
        public SpriteRenderer sr;
        /// The UI Image for the node's main visual (for UI-based nodes)
        public Image image;
        /// The SpriteRenderer for the visited state circle
        public SpriteRenderer visitedCircle;
        /// The UI Image for the attainable/visited state circle
        public Image circleImage;
        /// The UI Image for the swirl animation when visited
        public Image visitedCircleImage;

        /// The logical node data associated with this visual node
        public Node Node { get; private set; }
        /// The blueprint containing visual data for this node
        public NodeBlueprint Blueprint { get; private set; }

        private float initialScale;
        private const float HoverScaleFactor = 1.2f;
        private float mouseDownTime;
        private const float MaxClickDuration = 0.5f;

        // Coroutines for animation control
        private Coroutine scaleCoroutineSr;
        private Coroutine scaleCoroutineImg;
        private Coroutine colorCoroutineSr;
        private Coroutine colorCoroutineImg;
        private Coroutine pulseCoroutineSr;
        private Coroutine pulseCoroutineImg;
        private Coroutine fillCoroutine;

        private bool lockInteractions = false;

        /// Initializes the node's visuals and state based on the provided node data and blueprint
        public void SetUp(Node node, NodeBlueprint blueprint)
        {
            Node = node;
            Blueprint = blueprint;
            NodePopUpBox popUpBox = GetComponentInChildren<NodePopUpBox>(true);
            popUpBox.messsage = Blueprint.description;
            if (sr != null) { sr.sprite = blueprint.sprite; }
            if (image != null) { image.sprite = blueprint.sprite; }
            if (node.nodeType == NodeType.MajorBoss) { transform.localScale *= 1.5f; }
            if (sr != null) { initialScale = sr.transform.localScale.x; }
            if (image != null) { initialScale = image.transform.localScale.x; }

            if (visitedCircle != null) {
                visitedCircle.color = MapView.Instance.visitedColor;
                visitedCircle.gameObject.SetActive(false);
            }

            if (circleImage != null) {
                circleImage.color = MapView.Instance.visitedColor;
                circleImage.gameObject.SetActive(false);
            }

            SetState(NodeStates.Locked);
        }

        /// Sets the visual state of the node (Locked, Visited, Attainable) and triggers corresponding animations
        public void SetState(NodeStates state)
        {
            if (visitedCircle != null) { visitedCircle.gameObject.SetActive(false); }
            if (circleImage != null) { circleImage.gameObject.SetActive(false); }
            ;

            // Stop any running color/pulse coroutines
            if (colorCoroutineSr != null) { StopCoroutine(colorCoroutineSr); }
            if (colorCoroutineImg != null) { StopCoroutine(colorCoroutineImg); }
            if (pulseCoroutineSr != null) { StopCoroutine(pulseCoroutineSr); }
            if (pulseCoroutineImg != null) { StopCoroutine(pulseCoroutineImg); }

            switch (state) {
                case NodeStates.Locked:
                    if (sr != null) {
                        colorCoroutineSr = StartCoroutine(ColorTo(sr, MapView.Instance.lockedColor, 0.1f));
                    }
                    if (image != null) {
                        colorCoroutineImg = StartCoroutine(ColorTo(image, MapView.Instance.lockedColor, 0.1f));
                    }
                    break;
                case NodeStates.Visited:
                    if (sr != null) {
                        colorCoroutineSr = StartCoroutine(ColorTo(sr, MapView.Instance.visitedColor, 0.1f));
                    }
                    if (image != null) {
                        colorCoroutineImg = StartCoroutine(ColorTo(image, MapView.Instance.visitedColor, 0.1f));
                    }
                    if (visitedCircle != null) { visitedCircle.gameObject.SetActive(true); }
                    if (circleImage != null) { circleImage.gameObject.SetActive(true); }
                    break;
                case NodeStates.Attainable:
                    // Start pulsating from locked to visited color
                    if (sr != null) {
                        pulseCoroutineSr = StartCoroutine(PulseColor(sr, MapView.Instance.lockedColor, MapView.Instance.visitedColor, 0.5f));
                    }
                    if (image != null) {
                        pulseCoroutineImg = StartCoroutine(PulseColor(image, MapView.Instance.lockedColor, MapView.Instance.visitedColor, 0.5f));
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }

        /// Triggers a scale-up animation
        public void OnPointerEnter(PointerEventData data)
        {
            if (lockInteractions) { return; } // Ignore if interactions are locked
            if (sr != null) {
                if (scaleCoroutineSr != null) { StopCoroutine(scaleCoroutineSr); }
                scaleCoroutineSr = StartCoroutine(ScaleTo(sr.transform, initialScale * HoverScaleFactor, 0.3f));
            }
            if (image != null) {
                if (scaleCoroutineImg != null) { StopCoroutine(scaleCoroutineImg); }
                scaleCoroutineImg = StartCoroutine(ScaleTo(image.transform, initialScale * HoverScaleFactor, 0.3f));
            }
        }

        /// Returning the node to its original scale
        public void OnPointerExit(PointerEventData data)
        {
            if (lockInteractions) { return; } // Ignore if interactions are locked
            if (sr != null) {
                if (scaleCoroutineSr != null) { StopCoroutine(scaleCoroutineSr); }
                scaleCoroutineSr = StartCoroutine(ScaleTo(sr.transform, initialScale, 0.3f));
            }
            if (image != null) {
                if (scaleCoroutineImg != null) { StopCoroutine(scaleCoroutineImg); }
                scaleCoroutineImg = StartCoroutine(ScaleTo(image.transform, initialScale, 0.3f));
            }
        }

        /// Recording the time for click duration detection
        public void OnPointerDown(PointerEventData data)
        {
            if (lockInteractions) { return; } // Ignore if interactions are locked
            mouseDownTime = Time.time;
        }

        /// Triggering node selection if the click was short
        public void OnPointerUp(PointerEventData data)
        {
            if (lockInteractions) { return; } // Ignore if interactions are locked
            if (Time.time - mouseDownTime < MaxClickDuration) {
                MapPlayerTracker.Instance.SelectNode(this);
            }
        }

        /// Plays a swirl fill animation on the visited circle image
        public void ShowSwirlAnimation()
        {
            if (visitedCircleImage == null) return;

            const float fillDuration = 0.3f;
            if (fillCoroutine != null) StopCoroutine(fillCoroutine);

            // Reset fillAmount to 0 before starting the animation
            visitedCircleImage.fillAmount = 0f;
            fillCoroutine = StartCoroutine(FillTo(visitedCircleImage, 1f, fillDuration));
        }

        /// Cleans up any running coroutines when the node is destroyed
        private void OnDestroy()
        {
            if (scaleCoroutineSr != null) { StopCoroutine(scaleCoroutineSr); }
            if (scaleCoroutineImg != null) { StopCoroutine(scaleCoroutineImg); }
            if (colorCoroutineSr != null) { StopCoroutine(colorCoroutineSr); }
            if (colorCoroutineImg != null) { StopCoroutine(colorCoroutineImg); }
            if (pulseCoroutineSr != null) { StopCoroutine(pulseCoroutineSr); }
            if (pulseCoroutineImg != null) { StopCoroutine(pulseCoroutineImg); }
            if (fillCoroutine != null) { StopCoroutine(fillCoroutine); }
        }

        // --- Animation Coroutines ---

        /// Smoothly scales a transform to the target scale over the given duration
        private IEnumerator ScaleTo(Transform target, float targetScale, float duration)
        {
            float startScale = target.localScale.x;
            float time = 0f;
            while (time < duration) {
                float t = time / duration;
                float scale = Mathf.Lerp(startScale, targetScale, t);
                target.localScale = new Vector3(scale, scale, scale);
                time += Time.unscaledDeltaTime;
                yield return null;
            }
            target.localScale = new Vector3(targetScale, targetScale, targetScale);
        }

        /// Smoothly transitions a SpriteRenderer's color to the target color over the given duration
        private IEnumerator ColorTo(SpriteRenderer renderer, Color targetColor, float duration)
        {
            Color startColor = renderer.color;
            float time = 0f;
            while (time < duration) {
                renderer.color = Color.Lerp(startColor, targetColor, time / duration);
                time += Time.unscaledDeltaTime;
                yield return null;
            }
            renderer.color = targetColor;
        }

        /// Smoothly transitions an Image's color to the target color over the given duration
        private IEnumerator ColorTo(Image img, Color targetColor, float duration)
        {
            Color startColor = img.color;
            float time = 0f;
            while (time < duration) {
                img.color = Color.Lerp(startColor, targetColor, time / duration);
                time += Time.unscaledDeltaTime;
                yield return null;
            }
            img.color = targetColor;
        }

        /// Continuously pulses a SpriteRenderer's color between two colors
        private IEnumerator PulseColor(SpriteRenderer renderer, Color colorA, Color colorB, float duration)
        {
            while (true) {
                // Lerp to colorB
                float time = 0f;
                while (time < duration) {
                    renderer.color = Color.Lerp(colorA, colorB, time / duration);
                    time += Time.unscaledDeltaTime;
                    yield return null;
                }
                // Lerp back to colorA
                time = 0f;
                while (time < duration) {
                    renderer.color = Color.Lerp(colorB, colorA, time / duration);
                    time += Time.unscaledDeltaTime;
                    yield return null;
                }
            }
        }

        /// Continuously pulses an Image's color between two colors
        private IEnumerator PulseColor(Image img, Color colorA, Color colorB, float duration)
        {
            while (true) {
                float time = 0f;
                while (time < duration) {
                    img.color = Color.Lerp(colorA, colorB, time / duration);
                    time += Time.unscaledDeltaTime;
                    yield return null;
                }
                time = 0f;
                while (time < duration) {
                    img.color = Color.Lerp(colorB, colorA, time / duration);
                    time += Time.unscaledDeltaTime;
                    yield return null;
                }
            }
        }

        /// Smoothly fills an Image's fillAmount to the target value over the given duration
        private IEnumerator FillTo(Image img, float targetFill, float duration)
        {
            float startFill = img.fillAmount;
            float time = 0f;
            while (time < duration) {
                img.fillAmount = Mathf.Lerp(startFill, targetFill, time / duration);
                time += Time.unscaledDeltaTime;
                yield return null;
            }
            img.fillAmount = targetFill;
        }

        public void LockInteractions(bool lockInteractions)
        {
            this.lockInteractions = lockInteractions;
        }
    }
}