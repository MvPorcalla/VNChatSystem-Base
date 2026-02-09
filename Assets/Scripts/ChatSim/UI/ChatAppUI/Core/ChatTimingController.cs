// ════════════════════════════════════════════════════════════════════════
// Assets/Scripts/UI/ChatApp/Core/ChatTimingController.cs
// Phone Chat Simulation Game - Message Timing & Animation (FIXED)
// ════════════════════════════════════════════════════════════════════════

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BubbleSpinner.Data;
using ChatSim.Core;

namespace ChatSim.UI.ChatApp
{
    /// <summary>
    /// Controls message display timing, typing indicators, and animations.
    /// Attach to: ChatAppController GameObject
    /// </summary>
    public class ChatTimingController : MonoBehaviour
    {
        // ═══════════════════════════════════════════════════════════
        // ░ INSPECTOR SETTINGS
        // ═══════════════════════════════════════════════════════════
        
        [Header("Timing Settings")]
        [SerializeField] private float messageDelay = 1.2f;
        [SerializeField] private float typingIndicatorDuration = 1.5f;
        [SerializeField] private float playerMessageDelay = 0.3f;
        [SerializeField] private float finalDelayBeforeChoices = 0.2f;

        [Header("Fast Mode")]
        [SerializeField] private bool isFastMode = false;
        [SerializeField] private float fastModeSpeed = 0.1f;

        [Header("References")]
        [SerializeField] private ChatMessageDisplay messageDisplay;
        
        [Header("✅ PHASE 2: Typing Indicator Prefab")]
        [Tooltip("Prefab to spawn for typing indicator (will be pooled)")]
        [SerializeField] private GameObject typingIndicatorPrefab;
        
        [Header("Pooling")]
        [SerializeField] private PoolingManager poolingManager;
        
        // ═══════════════════════════════════════════════════════════
        // ░ STATE
        // ═══════════════════════════════════════════════════════════
        
        private Queue<MessageData> messageQueue = new Queue<MessageData>();
        private bool isDisplayingMessages = false;
        private Coroutine currentMessageSequence;
        private System.Action pendingCallback = null;
        private bool isSequenceCancelled = false;

        // Reference to ChatAppController for callbacks
        private ChatAppController chatController;
        
        // ✅ PHASE 2: Track active typing indicator instance
        private GameObject activeTypingIndicator = null;

        // ═══════════════════════════════════════════════════════════
        // ░ PROPERTIES
        // ═══════════════════════════════════════════════════════════

        public bool IsDisplayingMessages => isDisplayingMessages;

        // ═══════════════════════════════════════════════════════════
        // ░ INITIALIZATION
        // ═══════════════════════════════════════════════════════════

        private void Awake()
        {
            chatController = GetComponent<ChatAppController>();
            
            if (chatController == null)
            {
                Debug.LogError("[ChatTimingController] ChatAppController not found on same GameObject!");
            }
            
            // Get or create pooling manager
            if (poolingManager == null)
            {
                poolingManager = GetComponent<PoolingManager>();
                
                if (poolingManager == null)
                {
                    poolingManager = gameObject.AddComponent<PoolingManager>();
                    Debug.Log("[ChatTimingController] Created PoolingManager component");
                }
            }

            ValidateReferences();
        }

        private void ValidateReferences()
        {
            if (messageDisplay == null)
            {
                Debug.LogError("[ChatTimingController] messageDisplay not assigned!");
            }

            if (typingIndicatorPrefab == null)
            {
                Debug.LogWarning("[ChatTimingController] typingIndicatorPrefab not assigned - typing indicators disabled");
            }
        }

