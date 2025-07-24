using System.IO;

namespace MidiUtil.Midi;

public static class MidiParser
{
    // "MThd", 6 bytes header size
    private static readonly byte[] MidiHeader = [0x4D, 0x54, 0x68, 0x64, 0x00, 0x00, 0x00, 0x06];

    // "MTrk"
    private static readonly byte[] TrackHeader = [0x4D, 0x54, 0x72, 0x6B];

    public static MidiFile Parse(Stream stream)
    {
        var reader = new BigEndianBinaryReader(stream);
        ValidateSequence(reader, MidiHeader);

        var format = reader.ReadUInt16();
        if (format != 1)
            throw new MidiParserException();

        var trackCount = reader.ReadUInt16();
        if (trackCount == 0)
            throw new MidiParserException("MIDI file must contain at least one track.");

        // skip time division
        reader.ReadUInt16();

        var tracks = new List<Track>(trackCount);
        for (var i = 0; i < trackCount; i++)
        {
            var track = ParseTrack(reader);
            tracks.Add(track);
        }

        if (tracks.Count != trackCount)
            throw new MidiParserException("Not all tracks were parsed successfully.");

        if (reader.Position != reader.Length)
            throw new MidiParserException("Unexpected data after the last track.");

        return new MidiFile
        {
            Data = reader.ToArray(),
            Tracks = tracks
        };
    }

    private static Track ParseTrack(BigEndianBinaryReader reader)
    {
        var trackStartIndex = reader.Position;
        ValidateSequence(reader, TrackHeader);

        var trackLength = reader.ReadUInt32();
        var trackDataStartIndex = reader.Position;
        var trackDataEndIndex = trackDataStartIndex + trackLength;

        var events = new List<ITrackEvent>();
        while (true)
        {
            var trackEvent = ParseTrackEvent(reader);
            events.Add(trackEvent);

            var trackEventEndIndex = reader.Position;

            // track data exceeds the declared length
            if (trackEventEndIndex > trackDataEndIndex)
                throw new MidiParserException("Unexpected end of track data.");

            // End of track event is the last event in the track and is exactly at the end of the declared length
            var eventIsEndOfTrack = trackEvent is MetaEvent { MetaType: 0x2F };
            if (!eventIsEndOfTrack)
                continue;

            if (trackEventEndIndex == trackDataEndIndex)
                break;

            // End of track event is not at the end of the declared length
            throw new MidiParserException("Unexpected end of track data.");
        }

        if (events.Count == 0)
            throw new MidiParserException("Track contains no events.");

        var trackEndIndex = reader.Position;
        return new Track
        {
            StartIndex = trackStartIndex,
            Length = trackEndIndex - trackStartIndex,
            Events = events
        };
    }

    private static ITrackEvent ParseTrackEvent(BigEndianBinaryReader reader)
    {
        var startIndex = reader.Position;
        var delta = GetDelta(reader);
        var dataStartIndex = reader.Position;

        var eventHeader = reader.ReadByte();

        // sysex are not supported
        if (eventHeader is 0xF0 or 0xF7)
            throw new MidiParserException("SysEx events are not supported in this parser.");

        // meta
        if (eventHeader == 0xFF)
            return ParseMetaEvent(reader, startIndex, delta, dataStartIndex);

        // channel event
        return ParseMidiEvent(reader, startIndex, delta, dataStartIndex, eventHeader);
    }

    private static MetaEvent ParseMetaEvent(BigEndianBinaryReader reader, int startIndex, byte[] delta, int dataStartIndex)
    {
        var metaType = reader.ReadByte();
        var metaDataLength = reader.ReadByte();
        var metaData = reader.ReadBytes(metaDataLength);
        var endIndex = reader.Position;

        return new MetaEvent
        {
            StartIndex = startIndex,
            DataStartIndex = dataStartIndex,
            Delta = delta,
            Length = endIndex - startIndex,
            MetaType = metaType,
            MetaData = metaData
        };
    }

    private static MidiEvent ParseMidiEvent(BigEndianBinaryReader reader, int startIndex, byte[] delta, int dataStartIndex, byte eventHeader)
    {
        var channelEventType = eventHeader & 0xF0;
        var channel = eventHeader & 0x0F;

        var midiDataLength = channelEventType switch
        {
            0xC0 or 0xD0 => 1, // Program change and Channel pressure have one data byte
            _ => 2 // All other channel events have two data bytes
        };
        var midiData = reader.ReadBytes(midiDataLength);

        var endIndex = reader.Position;

        return new MidiEvent
        {
            StartIndex = startIndex,
            DataStartIndex = dataStartIndex,
            Delta = delta,
            Length = endIndex - startIndex,
            Channel = (byte)channel,
            MidiType = (byte)channelEventType,
            MidiData = midiData
        };
    }

    private static void ValidateSequence(BigEndianBinaryReader reader, byte[] target)
    {
        if (target.Length == 0)
            return;

        foreach (var b in target)
        {
            if (reader.ReadByte() != b)
                throw new MidiParserException();
        }
    }

    private static byte[] GetDelta(BigEndianBinaryReader reader)
    {
        var delta = new List<byte>(2);
        while (true)
        {
            var deltaByte = reader.ReadByte();
            delta.Add(deltaByte);
            if ((deltaByte & 0x80) == 0)
                break;
        }

        return delta.ToArray();
    }
}
