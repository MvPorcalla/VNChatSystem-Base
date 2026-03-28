TODO: Update bubblespinner docs

DialogueExecutor.cs

is this case sensitive?

// REPLACE WITH THIS
// nodeName IS the chapterId when not found locally
// Entry point of that chapter file is always "Start" unless the jump specifies otherwise
LoadChapterById(nodeName, "Start");

---

Editor

So better long-term:

CharacterDatabase.cs → ONLY data + runtime queries
CharacterDatabaseEditor.cs → find / validation / tools

---

suggestion .bub syntax

first jum title make it into a new syntax chapter id
cross chapter jumping instead instead of <jump Node_name> make a neew syntax and keep <jump Node_name> as  a chapter node jump only

---

Question:

> In BubbleSpinner, does chapter progression strictly follow the numerical order (chapter1 → chapter2 → chapter3), or can I skip chapters using `<<jump NodeName>>`? For example, can a choice in chapter1 jump directly to chapter3, bypassing chapter2? And if I have multiple Chapter2 nodes like `Chapter2_AffectionR` and `Chapter2_CorruptionR`, can I route to them based on player choices?

---

*"Does BubbleSpinner care about the physical order of `.bub` files, or does it only follow the flow defined by `<<jump NodeName>>`? For example, can I have my files ordered as Chapter3, Chapter1, Chapter2, ChapterEnd, and still have the game start at Chapter1 and follow jumps between nodes regardless of the file order?"*

---

> What happens if Chapter 1 jumps to Chapter 2, and Chapter 2 jumps back to Chapter 1? What happens if nodes loop back to previous chapters or nodes? Is there a safe loop prevention mechanism?

---

> Are pause points (...) handled correctly if they appear consecutively or before a Player: line?

---

> How easy is it to add new variable types (affection, corruption, inventory) without breaking old .bub files?

---

---

**Suggestion for `.bub` syntax**

* make a new chapter `jump title` syntax for **chapter IDs**.
* For cross-chapter jumping, introduce a new syntax instead of `<jump Node_name>`.
* Keep `<jump Node_name>` strictly for **node jumps within the same chapter**.

e.g:

# chapter 1 - file 1 .bub

contact: Sofia

**chapter IDs** <- where chapter jump e.g. (chapter: Ch1)

// after that it reads the chapter downward starting to the first title node

title: Start
---
System: "9:42 AM"
Sofia: "Batch 1 - Message A"
Sofia: "Batch 1 - Message B"

<<new syntax for corss jumping chapter **chapter IDs**>> e.g. ( <<jump chapter:Ch2>> )

---

# chapter 2 - file 2 .bub

**chapter IDs** <- where chapter jump e.g. (chapter: Ch2)

title: Start
---
System: "9:42 AM"
Sofia: "Batch 2 - Message C"
Sofia: "Batch 2 - Message D"


what do you think of this?

reasoning:

Node jumps are restricted to the current file to keep control flow local and predictable.
Cross-file jumps are handled separately and are only used to move between chapters/files.
This separation enforces clear structural boundaries in the story flow.


---


=-==============================================

---

**Indent Unit**
- 1 tab = 1 indent level
- Spaces: count them, round to nearest tab equivalent (4 spaces = 1 level, 2 spaces = round to 1, 6 spaces = round to 1 or 2 — use `Math.Round(spaceCount / 4.0)`)
- Mixed tabs and spaces on the same line → warn + treat spaces as tabs

---

**Indent Levels and What They Mean**

```
indent 0    — node level
                >> choice
                >> endchoice
                <<jump Node>>
                Speaker: "text"
                ...
                >> media

indent 1    — choice option
                -> "Option text"
                -> "Option text" <<jump Node>>   // inline jump still valid

indent 2    — belongs to the choice directly above
                <<jump Node>>
                <<if condition>>                 // future

indent 3    — belongs to the if block above
                <<jump Node>>                    // future
                <<else>>                         // future

indent 4+   — deeper conditional nesting         // future
```

---

**Strict Rules**

| Situation | Behaviour |
|---|---|
| `<<jump>>` at indent 0 inside `>> choice` block | Error: unexpected jump at node level inside choice block |
| `<<jump>>` at indent 1 (same as `->`) | Error: jump must be at indent 2 to belong to a choice |
| `->` at indent 0 | Error: choice option must be indented inside `>> choice` |
| `->` at indent 2+ | Error: choice option must be at indent 1 |
| `<<jump>>` at indent 2 with no open `currentChoice` | Error: jump at choice level but no choice is open |
| `>> choice` at indent 1+ | Error: choice block must be at indent 0 |
| `>> endchoice` at indent 1+ | Warn + recover: treat as indent 0 |
| Wrong indent unit (spaces) | Warn + recover to nearest level |

---

**Inline jump stays valid at indent 1**

```
>> choice
    -> "Option" <<jump Node>>    // indent 1, inline — still fine
>> endchoice
```

This is equivalent to:

```
>> choice
    -> "Option"                  // indent 1
        <<jump Node>>            // indent 2
>> endchoice
```

Both are supported. Inline takes priority — if a `->` line already has a jump, any indent-2 `<<jump>>` below it is a duplicate and errors.

---

**Future conditionals at indent 2-3**

```
>> choice
    -> "Option"
        <<if hasMet == true>>
            <<jump Node_Met>>
        <<else>>
            <<jump Node_New>>
        <<endif>>
>> endchoice
```

The parser doesn't implement this yet — but the indent rules above leave room for it without breaking anything.

---

**What changes in the parser:**

1. Stop calling `Trim()` immediately — measure indent first
2. Add `MeasureIndent(string rawLine)` helper — returns int level
3. Add `int indentLevel` to `ParserContext`
4. Each `TryParse` method gets indent-aware validation at the top
5. `TryParseJumpCommand` routes based on indent level instead of `processingChoiceContent`
6. `TryParseChoiceOption` requires indent level 1 strictly

---

What limit makes sense for your format:

Based on your planned structure:

indent 0 — node level
indent 1 — choice option (->)
indent 2 — jump / <<if>>
indent 3 — <<else>> body
indent 4 — nested <<if>> inside <<else>>
4 is the right limit. It covers everything you've planned including deeply nested conditionals, and anything beyond 4 is almost certainly an author mistake.

Do you want to add the limit now?