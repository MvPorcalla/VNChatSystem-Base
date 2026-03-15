// ════════════════════════════════════════════════════════════════════════
// Assets/Scripts/ChatSim/UI/HomeScreen/Settings/SettingsPanel.cs
// ════════════════════════════════════════════════════════════════════════

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ChatSim.Core;

namespace ChatSim.UI.HomeScreen.Settings
{
    /// <summary>
    /// Settings panel — Gameplay, Data, and About sections.
    /// Attach to: SettingsPanel GameObject (child of Screens in 03_PhoneScreen)
    ///
    /// Hierarchy:
    ///   SettingsPanel                       ← ATTACH THIS SCRIPT — INACTIVE by default
    ///   └── ScrollView
    ///       └── Viewport
    ///           └── Content
    ///               ├── Section_Gameplay
    ///               │   ├── SectionHeader   (TMP — "Gameplay")
    ///               │   ├── MessageSpeed
    ///               │   │   ├── Label       (TMP — "Message Speed")
    ///               │   │   └── SpeedButton (Button)
    ///               │   │       ├── Icon    (Image)
    ///               │   │       └── StateText (TMP — "Normal" / "Fast")
    ///               │   └── TextSize
    ///               │       ├── Label       (TMP — "Text Size")
    ///               │       ├── SmallButton (Button)
    ///               │       ├── MediumButton(Button)
    ///               │       └── LargeButton (Button)
    ///               ├── Section_Data
    ///               │   ├── SectionHeader   (TMP — "Data")
    ///               │   └── ResetAllButton  (Button)
    ///               └── Section_About
    ///                   ├── SectionHeader   (TMP — "About")
    ///                   └── VersionText     (TMP)
    /// </summary>
    public class SettingsPanel : MonoBehaviour
    {
        // ═══════════════════════════════════════════════════════════
        // ░ INSPECTOR REFERENCES — GAMEPLAY
        // ═══════════════════════════════════════════════════════════

        [Header("Gameplay — Message Speed")]
        [SerializeField] private Button messageSpeedButton;
        [SerializeField] private TextMeshProUGUI messageSpeedLabel;
        [SerializeField] private Image messageSpeedIcon;
        [SerializeField] private Sprite normalModeSprite;
        [SerializeField] private Sprite fastModeSprite;

        [Header("Gameplay — Text Size")]
        [SerializeField] private Button smallTextButton;
        [SerializeField] private Button mediumTextButton;
        [SerializeField] private Button largeTextButton;

        // ═══════════════════════════════════════════════════════════
        // ░ INSPECTOR REFERENCES — DATA
        // ═══════════════════════════════════════════════════════════

        [Header("Data")]
        [SerializeField] private Button resetAllButton;
        [SerializeField] private SettingsResetAllDialog resetAllDialog;

        // ═══════════════════════════════════════════════════════════
        // ░ INSPECTOR REFERENCES — ABOUT
        // ═══════════════════════════════════════════════════════════

        [Header("About")]
        [SerializeField] private TextMeshProUGUI versionText;

        // ═══════════════════════════════════════════════════════════
        // ░ CONSTANTS
        // ═══════════════════════════════════════════════════════════

        private const string FAST_MODE_PREF_KEY = "ChatFastMode";
        private const string TEXT_SIZE_PREF_KEY = "ChatTextSize";

        private const float TEXT_SIZE_SMALL  = 36f;
        private const float TEXT_SIZE_MEDIUM = 42f;
        private const float TEXT_SIZE_LARGE  = 48f;

        // ═══════════════════════════════════════════════════════════
        // ░ STATE
        // ═══════════════════════════════════════════════════════════

        private float currentTextSize;
        private bool isFastMode = false;

        // ═══════════════════════════════════════════════════════════
        // ░ UNITY LIFECYCLE
        // ═══════════════════════════════════════════════════════════

        private void Awake()
        {
            SetupButtons();
        }

        private void OnEnable()
        {
            LoadAndApplySettings();
        }

        // ═══════════════════════════════════════════════════════════
        // ░ INITIALIZATION
        // ═══════════════════════════════════════════════════════════

        private void SetupButtons()
        {
            // Message speed button
            if (messageSpeedButton != null)
                messageSpeedButton.onClick.AddListener(OnMessageSpeedButtonClicked);
            else
                Debug.LogWarning("[SettingsPanel] messageSpeedButton not assigned!");

            // Text size buttons
            smallTextButton?.onClick.AddListener(() => OnTextSizeSelected(TEXT_SIZE_SMALL));
            mediumTextButton?.onClick.AddListener(() => OnTextSizeSelected(TEXT_SIZE_MEDIUM));
            largeTextButton?.onClick.AddListener(() => OnTextSizeSelected(TEXT_SIZE_LARGE));

            // Reset all button
            if (resetAllButton != null)
                resetAllButton.onClick.AddListener(OnResetAllClicked);
            else
                Debug.LogWarning("[SettingsPanel] resetAllButton not assigned!");

            // Version text
            if (versionText != null)
                versionText.text = $"Version {Application.version}";
        }

