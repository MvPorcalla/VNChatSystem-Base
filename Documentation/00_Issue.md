

Cross chapter jump issue

Cross-chapter jump does not correctly resolve the target node in the destination chapter.
Instead of jumping to the title: entry point of the target chapter file, the system attempts to resolve the node using the current chapter context, causing node lookup failure.

---

[DialogueExecutor] Node 'Start' not found in chapter 'Start_Ch2'

---

Steps to Reproduce

Run Chapter 1 dialogue
Reach final node containing:
<<jump Start_Ch2>>
System attempts to transition to Chapter 2
Chapter 2 loads, but node resolution fails
Error appears and dialogue stops

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


// ===========================================================

TODO: Critical 

Chapter 1:

// ─────────────────────────────────────────
// CHOICE VARIATION 3 — fall-through
// All options continue downward, no jumps
// ─────────────────────────────────────────

>> choice
    -> "Casual_17a - That makes sense"
    -> "Casual_17b - Tell me more"
    -> "Casual_17c - I see"
>> endchoice

Sofia: "Casual_17"
Sofia: "Casual_18"
Sofia: "Casual_19"
Sofia: "Casual_20"

<<jump EndNode>>

---

on title: Node_Casual when i reach choice block for // CHOICE VARIATION 3 — fall-through and pressing the choice does nothing

---

chapter 2: 

//=====================================
// CH2 - NODE B
//=====================================

title: Node_Ch2_B
---
Sofia: "Ch2_B_8"
Sofia: "Ch2_B_9"
Sofia: "Ch2_B_10"

Player: "Ch2_B_11 - Player message pause"

Sofia: "Ch2_B_12"
Sofia: "Ch2_B_13"

// ─────────────────────────────────────────
// CHOICE VARIATION 3 — fall-through
// All options continue downward, no jumps
// ─────────────────────────────────────────

>> choice
    -> "Ch2_B_14a - Hmm"
    -> "Ch2_B_14b - I see"
    -> "Ch2_B_14c - Go on"
>> endchoice

Sofia: "Ch2_B_15"
Sofia: "Ch2_B_16"
Sofia: "Ch2_B_17"

<<jump Node_Ch2_End>>

---

UI Display of title: Node_Ch2_B:

Sofia: "Ch2_B_8"
Sofia: "Ch2_B_9"
Sofia: "Ch2_B_10"
Player: "Ch2_B_11 - Player message pause"
Sofia: "Ch2_B_12"
Sofia: "Ch2_B_13"
Sofia: "Ch2_B_15"
Sofia: "Ch2_B_16"
Sofia: "Ch2_B_17"

then choice option showed up same problem but now pressing the choice does nothing

---