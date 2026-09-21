using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Platform.Storage;
using MidiUtil.Midi;
using MidiUtil.Models;

namespace MidiUtil;

public partial class MainWindow : Window
{
    private static readonly FilePickerFileType MidiFileType = new("MIDI Files")
    {
        Patterns = ["*.mid", "*.midi"],
        AppleUniformTypeIdentifiers = ["public.midi-audio"],
        MimeTypes = ["audio/midi", "audio/x-midi"]
    };

    private const string FileDialogDefaultExtension = "mid";

    public MainWindow()
    {
        InitializeComponent();
        DataContext = Model;
    }

    private MainModel Model { get; } = new();

    private void MenuItemExit_OnClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    private async void MenuItemOpen_OnClick(object? sender, RoutedEventArgs e)
    {
        var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            AllowMultiple = false,
            FileTypeFilter = [MidiFileType, FilePickerFileTypes.All]
        });
        if (files.Count == 0)
            return;

        try
        {
            var file = files[0];
            await using var stream = await file.OpenReadAsync();
            var midiFile = MidiParser.Parse(stream);
            Model.MidiFile = MidiFileModelConverter.ConvertToModel(file.Path.LocalPath, midiFile);
        }
        catch (Exception ex)
        {
            await ShowErrorMessage($"Error reading MIDI file: {ex.Message}");
        }
    }

    private async void MenuItemSave_OnClick(object? sender, RoutedEventArgs e)
    {
        await SaveFile(Model.MidiFile!.Path);
    }

    private async void MenuItemSaveAs_OnClick(object? sender, RoutedEventArgs e)
    {
        var midiFile = Model.MidiFile!;
        var directory = Path.GetDirectoryName(midiFile.Path);
        var file = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            DefaultExtension = FileDialogDefaultExtension,
            FileTypeChoices = [MidiFileType, FilePickerFileTypes.All],
            SuggestedFileName = midiFile.Filename,
            SuggestedStartLocation = directory == null
                ? null
                : await StorageProvider.TryGetFolderFromPathAsync(directory)
        });
        if (file == null)
            return;

        await SaveFile(file.Path.LocalPath);
    }

    private async Task SaveFile(string path)
    {
        try
        {
            var bytes = MidiFileModelConverter.SaveToBytes(Model.MidiFile!);
            await File.WriteAllBytesAsync(path, bytes);
        }
        catch (Exception ex)
        {
            await ShowErrorMessage($"Error saving MIDI file: {ex.Message}");
        }
    }

    private Task ShowErrorMessage(string message)
    {
        var okButton = new Button
        {
            Content = "OK",
            HorizontalAlignment = HorizontalAlignment.Right,
            Padding = new Avalonia.Thickness(16, 4)
        };
        var dialog = new Window
        {
            Title = "Error",
            Width = 400,
            SizeToContent = SizeToContent.Height,
            CanResize = false,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Content = new StackPanel
            {
                Margin = new Avalonia.Thickness(16),
                Spacing = 16,
                Children =
                {
                    new TextBlock { Text = message, TextWrapping = Avalonia.Media.TextWrapping.Wrap },
                    okButton
                }
            }
        };
        okButton.Click += (_, _) => dialog.Close();
        return dialog.ShowDialog(this);
    }
}
