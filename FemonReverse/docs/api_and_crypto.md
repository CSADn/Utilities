# FemonReverse - API & Cryptography Specs

## Firebase Remote Config API
- **Endpoint**: `https://firebaseremoteconfig.googleapis.com/v1/projects/femon-play/namespaces/firebase:fetch?key={FirebaseApiKey}`
- **Method**: POST
- **Required Headers**:
    - `X-Goog-Api-Key`: The Firebase API Key.
    - `X-Android-Package`: `com.example.myapplication`
    - `X-Firebase-GMPID`: Firebase App ID.
    - `User-Agent`: `Firebase/5/21.0.0/28/Android`
    - `X-Firebase-AppCheck`: `null`
- **Request Payload**: JSON containing `appId`, `appInstanceId` (random UUID), `appVersion`, `countryCode` (PY), `languageCode` (es), `platformVersion`, `sdkVersion`, and `packageName`.

## Cryptography Implementation

### AES Configuration
- **Algorithm**: AES
- **Mode**: ECB (Electronic Codebook)
- **Padding**: PKCS7 (Compatible with Android's PKCS5)
- **Encoding**: Input is Base64; Output is UTF-8.

### Double Decryption (`DecryptDouble`)
The application uses a nested decryption approach:
`Plaintext = AES_Decrypt(AES_Decrypt(Ciphertext, Key), Key)`

### Smart Decryption (`SmartDecrypt`)
A heuristic approach that runs up to 3 layers of:
1. Base64 Decode.
2. Check if result is a URL (`http://` or `https://`).
3. If length is a multiple of 16, attempt AES decryption.
4. Repeat until a URL is found or max layers are reached.

### Key Derivation (`BuildKeyBytes`)
The tool supports three formats for the `claveapp`:
1. **Base64**: If decoding results in exactly 16 bytes.
2. **Hex**: If the string is exactly 32 characters of valid hex.
3. **Raw UTF-8**: The string is converted to bytes and then padded or truncated to exactly 16 bytes.
