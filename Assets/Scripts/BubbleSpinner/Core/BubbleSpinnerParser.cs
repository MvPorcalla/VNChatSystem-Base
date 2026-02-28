// ════════════════════════════════════════════════════════════════════════
// Assets/Scripts/BubbleSpinner/Core/BubbleSpinnerParser.cs
// ════════════════════════════════════════════════════════════════════════

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using BubbleSpinner.Data;

namespace BubbleSpinner.Core
{
    /// <summary>
    /// Parses .bub dialogue files into a structured format of DialogueNodes, Messages, and Choices.
    /// Supports dialogue lines, media commands, jump commands, pause points, and choice blocks.
    /// </summary>
    public static class BubbleSpinnerParser
    {
        private class ParserContext
        {
            public DialogueNode currentNode;
            public ChoiceData currentChoice;
            public bool inChoiceBlock;
            public bool processingChoiceContent;
            public int lineNumber;
            public string fileName;
        }

        // Regex pattern to identify cross-chapter jump targets (e.g. "_ch2", "_Ch12", "_CH3").
        private static readonly Regex CrossChapterPattern =
            new Regex(@"_ch\d+", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        // ═══════════════════════════════════════════════════════════
        // PUBLIC API
        // ═══════════════════════════════════════════════════════════

        public static Dictionary<string, DialogueNode> Parse(TextAsset bubbleFile, string expectedCharacterName = "")
        {
            var nodes = new Dictionary<string, DialogueNode>();

            if (bubbleFile == null)
            {
                BSDebug.LogError("[BubbleSpinner] Null .bub file provided");
                return nodes;
            }

            BSDebug.Log($"[BubbleSpinner] Parsing: {bubbleFile.name}");

            string[] lines = bubbleFile.text.Split('\n');
            var context = new ParserContext { fileName = bubbleFile.name };

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                context.lineNumber = i + 1;

                try
                {
                    line = StripInlineComments(line);

                    if (string.IsNullOrEmpty(line) || line.StartsWith("//"))
                        continue;

                    if (line.StartsWith("contact:"))
                    {
                        if (!string.IsNullOrEmpty(expectedCharacterName))
                        {
                            string contactName = line.Substring(8).Trim();
                            if (!string.IsNullOrEmpty(contactName) &&
                                !contactName.Equals(expectedCharacterName, StringComparison.OrdinalIgnoreCase))
                            {
                                BSDebug.LogWarning($"[BubbleSpinner] [{context.fileName}] contact: mismatch! " +
                                    $"File says '{contactName}' but asset expects '{expectedCharacterName}'");
                            }
                        }
                        continue;
                    }

                    if (TryParseNodeTitle(line, context, nodes)) continue;
                    if (line == "---" || line == "===") continue;

                    if (context.currentNode == null)
                    {
                        BSDebug.LogWarning($"[BubbleSpinner] [{context.fileName}:{context.lineNumber}] Content outside node: {line}");
                        continue;
                    }

                    if (TryParseJumpCommand(line, context)) continue;
                    if (TryParsePauseButton(line, context, lines, i)) continue;
                    if (TryParseChoiceBlockStart(line, context)) continue;
                    if (TryParseChoiceBlockEnd(line, context)) continue;
                    if (TryParseChoiceOption(line, context)) continue;
                    if (TryParseMediaCommand(line, context)) continue;
                    if (TryParseDialogueLine(line, context)) continue;

                    BSDebug.LogWarning($"[BubbleSpinner] [{context.fileName}:{context.lineNumber}] Unrecognized: {line}");
                }
                catch (Exception ex)
                {
                    BSDebug.LogError($"[BubbleSpinner] [{context.fileName}:{context.lineNumber}] Parse error: {line}\n{ex.Message}");
                }
            }

            FinalizeParser(context, nodes);
            ValidateDialogueGraph(nodes, bubbleFile.name);
            AssignNodeMessageIds(nodes);

            BSDebug.Log($"[BubbleSpinner] Parsed {nodes.Count} nodes from {bubbleFile.name}");
            return nodes;
        }

