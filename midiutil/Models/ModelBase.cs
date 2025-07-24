using System.Collections;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MidiUtil.Models;

public abstract class ModelBase : INotifyPropertyChanged, INotifyDataErrorInfo
{
    private readonly Dictionary<string, IReadOnlyList<string>> _errors = new();

    public IEnumerable GetErrors(string? propertyName)
    {
        var key = propertyName ?? string.Empty;
        if (_errors.TryGetValue(key, out var errors))
            return errors;
        return Enumerable.Empty<List<string>>();
    }

    public bool HasErrors => _errors.Count > 0;

    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void SetErrors(string propertyName, IReadOnlyList<string> errors)
    {
        var currentErrors = _errors.GetValueOrDefault(propertyName, []);
        if (currentErrors.SequenceEqual(errors))
            return;

        if (errors.Count == 0)
            _errors.Remove(propertyName);
        else
            _errors[propertyName] = errors.ToArray();

        var handler = ErrorsChanged;
        if (handler != null)
            handler(this, new DataErrorsChangedEventArgs(propertyName));
    }

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        var handler = PropertyChanged;
        if (handler != null)
            handler(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool ChangeProperty<T>(ref T currentValue, T newValue, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(currentValue, newValue))
            return false;

        currentValue = newValue;
        OnPropertyChanged(propertyName);
        return true;
    }
}
