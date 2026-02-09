// ════════════════════════════════════════════════════════════════════════
// Assets/Scripts/BubbleSpinner/Core/ConversationManager.cs
// BubbleSpinner - Conversation State Management (FIXED)
// ════════════════════════════════════════════════════════════════════════

using System.Collections.Generic;
using UnityEngine;
using BubbleSpinner.Data;
using ChatSim.Core;
using ChatSim.Data;

namespace BubbleSpinner.Core
{
    /// <summary>
    /// Manages conversations and integrates with GameBootstrap save system.
    /// Access via: GameBootstrap.Conversation
    /// </summary>
    public class ConversationManager : MonoBehaviour
    {
        // ═══════════════════════════════════════════════════════════
        // ░ CONSTANTS
        // ═══════════════════════════════════════════════════════════

        private const float SAVE_THROTTLE_DELAY = 0.5f;

        // ═══════════════════════════════════════════════════════════
        // ░ STATE
        // ═══════════════════════════════════════════════════════════

        private Dictionary<string, DialogueExecutor> activeExecutors = new Dictionary<string, DialogueExecutor>();
        private Dictionary<string, ConversationState> activeStates = new Dictionary<string, ConversationState>();
        private DialogueExecutor currentExecutor;
        private string currentConversationId;

        // Save throttling
        private float lastSaveTime = -999f;
        private bool hasPendingSave = false;

        // ═══════════════════════════════════════════════════════════
        // ░ PROPERTIES
        // ═══════════════════════════════════════════════════════════

        public DialogueExecutor CurrentExecutor => currentExecutor;
        public string CurrentConversationId => currentConversationId;
        public bool HasActiveConversation => currentExecutor != null;

        // ═══════════════════════════════════════════════════════════
        // ░ INITIALIZATION
        // ═══════════════════════════════════════════════════════════

        public void Init()
        {
            Debug.Log("[ConversationManager] Initialized");
        }

        // ═══════════════════════════════════════════════════════════
        // ░ PUBLIC API - CONVERSATION CONTROL
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Starts or resumes a conversation.
        /// Returns the DialogueExecutor for UI to subscribe to.
        /// </summary>
        public DialogueExecutor StartConversation(ConversationAsset asset)
        {
            if (asset == null)
            {
                Debug.LogError("[ConversationManager] Cannot start null conversation");
                return null;
            }

            string convId = asset.ConversationId;
            Debug.Log($"[ConversationManager] Starting conversation: {asset.characterName} (ID: {convId})");

            // Get or create state
            ConversationState state = GetOrCreateState(convId, asset.characterName);

            if (!activeStates.ContainsKey(convId))
            {
                activeStates[convId] = state;
            }

            // Create or reuse executor
            if (!activeExecutors.ContainsKey(convId))
            {
                var executor = new DialogueExecutor();
                
                // Initialize executor (this sets currentNodeName)
                executor.Initialize(asset, state);
                
                // Subscribe to executor events for auto-save
                SubscribeToExecutorEvents(executor);
                
                activeExecutors[convId] = executor;
                Debug.Log($"[ConversationManager] Created new executor for {convId}");
                
                // Save after executor initialized state properly
                ForceSaveGame();
                Debug.Log($"[ConversationManager] ✓ Initial save complete: Node='{state.currentNodeName}'");
            }
            else
            {
                Debug.Log($"[ConversationManager] Reusing existing executor for {convId}");
            }

            currentExecutor = activeExecutors[convId];
            currentConversationId = convId;

            // Trigger global event
            GameEvents.TriggerConversationStarted(convId);

            return currentExecutor;
        }

        /// <summary>
        /// Saves the current conversation state (throttled).
        /// </summary>
        public void SaveCurrentConversation()
        {
            if (string.IsNullOrEmpty(currentConversationId) || currentExecutor == null)
            {
                Debug.LogWarning("[ConversationManager] No active conversation to save");
                return;
            }

            SaveConversationState(throttle: true);
        }

