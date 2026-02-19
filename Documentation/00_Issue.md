# ISSUE:

---

TODO:

another is when i enter a chatapppanel i see the flicker of the content from empty to populating it


---

TODO:

## Prompt

Here’s the corrected and clearer version:

---

i have this issue where if i go out at mid convo and comback it show this `-> ...` button and if i press it it show the choice button or end button
it shouldnt do that it show the choice or end button right away `-> ...` this should only show if its a continue button

---

TODO:

simplify GalleryFullscreenViewer.cs

the backbutton in the PhoneHomescreen when i open full screen it directly send it back to homescreenpanel instead of back to gallerypanel like the closebutton
it should be when i open the fullscreen of image when iback from the phonehomescreen.cs 
fullscreen -> gallerypanel -> homescreen

consider making a GalleryController and put all the script there

---

TODO: 

Fix image fullscreen viewer there is 2 script maybve combine it and put in common folder for reusability
Fix confirmation dialogue currently its messy

---

What's wrong in ChatAppController
Problem 1: Chapter navigation logic is sitting in the UI layer.
This block doesn't belong in a UI controller:
csharpprivate void OnContinueToNextChapterClicked()
{
    state.currentChapterIndex++;
    state.currentNodeName = "Start";
    state.currentMessageIndex = 0;
    // ...
}
The UI is directly mutating ConversationState. That's DialogueExecutor's job. If you ever change how chapters work in BubbleSpinner, you now have to fix the UI too.
Problem 2: Panel navigation and conversation lifecycle are mixed.
SwitchToChatPanel, SwitchToContactList, OnPhoneBackPressed, OnPhoneHomePressed are navigation concerns. StartConversation, PerformConversationCleanup, executor subscriptions are conversation lifecycle concerns. They're all in one class.

What I'd suggest
One new method on DialogueExecutor in BubbleSpinner:
csharppublic void AdvanceToNextChapter()
That moves the state mutation out of the UI entirely.
And optionally extract panel switching into a lightweight ChatNavigationController — but that's lower priority than the chapter issue.