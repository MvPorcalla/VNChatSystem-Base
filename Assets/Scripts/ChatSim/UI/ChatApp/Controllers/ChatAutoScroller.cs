// ════════════════════════════════════════════════════════════════════════
// Assets/Scripts/UI/ChatApp/Core/ChatAutoScroller.cs
// ════════════════════════════════════════════════════════════════════════

using System;
using UnityEngine;
using UnityEngine.UI;
using ChatSim.Core;

namespace ChatSim.UI.ChatApp.Controllers
{
    /// <summary>
    /// Event-driven auto-scroll that monitors Content height changes.
    /// Attach to: ChatAppController GameObject
    /// </summary>
    public class ChatAutoScroller : MonoBehaviour
    {
        // ═══════════════════════════════════════════════════════════
        // ░ INSPECTOR SETTINGS
        // ═══════════════════════════════════════════════════════════
        
        [Header("References")]
        [SerializeField] private ScrollRect chatScrollRect;
        
        [Header("Auto-Scroll Settings")]
        [Tooltip("Enable or disable auto-scrolling behavior. When disabled, the scroll position will not automatically adjust when new messages are added.")]
        [SerializeField] private bool autoScrollEnabled = true;
        
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

        public event Action OnScrollReachedBottom;

        // ═══════════════════════════════════════════════════════════
        // ░ PROPERTIES
        // ═══════════════════════════════════════════════════════════

        public float CurrentScrollPosition => chatScrollRect?.verticalNormalizedPosition ?? -1f;
        public bool IsInitialized => isInitialized;
        private float BottomThreshold => GameBootstrap.Config != null ? GameBootstrap.Config.bottomThreshold : 0.01f;

        // ═══════════════════════════════════════════════════════════
        // ░ UNITY LIFECYCLE
        // ═══════════════════════════════════════════════════════════

        private void LateUpdate()
        {
            if (!isInitialized && !TryInitialize())
                return;

            if (!chatScrollRect.gameObject.activeInHierarchy || !autoScrollEnabled)
                return;

            if (contentTransform == null)
            {
                LogWarning("contentTransform is null - reinitializing");
                isInitialized = false;
                return;
            }

            bool currentlyAtBottom = IsAtBottom();
            float currentHeight = contentTransform.rect.height;
            int currentChildCount = contentTransform.childCount;

            bool heightChanged = !Mathf.Approximately(currentHeight, lastContentHeight);
            bool childCountChanged = currentChildCount != lastChildCount;

            if ((heightChanged || childCountChanged) && wasAtBottom)
            {
                ScrollToBottom();
                currentlyAtBottom = true;
            }

            if (!wasAtBottom && currentlyAtBottom)
            {
                Log("User scrolled to bottom");
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

        private bool TryInitialize()
        {
            if (chatScrollRect == null)
            {
                chatScrollRect = GetComponentInChildren<ScrollRect>(true);
                if (chatScrollRect == null)
                {
                    LogError("No ScrollRect found!");
                    return false;
                }
            }

            contentTransform = chatScrollRect.content;
            if (contentTransform == null)
            {
                LogError("ScrollRect has no Content!");
                return false;
            }

            lastContentHeight = contentTransform.rect.height;
            lastChildCount = contentTransform.childCount;
            wasAtBottom = true;
            isInitialized = true;

            Log($"Initialized. Content: {contentTransform.name}");
            return true;
        }

        // ═══════════════════════════════════════════════════════════
        // ░ PUBLIC API
        // ═══════════════════════════════════════════════════════════

        public bool IsAtBottom()
        {
            if (!isInitialized || chatScrollRect == null)
                return false;

            return chatScrollRect.verticalNormalizedPosition <= BottomThreshold;
        }

        public void ScrollToBottom()
        {
            if (!isInitialized || chatScrollRect == null)
                return;

            if (contentTransform == null)
            {
                LogWarning("Cannot scroll - contentTransform is null");
                return;
            }

            Canvas.ForceUpdateCanvases();
            chatScrollRect.verticalNormalizedPosition = 0f;
            LayoutRebuilder.ForceRebuildLayoutImmediate(contentTransform);
        }

        public void ForceScrollToBottom()
        {
            if (!isInitialized && !TryInitialize())
            {
                LogWarning("Cannot force scroll - initialization failed");
                return;
            }

            wasAtBottom = true;
            lastContentHeight = contentTransform.rect.height;
            lastChildCount = contentTransform.childCount;

            ScrollToBottom();

            Log("Forced scroll to bottom and reset tracking");
        }

        public void SetAutoScrollEnabled(bool enabled)
        {
            autoScrollEnabled = enabled;
            Log($"Auto-scroll {(enabled ? "enabled" : "disabled")}");
        }

        public void RefreshReferences()
        {
            isInitialized = false;

            if (TryInitialize())
            {
                ForceScrollToBottom();
                Log("References refreshed");
            }
        }

        // ═══════════════════════════════════════════════════════════
        // ░ LOGGING
        // ═══════════════════════════════════════════════════════════

        [System.Diagnostics.Conditional("UNITY_EDITOR"), System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]
        private void Log(string message)
        {
            if (GameBootstrap.Config == null || !GameBootstrap.Config.chatAutoScrollerDebugLogs) return;
            UnityEngine.Debug.Log($"[ChatAutoScroller] {message}");
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR"), System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]
        private void LogWarning(string message)
        {
            if (GameBootstrap.Config == null || !GameBootstrap.Config.chatAutoScrollerDebugLogs) return;
            UnityEngine.Debug.LogWarning($"[ChatAutoScroller] WARNING: {message}");
        }

        private void LogError(string message)
        {
            // Always show errors — no Conditional, fires in all builds
            UnityEngine.Debug.LogError($"[ChatAutoScroller] ERROR: {message}");
        }
    }
}