        // ═══════════════════════════════════════════════════════════
        // ░ PUBLIC API
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Queue messages for timed display with typing indicators.
        /// </summary>
        public void QueueMessages(List<MessageData> messages, System.Action onComplete = null)
        {
            if (messages == null || messages.Count == 0)
            {
                Debug.LogWarning("[ChatTimingController] Empty message list");
                onComplete?.Invoke();
                return;
            }

            Debug.Log($"[ChatTimingController] Queueing {messages.Count} messages (Fast Mode: {isFastMode})");

            StopCurrentSequenceIfRunning();
            ResetSequenceState();

            pendingCallback = onComplete;

            // Enqueue all messages
            messageQueue.Clear();
            foreach (var message in messages)
            {
                messageQueue.Enqueue(message);
            }

            // Start display sequence
            currentMessageSequence = StartCoroutine(DisplayMessagesSequence());
        }

        /// <summary>
        /// Stop the current message sequence.
        /// </summary>
        public void StopCurrentSequence()
        {
            Debug.Log("[ChatTimingController] Stopping current sequence");

            isSequenceCancelled = true;

            var callbackToCancel = pendingCallback;
            pendingCallback = null;

            StopAllSequenceCoroutines();
            CleanupTypingIndicator(); // ✅ PHASE 2: Use new cleanup method
            ClearMessageQueue();

            isDisplayingMessages = false;

            Debug.Log($"[ChatTimingController] Sequence stopped. Callback {(callbackToCancel != null ? "cancelled" : "was null")}");
        }

        /// <summary>
        /// Set fast mode on/off.
        /// </summary>
        public void SetFastMode(bool enabled)
        {
            isFastMode = enabled;
            Debug.Log($"[ChatTimingController] Fast mode: {(enabled ? "ENABLED" : "DISABLED")}");
        }

        // ═══════════════════════════════════════════════════════════
        // ░ MESSAGE DISPLAY SEQUENCE
        // ═══════════════════════════════════════════════════════════

        private IEnumerator DisplayMessagesSequence()
        {
            isDisplayingMessages = true;
            Debug.Log("[ChatTimingController] Starting message display sequence");

            while (messageQueue.Count > 0)
            {
                if (isSequenceCancelled)
                {
                    Debug.Log("[ChatTimingController] Sequence aborted (cancelled)");
                    break;
                }

                var message = messageQueue.Dequeue();

                // ✅ PHASE 2: Show typing indicator for NPC messages
                if (ShouldShowTypingIndicator(message))
                {
                    yield return StartCoroutine(ShowTypingIndicatorSequence());
                    
                    if (isSequenceCancelled)
                        break;
                }

                // Display the message
                messageDisplay.DisplayMessage(message, instant: isFastMode);

                // Notify ChatAppController about new message (for new message indicator)
                if (chatController != null)
                {
                    chatController.OnNewMessageDisplayed(message);
                }

                // Wait between messages
                if (messageQueue.Count > 0)
                {
                    yield return new WaitForSeconds(GetMessageDelay(message));
                }
            }

            if (!isSequenceCancelled)
            {
                // Final delay before showing choices
                if (!isFastMode && finalDelayBeforeChoices > 0)
                {
                    yield return new WaitForSeconds(finalDelayBeforeChoices);
                }

                isDisplayingMessages = false;
                Debug.Log("[ChatTimingController] Message sequence complete");
                InvokeCallbackSafely();
            }
            else
            {
                isDisplayingMessages = false;
                Debug.Log("[ChatTimingController] Message sequence cancelled - callback suppressed");
            }
        }

        // ═══════════════════════════════════════════════════════════
        // ░ ✅ PHASE 2: TYPING INDICATOR (PREFAB POOLING SYSTEM)
        // ═══════════════════════════════════════════════════════════

        private bool ShouldShowTypingIndicator(MessageData message)
        {
            if (isFastMode)
                return false;

            string speaker = message.speaker.ToLower();
            
            // Show for NPC text messages only
            return typingIndicatorPrefab != null &&
                   speaker != "player" &&
                   speaker != "system" &&
                   message.type == MessageData.MessageType.Text;
        }

