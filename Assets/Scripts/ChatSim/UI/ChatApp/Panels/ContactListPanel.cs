// ════════════════════════════════════════════════════════════════════════
// Assets/Scripts/UI/ChatApp/Panels/ContactListPanel.cs
// ════════════════════════════════════════════════════════════════════════

using System.Collections.Generic;
using UnityEngine;
using BubbleSpinner.Data;
using ChatSim.UI.ChatApp.Controllers;
using ChatSim.Core;

namespace ChatSim.UI.ChatApp.Panels
{
    /// <summary>
    /// Manages the contact list UI and populates character buttons
    /// Attach to: ContactListPanel GameObject
    /// </summary>
    public class ContactListPanel : MonoBehaviour
    {
        #region Inspector References
        
        [Header("Database")]
        [SerializeField] private CharacterDatabase characterDatabase;
        
        [Header("UI References")]
        [SerializeField] private Transform contactContainer;
        [SerializeField] private GameObject ContactListItemPrefab;
        
        [Header("Controller Reference")]
        [SerializeField] private ChatAppController chatController;
        
        #endregion
        
        #region Unity Lifecycle
        
        private void OnEnable()
        {
            PopulateContactList();
        }
        
        #endregion
        
        #region Contact List Population
        
        private void PopulateContactList()
        {
            ClearContactList();

            if (characterDatabase == null)
            {
                LogError("CharacterDatabase is not assigned!");
                return;
            }

            var conversations = characterDatabase.GetAllCharacters();

            if (conversations == null || conversations.Count == 0)
            {
                LogWarning("No conversations found in database!");
                return;
            }

            foreach (var conversation in conversations)
            {
                if (conversation == null)
                {
                    LogWarning("Null conversation in database, skipping");
                    continue;
                }

                CreateContactButton(conversation);
            }

            Log($"Populated {conversations.Count} contacts from database");
        }
        
        private void CreateContactButton(ConversationAsset conversation)
        {
            GameObject buttonObj = Instantiate(ContactListItemPrefab, contactContainer);
            var contactItem = buttonObj.GetComponent<ContactListItem>();

            if (contactItem != null)
            {
                string lastMessage = GetLastMessagePreview(conversation.ConversationId);
                contactItem.Initialize(conversation, chatController, lastMessage);
            }
            else
            {
                LogError("ContactListItem component missing on prefab!");
            }
        }

        private string GetLastMessagePreview(string conversationId)
        {
            var saveData = GameBootstrap.Save?.GetOrCreateSaveData();
            var state = saveData?.conversationStates
                .Find(s => s.conversationId == conversationId);

            if (state == null || state.messageHistory == null || state.messageHistory.Count == 0)
                return "";

            for (int i = state.messageHistory.Count - 1; i >= 0; i--)
            {
                var msg = state.messageHistory[i];

                if (msg.IsSystemMessage)
                    continue;

                if (msg.type == MessageData.MessageType.Image)
                    return msg.IsPlayerMessage ? "You sent an image." : "Sent an image.";

                if (msg.type == MessageData.MessageType.Text)
                    return msg.content;
            }

            return "";
        }
        
        private void ClearContactList()
        {
            foreach (Transform child in contactContainer)
            {
                Destroy(child.gameObject);
            }
        }
        
        #endregion
        
        #region Public API
        
        /// <summary>
        /// Refresh the contact list (call after adding/removing conversations)
        /// </summary>
        public void RefreshContactList()
        {
            PopulateContactList();
        }
        
        #endregion

        #region Logging

        [System.Diagnostics.Conditional("UNITY_EDITOR"), System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]
        private void Log(string message)
        {
            if (GameBootstrap.Config == null || !GameBootstrap.Config.contactChatListDebugLogs) return;
            UnityEngine.Debug.Log($"[ContactListPanel] {message}");
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR"), System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]
        private void LogWarning(string message)
        {
            if (GameBootstrap.Config == null || !GameBootstrap.Config.contactChatListDebugLogs) return;
            UnityEngine.Debug.LogWarning($"[ContactListPanel] WARNING: {message}");
        }

        private void LogError(string message)
        {
            UnityEngine.Debug.LogError($"[ContactListPanel] ERROR: {message}");
        }

        #endregion
    }
}