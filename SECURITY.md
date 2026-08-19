# Security Policy

## Supported Versions

| Version | Supported          |
| ------- | ------------------ |
| 1.0.x   | :white_check_mark: |

## Reporting a Vulnerability

Security and offline privacy are fundamental principles of MASA Password Generator.

If you discover a security vulnerability or privacy flaw:

1. **Do not create a public issue on GitHub.**
2. Report the vulnerability privately by opening a GitHub Private Security Advisory or contacting the maintainers directly.
3. Provide a detailed summary, proof of concept, and steps to reproduce.

### Cryptographic & Privacy Assurances
- MASA Password Generator uses `System.Security.Cryptography.RandomNumberGenerator` exclusively. `System.Random` is never used for security-critical functions.
- Passwords are never logged or transmitted to remote servers.
- Password history storage is disabled by default and encrypted using Windows DPAPI when enabled.
- Breach verification uses a privacy-preserving k-Anonymity model (5-character SHA-1 prefix) and is strictly opt-in.
