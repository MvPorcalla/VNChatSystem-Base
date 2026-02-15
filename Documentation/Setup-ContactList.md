## 🔧 Setup Steps

### **STEP 1: Create ContactListItem Prefab**

Your hierarchy already has a ContactListItem template. Now make it a prefab:
```
ContactListPanel
└─ ContactScroll
    └─ Viewport
        └─ Content
            └─ ContactListItem ← This one!
                ├─ ProfileIMG
                ├─ ProfileName
                └─ Badge
```

**Make it a prefab:**
1. Drag `ContactListItem` from Hierarchy to `Prefabs/ChatApp/UI/`
2. **Delete** ContactListItem from the scene (it will be spawned)
3. Open the prefab and add components:
   - Add Component → `ContactListItem` (the script)
   - Assign references in Inspector:
```
     button       → Button component (on root)
     profileIMG   → ProfileIMG (Image)
     profileName  → ProfileName (TMP)
     badge        → Badge (GameObject)
```

---

### **STEP 2: Setup ContactListPanel in Scene**

**Select ContactListPanel in Hierarchy:**

1. Add Component → `ContactListPanel`
2. Assign references:
```
Contact List Panel (Script)
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[UI References]
contactContainer        → Content (from ContactScroll/Viewport/Content)
ContactListItemPrefab   → ContactListItem.prefab (from Project)

[Available Conversations]
conversations           → (Leave empty for now, we'll add test data)

[Controller Reference]
chatController          → ChatAppPanel (the GameObject with ChatAppController)
```

---

### **STEP 3: Clean Scene Hierarchy**

After setup, your scene should look like:
```
ContactListPanel [ContactListPanel script]
├─ Header
│   └─ Title
└─ ContactScroll
    └─ Viewport
        └─ Content (empty - buttons spawn here)
```

**Remove** the ContactListItem template from scene if still there.

---

## 🎯 Inspector Assignment Guide

### **ContactListPanel (on ContactListPanel GameObject):**
```
☐ contactContainer → Content (RectTransform under ContactScroll/Viewport)
☐ ContactListItemPrefab → ContactListItem.prefab
☐ conversations → Size: 0 (we'll add test conversations later)
☐ chatController → ChatAppPanel (GameObject with ChatAppController)
```

### **ContactListItem.prefab:**
```
☐ button → Button component (on root)
☐ profileIMG → ProfileIMG (Image)
☐ profileName → ProfileName (TMP)
☐ badge → Badge (GameObject)