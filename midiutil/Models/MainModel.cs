namespace MidiUtil.Models;

public sealed class MainModel : ModelBase
{
    private MidiFileModel? _midiFile;

    public MidiFileModel? MidiFile
    {
        get => _midiFile;
        set => ChangeProperty(ref _midiFile, value);
    }
}
