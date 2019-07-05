using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using WindowsInput;
using WindowsInput.Native;

namespace jwldnr.LLKeybdHook.App
{
    public class Ability
    {
        VirtualKeyCode KeyCode { get; }

        int Cooldown { get; }

        int Delay { get; }

        public Ability(
            VirtualKeyCode keyCode,
            int cooldown,
            int delay)
        {
            KeyCode = keyCode;
            Cooldown = cooldown;
            Delay = delay;
        }
    }

    public partial class MainForm : Form
    {
        private GlobalHook _mouseHook;
        private GlobalHook _keyboardHook;

        private readonly IKeyboardSimulator _keyboard = new InputSimulator().Keyboard;
        private readonly Random _random = new Random();

        private bool _qOnCooldown;
        private bool _eOnCooldown;
        private bool _rOnCooldown;

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

            if (VirtualKeyCode.VK_Q == key || VirtualKeyCode.VK_E == key)
            {
                // dont send "w" for storm brand curse on hit or wave of conviction trap
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
            if (MouseButtons.Right != e.Button || _rOnCooldown)
                return;

            UseAbilityAt(VirtualKeyCode.VK_R, false);
            SetCooldownFor(VirtualKeyCode.VK_R, true);
        }

        private int GetCooldownFor(VirtualKeyCode key)
        {
            if (VirtualKeyCode.VK_Q == key)
                return _random.Next(2250, 2750);

            if (VirtualKeyCode.VK_E == key)
                return _random.Next(1750, 2250);

            if (VirtualKeyCode.VK_R == key)
                return _random.Next(4000, 4500);

            return 0;
        }

        private int GetDelayFor(VirtualKeyCode key)
        {
            if (VirtualKeyCode.VK_Q == key) // storm brand (curse on hit)
                return _random.Next(400, 450);

            if (VirtualKeyCode.VK_E == key) // wave of conviction trap
                return _random.Next(350, 400);

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
                //if (0 == delay)
                //{
                //    _keyboard
                //        .KeyPress(key);
                //}
                //else
                //{
                    _keyboard
                        .KeyDown(key)
                        .Sleep(delay)
                        .KeyUp(key);
                //}

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

            //if (false == _rOnCooldown)
            //    return VirtualKeyCode.VK_R;

            return VirtualKeyCode.VK_W;
        }
    }
}