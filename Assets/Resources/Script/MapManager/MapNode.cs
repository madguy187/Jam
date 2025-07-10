using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Map {
    public enum NodeStates {
        Locked,
        Visited,
        Attainable
    }
}

namespace Map {
    public class MapNode : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler {
        public SpriteRenderer sr;
        public Image image;
        public SpriteRenderer visitedCircle;
        public Image circleImage;
        public Image visitedCircleImage;

        public Node Node { get; private set; }
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

        public void SetUp(Node node, NodeBlueprint blueprint) {
            Node = node;
            Blueprint = blueprint;
            if (sr != null) sr.sprite = blueprint.sprite;
            if (image != null) image.sprite = blueprint.sprite;
            if (node.nodeType == NodeType.MajorBoss) transform.localScale *= 1.5f;
            if (sr != null) initialScale = sr.transform.localScale.x;
            if (image != null) initialScale = image.transform.localScale.x;

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

        public void SetState(NodeStates state) {
            if (visitedCircle != null) visitedCircle.gameObject.SetActive(false);
            if (circleImage != null) circleImage.gameObject.SetActive(false);

            // Stop any running color/pulse coroutines
            if (colorCoroutineSr != null) StopCoroutine(colorCoroutineSr);
            if (colorCoroutineImg != null) StopCoroutine(colorCoroutineImg);
            if (pulseCoroutineSr != null) StopCoroutine(pulseCoroutineSr);
            if (pulseCoroutineImg != null) StopCoroutine(pulseCoroutineImg);

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
                    if (visitedCircle != null) visitedCircle.gameObject.SetActive(true);
                    if (circleImage != null) circleImage.gameObject.SetActive(true);
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

        public void OnPointerEnter(PointerEventData data) {
            if (sr != null) {
                if (scaleCoroutineSr != null) StopCoroutine(scaleCoroutineSr);
                scaleCoroutineSr = StartCoroutine(ScaleTo(sr.transform, initialScale * HoverScaleFactor, 0.3f));
            }
            if (image != null) {
                if (scaleCoroutineImg != null) StopCoroutine(scaleCoroutineImg);
                scaleCoroutineImg = StartCoroutine(ScaleTo(image.transform, initialScale * HoverScaleFactor, 0.3f));
            }
        }

        public void OnPointerExit(PointerEventData data) {
            if (sr != null) {
                if (scaleCoroutineSr != null) StopCoroutine(scaleCoroutineSr);
                scaleCoroutineSr = StartCoroutine(ScaleTo(sr.transform, initialScale, 0.3f));
            }
            if (image != null) {
                if (scaleCoroutineImg != null) StopCoroutine(scaleCoroutineImg);
                scaleCoroutineImg = StartCoroutine(ScaleTo(image.transform, initialScale, 0.3f));
            }
        }

        public void OnPointerDown(PointerEventData data) {
            mouseDownTime = Time.time;
        }

        public void OnPointerUp(PointerEventData data) {
            if (Time.time - mouseDownTime < MaxClickDuration) {
                MapPlayerTracker.Instance.SelectNode(this);
            }
        }

        public void ShowSwirlAnimation() {
            if (visitedCircleImage == null)
                return;

            const float fillDuration = 0.3f;
            if (fillCoroutine != null) StopCoroutine(fillCoroutine);
            fillCoroutine = StartCoroutine(FillTo(visitedCircleImage, 1f, fillDuration));
        }

        private void OnDestroy() {
            if (scaleCoroutineSr != null) StopCoroutine(scaleCoroutineSr);
            if (scaleCoroutineImg != null) StopCoroutine(scaleCoroutineImg);
            if (colorCoroutineSr != null) StopCoroutine(colorCoroutineSr);
            if (colorCoroutineImg != null) StopCoroutine(colorCoroutineImg);
            if (pulseCoroutineSr != null) StopCoroutine(pulseCoroutineSr);
            if (pulseCoroutineImg != null) StopCoroutine(pulseCoroutineImg);
            if (fillCoroutine != null) StopCoroutine(fillCoroutine);
        }

        // --- Animation Coroutines ---

        private IEnumerator ScaleTo(Transform target, float targetScale, float duration) {
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

        private IEnumerator ColorTo(SpriteRenderer renderer, Color targetColor, float duration) {
            Color startColor = renderer.color;
            float time = 0f;
            while (time < duration) {
                renderer.color = Color.Lerp(startColor, targetColor, time / duration);
                time += Time.unscaledDeltaTime;
                yield return null;
            }
            renderer.color = targetColor;
        }

        private IEnumerator ColorTo(Image img, Color targetColor, float duration) {
            Color startColor = img.color;
            float time = 0f;
            while (time < duration) {
                img.color = Color.Lerp(startColor, targetColor, time / duration);
                time += Time.unscaledDeltaTime;
                yield return null;
            }
            img.color = targetColor;
        }

        private IEnumerator PulseColor(SpriteRenderer renderer, Color colorA, Color colorB, float duration) {
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

        private IEnumerator PulseColor(Image img, Color colorA, Color colorB, float duration) {
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

        private IEnumerator FillTo(Image img, float targetFill, float duration) {
            float startFill = img.fillAmount;
            float time = 0f;
            while (time < duration) {
                img.fillAmount = Mathf.Lerp(startFill, targetFill, time / duration);
                time += Time.unscaledDeltaTime;
                yield return null;
            }
            img.fillAmount = targetFill;
        }
    }
}