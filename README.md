# ClipEncrypt v1.0

ClipEncrypt is a small Windows application that encrypts and decrypts text in the clipboard using a simple key-based algorithm. This release contains the initial version of the application.
ClipEncypt runs in the system tray. It can be used when DLP is spying on your web browser clipboard operations. 

## Features
- Encrypts clipboard text with a simple key-based algorithm
- Decrypts clipboard text using the same key
- Supports two hotkeys for encryption and decryption (Ctrl+F1 and Ctrl+F2 respectively)
- Runs in the system tray and provides a simple context menu to exit the application

## Usage
1. Copy some text to the clipboard.
2. Press Ctrl+F1 to encrypt the text in the clipboard.
3. Paste the encrypted text wherever you want.
4. Press Ctrl+F2 to decrypt the text and have it typed into another program.

## Note
- This application uses a simple key-based encryption algorithm and should not be used for secure data encryption.
- The key used for encryption is hardcoded in the application and can be modified in the source code.
- The hotkey can be changed in the source code.
