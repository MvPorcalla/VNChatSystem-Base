// ════════════════════════════════════════════════════════════════════════
// Assets/Scripts/UI/ChatApp/Components/MessageBubble.cs
// Phone Chat Simulation Game - Message Bubble Component
// ════════════════════════════════════════════════════════════════════════

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using BubbleSpinner.Data;

namespace ChatSim.UI.ChatApp
{
    /// <summary>
    /// Attached to each message bubble prefab.
    /// Handles initialization of text/image content and any bubble-specific animations.
    /// </summary>
    public class MessageBubble : MonoBehaviour
    {
        // ═══════════════════════════════════════════════════════════
        // ░ COMPONENTS
        // ═══════════════════════════════════════════════════════════
        
        [SerializeField] private TextMeshProUGUI messageText;
        [SerializeField] private Image messageImage;
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
                    Debug.LogWarning($"[MessageBubble] AutoResizeText not found on {gameObject.name} - text won't auto-resize");
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
                
                case MessageData.MessageType.Image:
                    InitializeImageBubble(msg, instant);
                    break;
            }
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
        // ░ IMAGE BUBBLE INITIALIZATION
        // ═══════════════════════════════════════════════════════════
        
        private void InitializeImageBubble(MessageData msg, bool instant)
        {
            // TODO: Load image from Addressables using msg.imagePath
            // For now, just show placeholder
            
            if (messageImage != null)
            {
                // Future: LoadImageFromAddressables(msg.imagePath);
                Debug.LogWarning($"[MessageBubble] Image loading not yet implemented for: {msg.imagePath}");
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