// ════════════════════════════════════════════════════════════════════════
// Assets/Scripts/UI/Screens/LockScreen.cs
// ════════════════════════════════════════════════════════════════════════

using System.Collections.Generic;
using UnityEngine;
using TMPro;
using ChatSim.Core;
using ChatSim.Data;
using BubbleSpinner.Data;

namespace ChatSim.UI.Screens
{
    /// <summary>
    /// Manages the lock screen (02_LockScreen scene).
    /// Loaded after Bootstrap — GameBootstrap.SceneFlow is guaranteed available.
    ///
    /// Unlock: swipe up anywhere on screen.
    /// Content fades out as player swipes up, fades back in if they release early.
    ///
    /// Flow: 02_LockScreen → 03_PhoneScreen
    /// </summary>
    public class LockScreen : MonoBehaviour
    {
        // ─────────────────────────────────────────────
        // UI References
        // ─────────────────────────────────────────────

        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI timeText;
        [SerializeField] private TextMeshProUGUI dateText;
        [SerializeField] private CanvasGroup contentGroup;

        [Header("Notifications")]
        [SerializeField] private GameObject notificationContainer;
        [SerializeField] private GameObject notificationItemPrefab;
        [SerializeField] private GameObject moreIndicator;

        // ─────────────────────────────────────────────
        // Private State
        // ─────────────────────────────────────────────

        private Vector2 _touchStartPos;
        private bool _isTouching = false;
        private bool _isUnlocking = false;

        // ─────────────────────────────────────────────
        // Config Accessors (fallback to defaults if config missing)
        // ─────────────────────────────────────────────

        private float SwipeThreshold => GameBootstrap.Config != null ? GameBootstrap.Config.swipeThreshold : 300f;
        private float FadeSwipeRange => GameBootstrap.Config != null ? GameBootstrap.Config.fadeSwipeRange : 400f;
        private int MaxNotifications => GameBootstrap.Config != null ? GameBootstrap.Config.maxIndividualNotifications : 3;
        private string TimeFormat => GameBootstrap.Config != null ? GameBootstrap.Config.GetTimeFormatString() : "HH:mm";
        private string DateFormat => GameBootstrap.Config != null ? GameBootstrap.Config.GetDateFormatString() : "dddd, MMMM dd";
        private bool DebugLogs => GameBootstrap.Config != null ? GameBootstrap.Config.lockScreenDebugLogs : true;

        // ─────────────────────────────────────────────
        // Unity Lifecycle
        // ─────────────────────────────────────────────

        private void Start()
        {
            if (GameBootstrap.Config == null)
                Debug.LogWarning("[LockScreen] No config found — using defaults");

            UpdateTimeDate();

            if (contentGroup != null)
                contentGroup.alpha = 1f;

            PopulateNotifications();

            GameEvents.TriggerPhoneLocked();
            Log("Lock screen ready — swipe up to unlock");
        }

        private void Update()
        {
            UpdateTimeDate();

            if (_isUnlocking) return;

            HandleSwipeInput();
        }

        // ─────────────────────────────────────────────
        // Time & Date
        // ─────────────────────────────────────────────

        private void UpdateTimeDate()
        {
            if (timeText != null)
                timeText.text = System.DateTime.Now.ToString(TimeFormat);

            if (dateText != null)
                dateText.text = System.DateTime.Now.ToString(DateFormat);
        }

        // ─────────────────────────────────────────────
        // Notifications
        // ─────────────────────────────────────────────

        private void PopulateNotifications()
        {
            if (notificationContainer == null || notificationItemPrefab == null)
            {
                Log("Notification refs not assigned — skipping");
                return;
            }

            // Clear existing
            foreach (Transform child in notificationContainer.transform)
            {
                if (child.gameObject != moreIndicator)
                    Destroy(child.gameObject);
            }

            if (moreIndicator != null)
                moreIndicator.SetActive(false);

            if (MaxNotifications == 0) return;

            if (GameBootstrap.Save == null)
            {
                LogWarning("SaveManager not ready — skipping notifications");
                return;
            }

            SaveData saveData = GameBootstrap.Save.GetOrCreateSaveData();
            if (saveData == null) return;

            List<ConversationState> unread = GetUnreadConversations(saveData);

            if (unread.Count == 0)
            {
                Log("No unread conversations");
                return;
            }

            Log($"Unread conversations: {unread.Count}");

            int individualCount = Mathf.Min(unread.Count, MaxNotifications);
            int remainingCount  = unread.Count - individualCount;

            for (int i = 0; i < individualCount; i++)
                SpawnNotificationItem(unread[i]);

            if (remainingCount > 0)
                ShowMoreIndicator();
        }

