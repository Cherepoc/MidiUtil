namespace MidiUtil.Midi;

public sealed class MidiParserException : Exception
{
    public MidiParserException()
    {
    }

    public MidiParserException(string? message) : base(message)
    {
    }
}
