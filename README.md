# RMG MIDI Util
A small utility with GUI for midi file manipulation.
Features include:
- Viewing track and program data
- Removing entire tracks
- Randomizing track programs

Only midi files of format 1 (single song multiple tracks) without sysex events are supported.

Cross-platform (Windows, Linux, macOS), built with [Avalonia UI](https://avaloniaui.net/) on .NET 10.

For running a published build install the [.NET 10 Runtime](https://dotnet.microsoft.com/en-us/download/dotnet/10.0).
For building and running from source install the [.NET 10 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)
(on Arch/CachyOS: `sudo pacman -S dotnet-sdk`), then:

```
dotnet run --project midiutil
```

To publish a self-contained build: `dotnet publish midiutil -c Release -r linux-x64 --self-contained` (also `win-x64`, `osx-arm64`).

## License

MIT, see [LICENSE](LICENSE).
