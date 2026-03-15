// ════════════════════════════════════════════════════════════════════════
// Assets/Scripts/ChatSim/UI/HomeScreen/Settings/SettingsResetAllDialog.cs
// ════════════════════════════════════════════════════════════════════════

using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ChatSim.UI.HomeScreen.Settings
{
    /// <summary>
    /// Confirmation dialog for Reset All Stories.
    /// Pure UI middleman — calls back to SettingsPanel on confirm.
    ///
    /// Attach to: SettingsResetAllDialog GameObject (child of SettingsPanel)
    ///
    /// Hierarchy:
    ///   SettingsResetAllDialog      ← ATTACH THIS SCRIPT — ACTIVE in scene
    ///   └── ConfirmationDialog      ← INACTIVE in scene
    ///       └── ContentPanel
    ///           ├── TitleText
    ///           ├── MessageText
    ///           ├── YesButton
    ///           └── NoButton
    /// </summary>
    public class SettingsResetAllDialog : MonoBehaviour
    {
        // ═══════════════════════════════════════════════════════════
        // ░ INSPECTOR REFERENCES
        // ═══════════════════════════════════════════════════════════

        [Header("UI Elements")]
        [SerializeField] private GameObject confirmationDialog;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI messageText;
        [SerializeField] private Button yesButton;
        [SerializeField] private Button noButton;

        // ═══════════════════════════════════════════════════════════
        // ░ CONSTANTS
        // ═══════════════════════════════════════════════════════════

        private const string TITLE   = "Reset All Stories?";
        private const string MESSAGE = "This will erase ALL chat history and progress for every character. This cannot be undone.";

        // ═══════════════════════════════════════════════════════════
        // ░ STATE
        // ═══════════════════════════════════════════════════════════

        private Action _onConfirmed;

        // ═══════════════════════════════════════════════════════════
        // ░ UNITY LIFECYCLE
        // ═══════════════════════════════════════════════════════════

        private void Awake()
        {
            SetupButtons();

            if (confirmationDialog != null)
                confirmationDialog.SetActive(false);
        }

        // ═══════════════════════════════════════════════════════════
        // ░ SETUP
        // ═══════════════════════════════════════════════════════════

        private void SetupButtons()
        {
            if (yesButton != null)
            {
                yesButton.onClick.RemoveAllListeners();
                yesButton.onClick.AddListener(OnYesClicked);
            }
            else
            {
                Debug.LogError("[SettingsResetAllDialog] yesButton not assigned!");
            }

            if (noButton != null)
            {
                noButton.onClick.RemoveAllListeners();
                noButton.onClick.AddListener(OnNoClicked);
            }
            else
            {
                Debug.LogError("[SettingsResetAllDialog] noButton not assigned!");
            }
        }

        // ═══════════════════════════════════════════════════════════
        // ░ PUBLIC API
        // ═══════════════════════════════════════════════════════════

        public void Show(Action onConfirmed)
        {
            _onConfirmed = onConfirmed;

            if (titleText != null)   titleText.text   = TITLE;
            if (messageText != null) messageText.text = MESSAGE;

            if (confirmationDialog != null)
                confirmationDialog.SetActive(true);

            Debug.Log("[SettingsResetAllDialog] Showing reset all confirmation");
        }

        public void Hide()
        {
            _onConfirmed = null;

            if (confirmationDialog != null)
                confirmationDialog.SetActive(false);
        }

        // ═══════════════════════════════════════════════════════════
        // ░ BUTTON HANDLERS
        // ═══════════════════════════════════════════════════════════

        private void OnYesClicked()
        {
            var callback = _onConfirmed;
            Hide();
            callback?.Invoke();
        }

        private void OnNoClicked()
        {
            Debug.Log("[SettingsResetAllDialog] Reset all cancelled");
            Hide();
        }
    }
}