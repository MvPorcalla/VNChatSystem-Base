// ════════════════════════════════════════════════════════════════════════
// Assets/Scripts/BubbleSpinner/Core/IBubbleSpinnerCallbacks.cs
// ════════════════════════════════════════════════════════════════════════

using BubbleSpinner.Data;

namespace BubbleSpinner.Core
{
    /// <summary>
    /// Contract for your game code to handle BubbleSpinner save/load and conversation events.
    /// </summary>
    public interface IBubbleSpinnerCallbacks
    {
        // ═══════════════════════════════════════════════════════════
        // SAVE / LOAD
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Persist the given conversation state. Returns true on success.
        /// </summary>
        bool SaveConversationState(ConversationState state);

        /// <summary>
        /// Load a previously saved conversation state. Returns null if no save exists.
        /// </summary>
        ConversationState LoadConversationState(string conversationId);

        /// <summary>
        /// Permanently delete the saved state for the given conversation ID.
        /// </summary>
        void DeleteConversationState(string conversationId);

        // ═══════════════════════════════════════════════════════════
        // EVENTS
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Called when a conversation session begins.
        /// </summary>
        void OnConversationStarted(string conversationId);

        /// <summary>
        /// Called when a CG image is unlocked during dialogue.
        /// Use the key to update gallery state in your save system.
        /// </summary>
        void OnCGUnlocked(string cgAddressableKey);

        /// <summary>
        /// Called when a conversation session ends.
        /// </summary>
        void OnConversationEnded(string conversationId);

        /// <summary>
        /// Called when the executor advances to a new chapter.
        /// </summary>
        void OnChapterChanged(string conversationId, int chapterIndex, string chapterName);
    }
}