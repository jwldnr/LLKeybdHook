using System;
using System.Runtime.InteropServices;

namespace jwldnr.LLKeybdHook.App.WinApi
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct KEYBDINPUT
    {
        public ushort wVk;
        public ushort wScan;
        public uint dwFlags;
        public uint time;
        public UIntPtr dwExtraInfo;
    }
}