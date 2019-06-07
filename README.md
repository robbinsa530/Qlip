# Qlip
Qlip is a simple but effective clipboard manager for Windows. It saves the contents of your clipboard as it changes and gives you quick access to a history of your clips in the order that they appeared on your clipboard.

You don't need to do anything extra to get your clipboard contents into Qlip. As long as Qlip is running, each time you copy a new piece of text, it will be added to your Qlip History. You can bring up the Qlip History Viewer with the global keyboard shortcut CTRL+SHIFT+V. Once open, you can cycle through your history with V, TAB, or the arrow keys. Paste the current clip by either lifting all keys, or by pressing ENTER. 

The Qlip preferences menu can be accessed via the Qlip icon in your system tray. Click on the Qlip icon and select "Open Preferences" to edit settings. Editable settings include:

| Setting              | Description           
|:---------------------|:-------------
| Save Count           | How many clips to hold on to in history 
| Reset on Paste       | Reset history viewer back to start each time you paste
| Reset On Cancel      | Reset history viewer back to start when you cancel a paste
| Paste Timeout        | Amount of time (in seconds) with no user action to wait before auto-pasting
| Move Pasted to Front | Move pasted clip to front of history viewer after paste 

### Full list of Qlip functionality (When Qlip History Viewer is open):
| Key         | Action           
|:------------|:-------------
| V           | Next Clip
| TAB         | Next Clip
| UP-Arrow    | Next Clip
| RIGHT-Arrow | Next Clip
| DOWN-Arrow  | Previous Clip 
| LEFT-Arrow  | Previous Clip
| ENTER       | Paste
| ESC         | Cancel Paste
| HOME        | Go To First Clip In History
| END         | Go To Last Clip In History
| DELETE      | Remove Current Clip From History
| BACKSPACE   | Remove Current Clip From History
| X           | Remove Current Clip From History

### Possible Future Improvements:
- [ ] Exploded view of all clips
- [ ] Allow for saving of binary clips (e.g. images, files, etc.)
- [ ] Make a better logo...