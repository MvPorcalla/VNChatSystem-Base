// ════════════════════════════════════════════════════════════════════════
// Assets/Scripts/ChatSim/UI/PhoneOS/Gallery/Components/GalleryFullscreenViewer.cs
// Fullscreen CG Viewer for Gallery - Pinch zoom, pan, tap-to-close
// ════════════════════════════════════════════════════════════════════════

using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ChatSim.UI.PhoneOS.Gallery
{
    /// <summary>
    /// Displays CG images in fullscreen mode from the gallery.
    /// Features: Pinch-to-zoom, pan, tap-to-close, swipe navigation (optional)
    /// </summary>
    public class GalleryFullscreenViewer : MonoBehaviour
    {
        // ═══════════════════════════════════════════════════════════
        // ░ INSPECTOR REFERENCES
        // ═══════════════════════════════════════════════════════════
        
        [Header("UI Elements")]
        [SerializeField] private GameObject viewerPanel;
        [SerializeField] private Image cgImage;
        [SerializeField] private Button closeButton;
        [SerializeField] private TextMeshProUGUI cgNameText;
        [SerializeField] private CanvasGroup canvasGroup;
        
        [Header("Background")]
        [SerializeField] private Image backgroundOverlay; // Dark overlay behind image
        
        [Header("Zoom Settings")]
        [SerializeField] private float minZoom = 1f;
        [SerializeField] private float maxZoom = 3f;
        [SerializeField] private float zoomSpeed = 0.1f;
        [SerializeField] private float doubleTapZoom = 2f;
        [SerializeField] private float doubleTapTime = 0.3f;
        
        [Header("Pan Settings")]
        [SerializeField] private bool enablePanLimits = true;
        
        [Header("Animation")]
        [SerializeField] private float fadeDuration = 0.3f;
        
        // ═══════════════════════════════════════════════════════════
        // ░ STATE
        // ═══════════════════════════════════════════════════════════
        
        private float currentZoom = 1f;
        private Vector2 panOffset = Vector2.zero;
        private RectTransform imageRect;
        private RectTransform panelRect;
        
        // Touch/Input tracking
        private Vector2 lastTouchPosition;
        private bool isDragging = false;
        private float lastTapTime = 0f;
        
        // Animation
        private Coroutine fadeCoroutine;
        private Coroutine zoomCoroutine;
        
        // ═══════════════════════════════════════════════════════════
        // ░ INITIALIZATION
        // ═══════════════════════════════════════════════════════════
        
        private void Awake()
        {
            if (cgImage != null)
            {
                imageRect = cgImage.rectTransform;
            }
            
            if (viewerPanel != null)
            {
                panelRect = viewerPanel.GetComponent<RectTransform>();
            }
            
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(Hide);
            }
            
            // Start hidden
            if (viewerPanel != null)
            {
                viewerPanel.SetActive(false);
            }
        }
        
        // ═══════════════════════════════════════════════════════════
        // ░ PUBLIC API
        // ═══════════════════════════════════════════════════════════
        
        /// <summary>
        /// Show a CG in fullscreen mode
        /// </summary>
        public void Show(Sprite sprite, string cgName = "")
        {
            if (sprite == null)
            {
                Debug.LogError("[GalleryFullscreenViewer] Cannot show null sprite!");
                return;
            }
            
            Debug.Log($"[GalleryFullscreenViewer] Showing: {cgName}");
            
            // Set sprite
            if (cgImage != null)
            {
                cgImage.sprite = sprite;
            }
            
            // Set name
            if (cgNameText != null)
            {
                if (!string.IsNullOrEmpty(cgName))
                {
                    cgNameText.text = cgName;
                    cgNameText.gameObject.SetActive(true);
                }
                else
                {
                    cgNameText.gameObject.SetActive(false);
                }
            }
            
            // Reset transform
            ResetTransform();
            
            // Show panel
            if (viewerPanel != null)
            {
                viewerPanel.SetActive(true);
            }
            
            // Fade in
            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
            }
            fadeCoroutine = StartCoroutine(FadeIn());
        }
        
        /// <summary>
        /// Hide the fullscreen viewer
        /// </summary>
        public void Hide()
        {
            Debug.Log("[GalleryFullscreenViewer] Hiding");
            
            // Fade out
            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
            }
            fadeCoroutine = StartCoroutine(FadeOut());
        }
        
        // ═══════════════════════════════════════════════════════════
        // ░ FADE ANIMATIONS
        // ═══════════════════════════════════════════════════════════
        
        private IEnumerator FadeIn()
        {
            if (canvasGroup == null)
            {
                yield break;
            }
            
            float elapsed = 0f;
            canvasGroup.alpha = 0f;
            
            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / fadeDuration);
                canvasGroup.alpha = t;
                yield return null;
            }
            
            canvasGroup.alpha = 1f;
        }
        
        private IEnumerator FadeOut()
        {
            if (canvasGroup == null)
            {
                if (viewerPanel != null)
                    viewerPanel.SetActive(false);
                yield break;
            }
            
            float elapsed = 0f;
            canvasGroup.alpha = 1f;
            
            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / fadeDuration);
                canvasGroup.alpha = 1f - t;
                yield return null;
            }
            
            canvasGroup.alpha = 0f;
            
            if (viewerPanel != null)
            {
                viewerPanel.SetActive(false);
            }
        }
        
        // ═══════════════════════════════════════════════════════════
        // ░ INPUT HANDLING
        // ═══════════════════════════════════════════════════════════
        
        private void Update()
        {
            if (!viewerPanel.activeSelf || imageRect == null)
                return;
            
            // Mobile touch input
            if (Input.touchCount == 2)
            {
                HandlePinchZoom();
            }
            else if (Input.touchCount == 1)
            {
                HandleTouch();
            }
            // Desktop input (editor/PC)
            else if (Input.mouseScrollDelta.y != 0)
            {
                HandleMouseWheelZoom();
            }
        }
        
        // ─────────────────────────────────────────────────────────
        // Touch Handling
        // ─────────────────────────────────────────────────────────
        
        private void HandleTouch()
        {
            Touch touch = Input.GetTouch(0);
            
            if (touch.phase == TouchPhase.Began)
            {
                // Check for double-tap
                float timeSinceLastTap = Time.time - lastTapTime;
                if (timeSinceLastTap < doubleTapTime)
                {
                    HandleDoubleTap(touch.position);
                    lastTapTime = 0f; // Reset to prevent triple-tap
                }
                else
                {
                    lastTapTime = Time.time;
                }
                
                isDragging = true;
                lastTouchPosition = touch.position;
            }
            else if (touch.phase == TouchPhase.Moved && isDragging)
            {
                Vector2 delta = touch.position - lastTouchPosition;
                Pan(delta);
                lastTouchPosition = touch.position;
            }
            else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                isDragging = false;
            }
        }
        
        private void HandleDoubleTap(Vector2 tapPosition)
        {
            if (currentZoom > 1f)
            {
                // Zoom out to 1x
                AnimateZoomTo(1f, Vector2.zero);
            }
            else
            {
                // Zoom in to double-tap zoom level at tap position
                Vector2 zoomCenter = GetImageLocalPosition(tapPosition);
                AnimateZoomTo(doubleTapZoom, zoomCenter);
            }
        }
        
        // ─────────────────────────────────────────────────────────
        // Pinch Zoom
        // ─────────────────────────────────────────────────────────
        
        private void HandlePinchZoom()
        {
            Touch touch0 = Input.GetTouch(0);
            Touch touch1 = Input.GetTouch(1);
            
            Vector2 touch0PrevPos = touch0.position - touch0.deltaPosition;
            Vector2 touch1PrevPos = touch1.position - touch1.deltaPosition;
            
            float prevMagnitude = (touch0PrevPos - touch1PrevPos).magnitude;
            float currentMagnitude = (touch0.position - touch1.position).magnitude;
            
            float difference = currentMagnitude - prevMagnitude;
            float zoomDelta = difference * zoomSpeed * Time.deltaTime;
            
            // Calculate pinch center
            Vector2 pinchCenter = (touch0.position + touch1.position) / 2f;
            Vector2 imageLocalCenter = GetImageLocalPosition(pinchCenter);
            
            SetZoom(currentZoom + zoomDelta, imageLocalCenter);
        }
        
        // ─────────────────────────────────────────────────────────
        // Mouse Wheel Zoom (Desktop)
        // ─────────────────────────────────────────────────────────
        
        private void HandleMouseWheelZoom()
        {
            float scrollDelta = Input.mouseScrollDelta.y;
            float zoomDelta = scrollDelta * 0.1f;
            
            Vector2 mousePos = Input.mousePosition;
            Vector2 imageLocalPos = GetImageLocalPosition(mousePos);
            
            SetZoom(currentZoom + zoomDelta, imageLocalPos);
        }
        
        // ═══════════════════════════════════════════════════════════
        // ░ ZOOM CONTROL
        // ═══════════════════════════════════════════════════════════
        
        private void SetZoom(float newZoom, Vector2 zoomCenter)
        {
            float oldZoom = currentZoom;
            currentZoom = Mathf.Clamp(newZoom, minZoom, maxZoom);
            
            // Adjust pan offset to keep zoom centered
            float zoomRatio = currentZoom / oldZoom;
            panOffset = (panOffset - zoomCenter) * zoomRatio + zoomCenter;
            
            ApplyTransform();
        }
        
        private void AnimateZoomTo(float targetZoom, Vector2 targetCenter)
        {
            if (zoomCoroutine != null)
            {
                StopCoroutine(zoomCoroutine);
            }
            zoomCoroutine = StartCoroutine(AnimateZoomCoroutine(targetZoom, targetCenter));
        }
        
        private IEnumerator AnimateZoomCoroutine(float targetZoom, Vector2 targetCenter)
        {
            float startZoom = currentZoom;
            Vector2 startOffset = panOffset;
            float elapsed = 0f;
            float duration = 0.3f;
            
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
                
                currentZoom = Mathf.Lerp(startZoom, targetZoom, t);
                panOffset = Vector2.Lerp(startOffset, targetCenter, t);
                
                ApplyTransform();
                yield return null;
            }
            
            currentZoom = targetZoom;
            panOffset = targetCenter;
            ApplyTransform();
        }
        
        // ═══════════════════════════════════════════════════════════
        // ░ PAN CONTROL
        // ═══════════════════════════════════════════════════════════
        
        private void Pan(Vector2 delta)
        {
            panOffset += delta;
            ApplyTransform();
        }
        
        // ═══════════════════════════════════════════════════════════
        // ░ TRANSFORM APPLICATION
        // ═══════════════════════════════════════════════════════════
        
        private void ApplyTransform()
        {
            if (imageRect == null) return;
            
            // Apply zoom
            imageRect.localScale = Vector3.one * currentZoom;
            
            // Apply pan with limits
            Vector2 clampedOffset = panOffset;
            
            if (enablePanLimits && panelRect != null)
            {
                clampedOffset = ClampPanOffset(panOffset);
            }
            
            imageRect.anchoredPosition = clampedOffset;
        }
        
        private Vector2 ClampPanOffset(Vector2 offset)
        {
            if (imageRect == null || panelRect == null)
                return offset;
            
            // Calculate image bounds in panel space
            Vector2 imageSize = imageRect.rect.size * currentZoom;
            Vector2 panelSize = panelRect.rect.size;
            
            // Calculate max pan limits
            Vector2 maxPan = (imageSize - panelSize) / 2f;
            maxPan = Vector2.Max(maxPan, Vector2.zero);
            
            return new Vector2(
                Mathf.Clamp(offset.x, -maxPan.x, maxPan.x),
                Mathf.Clamp(offset.y, -maxPan.y, maxPan.y)
            );
        }
        
        // ═══════════════════════════════════════════════════════════
        // ░ HELPER METHODS
        // ═══════════════════════════════════════════════════════════
        
        private Vector2 GetImageLocalPosition(Vector2 screenPosition)
        {
            if (imageRect == null) return Vector2.zero;
            
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                imageRect,
                screenPosition,
                null,
                out Vector2 localPoint
            );
            
            return localPoint;
        }
        
        private void ResetTransform()
        {
            currentZoom = 1f;
            panOffset = Vector2.zero;
            
            if (imageRect != null)
            {
                imageRect.localScale = Vector3.one;
                imageRect.anchoredPosition = Vector2.zero;
            }
        }
        
        // ═══════════════════════════════════════════════════════════
        // ░ CLEANUP
        // ═══════════════════════════════════════════════════════════
        
        private void OnDestroy()
        {
            if (closeButton != null)
            {
                closeButton.onClick.RemoveListener(Hide);
            }
            
            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
            }
            
            if (zoomCoroutine != null)
            {
                StopCoroutine(zoomCoroutine);
            }
        }
        
        // ═══════════════════════════════════════════════════════════
        // ░ EDITOR TOOLS
        // ═══════════════════════════════════════════════════════════
        
        #if UNITY_EDITOR
        [ContextMenu("Debug/Test Show")]
        private void DebugTestShow()
        {
            if (cgImage != null && cgImage.sprite != null)
            {
                Show(cgImage.sprite, "Test CG");
            }
            else
            {
                Debug.LogWarning("No sprite assigned to test with!");
            }
        }
        
        [ContextMenu("Debug/Test Hide")]
        private void DebugTestHide()
        {
            Hide();
        }
        #endif
    }
}