// TODO: CharacterDatabase is serialized in multiple classes (GalleryController, ContactListPanel, ContactsAppPanel).
// Move it to GameBootstrap and use a shared reference to ensure a single source of truth.

---

// TODO: Investigate why cgImage is not respecting inspector offsets
// Question: I set cgImage to stretch with Top = 120, Bottom = 0, 
// but in Playmode it shows Top = 80, Bottom = 80. Why is this happening?

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

---

TODO: pooling
lockscreen.cs

logs

Internal: deleting an allocation that is older than its permitted lifetime of 4 frames (age = 10)

---

it’s almost certainly because of the **way you handle notifications** on the LockScreen. Specifically:

```csharp
foreach (Transform child in notificationContainer.transform)
{
    if (child.gameObject != moreIndicator)
        Destroy(child.gameObject);
}
```

Every time `PopulateNotifications()` runs, you **destroy old notification items** and instantiate new ones:

```csharp
GameObject item = Instantiate(notificationItemPrefab, notificationContainer.transform);
```

**Why this triggers the warning:**

* Unity tracks newly created allocations for safety. Some internal UI objects (like `Canvas`, `TextMeshProUGUI`, or `RectTransform`) are pooled internally for rendering optimization.
* Destroying them **too soon or frequently** can hit Unity’s internal memory safety checks. Here, Unity says “hey, this object is older than the minimum 4-frame safety window — fine, deleting now, but just FYI.”
* TMP objects are especially sensitive because they allocate internal buffers. Rapid create/destroy cycles are exactly the scenario Unity warns about.

**TL;DR:**

* This warning comes from your **dynamic creation and destruction of notification prefabs** each frame or scene start.
* It’s **not a crash**, but it’s Unity telling you: “You’re hitting the internal memory safety limits for object lifetimes.”

**Fix:** Use **object pooling** — keep a pool of notification items and **re-use them instead of destroying/instantiating every time**. This eliminates the warning and improves performance.

I can draft a **LockScreen notification pooling system** for you that fits your current setup. Do you want me to do that?
