// ════════════════════════════════════════════════════════════════════════
// Assets/Scripts/UI/ChatApp/Core/ChatAppController.cs
// Phone Chat Simulation Game - Chat UI Controller (FIXED)
// ════════════════════════════════════════════════════════════════════════

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using BubbleSpinner.Core;
using BubbleSpinner.Data;
using ChatSim.Core;

namespace ChatSim.UI.ChatApp
{
    /// <summary>
    /// Main controller for Chat App UI - interfaces with BubbleSpinner.
    /// Attach to: ChatAppController GameObject
    /// </summary>
    public class ChatAppController : MonoBehaviour
    {
        // ═══════════════════════════════════════════════════════════
        // ░ INSPECTOR REFERENCES
        // ═══════════════════════════════════════════════════════════
        
        [Header("Panels")]
        [SerializeField] private GameObject contactListPanel;
        [SerializeField] private GameObject chatAppPanel;
        
        [Header("Chat Header")]
        [SerializeField] private Button chatBackButton;
        [SerializeField] private Image chatProfileIMG;
        [SerializeField] private TextMeshProUGUI chatProfileName;

        [Header("Chat Mode Toggle")]
        [SerializeField] private Button chatModeButton;
        [SerializeField] private Image chatModeIcon;
        [SerializeField] private Sprite fastModeSprite;
        [SerializeField] private Sprite normalModeSprite;
        
        [Header("Chat Display")]
        [SerializeField] private ScrollRect chatScrollRect;
        [SerializeField] private RectTransform chatContent;
        [SerializeField] private ChatMessageDisplay messageDisplay;
        [SerializeField] private ChatChoiceDisplay choiceDisplay;
        
        [Header("Timing Controller")]
        [SerializeField] private ChatTimingController timingController;
        
        [Header("Auto Scroll")]
        [SerializeField] private ChatAutoScroll autoScroll;
        
        [Header("New Message Indicator")]
        [SerializeField] private GameObject newMessageIndicator;
        [SerializeField] private Button newMessageButton;
        [SerializeField] private TextMeshProUGUI newMessageText;
        
        // ═══════════════════════════════════════════════════════════
        // ░ STATE
        // ═══════════════════════════════════════════════════════════
        
        private DialogueExecutor currentExecutor;
        private ConversationAsset currentConversation;
        private bool isFastMode = false;
        private int unreadMessageCount = 0;
        
        // ═══════════════════════════════════════════════════════════
        // ░ CONSTANTS
        // ═══════════════════════════════════════════════════════════
        
        private const string FAST_MODE_PREF_KEY = "ChatFastMode";
        
        // ═══════════════════════════════════════════════════════════
        // ░ UNITY LIFECYCLE
        // ═══════════════════════════════════════════════════════════
        
        private void Awake()
        {
            ValidateReferences();
            LoadFastModePreference();
            SetupEventListeners();
        }
        
        private void OnEnable()
        {
            SubscribeToScrollEvents();
        }
        
        private void OnDisable()
        {
            UnsubscribeFromScrollEvents();
            UnsubscribeFromExecutorEvents();
        }
        
        // ═══════════════════════════════════════════════════════════
        // ░ INITIALIZATION
        // ═══════════════════════════════════════════════════════════
        
        private void ValidateReferences()
        {
            if (chatBackButton == null)
                Debug.LogError("[ChatAppController] chatBackButton not assigned!");
            
            if (messageDisplay == null)
                Debug.LogError("[ChatAppController] messageDisplay not assigned!");
            
            if (choiceDisplay == null)
                Debug.LogError("[ChatAppController] choiceDisplay not assigned!");
                
            if (timingController == null)
                Debug.LogError("[ChatAppController] timingController not assigned!");
            
            if (autoScroll == null)
                Debug.LogWarning("[ChatAppController] autoScroll not assigned - auto-scroll disabled");
        }
        
        private void SetupEventListeners()
        {
            chatBackButton?.onClick.AddListener(OnBackButtonClicked);
            chatModeButton?.onClick.AddListener(OnModeButtonClicked);
            newMessageButton?.onClick.AddListener(OnNewMessageIndicatorClicked);
            
            // Set initial icon
            UpdateModeIcon();
            
            // Hide new message indicator initially
            if (newMessageIndicator != null)
            {
                newMessageIndicator.SetActive(false);
            }
        }
        
