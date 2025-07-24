using System.Collections.Immutable;

namespace MidiUtil.Models;

public sealed class ProgramModel
{
    public ProgramModel(byte program, string name)
    {
        Program = program;
        Name = name;
    }

    public byte Program { get; }
    public string Name { get; }

    public static ImmutableArray<ProgramModel> Items { get; } = MidiInstrumentNames.Names
        .Select((name, index) => new ProgramModel((byte)index, $"{index} - {name}"))
        .ToImmutableArray();

    public static ImmutableArray<ProgramModel> GetMinAvailablePrograms(byte maxProgram)
    {
        return Items[..(maxProgram + 1)];
    }

    public static ImmutableArray<ProgramModel> GetMaxAvailablePrograms(byte minProgram)
    {
        return Items[minProgram..];
    }
}
