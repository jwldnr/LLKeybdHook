using Microsoft.Win32.SafeHandles;

namespace jwldnr.LLKeybdHook.App.WinApi
{
    internal class HookProcedureHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        public HookProcedureHandle()
            : base(true)
        {
        }

        protected override void Dispose(bool disposing)
        {
            ReleaseHandle();
        }

        protected override bool ReleaseHandle()
        {
            return NativeMethods.UnhookWindowsHookEx(handle) != 0;
        }
    }
}