using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using WindowsInput;
using WindowsInput.Native;

namespace jwldnr.LLKeybdHook.App
{
    public partial class MainForm : Form
    {
        private GlobalHook _mouseHook;
        private GlobalHook _keyboardHook;

        private readonly IKeyboardSimulator _keyboard = new InputSimulator().Keyboard;
        private readonly Random _random = new Random();

        private bool _qOnCooldown;
        private bool _eOnCooldown;
        private bool _rOnCooldown;
        private bool _tOnCooldown;


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

            // default key, no other action available
            if (VirtualKeyCode.VK_W == key)
                return;

            e.SuppressKeyPress = true;

            if (key == VirtualKeyCode.VK_Q || key == VirtualKeyCode.VK_E)
            {
                UseAbilityAt(key, false);
            }
            else
            {
                UseAbilityAt(key);
            }

            SetCooldownFor(key, true);

            e.Handled = true;
        }

        private void OnMouseDown(object sender, MouseEventArgs e)
        {
            if (MouseButtons.Right != e.Button || _tOnCooldown)
                return;

            UseAbilityAt(VirtualKeyCode.VK_T, false);
            SetCooldownFor(VirtualKeyCode.VK_T, true);
        }

        private int GetCooldownFor(VirtualKeyCode key)
        {
            if (VirtualKeyCode.VK_Q == key)
                return _random.Next(4250, 4750);

            if (VirtualKeyCode.VK_E == key)
                return _random.Next(1750, 2250);

            if (VirtualKeyCode.VK_R == key)
                return _random.Next(6250, 6750);

            if (VirtualKeyCode.VK_T == key)
                return _random.Next(2750, 3250);

            return 0;
        }

        private int GetDelayFor(VirtualKeyCode key)
        {
            if (VirtualKeyCode.VK_Q == key) // curse on hit storm brand
                return _random.Next(600, 650);

            if (VirtualKeyCode.VK_E == key) // wave of conviction trap
                return _random.Next(350, 400);

            if (VirtualKeyCode.VK_R == key) // vaal ancestral warchief
                return _random.Next(450, 500);

            return 0;
        }

        private void SetCooldownFor(VirtualKeyCode key, bool value)
        {
            if (VirtualKeyCode.VK_Q == key)
                _qOnCooldown = value;

            if (VirtualKeyCode.VK_E == key)
                _eOnCooldown = value;

            if (VirtualKeyCode.VK_R == key)
                _rOnCooldown = value;

            if (VirtualKeyCode.VK_T == key)
                _tOnCooldown = value;
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
                    _keyboard
                        .KeyPress(VirtualKeyCode.VK_W);
                }

                var delay = GetDelayFor(key);
                if (0 == delay)
                {
                    _keyboard
                       .KeyPress(key);
                }
                else
                {
                    _keyboard
                        .KeyDown(key)
                        .Sleep(delay)
                        .KeyUp(key);
                }

                await Task.Delay(cooldown)
                    .ConfigureAwait(false);

                SetCooldownFor(key, false);
            });
        }

        private VirtualKeyCode GetAvailableAbility()
        {
            if (false == _tOnCooldown)
                return VirtualKeyCode.VK_T;

            if (false == _rOnCooldown)
                return VirtualKeyCode.VK_R;

            if (false == _qOnCooldown)
                return VirtualKeyCode.VK_Q;

            if (false == _eOnCooldown)
                return VirtualKeyCode.VK_E;

            return VirtualKeyCode.VK_W;
        }
    }
}