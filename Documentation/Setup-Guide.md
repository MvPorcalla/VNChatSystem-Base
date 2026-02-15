# Phone Chat Simulation Game - Unity Setup Guide

## ✅ Step-by-Step Implementation

### Phase 1: Create Folder Structure

```
Assets/
├── Scenes/
│   ├── 00_Disclaimer.unity
│   ├── 01_Bootstrap.unity
│   ├── 02_Lockscreen.unity
│   ├── 03_PhoneScreen.unity
│   └── 04_ChatApp.unity

```
---

Add AutoResizeText to Bubble Prefabs:
For each TEXT bubble prefab:
NpcTextBubble:
└── NpcBubble
    └── NpcMessage [TextMeshProUGUI]
        ├── [AutoResizeText.cs] ← ADD THIS
        └── [LayoutElement] ← Should already exist
Do this for:

SystemBubble → SystemMessage TMP
NpcTextBubble → NpcMessage TMP
PlayerTextBubble → PlayerMessage TMP

Settings for AutoResizeText:

Max Width: 650
Min Width: 40
Width Change Threshold: 0.1

---

```
Assets/Scripts/
├── BubbleSpinner/                    # ✅ NEW: Standalone dialogue module
│   ├── Core/
│   │   ├── BubbleSpinnerParser.cs           # Parses .bub files
│   │   ├── DialogueExecutor.cs              # Executes nodes, handles flow
│   │   └── ConversationManager.cs           # Integrates with GameBootstrap
│   │
│   ├── Data/
│   │   ├── DialogueNode.cs                  # Node structure
│   │   ├── MessageData.cs                   # Message/Choice/CG data
│   │   └── ConversationAsset.cs             # ScriptableObject (replaces NPCChatData)
│   │
│   └── Events/
│       └── DialogueEvents.cs                # Dialogue-specific events (Dont have this yet)
│
├── Core/                              # Your existing bootstrap
│   ├── GameBootstrap.cs               # ✅ UPDATED: Adds ConversationManager
│   ├── GameEvents.cs                  # ✅ UPDATED: Adds dialogue events
│   ├── SaveManager.cs                 # ✅ UPDATED: Handles ConversationState
│   ├── SceneFlowManager.cs
│   └── Scenename.cs
│
├── Data/
│   ├── Config/
│   │   └── 
│   └── SaveData.cs
│
└── UI/
    ├── ChatAppUI/
    │   ├── Components/
    │   │   ├── AutoResizeText
    │   │   ├── ChoiceButton.cs             <- Individual choice button
    │   │   └── MessageBubble.cs            <- Individual bubble behavior
    │   │
    │   ├── Core/
    │   │   ├── ChatAppController.cs        <- Main controller (interfaces with BubbleSpinner)
    │   │   ├── ChatAutoScroller.cs           <- 
    │   │   ├── ChatChoiceSpawner.cs        <- Handles choice button spawning
    │   │   ├── ChatMessageSpawner.cs       <- Handles message bubble spawning/animation
    │   │   ├── ChatTimingController.cs     <- 
    │   │   ├── PooledObject.cs             <- 
    │   │   └── PoolingManager.cs           <- 
    │   │
    │   └── Panels/
    │       ├── ContactListPanel.cs         <- Contact list UI
    │       ├── ChatAppPanel.cs             <- future
    │       └── ContactListItem.cs          <- Contact list item

    ├── UIManager
    │   ├── ChatAppUIManager.cs
    │   ├── LockScreen.cs
    │   └── PhoneScreenManager.cs

    └── DisclaimerScreen.cs
```

## 📋 Setup Checklist

### **1. Create Script Files**
```
✅ ChatAppController.cs → Attach to ChatAppPanel
✅ ChatMessageSpawner.cs → Attach to ChatPanel
✅ ChatChoiceSpawner.cs → Attach to ChatChoices
✅ MessageBubble.cs → Attach to all bubble prefabs
✅ ChoiceButton.cs → Attach to ChoiceButton prefab
```

