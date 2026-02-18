// ════════════════════════════════════════════════════════════════════════
// Assets/Scripts/UI/PhoneOS/Contacts/ContactsAppItem.cs
// Phone Chat Simulation Game - Contacts App Item
// ════════════════════════════════════════════════════════════════════════

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using BubbleSpinner.Data;
using ChatSim.Core;

namespace ChatSim.UI.PhoneOS.Contacts
{
    /// <summary>
    /// Individual contact button in the Contacts App.
    /// Displays: profile picture, character name, and a reset story button.
    ///
    /// TODO: Clicking the item (itemButton) opens ContactsAppDetailPanel with full character info.
    ///       - Wire up detailPanel reference in ContactsAppPanel.SpawnContactItem()
    ///       - Pass detail panel into Initialize()
    ///       - Uncomment detailPanel field and OnItemClicked body below
    ///       - The detail panel will show all ConversationAsset fields (name, age,
    ///         birthdate, bio, description) with N/A fallback for missing fields
    ///
    /// LOGIC OWNER: This class owns ExecuteReset().
    /// ResetConfirmationDialog is an optional UI middleman.
    ///
    /// Attach to: ContactsAppItem prefab root
    ///
    /// Hierarchy:
    ///   ContactsAppItem              ← Button component (itemButton) + this script
    ///   ├── ProfileImage             ← Image component
    ///   ├── NameText                 ← TextMeshProUGUI
    ///   └── ResetButton              ← Button component (resetButton)
    ///       └── Text                 ← TextMeshProUGUI "Reset Story"
    /// </summary>
    public class ContactsAppItem : MonoBehaviour
    {
        #region Inspector References

        [Header("UI Elements")]
        [SerializeField] private Button itemButton;
        [SerializeField] private Image profileImage;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private Button resetButton;

        // TODO: Uncomment when ContactsAppDetailPanel is ready
        // [Header("Detail Panel Reference")]
        // [SerializeField] private ContactsAppDetailPanel detailPanel;

        #endregion

        #region State

        private ConversationAsset _conversationAsset;
        private ResetConfirmationDialog _confirmationDialog;
        private bool _useConfirmationDialog;
        private AsyncOperationHandle<Sprite> _imageLoadHandle;

        #endregion

        #region Initialization

        /// <summary>
        /// Initialize this contact button.
        /// Called by ContactsAppPanel when spawning items.
        /// </summary>
        /// <param name="asset">The character's ConversationAsset</param>
        /// <param name="dialog">Shared ResetConfirmationDialog — can be null</param>
        /// <param name="useDialog">If false, reset skips the dialog entirely</param>
        public void Initialize(ConversationAsset asset, ResetConfirmationDialog dialog, bool useDialog)
        {
            _conversationAsset = asset;
            _confirmationDialog = dialog;
            _useConfirmationDialog = useDialog;

            // TODO: Accept ContactsAppDetailPanel parameter once detail panel is ready
            // public void Initialize(ConversationAsset asset, ContactsAppDetailPanel detail, ResetConfirmationDialog dialog, bool useDialog)
            // detailPanel = detail;

            SetupName(asset);
            SetupProfileImage(asset);
            SetupItemButton();
            SetupResetButton();
        }

        private void SetupName(ConversationAsset asset)
        {
            if (nameText != null)
                nameText.text = asset.characterName;
            else
                Debug.LogWarning($"[ContactsAppItem] nameText not assigned on {gameObject.name}");
        }

        private void SetupProfileImage(ConversationAsset asset)
        {
            // TODO: Uncomment when profile images are ready in Addressables

            // if (profileImage == null)
            // {
            //     Debug.LogWarning($"[ContactsAppItem] profileImage not assigned on {gameObject.name}");
            //     return;
            // }

            // if (asset.profileImage != null && asset.profileImage.RuntimeKeyIsValid())
            // {
            //     _imageLoadHandle = asset.profileImage.LoadAssetAsync<Sprite>();
            //     _imageLoadHandle.Completed += OnProfileImageLoaded;
            // }
            // else
            // {
            //     Debug.LogWarning($"[ContactsAppItem] No valid profile image for {asset.characterName}");
            // }
        }

        private void SetupItemButton()
        {
            if (itemButton == null)
            {
                Debug.LogError($"[ContactsAppItem] itemButton not assigned on {gameObject.name}");
                return;
            }

            itemButton.onClick.RemoveAllListeners();
            itemButton.onClick.AddListener(OnItemClicked);
        }

        private void SetupResetButton()
        {
            if (resetButton == null)
            {
                Debug.LogError($"[ContactsAppItem] resetButton not assigned on {gameObject.name}");
                return;
            }

            // RemoveAllListeners clears any stale Inspector onClick wiring
            resetButton.onClick.RemoveAllListeners();
            resetButton.onClick.AddListener(RequestReset);
        }

        #endregion

        #region Image Loading

        private void OnProfileImageLoaded(AsyncOperationHandle<Sprite> handle)
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                if (profileImage != null)
                    profileImage.sprite = handle.Result;
            }
            else
            {
                Debug.LogError($"[ContactsAppItem] Failed to load profile image for {_conversationAsset?.characterName}");
            }
        }

        #endregion

        #region Item Button

        private void OnItemClicked()
        {
            // TODO: Open ContactsAppDetailPanel with full character info
            // Uncomment once ContactsAppDetailPanel is ready:
            //
            // if (_conversationAsset == null)
            // {
            //     Debug.LogError("[ContactsAppItem] Cannot open detail: ConversationAsset is null!");
            //     return;
            // }
            // if (detailPanel == null)
            // {
            //     Debug.LogError("[ContactsAppItem] detailPanel reference is missing!");
            //     return;
            // }
            // detailPanel.Show(_conversationAsset, this);

            Debug.Log($"[ContactsAppItem] TODO: Open detail panel for {_conversationAsset?.characterName}");
        }

        #endregion

        #region Reset Logic

        /// <summary>
        /// Entry point for reset — wired to resetButton.onClick via SetupResetButton().
        /// Routes through confirmation dialog if enabled, otherwise resets directly.
        /// </summary>
        public void RequestReset()
        {
            if (_conversationAsset == null)
            {
                Debug.LogError("[ContactsAppItem] Cannot reset: ConversationAsset is null!");
                return;
            }

            if (_useConfirmationDialog && _confirmationDialog != null)
            {
                _confirmationDialog.Show(
                    characterName: _conversationAsset.characterName,
                    conversationId: _conversationAsset.ConversationId,
                    caller: this
                );
                return;
            }

            ExecuteReset();
        }

        /// <summary>
        /// Performs the actual story reset for this character.
        /// Called directly by RequestReset() (no dialog),
        /// OR called by ResetConfirmationDialog after the player confirms Yes.
        /// </summary>
        public void ExecuteReset()
        {
            if (_conversationAsset == null)
            {
                Debug.LogError("[ContactsAppItem] ExecuteReset: ConversationAsset is null!");
                return;
            }

            if (GameBootstrap.Save == null)
            {
                Debug.LogError("[ContactsAppItem] ExecuteReset: GameBootstrap.Save is null!");
                return;
            }

            Debug.Log($"[ContactsAppItem] Executing story reset for: {_conversationAsset.characterName}");
            GameBootstrap.Save.ResetCharacterStory(_conversationAsset.ConversationId);
        }

        #endregion

        #region Cleanup

        private void OnDestroy()
        {
            // TODO: Uncomment when profile image loading is enabled
            // if (_imageLoadHandle.IsValid())
            //     Addressables.Release(_imageLoadHandle);
        }

        #endregion
    }
}