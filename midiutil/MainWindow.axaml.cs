using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using MidiUtil.Midi;
using MidiUtil.Models;

namespace MidiUtil;

public partial class MainWindow : Window
{
    private const string FileDialogDefaultExtension = "mid";

    private static readonly FilePickerFileType MidiFileType = new("MIDI Files")
    {
        Patterns = ["*.mid", "*.midi"],
        AppleUniformTypeIdentifiers = ["public.midi-audio"],
        MimeTypes = ["audio/midi", "audio/x-midi"]
    };

    private bool _closeConfirmed;

    public MainWindow()
    {
        InitializeComponent();
        DataContext = Model;

        DragDrop.SetAllowDrop(this, true);
        AddHandler(DragDrop.DragOverEvent, OnDragOver);
        AddHandler(DragDrop.DropEvent, OnDrop);
    }

    private MainModel Model { get; } = new();

    #region Keyboard shortcuts

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (e.Handled || !e.KeyModifiers.HasFlag(KeyModifiers.Control))
            return;

        var shift = e.KeyModifiers.HasFlag(KeyModifiers.Shift);
        switch (e.Key)
        {
            case Key.O when !shift:
                e.Handled = true;
                _ = OpenWithPickerAsync();
                break;
            case Key.S when !shift:
                e.Handled = true;
                _ = SaveAsync();
                break;
            case Key.S:
                e.Handled = true;
                _ = SaveAsAsync();
                break;
        }
    }

    #endregion

    #region Closing

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        base.OnClosing(e);
        if (_closeConfirmed || Model.MidiFile is not { IsDirty: true })
            return;

        // the confirmation is asynchronous, so cancel now and close again once the user agrees
        e.Cancel = true;
        _ = CloseAfterConfirmAsync();
    }

    private async Task CloseAfterConfirmAsync()
    {
        if (!await ConfirmDiscardAsync())
            return;

        _closeConfirmed = true;
        Close();
    }

    #endregion

    #region Menu handlers

    private void MenuItemExit_OnClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    private void MenuItemOpen_OnClick(object? sender, RoutedEventArgs e)
    {
        _ = OpenWithPickerAsync();
    }

    private void MenuItemSave_OnClick(object? sender, RoutedEventArgs e)
    {
        _ = SaveAsync();
    }

    private void MenuItemSaveAs_OnClick(object? sender, RoutedEventArgs e)
    {
        _ = SaveAsAsync();
    }

    #endregion

    #region Drag and drop

    private static IStorageFile? GetDroppedFile(DragEventArgs e)
    {
        return e.DataTransfer.TryGetFiles()?.OfType<IStorageFile>().FirstOrDefault();
    }

    private static void OnDragOver(object? sender, DragEventArgs e)
    {
        e.DragEffects = GetDroppedFile(e) != null ? DragDropEffects.Copy : DragDropEffects.None;
        e.Handled = true;
    }

    private async void OnDrop(object? sender, DragEventArgs e)
    {
        e.Handled = true;
        var file = GetDroppedFile(e);
        if (file == null)
            return;

        if (!await ConfirmDiscardAsync())
            return;

        await OpenFileAsync(file);
    }

    #endregion

    #region Open / save

    private async Task OpenWithPickerAsync()
    {
        if (!await ConfirmDiscardAsync())
            return;

        var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            AllowMultiple = false,
            FileTypeFilter = [MidiFileType, FilePickerFileTypes.All]
        });
        if (files.Count == 0)
            return;

        await OpenFileAsync(files[0]);
    }

    private async Task OpenFileAsync(IStorageFile file)
    {
        try
        {
            await using var stream = await file.OpenReadAsync();
            var midiFile = MidiParser.Parse(stream);
            var model = MidiFileModelConverter.ConvertToModel(file.Path.LocalPath, midiFile);
            Model.MidiFile = model;
            Model.StatusMessage = $"Opened {model.Filename} ({model.Tracks.Count} tracks)";
        }
        catch (Exception ex)
        {
            await ShowMessageAsync("Can't open file", $"{file.Name} could not be opened.\n\n{ex.Message}");
        }
    }

    private async Task<bool> SaveAsync()
    {
        if (Model.MidiFile is not { CanSave: true } midiFile)
            return false;

        return await SaveFileAsync(midiFile.Path);
    }

    private async Task SaveAsAsync()
    {
        if (Model.MidiFile is not { CanSave: true } midiFile)
            return;

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

        await SaveFileAsync(file.Path.LocalPath);
    }

    private async Task<bool> SaveFileAsync(string path)
    {
        var midiFile = Model.MidiFile!;
        try
        {
            var bytes = MidiFileModelConverter.SaveToBytes(midiFile);
            await File.WriteAllBytesAsync(path, bytes);
        }
        catch (Exception ex)
        {
            await ShowMessageAsync("Can't save file", $"Error saving MIDI file: {ex.Message}");
            return false;
        }

        midiFile.MarkSaved(path);
        Model.StatusMessage = $"Saved to {path}";
        return true;
    }

    #endregion

    #region Dialogs

    /// <summary>
    ///     Returns true when it is fine to replace or close the current file.
    /// </summary>
    private async Task<bool> ConfirmDiscardAsync()
    {
        if (Model.MidiFile is not { IsDirty: true } midiFile)
            return true;

        var message = $"{midiFile.Filename} has unsaved changes.";
        if (!midiFile.CanSave)
        {
            var discard = await ShowDialogAsync("Unsaved changes", message, "Discard", "Cancel");
            return discard == 0;
        }

        var choice = await ShowDialogAsync("Unsaved changes", message, "Save", "Discard", "Cancel");
        return choice switch
        {
            0 => await SaveAsync(),
            1 => true,
            _ => false
        };
    }

    private Task ShowMessageAsync(string title, string message)
    {
        return ShowDialogAsync(title, message, "OK");
    }

    /// <summary>
    ///     Shows a modal dialog and returns the index of the clicked button, or -1 if it was closed otherwise.
    /// </summary>
    private async Task<int> ShowDialogAsync(string title, string message, params string[] buttons)
    {
        var result = -1;
        var dialog = new Window
        {
            Title = title,
            Width = 420,
            SizeToContent = SizeToContent.Height,
            CanResize = false,
            ShowInTaskbar = false,
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };

        var buttonPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Spacing = 8
        };
        for (var i = 0; i < buttons.Length; i++)
        {
            var index = i;
            var button = new Button { Content = buttons[i], Padding = new Thickness(16, 6) };
            if (i == 0)
                button.Classes.Add("accent");
            button.Click += (_, _) =>
            {
                result = index;
                dialog.Close();
            };
            buttonPanel.Children.Add(button);
        }

        dialog.Content = new StackPanel
        {
            Margin = new Thickness(20),
            Spacing = 20,
            Children =
            {
                new TextBlock { Text = message, TextWrapping = TextWrapping.Wrap },
                buttonPanel
            }
        };

        await dialog.ShowDialog(this);
        return result;
    }

    #endregion
}
