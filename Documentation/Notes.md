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

* Convert the first `jump title` into a new syntax for **chapter IDs**.
* For cross-chapter jumping, introduce a new syntax instead of `<jump Node_name>`.
* Keep `<jump Node_name>` strictly for **node jumps within the same chapter**.