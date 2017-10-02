using System;

namespace jwldnr.LLKeybdHook.App.WinApi
{
    public delegate IntPtr HookProcedure(int nCode, IntPtr wParam, IntPtr lParam);
}