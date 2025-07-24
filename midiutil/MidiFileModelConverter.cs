using System.Buffers.Binary;
using MidiUtil.Midi;
using MidiUtil.Models;

namespace MidiUtil;

public static class MidiFileModelConverter
{
    public static MidiFileModel ConvertToModel(string path, MidiFile midiFile)
    {
        var tracks = new List<MidiTrackModel>(midiFile.Tracks.Count);
        for (var i = 0; i < midiFile.Tracks.Count; i++)
        {
            var midiTrack = midiFile.Tracks[i];

            var programChanges = midiTrack.Events
                .OfType<MidiEvent>()
                .Where(x => x.MidiType == 0xC0)
                .GroupBy(x => (x.Channel, x.MidiData[0]))
                .Select(x =>
                    {
                        var (channel, program) = x.Key;
                        var indexes = x
                            .Select(e => e.DataStartIndex + 1)
                            .ToArray();
                        return new ProgramChangeEventModel(channel, program, indexes);
                    }
                )
                .ToArray();

            var track = new MidiTrackModel(i, midiTrack.StartIndex, midiTrack.Length, programChanges);
            tracks.Add(track);
        }

        return new MidiFileModel(path, midiFile.Data, tracks);
    }

    public static byte[] SaveToBytes(MidiFileModel model)
    {
        var totalExcludedLength = 0;
        var trackCount = 0;
        foreach (var midiTrackModel in model.Tracks)
        {
            if (midiTrackModel.IsEnabled)
                trackCount++;
            else
                totalExcludedLength += midiTrackModel.Length;
        }

        var newData = new byte[model.Data.Length - totalExcludedLength];
        // Copy the header, format, track count, division
        Array.Copy(model.Data, 0, newData, 0, 14);

        // update track count
        var trackCountSpan = newData.AsSpan(10, 2);
        BinaryPrimitives.WriteUInt16BigEndian(trackCountSpan, (ushort)trackCount);

        var position = 14;
        var excludedLength = 0;
        foreach (var midiTrackModel in model.Tracks)
        {
            if (!midiTrackModel.IsEnabled)
            {
                excludedLength += midiTrackModel.Length;
                continue;
            }

            // copy the whole byte array of the track
            Array.Copy(model.Data, midiTrackModel.StartIndex, newData, position, midiTrackModel.Length);

            // tweak individual program change events
            foreach (var programChangeEventModel in midiTrackModel.ProgramChangeEvents)
            foreach (var index in programChangeEventModel.Indexes)
                newData[index - excludedLength] = programChangeEventModel.Program;

            position += midiTrackModel.Length;
        }

        return newData;
    }
}
