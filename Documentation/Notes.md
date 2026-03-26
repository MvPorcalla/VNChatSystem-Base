TODO: Update bubblespinner docs

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

TODO:

---

**BubbleSpinner — Chapter Registry Refactor**

The current chapter system in `DialogueExecutor` and `ConversationAsset` uses a flat `List<TextAsset>` indexed sequentially (`currentChapterIndex++`). This needs to be replaced with a named chapter registry system.

**What needs to change:**

`ConversationAsset` — replace `List<TextAsset> chapters` with a `List<ChapterEntry>` where each entry has a `chapterId` (string key) and a `TextAsset file`. The first entry (index 0) is always the entry point and loads automatically on conversation start. All other entries are a jump registry — order does not matter.

`ChapterEntry` — new serializable class with `chapterId` and `file` fields. The `chapterId` is auto-read from the first `title:` line in the `.bub` file via a custom Inspector Editor script. Writer can override it manually if needed.

`DialogueExecutor` — replace `currentChapterIndex++` logic in `LoadNextChapter()` with a dictionary lookup by `chapterId`. When `<<jump SomeKey>>` fails to find a node in the current file, the engine looks up `SomeKey` in the chapter registry by `chapterId` and loads that file.

`ConversationState` — replace `currentChapterIndex` (int) with `currentChapterId` (string) to track the active chapter by key instead of index.

`Custom Inspector Editor` — a `[CustomEditor]` for `ConversationAsset` that reads the first `title:` from a dragged `.bub` file and auto-fills the `chapterId` field.

**The contract:**
- Index 0 entry is always the first chapter loaded
- Every other entry is only reached via `<<jump ChapterId>>`
- The key in the registry must match the first `title:` in the file exactly
- File naming has no rules — only the `title:` matters

**Do not assume or invent any code not explicitly provided. Ask for the full relevant scripts before making changes.**

---