        /// <summary>
        /// ✅ PHASE 2 FIX: Spawn typing indicator from prefab via pooling
        /// </summary>
        private IEnumerator ShowTypingIndicatorSequence()
        {
            if (typingIndicatorPrefab == null || poolingManager == null)
            {
                yield break;
            }
            
            Debug.Log("[ChatTimingController] Showing typing indicator");

            // Get from pool and parent to chat content
            Transform chatContent = messageDisplay.GetChatContent();
            activeTypingIndicator = poolingManager.Get(typingIndicatorPrefab, chatContent, activateOnGet: true);
            
            // Move to bottom (last sibling)
            if (activeTypingIndicator != null)
            {
                activeTypingIndicator.transform.SetAsLastSibling();
            }

            // Wait for typing duration
            yield return new WaitForSeconds(GetTypingIndicatorDuration());

            // Cleanup indicator
            if (!isSequenceCancelled)
            {
                CleanupTypingIndicator();
            }
        }

        /// <summary>
        /// ✅ PHASE 2 FIX: Recycle typing indicator back to pool
        /// </summary>
        private void CleanupTypingIndicator()
        {
            if (activeTypingIndicator == null)
                return;

            if (poolingManager != null)
            {
                poolingManager.Recycle(activeTypingIndicator);
                Debug.Log("[ChatTimingController] Typing indicator recycled");
            }
            else
            {
                Destroy(activeTypingIndicator);
                Debug.LogWarning("[ChatTimingController] No pooling manager - destroyed typing indicator");
            }

            activeTypingIndicator = null;
        }

        // ═══════════════════════════════════════════════════════════
        // ░ TIMING CALCULATIONS
        // ═══════════════════════════════════════════════════════════

        private float GetTypingIndicatorDuration()
        {
            return isFastMode ? fastModeSpeed : typingIndicatorDuration;
        }

        private float GetMessageDelay(MessageData message)
        {
            if (isFastMode)
                return fastModeSpeed;

            // Player messages have shorter delay
            string speaker = message.speaker.ToLower();
            if (speaker == "player" || speaker.StartsWith("#"))
                return playerMessageDelay;

            return messageDelay;
        }

        // ═══════════════════════════════════════════════════════════
        // ░ HELPER METHODS
        // ═══════════════════════════════════════════════════════════

        private void StopCurrentSequenceIfRunning()
        {
            if (currentMessageSequence != null)
            {
                StopCoroutine(currentMessageSequence);
                currentMessageSequence = null;
                Debug.Log("[ChatTimingController] Stopped previous sequence");
            }
        }

        private void ResetSequenceState()
        {
            isSequenceCancelled = false;
        }

        private void StopAllSequenceCoroutines()
        {
            if (currentMessageSequence != null)
            {
                StopCoroutine(currentMessageSequence);
                currentMessageSequence = null;
            }
        }

        private void ClearMessageQueue()
        {
            if (messageQueue != null)
            {
                int queuedCount = messageQueue.Count;
                messageQueue.Clear();

                if (queuedCount > 0)
                {
                    Debug.Log($"[ChatTimingController] Cleared {queuedCount} queued messages");
                }
            }
        }

        private void InvokeCallbackSafely()
        {
            if (pendingCallback != null && !isSequenceCancelled)
            {
                Debug.Log("[ChatTimingController] Invoking completion callback");
                var callback = pendingCallback;
                pendingCallback = null;
                callback.Invoke();
            }
            else if (isSequenceCancelled)
            {
                Debug.Log("[ChatTimingController] Callback suppressed (cancelled)");
                pendingCallback = null;
            }
        }

        // ═══════════════════════════════════════════════════════════
        // ░ LIFECYCLE
        // ═══════════════════════════════════════════════════════════

        private void OnDestroy()
        {
            isSequenceCancelled = true;
            pendingCallback = null;
            CleanupTypingIndicator();
        }
    }
}