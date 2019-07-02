using System;
using System.Runtime.InteropServices;

namespace jwldnr.LLKeybdHook.App.WinApi
{
    internal static class NativeMethods
    {
        /// <summary>
        /// Passes the hook information to the next hook procedure in the current hook chain
        /// </summary>
        /// <param name="idHook"></param>
        /// <param name="nCode"></param>
        /// <param name="wParam"></param>
        /// <param name="lParam"></param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        internal static extern IntPtr CallNextHookEx(IntPtr idHook, int nCode, IntPtr wParam, IntPtr lParam);

        /// <summary>
        /// Installs an application-defined hook procedure into a hook chain
        /// </summary>
        /// <param name="idHook"></param>
        /// <param name="lpfn"></param>
        /// <param name="hInstance"></param>
        /// <param name="threadId"></param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        internal static extern IntPtr SetWindowsHookEx(int idHook, GlobalHook.HookProcCallBack lpfn, IntPtr hInstance, int threadId);

        /// <summary>
        /// Removes a hook procedure installed in a hook chain by the SetWindowsHookEx function.
        /// </summary>
        /// <param name="idHook"></param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        internal static extern bool UnhookWindowsHookEx(IntPtr idHook);

        /// <summary>
        /// The GetKeyboardState function copies the status of the 256 virtual keys to the specified buffer.
        /// </summary>
        /// <param name="pbKeyState"></param>
        /// <returns></returns>
        [DllImport("user32")]
        internal static extern int GetKeyboardState(byte[] pbKeyState);

        /// <summary>
        /// This function retrieves the status of the specified virtual key. The status specifies whether the key is up, down, or toggled on or off — alternating each time the key is pressed.
        /// </summary>
        /// <param name="vKey"></param>
        /// <returns></returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        internal static extern short GetKeyState(int vKey);

        /// <summary>
        /// The ToAscii function translates the specified virtual-key code and keyboard state to the corresponding character or characters.
        /// </summary>
        /// <param name="uVirtKey"></param>
        /// <param name="uScanCode"></param>
        /// <param name="lpbKeyState"></param>
        /// <param name="lpwTransKey"></param>
        /// <param name="fuState"></param>
        /// <returns></returns>
        [DllImport("user32")]
        internal static extern int ToAscii(uint uVirtKey, uint uScanCode, byte[] lpbKeyState, byte[] lpwTransKey, uint fuState);
    }
}