using System.Collections;
using UnityEngine;

namespace Map 
{
    /// Allows a non-UI GameObject to be dragged with the mouse and 
    /// snapped back within specified constraints
    public class ScrollNonUI : MonoBehaviour 
    {
        /// Duration (in seconds) for the snap-back animation when the object is
        /// released outside constraints
        public float snapBackDuration = 0.3f;
        /// If true, disables movement along the X axis
        public bool freezeX;
        /// Minimum and maximum allowed X positions (used if freezeY is true)
        public FloatMinMax xConstraints = new FloatMinMax();
        /// If true, disables movement along the Y axis
        public bool freezeY;
        /// Minimum and maximum allowed Y positions (used if freezeX is true)
        public FloatMinMax yConstraints = new FloatMinMax();

        private Vector2 offset;
        private Vector3 pointerDisplacement;
        private float zDisplacement;
        private bool dragging;
        private Camera mainCamera;

        // Coroutine for snapping back
        private Coroutine snapBackCoroutine;

        ///  Initializes the main camera reference and calculates the Z displacement 
        ///  for mouse world position
        private void Awake() 
        {
            mainCamera = Camera.main;
            zDisplacement = -mainCamera.transform.position.z + transform.position.z;
        }

        /// Begins dragging and stops any ongoing snap-back animation
        public void OnMouseDown() 
        {
            pointerDisplacement = -transform.position + MouseInWorldCoords();
            StopSnapBack();
            dragging = true;
        }

        /// Ends dragging and checks if the object needs to snap back to constraints
        public void OnMouseUp() 
        {
            dragging = false;
            SnapBackToConstraints();
        }

        private void Update()
        {
            if (!dragging) { return; }

            Vector3 mousePos = MouseInWorldCoords();
            transform.position = new Vector3(
                freezeX ? transform.position.x : mousePos.x - pointerDisplacement.x,
                freezeY ? transform.position.y : mousePos.y - pointerDisplacement.y,
                transform.position.z);
        }

        // Returns mouse position in World coordinates for our GameObject to follow. 
        private Vector3 MouseInWorldCoords() 
        {
            Vector3 screenMousePos = Input.mousePosition;
            screenMousePos.z = zDisplacement;
            return mainCamera.ScreenToWorldPoint(screenMousePos);
        }

        /// Updates the GameObject's position to follow the mouse while dragging
        private void SnapBackToConstraints() 
        {
            if (freezeY) {
                if (transform.localPosition.x >= xConstraints.min && transform.localPosition.x <= xConstraints.max) {
                    return;
                }
                float targetX = transform.localPosition.x < xConstraints.min ? xConstraints.min : xConstraints.max;
                StartSnapBackLocalPositionX(targetX, snapBackDuration);
            } else if (freezeX) {
                if (transform.localPosition.y >= yConstraints.min && transform.localPosition.y <= yConstraints.max) {
                    return;
                }
                float targetY = transform.localPosition.y < yConstraints.min ? yConstraints.min : yConstraints.max;
                StartSnapBackLocalPositionY(targetY, snapBackDuration);
            }
        }

        /// Returns the mouse position in world coordinates at the object's Z depth
        private void StartSnapBackLocalPositionX(float targetX, float duration) 
        {
            StopSnapBack();
            snapBackCoroutine = StartCoroutine(AnimateLocalPositionX(targetX, duration));
        }

        /// If the object is outside its allowed constraints, animates it back within bounds
        private void StartSnapBackLocalPositionY(float targetY, float duration) 
        {
            StopSnapBack();
            snapBackCoroutine = StartCoroutine(AnimateLocalPositionY(targetY, duration));
        }

        /// Starts the coroutine to animate the object's local X position back to the target value
        private IEnumerator AnimateLocalPositionX(float targetX, float duration) 
        {
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
            snapBackCoroutine = null;
        }

        /// Starts the coroutine to animate the object's local Y position back to the target value
        private IEnumerator AnimateLocalPositionY(float targetY, float duration) 
        {
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
            snapBackCoroutine = null;
        }

        /// Coroutine that animates the object's local Y position to the target value over the given duration
        private void StopSnapBack() 
        {
            if (snapBackCoroutine != null) {
                StopCoroutine(snapBackCoroutine);
                snapBackCoroutine = null;
            }
        }
    }
}