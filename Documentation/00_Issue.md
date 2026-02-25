# ISSUE:

---

TODO:

another is when i enter a chatapppanel i see the flicker of the content from empty to populating it


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

---


Phase 4 has two fixes with very different scopes. Let me be upfront about both before writing anything.

--- crtitical
Fix #6 — ContinueFromCurrentState vs ProcessCurrentNode entry points
This is a small, clean change. ProcessCurrentNode should be private — it already effectively is since nothing outside the class calls it, but making the intent explicit also means cleaning up ContinueFromCurrentState so it's clearly the single public entry point after initialization. No behaviour changes, just access clarity.
Only DialogueExecutor.cs touched.
---

Fix #4 — Addressable chapters
This one is larger and I want to be honest about the tradeoff before you commit to it.
Currently ConversationAsset.chapters is List<TextAsset> — all .bub files are bundled and loaded into memory at startup regardless of which chapter is active. For a short game this is fine. For a long one with many chapters it wastes memory.
The fix would change chapters to List<AssetReferenceT<TextAsset>> and make DialogueExecutor.LoadCurrentChapter() and LoadNextChapter() async — loading the chapter asset on demand and releasing the previous one after the jump.
The catch: DialogueExecutor is currently a plain C# class, not a MonoBehaviour. Addressables async operations use AsyncOperationHandle and need to be awaited. That means either:

Making DialogueExecutor aware of coroutines (requires a MonoBehaviour host), or
Using async/await with UniTask or C# Tasks, or
Doing a synchronous WaitForCompletion() load — simplest but blocks the main thread briefly on chapter transition




TODO: Critical

I’m noticing some behavior: DialougeExecutioner.cs

## Why is it doing this, and should I leave it as-is?

**Scenario: conversation reaches a choice, player backs out**

| Re-entry | Expected |
|---|---|
| 1st re-entry | Choice buttons shown directly — no pause button |
| Every re-entry after | Same — choice buttons shown directly |

**Scenario: conversation is mid-messages when player backs out**

| Re-entry | Expected |
|---|---|
| 1st re-entry | Pause button shown (safe resume point) → press it → messages replay → choice/end shown |
| Every re-entry after | Same as above until player actually progresses |

---

I think we should refactor this like this:

normal mdoe:

* Only show the pause button when it’s a real pause point.
* If it’s a choice, show only the choice.
* If it’s the end, show only the end button.
* Don’t replay messages unnecessarily if we’ve already reached a choice or the end.

My main issue is with fast mode:

* Every time we reach a choice or the end button, the pause button shows first before the choice or end button.
* Replaying is getting out of hand: when I reach the end and re-enter, it keeps replaying from the last pause point.


---

also in the fastmode when i select a choice it dont dissapear and stay the choices when it suppossed to dissapear aftter selecting a choice like how the behaviour on the normal mode
