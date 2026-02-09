// ════════════════════════════════════════════════════════════════════════
// Assets/Scripts/UI/ChatApp/Core/ChatAutoScroll.cs
// Phone Chat Simulation Game - Auto-Scroll System (PORTED)
// ════════════════════════════════════════════════════════════════════════

using System;
using UnityEngine;
using UnityEngine.UI;

namespace ChatSim.UI.ChatApp
{
    /// <summary>
    /// Event-driven auto-scroll that monitors Content height changes.
    /// Attach to: ChatAppController GameObject
    /// </summary>
    public class ChatAutoScroll : MonoBehaviour
    {
        // ═══════════════════════════════════════════════════════════
        // ░ INSPECTOR SETTINGS
        // ═══════════════════════════════════════════════════════════
        
        [Header("References")]
        [SerializeField] private ScrollRect chatScrollRect;
        
        [Header("Settings")]
        [SerializeField] private bool autoScrollEnabled = true;
        [SerializeField] private float bottomThreshold = 0.01f; // Consider "at bottom" if within 1%
        
        // ═══════════════════════════════════════════════════════════
        // ░ STATE
        // ═══════════════════════════════════════════════════════════
        
        private RectTransform contentTransform;
        private float lastContentHeight;
        private int lastChildCount;
        private bool wasAtBottom;
        private bool isInitialized;

        // ═══════════════════════════════════════════════════════════
        // ░ EVENTS
        // ═══════════════════════════════════════════════════════════

        /// <summary>Event fired when user scrolls to bottom</summary>
        public event Action OnScrollReachedBottom;

        // ═══════════════════════════════════════════════════════════
        // ░ PROPERTIES
        // ═══════════════════════════════════════════════════════════

        public float CurrentScrollPosition => chatScrollRect?.verticalNormalizedPosition ?? -1f;
        public bool IsInitialized => isInitialized;

        // ═══════════════════════════════════════════════════════════
        // ░ UNITY LIFECYCLE
        // ═══════════════════════════════════════════════════════════

        private void LateUpdate()
        {
            if (!isInitialized && !TryInitialize())
                return;

            // Only monitor if panel is active and auto-scroll is enabled
            if (!chatScrollRect.gameObject.activeInHierarchy || !autoScrollEnabled)
                return;

            // Check current state
            bool currentlyAtBottom = IsAtBottom();
            float currentHeight = contentTransform.rect.height;
            int currentChildCount = contentTransform.childCount;

            // Check for Content changes
            bool heightChanged = !Mathf.Approximately(currentHeight, lastContentHeight);
            bool childCountChanged = currentChildCount != lastChildCount;

            // If Content grew AND we were at bottom, scroll to new bottom
            if ((heightChanged || childCountChanged) && wasAtBottom)
            {
                ScrollToBottom();
                currentlyAtBottom = true; // We just scrolled, so we're at bottom now
            }

            // Detect when user scrolls back to bottom (BEFORE updating wasAtBottom)
            if (!wasAtBottom && currentlyAtBottom)
            {
                Debug.Log("[ChatAutoScroll] User scrolled to bottom");
                OnScrollReachedBottom?.Invoke();
            }

            // Update tracking variables at the end
            lastContentHeight = currentHeight;
            lastChildCount = currentChildCount;
            wasAtBottom = currentlyAtBottom;
        }

        private void OnEnable()
        {
            // Re-initialize if references were lost
            if (chatScrollRect == null || contentTransform == null || !isInitialized)
            {
                Debug.LogWarning("[ChatAutoScroll] References lost - re-initializing");
                isInitialized = false;
                
                if (!TryInitialize())
                {
                    Debug.LogError("[ChatAutoScroll] Failed to initialize on enable");
                    return;
                }
            }

            ForceScrollToBottom();
        }

        private void OnDisable()
        {
            // Reset tracking when disabled to avoid stale state
            wasAtBottom = true;
        }

        // ═══════════════════════════════════════════════════════════
        // ░ INITIALIZATION
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Initialize references - called on-demand
        /// </summary>
        private bool TryInitialize()
        {
            if (chatScrollRect == null)
            {
                chatScrollRect = GetComponentInChildren<ScrollRect>(true);
                if (chatScrollRect == null)
                {
                    Debug.LogError("[ChatAutoScroll] No ScrollRect found!");
                    return false;
                }
            }

            contentTransform = chatScrollRect.content;
            if (contentTransform == null)
            {
                Debug.LogError("[ChatAutoScroll] ScrollRect has no Content!");
                return false;
            }

            lastContentHeight = contentTransform.rect.height;
            lastChildCount = contentTransform.childCount;
            wasAtBottom = true; // Start assuming we're at bottom
            isInitialized = true;
            
            Debug.Log($"[ChatAutoScroll] Initialized. Content: {contentTransform.name}");
            return true;
        }

        // ═══════════════════════════════════════════════════════════
        // ░ PUBLIC API
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Checks if scroll view is at or near bottom
        /// </summary>
        public bool IsAtBottom()
        {
            if (!isInitialized || chatScrollRect == null) 
                return false;
            
            return chatScrollRect.verticalNormalizedPosition <= bottomThreshold;
        }

        /// <summary>
        /// Immediately scroll to bottom with layout rebuild
        /// </summary>
        public void ScrollToBottom()
        {
            if (!isInitialized || chatScrollRect == null) 
                return;
            
            // Force layout update first
            Canvas.ForceUpdateCanvases();
            
            // Then scroll
            chatScrollRect.verticalNormalizedPosition = 0f;
            
            // Force rebuild to ensure correct positioning
            if (contentTransform != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(contentTransform);
            }
        }

        /// <summary>
        /// Force scroll to bottom and reset tracking (for loading new chat)
        /// </summary>
        public void ForceScrollToBottom()
        {
            if (!TryInitialize())
            {
                Debug.LogWarning("[ChatAutoScroll] Cannot force scroll - initialization failed");
                return;
            }

            // Reset tracking to force auto-scroll behavior
            wasAtBottom = true;
            lastContentHeight = contentTransform.rect.height;
            lastChildCount = contentTransform.childCount;
            
            ScrollToBottom();
            
            Debug.Log("[ChatAutoScroll] Forced scroll to bottom and reset tracking");
        }

        /// <summary>
        /// Enable/disable auto-scrolling behavior
        /// </summary>
        public void SetAutoScrollEnabled(bool enabled)
        {
            autoScrollEnabled = enabled;
            Debug.Log($"[ChatAutoScroll] Auto-scroll {(enabled ? "enabled" : "disabled")}");
        }
    }
}