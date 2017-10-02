using System;
using System.Runtime.InteropServices;

namespace jwldnr.LLKeybdHook.App.WinApi
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct KBDLLHOOKSTRUCT
    {
        public uint vkCode;

        public uint scanCode;

        public uint flags;

        public uint time;

        public UIntPtr dwExtraInfo;
    }
}