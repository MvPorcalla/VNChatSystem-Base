// ════════════════════════════════════════════════════════════════════════
// Assets/Scripts/BubbleSpinner/Core/DialogueExecutor.cs
// ════════════════════════════════════════════════════════════════════════

using System;
using System.Collections.Generic;
using UnityEngine;
using BubbleSpinner.Data;

namespace BubbleSpinner.Core
{
    /// <summary>
    /// Executes dialogue nodes and manages conversation flow.
    /// This class is responsible for processing the current dialogue node, 
    /// determining what messages to show, when to show choices, when to pause, and when to jump to other nodes or chapters.
    /// It maintains the current state of the conversation and communicates with the UI via events.
    /// </summary>
    public class DialogueExecutor
    {
        // ═══════════════════════════════════════════════════════════
        // ░ DEPENDENCIES (injected)
        // ═══════════════════════════════════════════════════════════

        private ConversationAsset conversationAsset;
        private ConversationState state;
        private Dictionary<string, DialogueNode> currentNodes;
        private DialogueNode currentNode;
        
        private IBubbleSpinnerCallbacks callbacks;

        // ═══════════════════════════════════════════════════════════
        // ░ EVENTS (UI subscribes to these)
        // ═══════════════════════════════════════════════════════════

        /// <summary>Fired when new messages are ready to display</summary>
        public event Action<List<MessageData>> OnMessagesReady;
        
        /// <summary>Fired when choices should be shown</summary>
        public event Action<List<ChoiceData>> OnChoicesReady;
        
        /// <summary>Fired when pause button should be shown</summary>
        public event Action OnPauseReached;
        
        /// <summary>Fired when conversation ends</summary>
        public event Action OnConversationEnd;
        
        /// <summary>Fired when chapter changes</summary>
        public event Action<string> OnChapterChange;

        // ═══════════════════════════════════════════════════════════
        // ░ PROPERTIES
        // ═══════════════════════════════════════════════════════════

        public bool IsInPauseState => state?.isInPauseState ?? false;
        public string CurrentNodeName => state?.currentNodeName ?? "";
        public int CurrentMessageIndex => state?.currentMessageIndex ?? 0;
        public ConversationState GetState() => state;

        // ═══════════════════════════════════════════════════════════
        // ░ INITIALIZATION
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Initialize executor with conversation asset, state, and callbacks.
        /// ConversationState should be loaded from save or created new before calling this.
        /// </summary>
        public void Initialize(
            ConversationAsset asset, 
            ConversationState conversationState,
            IBubbleSpinnerCallbacks externalCallbacks = null)
        {
            conversationAsset = asset ?? throw new ArgumentNullException(nameof(asset));
            state = conversationState ?? throw new ArgumentNullException(nameof(conversationState));
            callbacks = externalCallbacks;

            ValidateChapterIndex();
            LoadCurrentChapter();
            ValidateState();

            // Set current node
            if (!string.IsNullOrEmpty(state.currentNodeName) && currentNodes.ContainsKey(state.currentNodeName))
            {
                currentNode = currentNodes[state.currentNodeName];
            }

            Debug.Log($"[DialogueExecutor] Initialized: {asset.characterName} | " +
                     $"Chapter: {state.currentChapterIndex} | " +
                     $"Node: {state.currentNodeName} | " +
                     $"Message: {state.currentMessageIndex} | " +
                     $"Paused: {state.isInPauseState}");
        }

        // ═══════════════════════════════════════════════════════════
        // ░ PUBLIC API - MAIN FLOW CONTROL
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Continue from current state (called after loading history)
        /// </summary>
        public void ContinueFromCurrentState()
        {
            if (state == null || currentNodes == null)
            {
                Debug.LogError("[DialogueExecutor] Cannot continue: not initialized");
                return;
            }

            if (!currentNodes.ContainsKey(state.currentNodeName))
            {
                Debug.LogError($"[DialogueExecutor] Node '{state.currentNodeName}' not found");
                return;
            }

            currentNode = currentNodes[state.currentNodeName];

            // If we were paused, show pause button immediately
            if (state.isInPauseState)
            {
                Debug.Log("[DialogueExecutor] Resuming from pause state");
                DetermineNextAction();
            }
            else
            {
                // Normal flow - process current node
                ProcessCurrentNode();
            }
        }

