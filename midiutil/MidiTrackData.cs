namespace MidiUtil;

public sealed record MidiTrackData
{
    public required int TrackStartIndex { get; init; }
    public required int TrackLength { get; init; }
    public required int InstrumentEventIndex { get; init; }
    public required int Channel { get; init; }
    public required int Instrument { get; init; }
    public required bool IsPercussion { get; init; }
}
