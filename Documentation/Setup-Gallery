# 🎯 Gallery Setup - Serialized Fields Guide

---

## 📋 **1. Gallery Controller (Script)**

**Location:** Attach to `GalleryPanel` GameObject

```
Gallery Controller (Script)
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[Gallery UI]
contentContainer        → Drag: Content (from GalleryPanel > ScrollView > Viewport > Content)
progressText            → Drag: ProgressText (from GalleryPanel)

[Prefabs]
characterSectionPrefab  → Drag: CGContainer (from Project folder)
thumbnailPrefab         → Drag: CGThumbnail (from Project folder)

[Character Data]
characterDatabase → Drag: CharacterDatabase.asset (from Project folder)

[Display Options]
showLockedCGs           → ☑ Checked
showEmptySections       → ☐ Unchecked
lockedCGSprite          → Drag: (Optional placeholder sprite from Project)

[Fullscreen Viewer]
fullscreenViewer        → Drag: GalleryFullscreenViewer (from Hierarchy)
```

---

## 📋 **2. Gallery Fullscreen Viewer (Script)**

**Location:** Attach to `GalleryFullscreenViewer` GameObject

**⚠️ IMPORTANT:** Also add `Canvas Group` component to the same GameObject!

```
Gallery Fullscreen Viewer (Script)
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[UI Elements]
viewerPanel  → Drag: GalleryFullscreenViewer (this GameObject - itself)
cgImage      → Drag: CGImage (from ImageContainer > CGImage)
closeButton  → Drag: CloseButton (from TopBar > CloseButton)
cgNameText   → Drag: CGNameText (from TopBar > CGNameText)
canvasGroup  → Auto-assigns (Canvas Group component on same object)

[Background]
backgroundOverlay → Drag: BackgroundOverlay (from Hierarchy)

[Zoom Settings]
minZoom        → 1
maxZoom        → 3
zoomSpeed      → 0.1
doubleTapZoom  → 2
doubleTapTime  → 0.3

[Pan Settings]
enablePanLimits → ☑ Checked

[Animation]
fadeDuration → 0.3
```

---

## 📋 **3. Gallery Thumbnail Item (Script)**

**Location:** Attach to `CGThumbnail` **Prefab** (in Project folder, NOT Hierarchy)

**⚠️ IMPORTANT:** Also ensure `Button` component is on the same GameObject!

```
Gallery Thumbnail Item (Script)
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[UI References]
thumbnailImage → Drag: ThumbnailImage (child of CGThumbnail)
lockedOverlay  → Drag: LockedOverlay (child of CGThumbnail)
```

**How to configure the prefab:**
1. In **Project** window, double-click `CGThumbnail` prefab
2. Select root `CGThumbnail` GameObject
3. Assign the two child references
4. **File > Save** (or Ctrl+S) to save prefab
5. Close prefab mode

---

## 🎯 **Component Checklist**

### **On GalleryPanel:**
- [ ] `GalleryController` script attached
- [ ] All 8 serialized fields assigned

### **On GalleryFullscreenViewer:**
- [ ] `GalleryFullscreenViewer` script attached
- [ ] `Canvas Group` component added
- [ ] All 11 serialized fields assigned

### **On CGThumbnail Prefab:**
- [ ] `Button` component present
- [ ] `GalleryThumbnailItem` script attached
- [ ] Both 2 child references assigned
- [ ] Prefab saved

---

## 📐 **Hierarchy Reference**

```
GalleryPanel [GalleryController]
├── Header
├── ProgressText ← Reference this
├── ScrollView
│   └── Viewport
│       └── Content ← Reference this
│
└── GalleryFullscreenViewer [GalleryFullscreenViewer + Canvas Group]
    ├── BackgroundOverlay ← Reference this
    ├── ImageContainer
    │   └── CGImage ← Reference this
    └── TopBar
        ├── CloseButton ← Reference this
        └── CGNameText ← Reference this
```

```
Project/Prefabs/Gallery/
├── CGContainer.prefab (character section)
│   ├── CharacterName
│   └── CGGrid
│
└── CGThumbnail.prefab [Button + GalleryThumbnailItem]
    ├── Background
    ├── ThumbnailImage ← Reference this (in prefab)
    └── LockedOverlay ← Reference this (in prefab)
```

---

## ⚠️ **Critical Notes**

1. **ConversationAssets must have `cgAddressableKeys` populated!**
   - Open each asset in Inspector
   - Check that the `Cg Addressable Keys` list has entries like:
     - `Sofia/CG1`
     - `Sofia/CG2`
     - etc.

2. **Canvas Group is required for fade animations!**
   - Without it, the viewer won't fade in/out properly

3. **Configure the PREFAB, not hierarchy instances!**
   - CGThumbnail must be configured in Project view prefab mode

4. **Drag from correct location:**
   - Hierarchy → Hierarchy (for scene objects)
   - Project → Inspector (for prefabs and assets)

---

**Ready to test? Let me know if any field is unclear!** 🎨