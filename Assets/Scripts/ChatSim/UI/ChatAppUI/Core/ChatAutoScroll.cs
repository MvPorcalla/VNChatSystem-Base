// ════════════════════════════════════════════════════════════════════════
// Assets/Scripts/UI/ChatApp/Core/ChatAutoScroll.cs
// Phone Chat Simulation Game - Auto-Scroll System (FIXED)
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

        /// <summary>
        /// Event fired when user scrolls to bottom
        /// </summary>
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

            if (!chatScrollRect.gameObject.activeInHierarchy || !autoScrollEnabled)
                return;

            // Check current state
            bool currentlyAtBottom = IsAtBottom();
            float currentHeight = contentTransform.rect.height;
            int currentChildCount = contentTransform.childCount;

            // Check for Content changes
            bool heightChanged = !Mathf.Approximately(currentHeight, lastContentHeight);
            bool childCountChanged = currentChildCount != lastChildCount;

            if ((heightChanged || childCountChanged) && wasAtBottom)
            {
                ScrollToBottom();
                currentlyAtBottom = true;
            }

            if (!wasAtBottom && currentlyAtBottom)
            {
                Debug.Log("[ChatAutoScroll] User scrolled to bottom");
                OnScrollReachedBottom?.Invoke();
            }

            lastContentHeight = currentHeight;
            lastChildCount = currentChildCount;
            wasAtBottom = currentlyAtBottom;
        }

        private void OnEnable()
        {
            isInitialized = false;
            wasAtBottom = true;
        }

        private void OnDisable()
        {
            wasAtBottom = true;
            isInitialized = false;
        }

        // ═══════════════════════════════════════════════════════════
        // ░ INITIALIZATION
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Try to initialize references and state. Returns true if successful.
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
            wasAtBottom = true;
            isInitialized = true;
            
            Debug.Log($"[ChatAutoScroll] Initialized. Content: {contentTransform.name}");
            return true;
        }

        // ═══════════════════════════════════════════════════════════
        // ░ PUBLIC API
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Check if user is currently at the bottom of the chat (within threshold)
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

        /// <summary>
        /// Manually refresh references (call when ChatAppPanel becomes active)
        /// </summary>
        public void RefreshReferences()
        {
            isInitialized = false;
            
            if (TryInitialize())
            {
                ForceScrollToBottom();
                Debug.Log("[ChatAutoScroll] References refreshed");
            }
        }
    }
}