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
    ///
    /// .bub syntax summary:
    ///   title: NodeName       — declares a node
    ///   ---                   — opens node content (must follow title:)
    ///   ===                   — closes node
    ///   ...                   — pure pacing pause (tap to continue, nothing sent)
    ///   Player: "text"        — implicit pause point; tap sends the message then NPC continues
    ///   Speaker: "text"       — NPC or System message
    ///   >> media npc type:image [unlock:true] path:Key  — image bubble
    ///   >> choice / >> endchoice                        — choice block (endchoice required)
    ///   -> "Option text"      — choice button (inside >> choice block only)
    ///   <<jump NodeName>>     — jump to node (cross-chapter if not found in current file)
    ///   //                    — comment (inline or full line)
    /// </summary>
    public static class BubbleSpinnerParser
    {
        private class ParserContext
        {
            public DialogueNode currentNode;
            public ChoiceData currentChoice;
            public bool inChoiceBlock;
            public bool processingChoiceContent;
            public bool lastParsedWasTitle;     // tracks whether --- follows a title: line
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
                BSDebug.Error("[BubbleSpinner] Null .bub file provided");
                return nodes;
            }

            BSDebug.Info($"[BubbleSpinner] Parsing: {bubbleFile.name}");

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
                                BSDebug.Warn($"[BubbleSpinner] [{context.fileName}] contact: mismatch! " +
                                    $"File says '{contactName}' but asset expects '{expectedCharacterName}'");
                            }
                        }
                        context.lastParsedWasTitle = false;
                        continue;
                    }

                    if (TryParseNodeTitle(line, context, nodes)) continue;
                    if (TryParseNodeOpen(line, context)) continue;
                    if (TryParseNodeClose(line, context, nodes)) continue;

                    if (context.currentNode == null)
                    {
                        BSDebug.Warn($"[BubbleSpinner] [{context.fileName}:{context.lineNumber}] Content outside node: {line}");
                        context.lastParsedWasTitle = false;
                        continue;
                    }

                    if (TryParseJumpCommand(line, context)) continue;
                    if (TryParsePausePoint(line, context)) continue;
                    if (TryParseChoiceBlockStart(line, context)) continue;
                    if (TryParseChoiceBlockEnd(line, context)) continue;
                    if (TryParseChoiceOption(line, context)) continue;
                    if (TryParseMediaCommand(line, context)) continue;
                    if (TryParseDialogueLine(line, context)) continue;

                    BSDebug.Warn($"[BubbleSpinner] [{context.fileName}:{context.lineNumber}] Unrecognized: {line}");
                    context.lastParsedWasTitle = false;
                }
                catch (Exception ex)
                {
                    BSDebug.Error($"[BubbleSpinner] [{context.fileName}:{context.lineNumber}] Parse error: {line}\n{ex.Message}");
                }
            }

            FinalizeParser(context, nodes);
            ValidateDialogueGraph(nodes, bubbleFile.name);
            AssignNodeMessageIds(nodes);

            BSDebug.Info($"[BubbleSpinner] Parsed {nodes.Count} nodes from {bubbleFile.name}");
            return nodes;
        }

        // ═══════════════════════════════════════════════════════════
        // DETERMINISTIC MESSAGE ID ASSIGNMENT
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Assigns deterministic messageIds to all messages after the full parse is complete.
        /// Format: "{nodeName}_{messageIndex}"
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
                BSDebug.Warn($"[BubbleSpinner] [{ctx.fileName}:{ctx.lineNumber}] Empty node name");
                ctx.lastParsedWasTitle = false;
                return true;
            }

            if (nodes.ContainsKey(nodeName))
            {
                BSDebug.Warn($"[BubbleSpinner] [{ctx.fileName}:{ctx.lineNumber}] Duplicate node '{nodeName}'");
            }

            ctx.currentNode = new DialogueNode(nodeName);
            ctx.inChoiceBlock = false;
            ctx.processingChoiceContent = false;
            ctx.currentChoice = null;
            ctx.lastParsedWasTitle = true;

            return true;
        }

        /// <summary>
        /// Handles --- which opens node content.
        /// Valid only directly after a title: line.
        /// </summary>
        private static bool TryParseNodeOpen(string line, ParserContext ctx)
        {
            if (line != "---")
                return false;

            if (!ctx.lastParsedWasTitle)
            {
                BSDebug.Warn($"[BubbleSpinner] [{ctx.fileName}:{ctx.lineNumber}] '---' found without preceding title: — ignored");
            }

            ctx.lastParsedWasTitle = false;
            return true;
        }

        /// <summary>
        /// Handles === which closes a node.
        /// Warns if a choice block is still open when the node closes.
        /// </summary>
        private static bool TryParseNodeClose(string line, ParserContext ctx, Dictionary<string, DialogueNode> nodes)
        {
            if (line != "===")
                return false;

            if (ctx.currentNode == null)
            {
                BSDebug.Warn($"[BubbleSpinner] [{ctx.fileName}:{ctx.lineNumber}] '===' found with no open node — ignored");
                ctx.lastParsedWasTitle = false;
                return true;
            }

            if (ctx.inChoiceBlock)
            {
                BSDebug.Warn($"[BubbleSpinner] [{ctx.fileName}:{ctx.lineNumber}] '===' reached with unclosed >> choice block — use >> endchoice before ===");

                // Close the choice block gracefully before finalizing
                if (ctx.currentChoice != null)
                {
                    ValidateAndAddChoice(ctx);
                    ctx.currentChoice = null;
                }

                ctx.inChoiceBlock = false;
                ctx.processingChoiceContent = false;
            }

            FinalizeCurrentNode(ctx, nodes);
            ctx.lastParsedWasTitle = false;
            return true;
        }

        private static bool TryParseJumpCommand(string line, ParserContext ctx)
        {
            if (!line.StartsWith("<<jump") || !line.EndsWith(">>"))
                return false;

            string jumpTarget = line.Substring(6, line.Length - 8).Trim();

            if (string.IsNullOrEmpty(jumpTarget))
            {
                BSDebug.Warn($"[BubbleSpinner] [{ctx.fileName}:{ctx.lineNumber}] Empty jump target");
                ctx.lastParsedWasTitle = false;
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

            ctx.lastParsedWasTitle = false;
            return true;
        }

        /// <summary>
        /// Handles standalone ... pause points.
        /// Pure pacing pause — shows continue button, nothing sent on tap.
        /// Player: "text" lines are implicit pause points handled in TryParseDialogueLine.
        /// </summary>
        private static bool TryParsePausePoint(string line, ParserContext ctx)
        {
            if (line != "...")
                return false;

            if (ctx.processingChoiceContent)
            {
                BSDebug.Warn($"[BubbleSpinner] [{ctx.fileName}:{ctx.lineNumber}] Pause point inside choice block ignored");
                ctx.lastParsedWasTitle = false;
                return true;
            }

            int stopIndex = ctx.currentNode.messages.Count;
            ctx.currentNode.pausePoints.Add(new PausePoint(stopIndex));

            ctx.lastParsedWasTitle = false;
            return true;
        }

        private static bool TryParseChoiceBlockStart(string line, ParserContext ctx)
        {
            if (line != ">> choice")
                return false;

            if (ctx.inChoiceBlock)
            {
                BSDebug.Warn($"[BubbleSpinner] [{ctx.fileName}:{ctx.lineNumber}] Nested choice blocks not supported");
                ctx.lastParsedWasTitle = false;
                return true;
            }

            ctx.inChoiceBlock = true;
            ctx.processingChoiceContent = false;
            ctx.lastParsedWasTitle = false;
            return true;
        }

        private static bool TryParseChoiceBlockEnd(string line, ParserContext ctx)
        {
            if (line != ">> endchoice")
                return false;

            if (!ctx.inChoiceBlock)
            {
                BSDebug.Warn($"[BubbleSpinner] [{ctx.fileName}:{ctx.lineNumber}] Unexpected >> endchoice — no open choice block");
                ctx.lastParsedWasTitle = false;
                return true;
            }

            if (ctx.currentChoice != null)
            {
                ValidateAndAddChoice(ctx);
                ctx.currentChoice = null;
            }

            ctx.inChoiceBlock = false;
            ctx.processingChoiceContent = false;
            ctx.lastParsedWasTitle = false;
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
                BSDebug.Warn($"[BubbleSpinner] [{ctx.fileName}:{ctx.lineNumber}] Empty choice text");
                ctx.lastParsedWasTitle = false;
                return true;
            }

            ctx.currentChoice = new ChoiceData(choiceText, "");
            ctx.processingChoiceContent = true;
            ctx.lastParsedWasTitle = false;
            return true;
        }

        private static bool TryParseMediaCommand(string line, ParserContext ctx)
        {
            if (!line.StartsWith(">> media"))
                return false;

            var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 3)
            {
                BSDebug.Warn($"[BubbleSpinner] [{ctx.fileName}:{ctx.lineNumber}] Invalid media command: {line}");
                ctx.lastParsedWasTitle = false;
                return true;
            }

            string speaker = parts[2];
            string imagePath = ExtractPathFromMediaCommand(line);

            if (string.IsNullOrEmpty(imagePath))
            {
                BSDebug.Warn($"[BubbleSpinner] [{ctx.fileName}:{ctx.lineNumber}] No path in media command");
                ctx.lastParsedWasTitle = false;
                return true;
            }

            bool shouldUnlock = line.Contains("unlock:true");

            var imageMessage = new MessageData(MessageData.MessageType.Image, speaker, "", imagePath)
            {
                shouldUnlockCG = shouldUnlock
            };

            if (shouldUnlock)
            {
                BSDebug.Info($"[BubbleSpinner] [{ctx.lineNumber}] Unlockable CG: {imagePath}");
            }

            ctx.currentNode.messages.Add(imageMessage);

            ctx.lastParsedWasTitle = false;
            return true;
        }

        private static bool TryParseDialogueLine(string line, ParserContext ctx)
        {
            if (!line.Contains(":"))
                return false;

            int colonIndex = line.IndexOf(':');
            string speaker = line.Substring(0, colonIndex).Trim();
            string content = line.Substring(colonIndex + 1).Trim();

            if (!Regex.IsMatch(speaker, @"^[\w\s]+$"))
            {
                BSDebug.Warn($"[BubbleSpinner] [{ctx.fileName}:{ctx.lineNumber}] Invalid speaker format, skipping: {line}");
                return false;
            }

            if (string.IsNullOrEmpty(speaker))
            {
                BSDebug.Warn($"[BubbleSpinner] [{ctx.fileName}:{ctx.lineNumber}] Empty speaker: {line}");
                ctx.lastParsedWasTitle = false;
                return true;
            }

            if (content.StartsWith("\"") && content.EndsWith("\""))
            {
                content = content.Substring(1, content.Length - 2);
            }

            if (ctx.processingChoiceContent)
            {
                // Any dialogue line inside a choice block is not supported.
                // Content belongs in the target node, not the choice itself.
                BSDebug.Warn($"[BubbleSpinner] [{ctx.fileName}:{ctx.lineNumber}] " +
                    $"Dialogue line '{speaker}' inside choice block ignored. " +
                    $"Place content in the target node instead.");
                ctx.lastParsedWasTitle = false;
                return true;
            }

            MessageData.MessageType msgType = speaker.ToLower() == "system"
                ? MessageData.MessageType.System
                : MessageData.MessageType.Text;

            var message = new MessageData(msgType, speaker, content);
            ctx.currentNode.messages.Add(message);

            // Player: lines are implicit pause points.
            // The pause stops before this message — player taps, message sends, NPC continues.
            if (message.IsPlayerMessage)
            {
                int playerMessageIndex = ctx.currentNode.messages.Count - 1;
                ctx.currentNode.pausePoints.Add(new PausePoint(playerMessageIndex, playerMessageIndex));
            }

            ctx.lastParsedWasTitle = false;
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
                BSDebug.Warn($"[BubbleSpinner] [{ctx.fileName}] Choice '{ctx.currentChoice.choiceText}' missing jump target");
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
                BSDebug.Warn($"[BubbleSpinner] [{ctx.fileName}] Node '{node.nodeName}' has no messages");
            }

            if (node.choices.Count > 0 && !string.IsNullOrEmpty(node.nextNode))
            {
                BSDebug.Warn($"[BubbleSpinner] [{ctx.fileName}] Node '{node.nodeName}' has both choices and auto-jump");
            }

            nodes[node.nodeName] = node;
            ctx.currentNode = null;
        }

        private static void FinalizeParser(ParserContext ctx, Dictionary<string, DialogueNode> nodes)
        {
            if (ctx.currentNode != null)
            {
                BSDebug.Warn($"[BubbleSpinner] [{ctx.fileName}] File ended without closing '===' on node '{ctx.currentNode.nodeName}'");
                FinalizeCurrentNode(ctx, nodes);
            }

            if (ctx.inChoiceBlock)
            {
                BSDebug.Warn($"[BubbleSpinner] [{ctx.fileName}] Choice block never closed with >> endchoice");
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
                        BSDebug.Warn($"[BubbleSpinner] [{fileName}] Node '{node.nodeName}' jumps to non-existent '{node.nextNode}'");
                }

                foreach (var choice in node.choices)
                {
                    if (!string.IsNullOrEmpty(choice.targetNode) && !nodes.ContainsKey(choice.targetNode))
                    {
                        if (!LooksLikeCrossChapterJump(choice.targetNode))
                            BSDebug.Warn($"[BubbleSpinner] [{fileName}] Choice '{choice.choiceText}' targets non-existent '{choice.targetNode}'");
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