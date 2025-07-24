namespace MidiUtil.Midi;

public sealed record MetaEvent : ITrackEvent
{
    public required byte MetaType { get; init; }
    public required byte[] MetaData { get; init; }
    public required int StartIndex { get; init; }
    public required int DataStartIndex { get; init; }
    public required byte[] Delta { get; init; }
    public required int Length { get; init; }
}
