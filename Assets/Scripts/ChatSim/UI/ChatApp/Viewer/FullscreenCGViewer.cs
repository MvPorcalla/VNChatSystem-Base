// ════════════════════════════════════════════════════════════════════════
// Assets/Scripts/ChatSim/UI/ChatApp/Viewers/FullscreenCGViewer.cs
// Fullscreen CG Viewer - Pinch zoom, pan, swipe to close
// ════════════════════════════════════════════════════════════════════════

using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ChatSim.UI.ChatApp.Components
{
    /// <summary>
    /// Displays CG images in fullscreen mode.
    /// Features: Pinch-to-zoom, pan, tap-to-close
    /// Attach to: FullscreenCGViewer GameObject (child of Canvas)
    /// </summary>
    public class FullscreenCGViewer : MonoBehaviour
    {
        // ═══════════════════════════════════════════════════════════
        // ░ INSPECTOR REFERENCES
        // ═══════════════════════════════════════════════════════════
        
        [Header("UI Elements")]
        [SerializeField] private GameObject viewerPanel;
        [SerializeField] private Image cgImage;
        [SerializeField] private Button closeButton;
        [SerializeField] private TextMeshProUGUI cgNameText;
        
        [Header("Zoom Settings")]
        [SerializeField] private float minZoom = 1f;
        [SerializeField] private float maxZoom = 3f;
        [SerializeField] private float zoomSpeed = 0.1f;
        
        [Header("Animation")]
        [SerializeField] private float fadeDuration = 0.3f;
        [SerializeField] private CanvasGroup canvasGroup;
        
        // ═══════════════════════════════════════════════════════════
        // ░ STATE
        // ═══════════════════════════════════════════════════════════
        
        private float currentZoom = 1f;
        private Vector2 lastTouchPosition;
        private bool isDragging = false;
        private RectTransform imageRect;
        private Coroutine fadeCoroutine;
        
        // ═══════════════════════════════════════════════════════════
        // ░ INITIALIZATION
        // ═══════════════════════════════════════════════════════════
        
        private void Awake()
        {
            if (cgImage != null)
            {
                imageRect = cgImage.rectTransform;
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
        public void ShowFullscreen(Sprite sprite, string cgName = "")
        {
            if (sprite == null)
            {
                Debug.LogError("[FullscreenCGViewer] Cannot show null sprite!");
                return;
            }
            
            Debug.Log($"[FullscreenCGViewer] Showing: {cgName}");
            
            // Set sprite
            if (cgImage != null)
            {
                cgImage.sprite = sprite;
            }
            
            // Set name (optional)
            if (cgNameText != null && !string.IsNullOrEmpty(cgName))
            {
                cgNameText.text = cgName;
                cgNameText.gameObject.SetActive(true);
            }
            else if (cgNameText != null)
            {
                cgNameText.gameObject.SetActive(false);
            }
            
            // Reset zoom/pan
            ResetTransform();
            
            // Show with fade-in
            if (viewerPanel != null)
            {
                viewerPanel.SetActive(true);
            }
            
            // Fade in
            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
            }
            fadeCoroutine = StartCoroutine(FadeCanvasGroup(0f, 1f, fadeDuration));
        }
        
        /// <summary>
        /// Hide the fullscreen viewer
        /// </summary>
        public void Hide()
        {
            Debug.Log("[FullscreenCGViewer] Hiding");
            
            // Fade out
            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
            }
            fadeCoroutine = StartCoroutine(FadeCanvasGroupAndHide());
        }
        
        // ═══════════════════════════════════════════════════════════
        // ░ FADE ANIMATIONS (Coroutine-based)
        // ═══════════════════════════════════════════════════════════
        
        private IEnumerator FadeCanvasGroup(float fromAlpha, float toAlpha, float duration)
        {
            if (canvasGroup == null)
                yield break;
            
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                canvasGroup.alpha = Mathf.Lerp(fromAlpha, toAlpha, t);
                yield return null;
            }
            
            canvasGroup.alpha = toAlpha;
        }
        
        private IEnumerator FadeCanvasGroupAndHide()
        {
            yield return FadeCanvasGroup(1f, 0f, fadeDuration);
            
            if (viewerPanel != null)
            {
                viewerPanel.SetActive(false);
            }
        }
        
        // ═══════════════════════════════════════════════════════════
        // ░ ZOOM & PAN (MOBILE TOUCH)
        // ═══════════════════════════════════════════════════════════
        
        private void Update()
        {
            if (!viewerPanel.activeSelf || imageRect == null)
                return;
            
            // Handle pinch-to-zoom (mobile)
            if (Input.touchCount == 2)
            {
                HandlePinchZoom();
            }
            // Handle single touch drag (pan)
            else if (Input.touchCount == 1)
            {
                HandleDrag();
            }
            // Handle mouse wheel zoom (editor/PC)
            else if (Input.mouseScrollDelta.y != 0)
            {
                HandleMouseWheelZoom();
            }
        }
        
        private void HandlePinchZoom()
        {
            Touch touch0 = Input.GetTouch(0);
            Touch touch1 = Input.GetTouch(1);
            
            // Get previous touch positions
            Vector2 touch0PrevPos = touch0.position - touch0.deltaPosition;
            Vector2 touch1PrevPos = touch1.position - touch1.deltaPosition;
            
            // Calculate previous and current distances
            float prevMagnitude = (touch0PrevPos - touch1PrevPos).magnitude;
            float currentMagnitude = (touch0.position - touch1.position).magnitude;
            
            // Calculate zoom delta
            float difference = currentMagnitude - prevMagnitude;
            float zoomDelta = difference * zoomSpeed * Time.deltaTime;
            
            // Apply zoom
            SetZoom(currentZoom + zoomDelta);
        }
        
        private void HandleDrag()
        {
            Touch touch = Input.GetTouch(0);
            
            if (touch.phase == TouchPhase.Began)
            {
                isDragging = true;
                lastTouchPosition = touch.position;
            }
            else if (touch.phase == TouchPhase.Moved && isDragging)
            {
                Vector2 delta = touch.position - lastTouchPosition;
                imageRect.anchoredPosition += delta;
                lastTouchPosition = touch.position;
            }
            else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                isDragging = false;
            }
        }
        
        private void HandleMouseWheelZoom()
        {
            float scrollDelta = Input.mouseScrollDelta.y;
            float zoomDelta = scrollDelta * 0.1f;
            SetZoom(currentZoom + zoomDelta);
        }
        
        // ═══════════════════════════════════════════════════════════
        // ░ TRANSFORM CONTROL
        // ═══════════════════════════════════════════════════════════
        
        private void SetZoom(float newZoom)
        {
            currentZoom = Mathf.Clamp(newZoom, minZoom, maxZoom);
            imageRect.localScale = Vector3.one * currentZoom;
        }
        
        private void ResetTransform()
        {
            currentZoom = 1f;
            
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
        }
    }
}