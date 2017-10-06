using System;
using System.Windows.Forms;

namespace jwldnr.LLKeybdHook.App
{
    public partial class MainForm : Form
    {
        private bool CooldownIsReady => false == _cooldownTimer.Enabled || false == _hasCooldown;

        private readonly KeyboardHook _keyboardHook = new KeyboardHook();
        private readonly Timer _cooldownTimer = new Timer();

        private bool _hasCooldown;

        public MainForm()
        {
            InitializeComponent();

            InstallKeyboardHook();

            InitializeCooldownTimer();
        }

        ~MainForm()
        {
            UninstallKeyboardHook();
        }

        private void InstallKeyboardHook()
        {
            _keyboardHook.Install();

            _keyboardHook.KeyDown += OnKeyDown;
            _keyboardHook.KeyUp += OnKeyUp;

            Application.ApplicationExit += OnApplicationExit;
        }

        private void OnApplicationExit(object sender, EventArgs e)
        {
            UninstallKeyboardHook();
        }

        private void InitializeCooldownTimer()
        {
            _cooldownTimer.Interval = 1000;
            _cooldownTimer.Tick += OnTimerTick;
        }

        private void OnTimerTick(object sender, EventArgs e)
        {
            Log(nameof(OnTimerTick));

            _cooldownTimer.Enabled = false;
            _hasCooldown = false;
        }

        private void UninstallKeyboardHook()
        {
            _keyboardHook.Uninstall();

            _keyboardHook.KeyDown -= OnKeyDown;
            _keyboardHook.KeyUp -= OnKeyUp;
        }

        private void Log(string message)
        {
            EventLog.AppendText($"{message + Environment.NewLine}");
            EventLog.ScrollToCaret();
        }

        private void OnKeyDown(object sender, KeyEventArgsEx e)
        {
            if (e.KeyCode != Keys.W)
                return;

            if (CooldownIsReady)
            {
                e.SuppressKeyPress = true;
                e.Handled = true;

                Log("send Q");
                Log("send W");

                _cooldownTimer.Enabled = true;
                _hasCooldown = true;
            }

            Log("send W (regular)");
        }

        private void OnKeyUp(object sender, KeyEventArgsEx e)
        {
            Log($"{e.KeyCode} up");
        }
    }
}