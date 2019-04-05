﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Forms;
using Qlip.Native;

namespace Qlip
{
    public sealed class SpongeWindow : NativeWindow
    {
        public event EventHandler<Message> WndProcCalled;
        private int[] codes;

        public SpongeWindow()
        {
            CreateHandle(new CreateParams());
            codes = new int[] { KeyCodes.WM_HOTKEY_MSG_ID, KeyCodes.WM_DRAWCLIPBOARD, KeyCodes.WM_CHANGECBCHAIN };
        }

        protected override void WndProc(ref Message m)
        {
            WndProcCalled?.Invoke(this, m);
            if (codes?.Contains(m.Msg) == false || codes?.Contains(m.Msg) == null) { base.WndProc(ref m); }
        }
    }
}