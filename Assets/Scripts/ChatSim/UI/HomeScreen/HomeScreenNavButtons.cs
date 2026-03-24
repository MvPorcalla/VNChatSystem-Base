// ════════════════════════════════════════════════════════════════════════
// Assets/Scripts/ChatSim/UI/HomeScreen/HomeScreenNavButtons.cs
// ════════════════════════════════════════════════════════════════════════

using UnityEngine;
using UnityEngine.UI;
using ChatSim.Core;

namespace ChatSim.UI.HomeScreen
{
    /// <summary>
    /// Handles the three phone OS navigation buttons on the Home Screen.
    /// Delegates navigation to HomeScreenController for panel stack management.
    /// Attach to: NavigationBar GameObject in HomeScreen scene
    /// </summary>
    public class HomeScreenNavButtons : MonoBehaviour
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
        // ░ INSPECTOR REFERENCES - HOME SCREEN
        // ═══════════════════════════════════════════════════════════

        [Header("Home Screen")]
        [SerializeField] private HomeScreenController homeScreenController;

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

            if (homeScreenController == null)
                LogError("homeScreenController not assigned!");
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
        /// Home always returns to the phone home screen panel.
        /// </summary>
        private void OnHomePressed()
        {
            Log("Home pressed");
            homeScreenController?.GoHome();
        }

        /// <summary>
        /// Back navigates to the previous panel in the stack.
        /// Falls back to GoHome if no history exists.
        /// </summary>
        private void OnBackPressed()
        {
            Log("Back pressed");
            homeScreenController?.GoBack();
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
            if (GameBootstrap.Config == null || !GameBootstrap.Config.homeScreenDebugLogs) return;
            UnityEngine.Debug.Log($"[HomeScreenNavButtons] {message}");
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR"), System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]
        private void LogWarning(string message)
        {
            if (GameBootstrap.Config == null || !GameBootstrap.Config.homeScreenDebugLogs) return;
            UnityEngine.Debug.LogWarning($"[HomeScreenNavButtons] WARNING: {message}");
        }

        private void LogError(string message)
        {
            UnityEngine.Debug.LogError($"[HomeScreenNavButtons] ERROR: {message}");
        }
    }
}