using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

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
        private static string key = "L33tS3cret!";

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
            // Initialize the tray icon
            trayIcon = new NotifyIcon()
            {
                Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath),
                Visible = true,
                ContextMenuStrip = new ContextMenuStrip(),
            };

            // Add a menu item to close the application
            var exitMenuItem = new ToolStripMenuItem("Exit", null, Exit);
            trayIcon.ContextMenuStrip.Items.Add(exitMenuItem);

            // Register the hotkeys using the Windows API
            const uint MOD_CTRL = 0x0002;
            RegisterHotKey(IntPtr.Zero, 1, MOD_CTRL, (uint)Keys.F1);
            RegisterHotKey(IntPtr.Zero, 2, MOD_CTRL, (uint)Keys.F2);

            // Hook into the message loop to capture the hotkeys
            Application.AddMessageFilter(new MessageFilter(HotkeyPressed));
        }

        private void Exit(object sender, EventArgs e)
        {
            trayIcon.Visible = false;
            Application.Exit();
        }

        public static string Encrypt(string input)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < input.Length; i++)
            {
                char c = input[i];
                char k = key[i % key.Length];
                char encrypted = (char)((int)c + (int)k);
                sb.Append(encrypted);
            }
            return sb.ToString();
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

        public static void SendKeyStroke(char key)
        {
            INPUT[] inputs = new INPUT[2];

            inputs[0] = new INPUT
            {
                type = 1, // Keyboard input
                u = new InputUnion
                {
                    ki = new KEYBDINPUT
                    {
                        wVk = (ushort)key,
                        wScan = (ushort)key,
                        dwFlags = (uint)KEYEVENTF.KEYDOWN
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
                        wVk = (ushort)key,
                        wScan = (ushort)key,
                        dwFlags = (uint)KEYEVENTF.KEYUP
                    }
                }
            };
            SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(INPUT)));
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
                        //string decryptedText = "Hello World!";

                        // Set the foreground window title
                        SetForegroundWindowTitle(windowTitle);

                        // Simulate keystrokes for the decrypted text
                        SendKeyStrokes(decryptedText);
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


        static bool NeedsEscaping(string character)
        {
            return character == "+" || character == "^" || character == "%" || character == "~" || character == "(" || character == ")" || character == "{" || character == "}" || character == "[" || character == "]" || character == "<" || character == ">";
        }

        // Rest of the code...

        class MessageFilter : IMessageFilter
        {
            private Action<object, EventArgs> _callback;

            public MessageFilter(Action<object, EventArgs> callback)
            {
                _callback = callback;
            }

            public bool PreFilterMessage(ref Message m)
            {
                if (m.Msg == WM_HOTKEY)
                {
                    _callback(null, new MessageEventArgs(m));
                    return true;
                }
                return false;
            }
        }

        class MessageEventArgs : EventArgs
        {
            public MessageEventArgs(Message message)
            {
                Message = message;
            }
            public Message Message { get; }
        }
    }
}
