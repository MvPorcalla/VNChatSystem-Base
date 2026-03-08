// ════════════════════════════════════════════════════════════════════════
// Assets/Scripts/UI/ChatApp/Components/TextMessageBubble.cs
// ════════════════════════════════════════════════════════════════════════

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using BubbleSpinner.Data;
using ChatSim.UI.Common.Components;

namespace ChatSim.UI.ChatApp.Components
{
    /// <summary>
    /// Attached to each message bubble prefab.
    /// Handles initialization of text/image content and any bubble-specific animations.
    /// </summary>
    public class TextMessageBubble : MonoBehaviour
    {
        // ═══════════════════════════════════════════════════════════
        // ░ COMPONENTS
        // ═══════════════════════════════════════════════════════════
        
        [SerializeField] private TextMeshProUGUI messageText;
        [SerializeField] private CanvasGroup canvasGroup;
        
        // Auto-resize component reference
        private AutoResizeText autoResize;
        
        // ═══════════════════════════════════════════════════════════
        // ░ INITIALIZATION
        // ═══════════════════════════════════════════════════════════
        
        private void Awake()
        {
            // Get AutoResizeText component (if text bubble)
            if (messageText != null)
            {
                autoResize = messageText.GetComponent<AutoResizeText>();
                
                if (autoResize == null)
                {
                    Debug.LogWarning($"[TextMessageBubble] AutoResizeText not found on {gameObject.name} - text won't auto-resize");
                }
            }
        }
        
        // ═══════════════════════════════════════════════════════════
        // ░ PUBLIC API
        // ═══════════════════════════════════════════════════════════
        
        public void Initialize(MessageData msg, bool instant = false)
        {
            switch (msg.type)
            {
                case MessageData.MessageType.System:
                case MessageData.MessageType.Text:
                    InitializeTextBubble(msg, instant);
                    break;
            }
        }

        // ═══════════════════════════════════════════════════════════
        // ░ POOLING RESET
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Called by ChatMessageSpawner before returning this bubble to the pool.
        /// Clears all content so stale data never shows on reuse.
        /// </summary>
        public void ResetForPool()
        {
            // Stop any running fade coroutine
            StopAllCoroutines();

            // Clear text
            if (messageText != null)
                messageText.text = string.Empty;

            // Reset alpha
            if (canvasGroup != null)
                canvasGroup.alpha = 1f;
        }
        
        // ═══════════════════════════════════════════════════════════
        // ░ TEXT BUBBLE INITIALIZATION
        // ═══════════════════════════════════════════════════════════
        
        private void InitializeTextBubble(MessageData msg, bool instant)
        {
            if (messageText != null)
            {
                if (autoResize != null)
                {
                    autoResize.SetText(msg.content);
                }
                else
                {
                    // Fallback to direct assignment
                    messageText.text = msg.content;
                }
            }
            
            // Fade-in animation (unless instant)
            if (!instant && canvasGroup != null)
            {
                StartCoroutine(FadeIn());
            }
            else if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
            }
        }
        
        // ═══════════════════════════════════════════════════════════
        // ░ ANIMATION
        // ═══════════════════════════════════════════════════════════
        
        private System.Collections.IEnumerator FadeIn()
        {
            float duration = 0.3f;
            float elapsed = 0f;
            
            canvasGroup.alpha = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / duration);
                yield return null;
            }
            
            canvasGroup.alpha = 1f;
        }
    }
}