using System.Windows.Input;

namespace MidiUtil;

public sealed class RelayCommand : ICommand
{
    private readonly Action<object?> _action;

    public RelayCommand(Action<object?> action)
    {
        _action = action;
    }

    public bool CanExecute(object? parameter)
    {
        return true;
    }

    public void Execute(object? parameter)
    {
        _action(parameter);
    }

    public event EventHandler? CanExecuteChanged;
}
