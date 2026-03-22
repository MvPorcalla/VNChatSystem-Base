Canvas
└── LockScreen                           ← LockScreen.cs
    └── Panel                            ← CanvasGroup → contentGroup
        ├── TimeText          (TMP)      ← timeText
        ├── DateText          (TMP)      ← dateText
        └── NotificationContainer        ← notificationContainer
            └── MoreIndicator  (TMP)     ← moreIndicator (set inactive by default)

Assets/Prefabs/
└── NotificationItem                     ← notificationItemPrefab
    ├── SenderText  (TMP)               ← must be named exactly "SenderText"
    └── PreviewText (TMP)               ← must be named exactly "PreviewText"