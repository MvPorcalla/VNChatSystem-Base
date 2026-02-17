// ════════════════════════════════════════════════════════════════════════
// Assets/Scripts/BubbleSpinner/Core/BubbleSpinnerParser.cs
// ════════════════════════════════════════════════════════════════════════

using System;
using System.Collections.Generic;
using UnityEngine;
using BubbleSpinner.Data;

namespace BubbleSpinner.Core
{
    /// <summary>
    ///  Parses .bub dialogue files into a structured format of DialogueNodes, Messages, and Choices.
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

        // ═══════════════════════════════════════════════════════════
        // ░ PUBLIC API
        // ═══════════════════════════════════════════════════════════

        public static Dictionary<string, DialogueNode> Parse(TextAsset bubbleFile)
        {
            var nodes = new Dictionary<string, DialogueNode>();
            
            if (bubbleFile == null)
            {
                Debug.LogError("[BubbleSpinner] Null .bub file provided");
                return nodes;
            }

            Debug.Log($"[BubbleSpinner] Parsing: {bubbleFile.name}");

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
                        continue;

                    if (TryParseNodeTitle(line, context, nodes)) continue;
                    if (line == "---" || line == "===") continue;

                    if (context.currentNode == null)
                    {
                        Debug.LogWarning($"[BubbleSpinner] [{context.fileName}:{context.lineNumber}] Content outside node: {line}");
                        continue;
                    }

                    if (TryParseJumpCommand(line, context)) continue;
                    if (TryParsePauseButton(line, context)) continue;
                    if (TryParseChoiceBlockStart(line, context)) continue;
                    if (TryParseChoiceBlockEnd(line, context)) continue;
                    if (TryParseChoiceOption(line, context)) continue;
                    if (TryParseMediaCommand(line, context)) continue;
                    if (TryParseDialogueLine(line, context)) continue;

                    Debug.LogWarning($"[BubbleSpinner] [{context.fileName}:{context.lineNumber}] Unrecognized: {line}");
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[BubbleSpinner] [{context.fileName}:{context.lineNumber}] Parse error: {line}\n{ex.Message}");
                }
            }

            FinalizeParser(context, nodes);
            ValidateDialogueGraph(nodes, bubbleFile.name);

            Debug.Log($"[BubbleSpinner] Parsed {nodes.Count} nodes from {bubbleFile.name}");
            return nodes;
        }

        // ═══════════════════════════════════════════════════════════
        // ░ PARSING METHODS
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
                Debug.LogWarning($"[BubbleSpinner] [{ctx.fileName}:{ctx.lineNumber}] Empty node name");
                return true;
            }

            if (nodes.ContainsKey(nodeName))
            {
                Debug.LogWarning($"[BubbleSpinner] [{ctx.fileName}:{ctx.lineNumber}] Duplicate node '{nodeName}'");
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
                Debug.LogWarning($"[BubbleSpinner] [{ctx.fileName}:{ctx.lineNumber}] Empty jump target");
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

        private static bool TryParsePauseButton(string line, ParserContext ctx)
        {
            if (line != "-> ...")
                return false;

            if (ctx.processingChoiceContent)
            {
                Debug.LogWarning($"[BubbleSpinner] [{ctx.fileName}:{ctx.lineNumber}] Pause button inside choice block");
            }
            else
            {
                int pauseAfterMessage = ctx.currentNode.messages.Count;
                ctx.currentNode.pausePoints.Add(pauseAfterMessage);
            }
            
            return true;
        }

        private static bool TryParseChoiceBlockStart(string line, ParserContext ctx)
        {
            if (line != ">> choice")
                return false;

            if (ctx.inChoiceBlock)
            {
                Debug.LogWarning($"[BubbleSpinner] [{ctx.fileName}:{ctx.lineNumber}] Nested choice blocks not supported");
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
                Debug.LogWarning($"[BubbleSpinner] [{ctx.fileName}:{ctx.lineNumber}] Unexpected >> endchoice");
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
            
            string choiceText = line.Substring(3, line.Length - 4);
            
            if (string.IsNullOrEmpty(choiceText))
            {
                Debug.LogWarning($"[BubbleSpinner] [{ctx.fileName}:{ctx.lineNumber}] Empty choice text");
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
                Debug.LogWarning($"[BubbleSpinner] [{ctx.fileName}:{ctx.lineNumber}] Invalid media command: {line}");
                return true;
            }

            string speaker = parts[2];
            string imagePath = ExtractPathFromMediaCommand(line);

            if (string.IsNullOrEmpty(imagePath))
            {
                Debug.LogWarning($"[BubbleSpinner] [{ctx.fileName}:{ctx.lineNumber}] No path in media command");
                return true;
            }

            bool shouldUnlock = line.Contains("unlock:true");

            var imageMessage = new MessageData(MessageData.MessageType.Image, speaker, "", imagePath)
            {
                shouldUnlockCG = shouldUnlock
            };

            if (shouldUnlock)
            {
                Debug.Log($"[BubbleSpinner] [{ctx.lineNumber}] 📸 Unlockable CG: {imagePath}");
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
                Debug.LogWarning($"[BubbleSpinner] [{ctx.fileName}:{ctx.lineNumber}] Empty speaker: {line}");
                return true;
            }
            
            if (content.StartsWith("\"") && content.EndsWith("\""))
            {
                content = content.Substring(1, content.Length - 2);
            }

            MessageData.MessageType msgType = speaker.ToLower() == "system" 
                ? MessageData.MessageType.System 
                : MessageData.MessageType.Text;

            var message = new MessageData(msgType, speaker, content);
            
            if (ctx.processingChoiceContent && ctx.currentChoice != null && speaker.StartsWith("#"))
            {
                speaker = speaker.Substring(1).Trim();
                message.speaker = speaker;
                ctx.currentChoice.playerMessages.Add(message);
            }
            else
            {
                ctx.currentNode.messages.Add(message);
            }
            
            return true;
        }

        // ═══════════════════════════════════════════════════════════
        // ░ HELPER METHODS
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
                Debug.LogWarning($"[BubbleSpinner] [{ctx.fileName}] Choice '{ctx.currentChoice.choiceText}' missing jump target");
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
                Debug.LogWarning($"[BubbleSpinner] [{ctx.fileName}] Node '{node.nodeName}' has no messages");
            }

            if (node.choices.Count > 0 && !string.IsNullOrEmpty(node.nextNode))
            {
                Debug.LogWarning($"[BubbleSpinner] [{ctx.fileName}] Node '{node.nodeName}' has both choices and auto-jump");
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
                Debug.LogWarning($"[BubbleSpinner] [{ctx.fileName}] Choice block never closed");
            }
        }

        // ═══════════════════════════════════════════════════════════
        // ░ VALIDATION
        // ═══════════════════════════════════════════════════════════

        private static void ValidateDialogueGraph(Dictionary<string, DialogueNode> nodes, string fileName)
        {
            foreach (var kvp in nodes)
            {
                var node = kvp.Value;
                
                if (!string.IsNullOrEmpty(node.nextNode))
                {
                    if (!nodes.ContainsKey(node.nextNode))
                    {
                        // NEW: Only warn if it looks like an internal node (no chapter prefix)
                        if (!LooksLikeCrossChapterJump(node.nextNode))
                        {
                            Debug.LogWarning($"[BubbleSpinner] [{fileName}] Node '{node.nodeName}' jumps to non-existent '{node.nextNode}'");
                        }
                        else
                        {
                            Debug.Log($"[BubbleSpinner] [{fileName}] Node '{node.nodeName}' has cross-chapter jump to '{node.nextNode}' (OK)");
                        }
                    }
                }
                
                foreach (var choice in node.choices)
                {
                    if (!string.IsNullOrEmpty(choice.targetNode))
                    {
                        if (!nodes.ContainsKey(choice.targetNode))
                        {
                            if (!LooksLikeCrossChapterJump(choice.targetNode))
                            {
                                Debug.LogWarning($"[BubbleSpinner] [{fileName}] Choice '{choice.choiceText}' targets non-existent '{choice.targetNode}'");
                            }
                            else
                            {
                                Debug.Log($"[BubbleSpinner] [{fileName}] Choice '{choice.choiceText}' has cross-chapter target '{choice.targetNode}' (OK)");
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Heuristic to detect if a node name looks like a cross-chapter jump.
        /// Cross-chapter nodes typically have patterns like:
        /// - "Start_Ch2", "EndNode_Ch3"
        /// - "Chapter2_Start"
        /// - Any node with "Ch" or "Chapter" in the name
        /// </summary>
        private static bool LooksLikeCrossChapterJump(string nodeName)
        {
            if (string.IsNullOrEmpty(nodeName))
                return false;
            
            nodeName = nodeName.ToLower();
            
            // Common patterns for cross-chapter jumps
            return nodeName.Contains("_ch") ||      // Start_Ch2
                nodeName.Contains("chapter") ||  // Chapter2_Start
                nodeName.Contains("ch2") ||      // StartCh2
                nodeName.Contains("ch3") ||      // etc...
                nodeName.Contains("ch4") ||
                nodeName.Contains("ch5");
        }
    }
}