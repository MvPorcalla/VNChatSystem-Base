Here’s your **Contacts system documentation**, formatted in the same clean style as your Gallery reference.

---

## **Location:** Attach to `ContactsPanel` GameObject

```
Contacts App Panel (Script)
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[Prefabs]
contactsAppItemPrefab   → Drag: ContactsAppItem (from Project folder)

[Contacts UI]
contactContainer        → Drag: Content (from ContactsPanel > ScrollView > Viewport > Content)
Contact App Item Prefab → Drag: ContactsAppItem.prefab (from Project folder)

[Character Data]
characterDatabase       → Drag: CharacterDatabase.asset (from Project folder)

[Reset Options]
useConfirmationDialog   → ☑ Checked (recommended)
resetConfirmationDialog → Drag: ResetConfirmationDialog (from ContactsPanel)

[Debug / Optional]
autoRefreshOnEnable     → ☑ Checked (if available in your script)
```

---

## **Location:** Attach to `ContactsAppItem` prefab (Root)

```
Contacts App Item (Script)
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[UI References]
profileImage    → Drag: ProfileImage (child Image)
nameText        → Drag: InfoGroup/NameText (TMP_Text)
bioText         → Drag: InfoGroup/BioText (TMP_Text)
resetButton     → Drag: ResetButton (Button component)

[Runtime]
characterId     → (Set automatically at runtime — do not assign manually)
```

⚠ Do NOT manually hook the button’s OnClick in Inspector if your script wires it automatically.

---

## **Location:** Attach to `ResetConfirmationDialog` GameObject

```
Reset Confirmation Dialog (Script)
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[Dialog UI]
titleText       → Drag: DialogPanel/TitleText
messageText     → Drag: DialogPanel/MessageText
yesButton       → Drag: DialogPanel/YesButton
noButton        → Drag: DialogPanel/NoButton

[Behavior]
closeOnNo       → ☑ Checked
blockRaycasts   → ☑ Checked (Overlay must have Raycast Target ON)
```

---

## **ScrollView Setup (Important)**

**Location:** ContactsPanel > ScrollView > Viewport > Content

Add Components:

```
Vertical Layout Group
  - Control Child Size (Width) → ☑ Checked
  - Control Child Size (Height) → ☐ Optional
  - Spacing → Set as needed

Content Size Fitter
  - Vertical Fit → Preferred Size
```

---

## **Home Screen Hookup**

**Location:** HomeScreenPanel > AppGrid > AppButton_Contacts

Button → OnClick()

```
PhoneScreenManager.OpenScreen(ContactsPanel)
```

(or whatever your screen switch system uses)

Make sure:

* Only one screen is active at a time.
* ContactsPanel is inactive by default.

---

## Final Safety Check

* `ResetConfirmationDialog` → ❌ NOT active by default.
* `ContactsAppItem` → saved as prefab in Project folder.
* `CharacterDatabase` → contains valid character entries.
* `SaveManager` properly handles character-specific reset.
