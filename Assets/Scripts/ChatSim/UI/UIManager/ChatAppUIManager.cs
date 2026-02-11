// ════════════════════════════════════════════════════════════════════════
// Assets/Scripts/UI/ChatApp/ChatAppUIManager.cs
// Manages the UI panels and navigation for the Chat App, including contact list and chat conversation screens.
// ════════════════════════════════════════════════════════════════════════

using UnityEngine;
using UnityEngine.UI;
using ChatSim.Core;

namespace ChatSim.UI.UIManager
{
    public class ChatAppUIManager : MonoBehaviour
    {
        [Header("Panel References")]
        [SerializeField] private GameObject contactListPanel;
        [SerializeField] private GameObject chatAppPanel;
        
        [Header("Controller Reference")]
        [SerializeField] private ChatApp.ChatAppController chatController;
        
        [Header("Navigation Buttons")]
        [SerializeField] private Button quitButton;
        [SerializeField] private Button homeButton;
        [SerializeField] private Button backButton;

        [Header("Quit Confirmation")]
        [SerializeField] private GameObject quitConfirmationPanel;
        [SerializeField] private Button yesQuitButton;
        [SerializeField] private Button noQuitButton;
        
        private void Awake()
        {
            InitializePanelStates();
            InitializeNavigation();
        }
        
        private void InitializePanelStates()
        {
            if (contactListPanel == null || chatAppPanel == null)
            {
                Debug.LogError("[ChatAppUIManager] Panel references are missing!");
                return;
            }
            
            contactListPanel.SetActive(true);
            chatAppPanel.SetActive(false);
            
            if (quitConfirmationPanel != null)
                quitConfirmationPanel.SetActive(false);
            
            Debug.Log("[ChatAppUIManager] Initialized: ContactList=ACTIVE, ChatApp=INACTIVE");
        }

        private void InitializeNavigation()
        {
            // Quit button
            if (quitButton != null)
                quitButton.onClick.AddListener(() => quitConfirmationPanel?.SetActive(true));
            else
                Debug.LogWarning("[ChatAppUIManager] QuitButton not assigned!");

            // Quit confirmation
            if (yesQuitButton != null)
                yesQuitButton.onClick.AddListener(OnConfirmQuit);
            
            if (noQuitButton != null)
                noQuitButton.onClick.AddListener(() => quitConfirmationPanel?.SetActive(false));

            // Home button
            if (homeButton != null)
                homeButton.onClick.AddListener(OnHomePressed);
            else
                Debug.LogWarning("[ChatAppUIManager] HomeButton not assigned!");

            // Back button
            if (backButton != null)
                backButton.onClick.AddListener(OnBackPressed);
            else
                Debug.LogWarning("[ChatAppUIManager] BackButton not assigned!");
        }

        private void OnConfirmQuit()
        {
            Debug.Log("[ChatAppUIManager] Quitting game...");
            
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #else
            Application.Quit();
            #endif
        }

        private void OnHomePressed()
        {
            Debug.Log("[ChatAppUIManager] Home button pressed");
            
            // Clean up conversation if chat is active
            CleanupChatIfActive();
            
            // Navigate to phone screen
            GameBootstrap.SceneFlow.GoToPhoneScreen();
        }

        private void OnBackPressed()
        {
            if (chatAppPanel != null && chatAppPanel.activeSelf)
            {
                // In Chat → Go to Contact List
                Debug.Log("[ChatAppUIManager] Back: ChatApp → ContactList");
                
                // CRITICAL: Clean up conversation BEFORE switching panels
                if (chatController != null)
                {
                    chatController.ExitCurrentConversation();
                }
                
                ShowContactList();
            }
            else if (contactListPanel != null && contactListPanel.activeSelf)
            {
                // In Contact List → Go to Phone Home Screen
                Debug.Log("[ChatAppUIManager] Back: ContactList → PhoneScreen");
                GameBootstrap.SceneFlow.GoToPhoneScreen();
            }
            else
            {
                Debug.LogWarning("[ChatAppUIManager] Back pressed but no valid panel is active!");
            }
        }
        
        public void ShowContactList()
        {
            if (chatAppPanel != null)
                chatAppPanel.SetActive(false);
            
            if (contactListPanel != null)
                contactListPanel.SetActive(true);
            
            Debug.Log("[ChatAppUIManager] Switched to ContactList");
        }
        
        public void ShowChatApp()
        {
            if (contactListPanel != null)
                contactListPanel.SetActive(false);
            
            if (chatAppPanel != null)
                chatAppPanel.SetActive(true);
            
            Debug.Log("[ChatAppUIManager] Switched to ChatApp");
        }
        
        /// <summary>
        /// Clean up chat conversation if chat panel is currently active
        /// </summary>
        private void CleanupChatIfActive()
        {
            if (chatController != null && chatAppPanel != null && chatAppPanel.activeSelf)
            {
                Debug.Log("[ChatAppUIManager] Cleaning up active chat conversation");
                chatController.ExitCurrentConversation();
            }
        }
    }
}