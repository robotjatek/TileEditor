using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

using Microsoft.Win32;

using System.Collections.ObjectModel;
using TileEditor.Domain;

using static TileEditor.MainWindowViewModel;

namespace TileEditor;

internal partial class TileSelectorViewModel : ObservableObject
{
    public TileSelectorViewModel()
    {
        WeakReferenceMessenger.Default.Register<LevelLoadedMessage>(this, (_, m) => ReceiveTileFilenames(m.Value));
    }

    [ObservableProperty]
    private Tile? selected;

    public ObservableCollection<Tile> Data { get; set; } = new ObservableCollection<Tile>([
        new Tile
        {
            TexturePath = null
        }]);

    [RelayCommand]
    public void SelectTiles()
    {
        var dialog = new OpenFileDialog
        {
            Multiselect = true,
            Title = "Add tiles...",
            Filter = "Image files (*.png;*.jpg;*.jpeg;*.bmp)|*.png;*.jpg;*.jpeg;*.bmp",
        };

        if (dialog.ShowDialog() == true)
        {
            var files = dialog.FileNames;
            var tilesToAdd = files.Select(f => new Tile { TexturePath = f }).Where(t => !Data.Any(d => d.TexturePath == t.TexturePath));
            foreach (var tile in tilesToAdd)
            {
                Data.Add(tile);
            }
        }
    }

    private void ReceiveTileFilenames(HashSet<string> tileFilenames)
    {
        Data.Clear();
        Data.Add(new Tile());
        var tiles = tileFilenames.Select(f => new Tile { TexturePath = f });
        foreach (var tile in tiles)
        {
            Data.Add(tile);
        }
    }
}