        // ═══════════════════════════════════════════════════════════
        // DETERMINISTIC MESSAGE ID ASSIGNMENT (PHASE 1 FIX #5)
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Assigns deterministic messageIds to all messages after the full parse is complete.
        /// Format: "{nodeName}_{messageIndex}"
        /// Player messages inside choices: "{nodeName}_choice{choiceIndex}_player{messageIndex}"
        /// Called once per Parse() invocation after all nodes are finalized.
        /// </summary>
        private static void AssignNodeMessageIds(Dictionary<string, DialogueNode> nodes)
        {
            foreach (var kvp in nodes)
            {
                var node = kvp.Value;
                string nodeName = node.nodeName;

                for (int i = 0; i < node.messages.Count; i++)
                {
                    node.messages[i].messageId = $"{nodeName}_{i}";
                }

                for (int c = 0; c < node.choices.Count; c++)
                {
                    var choice = node.choices[c];
                    for (int p = 0; p < choice.playerMessages.Count; p++)
                    {
                        choice.playerMessages[p].messageId = $"{nodeName}_choice{c}_player{p}";
                    }
                }
            }
        }

        // ═══════════════════════════════════════════════════════════
        // PARSING METHODS
        // ═══════════════════════════════════════════════════════════

        private static string StripInlineComments(string line)
        {
            int commentIndex = line.IndexOf("//");
            return commentIndex >= 0 ? line.Substring(0, commentIndex).Trim() : line;
        }

        private static bool TryParseNodeTitle(string line, ParserContext ctx, Dictionary<string, DialogueNode> nodes)
        {
            if (!line.StartsWith("title:"))
                return false;

            if (ctx.currentNode != null)
            {
                FinalizeCurrentNode(ctx, nodes);
            }

            string nodeName = line.Substring(6).Trim();

            if (string.IsNullOrEmpty(nodeName))
            {
                BSDebug.LogWarning($"[BubbleSpinner] [{ctx.fileName}:{ctx.lineNumber}] Empty node name");
                return true;
            }

            if (nodes.ContainsKey(nodeName))
            {
                BSDebug.LogWarning($"[BubbleSpinner] [{ctx.fileName}:{ctx.lineNumber}] Duplicate node '{nodeName}'");
            }

            ctx.currentNode = new DialogueNode(nodeName);
            ctx.inChoiceBlock = false;
            ctx.processingChoiceContent = false;
            ctx.currentChoice = null;

            return true;
        }

        private static bool TryParseJumpCommand(string line, ParserContext ctx)
        {
            if (!line.StartsWith("<<jump") || !line.EndsWith(">>"))
                return false;

            string jumpTarget = line.Substring(6, line.Length - 8).Trim();

            if (string.IsNullOrEmpty(jumpTarget))
            {
                BSDebug.LogWarning($"[BubbleSpinner] [{ctx.fileName}:{ctx.lineNumber}] Empty jump target");
                return true;
            }

            if (ctx.processingChoiceContent && ctx.currentChoice != null)
            {
                ctx.currentChoice.targetNode = jumpTarget;
            }
            else
            {
                ctx.currentNode.nextNode = jumpTarget;
            }

            return true;
        }

        private static bool TryParsePauseButton(string line, ParserContext ctx, string[] lines, int currentLineIndex)
        {
            if (line != "-> ...")
                return false;

            if (ctx.processingChoiceContent)
            {
                BSDebug.LogWarning($"[BubbleSpinner] [{ctx.fileName}:{ctx.lineNumber}] Pause button inside choice block ignored");
                return true;
            }

            int stopIndex = ctx.currentNode.messages.Count;

            // Look ahead to find the next non-empty, non-comment line.
            // If it is a Player: line (contains ":" and does not start with
            // a command prefix), the pause is a player-turn pause and we
            // record the index where that player message will land.
            // That index is messages.Count + 0 because the Player: line
            // will be parsed on the very next iteration and appended at
            // messages.Count — which equals stopIndex right now.
            int playerMessageIndex = -1;

            for (int peek = currentLineIndex + 1; peek < lines.Length; peek++)
            {
                string peekLine = StripInlineComments(lines[peek].Trim());

                if (string.IsNullOrEmpty(peekLine) || peekLine.StartsWith("//"))
                    continue;

                // Stop peeking at any command or node boundary
                if (peekLine.StartsWith(">>") ||
                    peekLine.StartsWith("<<") ||
                    peekLine.StartsWith("->") ||
                    peekLine.StartsWith("title:") ||
                    peekLine == "---" ||
                    peekLine == "===")
                    break;

                // Check if this is a Player: line
                if (peekLine.Contains(":"))
                {
                    int colonIndex = peekLine.IndexOf(':');
                    string speaker = peekLine.Substring(0, colonIndex).Trim();

                    if (speaker.ToLower() == "player")
                    {
                        // This player message will land at messages.Count
                        // (stopIndex) on the next parse iteration
                        playerMessageIndex = stopIndex;
                    }
                }

                // First non-empty non-comment line seen — stop peeking regardless
                break;
            }

            ctx.currentNode.pausePoints.Add(new PausePoint(stopIndex, playerMessageIndex));
            return true;
        }

