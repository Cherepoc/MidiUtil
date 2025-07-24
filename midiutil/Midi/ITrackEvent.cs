namespace MidiUtil.Midi;

public interface ITrackEvent
{
    int StartIndex { get; }

    int DataStartIndex { get; }

    byte[] Delta { get; }

    int Length { get; }
}
