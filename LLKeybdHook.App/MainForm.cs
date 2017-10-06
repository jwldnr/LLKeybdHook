using System;
using System.Windows.Forms;
using WindowsInput;
using WindowsInput.Native;

namespace jwldnr.LLKeybdHook.App
{
    public partial class MainForm : Form
    {
        private bool CooldownIsReady => false == _cooldownTimer.Enabled || false == _hasCooldown;

        private readonly Timer _cooldownTimer = new Timer();
        private readonly InputSimulator _inputSimulator = new InputSimulator();
        private readonly KeyboardHook _keyboardHook = new KeyboardHook();

        private bool _hasCooldown;

        public MainForm()
        {
            InitializeComponent();
            InitializeCooldownTimer();

            InstallHook();
        }

        private void InitializeCooldownTimer()
        {
            _cooldownTimer.Interval = 2000;
            _cooldownTimer.Tick += OnTimerTick;
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
            if (Keys.W != e.KeyCode || e.IsInjected)
                return;

            //if (e.IsInjected)
            //    e.Handled = true;

            if (!CooldownIsReady)
                return;

            e.SuppressKeyPress = true;
            e.Handled = true;

            _cooldownTimer.Enabled = true;
            _hasCooldown = true;

            _inputSimulator.Keyboard
                .KeyPress(VirtualKeyCode.VK_Q)
                .KeyPress(VirtualKeyCode.VK_W);

            //Log($"{e.KeyCode} down");
        }

        private void OnKeyUp(object sender, KeyEventArgsEx e)
        {
            //if (Keys.W != e.KeyCode)
            //    return;

            //if (e.IsInjected)
            //    e.Handled = true;

            //Log($"{e.KeyCode} up");
        }

        private void OnTimerTick(object sender, EventArgs e)
        {
            Log(nameof(OnTimerTick));

            _cooldownTimer.Enabled = false;
            _hasCooldown = false;
        }

        private void UninstallHook()
        {
            _keyboardHook.KeyDown -= OnKeyDown;
            _keyboardHook.KeyUp -= OnKeyUp;

            _keyboardHook.Dispose();
        }
    }
}