// TODO: CharacterDatabase is serialized in multiple classes (GalleryController, ContactListPanel, ContactsAppPanel).
// Move it to GameBootstrap and use a shared reference to ensure a single source of truth.

---

// TODO: Investigate why cgImage is not respecting inspector offsets
// Question: I set cgImage to stretch with Top = 120, Bottom = 0, 
// but in Playmode it shows Top = 80, Bottom = 80. Why is this happening?

---

// ──────────────────────────────────────────────────────────────────────────────
// TODO: LOGGING SETUP
// ──────────────────────────────────────────────────────────────────────────────
//
// TODO: Logging safety
// - Question: What happens if GameBootstrap.Config is null or not assigned?
// - Answer: Debug logs will throw a null reference error or won't respect toggles.
// - Solution: Default to false when GameBootstrap.Config is null to prevent errors:
//     Example: if (GameBootstrap.Config?.galleryAppDebugLogs ?? false) { ... }

---

// TODO: Logging Helper
// - Goal: Refactor repetitive logging code into a reusable helper function.
// - Benefits: 
//     • Avoid repeating GameBootstrap.Config checks in every class
//     • Centralize log formatting and conditional toggles
//     • Make it easy to update log behavior globally

---

// TODO: Review Chat App autoscroll behavior.
// Issue: At the start, the autoscroller doesn’t work correctly — you need to swipe up manually to activate it.
//        The new message indicator keeps showing initially.
// Cause (suspected): The messages haven’t fully rendered/passed the visible area yet when the chat initializes.
// Action: Ensure the autoscroller properly calculates content height and triggers the scroll-to-bottom behavior on startup.