        /// <summary>
        /// Called when pause button is clicked by player
        /// </summary>
        public void OnPauseButtonClicked()
        {
            Debug.Log("[DialogueExecutor] Pause button clicked - continuing dialogue");
            
            state.isInPauseState = false;

            // Instead, check what comes AFTER the pause: choices, jump, or end
            
            // Check if there are more messages AFTER the current pause point
            int nextPauseIndex = GetNextPausePoint(state.currentMessageIndex);
            
            if (nextPauseIndex > state.currentMessageIndex && nextPauseIndex <= currentNode.messages.Count)
            {
                // There are more messages before the next pause/end
                ProcessCurrentNode();
            }
            else
            {
                // No more messages - proceed to choices/jump/end
                DetermineNextActionSkipPause();
            }
        }

        /// <summary>
        /// Determine next action but skip checking the current pause point
        /// </summary>
        private void DetermineNextActionSkipPause()
        {
            Debug.Log($"[DialogueExecutor] Determining next action (skipping current pause) for node: {currentNode.nodeName}");

            // Skip pause check - we just cleared it

            // Priority 1: Check for choices
            if (currentNode.choices != null && currentNode.choices.Count > 0)
            {
                Debug.Log($"[DialogueExecutor] → Showing {currentNode.choices.Count} choices");
                OnChoicesReady?.Invoke(currentNode.choices);
                return;
            }

            // Priority 2: Check for auto-jump
            if (!string.IsNullOrEmpty(currentNode.nextNode))
            {
                Debug.Log($"[DialogueExecutor] → Auto-jump to: {currentNode.nextNode}");
                JumpToNode(currentNode.nextNode);
                return;
            }

            // Priority 3: End of conversation
            Debug.Log("[DialogueExecutor] → End of conversation");
            OnConversationEnd?.Invoke();
        }

        /// <summary>
        /// Get the next pause point after the given index
        /// Returns messages.Count if no more pauses
        /// </summary>
        private int GetNextPausePoint(int afterIndex)
        {
            int nextPause = currentNode.messages.Count;
            
            foreach (int pausePoint in currentNode.pausePoints)
            {
                if (pausePoint > afterIndex)
                {
                    nextPause = pausePoint;
                    break;
                }
            }
            
            return nextPause;
        }

        /// <summary>
        /// Called when player selects a choice
        /// </summary>
        public void OnChoiceSelected(ChoiceData choice)
        {
            Debug.Log($"[DialogueExecutor] Choice selected: {choice.choiceText} -> {choice.targetNode}");

            state.isInPauseState = false;

            // If choice has player messages, queue them first
            if (choice.playerMessages != null && choice.playerMessages.Count > 0)
            {
                Debug.Log($"[DialogueExecutor] Queueing {choice.playerMessages.Count} player messages");
                
                // Add to history
                foreach (var msg in choice.playerMessages)
                {
                    state.messageHistory.Add(msg);
                    state.readMessageIds.Add(msg.messageId);
                }

                // Send to UI for display
                OnMessagesReady?.Invoke(choice.playerMessages);
            }

            // Jump to target node
            JumpToNode(choice.targetNode);
        }

        /// <summary>
        /// Called by UI when messages have finished displaying
        /// </summary>
        public void OnMessagesDisplayComplete()
        {
            Debug.Log("[DialogueExecutor] Messages display complete - determining next action");
            DetermineNextAction();
        }

        // ═══════════════════════════════════════════════════════════
        // ░ CORE PROCESSING LOGIC
        // ═══════════════════════════════════════════════════════════

        private void ProcessCurrentNode()
        {
            if (currentNode == null || state == null)
            {
                Debug.LogError("[DialogueExecutor] Cannot process: invalid state");
                return;
            }

            Debug.Log($"[DialogueExecutor] Processing node: {state.currentNodeName} " +
                     $"(msg {state.currentMessageIndex}/{currentNode.messages.Count})");

            // Get messages to display (from current index to next pause point or end)
            var messagesToShow = GetUnreadMessagesToNextPause();

            if (messagesToShow.Count > 0)
            {
                Debug.Log($"[DialogueExecutor] Queueing {messagesToShow.Count} new messages");

                // Add to history and mark as read
                foreach (var message in messagesToShow)
                {
                    state.messageHistory.Add(message);
                    state.readMessageIds.Add(message.messageId);

                    // Check for CG unlock
                    CheckAndUnlockCG(message);
                }

                // Update message index
                int endIndex = GetEndIndexForNextPause();
                state.currentMessageIndex = endIndex;

                // Send messages to UI
                OnMessagesReady?.Invoke(messagesToShow);

                // UI will call OnMessagesDisplayComplete() when done
            }
            else
            {
                Debug.Log("[DialogueExecutor] No new messages to display");
                DetermineNextAction();
            }
        }