        private void SubscribeToScrollEvents()
        {
            if (autoScroll != null)
            {
                autoScroll.OnScrollReachedBottom += OnScrollReachedBottom;
            }
        }
        
        private void UnsubscribeFromScrollEvents()
        {
            if (autoScroll != null)
            {
                autoScroll.OnScrollReachedBottom -= OnScrollReachedBottom;
            }
        }
        
        private void LoadFastModePreference()
        {
            isFastMode = PlayerPrefs.GetInt(FAST_MODE_PREF_KEY, 0) == 1;
            
            if (timingController != null)
            {
                timingController.SetFastMode(isFastMode);
            }
        }
        
        // ═══════════════════════════════════════════════════════════
        // ░ PUBLIC API - START CONVERSATION
        // ═══════════════════════════════════════════════════════════
        
        /// <summary>
        /// Called by CharacterButton when a conversation is selected
        /// </summary>
        public void StartConversation(ConversationAsset conversationAsset)
        {
            if (conversationAsset == null)
            {
                Debug.LogError("[ChatAppController] Cannot start null conversation!");
                return;
            }
            
            StartCoroutine(StartConversationSequence(conversationAsset));
        }

        private IEnumerator StartConversationSequence(ConversationAsset conversationAsset)
        {
            Debug.Log($"[ChatAppController] Starting conversation: {conversationAsset.characterName}");
            
            // STEP 1: Switch to chat panel FIRST
            SwitchToChatPanel();
            
            // STEP 2: Wait for panel to activate
            yield return null;
            Canvas.ForceUpdateCanvases();
            
            // STEP 3: Store reference
            currentConversation = conversationAsset;
            
            // STEP 4: Setup UI
            SetupChatHeader(conversationAsset);
            ClearChatDisplay();
            HideNewMessageIndicator();
            
            // STEP 5: Start conversation via GameBootstrap
            currentExecutor = GameBootstrap.Conversation.StartConversation(conversationAsset);
            
            if (currentExecutor == null)
            {
                Debug.LogError("[ChatAppController] Failed to start conversation!");
                yield break;
            }
            
            // STEP 6: Subscribe to executor events
            SubscribeToExecutorEvents();
            
            // STEP 7: Load conversation history (if resuming)
            LoadConversationHistory();
            
            // STEP 8: Check if we're resuming in pause state
            var state = currentExecutor.GetState();
            if (state != null && state.isInPauseState)
            {
                Debug.Log("[ChatAppController] Resuming in pause state - showing pause button");
                
                // Wait a frame to ensure UI is ready
                yield return null;
                
                // Show pause button immediately without processing nodes
                choiceDisplay.ShowContinueButton(OnContinueButtonClicked);
            }
            else
            {
                // STEP 9: Start dialogue flow (normal case)
                currentExecutor.ContinueFromCurrentState();
            }
        }
        
        // ═══════════════════════════════════════════════════════════
        // ░ UI SETUP
        // ═══════════════════════════════════════════════════════════
        
        private void SetupChatHeader(ConversationAsset asset)
        {
            if (chatProfileName != null)
            {
                chatProfileName.text = asset.characterName;
            }
            
            // TODO: Load profile image from Addressables
            // if (chatProfileIMG != null && asset.profileImage != null)
            // {
            //     LoadProfileImage(asset.profileImage);
            // }
        }
        
        private void ClearChatDisplay()
        {
            messageDisplay.ClearAllMessages();
            choiceDisplay.ClearChoices();
        }
        
        private void SwitchToChatPanel()
        {
            contactListPanel.SetActive(false);
            chatAppPanel.SetActive(true);
            
            Debug.Log("[ChatAppController] Switched to chat panel");
        }
        
        private void SwitchToContactList()
        {
            chatAppPanel.SetActive(false);
            contactListPanel.SetActive(true);
            
            Debug.Log("[ChatAppController] Switched to contact list");
        }
        
        // ═══════════════════════════════════════════════════════════
        // ░ CONVERSATION HISTORY
        // ═══════════════════════════════════════════════════════════
        
        private void LoadConversationHistory()
        {
            if (currentExecutor == null) return;
            
            var state = currentExecutor.GetState();
            
            if (state?.messageHistory != null && state.messageHistory.Count > 0)
            {
                Debug.Log($"[ChatAppController] Loading {state.messageHistory.Count} historical messages");
                
                // Display all historical messages instantly (no animation)
                foreach (var msg in state.messageHistory)
                {
                    messageDisplay.DisplayMessage(msg, instant: true);
                }
                
                // Scroll to bottom after history is loaded
                StartCoroutine(ScrollToBottomDelayed());
            }
        }
        