        private List<ConversationState> GetUnreadConversations(SaveData saveData)
        {
            var unread = new List<ConversationState>();

            if (saveData.conversationStates == null) return unread;

            foreach (var state in saveData.conversationStates)
            {
                if (state == null) continue;
                if (state.messageHistory == null || state.messageHistory.Count == 0) continue;

                bool isPending = state.resumeTarget == ResumeTarget.Pause
                              || state.resumeTarget == ResumeTarget.Interrupted;

                if (isPending)
                    unread.Add(state);
            }

            return unread;
        }

        private void SpawnNotificationItem(ConversationState state)
        {
            GameObject item = Instantiate(notificationItemPrefab, notificationContainer.transform);

            TextMeshProUGUI senderText  = FindTMP(item, "SenderText");
            TextMeshProUGUI previewText = FindTMP(item, "PreviewText");

            if (senderText != null)
                senderText.text = state.characterName;

            if (previewText != null)
                previewText.text = GetLastNpcMessage(state);
        }

        private void ShowMoreIndicator()
        {
            if (moreIndicator != null)
            {
                moreIndicator.SetActive(true);
                moreIndicator.transform.SetAsLastSibling();
            }
        }

        private string GetLastNpcMessage(ConversationState state)
        {
            if (state.messageHistory == null) return "New message";

            for (int i = state.messageHistory.Count - 1; i >= 0; i--)
            {
                var msg = state.messageHistory[i];
                if (msg == null) continue;
                if (msg.IsSystemMessage) continue;

                if (msg.type == MessageData.MessageType.Image)
                    return msg.IsPlayerMessage ? "You sent an image." : "Sent an image.";

                if (msg.type == MessageData.MessageType.Text && !string.IsNullOrEmpty(msg.content))
                    return msg.content;
            }

            return "New message";
        }

        private TextMeshProUGUI FindTMP(GameObject root, string childName)
        {
            Transform found = root.transform.Find(childName);
            if (found != null) return found.GetComponent<TextMeshProUGUI>();

            LogWarning($"Could not find '{childName}' in notification prefab");
            return null;
        }

        // ─────────────────────────────────────────────
        // Swipe Input
        // ─────────────────────────────────────────────

        private void HandleSwipeInput()
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            if (Input.GetMouseButtonDown(0))
            {
                _touchStartPos = Input.mousePosition;
                _isTouching = true;
            }

            if (_isTouching)
            {
                float delta = ((Vector2)Input.mousePosition - _touchStartPos).y;
                UpdateFade(delta);

                if (Input.GetMouseButtonUp(0))
                {
                    Log($"Swipe delta: {delta}px (threshold: {SwipeThreshold}px)");

                    if (delta >= SwipeThreshold)
                        OnUnlockTriggered();
                    else
                        ResetFade();

                    _isTouching = false;
                }
            }
#else
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);

                if (touch.phase == TouchPhase.Began)
                {
                    _touchStartPos = touch.position;
                    _isTouching = true;
                }

                if (_isTouching)
                {
                    float delta = (touch.position - _touchStartPos).y;
                    UpdateFade(delta);

                    if (touch.phase == TouchPhase.Ended)
                    {
                        Log($"Swipe delta: {delta}px (threshold: {SwipeThreshold}px)");

                        if (delta >= SwipeThreshold)
                            OnUnlockTriggered();
                        else
                            ResetFade();

                        _isTouching = false;
                    }
                }
            }
#endif
        }

        // ─────────────────────────────────────────────
        // Fade
        // ─────────────────────────────────────────────

        private void UpdateFade(float swipeDelta)
        {
            if (contentGroup == null) return;

            float t = Mathf.Clamp01(swipeDelta / FadeSwipeRange);
            contentGroup.alpha = 1f - t;
        }

        private void ResetFade()
        {
            if (contentGroup == null) return;
            contentGroup.alpha = 1f;
            Log("Swipe released — content restored");
        }

        // ─────────────────────────────────────────────
        // Unlock
        // ─────────────────────────────────────────────

        private void OnUnlockTriggered()
        {
            if (_isUnlocking) return;
            _isUnlocking = true;

            Log("Unlocked via swipe");
            GameEvents.TriggerPhoneUnlocked();

            if (GameBootstrap.SceneFlow != null)
                GameBootstrap.SceneFlow.GoToPhoneScreen();
            else
                Debug.LogError("[LockScreen] GameBootstrap.SceneFlow not found!");
        }

        // ─────────────────────────────────────────────
        // Logging
        // ─────────────────────────────────────────────

        private void Log(string message)
        {
            if (DebugLogs) Debug.Log($"[LockScreen] {message}");
        }

        private void LogWarning(string message)
        {
            if (DebugLogs) Debug.LogWarning($"[LockScreen] {message}");
        }
    }
}