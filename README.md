# ClipEncrypt v1.0

ClipEncrypt is a small Windows application that encrypts and decrypts text in the clipboard using a simple key-based algorithm. This release contains the initial version of the application.

## Features
- Encrypts clipboard text with a simple key-based algorithm
- Decrypts clipboard text using the same key
- Supports two hotkeys for encryption and decryption (Ctrl+F1 and Ctrl+F2 respectively)
- Runs in the system tray and provides a simple context menu to exit the application

## Usage
1. Copy some text to the clipboard.
2. Press Ctrl+F1 to encrypt the text in the clipboard.
3. Paste the encrypted text wherever you want.
4. To decrypt the text, copy the encrypted text to the clipboard.
5. Press Ctrl+F2 to decrypt the text in the clipboard.
6. Paste the decrypted text wherever you want.

## Note
- This application uses a simple key-based encryption algorithm and should not be used for secure data encryption.
- The key used for encryption is hardcoded in the application and can be modified in the source code.
