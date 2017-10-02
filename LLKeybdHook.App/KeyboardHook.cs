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

        public delegate void KeyboardHookCallback(object sender, KeyEventArgsEx e);

        public event KeyboardHookCallback KeyDown;

        public event KeyboardHookCallback KeyUp;

        public void Dispose()
        {
            Uninstall();
        }

        public void Install()
        {
            _hookProcedureHandle = SetHook(HookIds.WH_KEYBOARD_LL, Callback);
        }

        public void Uninstall()
        {
            _hookProcedureHandle.Dispose();
        }

        private static IntPtr HookProcedure(int nCode, IntPtr wParam, IntPtr lParam, Callback callback)
        {
            if (nCode != 0)
                return NativeMethods.CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);

            var callbackData = new CallbackData(wParam, lParam);

            return callback(callbackData)
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
            if (handler == null || e.Handled || !e.IsKeyDown)
                return;

            handler(this, e);
        }

        private void InvokeKeyUp(KeyEventArgsEx e)
        {
            var handler = KeyUp;
            if (null == handler || e.Handled || !e.IsKeyUp)
                return;

            handler(this, e);
        }

        private HookProcedureHandle SetHook(int hookId, Callback callback)
        {
            _hookProcedure = (code, param, lParam) => HookProcedure(code, param, lParam, callback);

            var hookHandle = NativeMethods.SetWindowsHookEx(hookId, _hookProcedure, Process.GetCurrentProcess().MainModule.BaseAddress, 0);

            if (!hookHandle.IsInvalid)
                return hookHandle;

            var errorCode = Marshal.GetLastWin32Error();
            throw new Win32Exception(errorCode);
        }
    }
}