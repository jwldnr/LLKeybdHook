using System.Windows.Forms;

namespace jwldnr.LLKeybdHook.App
{
    internal class KeyEventArgsEx : KeyEventArgs
    {
        internal bool Injected { get; }

        internal KeyEventArgsEx(Keys keyData, bool injected = false) : base(keyData)
        {
            Injected = injected;
        }
    }
}