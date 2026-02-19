# BubbleSpinner Chat Script Legend

**Engine / Format Name:** BubbleSpinner (bub)
**File Extension:** `.bub`
**Purpose:** Script files in this format contain dialogue, media, and branching choices for chat-focused visual novels. Each file can represent a chapter, scene, or node sequence. Each `.bub` file usually represents one chapter/scene to make file organization explicit.

**These files currently support:**
- Text bubbles
- Image media bubbles (standard and unlockable CGs)
- Player choices and branching paths
- Tap-to-continue pause points
- Cross-chapter node jumps

**Not yet implemented (planned):** Variables, conditional dialogue, audio media, typing indicators, per-message delays.

**Example File Name:** `C1_Intro.bub`

### Note

All character profile images, CGs, and media assets are loaded via Unity's Addressables system. Do not hard-reference sprites in prefabs. The parser and ConversationManager should request assets by their addressable keys (from the `.bub` `path:` field), then asynchronously load them before displaying in chat bubbles. Make sure image loading is asynchronous, with fallback/error handling if an asset cannot be found.

---

## Legend for Chat Script Commands

| Symbol / Command | Purpose | Notes |
|---|---|---|
| `contact: Name` | **Contact Assignment** | Decorative header identifying which character this file belongs to. Parsed but not currently used at runtime. Place at top of each `.bub` file. |
| `title: NodeName` | **Node Header** | Marks the start of a conversation or branch node. |
| `---` | **Node Content Separator** | Separates the node header from its content. Place after `title:`. |
| `===` | **Node End Separator** | Marks the end of a node block. Treated identically to `---` by the parser. |
| `[Speaker]: "Text"` | **Text Bubble** | Displays a text message bubble. Speaker label determines bubble alignment. Quotes around content are optional — the parser strips them if present. |
| `system: "Text"` | **System Message** | Displays a non-chat system message (e.g. timestamps, status notices). Speaker must be literally `system` (case-insensitive). |
| `>> media [Speaker] type:image path:[key]` | **Image Bubble** | Displays an image inside a chat bubble. `path:` must be a valid Unity Addressables key. |
| `>> media [Speaker] type:image unlock:true path:[key]` | **Unlockable CG** | Displays image in chat AND saves it to the player's CG gallery. `unlock:true` must appear before `path:` in the line. |
| `-> ...` | **Tap-to-Continue** | Inserts a pause point. Player must tap a button to continue. Cannot be placed inside a choice block. |
| `>> choice` | **Choice Block Start** | Opens a branching choice menu. Must be closed with `>> endchoice`. |
| `>> endchoice` | **Choice Block End** | Closes the choice block. Required — unclosed blocks produce a warning. |
| `-> "Choice Text"` | **Choice Option** | Defines one player choice button. Must be inside a `>> choice` block. Text must be wrapped in double quotes. |
| `# [Speaker]: "Text"` | **Player Message in Choice** | A message sent by the player after selecting a choice. Must appear inside a choice option block, after `-> "Choice Text"`. The `#` prefix marks it as the player's outgoing message. |
| `<<jump NodeName>>` | **Jump to Node** | Moves execution to another node. Works within the same `.bub` file or cross-chapter. If the target node is not found in the current chapter, the executor attempts to load the next chapter file and find it there. |
| `//` | **Comment** | Anything after `//` on a line is ignored by the parser. Inline comments on command lines are also stripped. |

---

## Media Command Breakdown

| Part | Meaning |
|---|---|
| `>> media` | Marks this line as a command to display media content inside a chat bubble. |
| `[Speaker]` | Specifies who the bubble belongs to — e.g. `npc` or `player`. Determines bubble side. |
| `type:image` | The type of media. Currently only `image` is implemented. `audio` is not yet supported. |
| `unlock:true` | Optional flag. If present, the image is shown in chat AND added to `ConversationState.unlockedCGs`. Must appear before `path:` in the line. |
| `path:[key]` | The Addressables key of the asset to load. Everything after `path:` to end of line is used as the key, so place it last. |

---

## Cross-Chapter Jumps

When `<<jump NodeName>>` targets a node not found in the current `.bub` file, the executor automatically advances to the next chapter file in the `ConversationAsset.chapters` list and searches for the node there.

**Current heuristic warning suppression:** The validator skips "missing node" warnings for jump targets whose names contain `_ch`, `chapter`, `ch2`, `ch3`, `ch4`, or `ch5`. This covers chapters 2–5 only. Targets for chapter 6+ will still emit false warnings in the console. This is a known limitation.

---

## Choice Block Structure

```
>> choice
    -> "Option A"
        # Player: "Option A text."
        <<jump NodeA>>

    -> "Option B"
        # Player: "Option B text."
        <<jump NodeB>>
>> endchoice
```

**Rules:**
- Every choice option should have a `<<jump>>` — choices without a jump target produce a console warning but don't crash.
- Indentation is cosmetic only. The parser uses `>> endchoice` as the terminator, not indentation.
- Nested choice blocks are not supported. A `>> choice` inside an existing choice block is ignored with a warning.
- Pause points (`-> ...`) inside choice blocks are not supported and produce a warning.
- NPC lines inside a choice block (without `#` prefix) are added to the parent node's message list, not the choice. This is likely unintended — keep choice block content to `# Player:` lines and `<<jump>>` only.

---

## Full Example `.bub` File

```
contact: NPC_Elena

title: C1_Start
---
system: "4:50 PM"

NPC: "Hi! Let's go out."
NPC: "I want coffee."
>> media npc type:image unlock:true path:CGs/npc_CG1.png      // Unlockable CG
>> media npc type:image path:npc_happy.png                     // Standard image

-> ...                                                         // Tap to continue

Player: "Ok, let's go."
Player: "Blah blah."

NPC: "Look at this sunset! 🌅"
>> media npc type:image unlock:true path:CGs/Date_Beach_Sunset.png
NPC: "I'll never forget this moment."

>> choice
    -> "Let's meet at the park"
        # Player: "Let's meet at the park."
        <<jump C1_ParkNode>>

    -> "Let's meet at the cafe"
        # Player: "Let's meet at the cafe."
        <<jump C1_CafeNode>>
>> endchoice

===

title: C1_ParkNode
---
NPC: "Tea drinkers are classy. 🍵"
NPC: "You have my respect."

-> ...

Player: "Thank you."
NPC: "Anytime!"
>> media npc type:image path:npc_smug.png

<<jump EndNode>>

===

title: C1_CafeNode
---
NPC: "Coffee lovers unite! ☕"
NPC: "Here, take this cup of joy."

-> ...

Player: "Thanks!"
NPC: "You're welcome!"
>> media npc type:image path:npc_winking.png

<<jump EndNode>>

===

title: EndNode
---
NPC: "Anyway, that's it. Thanks for playing! 🎉"
```

---

## Known Limitations

- `contact:` is parsed and skipped — it is not validated against the `ConversationAsset` character name at runtime.
- `type:audio` in media commands is not implemented. Audio lines will fall through to an unrecognized line warning.
- Variables (`<<set>>`) and conditionals (`<<if>>`) are not yet implemented despite being listed as planned features.
- `>> endchoice` is required but not documented in the original legend — missing it silently breaks choice blocks.
- Message timestamps are assigned at parse time using `DateTime.Now`, not at display time. Restored conversations will show the load time, not the original play time.
- Cross-chapter jump validation only suppresses false warnings for chapters 2–5.
- All messages are stored in full in `ConversationState.messageHistory`. Save file size will grow with long playthroughs.