        private void DetermineNextAction()
        {
            Debug.Log($"[DialogueExecutor] Determining next action for node: {currentNode.nodeName}");

            // Priority 1: Check for pause point
            if (currentNode.ShouldPauseAfter(state.currentMessageIndex))
            {
                Debug.Log("[DialogueExecutor] → Pause point reached");
                state.isInPauseState = true;
                OnPauseReached?.Invoke();
                return;
            }

            // Priority 2: Check for choices
            if (currentNode.choices != null && currentNode.choices.Count > 0)
            {
                Debug.Log($"[DialogueExecutor] → Showing {currentNode.choices.Count} choices");
                state.isInPauseState = false;
                OnChoicesReady?.Invoke(currentNode.choices);
                return;
            }

            // Priority 3: Check for auto-jump
            if (!string.IsNullOrEmpty(currentNode.nextNode))
            {
                Debug.Log($"[DialogueExecutor] → Auto-jump to: {currentNode.nextNode}");
                state.isInPauseState = false;
                JumpToNode(currentNode.nextNode);
                return;
            }

            // Priority 4: End of conversation
            Debug.Log("[DialogueExecutor] → End of conversation");
            state.isInPauseState = false;
            OnConversationEnd?.Invoke();
        }

        // ═══════════════════════════════════════════════════════════
        // ░ NODE NAVIGATION
        // ═══════════════════════════════════════════════════════════

        private void JumpToNode(string nodeName)
        {
            Debug.Log($"[DialogueExecutor] Jumping to node: {nodeName}");

            if (currentNodes.ContainsKey(nodeName))
            {
                state.currentNodeName = nodeName;
                state.currentMessageIndex = 0;
                currentNode = currentNodes[nodeName];
                
                ProcessCurrentNode();
            }
            else
            {
                Debug.Log($"[DialogueExecutor] Node '{nodeName}' not found in current chapter - attempting chapter load");
                LoadNextChapter(nodeName);
            }
        }

        private void LoadNextChapter(string targetNode)
        {
            // Check if already at last chapter
            if (state.currentChapterIndex >= conversationAsset.chapters.Count - 1)
            {
                Debug.Log("[DialogueExecutor] Already at last chapter - ending conversation");
                OnConversationEnd?.Invoke();
                return;
            }

            // Move to next chapter
            state.currentChapterIndex++;

            var nextChapter = conversationAsset.chapters[state.currentChapterIndex];
            if (nextChapter == null)
            {
                Debug.LogError($"[DialogueExecutor] Chapter {state.currentChapterIndex} is NULL!");
                OnConversationEnd?.Invoke();
                return;
            }

            Debug.Log($"[DialogueExecutor] Loading chapter {state.currentChapterIndex}");

            // Parse new chapter
            currentNodes = BubbleSpinnerParser.Parse(nextChapter);
    
            if (currentNodes == null || currentNodes.Count == 0)
            {
                Debug.LogError($"[DialogueExecutor] Failed to parse chapter {state.currentChapterIndex}");
                OnConversationEnd?.Invoke();
                return;
            }

            // Notify chapter change
            string chapterName = $"Chapter {state.currentChapterIndex + 1}";
            callbacks?.OnChapterChanged(state.conversationId, state.currentChapterIndex, chapterName);
            
            // Notify UI
            OnChapterChange?.Invoke(chapterName);

            // Jump to target node in new chapter
            if (currentNodes.ContainsKey(targetNode))
            {
                state.currentNodeName = targetNode;
                state.currentMessageIndex = 0;
                currentNode = currentNodes[targetNode];
                ProcessCurrentNode();
            }
            else
            {
                Debug.LogError($"[DialogueExecutor] Node '{targetNode}' not found in chapter {state.currentChapterIndex}");
                OnConversationEnd?.Invoke();
            }
        }

