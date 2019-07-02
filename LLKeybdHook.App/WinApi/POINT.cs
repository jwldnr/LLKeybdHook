using System.Runtime.InteropServices;

namespace jwldnr.LLKeybdHook.App.WinApi
{
    /// <summary>
    /// Represents a point
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct POINT
    {
        public int x;
        public int y;

        public POINT(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }
}
