// ════════════════════════════════════════════════════════════════════════
// Assets/Scripts/UI/Controllers/DisclaimerScreen.cs
// ════════════════════════════════════════════════════════════════════════

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using ChatSim.Core;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ChatSim.UI.Common.Screens
{
    /// <summary>
    /// CRITICAL: This scene loads FIRST (before Bootstrap)
    /// - No game systems are active yet
    /// - No managers initialized
    /// - Completely isolated
    /// 
    /// Attach to: 00_Disclaimer → Canvas
    /// Flow: 00_Disclaimer → 01_Bootstrap
    /// </summary>
    public class DisclaimerScreen : MonoBehaviour
    {
        #region Serialized Fields

        [Header("Panels")]
        [SerializeField] private GameObject disclaimerPanel;
        [SerializeField] private GameObject tosPanel;

        [Header("UI References")]
        [SerializeField] private Toggle checkBoxToggle;
        [SerializeField] private Button agreeButton;
        [SerializeField] private Button exitButton;
        [SerializeField] private Button tosButton;
        [SerializeField] private Button tosBackButton;

        [Header("Debug Settings")]
        [SerializeField] private bool skipForTesting = false;
        [SerializeField] private bool enableDebugLogs = true;

        #endregion

        #region Constants

        private const string DISCLAIMER_KEY = "HasSeenDisclaimer";

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            if (!ValidateReferences())
            {
                Debug.LogError("[Disclaimer] Missing UI references!");
                return;
            }

            // Already accepted → skip entirely
            if (skipForTesting || HasAcceptedDisclaimer())
            {
                Log("Disclaimer already accepted - proceeding to Bootstrap");
                LoadBootstrap();
                return;
            }

            InitializeUI();
        }

        private void Start()
        {
            if (!skipForTesting && !HasAcceptedDisclaimer())
            {
                RegisterListeners();
                Log("Disclaimer ready - awaiting user input");
            }
        }

#if UNITY_EDITOR
        private void Update()
        {
            // F10: Force accept and continue
            if (Input.GetKeyDown(KeyCode.F10))
            {
                MarkAccepted();
                LoadBootstrap();
            }

            // F9: Reset disclaimer
            if (Input.GetKeyDown(KeyCode.F9))
            {
                ResetDisclaimer();
            }
        }
#endif

        #endregion

        #region Setup

        private bool ValidateReferences()
        {
            return disclaimerPanel != null
                && tosPanel != null
                && checkBoxToggle != null
                && agreeButton != null
                && exitButton != null
                && tosButton != null
                && tosBackButton != null;
        }

        private void InitializeUI()
        {
            checkBoxToggle.isOn = false;
            agreeButton.interactable = false;

            tosPanel.SetActive(false);
            disclaimerPanel.SetActive(true);
        }

        private void RegisterListeners()
        {
            checkBoxToggle.onValueChanged.AddListener(OnToggleChanged);
            agreeButton.onClick.AddListener(OnContinue);
            exitButton.onClick.AddListener(OnExit);
            tosButton.onClick.AddListener(OpenTOS);
            tosBackButton.onClick.AddListener(BackToDisclaimer);
        }

        #endregion

        #region UI Callbacks

        private void OnToggleChanged(bool isOn)
        {
            agreeButton.interactable = isOn;
            Log($"Agreement toggle: {(isOn ? "accepted" : "declined")}");
        }

        private void OnContinue()
        {
            if (!checkBoxToggle.isOn)
            {
                Log("Cannot continue without accepting disclaimer");
                return;
            }

            MarkAccepted();
            Log("Disclaimer accepted - loading Bootstrap");
            LoadBootstrap();
        }

        private void OnExit()
        {
            Log("User declined - exiting");

#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void OpenTOS()
        {
            tosPanel.SetActive(true);
            disclaimerPanel.SetActive(false);
            Log("Opened Terms of Service panel");
        }

        private void BackToDisclaimer()
        {
            tosPanel.SetActive(false);
            disclaimerPanel.SetActive(true);
            Log("Returned to Disclaimer panel");
        }

        #endregion

        #region Scene Loading

        private void LoadBootstrap()
        {
            Log("Loading Bootstrap scene...");
            SceneManager.LoadScene(SceneNames.BOOTSTRAP);
        }

        #endregion

        #region Disclaimer State (PlayerPrefs)

        private static bool HasAcceptedDisclaimer()
        {
            return PlayerPrefs.GetInt(DISCLAIMER_KEY, 0) == 1;
        }

        private static void MarkAccepted()
        {
            PlayerPrefs.SetInt(DISCLAIMER_KEY, 1);
            PlayerPrefs.Save();
            Debug.Log("[Disclaimer] Acceptance saved");
        }

        #endregion

        #region Debug Tools

#if UNITY_EDITOR
        [ContextMenu("Reset Disclaimer")]
        private void ResetDisclaimer()
        {
            PlayerPrefs.DeleteKey(DISCLAIMER_KEY);
            PlayerPrefs.Save();
            Debug.Log("[Disclaimer] Reset - will show on next launch");
        }
#endif

        #endregion

        #region Logging

        private void Log(string message)
        {
            if (enableDebugLogs)
                Debug.Log($"[Disclaimer] {message}");
        }

        #endregion
    }
}