        // ═══════════════════════════════════════════════════════════
        // ░ EXECUTOR EVENT SUBSCRIPTIONS
        // ═══════════════════════════════════════════════════════════
        
        private void SubscribeToExecutorEvents()
        {
            if (currentExecutor == null) return;
            
            currentExecutor.OnMessagesReady += HandleMessagesReady;
            currentExecutor.OnChoicesReady += HandleChoicesReady;
            currentExecutor.OnPauseReached += HandlePauseReached;
            currentExecutor.OnConversationEnd += HandleConversationEnd;
            currentExecutor.OnChapterChange += HandleChapterChange;
            
            Debug.Log("[ChatAppController] Subscribed to executor events");
        }
        
        private void UnsubscribeFromExecutorEvents()
        {
            if (currentExecutor == null) return;
            
            currentExecutor.OnMessagesReady -= HandleMessagesReady;
            currentExecutor.OnChoicesReady -= HandleChoicesReady;
            currentExecutor.OnPauseReached -= HandlePauseReached;
            currentExecutor.OnConversationEnd -= HandleConversationEnd;
            currentExecutor.OnChapterChange -= HandleChapterChange;
            
            Debug.Log("[ChatAppController] Unsubscribed from executor events");
        }
        
        // ═══════════════════════════════════════════════════════════
        // ░ EXECUTOR EVENT HANDLERS
        // ═══════════════════════════════════════════════════════════
        
        private void HandleMessagesReady(List<MessageData> messages)
        {
            Debug.Log($"[ChatAppController] Handling {messages.Count} new messages");
            
            if (messages.Count == 0)
            {
                currentExecutor.OnMessagesDisplayComplete();
                return;
            }
            
            // Queue messages for timed display
            timingController.QueueMessages(messages, OnMessagesDisplayComplete);
        }
        
        private void HandleChoicesReady(List<ChoiceData> choices)
        {
            Debug.Log($"[ChatAppController] Showing {choices.Count} choices");
            
            choiceDisplay.DisplayChoices(choices, OnChoiceSelected);
        }
        
        private void HandlePauseReached()
        {
            Debug.Log("[ChatAppController] Pause reached - showing continue button");
            
            choiceDisplay.ShowContinueButton(OnContinueButtonClicked);
        }
        
        /// <summary>
        /// Proper end button handling with chapter detection
        /// </summary>
        private void HandleConversationEnd()
        {
            Debug.Log("[ChatAppController] Conversation ended");
            
            // Save conversation state
            GameBootstrap.Conversation.SaveCurrentConversation();
            
            // Clear choices
            choiceDisplay.ClearChoices();
            
            // Check if there are more chapters
            bool hasMoreChapters = HasMoreChapters();
            
            if (hasMoreChapters)
            {
                Debug.Log("[ChatAppController] More chapters available - showing continue to next chapter button");
                choiceDisplay.ShowEndButton("Continue to Next Chapter", OnContinueToNextChapterClicked);
            }
            else
            {
                Debug.Log("[ChatAppController] No more chapters - showing end/reset button");
                choiceDisplay.ShowEndButton("Return to Contacts", OnReturnToContactsClicked);
            }
        }
        
        private void HandleChapterChange(string chapterName)
        {
            Debug.Log($"[ChatAppController] Chapter changed: {chapterName}");
            
            // Optional: Show chapter transition UI
            // messageDisplay.DisplaySystemMessage($"--- {chapterName} ---");
        }
        
        // ═══════════════════════════════════════════════════════════
        // ░ CHAPTER DETECTION & NAVIGATION
        // ═══════════════════════════════════════════════════════════
        
        /// <summary>
        /// Check if current conversation has more chapters after current one
        /// </summary>
        private bool HasMoreChapters()
        {
            if (currentConversation == null || currentExecutor == null)
                return false;
            
            var state = currentExecutor.GetState();
            if (state == null)
                return false;
            
            int totalChapters = currentConversation.chapters.Count;
            int currentChapter = state.currentChapterIndex;
            
            return currentChapter < totalChapters - 1;
        }
        
