# Warhammer 40K Dice Calculator

A Windows 11 desktop application for calculating dice probabilities and expected outcomes for Warhammer 40,000 10th Edition tabletop battles.

## Features

- **Accurate 10th Edition Mechanics**: Implements all core rules including hit/wound/save rolls
- **Advanced Weapon Abilities**: Supports Lethal Hits, Sustained Hits, Devastating Wounds, Torrent, and more
- **Probability Calculator**: Calculate expected damage and model casualties
- **Monte Carlo Simulation**: Run thousands of simulated dice rolls to see outcome distributions
- **Unit Library**: Save and manage attacker/defender profiles
- **Modern Windows 11 UI**: Built with WinUI 3 and Fluent Design

## System Requirements

- **OS**: Windows 11 22H2 or later
- **Runtime**: .NET 8.0 Runtime or SDK
- **Architecture**: x64

## Security

This application is built with security as a priority:

- **Offline-Only**: No network connectivity required or used
- **Encrypted Storage**: SQLite database encrypted with AES-256 via SQLCipher
- **Code Signed**: Application binary is Authenticode signed
- **Secure Coding**: Follows OWASP and NIST secure development practices
- **Regular Security Audits**: Dependencies scanned for vulnerabilities

See [SECURITY_REQUIREMENTS.md](SECURITY_REQUIREMENTS.md) for detailed security documentation.

## Building from Source

### Prerequisites

1. Windows 11 22H2 or later
2. Visual Studio 2022 (17.8+) with:
   - .NET Desktop Development workload
   - Windows App SDK development workload
3. .NET 8.0 SDK

### Build Steps

```powershell
# Clone the repository
git clone https://github.com/your-org/DiceRoller.git
cd DiceRoller

# Restore NuGet packages
dotnet restore

# Build the solution
dotnet build --configuration Release

# Run tests
dotnet test

# Run the application
dotnet run --project src/DiceRoller.App/DiceRoller.App.csproj
```

## Project Structure

```
DiceRoller/
├── src/
│   ├── DiceRoller.App/          # WinUI 3 desktop application
│   │   ├── Views/               # XAML UI views
│   │   ├── ViewModels/          # MVVM ViewModels
│   │   └── Services/            # Application services
│   ├── DiceRoller.Core/         # Core business logic
│   │   ├── Models/              # Domain models
│   │   ├── Services/            # Core services
│   │   │   ├── Calculation/     # Dice calculation engine
│   │   │   ├── Database/        # Database operations
│   │   │   └── Security/        # Encryption & key management
│   │   └── Enums/               # Enumerations
│   └── DiceRoller.Core.Tests/   # xUnit tests
│       ├── Unit/                # Unit tests
│       ├── Integration/         # Integration tests
│       └── Security/            # Security tests
├── docs/                        # Additional documentation
├── SECURITY_REQUIREMENTS.md     # Security requirements doc
├── .editorconfig                # Code style rules
└── Directory.Build.props        # Common build properties
```

## Architecture

This application follows the **MVVM (Model-View-ViewModel)** pattern:

- **View**: WinUI 3 XAML UI components
- **ViewModel**: Business logic, input validation, commands
- **Model**: Domain entities and data access

### Security Architecture

```
User Input → View → ViewModel (Validation) → Model → Encrypted SQLite DB
```

Multiple layers of validation ensure data integrity and prevent injection attacks.

## Development

### Code Style

This project uses `.editorconfig` to enforce consistent code style. Key conventions:

- 4 spaces for indentation
- No `var` keyword (explicit types)
- PascalCase for public members
- Security analyzer rules as errors

### Security Guidelines

When contributing code:

1. **Input Validation**: Always validate in ViewModel layer
2. **SQL Queries**: Use parameterized queries only (never string concatenation)
3. **Cryptography**: Use .NET BCL crypto APIs only (no custom implementations)
4. **Error Handling**: Don't expose sensitive information in error messages
5. **Dependencies**: Only use well-known NuGet packages from verified publishers

### Testing Requirements

- Minimum 80% code coverage
- All security-critical functions must have dedicated tests
- Tests for SQL injection prevention
- Tests for integer overflow scenarios
- Cryptographic operation verification

## Contributing

1. Read [SECURITY_REQUIREMENTS.md](SECURITY_REQUIREMENTS.md)
2. Create a feature branch
3. Write code following security guidelines
4. Add tests (minimum 80% coverage)
5. Ensure all security analyzers pass
6. Submit pull request

## Security Vulnerabilities

If you discover a security vulnerability, please email security@diceroller.example.com instead of using the issue tracker.

## License

MIT License - See LICENSE file for details

## Acknowledgments

- Built with [WinUI 3](https://learn.microsoft.com/en-us/windows/apps/winui/)
- Database encryption via [SQLCipher](https://www.zetetic.net/sqlcipher/)
- MVVM toolkit by [CommunityToolkit](https://github.com/CommunityToolkit)
- Warhammer 40,000 is a registered trademark of Games Workshop Limited

## Disclaimer

This is an unofficial fan-made tool. Not affiliated with or endorsed by Games Workshop.
