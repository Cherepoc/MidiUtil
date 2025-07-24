using System.Collections.Immutable;
using System.ComponentModel;
using System.Windows.Input;

namespace MidiUtil.Models;

public sealed class MidiFileModel : ModelBase
{
    private bool _canSave = true;
    private byte _maxRandomProgram = 119;
    private byte _minRandomProgram;

    public MidiFileModel(string path, byte[] data, IReadOnlyList<MidiTrackModel> tracks)
    {
        Path = path;
        Data = data;
        Tracks = tracks;

        Filename = System.IO.Path.GetFileName(path);

        foreach (var midiTrackModel in tracks)
            midiTrackModel.PropertyChanged += MidiTrackModelOnPropertyChanged;
    }

    public bool CanSave
    {
        get => _canSave;
        private set => ChangeProperty(ref _canSave, value);
    }

    public string Path { get; }

    public string Filename { get; }

    public byte[] Data { get; }

    public byte MinRandomProgram
    {
        get => _minRandomProgram;
        set
        {
            var changed = ChangeProperty(ref _minRandomProgram, value);
            NotifyAvailableRandomProgramsChanged(changed);
        }
    }

    public byte MaxRandomProgram
    {
        get => _maxRandomProgram;
        set
        {
            var changed = ChangeProperty(ref _maxRandomProgram, value);
            NotifyAvailableRandomProgramsChanged(changed);
        }
    }

    public ImmutableArray<ProgramModel> MinAvailableRandomPrograms => ProgramModel.GetMinAvailablePrograms(_maxRandomProgram);

    public ImmutableArray<ProgramModel> MaxAvailableRandomPrograms => ProgramModel.GetMaxAvailablePrograms(_minRandomProgram);

    public IReadOnlyList<MidiTrackModel> Tracks { get; }

    public ICommand RandomizeAllProgramsCommand => new RelayCommand(RandomizePrograms);

    public ICommand RandomizeProgramCommand => new RelayCommand(RandomizeProgram);

    private void MidiTrackModelOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MidiTrackModel.IsEnabled))
        {
            var hasAtLeastOneTrackEnabledError = Tracks.All(x => !x.IsEnabled);
            CanSave = !hasAtLeastOneTrackEnabledError;
            foreach (var midiTrackModel in Tracks)
                midiTrackModel.SetAtLeasOneTrackEnabledError(hasAtLeastOneTrackEnabledError);
        }
    }

    private void RandomizePrograms(object? target)
    {
        foreach (var midiTrackModel in Tracks)
        foreach (var programChangeEventModel in midiTrackModel.ProgramChangeEvents)
        {
            if (!programChangeEventModel.IsPercussion)
                RandomizeProgram(programChangeEventModel);
        }
    }

    private void RandomizeProgram(object? obj)
    {
        if (obj is not ProgramChangeEventModel programChangeEventModel)
            return;

        programChangeEventModel.Program = (byte)Random.Shared.Next(_minRandomProgram, _maxRandomProgram + 1);
    }

    private void NotifyAvailableRandomProgramsChanged(bool valuesChanged)
    {
        if (!valuesChanged)
            return;

        OnPropertyChanged(nameof(MinAvailableRandomPrograms));
        OnPropertyChanged(nameof(MaxAvailableRandomPrograms));
    }
}