        /// <summary>
        /// Forces an immediate save (bypasses throttle).
        /// Use for critical save points (quit, pause, chapter end).
        /// </summary>
        public void ForceSaveCurrentConversation()
        {
            if (string.IsNullOrEmpty(currentConversationId) || currentExecutor == null)
            {
                Debug.LogWarning("[ConversationManager] No active conversation to force save");
                return;
            }

            SaveConversationState(throttle: false);
        }

        /// <summary>
        /// Ends the current conversation and saves state.
        /// </summary>
        public void EndCurrentConversation()
        {
            if (currentExecutor == null)
            {
                Debug.LogWarning("[ConversationManager] No active conversation to end");
                return;
            }

            Debug.Log($"[ConversationManager] Ending conversation: {currentConversationId}");

            // Force save before clearing
            ForceSaveCurrentConversation();

            // Unsubscribe from events
            UnsubscribeFromExecutorEvents(currentExecutor);

            currentExecutor = null;
            currentConversationId = null;

            Debug.Log("[ConversationManager] Conversation ended");
        }

        /// <summary>
        /// Resets a conversation to its initial state (clears save data).
        /// </summary>
        public void ResetConversation(string conversationId)
        {
            Debug.Log($"[ConversationManager] Resetting conversation: {conversationId}");

            // Remove from active executors
            if (activeExecutors.ContainsKey(conversationId))
            {
                var executor = activeExecutors[conversationId];
                UnsubscribeFromExecutorEvents(executor);
                activeExecutors.Remove(conversationId);
            }

            if (activeStates.ContainsKey(conversationId))
            {
                activeStates.Remove(conversationId);
            }

            // Clear current if it matches
            if (currentConversationId == conversationId)
            {
                currentExecutor = null;
                currentConversationId = null;
            }

            // Remove from save data
            var saveData = GameBootstrap.Save.LoadGame();
            if (saveData != null)
            {
                saveData.conversationStates.RemoveAll(c => c.conversationId == conversationId);
                GameBootstrap.Save.SaveGame(saveData);
            }

            Debug.Log($"[ConversationManager] Conversation reset complete: {conversationId}");
        }

        // ═══════════════════════════════════════════════════════════
        // ░ CG GALLERY API
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Gets all unlocked CGs for a specific conversation.
        /// </summary>
        public List<string> GetUnlockedCGs(string conversationId)
        {
            var state = GetOrCreateState(conversationId, "");
            return state?.unlockedCGs ?? new List<string>();
        }

        /// <summary>
        /// Gets all unlocked CGs across all conversations.
        /// </summary>
        public List<string> GetAllUnlockedCGs()
        {
            var allCGs = new List<string>();
            var saveData = GameBootstrap.Save.LoadGame();

            if (saveData?.conversationStates != null)
            {
                foreach (var state in saveData.conversationStates)
                {
                    if (state.unlockedCGs != null)
                    {
                        allCGs.AddRange(state.unlockedCGs);
                    }
                }
            }

            return allCGs;
        }

        /// <summary>
        /// Checks if a specific CG is unlocked in a conversation.
        /// </summary>
        public bool IsCGUnlocked(string conversationId, string cgPath)
        {
            var state = GetOrCreateState(conversationId, "");
            return state?.unlockedCGs?.Contains(cgPath) ?? false;
        }

        // ═══════════════════════════════════════════════════════════
        // ░ EVENT SUBSCRIPTIONS (for auto-save)
        // ═══════════════════════════════════════════════════════════

        private void SubscribeToExecutorEvents(DialogueExecutor executor)
        {
            executor.OnMessagesReady += OnExecutorMessagesReady;
            executor.OnChoicesReady += OnExecutorChoicesReady;
            executor.OnPauseReached += OnExecutorPauseReached;
            executor.OnConversationEnd += OnExecutorConversationEnd;
            executor.OnChapterChange += OnExecutorChapterChange;

            Debug.Log("[ConversationManager] Subscribed to executor events for auto-save");
        }

