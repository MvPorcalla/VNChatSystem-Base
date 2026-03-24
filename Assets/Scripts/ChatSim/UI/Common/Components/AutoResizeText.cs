// ════════════════════════════════════════════════════════════════════════
// Assets/Scripts/ChatSim/UI/Common/Components/AutoResizeText.cs
// ════════════════════════════════════════════════════════════════════════

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using ChatSim.Core;

namespace ChatSim.UI.Common.Components
{
    /// <summary>
    /// Automatically resizes TextMeshProUGUI width based on content.
    /// Designed for message bubbles to ensure proper sizing without manual adjustments.
    /// Attach to: TextMeshProUGUI GameObject with LayoutElement
    /// </summary>
    [RequireComponent(typeof(TextMeshProUGUI))]
    [RequireComponent(typeof(LayoutElement))]
    public class AutoResizeText : MonoBehaviour
    {
        // ═══════════════════════════════════════════════════════════
        // ░ COMPONENTS
        // ═══════════════════════════════════════════════════════════
        
        private TextMeshProUGUI textComponent;
        private LayoutElement layoutElement;
        private RectTransform rectTransform;
        
        // ═══════════════════════════════════════════════════════════
        // ░ STATE
        // ═══════════════════════════════════════════════════════════
        
        private float lastCalculatedWidth = -1f;
        private Coroutine layoutRebuildCoroutine;
        private bool isInitialized = false;

        // ═══════════════════════════════════════════════════════════
        // ░ PROPERTIES
        // ═══════════════════════════════════════════════════════════
        
        public bool IsInitialized => isInitialized;

        // Config values with fallbacks
        private float MaxWidth              => GameBootstrap.Config != null ? GameBootstrap.Config.bubbleMaxWidth              : 650f;
        private float MinWidth              => GameBootstrap.Config != null ? GameBootstrap.Config.bubbleMinWidth              : 40f;
        private float WidthChangeThreshold  => GameBootstrap.Config != null ? GameBootstrap.Config.bubbleWidthChangeThreshold  : 0.1f;

        // ═══════════════════════════════════════════════════════════
        // ░ INITIALIZATION
        // ═══════════════════════════════════════════════════════════

        void Awake()
        {
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            if (isInitialized) return;

            try
            {
                textComponent = GetComponent<TextMeshProUGUI>();
                layoutElement = GetComponent<LayoutElement>();
                rectTransform = transform as RectTransform;

                if (textComponent == null)
                {
                    UnityEngine.Debug.LogError($"[AutoResize] TextMeshProUGUI missing on {gameObject.name}");
                    return;
                }

                if (layoutElement == null)
                {
                    UnityEngine.Debug.LogError($"[AutoResize] LayoutElement missing on {gameObject.name}");
                    return;
                }

                textComponent.enableWordWrapping = true;
                isInitialized = true;
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogError($"[AutoResize] Initialization failed on {gameObject.name}: {e.Message}");
                isInitialized = false;
            }
        }

        void Start()
        {
            if (!isInitialized)
            {
                InitializeComponents();
            }

            if (isInitialized)
            {
                SetupLayoutElement();
                UpdateWidthImmediate();
            }
        }

        void OnDestroy()
        {
            // Clean up any pending coroutines
            if (layoutRebuildCoroutine != null)
            {
                StopCoroutine(layoutRebuildCoroutine);
                layoutRebuildCoroutine = null;
            }
        }

        private void SetupLayoutElement()
        {
            if (layoutElement != null)
            {
                layoutElement.minWidth = MinWidth;
                layoutElement.flexibleWidth = 0;
            }
        }

        // ═══════════════════════════════════════════════════════════
        // ░ PUBLIC API
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Sets text and updates width. Preferred method for external usage.
        /// </summary>
        public void SetText(string newText)
        {
            if (!isInitialized)
            {
        #if UNITY_EDITOR
                UnityEngine.Debug.LogWarning($"[AutoResize] SetText called before init on {gameObject.name}");
        #endif
                InitializeComponents();

                if (!isInitialized)
                {
                    UnityEngine.Debug.LogError($"[AutoResize] Failed to initialize on {gameObject.name}");
                    return;
                }
            }

            if (textComponent != null && textComponent.text != newText)
            {
                textComponent.text = newText;
                UpdateWidth();
            }
        }

