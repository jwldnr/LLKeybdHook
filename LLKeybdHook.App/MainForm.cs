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

        private readonly Random _random = new Random();

        private bool _qOnCooldown;
        private bool _eOnCooldown;

        private readonly VirtualKeyCode[] hotkeys = new VirtualKeyCode[] { VirtualKeyCode.VK_Q, VirtualKeyCode.VK_W, VirtualKeyCode.VK_E, VirtualKeyCode.VK_R, VirtualKeyCode.VK_T };

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
                _mouseHook.MouseDown += OnMouseDown;
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
                _mouseHook.MouseDown -= OnMouseDown;

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

            var key = GetAvailableAbility();
            if (VirtualKeyCode.VK_W == key)
                return;

            e.SuppressKeyPress = true;

            UseAbilityAt(key);
            SetCooldownFor(key, true);

            e.Handled = true;
        }

        private void OnMouseDown(object sender, MouseEventArgs e)
        {
            if (MouseButtons.Right != e.Button || _qOnCooldown)
                return;

            UseAbilityAt(VirtualKeyCode.VK_Q, false);
            SetCooldownFor(VirtualKeyCode.VK_Q, true);
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

        private Task UseAbilityAt(VirtualKeyCode key, bool sendDefault = true)
        {
            return Task.Run(async () =>
            {
                var cooldown = GetCooldownFor(key);
                if (0 == cooldown)
                    return;

                if (sendDefault)
                {
                    _inputSimulator
                        .Keyboard
                        .KeyPress(VirtualKeyCode.VK_W);
                }

                var delay = GetDelayFor(key);
                _inputSimulator
                    .Keyboard
                    .KeyDown(key)
                    .Sleep(delay)
                    .KeyUp(key);

                await Task.Delay(cooldown)
                    .ConfigureAwait(false);

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
    }
}