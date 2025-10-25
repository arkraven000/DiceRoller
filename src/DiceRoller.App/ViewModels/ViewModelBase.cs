using CommunityToolkit.Mvvm.ComponentModel;
using System.Threading.Tasks;

namespace DiceRoller.App.ViewModels;

/// <summary>
/// Base class for all ViewModels.
/// Provides common functionality and implements INotifyPropertyChanged via MVVM Toolkit.
/// </summary>
public abstract class ViewModelBase : ObservableObject
{
    private bool _isBusy;
    private string _statusMessage = string.Empty;

    /// <summary>
    /// Indicates whether the ViewModel is currently performing an operation.
    /// </summary>
    public bool IsBusy
    {
        get => _isBusy;
        set => SetProperty(ref _isBusy, value);
    }

    /// <summary>
    /// Status message to display to the user.
    /// </summary>
    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    /// <summary>
    /// Called when the ViewModel is activated (navigated to).
    /// Synchronous wrapper that calls the async version.
    /// </summary>
    public virtual void OnNavigatedTo()
    {
        // Call async version without waiting
        _ = OnNavigatedToAsync();
    }

    /// <summary>
    /// Called when the ViewModel is activated (navigated to).
    /// Async version for derived classes that need to perform async operations.
    /// </summary>
    public virtual Task OnNavigatedToAsync()
    {
        // Override in derived classes
        return Task.CompletedTask;
    }

    /// <summary>
    /// Called when the ViewModel is deactivated (navigated away from).
    /// </summary>
    public virtual void OnNavigatedFrom()
    {
        // Override in derived classes
    }
}
