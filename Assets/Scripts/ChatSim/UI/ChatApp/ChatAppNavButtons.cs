// ════════════════════════════════════════════════════════════════════════
// Assets/Scripts/ChatSim/UI/HomeScreen/ChatAppNavButtons.cs
// ════════════════════════════════════════════════════════════════════════

using UnityEngine;
using UnityEngine.UI;
using ChatSim.Core;
using ChatSim.UI.ChatApp.Controllers;

namespace ChatSim.UI.HomeScreen
{
    /// <summary>
    /// Handles the three phone OS navigation buttons: Home, Back, and Quit.
    /// Context-sensitive: behavior changes depending on which panel is active.
    /// Attach to: NavigationBar GameObject
    /// </summary>
    public class ChatAppNavButtons : MonoBehaviour
    {
        // ═══════════════════════════════════════════════════════════
        // ░ INSPECTOR REFERENCES - NAVIGATION BUTTONS
        // ═══════════════════════════════════════════════════════════

        [Header("Navigation Buttons")]
        [SerializeField] private Button homeButton;
        [SerializeField] private Button backButton;
        [SerializeField] private Button quitButton;

        // ═══════════════════════════════════════════════════════════
        // ░ INSPECTOR REFERENCES - QUIT CONFIRMATION
        // ═══════════════════════════════════════════════════════════

        [Header("Quit Confirmation")]
        [SerializeField] private GameObject quitConfirmationPanel;
        [SerializeField] private Button yesQuitButton;
        [SerializeField] private Button noQuitButton;

        // ═══════════════════════════════════════════════════════════
        // ░ INSPECTOR REFERENCES - CHAT APP
        // ═══════════════════════════════════════════════════════════

        [Header("Chat App")]
        [SerializeField] private ChatAppController chatAppController;

        // ═══════════════════════════════════════════════════════════
        // ░ UNITY LIFECYCLE
        // ═══════════════════════════════════════════════════════════

        private void Awake()
        {
            ValidateReferences();
            SetupEventListeners();
            InitializeState();
        }

        // ═══════════════════════════════════════════════════════════
        // ░ INITIALIZATION
        // ═══════════════════════════════════════════════════════════

        private void ValidateReferences()
        {
            if (homeButton == null)
                Debug.LogWarning("[ChatAppNavButtons] homeButton not assigned!");

            if (backButton == null)
                Debug.LogWarning("[ChatAppNavButtons] backButton not assigned!");

            if (quitButton == null)
                Debug.LogWarning("[ChatAppNavButtons] quitButton not assigned!");

            if (quitConfirmationPanel == null)
                Debug.LogWarning("[ChatAppNavButtons] quitConfirmationPanel not assigned!");

            if (chatAppController == null)
                Debug.LogError("[ChatAppNavButtons] chatAppController not assigned!");
        }

        private void SetupEventListeners()
        {
            homeButton?.onClick.AddListener(OnHomePressed);
            backButton?.onClick.AddListener(OnBackPressed);
            quitButton?.onClick.AddListener(() => quitConfirmationPanel?.SetActive(true));
            yesQuitButton?.onClick.AddListener(OnConfirmQuit);
            noQuitButton?.onClick.AddListener(() => quitConfirmationPanel?.SetActive(false));
        }

        private void InitializeState()
        {
            if (quitConfirmationPanel != null)
                quitConfirmationPanel.SetActive(false);
        }

        // ═══════════════════════════════════════════════════════════
        // ░ BUTTON HANDLERS
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Home always returns to phone home screen.
        /// Cleans up chat first if active.
        /// </summary>
        private void OnHomePressed()
        {
            Debug.Log("[ChatAppNavButtons] Home pressed");

            if (chatAppController != null && chatAppController.IsChatActive)
                chatAppController.ExitForSceneTransition();

            GameBootstrap.SceneFlow.GoToPhoneScreen();
        }

        /// <summary>
        /// Back is context-sensitive:
        /// In Chat → Contact List
        /// In Contact List → Phone Home Screen
        /// </summary>
        private void OnBackPressed()
        {
            Debug.Log("[ChatAppNavButtons] Back pressed");

            if (chatAppController != null && chatAppController.IsChatActive)
            {
                Debug.Log("[ChatAppNavButtons] Back: ChatApp → ContactList");
                chatAppController.ExitToContactList();
            }
            else
            {
                Debug.Log("[ChatAppNavButtons] Back: ContactList → PhoneScreen");
                GameBootstrap.SceneFlow.GoToPhoneScreen();
            }
        }

        /// <summary>
        /// Quit confirmation handler.
        /// </summary>
        private void OnConfirmQuit()
        {
            Debug.Log("[ChatAppNavButtons] Quitting game...");

            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #else
            Application.Quit();
            #endif
        }
    }
}