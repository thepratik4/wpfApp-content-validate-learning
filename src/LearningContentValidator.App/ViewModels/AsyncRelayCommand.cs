using System.Windows.Input;

namespace LearningContentValidator.App.ViewModels;

/// <summary>
/// Asynchronous ICommand implementation designed to run async tasks cleanly from WPF bindings.
/// Automatically prevents re-entrancy, tracks execution state, and safely handles exceptions.
/// </summary>
public sealed class AsyncRelayCommand : ICommand
{
    private readonly Func<object?, Task> _execute;
    private readonly Predicate<object?>? _canExecute;
    private readonly Action<Exception>? _onException;
    private bool _isExecuting;

    public AsyncRelayCommand(
        Func<object?, Task> execute,
        Predicate<object?>? canExecute = null,
        Action<Exception>? onException = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
        _onException = onException;
    }

    public AsyncRelayCommand(
        Func<Task> execute,
        Func<bool>? canExecute = null,
        Action<Exception>? onException = null)
        : this(_ => execute(), canExecute != null ? _ => canExecute() : null, onException)
    {
    }

    public bool IsExecuting
    {
        get => _isExecuting;
        private set
        {
            _isExecuting = value;
            RaiseCanExecuteChanged();
        }
    }

    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    public bool CanExecute(object? parameter)
    {
        return !_isExecuting && (_canExecute?.Invoke(parameter) ?? true);
    }

    public async void Execute(object? parameter)
    {
        if (!CanExecute(parameter))
            return;

        try
        {
            IsExecuting = true;
            await _execute(parameter);
        }
        catch (OperationCanceledException)
        {
            // Cooperative cancellation is an expected workflow, not an error
        }
        catch (Exception ex)
        {
            _onException?.Invoke(ex);
        }
        finally
        {
            IsExecuting = false;
        }
    }

    public void RaiseCanExecuteChanged() => CommandManager.InvalidateRequerySuggested();
}

