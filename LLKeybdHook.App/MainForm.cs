using System;
using System.Windows.Forms;
using WindowsInput;
using WindowsInput.Native;

namespace jwldnr.LLKeybdHook.App
{
    public partial class MainForm : Form
    {
        private readonly InputSimulator _inputSimulator = new InputSimulator();
        private readonly KeyboardHook _keyboardHook = new KeyboardHook();

        private int i;

        private int j;

        public MainForm()
        {
            InitializeComponent();

            InstallHook();
        }

        private void InstallHook()
        {
            _keyboardHook.Install();

            _keyboardHook.KeyDown += OnKeyDown;
            _keyboardHook.KeyUp += OnKeyUp;

            Application.ApplicationExit += OnApplicationExit;
        }

        private void Log(string message)
        {
            EventLog.AppendText($"{message + Environment.NewLine}");
            EventLog.ScrollToCaret();
        }

        private void OnApplicationExit(object sender, EventArgs e)
        {
            UninstallHook();
        }

        private void OnKeyDown(object sender, KeyEventArgsEx e)
        {
            if (e.IsInjected || Keys.W != e.KeyCode)
                return;

            if (i % 6 == 0)
            {
                e.SuppressKeyPress = true;

                _inputSimulator.Keyboard.KeyPress(VirtualKeyCode.VK_Q);
                _inputSimulator.Keyboard.KeyPress(VirtualKeyCode.VK_W);

                i = 0;
            }
            else if (j % 4 == 0)
            {
                e.SuppressKeyPress = true;

                _inputSimulator.Keyboard.KeyPress(VirtualKeyCode.VK_E);
                _inputSimulator.Keyboard.KeyPress(VirtualKeyCode.VK_W);

                j = 0;
            }

            i = i + 1;
            j = j + 1;
        }

        private void OnKeyUp(object sender, KeyEventArgsEx e)
        {
            if (e.IsInjected || Keys.W != e.KeyCode)
                return;

            Log($"{e.KeyCode} up");
        }

        private void UninstallHook()
        {
            _keyboardHook.KeyDown -= OnKeyDown;
            _keyboardHook.KeyUp -= OnKeyUp;

            _keyboardHook.Dispose();
        }
    }
}