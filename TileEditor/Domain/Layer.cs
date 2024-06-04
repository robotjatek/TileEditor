using CommunityToolkit.Mvvm.ComponentModel;

using System.Collections.ObjectModel;

using TileEditor.Domain;

namespace TileEditor;

public partial class Layer : ObservableObject
{
    [ObservableProperty]
    private int _width = 32;

    [ObservableProperty]
    private int _height = 18;

    [ObservableProperty]
    private ObservableCollection<Tile> _tiles = [];

    [ObservableProperty]
    private bool _isDefault = false;

    public Tile? GetTile(int x, int y)
    {
        var index = y * Width + x;
        if (index >= Tiles.Count || x > Width || y > Height || x < 0 || y < 0)
            return null;

        return Tiles[index];
    }
}
