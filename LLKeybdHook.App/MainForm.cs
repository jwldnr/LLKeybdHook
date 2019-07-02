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

        private readonly Random _random = new Random();

        private bool _qOnCooldown;
        private bool _eOnCooldown;

        private readonly VirtualKeyCode[] hotkeys = new VirtualKeyCode[] { VirtualKeyCode.VK_Q, VirtualKeyCode.VK_W, VirtualKeyCode.VK_E, VirtualKeyCode.VK_R, VirtualKeyCode.VK_T };

        public MainForm()
        {
            InitializeComponent();

            InstallHook();
        }

        private void InstallHook()
        {
            _keyboardHook.Install();

            _keyboardHook.KeyDown += OnKeyDown;
            //_keyboardHook.KeyUp += OnKeyUp;

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

            var key = GetAvailableAbility();
            if (VirtualKeyCode.VK_W == key)
                return;

            e.SuppressKeyPress = true;

            UseAbilityAt(key);
            SetCooldownFor(key, true);

            e.Handled = true;
        }

        private void OnKeyUp(object sender, KeyEventArgsEx e)
        {
            if (Keys.Q != e.KeyCode
                && Keys.W != e.KeyCode
                && Keys.E != e.KeyCode
                && Keys.R != e.KeyCode
                && Keys.T != e.KeyCode)
            {
                return;
            }

            //_inputSimulator
            //    .Keyboard
            //    .Sleep(0);
        }

        private int GetCooldownFor(VirtualKeyCode key)
        {
            if (VirtualKeyCode.VK_Q == key)
                return _random.Next(2750, 2750);

            if (VirtualKeyCode.VK_E == key)
                return _random.Next(1250, 1750);

            return 0;
        }

        private int GetDelayFor(VirtualKeyCode key)
        {
            if (VirtualKeyCode.VK_E == key)
                return _random.Next(675, 725);

            return 0;
        }

        private void SetCooldownFor(VirtualKeyCode key, bool value)
        {
            if (VirtualKeyCode.VK_Q == key)
                _qOnCooldown = value;

            if (VirtualKeyCode.VK_E == key)
                _eOnCooldown = value;
        }

        private Task UseAbilityAt(VirtualKeyCode key)
        {
            return Task.Run(async () =>
            {
                var cooldown = GetCooldownFor(key);
                if (0 == cooldown)
                    return;

                var delay = GetDelayFor(key);
                _inputSimulator
                    .Keyboard
                    .KeyPress(VirtualKeyCode.VK_W)
                    .KeyDown(key)
                    .Sleep(delay)
                    .KeyUp(key);

                await Task.Delay(cooldown)
                    .ConfigureAwait(false);

                //Log("OnKeyDown callback");

                SetCooldownFor(key, false);
            });
        }

        private VirtualKeyCode GetAvailableAbility()
        {
            if (false == _qOnCooldown)
                return VirtualKeyCode.VK_Q;

            if (false == _eOnCooldown)
                return VirtualKeyCode.VK_E;

            return VirtualKeyCode.VK_W;
        }

        private void UninstallHook()
        {
            _keyboardHook.KeyDown -= OnKeyDown;
            _keyboardHook.KeyUp -= OnKeyUp;

            _keyboardHook.Dispose();
        }
    }
}