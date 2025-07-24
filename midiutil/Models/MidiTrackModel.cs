namespace MidiUtil.Models;

public sealed class MidiTrackModel : ModelBase
{
    private static readonly IReadOnlyList<string> AtLeastOneTrackErrors = ["At least one track must be enabled."];

    private bool _isEnabled = true;

    public MidiTrackModel(
        int index,
        int startIndex,
        int length,
        IReadOnlyList<ProgramChangeEventModel> programChangeEvents
    )
    {
        StartIndex = startIndex;
        Length = length;
        ProgramChangeEvents = programChangeEvents;

        Name = GenerateName(index, programChangeEvents);
    }

    public string Name { get; }

    public bool IsEnabled
    {
        get => _isEnabled;
        set => ChangeProperty(ref _isEnabled, value);
    }

    public int StartIndex { get; }

    public int Length { get; }

    public IReadOnlyList<ProgramChangeEventModel> ProgramChangeEvents { get; }

    public void SetAtLeasOneTrackEnabledError(bool value)
    {
        SetErrors(nameof(IsEnabled), value ? AtLeastOneTrackErrors : []);
    }

    private static string GenerateName(int index, IReadOnlyList<ProgramChangeEventModel> programChangeEvents)
    {
        if (programChangeEvents.Count == 0)
            return $"{index}: System Track (No Program Change Events)";

        var channelCount = programChangeEvents
            .Select(x => x.Channel)
            .Distinct()
            .Count();
        var programCount = programChangeEvents
            .Select(x => x.Channel)
            .Distinct()
            .Count();

        var channel = programChangeEvents[0].Channel;
        var channelText = channelCount > 1
            ? "Multiple Channels"
            : $"Channel {channel}";

        var program = programChangeEvents[0].Program;
        var programText = (programCount, channel) switch
        {
            (> 1, _) => "Multiple Programs",
            (_, 9) => "Percussion",
            (_, _) => $"{program} {MidiInstrumentNames.Names[program]}"
        };

        return $"{index}: {channelText} - {programText}";
    }
}