        private void UnsubscribeFromExecutorEvents(DialogueExecutor executor)
        {
            if (executor == null) return;

            executor.OnMessagesReady -= OnExecutorMessagesReady;
            executor.OnChoicesReady -= OnExecutorChoicesReady;
            executor.OnPauseReached -= OnExecutorPauseReached;
            executor.OnConversationEnd -= OnExecutorConversationEnd;
            executor.OnChapterChange -= OnExecutorChapterChange;

            Debug.Log("[ConversationManager] Unsubscribed from executor events");
        }

        // ═══════════════════════════════════════════════════════════
        // ░ EXECUTOR EVENT HANDLERS (auto-save triggers)
        // ═══════════════════════════════════════════════════════════

        private void OnExecutorMessagesReady(List<MessageData> messages)
        {
            SaveCurrentConversation();
        }

        private void OnExecutorChoicesReady(List<ChoiceData> choices)
        {
            SaveCurrentConversation();
        }

        private void OnExecutorPauseReached()
        {
            SaveCurrentConversation();
        }

        private void OnExecutorConversationEnd()
        {
            ForceSaveCurrentConversation();
        }

        private void OnExecutorChapterChange(string chapterName)
        {
            Debug.Log($"[ConversationManager] Chapter changed: {chapterName}");
            ForceSaveCurrentConversation();
        }

        // ═══════════════════════════════════════════════════════════
        // ░ SAVE/LOAD LOGIC
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Get or create state WITHOUT saving immediately.
        /// Save happens AFTER executor initialization.
        /// </summary>
        private ConversationState GetOrCreateState(string conversationId, string characterName)
        {
            var saveData = GameBootstrap.Save.LoadGame();
            
            if (saveData == null)
            {
                Debug.LogError("[ConversationManager] CRITICAL: SaveData is null!");
                return new ConversationState(conversationId) { characterName = characterName };
            }

            // Find existing state
            var existingState = saveData.conversationStates.Find(c => c.conversationId == conversationId);
            
            if (existingState != null)
            {
                Debug.Log($"[ConversationManager] Loaded existing state: {conversationId} " +
                         $"(Chapter: {existingState.currentChapterIndex}, Node: '{existingState.currentNodeName}')");
                return existingState;
            }

            // Create new state and add to list
            var newState = new ConversationState(conversationId)
            {
                characterName = characterName
            };

            saveData.conversationStates.Add(newState);
            
            Debug.Log($"[ConversationManager] Created new state: {conversationId}");

            return newState;
        }

        /// <summary>
        /// Save conversation state with optional throttling.
        /// </summary>
        private void SaveConversationState(bool throttle)
        {
            if (throttle)
            {
                float timeSinceLastSave = Time.realtimeSinceStartup - lastSaveTime;
                
                if (timeSinceLastSave < SAVE_THROTTLE_DELAY)
                {
                    hasPendingSave = true;
                    return;
                }
            }

            ForceSaveGame();
        }

        /// <summary>
        /// Performs the actual save operation.
        /// </summary>
        private void ForceSaveGame()
        {
            var saveData = GameBootstrap.Save.LoadGame();
            
            if (saveData == null)
            {
                Debug.LogError("[ConversationManager] Cannot save: SaveData is null");
                return;
            }

            if (!string.IsNullOrEmpty(currentConversationId) && activeStates.ContainsKey(currentConversationId))
            {
                var cachedState = activeStates[currentConversationId];
                
                // Find and replace the state in saveData
                var index = saveData.conversationStates.FindIndex(c => c.conversationId == currentConversationId);
                if (index >= 0)
                {
                    saveData.conversationStates[index] = cachedState;
                }
                else
                {
                    // State doesn't exist yet, add it
                    saveData.conversationStates.Add(cachedState);
                }
            }

            bool success = GameBootstrap.Save.SaveGame(saveData);

            if (success)
            {
                lastSaveTime = Time.realtimeSinceStartup;
                hasPendingSave = false;
                
                if (!string.IsNullOrEmpty(currentConversationId) && activeStates.ContainsKey(currentConversationId))
                {
                    var cachedState = activeStates[currentConversationId];
                    Debug.Log($"[ConversationManager] ✓ Saved: {currentConversationId} " +
                             $"(Node: '{cachedState.currentNodeName}', Chapter: {cachedState.currentChapterIndex})");
                }
            }
            else
            {
                Debug.LogError($"[ConversationManager] ✗ Save failed for: {currentConversationId}");
            }
        }

