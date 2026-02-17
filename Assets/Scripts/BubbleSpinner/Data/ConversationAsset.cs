// ════════════════════════════════════════════════════════════════════════
// Assets/Scripts/BubbleSpinner/Data/ConversationAsset.cs
// ════════════════════════════════════════════════════════════════════════

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace BubbleSpinner.Data
{

    /// <summary>
    /// ScriptableObject that defines a character and their associated conversation data.
    /// Each ConversationAsset represents a character in the game and contains:
    /// - Character name and profile image
    /// - Unique conversation ID (auto-generated)
    /// - List of dialogue chapters (.bub files)
    /// - List of CGs to unlock (Addressable keys)
    /// 
    /// This asset is used by BubbleSpinner to look up conversation data when starting a conversation.
    /// You can create multiple ConversationAssets for different characters in your game.
    /// The unique ConversationId is generated automatically based on the character name, but you can modify it if needed (just ensure it remains unique).
    /// Make sure to keep this asset updated with any new characters or conversations you add to your game.
    /// Note: This asset is separate from the CharacterDatabase. It is meant to be referenced by the database and used at runtime for conversation management.
        /// </summary>

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