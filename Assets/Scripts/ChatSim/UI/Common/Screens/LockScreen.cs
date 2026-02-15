// ════════════════════════════════════════════════════════════════════════
// Assets/Scripts/UI/LockScreen.cs
// ════════════════════════════════════════════════════════════════════════

using UnityEngine;
using UnityEngine.UI;
using ChatSim.Core;

namespace ChatSim.UI.Common.Screens
{
    /// <summary>
    /// Manages the lock screen (02_LockScreen scene)
    /// Entry point for the game - always shown first
    /// </summary>
    public class LockScreen : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Button unlockButton;
        
        [Header("Optional: Lock Screen Info")]
        [SerializeField] private TMPro.TextMeshProUGUI timeText;
        [SerializeField] private TMPro.TextMeshProUGUI dateText;
        
        private void Start()
        {
            InitializeUI();
            UpdateLockScreenInfo();
            
            GameEvents.TriggerPhoneLocked();
            Debug.Log("[LockScreen] Lock screen ready");
        }

        private void InitializeUI()
        {
            if (unlockButton != null)
            {
                unlockButton.onClick.AddListener(OnUnlockPressed);
            }
            else
            {
                Debug.LogWarning("[LockScreen] Unlock button not assigned!");
            }
        }

        private void UpdateLockScreenInfo()
        {
            // Optional: Display current time/date
            if (timeText != null)
            {
                timeText.text = System.DateTime.Now.ToString("HH:mm");
            }
            
            if (dateText != null)
            {
                dateText.text = System.DateTime.Now.ToString("dddd, MMMM dd");
            }
        }

        private void OnUnlockPressed()
        {
            Debug.Log("[LockScreen] Unlock button pressed");
            
            GameEvents.TriggerPhoneUnlocked();
            
            if (GameBootstrap.SceneFlow != null)
            {
                GameBootstrap.SceneFlow.GoToPhoneScreen();
            }
            else
            {
                Debug.LogError("[LockScreen] GameBootstrap.SceneFlow not found!");
            }
        }
    }
}