        private static bool TryParseChoiceBlockStart(string line, ParserContext ctx)
        {
            if (line != ">> choice")
                return false;

            if (ctx.inChoiceBlock)
            {
                BSDebug.LogWarning($"[BubbleSpinner] [{ctx.fileName}:{ctx.lineNumber}] Nested choice blocks not supported");
                return true;
            }

            ctx.inChoiceBlock = true;
            ctx.processingChoiceContent = false;
            return true;
        }

        private static bool TryParseChoiceBlockEnd(string line, ParserContext ctx)
        {
            if (line != ">> endchoice")
                return false;

            if (!ctx.inChoiceBlock)
            {
                BSDebug.LogWarning($"[BubbleSpinner] [{ctx.fileName}:{ctx.lineNumber}] Unexpected >> endchoice");
                return true;
            }

            if (ctx.currentChoice != null)
            {
                ValidateAndAddChoice(ctx);
                ctx.currentChoice = null;
            }

            ctx.inChoiceBlock = false;
            ctx.processingChoiceContent = false;
            return true;
        }

        private static bool TryParseChoiceOption(string line, ParserContext ctx)
        {
            if (!ctx.inChoiceBlock || !line.StartsWith("-> \"") || !line.EndsWith("\""))
                return false;

            if (ctx.currentChoice != null)
            {
                ValidateAndAddChoice(ctx);
            }

            string choiceText = line.Substring(4, line.Length - 5);

            if (string.IsNullOrEmpty(choiceText))
            {
                BSDebug.LogWarning($"[BubbleSpinner] [{ctx.fileName}:{ctx.lineNumber}] Empty choice text");
                return true;
            }

            ctx.currentChoice = new ChoiceData(choiceText, "");
            ctx.processingChoiceContent = true;
            return true;
        }

        private static bool TryParseMediaCommand(string line, ParserContext ctx)
        {
            if (!line.StartsWith(">> media"))
                return false;

            var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 3)
            {
                BSDebug.LogWarning($"[BubbleSpinner] [{ctx.fileName}:{ctx.lineNumber}] Invalid media command: {line}");
                return true;
            }

            string speaker = parts[2];
            string imagePath = ExtractPathFromMediaCommand(line);

            if (string.IsNullOrEmpty(imagePath))
            {
                BSDebug.LogWarning($"[BubbleSpinner] [{ctx.fileName}:{ctx.lineNumber}] No path in media command");
                return true;
            }

            bool shouldUnlock = line.Contains("unlock:true");

            var imageMessage = new MessageData(MessageData.MessageType.Image, speaker, "", imagePath)
            {
                shouldUnlockCG = shouldUnlock
            };

            if (shouldUnlock)
            {
                BSDebug.Log($"[BubbleSpinner] [{ctx.lineNumber}] 📸 Unlockable CG: {imagePath}");
            }

            if (ctx.processingChoiceContent && ctx.currentChoice != null)
            {
                ctx.currentChoice.playerMessages.Add(imageMessage);
            }
            else
            {
                ctx.currentNode.messages.Add(imageMessage);
            }

            return true;
        }

        private static bool TryParseDialogueLine(string line, ParserContext ctx)
        {
            if (!line.Contains(":"))
                return false;

            int colonIndex = line.IndexOf(':');
            string speaker = line.Substring(0, colonIndex).Trim();
            string content = line.Substring(colonIndex + 1).Trim();

            if (string.IsNullOrEmpty(speaker))
            {
                BSDebug.LogWarning($"[BubbleSpinner] [{ctx.fileName}:{ctx.lineNumber}] Empty speaker: {line}");
                return true;
            }

            if (content.StartsWith("\"") && content.EndsWith("\""))
            {
                content = content.Substring(1, content.Length - 2);
            }

            if (ctx.processingChoiceContent && ctx.currentChoice != null)
            {
                if (speaker.StartsWith("#"))
                {
                    // Strip # prefix from speaker name
                    string cleanSpeaker = speaker.Substring(1).Trim();

                    MessageData.MessageType msgType = cleanSpeaker.ToLower() == "system"
                        ? MessageData.MessageType.System
                        : MessageData.MessageType.Text;

                    var playerMessage = new MessageData(msgType, cleanSpeaker, content);
                    ctx.currentChoice.playerMessages.Add(playerMessage);
                }
                else
                {
                    // Non-player lines inside choices are not supported, 
                    // since they won't be paired with a pause button and could cause confusion about which lines are player vs NPC. 
                    // Log a warning and ignore them.
                    BSDebug.LogWarning($"[BubbleSpinner] [{ctx.fileName}:{ctx.lineNumber}] " +
                        $"Non-player line '{speaker}' inside choice block ignored. " +
                        $"Only '# Speaker: text' lines are valid inside choices.");
                }

                return true;
            }

            // Normal node message (outside choice block)
            MessageData.MessageType normalMsgType = speaker.ToLower() == "system"
                ? MessageData.MessageType.System
                : MessageData.MessageType.Text;

            ctx.currentNode.messages.Add(new MessageData(normalMsgType, speaker, content));
            return true;
        }