        private void LoadAndApplySettings()
        {
            // Load and apply message speed
            isFastMode = PlayerPrefs.GetInt(FAST_MODE_PREF_KEY, 0) == 1;
            UpdateMessageSpeedVisuals();

            // Load and apply text size — default to Large
            currentTextSize = PlayerPrefs.GetFloat(TEXT_SIZE_PREF_KEY, TEXT_SIZE_LARGE);
            UpdateTextSizeButtonStates(currentTextSize);
        }

        // ═══════════════════════════════════════════════════════════
        // ░ GAMEPLAY — MESSAGE SPEED
        // ═══════════════════════════════════════════════════════════

        private void OnMessageSpeedButtonClicked()
        {
            isFastMode = !isFastMode;

            Debug.Log($"[SettingsPanel] Message speed: {(isFastMode ? "Fast" : "Normal")}");

            // Save preference
            PlayerPrefs.SetInt(FAST_MODE_PREF_KEY, isFastMode ? 1 : 0);
            PlayerPrefs.Save();

            // Update visuals
            UpdateMessageSpeedVisuals();

            // Notify other systems
            GameEvents.TriggerMessageSpeedChanged(isFastMode);
        }

        private void UpdateMessageSpeedVisuals()
        {
            if (messageSpeedLabel != null)
                messageSpeedLabel.text = isFastMode ? "Fast" : "Normal";

            if (messageSpeedIcon != null && normalModeSprite != null && fastModeSprite != null)
                messageSpeedIcon.sprite = isFastMode ? fastModeSprite : normalModeSprite;
        }

        // ═══════════════════════════════════════════════════════════
        // ░ GAMEPLAY — TEXT SIZE
        // ═══════════════════════════════════════════════════════════

        private void OnTextSizeSelected(float fontSize)
        {
            if (Mathf.Approximately(fontSize, currentTextSize)) return;

            currentTextSize = fontSize;

            Debug.Log($"[SettingsPanel] Text size: {fontSize}");

            // Save preference
            PlayerPrefs.SetFloat(TEXT_SIZE_PREF_KEY, fontSize);
            PlayerPrefs.Save();

            // Update button visual states
            UpdateTextSizeButtonStates(fontSize);

            // Notify other systems
            GameEvents.TriggerTextSizeChanged(fontSize);
        }

        private void UpdateTextSizeButtonStates(float selectedSize)
        {
            SetButtonAlpha(smallTextButton,  Mathf.Approximately(selectedSize, TEXT_SIZE_SMALL)  ? 1f : 0.4f);
            SetButtonAlpha(mediumTextButton, Mathf.Approximately(selectedSize, TEXT_SIZE_MEDIUM) ? 1f : 0.4f);
            SetButtonAlpha(largeTextButton,  Mathf.Approximately(selectedSize, TEXT_SIZE_LARGE)  ? 1f : 0.4f);
        }

        private void SetButtonAlpha(Button button, float alpha)
        {
            if (button == null) return;

            var canvasGroup = button.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = button.gameObject.AddComponent<CanvasGroup>();

            canvasGroup.alpha = alpha;
        }

        // ═══════════════════════════════════════════════════════════
        // ░ DATA — RESET ALL
        // ═══════════════════════════════════════════════════════════

        private void OnResetAllClicked()
        {
            Debug.Log("[SettingsPanel] Reset all clicked");

            if (resetAllDialog != null)
            {
                resetAllDialog.Show(OnResetAllConfirmed);
            }
            else
            {
                Debug.LogWarning("[SettingsPanel] resetAllDialog not assigned — resetting directly");
                OnResetAllConfirmed();
            }
        }

        private void OnResetAllConfirmed()
        {
            Debug.Log("[SettingsPanel] Reset all confirmed");

            if (GameBootstrap.Save == null)
            {
                Debug.LogError("[SettingsPanel] GameBootstrap.Save is null!");
                return;
            }

            if (GameBootstrap.Conversation == null)
            {
                Debug.LogError("[SettingsPanel] GameBootstrap.Conversation is null!");
                return;
            }

            // Wipe disk
            GameBootstrap.Save.ResetAllData();

            // Evict all in-memory session caches so next conversation load starts fresh
            GameBootstrap.Conversation.EvictAllConversationCaches();
        }
    }
}