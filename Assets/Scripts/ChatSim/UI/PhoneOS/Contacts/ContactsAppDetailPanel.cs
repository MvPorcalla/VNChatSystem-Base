// ════════════════════════════════════════════════════════════════════════
// Assets/Scripts/UI/PhoneOS/Contacts/ContactsAppDetailPanel.cs
// Phone Chat Simulation Game - Contact Detail Panel
// ════════════════════════════════════════════════════════════════════════

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using BubbleSpinner.Data;

namespace ChatSim.UI.PhoneOS.Contacts
{
    /// <summary>
    /// Shows full character info when a contact is tapped.
    /// Pulls all data from ConversationAsset. Shows "N/A" for missing fields.
    /// Also owns the Reset Story button — calls back to ContactsAppItem.RequestReset().
    ///
    /// Attach to: ContactsAppDetailPanel GameObject (child of ContactsPanel)
    ///
    /// Hierarchy:
    ///   ContactsAppDetailPanel              ← ATTACH THIS SCRIPT (starts inactive)
    ///   ├── Overlay                         ← Image, black ~50% alpha, Raycast Target ON
    ///   └── DetailCard                      ← Image, card background
    ///       ├── CloseButton                 ← Button — hides this panel
    ///       ├── ProfileImage                ← Image component
    ///       ├── NameText                    ← TextMeshProUGUI
    ///       ├── InfoGroup                   ← Vertical layout
    ///       │   ├── AgeText                 ← TextMeshProUGUI   (TODO: wire to ConversationAsset.age)
    ///       │   ├── BirthdateText           ← TextMeshProUGUI   (TODO: wire to ConversationAsset.birthdate)
    ///       │   ├── BioText                 ← TextMeshProUGUI   (TODO: wire to ConversationAsset.bio)
    ///       │   └── DescriptionText         ← TextMeshProUGUI   (TODO: wire to ConversationAsset.description)
    ///       └── ResetButton                 ← Button — calls ContactsAppItem.RequestReset()
    ///           └── Text                    ← TextMeshProUGUI "Reset Story"
    ///
    /// TODO: Add these fields to ConversationAsset.cs when ready:
    ///
    ///   [Header("Character Profile")]
    ///   public string age = "";
    ///   public string birthdate = "";
    ///   [TextArea(2, 4)] public string bio = "";
    ///   [TextArea(2, 4)] public string description = "";
    ///
    ///   Then replace the N/A placeholder lines below with:
    ///   ageText.text       = string.IsNullOrWhiteSpace(asset.age)         ? NA : asset.age;
    ///   birthdateText.text = string.IsNullOrWhiteSpace(asset.birthdate)   ? NA : asset.birthdate;
    ///   bioText.text       = string.IsNullOrWhiteSpace(asset.bio)         ? NA : asset.bio;
    ///   descriptionText.text = string.IsNullOrWhiteSpace(asset.description) ? NA : asset.description;
    /// </summary>
    public class ContactsAppDetailPanel : MonoBehaviour
    {
        #region Inspector References

        [Header("UI Elements")]
        [SerializeField] private Button closeButton;
        [SerializeField] private Image profileImage;
        [SerializeField] private TextMeshProUGUI nameText;

        [Header("Info Fields")]
        [SerializeField] private TextMeshProUGUI ageText;
        [SerializeField] private TextMeshProUGUI birthdateText;
        [SerializeField] private TextMeshProUGUI bioText;
        [SerializeField] private TextMeshProUGUI descriptionText;

        [Header("Reset")]
        [SerializeField] private Button resetButton;

        #endregion

        #region Constants

        private const string NA = "N/A";

        #endregion

        #region State

        private ContactsAppItem _caller;

        #endregion

        #region Unity Lifecycle

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
            if (closeButton != null)
            {
                closeButton.onClick.RemoveAllListeners();
                closeButton.onClick.AddListener(Hide);
            }
            else
            {
                Debug.LogError("[ContactsAppDetailPanel] closeButton is not assigned!");
            }

            if (resetButton != null)
            {
                resetButton.onClick.RemoveAllListeners();
                resetButton.onClick.AddListener(OnResetClicked);
            }
            else
            {
                Debug.LogError("[ContactsAppDetailPanel] resetButton is not assigned!");
            }
        }

        #endregion

        #region Public API

        /// <summary>
        /// Show the detail panel for a character.
        /// Called by ContactsAppItem when tapped.
        /// </summary>
        /// <param name="asset">Character data source</param>
        /// <param name="caller">Item that opened this panel — used for reset callback</param>
        public void Show(ConversationAsset asset, ContactsAppItem caller)
        {
            _caller = caller;

            PopulateInfo(asset);

            gameObject.SetActive(true);

            Debug.Log($"[ContactsAppDetailPanel] Showing detail for: {asset.characterName}");
        }

        /// <summary>
        /// Hide the detail panel.
        /// </summary>
        public void Hide()
        {
            _caller = null;
            gameObject.SetActive(false);
        }

        #endregion

        #region Population

        private void PopulateInfo(ConversationAsset asset)
        {
            // Name
            if (nameText != null)
                nameText.text = asset.characterName;

            // Profile image
            // TODO: Uncomment when profile images are ready in Addressables
            // if (profileImage != null && asset.profileImage != null && asset.profileImage.RuntimeKeyIsValid())
            // {
            //     var handle = asset.profileImage.LoadAssetAsync<Sprite>();
            //     handle.Completed += h =>
            //     {
            //         if (profileImage != null && h.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
            //             profileImage.sprite = h.Result;
            //     };
            // }

            // TODO: Replace all NA placeholders below once fields are added to ConversationAsset.cs
            // See header comment in this file for full instructions.

            if (ageText != null)
                ageText.text = NA;          // TODO: asset.age

            if (birthdateText != null)
                birthdateText.text = NA;    // TODO: asset.birthdate

            if (bioText != null)
                bioText.text = NA;          // TODO: asset.bio

            if (descriptionText != null)
                descriptionText.text = NA;  // TODO: asset.description
        }

        #endregion

        #region Reset Button

        private void OnResetClicked()
        {
            if (_caller == null)
            {
                Debug.LogError("[ContactsAppDetailPanel] Reset clicked but caller is null!");
                return;
            }

            // Hide this panel first, then route through the item's reset logic
            // (item will show confirmation dialog if enabled)
            Hide();
            _caller.RequestReset();
        }

        #endregion
    }
}