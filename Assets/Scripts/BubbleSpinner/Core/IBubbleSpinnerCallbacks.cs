// ════════════════════════════════════════════════════════════════════════
// Assets/Scripts/BubbleSpinner/Core/IBubbleSpinnerCallbacks.cs
// ════════════════════════════════════════════════════════════════════════

using BubbleSpinner.Data;

namespace BubbleSpinner.Core
{
    /// <summary>
    /// Interface for BubbleSpinner to communicate with external systems (like your game) 
    /// for saving/loading conversation state and notifying about conversation events.
    /// </summary>
    public interface IBubbleSpinnerCallbacks
    {
        // ═══════════════════════════════════════════════════════════
        // ░ SAVE/LOAD CALLBACKS
        // ═══════════════════════════════════════════════════════════
        
        /// <summary>
        /// Called when BubbleSpinner needs to save conversation state.
        /// Return true if save was successful.
        /// </summary>
        bool SaveConversationState(ConversationState state);
        
        /// <summary>
        /// Called when BubbleSpinner needs to load conversation state.
        /// Return existing state or null if no save exists.
        /// </summary>
        ConversationState LoadConversationState(string conversationId);
        
        /// <summary>
        /// Called when BubbleSpinner needs to delete conversation state.
        /// </summary>
        void DeleteConversationState(string conversationId);
        
        // ═══════════════════════════════════════════════════════════
        // ░ EVENT CALLBACKS
        // ═══════════════════════════════════════════════════════════
        
        /// <summary>
        /// Called when a conversation starts.
        /// </summary>
        void OnConversationStarted(string conversationId);
        
        /// <summary>
        /// Called when a CG should be unlocked.
        /// </summary>
        void OnCGUnlocked(string cgAddressableKey);
        
        /// <summary>
        /// Called when a conversation ends.
        /// </summary>
        void OnConversationEnded(string conversationId);
        
        /// <summary>
        /// Called when a chapter changes.
        /// </summary>
        void OnChapterChanged(string conversationId, int chapterIndex, string chapterName);
    }
}