// ════════════════════════════════════════════════════════════════════════
// Assets/Scripts/UI/ChatApp/Core/ChatMessageSpawner.cs
// ════════════════════════════════════════════════════════════════════════

using UnityEngine;
using BubbleSpinner.Data;
using ChatSim.UI.ChatApp.Components;

namespace ChatSim.UI.ChatApp.Controllers
{
    /// <summary>
    /// Handles message bubble spawning and display.
    /// </summary>
    public class ChatMessageSpawner : MonoBehaviour
    {
        // ═══════════════════════════════════════════════════════════
        // ░ PREFAB REFERENCES
        // ═══════════════════════════════════════════════════════════
        
        [Header("Message Prefabs")]
        [SerializeField] private GameObject systemBubblePrefab;
        [SerializeField] private GameObject npcTextBubblePrefab;
        [SerializeField] private GameObject npcImageBubblePrefab;
        [SerializeField] private GameObject playerTextBubblePrefab;
        [SerializeField] private GameObject playerImageBubblePrefab;
        
        [Header("Content Container")]
        [SerializeField] private RectTransform chatContent;
        
        // ═══════════════════════════════════════════════════════════
        // ░ PUBLIC API
        // ═══════════════════════════════════════════════════════════
        
        /// <summary>
        /// Returns the content container where message bubbles are spawned.
        /// </summary>
        public Transform GetChatContent()
        {
            return chatContent;
        }
        
        public void DisplayMessage(MessageData msg, bool instant = false)
        {
            GameObject bubblePrefab = GetBubblePrefab(msg);
            
            if (bubblePrefab == null)
            {
                Debug.LogError($"[ChatMessageSpawner] No prefab for type: {msg.type}, speaker: {msg.speaker}");
                return;
            }
            
            // Instantiate bubble
            GameObject bubbleObj = Instantiate(bubblePrefab, chatContent);
            
            // Initialize bubble
            var bubble = bubbleObj.GetComponent<MessageBubble>();
            if (bubble != null)
            {
                bubble.Initialize(msg, instant);
            }
            else
            {
                Debug.LogError($"[ChatMessageSpawner] MessageBubble component missing on prefab!");
            }
        }
        
        /// <summary>
        /// Clear all messages from the chat (e.g. when switching conversations)
        /// </summary>
        public void ClearAllMessages()
        {
            if (chatContent == null)
            {
                Debug.LogError("[ChatMessageSpawner] chatContent is null!");
                return;
            }

            int destroyedCount = 0;
            
            // Destroy all children
            for (int i = chatContent.childCount - 1; i >= 0; i--)
            {
                Transform child = chatContent.GetChild(i);
                Destroy(child.gameObject);
                destroyedCount++;
            }
            
            Debug.Log($"[ChatMessageSpawner] Cleared {destroyedCount} messages");
        }
        
        // ═══════════════════════════════════════════════════════════
        // ░ PREFAB SELECTION
        // ═══════════════════════════════════════════════════════════
        
        private GameObject GetBubblePrefab(MessageData msg)
        {
            switch (msg.type)
            {
                case MessageData.MessageType.System:
                    return systemBubblePrefab;
                
                case MessageData.MessageType.Text:
                    return IsPlayerMessage(msg.speaker) ? playerTextBubblePrefab : npcTextBubblePrefab;
                
                case MessageData.MessageType.Image:
                    return IsPlayerMessage(msg.speaker) ? playerImageBubblePrefab : npcImageBubblePrefab;
                
                default:
                    Debug.LogWarning($"[ChatMessageSpawner] Unknown message type: {msg.type}");
                    return null;
            }
        }
        
        private bool IsPlayerMessage(string speaker)
        {
            return speaker.ToLower() == "player" || speaker.StartsWith("#");
        }
    }
}