// ════════════════════════════════════════════════════════════════════════
// Assets/Scripts/UI/ChatApp/Panels/ContactListPanel.cs
// Phone Chat Simulation Game - Contact List Controller
// ════════════════════════════════════════════════════════════════════════

using System.Collections.Generic;
using UnityEngine;
using BubbleSpinner.Data;
using ChatSim.UI.ChatApp;

namespace ChatSim.UI.ChatApp
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
        [SerializeField] private GameObject characterButtonPrefab;
        
        [Header("Controller Reference")]
        [SerializeField] private ChatAppController chatController;
        
        #endregion
        
        #region Unity Lifecycle
        
        private void Start()
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
                Debug.LogError("[ContactListPanel] CharacterDatabase is not assigned!");
                return;
            }
            
            var conversations = characterDatabase.GetAllCharacters();
            
            if (conversations == null || conversations.Count == 0)
            {
                Debug.LogWarning("[ContactListPanel] No conversations found in database!");
                return;
            }
            
            foreach (var conversation in conversations)
            {
                if (conversation == null)
                {
                    Debug.LogWarning("[ContactListPanel] Null conversation in database, skipping");
                    continue;
                }
                
                CreateContactButton(conversation);
            }
            
            Debug.Log($"[ContactListPanel] Populated {conversations.Count} contacts from database");
        }
        
        private void CreateContactButton(ConversationAsset conversation)
        {
            GameObject buttonObj = Instantiate(characterButtonPrefab, contactContainer);
            
            var characterButton = buttonObj.GetComponent<CharacterButton>();
            
            if (characterButton != null)
            {
                characterButton.Initialize(conversation, chatController);
            }
            else
            {
                Debug.LogError("[ContactListPanel] CharacterButton component missing on prefab!");
            }
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
    }
}