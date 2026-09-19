using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace LearningContentValidator.App.ViewModels;

/// <summary>
/// Foundation base class for all ViewModels implementing INotifyPropertyChanged.
/// Encapsulates change notification mechanics using C# CallerMemberName attribute,
/// eliminating magic strings and ensuring clean two-way data binding with WPF views.
/// </summary>
public abstract class ViewModelBase : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Updates field value and raises PropertyChanged if value actually changed.
    /// Demonstrates encapsulation, ref parameters, and generic method design.
    /// </summary>
    protected bool SetProperty<T>(
        ref T storage,
        T value,
        [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(storage, value))
            return false;

        storage = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    /// <summary>
    /// Raises the PropertyChanged event for the specified property name.
    /// </summary>
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

