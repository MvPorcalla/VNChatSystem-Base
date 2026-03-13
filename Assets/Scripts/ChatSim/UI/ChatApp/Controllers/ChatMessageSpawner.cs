// ════════════════════════════════════════════════════════════════════════
// Assets/Scripts/UI/ChatApp/Core/ChatMessageSpawner.cs
// ════════════════════════════════════════════════════════════════════════

using System.Collections.Generic;
using UnityEngine;
using BubbleSpinner.Data;
using ChatSim.UI.ChatApp.Components;
using ChatSim.UI.Common.Components;
using ChatSim.UI.Common.Pooling;

namespace ChatSim.UI.ChatApp.Controllers
{
    /// <summary>
    /// Handles message bubble spawning and display.
    /// Pools MessageBubble (text/system) prefabs for performance.
    /// ImageMessageBubble is NOT pooled due to async Addressables loading.
    /// </summary>
    public class ChatMessageSpawner : MonoBehaviour
    {
        // ═══════════════════════════════════════════════════════════
        // ░ INSPECTOR REFERENCES
        // ═══════════════════════════════════════════════════════════

        [Header("Message Prefabs")]
        [SerializeField] private GameObject systemBubblePrefab;
        [SerializeField] private GameObject npcTextBubblePrefab;
        [SerializeField] private GameObject npcImageBubblePrefab;
        [SerializeField] private GameObject playerTextBubblePrefab;
        [SerializeField] private GameObject playerImageBubblePrefab;

        [Header("Content Container")]
        [SerializeField] private RectTransform chatContent;

        [Header("Pooling")]
        [SerializeField] private PoolingManager poolingManager;
        [SerializeField] private int prewarmCount = 10;

        // ═══════════════════════════════════════════════════════════
        // ░ STATE
        // ═══════════════════════════════════════════════════════════

        // Tracks pooled bubbles (text + system) for recycle on clear
        private List<GameObject> pooledBubbles = new List<GameObject>();

        // Tracks non-pooled image bubbles for destroy on clear
        private List<GameObject> imageBubbles = new List<GameObject>();

        // ═══════════════════════════════════════════════════════════
        // ░ INITIALIZATION
        // ═══════════════════════════════════════════════════════════

        private void Start()
        {
            PrewarmPools();
        }

        private void PrewarmPools()
        {
            if (poolingManager == null) return;

            // Only prewarm text/system bubbles — image bubbles are not pooled
            if (npcTextBubblePrefab != null)
                poolingManager.PreWarm(npcTextBubblePrefab, prewarmCount);

            if (playerTextBubblePrefab != null)
                poolingManager.PreWarm(playerTextBubblePrefab, prewarmCount / 2);

            if (systemBubblePrefab != null)
                poolingManager.PreWarm(systemBubblePrefab, 3);

            Debug.Log("[ChatMessageSpawner] Pools prewarmed");
        }

        // ═══════════════════════════════════════════════════════════
        // ░ PUBLIC API
        // ═══════════════════════════════════════════════════════════

        public Transform GetChatContent() => chatContent;

        /// <summary>
        /// Display a message bubble.
        /// Text and system bubbles are pooled.
        /// Image bubbles are instantiated directly (not pooled).
        /// </summary>
        public void DisplayMessage(MessageData msg, bool instant = false)
        {
            if (msg.type == MessageData.MessageType.Image)
            {
                SpawnImageBubble(msg, instant);
            }
            else
            {
                SpawnPooledBubble(msg, instant);
            }
        }

