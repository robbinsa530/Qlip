using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qlip
{
    public class Model
    {
        private List<string> clipboardHistory;
        private int current = 0;

        public Model()
        {
            clipboardHistory = new List<string>(40);
        }

        public void AddNewClip(string clip)
        {
            clipboardHistory.Add(clip);
            if (clipboardHistory.Count > 40)
                clipboardHistory.RemoveAt(0);
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

        public string GetMostRecentClip()
        {
            return clipboardHistory.Count > 0 ? clipboardHistory.ElementAt(clipboardHistory.Count - 1) : "";
        }

        public int GetCurrentForDisplay()
        {
            return current + 1;
        }

        public int GetCountForDisplay()
        {
            return clipboardHistory.Count;
        }
    }
}
