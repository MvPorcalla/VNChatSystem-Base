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
        // DEPENDENCIES (injected)
        // ═══════════════════════════════════════════════════════════

        private ConversationAsset conversationAsset;
        private ConversationState state;
        private Dictionary<string, DialogueNode> currentNodes;
        private DialogueNode currentNode;

        private IBubbleSpinnerCallbacks callbacks;
        private string pendingJumpNode = null;

        private bool pendingProcessAfterPlayerMessage = false;

        // ═══════════════════════════════════════════════════════════
        // EVENTS (UI subscribes to these)
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
        // PROPERTIES
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
        // INITIALIZATION
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

            if (!string.IsNullOrEmpty(state.currentNodeName) && currentNodes.ContainsKey(state.currentNodeName))
            {
                currentNode = currentNodes[state.currentNodeName];
            }
        }

        // ═══════════════════════════════════════════════════════════
        // PUBLIC API - MAIN FLOW CONTROL
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Single public entry point for starting or resuming dialogue after initialization.
        /// Determines where to resume based on the state's resumeTarget and current node/message indices.
        /// </summary>
        public void ContinueFromCurrentState()
        {
            if (state == null || currentNodes == null)
            {
                BSDebug.LogError("[DialogueExecutor] Cannot continue: not initialized");
                return;
            }

            if (!currentNodes.ContainsKey(state.currentNodeName))
            {
                BSDebug.LogError($"[DialogueExecutor] Node '{state.currentNodeName}' not found");
                return;
            }

            currentNode = currentNodes[state.currentNodeName];

            switch (state.resumeTarget)
            {
                case ResumeTarget.Pause:
                    state.isInPauseState = true;
                    OnPauseReached?.Invoke();
                    break;

                case ResumeTarget.Interrupted:
                    // Player exited mid-message sequence — show continue button if there are unread messages, otherwise determine next action directly.
                    var unreadOnResume = GetUnreadMessagesToNextPause();
                    if (unreadOnResume.Count > 0)
                    {
                        state.isInPauseState = true;
                        OnPauseReached?.Invoke();
                    }
                    else
                    {
                        state.isInPauseState = false;
                        state.resumeTarget = ResumeTarget.None;

                        DetermineNextActionSkipPause();
                    }
                    break;

                case ResumeTarget.Choices:
                    // Player was at choices — show them directly, no message processing needed.
                    state.isInPauseState = false;
                    OnChoicesReady?.Invoke(currentNode.choices);
                    break;

                case ResumeTarget.End:
                    // Player was at the end/next-chapter button — restore it directly.
                    state.isInPauseState = false;
                    OnConversationEnd?.Invoke();
                    break;

                case ResumeTarget.None:
                default:
                    // Fresh start or legacy save (version 1) with no resumeTarget recorded.
                    ProcessCurrentNode();
                    break;
            }
        }

        /// <summary>
        /// Called by UI when player clicks the pause/continue button at a pause point.
        /// </summary>
        public void OnPauseButtonClicked()
        {
            state.isInPauseState = false;
            state.resumeTarget = ResumeTarget.None;

            var pausePoint = currentNode.GetPauseAt(state.currentMessageIndex);

            if (pausePoint != null && pausePoint.HasPlayerMessage)
            {
                var playerMessage = currentNode.messages[pausePoint.playerMessageIndex];

                BSDebug.Log($"[DialogueExecutor] Player-turn pause — emitting player message: '{playerMessage.content}'");

                // Emit the paired player message for this pause point, 
                // then continue processing the node to show the next NPC batch.
                state.messageHistory.Add(playerMessage);
                state.readMessageIds.Add(playerMessage.messageId);
                state.currentMessageIndex = pausePoint.playerMessageIndex + 1;
                pendingProcessAfterPlayerMessage = true;

                OnMessagesReady?.Invoke(new List<MessageData> { playerMessage });
                return;
            }

            // No paired player message — pure pacing pause, continue directly
            var remainingMessages = GetUnreadMessagesToNextPause();

            if (remainingMessages.Count > 0)
            {
                ProcessCurrentNode();
            }
            else
            {
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

            BSDebug.Log("[DialogueExecutor] Conversation interrupted - setting ResumeTarget.Interrupted");
            state.isInPauseState = true;
            state.resumeTarget = ResumeTarget.Interrupted;
        }

        /// <summary>
        /// Called by UI when a choice is selected.
        /// </summary>
        public void OnChoiceSelected(ChoiceData choice)
        {
            BSDebug.Log($"[DialogueExecutor] Choice selected: {choice.choiceText} -> {choice.targetNode}");

            state.isInPauseState = false;
            state.resumeTarget = ResumeTarget.None;

            if (choice.playerMessages != null && choice.playerMessages.Count > 0)
            {
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
        /// Checks in priority order:
        /// 1) If a pending jump exists from a choice selection, executes it
        /// 2) If a player message was just displayed, resumes NPC processing
        /// 3) Otherwise determines the next dialogue action normally
        /// </summary>
        public void OnMessagesDisplayComplete()
        {
            if (!string.IsNullOrEmpty(pendingJumpNode))
            {
                string jumpTarget = pendingJumpNode;
                pendingJumpNode = null;
                JumpToNode(jumpTarget);
                return;
            }

            // If we just finished displaying a player message from a choice, we may need to resume the NPC batch that follows it.
            if (pendingProcessAfterPlayerMessage)
            {
                pendingProcessAfterPlayerMessage = false;
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
            BSDebug.Log("[DialogueExecutor] AdvanceToNextChapter called");
            state.resumeTarget = ResumeTarget.None;
            LoadNextChapter("Start");
        }

        // ═══════════════════════════════════════════════════════════
        // CORE PROCESSING LOGIC
        // ═══════════════════════════════════════════════════════════

        private void ProcessCurrentNode()
        {
            if (currentNode == null || state == null)
            {
                BSDebug.LogError("[DialogueExecutor] Cannot process: invalid state");
                return;
            }

            var messagesToShow = GetUnreadMessagesToNextPause();

            if (messagesToShow.Count > 0)
            {
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
                DetermineNextAction();
            }
        }

        /// <summary>
        /// Returns true if there is meaningful content remaining after a pause point —
        /// either a paired player message, more messages in the current batch,
        /// or remaining messages in the node.
        /// Prevents showing a continue button at the very end of a node where nothing follows.
        /// </summary>
        private bool HasContentAfterPause(PausePoint pausePoint)
        {
            if (pausePoint.HasPlayerMessage)
                return true;

            int nextIndex = state.currentMessageIndex + 1;

            if (GetEndIndexForNextPause(nextIndex) > nextIndex)
                return true;

            if (nextIndex < currentNode.messages.Count)
                return true;

            return false;
        }

        private void DetermineNextAction()
        {
            if (currentNode.ShouldPauseAfter(state.currentMessageIndex))
            {
                var pausePoint = currentNode.GetPauseAt(state.currentMessageIndex);

                if (HasContentAfterPause(pausePoint))
                {
                    state.isInPauseState = true;
                    state.resumeTarget = ResumeTarget.Pause;
                    OnPauseReached?.Invoke();
                    return;
                }
            }

            if (currentNode.choices != null && currentNode.choices.Count > 0)
            {
                state.isInPauseState = false;
                state.resumeTarget = ResumeTarget.Choices;
                OnChoicesReady?.Invoke(currentNode.choices);
                return;
            }

            if (!string.IsNullOrEmpty(currentNode.nextNode))
            {
                state.isInPauseState = false;
                state.resumeTarget = ResumeTarget.None;
                JumpToNode(currentNode.nextNode);
                return;
            }

            state.isInPauseState = false;
            state.resumeTarget = ResumeTarget.End;
            OnConversationEnd?.Invoke();
        }

        /// <summary>
        /// Determines next action while skipping pause points. 
        /// Used when resuming from Interrupted state or when player clicks continue without a paired player message.
        /// Priority order is the same as DetermineNextAction but without the pause check:
        /// 1) If there are choices, show them
        /// 2) Else if there's an auto-jump, jump to the target node
        /// 3) Else end the conversation (or next chapter)
        /// Note: This method does not modify the state.resumeTarget 
        /// since it's only used when resuming from a known state where we want to skip directly to the next actionable step.
        /// </summary>
        private void DetermineNextActionSkipPause()
        {
            if (currentNode.choices != null && currentNode.choices.Count > 0)
            {
                state.resumeTarget = ResumeTarget.Choices;
                OnChoicesReady?.Invoke(currentNode.choices);
                return;
            }

            if (!string.IsNullOrEmpty(currentNode.nextNode))
            {
                state.resumeTarget = ResumeTarget.None;
                JumpToNode(currentNode.nextNode);
                return;
            }

            state.resumeTarget = ResumeTarget.End;
            OnConversationEnd?.Invoke();
        }

        // ═══════════════════════════════════════════════════════════
        // NODE NAVIGATION
        // ═══════════════════════════════════════════════════════════

        private void JumpToNode(string nodeName)
        {
            BSDebug.Log($"[DialogueExecutor] Jumping to node: {nodeName}");

            if (currentNodes.ContainsKey(nodeName))
            {
                state.currentNodeName = nodeName;
                state.currentMessageIndex = 0;
                currentNode = currentNodes[nodeName];
                ProcessCurrentNode();
            }
            else
            {
                BSDebug.Log($"[DialogueExecutor] Node '{nodeName}' not found in current chapter - attempting chapter load");
                LoadNextChapter(nodeName);
            }
        }

        private void LoadNextChapter(string targetNode)
        {
            if (state.currentChapterIndex >= conversationAsset.chapters.Count - 1)
            {
                BSDebug.Log("[DialogueExecutor] Already at last chapter - ending conversation");
                state.resumeTarget = ResumeTarget.End;
                OnConversationEnd?.Invoke();
                return;
            }

            state.currentChapterIndex++;

            var nextChapter = conversationAsset.chapters[state.currentChapterIndex];
            if (nextChapter == null)
            {
                BSDebug.LogError($"[DialogueExecutor] Chapter {state.currentChapterIndex} is NULL!");
                state.resumeTarget = ResumeTarget.End;
                OnConversationEnd?.Invoke();
                return;
            }

            BSDebug.Log($"[DialogueExecutor] Loading chapter {state.currentChapterIndex}");

            currentNodes = BubbleSpinnerParser.Parse(nextChapter, conversationAsset.characterName);

            if (currentNodes == null || currentNodes.Count == 0)
            {
                BSDebug.LogError($"[DialogueExecutor] Failed to parse chapter {state.currentChapterIndex}");
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
                BSDebug.LogError($"[DialogueExecutor] Node '{targetNode}' not found in chapter {state.currentChapterIndex}");
                state.resumeTarget = ResumeTarget.End;
                OnConversationEnd?.Invoke();
            }
        }

        // ═══════════════════════════════════════════════════════════
        // MESSAGE COLLECTION
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

        /// <summary>
        /// Returns the index at which the next pause point stops message processing.
        /// If no pause point exists beyond fromIndex, returns the total message count.
        /// Defaults to state.currentMessageIndex when no fromIndex is provided.
        /// </summary>
        private int GetEndIndexForNextPause(int fromIndex = -1)
        {
            int startFrom = fromIndex >= 0 ? fromIndex : state.currentMessageIndex;
            int endIndex = currentNode.messages.Count;

            foreach (var pausePoint in currentNode.pausePoints)
            {
                if (pausePoint.stopIndex > startFrom)
                {
                    endIndex = pausePoint.stopIndex;
                    break;
                }
            }

            return endIndex;
        }

        // ═══════════════════════════════════════════════════════════
        // CG UNLOCK LOGIC
        // ═══════════════════════════════════════════════════════════

        private void CheckAndUnlockCG(MessageData message)
        {
            if (!message.shouldUnlockCG || string.IsNullOrEmpty(message.imagePath))
                return;

            if (state.unlockedCGs.Contains(message.imagePath))
            {
                BSDebug.Log($"[DialogueExecutor] CG already unlocked: {message.imagePath}");
                return;
            }

            state.unlockedCGs.Add(message.imagePath);
            BSDebug.Log($"[DialogueExecutor] CG UNLOCKED: {message.imagePath}");

            callbacks?.OnCGUnlocked(message.imagePath);
        }

        // ═══════════════════════════════════════════════════════════
        // VALIDATION
        // ═══════════════════════════════════════════════════════════

        private void ValidateChapterIndex()
        {
            if (state.currentChapterIndex < 0 ||
                state.currentChapterIndex >= conversationAsset.chapters.Count)
            {
                BSDebug.LogWarning($"[DialogueExecutor] Invalid chapter index {state.currentChapterIndex}, resetting to 0");
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

            BSDebug.Log($"[DialogueExecutor] Loaded chapter {state.currentChapterIndex} with {currentNodes.Count} nodes");
        }

        private void ValidateState()
        {
            if (string.IsNullOrEmpty(state.currentNodeName) ||
                !currentNodes.ContainsKey(state.currentNodeName))
            {
                var firstNode = GetFirstNodeName();
                BSDebug.LogWarning($"[DialogueExecutor] Invalid node '{state.currentNodeName}', resetting to '{firstNode}'");
                state.currentNodeName = firstNode;
                state.currentMessageIndex = 0;
                state.resumeTarget = ResumeTarget.None;
            }

            if (currentNodes.ContainsKey(state.currentNodeName))
            {
                var node = currentNodes[state.currentNodeName];
                if (state.currentMessageIndex < 0 || state.currentMessageIndex > node.messages.Count)
                {
                    BSDebug.LogWarning($"[DialogueExecutor] Invalid message index {state.currentMessageIndex}, resetting to 0");
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