        /// <summary>
        /// Recycles all pooled bubbles and destroys all image bubbles.
        /// </summary>
        public void ClearAllMessages()
        {
            if (chatContent == null)
            {
                Debug.LogError("[ChatMessageSpawner] chatContent is null!");
                return;
            }

            // Recycle pooled text/system bubbles
            foreach (var bubble in pooledBubbles)
            {
                if (bubble == null) continue;

                var messageBubble = bubble.GetComponent<TextMessageBubble>();
                messageBubble?.ResetForPool();

                if (poolingManager != null)
                    poolingManager.Recycle(bubble);
                else
                    Destroy(bubble);
            }
            pooledBubbles.Clear();

            // Destroy image bubbles (not pooled)
            foreach (var bubble in imageBubbles)
            {
                if (bubble != null)
                    Destroy(bubble);
            }
            imageBubbles.Clear();

            Debug.Log("[ChatMessageSpawner] All messages cleared");
        }

        // ═══════════════════════════════════════════════════════════
        // ░ SPAWNING
        // ═══════════════════════════════════════════════════════════

        private void SpawnPooledBubble(MessageData msg, bool instant)
        {
            GameObject prefab = GetTextBubblePrefab(msg);

            if (prefab == null)
            {
                Debug.LogError($"[ChatMessageSpawner] No prefab for type: {msg.type}, speaker: {msg.speaker}");
                return;
            }

            GameObject bubbleObj = poolingManager != null
                ? poolingManager.Get(prefab, chatContent, activateOnGet: true)
                : Instantiate(prefab, chatContent);

            var bubble = bubbleObj.GetComponent<TextMessageBubble>();
            if (bubble != null)
            {
                bubble.Initialize(msg, instant);
                pooledBubbles.Add(bubbleObj);
            }
            else
            {
                Debug.LogError("[ChatMessageSpawner] MessageBubble component missing!");
                Destroy(bubbleObj);
            }
        }

        private void SpawnImageBubble(MessageData msg, bool instant)
        {
            GameObject prefab = msg.IsPlayerMessage
                ? playerImageBubblePrefab
                : npcImageBubblePrefab;

            if (prefab == null)
            {
                Debug.LogError($"[ChatMessageSpawner] No image prefab for speaker: {msg.speaker}");
                return;
            }

            GameObject bubbleObj = Instantiate(prefab, chatContent);

            var imageBubble = bubbleObj.GetComponent<ImageMessageBubble>();
            if (imageBubble != null)
            {
                imageBubble.Initialize(msg, instant);
                imageBubbles.Add(bubbleObj);
            }
            else
            {
                Debug.LogError("[ChatMessageSpawner] ImageMessageBubble component missing!");
                Destroy(bubbleObj);
            }
        }

        // ═══════════════════════════════════════════════════════════
        // ░ PREFAB SELECTION
        // ═══════════════════════════════════════════════════════════

        private GameObject GetTextBubblePrefab(MessageData msg)
        {
            switch (msg.type)
            {
                case MessageData.MessageType.System:
                    return systemBubblePrefab;

                case MessageData.MessageType.Text:
                    return msg.IsPlayerMessage
                        ? playerTextBubblePrefab
                        : npcTextBubblePrefab;

                default:
                    return null;
            }
        }

        // ═══════════════════════════════════════════════════════════
        // ░ VALIDATION
        // ═══════════════════════════════════════════════════════════

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (systemBubblePrefab == null)
                Debug.LogWarning("[ChatMessageSpawner] systemBubblePrefab not assigned!");

            if (npcTextBubblePrefab == null)
                Debug.LogWarning("[ChatMessageSpawner] npcTextBubblePrefab not assigned!");

            if (npcImageBubblePrefab == null)
                Debug.LogWarning("[ChatMessageSpawner] npcImageBubblePrefab not assigned!");

            if (playerTextBubblePrefab == null)
                Debug.LogWarning("[ChatMessageSpawner] playerTextBubblePrefab not assigned!");

            if (playerImageBubblePrefab == null)
                Debug.LogWarning("[ChatMessageSpawner] playerImageBubblePrefab not assigned!");

            if (chatContent == null)
                Debug.LogError("[ChatMessageSpawner] chatContent not assigned!");

            if (poolingManager == null)
                Debug.LogWarning("[ChatMessageSpawner] poolingManager not assigned - will fall back to Instantiate!");
        }
#endif
    }
}