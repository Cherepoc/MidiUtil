namespace MidiUtil.Midi;

public sealed record Track
{
    public required int StartIndex { get; init; }
    public required int Length { get; init; }
    public required IReadOnlyList<ITrackEvent> Events { get; init; }
}
