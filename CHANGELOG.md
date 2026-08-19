## [1.0.0] - 2026-08-19

### Added
- Core cryptographic password generation engine using `System.Security.Cryptography.RandomNumberGenerator`.
- Fisher-Yates shuffle algorithm guaranteeing unbiased uniform distribution across selected character sets.
- Shannon entropy calculator ($E = L \times \log_2(N)$) and crack time estimation.
- Comprehensive offline password strength analyzer detecting sequential flaws, repetitions, and dictionary attacks.
- Diceware multi-word passphrase generator with configurable separators, casing, and number injection.
- Numeric PIN generator with customizable digit length.
- Bulk password generator supporting batch generation with CSV, JSON, and TXT export formats.
- Password checker with local analysis and optional k-Anonymity (SHA-1 prefix) breach verification.
- Local password history repository with Windows DPAPI at-rest encryption and SQLite backend (disabled by default).
- Safe clipboard auto-clear service with SHA-256 integrity verification.
- Enterprise password policy evaluator (Windows Active Directory, NIST/ISO 27001, Banking Standard) and custom presets.
- Modern WPF user interface with Dark, Light, and System themes.
- Comprehensive xUnit and FluentAssertions test suite covering generators, analyzers, policies, and security mechanisms.