### **2. Assign Inspector References**
```
ChatAppController:
  ✅ contactListPanel
  ✅ chatAppPanel
  ✅ chatBackButton
  ✅ chatProfileIMG
  ✅ chatProfileName
  ✅ chatModeToggle
  ✅ chatScrollRect
  ✅ chatContent
  ✅ messageDisplay (ChatMessageSpawner component)
  ✅ choiceDisplay (ChatChoiceSpawner component)
  ✅ typingIndicator
  ✅ newMessageIndicator

ChatMessageSpawner:
  ✅ systemBubblePrefab
  ✅ npcTextBubblePrefab
  ✅ npcImageBubblePrefab
  ✅ playerTextBubblePrefab
  ✅ playerImageBubblePrefab
  ✅ chatContent

ChatChoiceSpawner:
  ✅ choiceButtonPrefab
  ✅ continueButtonPrefab
  ✅ choiceContainer (this.transform)
```

### **3. Create Prefabs**
```
✅ SystemBubble prefab (with MessageBubble.cs)
✅ NpcTextBubble prefab (with MessageBubble.cs)
✅ NpcImageBubble prefab (with MessageBubble.cs)
✅ PlayerTextBubble prefab (with MessageBubble.cs)
✅ PlayerImageBubble prefab (with MessageBubble.cs)
✅ ChoiceButton prefab (with ChoiceButton.cs)
✅ ContinueButton prefab (with ChoiceButton.cs)

---

```
Assets/Scripts/

│
└── UI/
    └── Chat/
        ├── ChatDisplayController.cs   # ✅ NEW: Replaces ChatManager (UI only)
        └── CGGalleryManager.cs        # ✅ UPDATED: Listens to GameEvents
```

---

## Phase 2: Build Settings Configuration

1. **Open Build Settings** (Ctrl+Shift+B / Cmd+Shift+B)
2. **Add scenes in this EXACT order:**
   - `00_Disclaimer` (index 0)
   - `01_Bootstrap` (index 1)  
   - `02_Lockscreen` (index 2)
   - `03_PhoneScreen` (index 3)
   - `04_ChatApp` (index 4)

**⚠️ ORDER MATTERS!** Disclaimer MUST be index 0 (first scene loaded).

---

## Phase 3: Create 00_Disclaimer Scene

### Hierarchy Setup:
```
00_Disclaimer
├── Canvas (Screen Space - Overlay)
│   ├── DisclaimerPanel
│   │   ├── Title (TextMeshPro)
│   │   ├── Content (TextMeshPro)
│   │   ├── AgreeToggle (Toggle)
│   │   ├── ContinueButton (Button)
│   │   └── ExitButton (Button)
│   └── DisclaimerScreen (attach script here)
└── EventSystem
```

### Component Assignments:
1. Add `DisclaimerScreen.cs` to Canvas
2. Assign references in Inspector:
   - `Agree Toggle` → AgreeToggle
   - `Continue Button` → ContinueButton  
   - `Exit Button` → ExitButton
3. Set `Skip For Testing` = false (for production)
4. Set `Enable Debug Logs` = true (for development)

---

## Phase 4: Create 01_Bootstrap Scene

### Hierarchy Setup:
```
01_Bootstrap
└── GameBootstrap (GameObject)
    ├── SaveManager (child GameObject)
    └── SceneFlowManager (child GameObject)
