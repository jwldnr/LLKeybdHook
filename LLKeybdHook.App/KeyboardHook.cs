using jwldnr.LLKeybdHook.App.WinApi;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace jwldnr.LLKeybdHook.App
{
    internal class KeyboardHook : IDisposable
    {
        private HookProcedure _hookProcedure;

        private HookProcedureHandle _hookProcedureHandle;

        internal delegate void KeyboardHookCallback(object sender, KeyEventArgsEx e);

        internal delegate bool KeyboardHookHandler(CallbackData data);

        internal event KeyboardHookCallback KeyDown;

        internal event KeyboardHookCallback KeyUp;

        public void Dispose()
        {
            Uninstall();
        }

        internal void Install()
        {
            _hookProcedureHandle = SetHook(HookIds.WH_KEYBOARD_LL, Callback);
        }

        internal void Uninstall()
        {
            _hookProcedureHandle.Dispose();
        }

        private static IntPtr HookProcedure(int nCode, IntPtr wParam, IntPtr lParam, KeyboardHookHandler hookHandler)
        {
            if (nCode != 0)
                return NativeMethods.CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);

            var callbackData = new CallbackData(wParam, lParam);

            return hookHandler(callbackData)
                ? NativeMethods.CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam)
                : new IntPtr(-1);
        }

        private bool Callback(CallbackData data)
        {
            var e = KeyEventArgsEx.FromRawData(data);

            InvokeKeyDown(e);
            InvokeKeyUp(e);

            return !e.Handled;
        }

        private void InvokeKeyDown(KeyEventArgsEx e)
        {
            var handler = KeyDown;
            if (null == handler || e.Handled || false == e.IsKeyDown)
                return;

            handler(this, e);
        }

        private void InvokeKeyUp(KeyEventArgsEx e)
        {
            var handler = KeyUp;
            if (null == handler || e.Handled || false == e.IsKeyUp)
                return;

            handler(this, e);
        }

        private HookProcedureHandle SetHook(int hookId, KeyboardHookHandler hookHandler)
        {
            _hookProcedure = (code, param, lParam) => HookProcedure(code, param, lParam, hookHandler);

            var hookHandle = NativeMethods.SetWindowsHookEx(hookId, _hookProcedure, Process.GetCurrentProcess().MainModule.BaseAddress, 0);

            if (false == hookHandle.IsInvalid)
                return hookHandle;

            var errorCode = Marshal.GetLastWin32Error();
            throw new Win32Exception(errorCode);
        }
    }
}