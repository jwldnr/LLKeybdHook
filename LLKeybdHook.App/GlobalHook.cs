using jwldnr.LLKeybdHook.App.WinApi;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Windows.Forms;

namespace jwldnr.LLKeybdHook.App
{
    internal class GlobalHook
        : IDisposable
    {
        /// <summary>
        /// Types of available hooks
        /// </summary>
        public enum HookTypes
        {
            /// <summary>
            /// Installs a mouse hook
            /// </summary>
            Mouse,

            /// <summary>
            /// Installs a keyboard hook
            /// </summary>
            Keyboard
        }

        private HookProcCallBack _HookProc;
        private IntPtr _handle;
        private HookTypes _hookType;

        /// <summary>
        /// Occours when the hook captures a mouse click
        /// </summary>
        public event MouseEventHandler MouseClick;

        /// <summary>
        /// Occours when the hook captures a mouse double click
        /// </summary>
        public event MouseEventHandler MouseDoubleClick;

        /// <summary>
        /// Occours when the hook captures the mouse wheel
        /// </summary>
        public event MouseEventHandler MouseWheel;

        /// <summary>
        /// Occours when the hook captures the press of a mouse button
        /// </summary>
        public event MouseEventHandler MouseDown;

        /// <summary>
        /// Occours when the hook captures the release of a mouse button
        /// </summary>
        public event MouseEventHandler MouseUp;

        /// <summary>
        /// Occours when the hook captures the mouse moving over the screen
        /// </summary>
        public event MouseEventHandler MouseMove;

        /// <summary>
        /// Handler including additional event args (e.g. injected)
        /// </summary>
        public delegate void KeyEventHandlerEx(object sender, KeyEventArgsEx e);

        /// <summary>
        /// Occours when a key is pressed
        /// </summary>
        public event KeyEventHandlerEx KeyDown;

        /// <summary>
        /// Occours when a key is released
        /// </summary>
        public event KeyEventHandlerEx KeyUp;

        /// <summary>
        /// Occours when a key is pressed
        /// </summary>
        public event KeyPressEventHandler KeyPress;

        /// <summary>
        /// Delegate used to recieve HookProc
        /// </summary>
        /// <param name="nCode"></param>
        /// <param name="wParam"></param>
        /// <param name="lParam"></param>
        /// <returns></returns>
        internal delegate IntPtr HookProcCallBack(int nCode, IntPtr wParam, IntPtr lParam);

        /// <summary>
        /// Creates a new Hook of the specified type
        /// </summary>
        /// <param name="hookType"></param>
        public GlobalHook(HookTypes hookType)
        {
            _hookType = hookType;
            InstallHook();
        }

        ~GlobalHook()
        {
            Dispose(false);
        }

        ///// <summary>
        ///// Gets the type of this hook
        ///// </summary>
        //public HookTypes HookType { get { return _hookType; } }

        ///// <summary>
        ///// Gets the handle of the hook
        ///// </summary>
        //public IntPtr Handle { get { return _handle; } }

        /// <summary>
        /// Raises the <see cref="MouseClick"/> event
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnMouseClick(MouseEventArgs e)
        {
            MouseClick?.Invoke(this, e);
        }

        /// <summary>
        /// Raises the <see cref="MouseDoubleClick"/> event
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnMouseDoubleClick(MouseEventArgs e)
        {
            MouseDoubleClick?.Invoke(this, e);
        }

        /// <summary>
        /// Raises the <see cref="MouseWheel"/> event
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnMouseWheel(MouseEventArgs e)
        {
            MouseWheel?.Invoke(this, e);
        }

        /// <summary>
        /// Raises the <see cref="MouseDown"/> event
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnMouseDown(MouseEventArgs e)
        {
            MouseDown?.Invoke(this, e);
        }

        /// <summary>
        /// Raises the <see cref="MouseUp"/> event
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnMouseUp(MouseEventArgs e)
        {
            MouseUp?.Invoke(this, e);
        }

        /// <summary>
        /// Raises the <see cref="MouseMove"/> event
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnMouseMove(MouseEventArgs e)
        {
            MouseMove?.Invoke(this, e);
        }

        /// <summary>
        /// Raises the <see cref="KeyDown"/> event
        /// </summary>
        /// <param name="e">Event Data</param>
        protected virtual void OnKeyDown(KeyEventArgsEx e)
        {
            KeyDown?.Invoke(this, e);
        }

        /// <summary>
        /// Raises the <see cref="KeyUp"/> event
        /// </summary>
        /// <param name="e">Event Data</param>
        protected virtual void OnKeyUp(KeyEventArgsEx e)
        {
            KeyUp?.Invoke(this, e);
        }

        /// <summary>
        /// Raises the <see cref="KeyPress"/> event
        /// </summary>
        /// <param name="e">Event Data</param>
        protected virtual void OnKeyPress(KeyPressEventArgs e)
        {
            KeyPress?.Invoke(this, e);
        }

        /// <summary>
        /// Recieves the actual unsafe mouse hook procedure
        /// </summary>
        /// <param name="code"></param>
        /// <param name="wParam"></param>
        /// <param name="lParam"></param>
        /// <returns></returns>
        private IntPtr HookProc(int code, IntPtr wParam, IntPtr lParam)
        {
            if (code < 0)
            {
                return NativeMethods.CallNextHookEx(_handle, code, wParam, lParam);
            }

            switch (_hookType)
            {
                case HookTypes.Mouse:
                    return MouseProc(code, wParam, lParam);
                case HookTypes.Keyboard:
                    return KeyboardProc(code, wParam, lParam);
                default:
                    throw new ArgumentException("HookType not supported");
            }
        }

        /// <summary>
        /// Recieves the actual unsafe keyboard hook procedure
        /// </summary>
        /// <param name="code"></param>
        /// <param name="wParam"></param>
        /// <param name="lParam"></param>
        /// <returns></returns>
        private IntPtr KeyboardProc(int code, IntPtr wParam, IntPtr lParam)
        {
            KBDLLHOOKSTRUCT hookStruct = (KBDLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(KBDLLHOOKSTRUCT));

            int message = wParam.ToInt32();
            bool injected = (hookStruct.flags & (int)KEYBDHOOKF.LLKHF_INJECTED) != 0;
            bool handled = false;

            if (Messages.WM_KEYDOWN == message || Messages.WM_SYSKEYDOWN == message)
            {
                KeyEventArgsEx e = new KeyEventArgsEx((Keys)hookStruct.vkCode, injected);
                OnKeyDown(e);
                handled = e.Handled;
            }
            else if (Messages.WM_KEYUP == message || Messages.WM_SYSKEYUP == message)
            {
                KeyEventArgsEx e = new KeyEventArgsEx((Keys)hookStruct.vkCode, injected);
                OnKeyUp(e);
                handled = e.Handled;
            }

            if (Messages.WM_KEYDOWN == message && null != KeyPress)
            {
                byte[] keyState = new byte[256];
                byte[] buffer = new byte[2];
                NativeMethods.GetKeyboardState(keyState);
                int conversion = NativeMethods.ToAscii(hookStruct.vkCode, hookStruct.scanCode, keyState, buffer, hookStruct.flags);

                if (conversion == 1 || conversion == 2)
                {
                    bool shift = (NativeMethods.GetKeyState((byte)VirtualKeyCode.VK_SHIFT) & 0x80) == 0x80;
                    bool capital = NativeMethods.GetKeyState((byte)VirtualKeyCode.VK_CAPITAL) != 0;
                    char c = (char)buffer[0];

                    if ((shift ^ capital) && char.IsLetter(c))
                    {
                        c = char.ToUpper(c);
                    }

                    KeyPressEventArgs e = new KeyPressEventArgs(c);
                    OnKeyPress(e);
                    handled |= e.Handled;
                }
            }


            return handled ? (IntPtr)1 : NativeMethods.CallNextHookEx(_handle, code, wParam, lParam);
        }

        /// <summary>
        /// Processes Mouse Procedures
        /// </summary>
        /// <param name="code"></param>
        /// <param name="wParam"></param>
        /// <param name="lParam"></param>
        /// <returns></returns>
        private IntPtr MouseProc(int code, IntPtr wParam, IntPtr lParam)
        {
            MOUSELLHOOKSTRUCT hookStruct = (MOUSELLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MOUSELLHOOKSTRUCT));

            int message = wParam.ToInt32();
            int x = hookStruct.pt.x;
            int y = hookStruct.pt.y;
            int delta = (short)((hookStruct.mouseData >> 16) & 0xffff);

            if (Messages.WM_MOUSEWHEEL == message)
            {
                OnMouseWheel(new MouseEventArgs(MouseButtons.None, 0, x, y, delta));
            }
            else if (Messages.WM_MOUSEMOVE == message)
            {
                OnMouseMove(new MouseEventArgs(MouseButtons.None, 0, x, y, delta));
            }
            else if (Messages.WM_LBUTTONDBLCLK == message)
            {
                OnMouseDoubleClick(new MouseEventArgs(MouseButtons.Left, 0, x, y, delta));
            }
            else if (Messages.WM_LBUTTONDOWN == message)
            {
                OnMouseDown(new MouseEventArgs(MouseButtons.Left, 0, x, y, delta));
            }
            else if (Messages.WM_LBUTTONUP == message)
            {
                OnMouseUp(new MouseEventArgs(MouseButtons.Left, 0, x, y, delta));
                OnMouseClick(new MouseEventArgs(MouseButtons.Left, 0, x, y, delta));
            }
            else if (Messages.WM_MBUTTONDBLCLK == message)
            {
                OnMouseDoubleClick(new MouseEventArgs(MouseButtons.Middle, 0, x, y, delta));
            }
            else if (Messages.WM_MBUTTONDOWN == message)
            {
                OnMouseDown(new MouseEventArgs(MouseButtons.Middle, 0, x, y, delta));
            }
            else if (Messages.WM_MBUTTONUP == message)
            {
                OnMouseUp(new MouseEventArgs(MouseButtons.Middle, 0, x, y, delta));
            }
            else if (Messages.WM_RBUTTONDBLCLK == message)
            {
                OnMouseDoubleClick(new MouseEventArgs(MouseButtons.Right, 0, x, y, delta));
            }
            else if (Messages.WM_RBUTTONDOWN == message)
            {
                OnMouseDown(new MouseEventArgs(MouseButtons.Right, 0, x, y, delta));
            }
            else if (Messages.WM_RBUTTONUP == message)
            {
                OnMouseUp(new MouseEventArgs(MouseButtons.Right, 0, x, y, delta));
            }
            else if (Messages.WM_XBUTTONDBLCLK == message)
            {
                OnMouseDoubleClick(new MouseEventArgs(MouseButtons.XButton1, 0, x, y, delta));
            }
            else if (Messages.WM_XBUTTONDOWN == message)
            {
                OnMouseDown(new MouseEventArgs(MouseButtons.XButton1, 0, x, y, delta));
            }
            else if (Messages.WM_XBUTTONUP == message)
            {
                OnMouseUp(new MouseEventArgs(MouseButtons.XButton1, 0, x, y, delta));
            }

            return NativeMethods.CallNextHookEx(_handle, code, wParam, lParam);
        }

