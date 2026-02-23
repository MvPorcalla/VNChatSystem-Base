# ISSUE:

---

TODO:

another is when i enter a chatapppanel i see the flicker of the content from empty to populating it


---

TODO:

## Prompt

Here’s the corrected and clearer version:

---

i have this issue where if i go out at mid convo and comback it show this `-> ...` button and if i press it it show the choice button or end button
it shouldnt do that it show the choice or end button right away `-> ...` this should only show if its a continue button

---

TODO:

simplify GalleryFullscreenViewer.cs

the backbutton in the PhoneHomescreen when i open full screen it directly send it back to homescreenpanel instead of back to gallerypanel like the closebutton
it should be when i open the fullscreen of image when iback from the phonehomescreen.cs 
fullscreen -> gallerypanel -> homescreen

consider making a GalleryController and put all the script there

---

TODO: 

Fix image fullscreen viewer there is 2 script maybve combine it and put in common folder for reusability
Fix confirmation dialogue currently its messy

---

TODO: 

Note: BubbleSpinner is a standalone script for parsing `.bub` files. It connects to the UI through a bridge.

What's wrong in ChatAppController


Problem 2: Panel navigation and conversation lifecycle are mixed.
SwitchToChatPanel, SwitchToContactList, OnPhoneBackPressed, OnPhoneHomePressed are navigation concerns. StartConversation, PerformConversationCleanup, executor subscriptions are conversation lifecycle concerns. They're all in one class.

What I'd suggest

One new method on DialogueExecutor in BubbleSpinner:
csharppublic void AdvanceToNextChapter()
That moves the state mutation out of the UI entirely.
And optionally extract panel switching into a lightweight ChatNavigationController — but that's lower priority than the chapter issue.

---

Turn it into a **single focused architectural cleanup issue**, not a wall of commentary.

Here’s a clean GitHub issue version you can paste:

---

## Title

Refactor ChatAppController to reduce UI → domain coupling

## Description

`ChatAppController` still contains minor boundary violations and leftover structural noise after the chapter transition refactor.

This issue tracks remaining cleanup tasks to enforce clearer separation between UI and DialogueExecutor.

---

## Problems Identified

### 1️⃣ UI directly mutates `ConversationState` in cleanup

`PerformConversationCleanup()` forces:

```csharp
state.isInPauseState = true;
```

This is domain state and should be handled by `DialogueExecutor` (e.g. `NotifyInterrupted()` or `ForcePauseState()`).

**Impact:** Low risk but violates layering rules.

---

### 2️⃣ UI reads chapter structure from `ConversationAsset`

`HasMoreChapters()` queries:

```csharp
currentConversation.chapters.Count
```

Chapter availability should be exposed via `DialogueExecutor` (e.g. `bool HasMoreChapters`).

**Impact:** Same category as previously fixed chapter mutation issue.

---

### 3️⃣ Dead serialized fields

```csharp
[SerializeField] private ScrollRect chatScrollRect;
[SerializeField] private RectTransform chatContent;
```

Never referenced in code. Likely owned by `ChatAutoScroller`.

**Impact:** Zero functional risk. Cleanup only.

---

### 4️⃣ `OnNewMessageDisplayed` visibility unclear

Public method but no visible subscription in this class.

Clarify:

* Who calls it?
* Should it be event-driven instead?

**Impact:** Architecture clarity.

---

## Proposed Phases

Phase 1 — HasMoreChapters() moves to DialogueExecutor
Same category as the chapter mutation fix. Add a property to DialogueExecutor, remove the asset-querying logic from ChatAppController. Clean boundary, low risk.

Phase 2 — isInPauseState mutation moves out of PerformConversationCleanup()
Add a method to DialogueExecutor (something like NotifyInterrupted()) so the UI stops writing state directly. Slightly more involved because it needs to feel natural on the executor API.

Phase 3 — Dead serialized fields
Remove chatScrollRect and chatContent. Zero logic impact, just cleanup.

Phase 4 — OnNewMessageDisplayed visibility
Clarify who calls it and how. Needs a look at ChatTimingController or ChatMessageSpawner to answer that properly — do you want to paste one of those before we start?

---
