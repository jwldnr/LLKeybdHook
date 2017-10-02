using System;
using System.Windows.Forms;

namespace jwldnr.LLKeybdHook.App
{
    public partial class MainForm : Form
    {
        private readonly KeyboardHook _keyboardHook = new KeyboardHook();

        public MainForm()
        {
            InitializeComponent();

            _keyboardHook.Install();

            _keyboardHook.KeyDown += OnKeyDown;
            _keyboardHook.KeyUp += OnKeyUp;
        }

        private void Log(string message)
        {
            EventLog.AppendText($"{message + Environment.NewLine}");
            EventLog.ScrollToCaret();
        }

        private void OnKeyDown(object sender, KeyEventArgsEx e)
        {
            Log($"{e.KeyCode} down");
        }

        private void OnKeyUp(object sender, KeyEventArgsEx e)
        {
            Log($"{e.KeyCode} up");
        }
    }
}