// ════════════════════════════════════════════════════════════════════════
// Assets/Scripts/BubbleSpinner/Data/MessageData.cs
// Core data types for BubbleSpinner: messages, choices, dialogue nodes,
// pause points, resume targets, and serializable conversation state.
// ════════════════════════════════════════════════════════════════════════

using System;
using System.Collections.Generic;

namespace BubbleSpinner.Data
{
    // ═══════════════════════════════════════
    // MESSAGE DATA
    // ═══════════════════════════════════════

    [Serializable]
    public class MessageData
    {
        public enum MessageType { Text, Image, System }

        public MessageType type;
        public string speaker;
        public string content;
        public string imagePath;
        public string timestamp;
        public string messageId;    // Assigned later by parser; empty until then
        public bool shouldUnlockCG;

        /// <summary>
        /// Initializes a blank message with an empty ID and the current time as timestamp.
        /// </summary>
        public MessageData()
        {
            messageId = "";
            timestamp = DateTime.Now.ToString("HH:mm");
        }

        /// <summary>
        /// Initializes a message with the specified type, speaker, content, and optional image path.
        /// </summary>
        /// <param name="msgType">The type of the message (Text, Image, System).</param>
        /// <param name="msgSpeaker">The speaker of the message (character name or "System").</param>
        /// <param name="msgContent">The text content of the message (ignored for Image type).</param>
        /// <param name="imgPath">The image path for Image type messages (optional).</param>
        public MessageData(MessageType msgType, string msgSpeaker, string msgContent, string imgPath = "")
        {
            type          = msgType;
            speaker       = msgSpeaker;
            content       = msgContent;
            imagePath     = imgPath;
            timestamp     = DateTime.Now.ToString("HH:mm");
            messageId     = "";
            shouldUnlockCG = false;
        }
    }

    // ═══════════════════════════════════════
    // CHOICE DATA
    // ═══════════════════════════════════════

    [Serializable]
    public class ChoiceData
    {
        public string choiceText;
        public string targetNode;
        public List<MessageData> playerMessages;

        /// <summary>
        /// Initializes an empty choice with a blank player message list.
        /// </summary>
        public ChoiceData()
        {
            playerMessages = new List<MessageData>();
        }

        /// <summary>
        /// Initializes a choice with the given button text and target node name.
        /// </summary>
        public ChoiceData(string text, string target)
        {
            choiceText     = text;
            targetNode     = target;
            playerMessages = new List<MessageData>();
        }
    }

    // ═══════════════════════════════════════
    // DIALOGUE NODE
    // ═══════════════════════════════════════

    [Serializable]
    public class DialogueNode
    {
        public string nodeName;
        public List<MessageData> messages;
        public List<ChoiceData> choices;
        public List<PausePoint> pausePoints;
        public string nextNode;

        /// <summary>
        /// Initializes an empty dialogue node with blank lists and no next node.
        /// </summary>
        public DialogueNode()
        {
            messages    = new List<MessageData>();
            choices     = new List<ChoiceData>();
            pausePoints = new List<PausePoint>();
            nextNode    = "";
        }

        public DialogueNode(string name) : this()
        {
            nodeName = name;
        }

        public bool ShouldPauseAfter(int messageIndex)
        {
            foreach (var p in pausePoints)
                if (p.stopIndex == messageIndex) return true;
            return false;
        }

        /// <summary>
        /// Returns the PausePoint at the given message index, or null if none exists.
        /// </summary>
        public PausePoint GetPauseAt(int messageIndex)
        {
            foreach (var p in pausePoints)
                if (p.stopIndex == messageIndex) return p;
            return null;
        }
    }

    // ═══════════════════════════════════════
    // PAUSE POINT
    // ═══════════════════════════════════════

    /// <summary>
    /// Represents a pause point in a dialogue node.
    /// Stops message flow after <c>stopIndex</c> and optionally pairs with a player message.
    /// </summary>
    [Serializable]
    public class PausePoint
    {
        public int stopIndex;
        public int playerMessageIndex;

        /// <summary>
        /// Returns true if this pause point has a paired player message to emit on continue.
        /// </summary>
        public bool HasPlayerMessage => playerMessageIndex >= 0;

        /// <summary>
        /// Initializes a pause point at the given stop index with an optional paired player message index.
        /// </summary>
        public PausePoint(int stop, int playerMsg = -1)
        {
            stopIndex          = stop;
            playerMessageIndex = playerMsg;
        }
    }

    // ═══════════════════════════════════════
    // RESUME TARGET
    // ═══════════════════════════════════════

    /// <summary>
    /// UI state when exiting a conversation.
    /// Used to determine where to resume when the player returns to the conversation.
    /// </summary>
    /// <param name="None">No resume target; default state.</param>
    /// <param name="Pause">Resume at the most recent pause point.</param>
    /// <param name="Choices">Resume at the most recent choice point.</param>
    /// <param name="End">Resume at the end of the conversation (no more messages).</param>
    /// <param name="Interrupted">Resume at the exact message where the conversation was interrupted.</param>
    public enum ResumeTarget
    {
        None,
        Pause,
        Choices,
        End,
        Interrupted
    }

    // ═══════════════════════════════════════
    // CONVERSATION STATE
    // ═══════════════════════════════════════

    [Serializable]
    public class ConversationState
    {
        public const int CURRENT_VERSION = 2;

        public int version = CURRENT_VERSION;
        public string conversationId;
        public string characterName;
        public int currentChapterIndex;
        public string currentNodeName;
        public int currentMessageIndex;
        public bool isInPauseState;
        public ResumeTarget resumeTarget;
        public List<string> readMessageIds;
        public List<MessageData> messageHistory;
        public List<string> unlockedCGs;

        /// <summary>
        /// Initializes a blank conversation state at chapter 0 with empty history lists.
        /// </summary>
        public ConversationState()
        {
            version             = CURRENT_VERSION;
            conversationId      = "";
            characterName       = "";
            currentChapterIndex = 0;
            currentNodeName     = "";
            currentMessageIndex = 0;
            isInPauseState      = false;
            resumeTarget        = ResumeTarget.None;
            readMessageIds      = new List<string>();
            messageHistory      = new List<MessageData>();
            unlockedCGs         = new List<string>();
        }

        /// <summary>
        /// Initializes a blank conversation state with the given conversation ID.
        /// </summary>
        public ConversationState(string convId) : this()
        {
            conversationId = convId;
        }
    }
}