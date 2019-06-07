using System;
using System.Collections.Generic;
using System.Linq;

namespace Qlip
{
    public class Model
    {
        private List<string> clipboardHistory;
        private int current = 0;

        public bool ResetOnPaste() { return Properties.Settings.Default.ResetOnPaste; }
        public bool ResetOnCancel() { return Properties.Settings.Default.ResetOnCancel; }
        public int PasteTimeoutMS()
        {
            return (int)Math.Floor(Properties.Settings.Default.PasteTimeout * 1000);
        }
        public bool MovePastedToFront() { return Properties.Settings.Default.MovePastedToFront; }


        public Model()
        {
            Properties.Settings.Default.PropertyChanged += PreferencesChanged; 
            clipboardHistory = new List<string>(Properties.Settings.Default.SaveCount);
        }

        private void PreferencesChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals("SaveCount"))
            {
                int newSaveCount = Properties.Settings.Default.SaveCount;
                if (clipboardHistory.Count > newSaveCount)
                {
                    TrimClipsFromEnd(clipboardHistory.Count - newSaveCount);
                }
            }
        }

        public void AddNewClip(string clip)
        {
            if (clip.Length > 0)
            {
                clipboardHistory.Insert(0, clip);
                if (clipboardHistory.Count > Properties.Settings.Default.SaveCount)
                {
                    clipboardHistory.RemoveAt(clipboardHistory.Count - 1);         
                }
            }
        }

        public void RemoveCurrentClip()
        {
            RemoveClip(current);
        }

        public void RemoveClip(int index)
        {
            if (index >= 0 && index < clipboardHistory.Count)
            {
                clipboardHistory.RemoveAt(index);
                if (current > 0 && current > clipboardHistory.Count - 1)
                {
                    current--;
                }
            }
        }

        public void TrimClipsFromEnd(int numToTrim)
        {
            int start = clipboardHistory.Count - numToTrim;
            if (start > 0)
            {
                clipboardHistory.RemoveRange(start, numToTrim);
                if (current > clipboardHistory.Count - 1)
                {
                    current = clipboardHistory.Count - 1;
                }
            }
        }

        public void ClearClips()
        {
            clipboardHistory.Clear();
        }

        public string GetCurrentClip()
        {
            if (clipboardHistory.Count > 0)
                return clipboardHistory.ElementAt(current);
            else
                return "";
        }

        public void Next()
        {
            if (clipboardHistory.Count > 0)
                current = (current+1) % clipboardHistory.Count;
        }

        public void Prev()
        {
            int c = current - 1;
            if (clipboardHistory.Count > 0)
                current = ((c < 0) ? clipboardHistory.Count - 1 : c) % clipboardHistory.Count;
        }

        public void Reset()
        {
            current = 0;
        }

        public void GoToEnd()
        {
            current = clipboardHistory.Count - 1;
        }

        public string GetMostRecentClip()
        {
            return clipboardHistory.Count > 0 ? clipboardHistory.ElementAt(0) : "";
        }

        public int GetCurrentForDisplay()
        {
            return clipboardHistory.Count == 0 ? 0 : current + 1;
        }

        public int GetCountForDisplay()
        {
            return clipboardHistory.Count;
        }
    }
}
