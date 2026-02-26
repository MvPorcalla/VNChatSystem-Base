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
        private string pendingJumpNode = null;

        private bool pendingProcessAfterPlayerMessage = false;

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

        /// <summary>Returns true if there are more chapters after the current one</summary>
        public bool HasMoreChapters =>
            state != null &&
            conversationAsset != null &&
            state.currentChapterIndex < conversationAsset.chapters.Count - 1;

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
                     $"ResumeTarget: {state.resumeTarget}");
        }

        // ═══════════════════════════════════════════════════════════
        // ░ PUBLIC API - MAIN FLOW CONTROL
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Single public entry point for starting or resuming dialogue after initialization.
        ///
        /// Routes based on saved resumeTarget — the authoritative record of what the
        /// player was looking at when they last exited. This eliminates re-deriving UI
        /// state from node inspection, which was the cause of phantom pause buttons
        /// and incorrect message replays.
        ///
        ///   None        → fresh start → ProcessCurrentNode()
        ///   Pause       → was at a real pause point → fire OnPauseReached immediately
        ///   Interrupted → was mid-messages → fire OnPauseReached (safe resume point)
        ///   Choices     → was at choices → fire OnChoicesReady immediately
        ///   End         → was at end/next-chapter → fire OnConversationEnd immediately
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

            Debug.Log($"[DialogueExecutor] ContinueFromCurrentState: " +
                      $"Node='{state.currentNodeName}' ResumeTarget={state.resumeTarget} " +
                      $"MsgIndex={state.currentMessageIndex}");

            switch (state.resumeTarget)
            {
                case ResumeTarget.Pause:
                    Debug.Log("[DialogueExecutor] Resuming at Pause - firing OnPauseReached");
                    state.isInPauseState = true;
                    OnPauseReached?.Invoke();
                    break;

                case ResumeTarget.Interrupted:
                    // Mid-sequence exit: messages may have fully saved to history even though
                    // the timing animation hadn't finished. Check if there are actually unread
                    // messages remaining — if not, determine next action directly (choices/end)
                    // rather than showing a phantom pause button.
                    Debug.Log("[DialogueExecutor] Resuming at Interrupted - checking for unread messages");
                    var unreadOnResume = GetUnreadMessagesToNextPause();
                    if (unreadOnResume.Count > 0)
                    {
                        Debug.Log($"[DialogueExecutor] {unreadOnResume.Count} unread messages remain - showing pause button");
                        state.isInPauseState = true;
                        OnPauseReached?.Invoke();
                    }
                    else
                    {
                        Debug.Log("[DialogueExecutor] No unread messages - determining next action directly");
                        state.isInPauseState = false;
                        state.resumeTarget = ResumeTarget.None;
                        // DetermineNextActionSkipPause instead of DetermineNextAction here —
                        // currentMessageIndex is sitting on a pause point when interrupted,
                        // so DetermineNextAction would see it and fire OnPauseReached again,
                        // showing a phantom continue button on resume.
                        DetermineNextActionSkipPause();
                    }
                    break;

                case ResumeTarget.Choices:
                    // Player was at choices — show them directly, no message processing needed.
                    Debug.Log("[DialogueExecutor] Resuming at choices - firing OnChoicesReady");
                    state.isInPauseState = false;
                    OnChoicesReady?.Invoke(currentNode.choices);
                    break;

                case ResumeTarget.End:
                    // Player was at the end/next-chapter button — restore it directly.
                    Debug.Log("[DialogueExecutor] Resuming at end - firing OnConversationEnd");
                    state.isInPauseState = false;
                    OnConversationEnd?.Invoke();
                    break;

                case ResumeTarget.None:
                default:
                    // Fresh start or legacy save (version 1) with no resumeTarget recorded.
                    Debug.Log("[DialogueExecutor] Fresh start or legacy save - processing node normally");
                    ProcessCurrentNode();
                    break;
            }
        }

        /// <summary>
        /// Called by UI when player clicks the pause/continue button.
        /// </summary>
        public void OnPauseButtonClicked()
        {
            Debug.Log("[DialogueExecutor] Pause button clicked - continuing dialogue");

            state.isInPauseState = false;
            state.resumeTarget = ResumeTarget.None;

            // Check if the current pause point has a paired player message.
            // If so, emit it first and wait for OnMessagesDisplayComplete
            // before continuing to the next NPC batch.
            var pausePoint = currentNode.GetPauseAt(state.currentMessageIndex);

            if (pausePoint != null && pausePoint.HasPlayerMessage)
            {
                var playerMessage = currentNode.messages[pausePoint.playerMessageIndex];

                Debug.Log($"[DialogueExecutor] Player-turn pause — emitting player message: '{playerMessage.content}'");

                // Mark as read and add to history
                state.messageHistory.Add(playerMessage);
                state.readMessageIds.Add(playerMessage.messageId);

                // Advance index past the player message so ProcessCurrentNode
                // picks up from the next NPC line after OnMessagesDisplayComplete
                state.currentMessageIndex = pausePoint.playerMessageIndex + 1;
                pendingProcessAfterPlayerMessage = true;

                OnMessagesReady?.Invoke(new List<MessageData> { playerMessage });
                return;
            }

            // No paired player message — pure pacing pause, continue directly
            var remainingMessages = GetUnreadMessagesToNextPause();

            if (remainingMessages.Count > 0)
            {
                Debug.Log($"[DialogueExecutor] {remainingMessages.Count} unread messages remaining - processing node");
                ProcessCurrentNode();
            }
            else
            {
                Debug.Log("[DialogueExecutor] No unread messages remaining - determining next action");
                DetermineNextActionSkipPause();
            }
        }

        /// <summary>
        /// Called by UI when conversation is exited mid-message sequence.
        /// Sets resumeTarget to Interrupted so re-entry shows the continue button.
        /// </summary>
        public void NotifyInterrupted()
        {
            if (state == null) return;

            Debug.Log("[DialogueExecutor] Conversation interrupted - setting ResumeTarget.Interrupted");
            state.isInPauseState = true;
            state.resumeTarget = ResumeTarget.Interrupted;
        }

        /// <summary>
        /// Called by UI when a choice is selected.
        /// </summary>
        public void OnChoiceSelected(ChoiceData choice)
        {
            Debug.Log($"[DialogueExecutor] Choice selected: {choice.choiceText} -> {choice.targetNode}");

            state.isInPauseState = false;
            state.resumeTarget = ResumeTarget.None;

            if (choice.playerMessages != null && choice.playerMessages.Count > 0)
            {
                Debug.Log($"[DialogueExecutor] Queueing {choice.playerMessages.Count} player messages");

                foreach (var msg in choice.playerMessages)
                {
                    state.messageHistory.Add(msg);
                    state.readMessageIds.Add(msg.messageId);
                }

                pendingJumpNode = choice.targetNode;
                OnMessagesReady?.Invoke(choice.playerMessages);
            }
            else
            {
                JumpToNode(choice.targetNode);
            }
        }

        /// <summary>
        /// Called by UI when messages have finished displaying (including after a choice's player messages).
        /// Checks if there is a pending jump from a choice selection that needs to be processed, otherwise
        /// </summary>
        public void OnMessagesDisplayComplete()
        {
            Debug.Log("[DialogueExecutor] Messages display complete - determining next action");

            if (!string.IsNullOrEmpty(pendingJumpNode))
            {
                string jumpTarget = pendingJumpNode;
                pendingJumpNode = null;
                JumpToNode(jumpTarget);
                return;
            }

            // If we just emitted a paired player message from a pause point,
            // continue processing the node to show the next NPC batch.
            // DetermineNextAction would skip ProcessCurrentNode and go straight
            // to choices/jump/end, causing the following NPC messages to be lost.
            if (pendingProcessAfterPlayerMessage)
            {
                pendingProcessAfterPlayerMessage = false;
                Debug.Log("[DialogueExecutor] Post-player-message — resuming NPC batch");
                ProcessCurrentNode();
                return;
            }

            DetermineNextAction();
        }

        /// <summary>
        /// Called to advance to the next chapter (e.g. from a "Next Chapter" button).
        /// </summary>
        public void AdvanceToNextChapter()
        {
            Debug.Log("[DialogueExecutor] AdvanceToNextChapter called");
            state.resumeTarget = ResumeTarget.None;
            LoadNextChapter("Start");
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

            var messagesToShow = GetUnreadMessagesToNextPause();

            if (messagesToShow.Count > 0)
            {
                Debug.Log($"[DialogueExecutor] Queueing {messagesToShow.Count} new messages");

                foreach (var message in messagesToShow)
                {
                    state.messageHistory.Add(message);
                    state.readMessageIds.Add(message.messageId);
                    CheckAndUnlockCG(message);
                }

                int endIndex = GetEndIndexForNextPause();
                state.currentMessageIndex = endIndex;

                OnMessagesReady?.Invoke(messagesToShow);
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
                // A pause point is always real — either it has a paired player message,
                // or it has NPC messages after it, or it's a standalone pacing pause.
                // All three cases should show the continue button.
                // The only exception is a pure trailing pause with no player message
                // and no NPC messages after it — that falls through to choices/end.
                var pausePoint = currentNode.GetPauseAt(state.currentMessageIndex);
                bool hasContentAfterPause = pausePoint.HasPlayerMessage ||
                    GetEndIndexForNextPause(state.currentMessageIndex + 1) > state.currentMessageIndex + 1 ||
                    state.currentMessageIndex + 1 < currentNode.messages.Count;

                if (hasContentAfterPause)
                {
                    Debug.Log("[DialogueExecutor] → Pause point reached");
                    state.isInPauseState = true;
                    state.resumeTarget = ResumeTarget.Pause;
                    OnPauseReached?.Invoke();
                    return;
                }

                Debug.Log("[DialogueExecutor] → Trailing pause with no content after — falling through to choices/end");
            }

            // Priority 2: Check for choices
            if (currentNode.choices != null && currentNode.choices.Count > 0)
            {
                Debug.Log($"[DialogueExecutor] → Showing {currentNode.choices.Count} choices");
                state.isInPauseState = false;
                state.resumeTarget = ResumeTarget.Choices;
                OnChoicesReady?.Invoke(currentNode.choices);
                return;
            }

            // Priority 3: Check for auto-jump
            if (!string.IsNullOrEmpty(currentNode.nextNode))
            {
                Debug.Log($"[DialogueExecutor] → Auto-jump to: {currentNode.nextNode}");
                state.isInPauseState = false;
                state.resumeTarget = ResumeTarget.None;
                JumpToNode(currentNode.nextNode);
                return;
            }

            // Priority 4: End of conversation
            Debug.Log("[DialogueExecutor] → End of conversation");
            state.isInPauseState = false;
            state.resumeTarget = ResumeTarget.End;
            OnConversationEnd?.Invoke();
        }

        /// <summary>
        /// Determine next action skipping the current pause point.
        /// Called after the player clicks the pause/continue button.
        /// </summary>
        private void DetermineNextActionSkipPause()
        {
            Debug.Log($"[DialogueExecutor] Determining next action (skipping pause) for node: {currentNode.nodeName}");

            // Priority 1: Check for choices
            if (currentNode.choices != null && currentNode.choices.Count > 0)
            {
                Debug.Log($"[DialogueExecutor] → Showing {currentNode.choices.Count} choices");
                state.resumeTarget = ResumeTarget.Choices;
                OnChoicesReady?.Invoke(currentNode.choices);
                return;
            }

            // Priority 2: Check for auto-jump
            if (!string.IsNullOrEmpty(currentNode.nextNode))
            {
                Debug.Log($"[DialogueExecutor] → Auto-jump to: {currentNode.nextNode}");
                state.resumeTarget = ResumeTarget.None;
                JumpToNode(currentNode.nextNode);
                return;
            }

            // Priority 3: End of conversation
            Debug.Log("[DialogueExecutor] → End of conversation");
            state.resumeTarget = ResumeTarget.End;
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
            if (state.currentChapterIndex >= conversationAsset.chapters.Count - 1)
            {
                Debug.Log("[DialogueExecutor] Already at last chapter - ending conversation");
                state.resumeTarget = ResumeTarget.End;
                OnConversationEnd?.Invoke();
                return;
            }

            state.currentChapterIndex++;

            var nextChapter = conversationAsset.chapters[state.currentChapterIndex];
            if (nextChapter == null)
            {
                Debug.LogError($"[DialogueExecutor] Chapter {state.currentChapterIndex} is NULL!");
                state.resumeTarget = ResumeTarget.End;
                OnConversationEnd?.Invoke();
                return;
            }

            Debug.Log($"[DialogueExecutor] Loading chapter {state.currentChapterIndex}");

            currentNodes = BubbleSpinnerParser.Parse(nextChapter, conversationAsset.characterName);

            if (currentNodes == null || currentNodes.Count == 0)
            {
                Debug.LogError($"[DialogueExecutor] Failed to parse chapter {state.currentChapterIndex}");
                state.resumeTarget = ResumeTarget.End;
                OnConversationEnd?.Invoke();
                return;
            }

            string chapterName = $"Chapter {state.currentChapterIndex + 1}";
            callbacks?.OnChapterChanged(state.conversationId, state.currentChapterIndex, chapterName);
            OnChapterChange?.Invoke(chapterName);

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
                state.resumeTarget = ResumeTarget.End;
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

            foreach (var pausePoint in currentNode.pausePoints)
            {
                if (pausePoint.stopIndex > state.currentMessageIndex)
                {
                    endIndex = pausePoint.stopIndex;
                    break;
                }
            }

            return endIndex;
        }

        private int GetEndIndexForNextPause(int fromIndex)
        {
            int endIndex = currentNode.messages.Count;

            foreach (var pausePoint in currentNode.pausePoints)
            {
                if (pausePoint.stopIndex > fromIndex)
                {
                    endIndex = pausePoint.stopIndex;
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

            if (state.unlockedCGs.Contains(message.imagePath))
            {
                Debug.Log($"[DialogueExecutor] CG already unlocked: {message.imagePath}");
                return;
            }

            state.unlockedCGs.Add(message.imagePath);
            Debug.Log($"[DialogueExecutor] 🎨 CG UNLOCKED: {message.imagePath}");

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
                throw new InvalidOperationException($"Chapter {state.currentChapterIndex} is null!");

            currentNodes = BubbleSpinnerParser.Parse(chapter, conversationAsset.characterName);

            if (currentNodes == null || currentNodes.Count == 0)
                throw new InvalidOperationException($"Failed to parse chapter {state.currentChapterIndex}");

            Debug.Log($"[DialogueExecutor] Loaded chapter {state.currentChapterIndex} with {currentNodes.Count} nodes");

            foreach (var kvp in currentNodes)
            {
                Debug.Log($"[DEBUG] Node '{kvp.Key}': {kvp.Value.messages.Count} messages, " +
                        $"pausePoints=[{string.Join(",", kvp.Value.pausePoints.ConvertAll(p => $"{p.stopIndex}(pm:{p.playerMessageIndex})"))}]");
            }
        }

        private void ValidateState()
        {
            if (string.IsNullOrEmpty(state.currentNodeName) ||
                !currentNodes.ContainsKey(state.currentNodeName))
            {
                var firstNode = GetFirstNodeName();
                Debug.LogWarning($"[DialogueExecutor] Invalid node '{state.currentNodeName}', resetting to '{firstNode}'");
                state.currentNodeName = firstNode;
                state.currentMessageIndex = 0;
                state.resumeTarget = ResumeTarget.None;
            }

            if (currentNodes.ContainsKey(state.currentNodeName))
            {
                var node = currentNodes[state.currentNodeName];
                if (state.currentMessageIndex < 0 || state.currentMessageIndex > node.messages.Count)
                {
                    Debug.LogWarning($"[DialogueExecutor] Invalid message index {state.currentMessageIndex}, resetting to 0");
                    state.currentMessageIndex = 0;
                    state.resumeTarget = ResumeTarget.None;
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