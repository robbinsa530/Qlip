using System.Collections.Generic;
using System.Linq;

namespace Qlip
{
    public class Model
    {
        private List<string> clipboardHistory;
        private int current = 0;
        private ConfigHelper config;

        public bool ResetOnPaste() { return config.config.reset_on_paste; }
        public bool ResetOnCancel() { return config.config.reset_on_cancel; }
        public int PasteTimeout() { return config.config.paste_timeout; }
        public bool MovePastedToFront() { return config.config.move_pasted_to_front; }


        public Model()
        {
            config = new ConfigHelper();
            clipboardHistory = new List<string>(config.config.save_count);
        }

        public void AddNewClip(string clip)
        {
            if (clip.Length > 0)
            {
                clipboardHistory.Insert(0, clip);
                if (clipboardHistory.Count > config.config.save_count)
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
