# FemonReverse - Context Summary

## Project Overview
**FemonReverse** is a .NET 10.0 CLI tool designed to reverse engineer the behavior of the **Femon Play** Android application. Its primary purpose is to extract, decrypt, and sign TV channel URLs.

## High-Level Workflow
1. **Configuration Fetch**: Retrieves remote settings from Firebase Remote Config via REST API.
2. **Channel Extraction**: Downloads a JSON file containing encrypted channel data.
3. **Decryption**: Applies a double AES/ECB decryption process to recover URLs and headers.
4. **CDN Signing (Flow)**: For channels using Flow CDN, it acquires a bearer token and signs the URLs via a CDN generator.
5. **Export**: Saves the final decrypted and processed JSON to disk.

## Technical Stack
- **Framework**: .NET 10.0
- **Logging**: NLog (Daily rotating files)
- **Cryptography**: System.Security.Cryptography (AES/ECB/PKCS7)
- **JSON**: System.Text.Json

## Core File Map
- `Program.cs`: Main orchestrator and entry point.
- `RemoteConfig.cs`: Handles communication with Firebase API.
- `ChannelDecryptor.cs`: Implements the AES decryption algorithms and data models.
- `FlowSigner.cs`: Manages bearer token acquisition and Flow CDN URL signing.
- `NLog.config`: Logging configuration.
