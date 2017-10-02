using System;

namespace jwldnr.LLKeybdHook.App.WinApi
{
    internal struct CallbackData
    {
        public IntPtr LParam { get; }
        public IntPtr WParam { get; }

        public CallbackData(IntPtr wParam, IntPtr lParam)
        {
            WParam = wParam;
            LParam = lParam;
        }
    }
}