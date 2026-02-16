# 📦 Addressables - Complete First-Time Setup Guide

A step-by-step guide for someone who has **never used Addressables before**.

---

## 🎯 **PART 1: Install Addressables Package**

### **Step 1: Open Package Manager**

1. In Unity, click the top menu: **Window**
2. Click **Package Manager**
3. A new window opens titled "Package Manager"

### **Step 2: Find Addressables**

In the Package Manager window:

1. Look for a dropdown at the top-left that says "Packages: In Project" or similar
2. Click it and select **"Unity Registry"**
3. Wait a few seconds for the list to load
4. Scroll down until you find **"Addressables"**
   - Full name: "Addressables" or "com.unity.addressables"

### **Step 3: Install**

1. Click on **"Addressables"** in the list
2. On the right side, you'll see package details
3. Click the **"Install"** button (bottom-right)
4. Wait for installation (progress bar appears at bottom)
5. When done, close Package Manager

**✅ Checkpoint:** Console should say "Package Manager resolve complete" (no errors)

---

## 🎯 **PART 2: Initialize Addressables in Your Project**

### **Step 4: Open Addressables Groups Window**

1. Top menu: **Window**
2. Hover over **Asset Management**
3. Hover over **Addressables**
4. Click **Groups**

**You'll see one of two things:**

---

### **Scenario A: First Time (Most Likely)**

A window appears with a button that says:

**"Create Addressables Settings"**

**→ Click this button!**

Unity will:
- Create necessary folders
- Generate settings files
- Initialize Addressables system

**✅ You'll now see the Addressables Groups window with:**
```
Addressables Groups

Built In Data
└── (some default stuff)

Default Local Group (Default)
└── (empty)
```

---

### **Scenario B: Already Initialized**

You see the **Addressables Groups** window directly (no setup button).

**→ Skip to Step 5!**

---

## 🎯 **PART 3: Create Your CGs Group**

### **Step 5: Create a New Group**

In the **Addressables Groups** window:

1. Look for a button that says **"Create New Group"**
   - Usually bottom-left area OR in a toolbar
2. Click it
3. A new group appears with a default name like "New Group"

### **Step 6: Rename the Group**

1. **Right-click** on the new group
2. Select **"Rename Group"** (or click it and press F2)
3. Type: `CGs`
4. Press **Enter**

**✅ You should now see:**
```
Addressables Groups

Built In Data
Default Local Group (Default)
CGs ← Your new group!
```

---

## 🎯 **PART 4: Add Your Sofia CG Images**

### **Step 7: Navigate to Sofia Folder**

In the **Project** window (usually bottom of Unity):

1. Navigate to: `Assets` → `Characters` → `Sofia`
2. You should see your 4 CG images:
   - ChatA_CG1.png
   - ChatA_CG2.png
   - ChatA_CG3.png
   - ChatA_CG4.png

---

### **Step 8: Add Images (Method A - Drag & Drop)**

**Keep both windows visible side-by-side:**
- **Project** window (showing Sofia folder)
- **Addressables Groups** window (showing CGs group)

**Now:**

1. In **Project** window, click `ChatA_CG1.png`
2. **Hold and drag** it to the **CGs** group in Addressables window
3. **Release** (drop it in the CGs group)
4. The image appears in the CGs group!

**Repeat for the other 3 images:**
- Drag `ChatA_CG2.png` to CGs group
- Drag `ChatA_CG3.png` to CGs group
- Drag `ChatA_CG4.png` to CGs group

**✅ You should now see:**
```
Addressables Groups

CGs
├── ChatA_CG1 (Sprite)
├── ChatA_CG2 (Sprite)
├── ChatA_CG3 (Sprite)
└── ChatA_CG4 (Sprite)
```

---

### **Alternative: Step 8 (Method B - Inspector Checkbox)**

If drag & drop doesn't work:

1. In **Project** window, select `ChatA_CG1.png`
2. Look at the **Inspector** window (usually right side)
3. At the very top, find a checkbox labeled **"Addressable"**
4. ✅ **Check it**
5. New fields appear below
6. Repeat for all 4 images

---

## 🎯 **PART 5: Set Addressable Keys (Names)**

### **Step 9: Rename Each Image Entry**

The default names (ChatA_CG1, ChatA_CG2, etc.) aren't great. Let's change them.

**For ChatA_CG1:**

1. In **Addressables Groups** window, **click** on `ChatA_CG1`
2. Look at the **very top** of the Addressables window
3. You'll see a field labeled **"Address:"** with the text "ChatA_CG1"
4. **Click in this field** and change it to: `Sofia/CG1`
5. Press **Enter**

**Repeat for the others:**

| Old Name | New Address |
|----------|-------------|
| ChatA_CG1 | `Sofia/CG1` |
| ChatA_CG2 | `Sofia/CG2` |
| ChatA_CG3 | `Sofia/CG3` |
| ChatA_CG4 | `Sofia/CG4` |

