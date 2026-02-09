# Setup

## 🎯 Solution: Keep Container + Bubble Structure

Here's the **correct approach** for your case:

---

## 📦 Proper Prefab Structure (With Containers)

Your prefabs should look like this:

```
SystemContainer.prefab ← ADD MessageBubble.cs HERE
└─ SystemBubble
    └─ SystemMessage (TMP)

NpcChatContainer.prefab ← ADD MessageBubble.cs HERE
└─ NpcBubble
    └─ NpcMessage (TMP)

NpcCGContainer.prefab ← ADD MessageBubble.cs HERE
└─ NpcBubble
    └─ NpcImage (Image)

PlayerChatContainer.prefab ← ADD MessageBubble.cs HERE
└─ PlayerBubble
    └─ PlayerMessage (TMP)

PlayerCGContainer.prefab ← ADD MessageBubble.cs HERE
└─ PlayerBubble
    └─ PlayerImage (Image)
```

---

## 🛠️ How to Fix Your Current Prefabs

### **Step 1: 

---

### **Step 2: Add MessageBubble Script to Prefabs**

For each prefab:

#### **SystemBubble.prefab:**
1. **Open prefab** (double-click)
2. **Select ROOT** (`SystemContainer`)
3. **Add Component** → `MessageBubble`
4. **Add Component** → `Canvas Group` (to root)
5. **Assign references:**
   - `messageText` → `SystemMessage` (TextMeshProUGUI)
   - `messageImage` → None (leave empty)
   - `canvasGroup` → CanvasGroup component

#### **NpcTextBubble.prefab:**
1. **Select root** (`NpcChatContainer`)
2. **Add Component** → `MessageBubble`
3. **Add Component** → `Canvas Group`
4. **Assign:**
   - `messageText` → `NpcMessage`
   - `messageImage` → None
   - `canvasGroup` → CanvasGroup

#### **NpcImageBubble.prefab:**
1. **Select root** (`NpcCGContainer`)
2. **Add Component** → `MessageBubble`
3. **Add Component** → `Canvas Group`
4. **Assign:**
   - `messageText` → None (leave empty)
   - `messageImage` → `NpcImage` (Image component)
   - `canvasGroup` → CanvasGroup

#### **PlayerTextBubble.prefab:**
1. **Select root** (`PlayerChatContainer`)
2. **Add Component** → `MessageBubble`
3. **Add Component** → `Canvas Group`
4. **Assign:**
   - `messageText` → `PlayerMessage`
   - `messageImage` → None
   - `canvasGroup` → CanvasGroup

#### **PlayerImageBubble.prefab:**
1. **Select root** (`PlayerCGContainer`)
2. **Add Component** → `MessageBubble`
3. **Add Component** → `Canvas Group`
4. **Assign:**
   - `messageText` → None
   - `messageImage` → `PlayerImage`
   - `canvasGroup` → CanvasGroup

---

### **Step 3: Clean Up Scene Hierarchy**

**Remove all container instances from the scene:**

Your `Content` should be **empty** (except TypingIndicator):

```
Content (RectTransform) ← Bubbles spawn here
└─ TypingIndicator (keep this!)
```

**Delete these from scene:**
- ❌ SystemContainer
- ❌ NpcChatContainer
- ❌ NpcCGContainer
- ❌ PlayerChatContainer
- ❌ PlayerCGContainer

---

## 📐 Example: How Layout Should Work

Here's what a typical **NPC bubble prefab** structure looks like with layout:

```
NpcTextBubble.prefab (root)
├─ NpcChatContainer
│   ├─ RectTransform (anchor: left-aligned)
│   ├─ LayoutElement (preferredHeight: 60)
│   ├─ HorizontalLayoutGroup (childAlignment: MiddleLeft)
│   ├─ MessageBubble (script)
│   └─ CanvasGroup
│
└─ NpcBubble
    ├─ Image (bubble background)
    └─ NpcMessage (TextMeshProUGUI)
```

**Player bubble prefab** would be right-aligned:

```
PlayerTextBubble.prefab (root)
├─ PlayerChatContainer
│   ├─ RectTransform (anchor: right-aligned)
│   ├─ LayoutElement (preferredHeight: 60)
│   ├─ HorizontalLayoutGroup (childAlignment: MiddleRight)
│   ├─ MessageBubble (script)
│   └─ CanvasGroup
│
└─ PlayerBubble
    ├─ Image (bubble background)
    └─ PlayerMessage (TextMeshProUGUI)
```

---

## 🎨 Recommended Layout Settings

### **For NPC Bubbles (Left-Aligned):**

**NpcChatContainer (root):**
```
RectTransform:
  Anchors: Stretch (left)
  Pivot: (0, 0.5)

HorizontalLayoutGroup:
  Child Alignment: Middle Left
  Child Force Expand: Width ✓, Height ✗
  Padding: Left 10, Right 100

LayoutElement:
  Min Height: 60
  Preferred Height: -1 (flexible)
```

### **For Player Bubbles (Right-Aligned):**

**PlayerChatContainer (root):**
```
RectTransform:
  Anchors: Stretch (right)
  Pivot: (1, 0.5)

HorizontalLayoutGroup:
  Child Alignment: Middle Right
  Child Force Expand: Width ✓, Height ✗
  Padding: Left 100, Right 10

LayoutElement:
  Min Height: 60
  Preferred Height: -1 (flexible)
```

### **For System Messages (Centered):**

**SystemContainer (root):**
```
RectTransform:
  Anchors: Stretch
  Pivot: (0.5, 0.5)

HorizontalLayoutGroup:
  Child Alignment: Middle Center
  Child Force Expand: Width ✓, Height ✗

LayoutElement:
  Min Height: 40
```

---

## ✅ Final Checklist

After setup, verify:

```
☐ All container prefabs renamed to match bubble names
☐ MessageBubble component added to each prefab ROOT
☐ CanvasGroup component added to each prefab ROOT
☐ Layout components preserved in prefabs (HorizontalLayoutGroup, etc.)
☐ All containers removed from scene Content
☐ Content is empty except TypingIndicator
☐ Prefab references assigned in ChatMessageDisplay
```

---

## 🎯 Assignment in ChatMessageDisplay

Now assign your prefabs in the Inspector:

**ChatMessageDisplay (on ChatPanel):**
```
systemBubblePrefab      → SystemBubble.prefab (was SystemContainer)
npcTextBubblePrefab     → NpcTextBubble.prefab (was NpcChatContainer)
npcImageBubblePrefab    → NpcImageBubble.prefab (was NpcCGContainer)
playerTextBubblePrefab  → PlayerTextBubble.prefab (was PlayerChatContainer)
playerImageBubblePrefab → PlayerImageBubble.prefab (was PlayerCGContainer)
chatContent             → Content (RectTransform)
```

---

## 🚀 How It Works at Runtime

When a message comes in:

1. **ChatMessageDisplay** receives `MessageData`
2. **Selects correct prefab** based on speaker/type
3. **Spawns prefab** into `Content`: 
   ```csharp
   Instantiate(npcTextBubblePrefab, chatContent);
   ```
4. **Prefab brings its own layout settings** (left/right alignment)
5. **MessageBubble script** populates text/image
6. **CanvasGroup** fades in the bubble

---

## 💡 Key Insight

**Your containers aren't just containers — they're PART of the bubble!**

- Container = Layout wrapper
- Bubble = Visual element inside

**Both together = Complete bubble prefab**

So your instinct to keep them together was **100% correct**! We just needed to:
1. Rename them properly
2. Add the scripts
3. Remove instances from scene

---

**Does this make sense now? Ready to proceed with the setup?** 🎮