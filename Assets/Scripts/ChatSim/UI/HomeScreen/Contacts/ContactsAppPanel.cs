// ════════════════════════════════════════════════════════════════════════
// Assets/Scripts/UI/HomeScreen/Contacts/ContactsAppPanel.cs
// Phone Chat Simulation Game - Contacts App Panel (03_PhoneScreen)
// ════════════════════════════════════════════════════════════════════════

using System.Collections.Generic;
using UnityEngine;
using BubbleSpinner.Data;
using ChatSim.Core;
using ChatSim.UI.Overlay.Dialogs;

namespace ChatSim.UI.HomeScreen.Contacts
{
    /// <summary>
    /// Contacts App panel in 03_PhoneScreen > PhoneRoot > Screens.
    /// Spawns ContactsAppItem buttons from CharacterDatabase.
    /// 
    /// TODO: Wire up ContactsAppDetailPanel once it is ready:
    ///       - Add: [SerializeField] private ContactsAppDetailPanel detailPanel;
    ///       - Pass it into item.Initialize() alongside the other params
    ///
    /// Toggle useConfirmationDialog to enable/disable the "Are you sure?" popup.
    /// Attach to: ContactsPanel GameObject
    /// </summary>
    public class ContactsAppPanel : MonoBehaviour
    {
        #region Inspector References

        [Header("Database")]
        [SerializeField] private CharacterDatabase characterDatabase;

        [Header("UI References")]
        [SerializeField] private Transform contactContainer;
        [SerializeField] private GameObject contactsAppItemPrefab;

        // TODO: Uncomment when ContactsAppDetailPanel is ready
        // [Header("Detail Panel")]
        // [SerializeField] private ContactsAppDetailPanel detailPanel;

        [Header("Dialog")]
        [Tooltip("Uncheck to skip the confirmation dialog — reset will act immediately")]
        [SerializeField] private bool useConfirmationDialog = true;
        [SerializeField] private ResetConfirmationDialog resetConfirmationDialog;

        #endregion

        #region Unity Lifecycle

        private void Start()
        {
            ValidateReferences();
        }

        private void OnEnable()
        {
            PopulateContacts();
        }

        #endregion

        #region Validation

        private void ValidateReferences()
        {
            if (characterDatabase == null)
                LogError("CharacterDatabase is not assigned!");

            if (contactContainer == null)
                LogError("contactContainer is not assigned!");

            if (contactsAppItemPrefab == null)
                LogError("contactsAppItemPrefab is not assigned!");

            if (useConfirmationDialog && resetConfirmationDialog == null)
                LogWarning("useConfirmationDialog is ON but resetConfirmationDialog is not assigned — resets will act directly.");
        }

        #endregion

        #region Population

        private void PopulateContacts()
        {
            if (characterDatabase == null || contactContainer == null || contactsAppItemPrefab == null)
                return;

            ClearContacts();

            List<ConversationAsset> characters = characterDatabase.GetAllCharacters();

            if (characters == null || characters.Count == 0)
            {
                LogWarning("No characters found in CharacterDatabase.");
                return;
            }

            foreach (ConversationAsset character in characters)
            {
                if (character == null)
                {
                    LogWarning("Null entry in CharacterDatabase, skipping.");
                    continue;
                }

                SpawnContactItem(character);
            }

            Log($"Populated {characters.Count} contacts. Dialog: {useConfirmationDialog}");
        }

        private void SpawnContactItem(ConversationAsset character)
        {
            GameObject itemObj = Instantiate(contactsAppItemPrefab, contactContainer);

            ContactsAppItem item = itemObj.GetComponent<ContactsAppItem>();

            if (item != null)
            {
                item.Initialize(
                    asset: character,
                    dialog: resetConfirmationDialog,
                    useDialog: useConfirmationDialog
                    // TODO: Pass detailPanel here once ContactsAppDetailPanel is ready
                );
            }
            else
            {
                LogError("ContactsAppItem component missing on prefab!");
            }
        }

        private void ClearContacts()
        {
            foreach (Transform child in contactContainer)
            {
                Destroy(child.gameObject);
            }
        }

        #endregion

        #region Public API

        public void RefreshContacts()
        {
            PopulateContacts();
        }

        #endregion

        #region Logging

        [System.Diagnostics.Conditional("UNITY_EDITOR"), System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]
        private void Log(string message)
        {
            if (GameBootstrap.Config == null || !GameBootstrap.Config.contactsAppDebugLogs) return;
            UnityEngine.Debug.Log($"[ContactsAppPanel] {message}");
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR"), System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]
        private void LogWarning(string message)
        {
            if (GameBootstrap.Config == null || !GameBootstrap.Config.contactsAppDebugLogs) return;
            UnityEngine.Debug.LogWarning($"[ContactsAppPanel] WARNING: {message}");
        }

        private void LogError(string message)
        {
            UnityEngine.Debug.LogError($"[ContactsAppPanel] ERROR: {message}");
        }

        #endregion
    }
}