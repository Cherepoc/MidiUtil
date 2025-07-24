namespace MidiUtil.Midi;

public sealed record MidiFile
{
    public required byte[] Data { get; init; }

    public required IReadOnlyList<Track> Tracks { get; init; }
}
