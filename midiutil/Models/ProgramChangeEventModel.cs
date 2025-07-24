namespace MidiUtil.Models;

public sealed class ProgramChangeEventModel : ModelBase
{
    private byte _channel;
    private byte _program;

    public ProgramChangeEventModel(byte channel, byte program, IReadOnlyList<int> indexes)
    {
        _channel = channel;
        _program = program;
        Indexes = indexes;

        Name = FormatName(channel, program);
    }

    public string Name { get; }

    public byte Channel
    {
        get => _channel;
        set => ChangeProperty(ref _channel, value);
    }

    public byte Program
    {
        get => _program;
        set => ChangeProperty(ref _program, value);
    }

    public IReadOnlyList<int> Indexes { get; }

    public bool IsPercussion => Channel == 9;

    private static string FormatName(byte channel, byte program)
    {
        return channel switch
        {
            9 => "Percussion",
            _ => $"Channel {channel} - {program} {MidiInstrumentNames.Names[program]}"
        };
    }
}
