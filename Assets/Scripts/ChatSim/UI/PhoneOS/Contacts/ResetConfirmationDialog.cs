// ════════════════════════════════════════════════════════════════════════
// Assets/Scripts/UI/PhoneOS/Contacts/ResetConfirmationDialog.cs
// Phone Chat Simulation Game - Reset Story Confirmation Dialog
// ════════════════════════════════════════════════════════════════════════

using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ChatSim.UI.PhoneOS.Contacts
{
    /// <summary>
    /// Pure UI middleman for the "Are you sure?" reset confirmation.
    /// Does NOT own any reset logic — it simply calls back to the
    /// ContactsAppItem that opened it via ExecuteReset().
    ///
    /// To bypass this dialog entirely, untick useConfirmationDialog
    /// on ContactsAppPanel in the Inspector.
    ///
    /// Attach to: ResetConfirmationDialog GameObject (child of ContactsPanel)
    ///
    /// Hierarchy suggestion:
    /// ResetConfirmationDialog             ← ATTACH [ResetConfirmationDialog.cs] (Do not Put this panel Inactive) (active in scene)
    /// └── ConfirmationDialog
    ///     └── ContentPanel   
    ///         ├── TitleText
    ///         ├── MessageText 
    ///         ├── CancelButton
    ///         │   └── Text
    ///         └── ResetButton 
    ///             └── Text    
    /// </summary>
    public class ResetConfirmationDialog : MonoBehaviour
    {
        #region Inspector References

        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI messageText;
        [SerializeField] private Button yesButton;
        [SerializeField] private Button noButton;

        #endregion

        #region Constants

        private const string TITLE_FORMAT = "Reset {0}?";
        private const string MESSAGE_TEXT = "This will erase all chat history and progress for this character. This cannot be undone.";

        #endregion

        #region State

        // Reference back to the item that opened this dialog
        // Dialog calls item.ExecuteReset() on confirm — owns no logic itself
        private ContactsAppItem _caller;

        #endregion

        #region Unity Lifecycle

// AFTER
private void Awake()
{
    gameObject.SetActive(false);
}

private void Start()
{
    SetupButtons();
}

        #endregion

        #region Setup

        private void SetupButtons()
        {
            if (yesButton != null)
            {
                yesButton.onClick.RemoveAllListeners();
                yesButton.onClick.AddListener(OnYesClicked);
            }
            else
            {
                Debug.LogError("[ResetConfirmationDialog] yesButton is not assigned!");
            }

            if (noButton != null)
            {
                noButton.onClick.RemoveAllListeners();
                noButton.onClick.AddListener(OnNoClicked);
            }
            else
            {
                Debug.LogError("[ResetConfirmationDialog] noButton is not assigned!");
            }
        }

        #endregion

        #region Public API

        /// <summary>
        /// Show the dialog for a specific character.
        /// Stores the caller so we can invoke ExecuteReset() on confirmation.
        /// Called by ContactsAppItem.OnResetButtonClicked().
        /// </summary>
        /// <param name="characterName">Shown in the dialog title</param>
        /// <param name="conversationId">Passed for logging only — logic stays in the caller</param>
        /// <param name="caller">The ContactsAppItem that owns the reset logic</param>
        public void Show(string characterName, string conversationId, ContactsAppItem caller)
        {
            _caller = caller;

            if (titleText != null)
                titleText.text = string.Format(TITLE_FORMAT, characterName);

            if (messageText != null)
                messageText.text = MESSAGE_TEXT;

            gameObject.SetActive(true);

            Debug.Log($"[ResetConfirmationDialog] Showing for: {characterName} ({conversationId})");
        }

        /// <summary>
        /// Hide the dialog without taking any action.
        /// </summary>
        public void Hide()
        {
            _caller = null;
            gameObject.SetActive(false);
        }

        #endregion

        #region Button Handlers

        private void OnYesClicked()
        {
            if (_caller == null)
            {
                Debug.LogError("[ResetConfirmationDialog] Yes clicked but caller is null!");
                Hide();
                return;
            }

            // Delegate all logic back to the item — dialog is UI only
            _caller.ExecuteReset();
            Hide();
        }

        private void OnNoClicked()
        {
            Debug.Log("[ResetConfirmationDialog] Reset cancelled.");
            Hide();
        }

        #endregion
    }
}