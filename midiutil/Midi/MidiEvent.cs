namespace MidiUtil.Midi;

public sealed record MidiEvent : ITrackEvent
{
    public required byte Channel { get; init; }
    public required byte MidiType { get; init; }
    public required byte[] MidiData { get; init; }
    public required int StartIndex { get; init; }
    public required int DataStartIndex { get; init; }
    public required byte[] Delta { get; init; }
    public required int Length { get; init; }
}
