using jwldnr.LLKeybdHook.App.WinApi;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace jwldnr.LLKeybdHook.App
{
    internal class KeyEventArgsEx : KeyEventArgs
    {
        public bool IsExtendedKey { get; }
        public bool IsInjected { get; }

        public bool IsKeyDown { get; }

        public bool IsKeyUp { get; }

        public uint ScanCode { get; }

        public uint Timestamp { get; }

        public KeyEventArgsEx(Keys keyData) : base(keyData)
        {
        }

        internal KeyEventArgsEx(
            Keys keyData,
            uint scanCode,
            uint timestamp,
            bool isKeyDown,
            bool isKeyUp,
            bool isExtendedKey,
            bool isInjected) : this(keyData)
        {
            ScanCode = scanCode;
            Timestamp = timestamp;
            IsKeyDown = isKeyDown;
            IsKeyUp = isKeyUp;
            IsExtendedKey = isExtendedKey;
            IsInjected = isInjected;
        }

        internal static KeyEventArgsEx FromRawData(CallbackData data)
        {
            var wParam = data.WParam;
            var lParam = data.LParam;

            var keyboardHookStruct = (KBDLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(KBDLLHOOKSTRUCT));

            var keyData = (Keys)keyboardHookStruct.vkCode;
            var keyCode = (int)wParam;

            var isKeyDown = keyCode == Messages.WM_KEYDOWN || keyCode == Messages.WM_SYSKEYDOWN;
            var isKeyUp = keyCode == Messages.WM_KEYUP || keyCode == Messages.WM_SYSKEYUP;

            const uint maskExtendedKey = 0x1;

            var isExtendedKey = (keyboardHookStruct.flags & maskExtendedKey) > 0;
            var isInjected = (keyboardHookStruct.flags & (int)KEYBDHOOKF.LLKHF_INJECTED) != 0;

            return new KeyEventArgsEx(
                keyData,
                keyboardHookStruct.scanCode,
                keyboardHookStruct.time,
                isKeyDown,
                isKeyUp,
                isExtendedKey,
                isInjected);
        }
    }
}