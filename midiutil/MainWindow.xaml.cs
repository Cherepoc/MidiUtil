using System.IO;
using System.Windows;
using Microsoft.Win32;
using MidiUtil.Midi;
using MidiUtil.Models;

namespace MidiUtil;

/// <summary>
///     Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow
{
    private const string FileDialogFilter = "MIDI Files (*.mid, *.midi)|*.mid;*.midi|All Files (*.*)|*.*";
    private const string FileDialogDefaultExtension = ".mid";

    public MainWindow()
    {
        InitializeComponent();
        DataContext = Model;
    }

    private MainModel Model { get; } = new();

    private void MenuItemExit_OnClick(object sender, RoutedEventArgs e)
    {
        Application.Current.Shutdown();
    }

    private void MenuItemOpen_OnClick(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            DefaultExt = FileDialogDefaultExtension,
            Filter = FileDialogFilter
        };
        var result = dialog.ShowDialog();
        if (result != true)
            return;

        try
        {
            using var stream = dialog.OpenFile();
            var midiFile = MidiParser.Parse(stream);
            Model.MidiFile = MidiFileModelConverter.ConvertToModel(dialog.FileName, midiFile);
        }
        catch (Exception ex)
        {
            ShowErrorMessage($"Error reading MIDI file: {ex.Message}");
        }
    }

    private void MenuItemSave_OnClick(object sender, RoutedEventArgs e)
    {
        SaveFile(Model.MidiFile!.Path);
    }

    private void MenuItemSaveAs_OnClick(object sender, RoutedEventArgs e)
    {
        var dialog = new SaveFileDialog
        {
            DefaultExt = FileDialogDefaultExtension,
            Filter = FileDialogFilter,
            FileName = Model.MidiFile!.Filename,
            InitialDirectory = Path.GetDirectoryName(Model.MidiFile!.Path)
        };
        var result = dialog.ShowDialog();
        if (result != true)
            return;

        SaveFile(dialog.FileName);
    }

    private void SaveFile(string path)
    {
        try
        {
            var bytes = MidiFileModelConverter.SaveToBytes(Model.MidiFile!);
            File.WriteAllBytes(path, bytes);
        }
        catch (Exception ex)
        {
            ShowErrorMessage($"Error saving MIDI file: {ex.Message}");
        }
    }

    private static void ShowErrorMessage(string message)
    {
        MessageBox.Show(
            message,
            "Error",
            MessageBoxButton.OK,
            MessageBoxImage.Error
        );
    }
}
