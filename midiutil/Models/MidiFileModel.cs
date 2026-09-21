using System.Collections.Immutable;
using System.ComponentModel;
using System.Windows.Input;

namespace MidiUtil.Models;

public sealed class MidiFileModel : ModelBase
{
    private bool _canSave = true;
    private string _filename;
    private bool _isDirty;
    private string _path;
    private byte _maxRandomProgram = 119;
    private byte _minRandomProgram;

    public MidiFileModel(string path, byte[] data, IReadOnlyList<MidiTrackModel> tracks)
    {
        _path = path;
        Data = data;
        Tracks = tracks;

        _filename = System.IO.Path.GetFileName(path);

        RandomizeAllProgramsCommand = new RelayCommand(RandomizePrograms);
        RandomizeProgramCommand = new RelayCommand(RandomizeProgram);

        foreach (var midiTrackModel in tracks)
        {
            midiTrackModel.PropertyChanged += MidiTrackModelOnPropertyChanged;
            foreach (var programChangeEventModel in midiTrackModel.ProgramChangeEvents)
                programChangeEventModel.PropertyChanged += ProgramChangeEventModelOnPropertyChanged;
        }
    }

    public bool CanSave
    {
        get => _canSave;
        private set => ChangeProperty(ref _canSave, value);
    }

    public string Path
    {
        get => _path;
        private set => ChangeProperty(ref _path, value);
    }

    public string Filename
    {
        get => _filename;
        private set => ChangeProperty(ref _filename, value);
    }

    /// <summary>
    ///     True when there are edits that have not been written to disk.
    /// </summary>
    public bool IsDirty
    {
        get => _isDirty;
        private set => ChangeProperty(ref _isDirty, value);
    }

    public void MarkSaved(string path)
    {
        Path = path;
        Filename = System.IO.Path.GetFileName(path);
        IsDirty = false;
    }

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

    public ICommand RandomizeAllProgramsCommand { get; }

    public ICommand RandomizeProgramCommand { get; }

    private void MidiTrackModelOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MidiTrackModel.IsEnabled))
        {
            IsDirty = true;
            var hasAtLeastOneTrackEnabledError = Tracks.All(x => !x.IsEnabled);
            CanSave = !hasAtLeastOneTrackEnabledError;
            foreach (var midiTrackModel in Tracks)
                midiTrackModel.SetAtLeasOneTrackEnabledError(hasAtLeastOneTrackEnabledError);
        }
    }

    private void ProgramChangeEventModelOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ProgramChangeEventModel.Program))
            IsDirty = true;
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
