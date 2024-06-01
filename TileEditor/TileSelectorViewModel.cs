using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Microsoft.Win32;

using System.Collections.ObjectModel;
using TileEditor.Domain;

namespace TileEditor;

internal partial class TileSelectorViewModel : ObservableObject
{
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
}
