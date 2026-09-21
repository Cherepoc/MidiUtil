using System.ComponentModel;

namespace MidiUtil.Models;

public sealed class MainModel : ModelBase
{
    public const string AppName = "RMG MIDI Util";

    private MidiFileModel? _midiFile;
    private string? _statusMessage;

    public MidiFileModel? MidiFile
    {
        get => _midiFile;
        set
        {
            var oldFile = _midiFile;
            if (!ChangeProperty(ref _midiFile, value))
                return;

            if (oldFile != null)
                oldFile.PropertyChanged -= MidiFileOnPropertyChanged;
            if (value != null)
                value.PropertyChanged += MidiFileOnPropertyChanged;

            OnPropertyChanged(nameof(Title));
        }
    }

    public string Title => _midiFile == null
        ? AppName
        : $"{(_midiFile.IsDirty ? "*" : "")}{_midiFile.Filename} - {AppName}";

    public string? StatusMessage
    {
        get => _statusMessage;
        set => ChangeProperty(ref _statusMessage, value);
    }

    private void MidiFileOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(MidiFileModel.Filename):
                OnPropertyChanged(nameof(Title));
                break;
            case nameof(MidiFileModel.IsDirty):
                OnPropertyChanged(nameof(Title));
                // an edit makes a previous "Saved" message stale
                if (_midiFile!.IsDirty)
                    StatusMessage = null;
                break;
        }
    }
}
