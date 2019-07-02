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

        private GlobalHook _mouseHook;
        private GlobalHook _keyboardHook;

        private bool _isOnCooldown;

        public MainForm()
        {
            InitializeComponent();

            InstallHooks();
        }

        private void InstallHooks()
        {
            if (null == _mouseHook)
            {
                _mouseHook = new GlobalHook(GlobalHook.HookTypes.Mouse);
            }

            if (null == _keyboardHook)
            {
                _keyboardHook = new GlobalHook(GlobalHook.HookTypes.Keyboard);
                _keyboardHook.KeyDown += OnKeyDown;
            }


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
            if (null != _mouseHook)
            {
                _mouseHook.Dispose();
                _mouseHook = null;
            }

            if (null != _keyboardHook)
            {
                _keyboardHook.KeyDown -= OnKeyDown;

                _keyboardHook.Dispose();
                _keyboardHook = null;
            }
        }

        private void OnKeyDown(object sender, KeyEventArgsEx e)
        {
            if (Keys.W != e.KeyCode || e.Injected)
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
    }
}