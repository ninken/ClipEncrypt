# ClipEncrypt v1.0.1

ClipEncrypt is a simple program that allows you to encrypt and decrypt your clipboard text using a hotkey combination. It runs in the background and provides a system tray icon for easy access to its functionality.It can be used when DLP is spying on your web browser clipboard operations.

## Features
- Encrypt clipboard text: With a configurable hotkey combination, you can encrypt the text in your clipboard using a predefined encryption algorithm.
- Type encrypted text: Once the clipboard text is encrypted, you can use another hotkey combination to type the decrypted text directly into the active window.
- Decrypt clipboard text: If you have encrypted text in your clipboard, you can decrypt it and replace it with the original text using a hotkey combination.

## Usage

1. The program will appear in the system tray as an icon.
2. Right-click the system tray icon to access the context menu.
3. Use the context menu options to encrypt, type, or decrypt the clipboard text or use the hotkeys
4. You can also use the predefined hotkey combinations for each action.

## Configuration

The program can be configured by modifying the `app.config` file. The following settings are available:

- `EncyptSecret`: The secret key used for encryption. You can customize this key to enhance security.
- `EncyptClipboardHotkey`: The hotkey combination for encrypting the clipboard text.
- `TypeClipboardHotkey`: The hotkey combination for typing the decrypted text.
- `DecypteClipboardHotkey`: The hotkey combination for decrypting the clipboard text.
