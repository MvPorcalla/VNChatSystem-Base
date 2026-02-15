// ════════════════════════════════════════════════════════════════════════
// Assets/Scripts/UI/ChatApp/Panels/ContactListItem.cs
// Phone Chat Simulation Game - Contact List Button
// ════════════════════════════════════════════════════════════════════════

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using BubbleSpinner.Data;
using ChatSim.UI.ChatApp.Controllers;

namespace ChatSim.UI.ChatApp.Panels
{
    /// <summary>
    /// Individual contact button in the contact list
    /// Attach to: ContactListItem prefab
    /// </summary>
    public class ContactListItem : MonoBehaviour
    {
        #region Inspector References
        
        [Header("UI Elements")]
        [SerializeField] private Button button;
        [SerializeField] private Image profileIMG;
        [SerializeField] private TextMeshProUGUI profileName;
        [SerializeField] private GameObject badge;
        
        #endregion
        
        #region State
        
        private ConversationAsset conversationAsset;
        private ChatAppController chatController;
        
        #endregion
        
        #region Initialization
        
        /// <summary>
        /// Initialize the button with conversation data
        /// Called by ContactListPanel when creating buttons
        /// </summary>
        public void Initialize(ConversationAsset asset, ChatAppController controller)
        {
            conversationAsset = asset;
            chatController = controller;
            
            // Set character name
            if (profileName != null)
            {
                profileName.text = asset.characterName;
            }
            
            // TODO: Load profile image from Addressables
            // if (profileIMG != null && asset.profileImage != null)
            // {
            //     LoadProfileImage(asset.profileImage);
            // }
            
            // Hide badge by default (can show if there are unread messages)
            if (badge != null)
            {
                badge.SetActive(false);
            }
            
            // Setup button click
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(OnButtonClicked);
            }
            else
            {
                Debug.LogError("[ContactListItem] Button component not assigned!");
            }
        }
        
        #endregion
        
        #region Button Interaction
        
        private void OnButtonClicked()
        {
            if (conversationAsset == null)
            {
                Debug.LogError("[ContactListItem] No conversation asset assigned!");
                return;
            }
            
            if (chatController == null)
            {
                Debug.LogError("[ContactListItem] No chat controller assigned!");
                return;
            }
            
            Debug.Log($"[ContactListItem] Opening conversation with {conversationAsset.characterName}");
            
            // Start the conversation
            chatController.StartConversation(conversationAsset);
        }
        
        #endregion
        
        #region Public API
        
        /// <summary>
        /// Show/hide the notification badge
        /// </summary>
        public void SetBadgeVisible(bool visible)
        {
            if (badge != null)
            {
                badge.SetActive(visible);
            }
        }
        
        #endregion
    }
}