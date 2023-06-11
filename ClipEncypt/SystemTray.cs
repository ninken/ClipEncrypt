using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Configuration;

namespace ClipEncrypt
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            var context = new TrayApplicationContext();
            Application.Run(context);
        }
    }

    public class TrayApplicationContext : ApplicationContext
    {
        private const int WM_HOTKEY = 0x0312;
        private NotifyIcon trayIcon;
        private string clipboardText;
        private static string key = ConfigurationManager.AppSettings["EncyptSecret"];

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern bool SetWindowText(IntPtr hWnd, string lpString);

        private void SetForegroundWindowTitle(string title)
        {
            IntPtr foregroundWindowHandle = GetForegroundWindow();
            SetWindowText(foregroundWindowHandle, title);
        }

        public TrayApplicationContext()
        {
            trayIcon = new NotifyIcon()
            {
                Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath),
                Visible = true,
                ContextMenuStrip = new ContextMenuStrip(),
            };

            // Add context menu items
            RegisterContextMenu("Encrypt Clipboard (" + ConfigurationManager.AppSettings["EncyptClipboardHotkey"] + ")", (sender, e) => EncryptClipboard());
            RegisterContextMenu("Type Clipboard (" + ConfigurationManager.AppSettings["TypeClipboardHotkey"] + ")", (sender, e) => TypeClipboard());
            RegisterContextMenu("Decrypt Clipboard (" + ConfigurationManager.AppSettings["DecypteClipboardHotkey"] + ")", (sender, e) => DecryptClipboard());

            // Register hotkeys
            RegisterHotkey(1, (Keys)Enum.Parse(typeof(Keys), ConfigurationManager.AppSettings["EncyptClipboardHotkey"]));
            RegisterHotkey(2, (Keys)Enum.Parse(typeof(Keys), ConfigurationManager.AppSettings["TypeClipboardHotkey"]));
            RegisterHotkey(3, (Keys)Enum.Parse(typeof(Keys), ConfigurationManager.AppSettings["DecypteClipboardHotkey"]));

            // Add a menu item to close the application
            var exitMenuItem = new ToolStripMenuItem("Exit", null, Exit);
            trayIcon.ContextMenuStrip.Items.Add(exitMenuItem);

            // Hook into the message loop to capture the hotkeys
            Application.AddMessageFilter(new MessageFilter(HotkeyPressed));
        }

        private void RegisterContextMenu(string menuText, EventHandler eventHandler)
        {
            var menuItem = new ToolStripMenuItem(menuText, null, eventHandler);
            trayIcon.ContextMenuStrip.Items.Add(menuItem);
        }

        private void RegisterHotkey(int hotkeyId, Keys hotkey)
        {
            const uint MOD_CTRL = 0x0002;
            RegisterHotKey(IntPtr.Zero, hotkeyId, MOD_CTRL, (uint)hotkey);
        }

        private void EncryptClipboard()
        {
            if (Clipboard.ContainsText())
            {
                clipboardText = Encrypt(Clipboard.GetText());
                Clipboard.SetText(clipboardText);
            }
        }

        private void TypeClipboard()
        {
            if (clipboardText != null)
            {
                string windowTitle = GetForegroundWindowTitle();
                clipboardText = Clipboard.GetText();
                string decryptedText = Decrypt(clipboardText);

                SetForegroundWindowTitle(windowTitle);
                SendKeyStrokes(decryptedText);
            }
        }

        private void DecryptClipboard()
        {
            if (clipboardText != null)
            {
                string windowTitle = GetForegroundWindowTitle();

                clipboardText = Clipboard.GetText();
                Clipboard.SetText(Decrypt(clipboardText));
            }
        }

        private void Exit(object sender, EventArgs e)
        {
            trayIcon.Visible = false;
            Application.Exit();
        }

        public static string Encrypt(string input)
        {
            StringBuilder sb = new StringBuilder();
            try
            {
                for (int i = 0; i < input.Length; i++)
                {
                    char c = input[i];
                    char k = key[i % key.Length];
                    char encrypted = (char)((int)c + (int)k);
                    sb.Append(encrypted);
                }
                return sb.ToString();
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static string Decrypt(string input)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < input.Length; i++)
            {
                char c = input[i];
                char k = key[i % key.Length];
                char decrypted = (char)((int)c - (int)k);
                sb.Append(decrypted);
            }
            return sb.ToString();
        }

        private string GetForegroundWindowTitle()
        {
            IntPtr foregroundWindowHandle = GetForegroundWindow();
            const int maxWindowTitleLength = 256;
            StringBuilder windowTitle = new StringBuilder(maxWindowTitleLength);
            GetWindowText(foregroundWindowHandle, windowTitle, maxWindowTitleLength);
            return windowTitle.ToString();
        }

        [Flags]
        private enum KEYEVENTF : uint
        {
            KEYDOWN = 0x0000,
            KEYUP = 0x0002,
            UNICODE = 0x0004,
        }

        [StructLayout(LayoutKind.Sequential)]
        struct INPUT
        {
            public uint type;
            public InputUnion u;
        }

        [StructLayout(LayoutKind.Explicit)]
        struct InputUnion
        {
            [FieldOffset(0)]
            public MOUSEINPUT mi;

            [FieldOffset(0)]
            public KEYBDINPUT ki;

            [FieldOffset(0)]
            public HARDWAREINPUT hi;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public uint mouseData;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct KEYBDINPUT
        {
            public ushort wVk;
            public ushort wScan;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct HARDWAREINPUT
        {
            public uint uMsg;
            public ushort wParamL;
            public ushort wParamH;
        }

        private void HotkeyPressed(object sender, EventArgs e)
        {
            var message = e as MessageEventArgs;
            if (message?.Message.Msg == WM_HOTKEY)
            {
                if (message.Message.WParam.ToInt32() == 1)
                {
                    // Encrypt the clipboard text
                    if (Clipboard.ContainsText())
                    {
                        clipboardText = Encrypt(Clipboard.GetText());
                        Clipboard.SetText(clipboardText);
                    }
                }
                else if (message.Message.WParam.ToInt32() == 2)
                {
                    if (clipboardText != null)
                    {
                        string windowTitle = GetForegroundWindowTitle();
                        clipboardText = Clipboard.GetText();
                        string decryptedText = Decrypt(clipboardText);

                        SetForegroundWindowTitle(windowTitle);
                        SendKeyStrokes(decryptedText);
                    }
                }
                else if (message.Message.WParam.ToInt32() == 3) // Send encrypted text back to clipboard unencrypted
                {
                    if (clipboardText != null)
                    {
                        string windowTitle = GetForegroundWindowTitle();

                        clipboardText = Clipboard.GetText();
                        Clipboard.SetText(Decrypt(clipboardText));
                    }
                }
            }
        }

        private void SendKeyStrokes(string text)
        {
            const uint VK_SHIFT = 0x10;
            const int VK_RETURN = 0x0D;

            foreach (char c in text)
            {
                if (char.IsUpper(c))
                {
                    // Hold Shift key
                    SendKeyDown(VK_SHIFT);
                }

                if (c == '\n')
                {
                    // Send Enter key
                    SendKeyDown(VK_RETURN);
                    SendKeyUp(VK_RETURN);
                }
                else
                {
                    // Send the character as a keystroke
                    SendKeyPress(c);
                }

                if (char.IsUpper(c))
                {
                    // Release Shift key
                    SendKeyUp(VK_SHIFT);
                }
                // Delay between each character
                Thread.Sleep(5);
            }
        }

        private void SendKeyDown(uint keyCode)
        {
            INPUT[] inputs = new INPUT[1];
            inputs[0] = new INPUT
            {
                type = 1, // Keyboard input
                u = new InputUnion
                {
                    ki = new KEYBDINPUT
                    {
                        wVk = (ushort)keyCode,
                        wScan = 0,
                        dwFlags = (uint)KEYEVENTF.KEYDOWN,
                        time = 0,
                        dwExtraInfo = IntPtr.Zero
                    }
                }
            };
            SendInput(1, inputs, Marshal.SizeOf(typeof(INPUT)));
        }

        private void SendKeyUp(uint keyCode)
        {
            INPUT[] inputs = new INPUT[1];
            inputs[0] = new INPUT
            {
                type = 1, // Keyboard input
                u = new InputUnion
                {
                    ki = new KEYBDINPUT
                    {
                        wVk = (ushort)keyCode,
                        wScan = 0,
                        dwFlags = (uint)KEYEVENTF.KEYUP,
                        time = 0,
                        dwExtraInfo = IntPtr.Zero
                    }
                }
            };
            SendInput(1, inputs, Marshal.SizeOf(typeof(INPUT)));
        }

        private void SendKeyPress(char character)
        {
            INPUT[] inputs = new INPUT[2];
            inputs[0] = new INPUT
            {
                type = 1, // Keyboard input
                u = new InputUnion
                {
                    ki = new KEYBDINPUT
                    {
                        wVk = 0,
                        wScan = character,
                        dwFlags = (uint)(KEYEVENTF.KEYDOWN | KEYEVENTF.UNICODE),
                        time = 0,
                        dwExtraInfo = IntPtr.Zero
                    }
                }
            };
            inputs[1] = new INPUT
            {
                type = 1, // Keyboard input
                u = new InputUnion
                {
                    ki = new KEYBDINPUT
                    {
                        wVk = 0,
                        wScan = character,
                        dwFlags = (uint)(KEYEVENTF.KEYUP | KEYEVENTF.UNICODE),
                        time = 0,
                        dwExtraInfo = IntPtr.Zero
                    }
                }
            };
            SendInput(2, inputs, Marshal.SizeOf(typeof(INPUT)));
        }
    }

    public class MessageFilter : IMessageFilter
    {
        public event EventHandler<MessageEventArgs> MessageReceived;

        public MessageFilter(EventHandler<MessageEventArgs> messageReceived)
        {
            MessageReceived += messageReceived;
        }

        public bool PreFilterMessage(ref Message m)
        {
            MessageReceived?.Invoke(this, new MessageEventArgs(m));
            return false;
        }
    }

    public class MessageEventArgs : EventArgs
    {
        public Message Message { get; }

        public MessageEventArgs(Message message)
        {
            Message = message;
        }
    }
}
