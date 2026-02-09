## How to Configure in Inspector:

### For Scene-Loading Apps (Chat):
```
Apps → Element 0
├── Enabled: ✓
├── App Name: "Chat"
├── Button: AppButton_Chat
├── Target Scene: "04_ChatApp"  ← Fill this
└── Target Panel: None           ← Leave empty
```

### For In-Scene Panels (Contacts, Settings):
```
Apps → Element 1
├── Enabled: ✓
├── App Name: "Contacts"
├── Button: AppButton_Contacts
├── Target Scene: ""              ← Leave empty
└── Target Panel: ContactsPanel   ← Drag panel here

Apps → Element 2
├── Enabled: ✓
├── App Name: "Settings"
├── Button: AppButton_Settings
├── Target Scene: ""              ← Leave empty
└── Target Panel: SettingsPanel   ← Drag panel here