        // ═══════════════════════════════════════════════════════════
        // ░ LIFECYCLE (auto-save on app pause/focus/quit)
        // ═══════════════════════════════════════════════════════════

        private void Update()
        {
            if (hasPendingSave)
            {
                float timeSinceLastSave = Time.realtimeSinceStartup - lastSaveTime;
                
                if (timeSinceLastSave >= SAVE_THROTTLE_DELAY)
                {
                    if (!string.IsNullOrEmpty(currentConversationId))
                    {
                        SaveConversationState(throttle: false);
                    }
                }
            }
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus && currentExecutor != null && !string.IsNullOrEmpty(currentConversationId))
            {
                Debug.Log("[ConversationManager] App paused - force saving conversation");
                ForceSaveCurrentConversation();
            }
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus && currentExecutor != null && !string.IsNullOrEmpty(currentConversationId))
            {
                Debug.Log("[ConversationManager] App lost focus - force saving conversation");
                ForceSaveCurrentConversation();
            }
        }

        private void OnApplicationQuit()
        {
            if (currentExecutor != null && !string.IsNullOrEmpty(currentConversationId))
            {
                Debug.Log("[ConversationManager] App quitting - force saving conversation");
                ForceSaveCurrentConversation();
            }
        }

        // ═══════════════════════════════════════════════════════════
        // ░ EDITOR TOOLS
        // ═══════════════════════════════════════════════════════════

        #if UNITY_EDITOR
        [ContextMenu("Debug/Print Active Conversations")]
        private void DebugPrintActiveConversations()
        {
            Debug.Log("╔═══════════════ ACTIVE CONVERSATIONS ═══════════════╗");
            Debug.Log($"║ Current: {currentConversationId ?? "None"}");
            Debug.Log($"║ Active Executors: {activeExecutors.Count}");
            
            foreach (var kvp in activeExecutors)
            {
                var executor = kvp.Value;
                var state = executor.GetState();
                Debug.Log($"║   {kvp.Key}: Chapter {state.currentChapterIndex}, " +
                         $"Node '{state.currentNodeName}', Messages: {state.messageHistory.Count}");
            }
            
            Debug.Log("╚════════════════════════════════════════════════════╝");
        }

        [ContextMenu("Debug/Print Save Data")]
        private void DebugPrintSaveData()
        {
            var saveData = GameBootstrap.Save.LoadGame();
            
            Debug.Log("╔═══════════════ SAVE DATA ═══════════════╗");
            
            if (saveData == null)
            {
                Debug.Log("║ SaveData is NULL!");
            }
            else
            {
                Debug.Log($"║ Save Version: {saveData.saveVersion}");
                Debug.Log($"║ Conversations: {saveData.conversationStates.Count}");
                
                foreach (var state in saveData.conversationStates)
                {
                    Debug.Log($"║   {state.conversationId} ({state.characterName}):");
                    Debug.Log($"║     Chapter: {state.currentChapterIndex}");
                    Debug.Log($"║     Node: '{state.currentNodeName}'");
                    Debug.Log($"║     Messages: {state.messageHistory.Count}");
                    Debug.Log($"║     CGs: {state.unlockedCGs.Count}");
                }
            }
            
            Debug.Log("╚═════════════════════════════════════════╝");
        }

        [ContextMenu("Debug/Force Save Now")]
        private void DebugForceSave()
        {
            if (currentExecutor != null)
            {
                ForceSaveCurrentConversation();
                Debug.Log("✓ Force saved current conversation");
            }
            else
            {
                Debug.LogWarning("No active conversation to save");
            }
        }
        #endif
    }
}