        // ═══════════════════════════════════════════════════════════
        // ░ MESSAGE COLLECTION
        // ═══════════════════════════════════════════════════════════

        private List<MessageData> GetUnreadMessagesToNextPause()
        {
            var unread = new List<MessageData>();
            
            int startIndex = state.currentMessageIndex;
            int endIndex = GetEndIndexForNextPause();

            for (int i = startIndex; i < endIndex; i++)
            {
                var message = currentNode.messages[i];
                
                if (!state.readMessageIds.Contains(message.messageId))
                {
                    unread.Add(message);
                }
            }

            return unread;
        }

        private int GetEndIndexForNextPause()
        {
            int endIndex = currentNode.messages.Count;
            
            foreach (int pausePoint in currentNode.pausePoints)
            {
                if (pausePoint > state.currentMessageIndex)
                {
                    endIndex = pausePoint;
                    break;
                }
            }

            return endIndex;
        }

        // ═══════════════════════════════════════════════════════════
        // ░ CG UNLOCK LOGIC
        // ═══════════════════════════════════════════════════════════

        private void CheckAndUnlockCG(MessageData message)
        {
            if (!message.shouldUnlockCG || string.IsNullOrEmpty(message.imagePath))
                return;

            // Check if already unlocked
            if (state.unlockedCGs.Contains(message.imagePath))
            {
                Debug.Log($"[DialogueExecutor] CG already unlocked: {message.imagePath}");
                return;
            }

            // Unlock the CG
            state.unlockedCGs.Add(message.imagePath);
            Debug.Log($"[DialogueExecutor] 🎨 CG UNLOCKED: {message.imagePath}");

            // Notify external system (e.g. CG gallery) about the unlock
            callbacks?.OnCGUnlocked(message.imagePath);
        }

        // ═══════════════════════════════════════════════════════════
        // ░ VALIDATION
        // ═══════════════════════════════════════════════════════════

        private void ValidateChapterIndex()
        {
            if (state.currentChapterIndex < 0 || 
                state.currentChapterIndex >= conversationAsset.chapters.Count)
            {
                Debug.LogWarning($"[DialogueExecutor] Invalid chapter index {state.currentChapterIndex}, resetting to 0");
                state.currentChapterIndex = 0;
                state.currentMessageIndex = 0;
                state.readMessageIds.Clear();
            }
        }

        private void LoadCurrentChapter()
        {
            var chapter = conversationAsset.chapters[state.currentChapterIndex];
            
            if (chapter == null)
            {
                throw new InvalidOperationException($"Chapter {state.currentChapterIndex} is null!");
            }

            currentNodes = BubbleSpinnerParser.Parse(chapter);
            
            if (currentNodes == null || currentNodes.Count == 0)
            {
                throw new InvalidOperationException($"Failed to parse chapter {state.currentChapterIndex}");
            }

            Debug.Log($"[DialogueExecutor] Loaded chapter {state.currentChapterIndex} with {currentNodes.Count} nodes");
        }

        private void ValidateState()
        {
            // Validate node name
            if (string.IsNullOrEmpty(state.currentNodeName) || 
                !currentNodes.ContainsKey(state.currentNodeName))
            {
                var firstNode = GetFirstNodeName();
                Debug.LogWarning($"[DialogueExecutor] Invalid node '{state.currentNodeName}', resetting to '{firstNode}'");
                state.currentNodeName = firstNode;
                state.currentMessageIndex = 0;
            }

            // Validate message index
            if (currentNodes.ContainsKey(state.currentNodeName))
            {
                var node = currentNodes[state.currentNodeName];
                if (state.currentMessageIndex < 0 || state.currentMessageIndex > node.messages.Count)
                {
                    Debug.LogWarning($"[DialogueExecutor] Invalid message index {state.currentMessageIndex}, resetting to 0");
                    state.currentMessageIndex = 0;
                }
            }
        }

        private string GetFirstNodeName()
        {
            if (currentNodes.ContainsKey("Start"))
                return "Start";
            
            foreach (var key in currentNodes.Keys)
                return key;
            
            return "";
        }
    }
}