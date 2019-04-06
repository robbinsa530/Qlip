using System;
using System.Runtime.InteropServices;

namespace Qlip.Native
{
    class HotKey
    {
        /// <summary>
        /// Member variables
        /// </summary>
        private int modifier;
        private int key;
        private IntPtr hWnd;
        private int id;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="modifier">Keys which must be pressed in addition to 'key' for hot key to be triggered.
        /// If providing more than one, they should be added together.</param>
        /// <param name="key">Key to be pressed in addition to 'modifier''s</param>
        /// <param name="win">Window to attach handler to</param>
        public HotKey(int modifier, int key, IntPtr elemHndl)
        {
            this.modifier = modifier;
            this.key = key;
            this.hWnd = elemHndl;
            id = this.GetHashCode();
        }

        /// <summary>
        /// Get hash code of instance
        /// </summary>
        /// <returns>hash code</returns>
        public override int GetHashCode()
        {
            return modifier ^ key ^ hWnd.ToInt32();
        }

        /// <summary>
        /// Register listener
        /// </summary>
        /// <returns>succeeded?</returns>
        public bool Register()
        {
            return RegisterHotKey(hWnd, id, modifier, key); 
        }

        /// <summary>
        /// Unregister listener
        /// </summary>
        /// <returns>succeeded?</returns>
        public bool Unregiser()
        {
            return UnregisterHotKey(hWnd, id);
        }

        /// <summary>
        /// Getter for id
        /// </summary>
        /// <returns>id</returns>
        public int getId()
        {
            return id;
        }

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);
 
        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    }
}
