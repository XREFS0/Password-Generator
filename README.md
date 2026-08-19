# MASA Password Generator

A secure, modern Windows desktop password generator and cryptographic utility built with **C#**, **.NET 8**, **WPF**, and **MVVM**.

Designed with a focus on cryptographic integrity, memory hygiene, and local offline privacy.

---

## Screenshots

| Dashboard | Password Generator |
|:---:|:---:|
| ![Dashboard](Screenshots/01_dashboard.png) | ![Generator](Screenshots/02_generator.png) |

| Bulk Generator | Passphrase Generator |
|:---:|:---:|
| ![Bulk Generator](Screenshots/03_bulk_generator.png) | ![Passphrase](Screenshots/04_passphrase.png) |

| Password Checker & Analyzer | Presets & Enterprise Policies |
|:---:|:---:|
| ![Checker](Screenshots/05_password_checker.png) | ![Presets & Policies](Screenshots/07_presets_and_policies.png) |

---

## Features

- **Cryptographically Secure RNG**: Exclusively uses `System.Security.Cryptography.RandomNumberGenerator` with Fisher-Yates shuffle algorithms to eliminate modulo bias.
- **Customizable Generation Rules**:
  - Length slider & numeric input (4 to 128 characters).
  - Character class toggles (Uppercase, Lowercase, Digits, Symbols).
  - Ambiguous symbol exclusion (`{}[]()/\\'"` etc.) and visually similar character exclusion (`i, l, 1, L, o, 0, O`).
  - Custom alphanumeric character sets.
- **Entropy & Strength Analyzer**:
  - Calculates real-time Shannon information entropy ($E = L \times \log_2(N)$).
  - Evaluates character variety, sequential patterns (`1234`, `abcd`), repetitions (`aaaa`), and dictionary attacks.
- **Diceware Passphrase Generator**: Multi-word passphrase generator with configurable word count, separator, casing, and number injection.
- **PIN Code Generator**: Customizable digit-length numeric passcode generator.
- **Bulk Generation & Multi-Format Export**: Generates hundreds of passwords at once with export support for **CSV**, **JSON**, and **TXT**.
- **Privacy-Safe Password Checker**: Analyzes password strength and flaws locally. Includes optional k-Anonymity (5-character SHA-1 prefix) breach verification against known breach datasets.
- **Protected History Storage**: Opt-in password history encrypted at rest using **Windows DPAPI (Data Protection API)** and stored in a local SQLite database.
- **Safe Clipboard Engine**: Automatically clears copied passwords after a configurable interval (e.g. 30 seconds), verified by SHA-256 hash checks so subsequent user clipboard actions are never overwritten.
- **Enterprise Policy Validation**: Built-in compliance checks for Windows Active Directory, NIST/ISO 27001, and Banking standards.
- **Modern WPF UI**: Dark, Light, and System themes with rounded modern controls and responsive layout.

---

## Architecture & Project Structure

The project follows Clean Architecture with Dependency Injection:

```
MASA.PasswordGenerator/
├── src/
│   ├── MASA.PasswordGenerator.Core/              # Domain models, enums, interfaces, character set constants
│   ├── MASA.PasswordGenerator.Application/       # Cryptographic generators, entropy calculator, strength analyzer, policies
│   ├── MASA.PasswordGenerator.Infrastructure/    # DPAPI security, SQLite storage, clipboard timer, file export
│   └── MASA.PasswordGenerator.App/               # WPF UI, ViewModels (MVVM Toolkit), themes, views, controls
│
├── tests/
│   └── MASA.PasswordGenerator.Tests/             # xUnit & FluentAssertions test suite
│
├── Screenshots/                                  # High-resolution application screenshots
├── .github/workflows/                            # CI workflows (GitHub Actions)
└── MASA.PasswordGenerator.sln
```

---

## Requirements

- **Windows 10 / 11** (x64)
- **.NET 8.0 SDK** or later ([Download .NET 8](https://dotnet.microsoft.com/download/dotnet/8.0))

---

## Getting Started

### Clone the Repository
```bash
git clone https://github.com/your-username/MASA.PasswordGenerator.git
cd MASA.PasswordGenerator
```

### Build the Solution
```bash
dotnet build MASA.PasswordGenerator.sln
```

### Run Tests
```bash
dotnet test MASA.PasswordGenerator.sln
```

### Launch the Application
```bash
dotnet run --project src/MASA.PasswordGenerator.App/MASA.PasswordGenerator.App.csproj
```

---

## Keyboard Shortcuts

| Shortcut | Action |
|:---|:---|
| `Ctrl + G` | Generate New Password |
| `Ctrl + C` | Copy Current Password |
| `Ctrl + R` | Regenerate Password |
| `Ctrl + H` | Open Password History |
| `Ctrl + ,` | Open Application Settings |

---

## Security Principles

1. **No Insecure RNG**: `System.Random` is never used for password generation.
2. **Zero Telemetry & Offline by Default**: Passwords are never transmitted over the network or logged to files.
3. **Opt-in Storage**: No passwords are persisted to disk unless the user explicitly enables DPAPI-encrypted history in Settings.
4. **Memory Hygiene**: Clipboard auto-clear safely purges credentials from memory and clipboard after use.

---

## License

This project is licensed under the [MIT License](LICENSE).
