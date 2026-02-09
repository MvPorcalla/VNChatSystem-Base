# 🔌 Setting Up Core Scripts on Your Hierarchy

## 📍 Overview: Where Each Script Goes

```
PhoneRoot
│
├── ContactListPanel
│   └── (CharacterButton setup comes later)
│
└── ChatAppPanel ← ADD ChatAppController.cs HERE
    │
    ├── ChatPanel ← ADD ChatMessageDisplay.cs HERE
    │
    └── ChatChoices ← ADD ChatChoiceDisplay.cs HERE
```

---

## 🎯 STEP 1: Add ChatMessageDisplay to ChatPanel

### **1.1 Select ChatPanel**
- In Hierarchy, find and select: `PhoneRoot > ChatAppPanel > ChatPanel`

### **1.2 Add Component**
- In Inspector, click **Add Component**
- Search: `ChatMessageDisplay`
- Click to add

### **1.3 Assign References in Inspector**

```
Chat Message Display (Script)
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[Message Prefabs]
systemBubblePrefab      → Drag: SystemContainer.prefab
npcTextBubblePrefab     → Drag: NpcChatContainer.prefab
npcImageBubblePrefab    → Drag: NpcCGContainer.prefab
playerTextBubblePrefab  → Drag: PlayerChatContainer.prefab
playerImageBubblePrefab → Drag: PlayerCGContainer.prefab

[Content Container]
chatContent             → Drag: Content (from Hierarchy)
                           Path: ChatPanel > Viewport > Content
```

**How to assign:**
- **Prefabs:** Drag from **Project** window
- **chatContent:** Drag `Content` GameObject from **Hierarchy**

---

## 🎯 STEP 2: Add ChatChoiceDisplay to ChatChoices

### **2.1 Select ChatChoices**
- In Hierarchy: `PhoneRoot > ChatAppPanel > ChatChoices`

### **2.2 Add Component**
- Add Component → `ChatChoiceDisplay`

### **2.3 Assign References**

```
Chat Choice Display (Script)
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[Prefabs]
choiceButtonPrefab    → Drag: ChoiceButton.prefab (from Project)
continueButtonPrefab  → Drag: ContinueButton.prefab (from Project)*

[Container]
choiceContainer       → Drag: ChatChoices (this GameObject itself)
                         Just drag from Hierarchy onto this field
```

**\*Note:** If you don't have `ContinueButton.prefab` yet:
1. Duplicate `ChoiceButton.prefab` in Project
2. Rename to `ContinueButton.prefab`
3. Open it and change button text to `"..."`

---

## 🎯 STEP 3: Add ChatAppController to ChatAppPanel

### **3.1 Select ChatAppPanel**
- In Hierarchy: `PhoneRoot > ChatAppPanel`

### **3.2 Add Component**
- Add Component → `ChatAppController`

### **3.3 Assign ALL References** (This is the big one!)

```
Chat App Controller (Script)
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[Panels]
contactListPanel → Drag: ContactListPanel (from Hierarchy)
chatAppPanel     → Drag: ChatAppPanel (this GameObject)

[Chat Header]
chatBackButton   → Drag: ChatBackButton (from Hierarchy)
chatProfileIMG   → Drag: ChatProfileIMG (from Hierarchy)
chatProfileName  → Drag: ChatProfileName (from Hierarchy)

[Chat Mode Toggle]
chatModeButton   → Drag: ChatModeToggle (from Hierarchy)
chatModeIcon     → Drag: ChatModeIcon (from Hierarchy)
fastModeSprite   → Drag: Image
normalModeSprite → Drag: Image

[Chat Display]
chatScrollRect   → Drag: ChatPanel (the ScrollRect component)
chatContent      → Drag: Content (from ChatPanel > Viewport > Content)
messageDisplay   → Drag: ChatPanel (the ChatMessageDisplay component)
choiceDisplay    → Drag: ChatChoices (the ChatChoiceDisplay component)

[Typing Indicator]
typingIndicator  → Drag: TypingIndicator (from Hierarchy)

[New Message Indicator]
newMessageIndicator → Drag: NewMessageIndicator (from Hierarchy)
newMessageText      → Drag: IndicatorText (from Hierarchy)
```

**ChatTimingController.cs:** (Script)
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

```
Timing Settings:
├── Message Delay: 1.2
├── Typing Indicator Duration: 1.5
├── Player Message Delay: 0.3
└── Final Delay Before Choices: 0.2

Fast Mode:
├── Is Fast Mode: false (default)
└── Fast Mode Speed: 0.1

References:
├── Message Display: ChatPanel (ChatMessageDisplay component)
└── Typing Indicator: TypingIndicator (GameObject under Content)
```

### **ChatAutoScroll.cs:** (Script)
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

```
References:
└── Chat Scroll Rect: ChatPanel or Viewport (ScrollRect component)

Settings:
├── Auto Scroll Enabled: true
└── Bottom Threshold: 0.01
```

