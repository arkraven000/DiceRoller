using DiceRoller.App.ViewModels;
using DiceRoller.Core.Services.Calculation;
using DiceRoller.Core.Services.Database;
using DiceRoller.Core.Services.Security;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using System;
using System.IO;

namespace DiceRoller.App;

/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// Implements dependency injection and initializes secure database.
/// REQ-AUTH-001: Runs with standard user privileges (see app.manifest).
/// </summary>
public partial class App : Application
{
    private IServiceProvider? _serviceProvider;
    private Window? _mainWindow;

    /// <summary>
    /// Initializes the singleton application object.
    /// </summary>
    public App()
    {
        InitializeComponent();
        ConfigureServices();
    }

    /// <summary>
    /// Configures dependency injection container.
    /// REQ-ARCH-002: Dependency injection for loose coupling.
    /// </summary>
    private void ConfigureServices()
    {
        var services = new ServiceCollection();

        // Core Services
        // REQ-CRYPTO-001: KeyManagementService for DPAPI key protection
        services.AddSingleton<IKeyManagementService, KeyManagementService>();

        // REQ-DATA-001: DatabaseService with SQLCipher encryption
        services.AddSingleton<IDatabaseService, DatabaseService>();

        // REQ-CALC-001: DiceCalculator for Warhammer 40K calculations
        services.AddSingleton<IDiceCalculator, DiceCalculator>();

        // Repositories
        services.AddSingleton<IWeaponRepository, WeaponRepository>();
        services.AddSingleton<IUnitRepository, UnitRepository>();

        // ViewModels
        services.AddTransient<CalculatorViewModel>();
        services.AddTransient<WeaponLibraryViewModel>();
        services.AddTransient<UnitLibraryViewModel>();

        _serviceProvider = services.BuildServiceProvider();
    }

    /// <summary>
    /// Invoked when the application is launched.
    /// Initializes database and creates main window.
    /// </summary>
    protected override async void OnLaunched(LaunchActivatedEventArgs args)
    {
        try
        {
            // Initialize database on startup
            // REQ-DATA-003: Database stored in user's AppData directory
            await InitializeDatabaseAsync();

            // Create main window with grimdark theme
            _mainWindow = new MainWindow(_serviceProvider!);
            _mainWindow.Activate();
        }
        catch (Exception ex)
        {
            // REQ-ERROR-001: Log error (for now, just show to user)
            // In production, this would go to a logging service
            var errorWindow = new Window
            {
                Title = "Initialization Error - The Emperor Protects Not"
            };

            var errorContent = new Microsoft.UI.Xaml.Controls.TextBlock
            {
                Text = $"Failed to initialize application:\n\n{ex.Message}\n\nConsult the Tech-Priests for assistance.",
                TextWrapping = Microsoft.UI.Xaml.TextWrapping.Wrap,
                Margin = new Microsoft.UI.Xaml.Thickness(20)
            };

            errorWindow.Content = errorContent;
            errorWindow.Activate();
        }
    }

    /// <summary>
    /// Initializes encrypted database on first run.
    /// REQ-DATA-001: AES-256 encryption using SQLCipher.
    /// REQ-DATA-002: Key management via DPAPI.
    /// REQ-DATA-003: Database in user AppData directory.
    /// </summary>
    private async Task InitializeDatabaseAsync()
    {
        var keyService = _serviceProvider!.GetRequiredService<IKeyManagementService>();
        var dbService = _serviceProvider.GetRequiredService<IDatabaseService>();

        // Get or create encryption key
        // REQ-DATA-002: DPAPI-protected key from KeyManagementService
        const string keyName = "DiceRollerDatabase";

        // Retrieve protected key from storage
        byte[]? protectedKey = keyService.RetrieveProtectedKey(keyName);
        byte[]? encryptionKey = protectedKey != null ? keyService.UnprotectKey(protectedKey) : null;

        if (encryptionKey == null)
        {
            // First run - generate and store new key
            // REQ-CRYPTO-001: 256-bit AES key generation
            encryptionKey = keyService.GenerateKey();

            // Protect the key with DPAPI before storing
            byte[] protectedNewKey = keyService.ProtectKey(encryptionKey);
            keyService.StoreProtectedKey(protectedNewKey, keyName);
        }

        // REQ-DATA-003: Database path in user's AppData
        string appDataPath = Environment.GetFolderPath(
            Environment.SpecialFolder.LocalApplicationData,
            Environment.SpecialFolderOption.Create);

        string databasePath = Path.Combine(appDataPath, "DiceRoller", "profiles.db");

        // Initialize database with encryption
        // REQ-DATA-001: SQLCipher AES-256 encryption
        dbService.Initialize(databasePath, encryptionKey);

        // Create schema if not exists
        dbService.CreateSchema();

        // Clear key from memory
        // REQ-MEM-002: Sensitive data cleared after use
        Array.Clear(encryptionKey, 0, encryptionKey.Length);
    }

    /// <summary>
    /// Gets service from dependency injection container.
    /// </summary>
    public static T GetService<T>() where T : class
    {
        var app = Current as App;
        return app?._serviceProvider?.GetRequiredService<T>()
            ?? throw new InvalidOperationException($"Service {typeof(T).Name} not found.");
    }
}
