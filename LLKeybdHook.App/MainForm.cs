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

        private bool _4OnCooldown;
        private bool _5OnCooldown;

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

            var ability = GetAvailableAbility();
            if (VirtualKeyCode.VK_W != ability)
            {
                UseAbilityAt(ability, false);
            }

            var potion = GetAvailablePotion();
            if (VirtualKeyCode.VK_0 != potion)
            {
                UseAbilityAt(potion, false);
            }
        }

        private void OnMouseDown(object sender, MouseEventArgs e)
        {
            if (MouseButtons.Right != e.Button)
                return;

            // armour potion
            if (false == _4OnCooldown)
            {
                UseAbilityAt(VirtualKeyCode.VK_4, false);
            }

            // speed potion
            if (false == _5OnCooldown)
            {
                UseAbilityAt(VirtualKeyCode.VK_5, false);
            }
        }

        private int GetCooldownFor(VirtualKeyCode key)
        {
            // vortex
            if (VirtualKeyCode.VK_Q == key)
            {
                return _random.Next(1750, 2000);
            }

            // armour potion
            if (VirtualKeyCode.VK_4 == key)
            {
                return 5200;
            }

            // speed potion
            if (VirtualKeyCode.VK_5 == key)
            {
                return 4000;
            }

            return 0;
        }

        private int GetDelayFor(VirtualKeyCode key)
        {
            //if (VirtualKeyCode.VK_Q == key)
            //{
            //    return _random.Next(250, 300);
            //}

            return 0;
        }

        private void SetCooldownFor(VirtualKeyCode key, bool value)
        {
            if (VirtualKeyCode.VK_Q == key)
            {
                _qOnCooldown = value;
            }

            // evasion potion
            if (VirtualKeyCode.VK_4 == key)
                _4OnCooldown = value;

            // speed potion
            if (VirtualKeyCode.VK_5 == key)
                _5OnCooldown = value;
        }

        private Task UseAbilityAt(VirtualKeyCode key, bool sendDefault = true)
        {
            return Task.Run(async () =>
            {
                var cooldown = GetCooldownFor(key);
                if (0 == cooldown)
                    return;

                SetCooldownFor(key, true);

                var delay = GetDelayFor(key);
                if (0 == delay)
                {
                    if (sendDefault)
                    {
                        _keyboard
                            .KeyPress(key)
                            .KeyPress(VirtualKeyCode.VK_W);
                    }
                    else
                    {
                        _keyboard.KeyPress(key);
                    }
                }
                else
                {
                    if (sendDefault)
                    {
                        _keyboard
                            .KeyPress(VirtualKeyCode.VK_W)
                            .KeyDown(key)
                            .Sleep(delay)
                            .KeyUp(key);
                    }
                    else
                    {
                        _keyboard
                            .KeyDown(key)
                            .Sleep(delay)
                            .KeyUp(key);
                    }
                }

                await Task.Delay(cooldown)
                    .ConfigureAwait(false);

                SetCooldownFor(key, false);
            });
        }

        private VirtualKeyCode GetAvailableAbility()
        {
            if (false == _qOnCooldown)
                return VirtualKeyCode.VK_Q;

            //if (false == _eOnCooldown)
            //    return VirtualKeyCode.VK_E;

            //if (false == _yOnCooldown)
            //    return VirtualKeyCode.VK_Y;

            // do not include phase walk in default rotation
            //if (false == _rOnCooldown)
            //    return VirtualKeyCode.VK_R;

            return VirtualKeyCode.VK_W;
        }

        private VirtualKeyCode GetAvailablePotion()
        {
            if (false == _4OnCooldown)
                return VirtualKeyCode.VK_4;

            return VirtualKeyCode.VK_0;
        }
    }
}