---

## 📋 Visual Assignment Guide

### **Finding Components vs GameObjects:**

When the field type is:

**`GameObject`** → Drag the GameObject itself
```
contactListPanel → ContactListPanel (the whole GameObject)
```

**`Component` (Button, Image, TMP, etc.)** → Drag the GameObject, Unity auto-finds component
```
chatBackButton → Drag ChatBackButton GameObject
                 (Unity finds the Button component automatically)
```

**`Custom Script Component`** → Drag the GameObject that HAS that script
```
messageDisplay → Drag ChatPanel
                 (Unity finds ChatMessageDisplay component on it)
```

---

## 🎨 Step-by-Step Visual Path Guide

### **For chatContent:**
```
Hierarchy Path:
PhoneRoot
└─ ChatAppPanel
   └─ ChatPanel
      └─ Viewport
         └─ Content ← Drag this!
```

### **For chatScrollRect:**
```
Hierarchy Path:
PhoneRoot
└─ ChatAppPanel
   └─ ChatPanel ← Drag this! (has ScrollRect component)
```

### **For messageDisplay:**
```
Hierarchy Path:
PhoneRoot
└─ ChatAppPanel
   └─ ChatPanel ← Drag this! (has ChatMessageDisplay component)
```

### **For choiceDisplay:**
```
Hierarchy Path:
PhoneRoot
└─ ChatAppPanel
   └─ ChatChoices ← Drag this! (has ChatChoiceDisplay component)
```

### **For typingIndicator:**
```
Hierarchy Path:
PhoneRoot
└─ ChatAppPanel
   └─ ChatPanel
      └─ Viewport
         └─ Content
            └─ TypingIndicator ← Drag this!
```

---

## ✅ Verification Checklist

After assigning everything, verify in Inspector:

### **ChatMessageDisplay (on ChatPanel):**
```
☐ All 5 prefab slots filled (no "None")
☐ chatContent assigned
```

### **ChatChoiceDisplay (on ChatChoices):**
```
☐ choiceButtonPrefab assigned
☐ continueButtonPrefab assigned
☐ choiceContainer assigned (should say "ChatChoices")
```

### **ChatAppController (on ChatAppPanel):**
```
☐ contactListPanel assigned
☐ chatAppPanel assigned
☐ chatBackButton assigned
☐ chatProfileIMG assigned
☐ chatProfileName assigned
☐ chatModeToggle assigned
☐ chatScrollRect assigned
☐ chatContent assigned
☐ messageDisplay assigned (shows "Chat Panel (Chat Message Display)")
☐ choiceDisplay assigned (shows "Chat Choices (Chat Choice Display)")
☐ typingIndicator assigned
☐ newMessageIndicator assigned
☐ newMessageText assigned
```

**Total: 18 fields should be assigned on ChatAppController!**

---

## 🚨 Common Mistakes to Avoid

### ❌ **Wrong:**
```
messageDisplay → Dragging ChatMessageDisplay.cs file from Project
```
### ✅ **Correct:**
```
messageDisplay → Dragging ChatPanel GameObject from Hierarchy
                 (which has ChatMessageDisplay component on it)
```

---

### ❌ **Wrong:**
```
chatContent → Dragging the Viewport
```
### ✅ **Correct:**
```
chatContent → Dragging Content (the child INSIDE Viewport)
```

---

### ❌ **Wrong:**
```
choiceContainer → Leaving empty or dragging something else
```
### ✅ **Correct:**
```
choiceContainer → Dragging ChatChoices itself (the GameObject the script is on)
```

---

## 🎯 Final Scene Setup

After all assignments, your scene should look like this:

```
PhoneRoot
│
├── ContactListPanel
│   └── ContactScroll
│       └── Viewport
│           └─ Content (empty - will be populated later)
│
└── ChatAppPanel [ChatAppController]
    │
    ├── ChatHeader
    │   ├── ChatBackButton
    │   ├── ChatProfileIMG
    │   ├── ChatProfileName
    │   └── ChatModeToggle
    │
    ├── ChatPanel [ChatMessageDisplay, ScrollRect]
    │   └── Viewport
    │       └── Content (empty except TypingIndicator)
    │           └─ TypingIndicator (disabled by default)
    │
    └── ChatChoices [ChatChoiceDisplay]
        └── (empty - buttons spawn here)
```

---

## 🧪 Quick Test

After setup, you can test if everything is wired correctly:

1. **Select ChatAppPanel** in Hierarchy
2. **Look at ChatAppController** component
3. **Click the small circle** next to each field
4. **It should highlight the assigned object** in the Hierarchy or Project

If clicking does nothing or shows "None", that field isn't assigned correctly.

---

## 📸 Screenshot Recommendation

**Take a screenshot of your ChatAppController Inspector** after filling everything out, so you have a reference if something breaks later!

---

**Ready to move on? Once all these are assigned, you can create a test conversation!** 🚀