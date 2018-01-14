using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using WindowsInput;
using WindowsInput.Native;

namespace jwldnr.LLKeybdHook.App
{
    public partial class MainForm : Form
    {
        private readonly InputSimulator _inputSimulator = new InputSimulator();
        private readonly KeyboardHook _keyboardHook = new KeyboardHook();

        private bool _isOnCooldown;

        public MainForm()
        {
            InitializeComponent();

            InstallHook();
        }

        private void InstallHook()
        {
            _keyboardHook.Install();

            _keyboardHook.KeyDown += OnKeyDown;

            Application.ApplicationExit += OnApplicationExit;
        }

        private void Log(string message)
        {
            EventLog.BeginInvoke(new Action(() =>
            {
                EventLog.AppendText($"{message + Environment.NewLine}");
                EventLog.ScrollToCaret();
            }));
        }

        private void OnApplicationExit(object sender, EventArgs e)
        {
            UninstallHook();
        }

        private void OnKeyDown(object sender, KeyEventArgsEx e)
        {
            if (Keys.W != e.KeyCode || e.IsInjected)
                return;

            if (_isOnCooldown)
                return;

            e.SuppressKeyPress = true;
            e.Handled = true;

            _isOnCooldown = true;

            Task.Run(async () =>
            {
                _inputSimulator.Keyboard
                    .KeyPress(VirtualKeyCode.VK_Q)
                    .KeyPress(VirtualKeyCode.VK_W);

                await Task.Delay(2000)
                    .ConfigureAwait(false);

                Log("OnKeyDown callback");

                _isOnCooldown = false;
            });
        }

        private void UninstallHook()
        {
            _keyboardHook.KeyDown -= OnKeyDown;

            _keyboardHook.Dispose();
        }
    }
}