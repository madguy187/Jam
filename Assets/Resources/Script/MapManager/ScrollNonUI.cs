using System.Collections;
using UnityEngine;

namespace Map {
    public class ScrollNonUI : MonoBehaviour {
        // Tween -> In-between for interpolating animations
        public float tweenBackDuration = 0.3f;
        public bool freezeX;
        public FloatMinMax xConstraints = new FloatMinMax();
        public bool freezeY;
        public FloatMinMax yConstraints = new FloatMinMax();
        private Vector2 offset;
        private Vector3 pointerDisplacement;
        private float zDisplacement;
        private bool dragging;
        private Camera mainCamera;

        // Coroutine for tweening
        private Coroutine tweenCoroutine;

        private void Awake() {
            mainCamera = Camera.main;
            zDisplacement = -mainCamera.transform.position.z + transform.position.z;
        }

        public void OnMouseDown() {
            pointerDisplacement = -transform.position + MouseInWorldCoords();
            StopTween();
            dragging = true;
        }

        public void OnMouseUp() {
            dragging = false;
            TweenBack();
        }

        private void Update() {
            if (!dragging) return;

            Vector3 mousePos = MouseInWorldCoords();
            transform.position = new Vector3(
                freezeX ? transform.position.x : mousePos.x - pointerDisplacement.x,
                freezeY ? transform.position.y : mousePos.y - pointerDisplacement.y,
                transform.position.z);
        }

        // returns mouse position in World coordinates for our GameObject to follow. 
        private Vector3 MouseInWorldCoords() {
            Vector3 screenMousePos = Input.mousePosition;
            screenMousePos.z = zDisplacement;
            return mainCamera.ScreenToWorldPoint(screenMousePos);
        }

        private void TweenBack() {
            if (freezeY) {
                if (transform.localPosition.x >= xConstraints.min && transform.localPosition.x <= xConstraints.max)
                    return;

                float targetX = transform.localPosition.x < xConstraints.min ? xConstraints.min : xConstraints.max;
                StartTweenLocalPositionX(targetX, tweenBackDuration);
            } else if (freezeX) {
                if (transform.localPosition.y >= yConstraints.min && transform.localPosition.y <= yConstraints.max)
                    return;

                float targetY = transform.localPosition.y < yConstraints.min ? yConstraints.min : yConstraints.max;
                StartTweenLocalPositionY(targetY, tweenBackDuration);
            }
        }

        private void StartTweenLocalPositionX(float targetX, float duration) {
            StopTween();
            tweenCoroutine = StartCoroutine(TweenLocalPositionX(targetX, duration));
        }

        private void StartTweenLocalPositionY(float targetY, float duration) {
            StopTween();
            tweenCoroutine = StartCoroutine(TweenLocalPositionY(targetY, duration));
        }

        private IEnumerator TweenLocalPositionX(float targetX, float duration) {
            float startX = transform.localPosition.x;
            float elapsed = 0f;
            Vector3 startPos = transform.localPosition;
            Vector3 endPos = new Vector3(targetX, startPos.y, startPos.z);

            while (elapsed < duration) {
                float t = elapsed / duration;
                float newX = Mathf.Lerp(startX, targetX, t);
                transform.localPosition = new Vector3(newX, startPos.y, startPos.z);
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }
            transform.localPosition = endPos;
            tweenCoroutine = null;
        }

        private IEnumerator TweenLocalPositionY(float targetY, float duration) {
            float startY = transform.localPosition.y;
            float elapsed = 0f;
            Vector3 startPos = transform.localPosition;
            Vector3 endPos = new Vector3(startPos.x, targetY, startPos.z);

            while (elapsed < duration) {
                float t = elapsed / duration;
                float newY = Mathf.Lerp(startY, targetY, t);
                transform.localPosition = new Vector3(startPos.x, newY, startPos.z);
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }
            transform.localPosition = endPos;
            tweenCoroutine = null;
        }

        private void StopTween() {
            if (tweenCoroutine != null) {
                StopCoroutine(tweenCoroutine);
                tweenCoroutine = null;
            }
        }
    }
}