        /// <summary>
        /// Handle "Continue to Next Chapter" button click
        /// </summary>
        private void OnContinueToNextChapterClicked()
        {
            Debug.Log("[ChatAppController] Continue to next chapter clicked");
            
            if (currentConversation == null || currentExecutor == null)
            {
                Debug.LogError("[ChatAppController] Cannot continue - no active conversation");
                return;
            }
            
            var state = currentExecutor.GetState();
            if (state == null)
            {
                Debug.LogError("[ChatAppController] Cannot continue - no state");
                return;
            }
            
            // Move to next chapter
            state.currentChapterIndex++;
            
            // Reset to first node of next chapter
            state.currentNodeName = "Start"; // BubbleSpinner convention
            state.currentMessageIndex = 0;
            state.isInPauseState = false;
            
            // Clear UI
            choiceDisplay.ClearChoices();
            
            // Save state before reloading
            GameBootstrap.Conversation.ForceSaveCurrentConversation();
            
            // Reload conversation (will load new chapter)
            Debug.Log($"[ChatAppController] Loading chapter {state.currentChapterIndex}");
            StartCoroutine(ReloadCurrentConversation());
        }
        
        /// <summary>
        /// Handle "Return to Contacts" button click
        /// </summary>
        private void OnReturnToContactsClicked()
        {
            Debug.Log("[ChatAppController] Return to contacts clicked");
            
            // Clear choices
            choiceDisplay.ClearChoices();
            
            // Save and exit conversation
            OnBackButtonClicked();
        }
        
        /// <summary>
        /// Reload current conversation (used after chapter transitions)
        /// </summary>
        private IEnumerator ReloadCurrentConversation()
        {
            // Unsubscribe from current executor
            UnsubscribeFromExecutorEvents();
            
            // Clear display
            ClearChatDisplay();
            
            yield return null;
            
            // Restart conversation (will load from saved state with new chapter)
            currentExecutor = GameBootstrap.Conversation.StartConversation(currentConversation);
            
            if (currentExecutor == null)
            {
                Debug.LogError("[ChatAppController] Failed to reload conversation!");
                yield break;
            }
            
            // Resubscribe to events
            SubscribeToExecutorEvents();
            
            // Load history (should be empty for new chapter)
            LoadConversationHistory();
            
            // Continue from new chapter start
            currentExecutor.ContinueFromCurrentState();
        }
        
        // ═══════════════════════════════════════════════════════════
        // ░ TIMING CONTROLLER CALLBACKS
        // ═══════════════════════════════════════════════════════════
        
        private void OnMessagesDisplayComplete()
        {
            Debug.Log("[ChatAppController] Messages display complete");
            
            // Notify executor
            currentExecutor?.OnMessagesDisplayComplete();
        }
        
        public void OnNewMessageDisplayed(MessageData message)
        {
            // Show new message indicator if user is scrolled up
            if (autoScroll != null && !autoScroll.IsAtBottom())
            {
                unreadMessageCount++;
                ShowNewMessageIndicator();
            }
        }
        
        // ═══════════════════════════════════════════════════════════
        // ░ CHOICE HANDLING
        // ═══════════════════════════════════════════════════════════
        
        private void OnChoiceSelected(ChoiceData choice)
        {
            Debug.Log($"[ChatAppController] Choice selected: {choice.choiceText}");
            
            // Clear choices
            choiceDisplay.ClearChoices();
            
            // Notify executor
            currentExecutor.OnChoiceSelected(choice);
        }
        
        private void OnContinueButtonClicked()
        {
            Debug.Log("[ChatAppController] Continue button clicked");
            
            // Clear continue button
            choiceDisplay.ClearChoices();
            
            // Notify executor
            currentExecutor.OnPauseButtonClicked();
        }
        
        // ═══════════════════════════════════════════════════════════
        // ░ UI INTERACTIONS - INTERNAL BACK BUTTON
        // ═══════════════════════════════════════════════════════════
        
        private void OnBackButtonClicked()
        {
            Debug.Log("[ChatAppController] Internal back button clicked");
            
            // Use shared cleanup logic
            PerformConversationCleanup();
            
            // Return to contact list
            SwitchToContactList();
        }
        
        // ═══════════════════════════════════════════════════════════
        // ░ PUBLIC API - EXTERNAL CLEANUP
        // ═══════════════════════════════════════════════════════════
        