        // ═══════════════════════════════════════════════════════════
        // HELPER METHODS
        // ═══════════════════════════════════════════════════════════

        private static string ExtractPathFromMediaCommand(string line)
        {
            int pathIndex = line.IndexOf("path:");
            if (pathIndex == -1)
                return "";

            return line.Substring(pathIndex + 5).Trim();
        }

        private static void ValidateAndAddChoice(ParserContext ctx)
        {
            if (ctx.currentChoice == null) return;

            if (string.IsNullOrEmpty(ctx.currentChoice.targetNode))
            {
                BSDebug.LogWarning($"[BubbleSpinner] [{ctx.fileName}] Choice '{ctx.currentChoice.choiceText}' missing jump target");
            }

            ctx.currentNode.choices.Add(ctx.currentChoice);
        }

        private static void FinalizeCurrentNode(ParserContext ctx, Dictionary<string, DialogueNode> nodes)
        {
            if (ctx.currentChoice != null)
            {
                ValidateAndAddChoice(ctx);
                ctx.currentChoice = null;
            }

            var node = ctx.currentNode;

            if (node.messages.Count == 0 && (node.choices.Count > 0 || !string.IsNullOrEmpty(node.nextNode)))
            {
                BSDebug.LogWarning($"[BubbleSpinner] [{ctx.fileName}] Node '{node.nodeName}' has no messages");
            }

            if (node.choices.Count > 0 && !string.IsNullOrEmpty(node.nextNode))
            {
                BSDebug.LogWarning($"[BubbleSpinner] [{ctx.fileName}] Node '{node.nodeName}' has both choices and auto-jump");
            }

            nodes[node.nodeName] = node;
        }

        private static void FinalizeParser(ParserContext ctx, Dictionary<string, DialogueNode> nodes)
        {
            if (ctx.currentNode != null)
            {
                FinalizeCurrentNode(ctx, nodes);
            }

            if (ctx.inChoiceBlock)
            {
                BSDebug.LogWarning($"[BubbleSpinner] [{ctx.fileName}] Choice block never closed");
            }
        }

        // ═══════════════════════════════════════════════════════════
        // VALIDATION
        // ═══════════════════════════════════════════════════════════

        private static void ValidateDialogueGraph(Dictionary<string, DialogueNode> nodes, string fileName)
        {
            foreach (var kvp in nodes)
            {
                var node = kvp.Value;

                if (!string.IsNullOrEmpty(node.nextNode) && !nodes.ContainsKey(node.nextNode))
                {
                    if (!LooksLikeCrossChapterJump(node.nextNode))
                        BSDebug.LogWarning($"[BubbleSpinner] [{fileName}] Node '{node.nodeName}' jumps to non-existent '{node.nextNode}'");
                }

                foreach (var choice in node.choices)
                {
                    if (!string.IsNullOrEmpty(choice.targetNode) && !nodes.ContainsKey(choice.targetNode))
                    {
                        if (!LooksLikeCrossChapterJump(choice.targetNode))
                            BSDebug.LogWarning($"[BubbleSpinner] [{fileName}] Choice '{choice.choiceText}' targets non-existent '{choice.targetNode}'");
                    }
                }
            }
        }

        /// <summary>
        /// Returns true if the node name matches the cross-chapter jump convention.
        /// Pattern: underscore + "ch" + one or more digits, case-insensitive.
        /// Examples that match:  Start_Ch2, Node_Ch12, End_CH3
        /// Examples that don't: Fetch_ChocolateCake, ChapterIntro, StartCh2
        /// </summary>
        private static bool LooksLikeCrossChapterJump(string nodeName)
        {
            if (string.IsNullOrEmpty(nodeName))
                return false;

            return CrossChapterPattern.IsMatch(nodeName);
        }
    }
}