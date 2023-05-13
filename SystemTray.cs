using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
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
            Application.AddMessageFilter(new MessageFilter((sender, e) => HotkeyPressed(sender, e)));
        }

        private void Exit(object sender, EventArgs e)
        {
            // Close the tray icon and exit the application
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
                        if (clipboardText.Length > 1)
                        {
                            clipboardText = Clipboard.GetText();
                            string decryptedText = Decrypt(clipboardText);
                            if (decryptedText != null)
                            {
                                Clipboard.SetText(decryptedText);
                                SendKeys.Send("^v");
                                clipboardText = decryptedText;
                            }
                        }
                    }
                }
            }
        
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

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);
    }
}