using System.Runtime.InteropServices;

namespace jwldnr.LLKeybdHook.App.WinApi
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct HARDWAREINPUT
    {
        public uint uMsg;
        public ushort wParamL;
        public ushort wParamH;
    }
}