**✅ You should now see:**
```
Addressables Groups

CGs
├── Sofia/CG1 (Sprite)
├── Sofia/CG2 (Sprite)
├── Sofia/CG3 (Sprite)
└── Sofia/CG4 (Sprite)
```

---

## 🎯 **PART 6: Build Addressables (CRITICAL!)**

### **Step 10: Build**

**This is the most important step! Without building, images won't load!**

In the **Addressables Groups** window:

1. Look at the top toolbar/menu
2. Find and click the **"Build"** menu
3. A dropdown appears with options
4. Click **"New Build"**
5. Another menu appears
6. Click **"Default Build Script"**

**What happens:**
- Unity starts building
- Console shows progress messages
- After a few seconds: "Build completed in X.XX seconds"

**✅ Success indicators:**
- Console says "Build completed"
- No red error messages
- A new folder appears: `Assets/ServerData/` or `Assets/AddressableAssetsData/`

---

## 🎯 **PART 7: Update Your .bub File**

### **Step 11: Edit sofia_Chapter1.bub**

Open your `sofia_Chapter1.bub` file and add CG commands:

```bub
contact: Sofia

title: Start
---
System: "9:42 AM"

Sofia: "Hey! Check this out."

>> media npc type:image path:Sofia/CG1

-> ...

Sofia: "And here's another!"

>> media npc type:image path:Sofia/CG2

Sofia: "This one is special."

>> media npc type:image unlock:true path:Sofia/CG3

>> choice
    -> "Beautiful!"
        # Player: "That's beautiful!"
        <<jump End>>
    
    -> "Amazing!"
        # Player: "Amazing!"
        <<jump End>>

===

title: End
---
Sofia: "Thanks! 😊"
```

**Important:**
- Use `path:Sofia/CG1` (matches what you set in Step 9)
- No `.png` extension
- Exact spelling (case-sensitive)

---

## 🎯 **PART 8: Test**

### **Step 12: Play and Test**

1. Press **Play** in Unity
2. Navigate to Sofia's conversation
3. Watch the **Console** window

**You should see:**
```
[ImageMessageBubble] Loading: Sofia/CG1
[AddressablesImageLoader] Loading: Sofia/CG1
[AddressablesImageLoader] ✓ Loaded: Sofia/CG1
[ImageMessageBubble] ✓ Image loaded: Sofia/CG1
```

**If the image appears in chat:**
- ✅ **Success!** Click it to open fullscreen

**If you see errors:**
- See troubleshooting below

---

## 🐛 **Troubleshooting Common First-Time Issues**

### **Error: "Create Addressables Settings button doesn't appear"**

**Fix:**
1. Window → Asset Management → Addressables → Settings
2. If settings exist, skip back to Step 5
3. If not, try: Tools → Addressables → Create Settings

---

### **Error: "InvalidKeyException: Sofia/CG1"**

**Cause:** Addressables not built OR key doesn't exist

**Fix:**
1. Open Addressables Groups window
2. Verify `Sofia/CG1` exists in the list
3. Build → New Build → Default Build Script
4. Try again

---

### **Error: "Can't drag images to Addressables window"**

**Fix:** Use Inspector method instead:
1. Select image in Project
2. Check "Addressable" checkbox in Inspector
3. Set Address field to `Sofia/CG1`

---

### **Error: "Image doesn't appear in chat"**

**Check these:**
1. ✅ Addressables built? (Build → New Build)
2. ✅ .bub file has correct path? (`path:Sofia/CG1` not `path:ChatA_CG1`)
3. ✅ Image is in Addressables Groups window?
4. ✅ Check Console for error messages

---

### **Error: "Build menu is grayed out"**

**Fix:**
1. Close Addressables window
2. Window → Asset Management → Addressables → Groups
3. Try Build again

---

## 📋 **Quick Checklist - Did You Do Everything?**

- [ ] Installed Addressables package
- [ ] Clicked "Create Addressables Settings"
- [ ] Created "CGs" group
- [ ] Added all 4 Sofia images to CGs group
- [ ] Renamed addresses: Sofia/CG1, Sofia/CG2, Sofia/CG3, Sofia/CG4
- [ ] Built Addressables (Build → New Build)
- [ ] Updated .bub file with correct paths
- [ ] Tested in Play mode

---

## 🎯 **What You Just Learned:**

✅ How to install and initialize Addressables
✅ How to create groups
✅ How to add assets (images) to Addressables
✅ How to set Addressable keys/addresses
✅ How to build Addressables
✅ How to reference them in .bub files

---

## 🔄 **For Your Next Character:**

When you add a new character (e.g., Emma):

1. Create their folder: `Assets/Characters/Emma/`
2. Add their CGs to the **same CGs group**
3. Set addresses: `Emma/CG1`, `Emma/CG2`, etc.
4. **Build** → **Update a Previous Build** (faster than New Build)
5. Use in .bub: `path:Emma/CG1`

---

## 🎉 **You're Done!**

You've successfully set up Addressables for the first time! Your CGs should now load in your chat game.

**Still stuck?** Tell me exactly what you see on your screen and I'll help! 🚀