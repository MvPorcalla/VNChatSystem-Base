# ISSUE:

---

TODO:

another is when i enter a chatapppanel i see the flicker of the content from empty to populating it


---

TODO:

I’m noticing some behavior: DialougeExecutioner.cs

If I interrupt a conversation by backing out while it’s at a choice button or end button, the pause button appears first. Pressing the pause button then shows the choice or end button.

What’s unusual is that if I don’t press the pause button, go back, and re-enter the conversation, it replays the conversation from the last pause point up to the choice or end button.

Why is it doing this, and should I leave it as-is?

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

TODO: 

Note: BubbleSpinner is a standalone script for parsing `.bub` files. It connects to the UI through a bridge.

---