        /// <summary>
        /// Public API for external cleanup (called by ChatAppUIManager or other systems)
        /// Use this when external systems need to close the chat without using the internal back button
        /// </summary>
        public void ExitCurrentConversation()
        {
            Debug.Log("[ChatAppController] External exit requested - cleaning up conversation");
            
            // Use shared cleanup logic
            PerformConversationCleanup();
            
            Debug.Log("[ChatAppController] Conversation cleanup complete (external request)");
            
            // NOTE: We don't switch panels here - let the caller (UIManager) handle that
        }
        
        // ═══════════════════════════════════════════════════════════
        // ░ SHARED CLEANUP LOGIC
        // ═══════════════════════════════════════════════════════════
        
        /// <summary>
        /// Core cleanup logic used by both internal and external exit methods
        /// </summary>
        private void PerformConversationCleanup()
        {
            Debug.Log("[ChatAppController] Performing conversation cleanup");
            
            // STEP 1: Check if we're interrupting an active message sequence
            bool wasInterrupted = timingController != null && timingController.IsDisplayingMessages;
            
            // STEP 2: Stop timing controller FIRST (prevents coroutine errors)
            if (timingController != null)
            {
                timingController.StopCurrentSequence();
            }
            
            // STEP 3: Force pause state if we interrupted message display
            if (wasInterrupted && currentExecutor != null)
            {
                var state = currentExecutor.GetState();
                if (state != null && !state.isInPauseState)
                {
                    Debug.Log("[ChatAppController] Messages were interrupted - forcing pause state");
                    state.isInPauseState = true;
                }
            }
            
            // STEP 4: Save conversation state (INCLUDING forced pause state)
            if (currentExecutor != null)
            {
                GameBootstrap.Conversation.SaveCurrentConversation();
            }
            
            // STEP 5: Unsubscribe from events
            UnsubscribeFromExecutorEvents();
            
            // STEP 6: Clear current executor and conversation references
            currentExecutor = null;
            currentConversation = null;
            
            // STEP 7: Clear UI elements
            ClearChatDisplay();
            HideNewMessageIndicator();
            
            Debug.Log("[ChatAppController] Cleanup complete");
        }
        
        // ═══════════════════════════════════════════════════════════
        // ░ MODE TOGGLE
        // ═══════════════════════════════════════════════════════════
        
        private void OnModeButtonClicked()
        {
            // Toggle mode
            isFastMode = !isFastMode;
            
            // Save preference
            PlayerPrefs.SetInt(FAST_MODE_PREF_KEY, isFastMode ? 1 : 0);
            PlayerPrefs.Save();
            
            // Update timing controller
            if (timingController != null)
            {
                timingController.SetFastMode(isFastMode);
            }
            
            // Update icon
            UpdateModeIcon();
            
            Debug.Log($"[ChatAppController] Fast mode: {isFastMode}");
        }

        private void UpdateModeIcon()
        {
            if (chatModeIcon != null && fastModeSprite != null && normalModeSprite != null)
            {
                chatModeIcon.sprite = isFastMode ? fastModeSprite : normalModeSprite;
            }
        }
        
        // ═══════════════════════════════════════════════════════════
        // ░ NEW MESSAGE INDICATOR
        // ═══════════════════════════════════════════════════════════
        
        private void ShowNewMessageIndicator()
        {
            if (newMessageIndicator != null)
            {
                newMessageIndicator.SetActive(true);
                
                if (newMessageText != null)
                {
                    newMessageText.text = unreadMessageCount == 1 
                        ? "1 new message" 
                        : $"{unreadMessageCount} new messages";
                }
            }
        }
        
        private void HideNewMessageIndicator()
        {
            if (newMessageIndicator != null)
            {
                newMessageIndicator.SetActive(false);
            }
            
            unreadMessageCount = 0;
        }
        
        private void OnNewMessageIndicatorClicked()
        {
            Debug.Log("[ChatAppController] New message indicator clicked");
            
            // Scroll to bottom
            if (autoScroll != null)
            {
                autoScroll.ScrollToBottom();
            }
            
            HideNewMessageIndicator();
        }
        
        private void OnScrollReachedBottom()
        {
            Debug.Log("[ChatAppController] Scroll reached bottom");
            HideNewMessageIndicator();
        }
        
        // ═══════════════════════════════════════════════════════════
        // ░ SCROLLING
        // ═══════════════════════════════════════════════════════════
        
        public void ForceScrollToBottom()
        {
            if (autoScroll != null)
            {
                autoScroll.ForceScrollToBottom();
            }
        }
        
        private IEnumerator ScrollToBottomDelayed()
        {
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            ForceScrollToBottom();
        }
    }
}