        /// <summary>
        /// Installs the actual unsafe hook
        /// </summary>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        private void InstallHook()
        {
            if (_handle != IntPtr.Zero)
                throw new InvalidOperationException("Hook is already installed");

            int htype = 0;

            switch (_hookType)
            {
                case HookTypes.Mouse:
                    htype = HookIds.WH_MOUSE_LL;
                    break;
                case HookTypes.Keyboard:
                    htype = HookIds.WH_KEYBOARD_LL;
                    break;
                default:
                    throw new ArgumentException("HookType is not supported");
            }

            _HookProc = HookProc;
            _handle = NativeMethods.SetWindowsHookEx(htype, _HookProc, Marshal.GetHINSTANCE(Assembly.GetExecutingAssembly().GetModules()[0]), 0);
            //_handle = NativeMethods.SetWindowsHookEx(htype, _HookProc, Process.GetCurrentProcess().MainModule.BaseAddress, 0);

            int lastWin32Error = Marshal.GetLastWin32Error();

            if (_handle == IntPtr.Zero)
                throw new Win32Exception(lastWin32Error);
        }

        /// <summary>
        /// Unhooks the hook
        /// </summary>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        private void Unhook()
        {
            if (IntPtr.Zero != _handle)
            {
                try
                {
                    bool ret = NativeMethods.UnhookWindowsHookEx(_handle);

                    if (false == ret)
                    {
                        int lastWin32Error = Marshal.GetLastWin32Error();
                        Win32Exception ex = new Win32Exception(lastWin32Error);
                        if (ex.NativeErrorCode != 0)
                            throw ex;
                    }

                    _handle = IntPtr.Zero;
                }
                catch (Exception)
                {
                }

            }
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_handle != IntPtr.Zero)
            {
                Unhook();
            }
        }
    }
}
