// ════════════════════════════════════════════════════════════════════════
// Assets/Scripts/ChatSim/Core/BubbleSpinnerBridge.cs
// ════════════════════════════════════════════════════════════════════════

using UnityEngine;
using BubbleSpinner.Core;
using BubbleSpinner.Data;
using ChatSim.Data;

namespace ChatSim.Core
{
    /// <summary>
    /// Bridge class that implements IBubbleSpinnerCallbacks to connect BubbleSpinner's conversation system with ChatSim's game-specific logic.
    /// This class handles saving/loading conversation state using ChatSim's SaveSystem 
    /// and triggers ChatSim events when conversations start, end, or when CGs are unlocked.
    /// </summary>
    public class BubbleSpinnerBridge : IBubbleSpinnerCallbacks
    {
        private ConversationManager _conversationManager;

        public BubbleSpinnerBridge(ConversationManager conversationManager)
        {
            _conversationManager = conversationManager;
            GameEvents.OnCharacterStoryReset += OnCharacterStoryReset;
        }

        // ═══════════════════════════════════════════════════════════
        // ░ SAVE/LOAD CALLBACKS
        // ═══════════════════════════════════════════════════════════

        public bool SaveConversationState(ConversationState state)
        {
            if (state == null)
            {
                Debug.LogError("[BubbleSpinnerBridge] Cannot save null state");
                return false;
            }

            var saveData = GameBootstrap.Save.LoadGame();
            
            if (saveData == null)
            {
                Debug.LogError("[BubbleSpinnerBridge] SaveData is null!");
                return false;
            }

            // Find and replace existing state, or add new one
            var index = saveData.conversationStates.FindIndex(c => c.conversationId == state.conversationId);
            
            if (index >= 0)
            {
                saveData.conversationStates[index] = state;
            }
            else
            {
                saveData.conversationStates.Add(state);
            }

            // Save to disk
            bool success = GameBootstrap.Save.SaveGame(saveData);
            
            if (success)
            {
                Debug.Log($"[BubbleSpinnerBridge] ✓ Saved conversation: {state.conversationId}");
            }
            else
            {
                Debug.LogError($"[BubbleSpinnerBridge] ✗ Failed to save: {state.conversationId}");
            }

            return success;
        }

        public ConversationState LoadConversationState(string conversationId)
        {
            var saveData = GameBootstrap.Save.LoadGame();
            
            if (saveData == null)
            {
                Debug.LogWarning("[BubbleSpinnerBridge] SaveData is null, cannot load state");
                return null;
            }

            var state = saveData.conversationStates.Find(c => c.conversationId == conversationId);
            
            if (state != null)
            {
                Debug.Log($"[BubbleSpinnerBridge] ✓ Loaded conversation: {conversationId}");
            }
            else
            {
                Debug.Log($"[BubbleSpinnerBridge] No saved state for: {conversationId}");
            }

            return state;
        }

        public void DeleteConversationState(string conversationId)
        {
            var saveData = GameBootstrap.Save.LoadGame();
            
            if (saveData == null)
            {
                Debug.LogWarning("[BubbleSpinnerBridge] SaveData is null, cannot delete state");
                return;
            }

            int removed = saveData.conversationStates.RemoveAll(c => c.conversationId == conversationId);
            
            if (removed > 0)
            {
                GameBootstrap.Save.SaveGame(saveData);
                Debug.Log($"[BubbleSpinnerBridge] ✓ Deleted conversation: {conversationId}");
            }
            else
            {
                Debug.LogWarning($"[BubbleSpinnerBridge] No state found to delete: {conversationId}");
            }
        }

        // ═══════════════════════════════════════════════════════════
        // ░ EVENT CALLBACKS (bridge to GameEvents)
        // ═══════════════════════════════════════════════════════════

        public void OnConversationStarted(string conversationId)
        {
            GameEvents.TriggerConversationStarted(conversationId);
        }

        public void OnCGUnlocked(string cgAddressableKey)
        {
            GameEvents.TriggerCGUnlocked(cgAddressableKey);
        }

        public void OnConversationEnded(string conversationId)
        {
            // Optional: Trigger custom event if needed
            Debug.Log($"[BubbleSpinnerBridge] Conversation ended: {conversationId}");
        }

        public void OnChapterChanged(string conversationId, int chapterIndex, string chapterName)
        {
            Debug.Log($"[BubbleSpinnerBridge] Chapter changed: {conversationId} - {chapterName}");
        }

        private void OnCharacterStoryReset(string conversationId)
        {
            Debug.Log($"[BubbleSpinnerBridge] Story reset — evicting cache for: {conversationId}");
            _conversationManager?.ResetConversation(conversationId);
        }

        public void Cleanup()
        {
            GameEvents.OnCharacterStoryReset -= OnCharacterStoryReset;
            _conversationManager = null;
        }
    }
}