```

### Step-by-Step:

1. **Create Main GameObject:**
   - Right-click Hierarchy → Create Empty
   - Name: `GameBootstrap`
   - Add Component: `GameBootstrap.cs`

2. **Create SaveManager:**
   - Right-click `GameBootstrap` → Create Empty
   - Name: `SaveManager`
   - Add Component: `SaveManager.cs`

3. **Create SceneFlowManager:**
   - Right-click `GameBootstrap` → Create Empty
   - Name: `SceneFlowManager`
   - Add Component: `SceneFlowManager.cs`

4. **Configure GameBootstrap:**
   - Set `Show Debug Logs` = true
   - Set `Minimum Load Time` = 1
   - Set `Disclaimer Scene` = "00_Disclaimer"

**⚠️ IMPORTANT:** No Camera, no Canvas, no EventSystem in Bootstrap scene!
It's purely for managers. This scene should be visually empty.

---

## Phase 5: Create Placeholder Scenes

### 02_Lockscreen:
```
02_Lockscreen
├── Canvas
│   └── (Your Lockscreen UI here)
└── EventSystem
```

### 03_PhoneScreen:
```
03_PhoneScreen
├── Canvas
│   └── (Your phone UI here)
└── EventSystem
```

### 04_ChatApp:
```
04_ChatApp
├── Canvas
│   └── (Your chat UI here)
└── EventSystem
```

---

## Phase 6: Testing The Flow

### Test 1: First Launch (Disclaimer → Bootstrap → Next Scene)
1. **Set 00_Disclaimer as startup scene** (right-click in Build Settings)
2. Press Play
3. **Expected behavior:**
   - Disclaimer shows
   - Continue button disabled
   - Toggle checkbox → Continue button enables
   - Click Continue → Loads Bootstrap
   - Bootstrap initializes → Loads next scene (Disclaimer/Lockscreen/PhoneScreen based on logic)

### Test 2: Second Launch (Bootstrap → Skip Disclaimer)
1. Press Play again
2. **Expected behavior:**
   - Disclaimer skips automatically
   - Bootstrap loads and initializes
   - Loads appropriate scene based on save state

### Test 3: Reset Disclaimer
1. Stop play mode
2. Right-click `DisclaimerScreen` in scene
3. Select "Reset Disclaimer"
4. Press Play → Disclaimer shows again

---

## Phase 7: Debugging Tools

### GameBootstrap Context Menu (Right-click in Inspector):
- **Validate Bootstrap** - Checks if all managers found
- **Log GameEvents Subscribers** - Shows active event listeners
- **Clear PlayerPrefs (Disclaimer)** - Resets disclaimer flag

### SaveManager Shortcuts (Play Mode):
- **F12** - Open save folder in file explorer
- **F11** - Delete save file (with confirmation)

### SaveManager Context Menu:
- **Open Save Folder** - Opens persistent data folder
- **Delete Save File** - Removes save
- **Print Save Info** - Shows save file details
- **Create Test Save** - Generates test save file

### DisclaimerScreen Shortcuts (Play Mode):
- **F10** - Force accept and continue to Bootstrap

---

## Phase 8: Verification Checklist

### ✅ Scene Setup:
- [ ] All 5 scenes created
- [ ] All scenes added to Build Settings in correct order
- [ ] 00_Disclaimer is index 0

### ✅ Bootstrap Scene:
- [ ] GameBootstrap GameObject exists
- [ ] SaveManager child exists with script
- [ ] SceneFlowManager child exists with script
- [ ] No Camera/Canvas/EventSystem in scene

### ✅ Disclaimer Scene:
- [ ] DisclaimerScreen attached to Canvas
- [ ] All UI references assigned
- [ ] Toggle and buttons work

### ✅ Scripts:
- [ ] All scripts in correct folders
- [ ] No compilation errors
- [ ] Namespaces correct (ChatSim.Core, ChatSim.Data, ChatSim.UI.Controllers)

---

## Common Issues & Solutions

### Issue: "SaveManager not found!"
**Solution:** SaveManager must be a CHILD of GameBootstrap GameObject

### Issue: "Disclaimer shows every time"
**Solution:** Check PlayerPrefs - may need to call MarkAccepted()

### Issue: "Scene doesn't load after Disclaimer"
**Solution:** Verify SceneNames.BOOTSTRAP = "01_Bootstrap" (exact match)

### Issue: "Bootstrap scene is visible"
**Solution:** Bootstrap should have no visuals - only GameObjects with scripts

---

## Next Steps After Setup

Once everything works:
1. Build PhoneScreen UI
2. Build ChatApp UI  
3. Implement ConversationManager (uncomment in GameBootstrap)
4. Add phone state management
5. Build message system

---

## Flow Diagram

```
User Launches Game
        ↓
┌───────────────────┐
│  00_Disclaimer    │ (First launch only)
│  - Show terms     │
│  - Get agreement  │
└────────┬──────────┘
         ↓ (Accept)
┌───────────────────┐
│  01_Bootstrap     │ (Persistent - DontDestroyOnLoad)
│  - Init Managers  │
│  - Load Save      │
└────────┬──────────┘
         ↓
    ┌────┴────┐
    │         │
    ↓         ↓
Lockscreen   PhoneScreen (based on save state)
```

---

Your core initialization is now complete! 🎉