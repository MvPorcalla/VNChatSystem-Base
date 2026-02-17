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
        
        public MessageData() 
        {
            messageId = Guid.NewGuid().ToString();
            timestamp = DateTime.Now.ToString("HH:mm");
        }
        
        public MessageData(MessageType msgType, string msgSpeaker, string msgContent, string imgPath = "")
        {
            type = msgType;
            speaker = msgSpeaker;
            content = msgContent;
            imagePath = imgPath;
            timestamp = DateTime.Now.ToString("HH:mm");
            messageId = Guid.NewGuid().ToString();
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
        public List<int> pausePoints;
        public string nextNode;

        public DialogueNode()
        {
            messages = new List<MessageData>();
            choices = new List<ChoiceData>();
            pausePoints = new List<int>();
            nextNode = "";
        }

        public DialogueNode(string name)
        {
            nodeName = name;
            messages = new List<MessageData>();
            choices = new List<ChoiceData>();
            pausePoints = new List<int>();
            nextNode = "";
        }

        public bool ShouldPauseAfter(int messageIndex)
        {
            return pausePoints.Contains(messageIndex);
        }
    }

    // ═══════════════════════════════════════════════════════════
    // ░ CONVERSATION STATE (for saves)
    // ═══════════════════════════════════════════════════════════

    [Serializable]
    public class ConversationState
    {
        public const int CURRENT_VERSION = 1;
        
        public int version = CURRENT_VERSION;
        public string conversationId;
        public string characterName;
        public int currentChapterIndex;
        public string currentNodeName;
        public int currentMessageIndex;
        public bool isInPauseState;
        
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