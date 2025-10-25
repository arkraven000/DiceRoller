using CommunityToolkit.Mvvm.ComponentModel;

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
    /// </summary>
    public virtual void OnNavigatedTo()
    {
        // Override in derived classes
    }

    /// <summary>
    /// Called when the ViewModel is deactivated (navigated away from).
    /// </summary>
    public virtual void OnNavigatedFrom()
    {
        // Override in derived classes
    }
}