        /// <summary>
        /// Force immediate width recalculation and layout rebuild.
        /// Use when pulling from pool or after manual text changes.
        /// </summary>
        public void RefreshWidth()
        {
            if (!isInitialized)
            {
        #if UNITY_EDITOR
                UnityEngine.Debug.LogWarning($"[AutoResize] RefreshWidth called before init on {gameObject.name}");
        #endif
                return;
            }

            if (gameObject.activeInHierarchy)
                UpdateWidth();
            else
                UpdateWidthImmediate();
        }

        [ContextMenu("Force Reinitialize")]
        public void ForceReinitialize()
        {
            isInitialized = false;
            lastCalculatedWidth = -1f;
            
            InitializeComponents();
            
            if (isInitialized)
            {
                SetupLayoutElement();
                UpdateWidthImmediate();
            }
        }

        // ═══════════════════════════════════════════════════════════
        // ░ WIDTH CALCULATION
        // ═══════════════════════════════════════════════════════════

        private float CalculatePreferredWidth()
        {
            if (string.IsNullOrEmpty(textComponent.text))
                return MinWidth;

            Vector2 textSize = textComponent.GetPreferredValues(textComponent.text, MaxWidth, 0);
            return Mathf.Clamp(textSize.x, MinWidth, MaxWidth);
        }

        /// <summary>
        /// Calculates and applies width if changed.
        /// Returns true if width was updated.
        /// </summary>
        private bool CalculateAndApplyWidth()
        {
            if (!isInitialized || textComponent == null || layoutElement == null)
                return false;

            try
            {
                float preferredWidth = CalculatePreferredWidth();

                if (Mathf.Abs(preferredWidth - lastCalculatedWidth) > WidthChangeThreshold)
                {
                    layoutElement.preferredWidth = preferredWidth;
                    lastCalculatedWidth = preferredWidth;
                    return true;
                }
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogError($"[AutoResize] Width calculation failed on {gameObject.name}: {e.Message}");
            }

            return false;
        }

        // ═══════════════════════════════════════════════════════════
        // ░ UPDATE METHODS
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Updates width with deferred layout rebuild (end of frame).
        /// Use for runtime text changes.
        /// </summary>
        private void UpdateWidth()
        {
            if (!CalculateAndApplyWidth())
                return; // No change needed

            if (gameObject.activeInHierarchy)
            {
                // Cancel any pending rebuild
                if (layoutRebuildCoroutine != null)
                {
                    StopCoroutine(layoutRebuildCoroutine);
                }

                layoutRebuildCoroutine = StartCoroutine(RebuildLayoutEndOfFrame());
            }
            else
            {
                // Object inactive - rebuild immediately
                if (rectTransform != null)
                {
                    LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
                }
            }
        }

        /// <summary>
        /// Updates width with immediate layout rebuild.
        /// Use for pooled objects or initialization.
        /// </summary>
        private void UpdateWidthImmediate()
        {
            if (!CalculateAndApplyWidth())
                return; // No change needed

            if (rectTransform != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
            }
        }

        private IEnumerator RebuildLayoutEndOfFrame()
        {
            yield return new WaitForEndOfFrame();

            try
            {
                if (rectTransform != null)
                    LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogError($"[AutoResize] Layout rebuild failed on {gameObject.name}: {e.Message}");
            }
            finally
            {
                layoutRebuildCoroutine = null;
            }
        }

        // ═══════════════════════════════════════════════════════════
        // ░ EDITOR SUPPORT
        // ═══════════════════════════════════════════════════════════

        void OnValidate()
        {
            if (Application.isPlaying && isInitialized)
            {
                RefreshWidth();
            }
        }
    }
}