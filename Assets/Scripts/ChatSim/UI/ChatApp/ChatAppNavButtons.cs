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
                LogWarning("homeButton not assigned!");

            if (backButton == null)
                LogWarning("backButton not assigned!");

            if (quitButton == null)
                LogWarning("quitButton not assigned!");

            if (quitConfirmationPanel == null)
                LogWarning("quitConfirmationPanel not assigned!");

            if (chatAppController == null)
                LogError("chatAppController not assigned!");
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
            Log("Home pressed");

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
            Log("Back pressed");

            if (chatAppController != null && chatAppController.IsChatActive)
            {
                Log("Back: ChatApp → ContactList");
                chatAppController.ExitToContactList();
            }
            else
            {
                Log("Back: ContactList → PhoneScreen");
                GameBootstrap.SceneFlow.GoToPhoneScreen();
            }
        }

        /// <summary>
        /// Quit confirmation handler.
        /// </summary>
        private void OnConfirmQuit()
        {
            Log("Quitting game...");

            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #else
            Application.Quit();
            #endif
        }

        // ═══════════════════════════════════════════════════════════
        // ░ LOGGING
        // ═══════════════════════════════════════════════════════════

        [System.Diagnostics.Conditional("UNITY_EDITOR"), System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]
        private void Log(string message)
        {
            if (GameBootstrap.Config == null || !GameBootstrap.Config.chatAppNavButtonsDebugLogs) return;
            UnityEngine.Debug.Log($"[ChatAppNavButtons] {message}");
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR"), System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]
        private void LogWarning(string message)
        {
            if (GameBootstrap.Config == null || !GameBootstrap.Config.chatAppNavButtonsDebugLogs) return;
            UnityEngine.Debug.LogWarning($"[ChatAppNavButtons] WARNING: {message}");
        }

        private void LogError(string message)
        {
            UnityEngine.Debug.LogError($"[ChatAppNavButtons] ERROR: {message}");
        }
    }
}