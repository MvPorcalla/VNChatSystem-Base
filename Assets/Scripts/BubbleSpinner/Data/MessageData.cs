// ════════════════════════════════════════════════════════════════════════
// Assets/Scripts/BubbleSpinner/Data/MessageData.cs
// ════════════════════════════════════════════════════════════════════════

using System;
using System.Collections.Generic;

namespace BubbleSpinner.Data
{
    /// <summary>
    /// Defines the data structures for messages, choices, dialogue nodes, and conversation state used by BubbleSpinner.
    /// These classes are used to represent the parsed dialogue data from .bub files and to manage conversation state during execution.
    /// </summary>

    // ═══════════════════════════════════════════════════════════
    // ░ MESSAGE DATA
    // ═══════════════════════════════════════════════════════════

    [Serializable]
    public class MessageData
    {
        public enum MessageType
        {
            Text,
            Image,
            System
        }

        public MessageType type;
        public string speaker;
        public string content;
        public string imagePath;
        public string timestamp;
        public string messageId;
        public bool shouldUnlockCG;

        // ── PHASE 1 FIX (#5) ────────────────────────────────────────────
        // messageId is NO LONGER generated here.
        // It is assigned by BubbleSpinnerParser after construction using:
        //   "{nodeName}_{messageIndexWithinNode}"
        // This makes IDs deterministic across parses, so readMessageIds
        // in ConversationState correctly deduplicates on save/load resume.
        //
        // timestamp is still set at construction — it represents when
        // the message object was created (parse time), matching the
        // existing known limitation documented in the .bub format spec.
        // ────────────────────────────────────────────────────────────────

        public MessageData()
        {
            messageId = "";
            timestamp = DateTime.Now.ToString("HH:mm");
        }

        public MessageData(MessageType msgType, string msgSpeaker, string msgContent, string imgPath = "")
        {
            type = msgType;
            speaker = msgSpeaker;
            content = msgContent;
            imagePath = imgPath;
            timestamp = DateTime.Now.ToString("HH:mm");
            messageId = "";
            shouldUnlockCG = false;
        }
    }

    // ═══════════════════════════════════════════════════════════
    // ░ CHOICE DATA
    // ═══════════════════════════════════════════════════════════

    [Serializable]
    public class ChoiceData
    {
        public string choiceText;
        public string targetNode;
        public List<MessageData> playerMessages;

        public ChoiceData()
        {
            playerMessages = new List<MessageData>();
        }

        public ChoiceData(string text, string target)
        {
            choiceText = text;
            targetNode = target;
            playerMessages = new List<MessageData>();
        }
    }

    // ═══════════════════════════════════════════════════════════
    // ░ DIALOGUE NODE
    // ═══════════════════════════════════════════════════════════

    [Serializable]
    public class DialogueNode
    {
        public string nodeName;
        public List<MessageData> messages;
        public List<ChoiceData> choices;
        public List<PausePoint> pausePoints;
        public string nextNode;

        public DialogueNode()
        {
            messages = new List<MessageData>();
            choices = new List<ChoiceData>();
            pausePoints = new List<PausePoint>();
            nextNode = "";
        }

        public DialogueNode(string name)
        {
            nodeName = name;
            messages = new List<MessageData>();
            choices = new List<ChoiceData>();
            pausePoints = new List<PausePoint>();
            nextNode = "";
        }

        /// <summary>
        /// Returns true if there is a pause point whose stopIndex matches messageIndex.
        /// </summary>
        public bool ShouldPauseAfter(int messageIndex)
        {
            foreach (var p in pausePoints)
                if (p.stopIndex == messageIndex) return true;
            return false;
        }

        /// <summary>
        /// Returns the PausePoint whose stopIndex matches messageIndex, or null if none.
        /// </summary>
        public PausePoint GetPauseAt(int messageIndex)
        {
            foreach (var p in pausePoints)
                if (p.stopIndex == messageIndex) return p;
            return null;
        }
    }

    // ═══════════════════════════════════════════════════════════
    // ░ PAUSE POINT
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// Represents a -> ... pause point in a dialogue node.
    /// stopIndex        — pause is triggered after the message at this index is shown.
    /// playerMessageIndex — index of the paired Player: message in node.messages, or -1 if none.
    /// The parser sets playerMessageIndex by looking ahead from the -> ... line.
    /// The executor uses HasPlayerMessage to decide whether to emit a player bubble on click.
    /// </summary>
    [Serializable]
    public class PausePoint
    {
        public int stopIndex;
        public int playerMessageIndex;

        public bool HasPlayerMessage => playerMessageIndex >= 0;

        public PausePoint(int stop, int playerMsg = -1)
        {
            stopIndex = stop;
            playerMessageIndex = playerMsg;
        }
    }

    // ═══════════════════════════════════════════════════════════
    // ░ RESUME TARGET
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// Records exactly what the player was looking at when they exited the conversation.
    /// Used by ContinueFromCurrentState() to restore the correct UI on re-entry
    /// without re-deriving state from node inspection (which was the source of
    /// phantom pause buttons and incorrect replays).
    ///
    ///   None        — Fresh start, no previous exit point recorded
    ///   Pause       — Stopped at a real -> ... pause point (show continue button)
    ///   Choices     — Stopped at choice buttons (re-show choices directly)
    ///   End         — Stopped at end/next-chapter button (re-show end button directly)
    ///   Interrupted — Exited mid-message sequence (show continue button as safe resume)
    /// </summary>
    public enum ResumeTarget
    {
        None,
        Pause,
        Choices,
        End,
        Interrupted
    }

    // ═══════════════════════════════════════════════════════════
    // ░ CONVERSATION STATE (for saves)
    // ═══════════════════════════════════════════════════════════

    [Serializable]
    public class ConversationState
    {
        // ── VERSION BUMP ─────────────────────────────────────────
        // Version 2 adds resumeTarget. Existing saves on version 1
        // will deserialize resumeTarget as None (default enum value),
        // which routes to ProcessCurrentNode — same as the old
        // isInPauseState = false path. Safe migration, no data loss.
        // ─────────────────────────────────────────────────────────
        public const int CURRENT_VERSION = 2;

        public int version = CURRENT_VERSION;
        public string conversationId;
        public string characterName;
        public int currentChapterIndex;
        public string currentNodeName;
        public int currentMessageIndex;

        /// <summary>
        /// Kept for reference and NotifyInterrupted compatibility.
        /// Resume routing now uses resumeTarget instead.
        /// </summary>
        public bool isInPauseState;

        /// <summary>
        /// The authoritative signal for what to show on re-entry.
        /// Set by DialogueExecutor at every player-facing stop point.
        /// </summary>
        public ResumeTarget resumeTarget;

        public List<string> readMessageIds;
        public List<MessageData> messageHistory;
        public List<string> unlockedCGs;

        public ConversationState()
        {
            version = CURRENT_VERSION;
            conversationId = "";
            characterName = "";
            currentChapterIndex = 0;
            currentNodeName = "";
            currentMessageIndex = 0;
            isInPauseState = false;
            resumeTarget = ResumeTarget.None;
            readMessageIds = new List<string>();
            messageHistory = new List<MessageData>();
            unlockedCGs = new List<string>();
        }

        public ConversationState(string convId) : this()
        {
            conversationId = convId;
        }
    }
}