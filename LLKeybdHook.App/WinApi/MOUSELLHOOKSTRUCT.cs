using System.Runtime.InteropServices;

namespace jwldnr.LLKeybdHook.App.WinApi
{
    /// <summary>
    /// Contains information about a mouse event passed to a WH_MOUSE hook procedure
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal class MOUSELLHOOKSTRUCT
    {
        public POINT pt;
        public int mouseData;
        public int flags;
        public int time;
        public int extraInfo;
    }
}
