// ════════════════════════════════════════════════════════════════════════
// Assets/Scripts/BubbleSpinner/Data/ConversationAsset.cs
// BubbleSpinner - Conversation Configuration
// ════════════════════════════════════════════════════════════════════════

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace BubbleSpinner.Data
{
    [CreateAssetMenu(fileName = "NewConversation", menuName = "BubbleSpinner/Conversation Asset")]
    public class ConversationAsset : ScriptableObject
    {
        [Header("Character Info")]
        public string characterName;
        public AssetReference profileImage;

        [Header("Unique Identifier")]
        [Tooltip("Auto-generated unique ID. DO NOT MODIFY.")]
        [SerializeField] private string conversationId;

        [Header("Dialogue Chapters")]
        [Tooltip("List of .bub files in chapter order")]
        public List<TextAsset> chapters = new List<TextAsset>();

        [Header("CG Gallery")]
        [Tooltip("All CGs for this character (Addressable keys)")]
        public List<string> cgAddressableKeys = new List<string>();

        public string ConversationId
        {
            get
            {
#if UNITY_EDITOR
                if (string.IsNullOrEmpty(conversationId))
                {
                    GenerateNewId();
                    UnityEditor.EditorUtility.SetDirty(this);
                }
#else
                if (string.IsNullOrEmpty(conversationId))
                {
                    Debug.LogError($"ConversationId is empty for {characterName}!");
                    return $"INVALID_{characterName}";
                }
#endif
                return conversationId;
            }
        }

        private void GenerateNewId()
        {
            string guid = System.Guid.NewGuid().ToString("N");
            string shortId = guid.Substring(0, 6);
            conversationId = $"{shortId}_{characterName}";
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (string.IsNullOrEmpty(conversationId))
            {
                GenerateNewId();
                UnityEditor.EditorUtility.SetDirty(this);
            }
        }
#endif
    }
}